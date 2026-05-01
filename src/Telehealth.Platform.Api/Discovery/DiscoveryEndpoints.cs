using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Time;
using Telehealth.Platform.Domain.Auditing;
using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Domain.Doctors;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Discovery;

internal static class DiscoveryEndpoints
{
    public static IEndpointRouteBuilder MapDiscoveryEndpoints(this IEndpointRouteBuilder app)
    {
        var discovery = app.MapGroup("/platform/discovery").WithTags("Discovery");
        discovery.MapGet("/doctors", SearchDoctorsAsync);
        discovery.MapGet("/doctors/{doctorId:guid}", GetDoctorDetailAsync);

        var bookings = app.MapGroup("/platform/bookings").WithTags("Bookings");
        bookings.MapPost(string.Empty, CreateBookingAsync);
        bookings.MapGet("/patients/{patientId:guid}", GetPatientBookingsAsync);

        return app;
    }

    private static async Task<Ok<DoctorSearchResponse>> SearchDoctorsAsync(
        string? q,
        string? specialty,
        string? language,
        string? country,
        string? consultationMode,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var doctors = await dbContext.DoctorProfiles
            .Where(x => x.VerificationStatus == DoctorVerificationStatus.Verified)
            .Where(x => x.MarketplaceStatus == DoctorMarketplaceStatus.Available)
            .ToListAsync(cancellationToken);

        var languages = await dbContext.DoctorLanguages.ToListAsync(cancellationToken);
        var availability = await dbContext.DoctorAvailabilityWindows.ToListAsync(cancellationToken);

        var results = doctors
            .Select(doctor =>
            {
                var doctorLanguages = languages
                    .Where(x => x.DoctorProfileId == doctor.Id)
                    .Select(x => x.LanguageCode)
                    .OrderBy(x => x)
                    .ToArray();

                var doctorAvailability = availability
                    .Where(x => x.DoctorProfileId == doctor.Id)
                    .OrderBy(x => x.StartsAt)
                    .ToArray();

                return new DoctorSearchItem(
                    doctor.Id,
                    doctor.DisplayName,
                    doctor.PrimarySpecialty,
                    doctor.CountryCode,
                    doctor.PricePerSecond.MinorUnits,
                    doctor.PricePerSecond.Currency,
                    doctor.VerificationStatus.ToString(),
                    doctorLanguages,
                    doctorAvailability.FirstOrDefault()?.StartsAt,
                    doctorAvailability.Select(window => new DoctorAvailabilityPreview(
                        window.Id,
                        window.StartsAt,
                        window.EndsAt,
                        window.ConsultationMode.ToString())).ToArray());
            })
            .Where(item => string.IsNullOrWhiteSpace(q)
                || $"{item.DisplayName} {item.PrimarySpecialty} {string.Join(' ', item.Languages)}"
                    .Contains(q, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(specialty)
                || item.PrimarySpecialty.Contains(specialty, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(language)
                || item.Languages.Any(x => x.Equals(language, StringComparison.OrdinalIgnoreCase)))
            .Where(item => string.IsNullOrWhiteSpace(country)
                || item.CountryCode.Equals(country, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(consultationMode)
                || item.AvailabilityWindows.Any(x => x.ConsultationMode.Equals(consultationMode, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(item => item.NextAvailableAt ?? DateTimeOffset.MaxValue)
            .ToArray();

        return TypedResults.Ok(new DoctorSearchResponse(results));
    }

    private static async Task<Results<Ok<DoctorDetailResponse>, NotFound>> GetDoctorDetailAsync(
        Guid doctorId,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var doctor = await dbContext.DoctorProfiles.SingleOrDefaultAsync(x => x.Id == doctorId, cancellationToken);
        if (doctor is null)
        {
            return TypedResults.NotFound();
        }

        var onboarding = await dbContext.DoctorOnboardingRecords.SingleOrDefaultAsync(x => x.DoctorProfileId == doctorId, cancellationToken);
        var languages = await dbContext.DoctorLanguages
            .Where(x => x.DoctorProfileId == doctorId)
            .Select(x => x.LanguageCode)
            .OrderBy(x => x)
            .ToArrayAsync(cancellationToken);
        var availability = await dbContext.DoctorAvailabilityWindows
            .Where(x => x.DoctorProfileId == doctorId)
            .OrderBy(x => x.StartsAt)
            .Select(window => new DoctorAvailabilityPreview(
                window.Id,
                window.StartsAt,
                window.EndsAt,
                window.ConsultationMode.ToString()))
            .ToArrayAsync(cancellationToken);

        return TypedResults.Ok(new DoctorDetailResponse(
            doctor.Id,
            doctor.DisplayName,
            doctor.PrimarySpecialty,
            doctor.CountryCode,
            doctor.PricePerSecond.MinorUnits,
            doctor.PricePerSecond.Currency,
            doctor.VerificationStatus.ToString(),
            languages,
            onboarding?.Biography ?? string.Empty,
            onboarding?.Qualifications ?? string.Empty,
            availability));
    }

    private static async Task<Results<Ok<BookingResponse>, ValidationProblem, NotFound>> CreateBookingAsync(
        CreateBookingRequest request,
        PlatformDbContext dbContext,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        var patient = await dbContext.PatientAccounts.SingleOrDefaultAsync(x => x.Id == request.PatientAccountId, cancellationToken);
        if (patient is null)
        {
            return TypedResults.NotFound();
        }

        var doctor = await dbContext.DoctorProfiles.SingleOrDefaultAsync(x => x.Id == request.DoctorProfileId, cancellationToken);
        if (doctor is null)
        {
            return TypedResults.NotFound();
        }

        if (request.ScheduledEndsAt <= request.ScheduledStartsAt)
        {
            errors["scheduledStartsAt"] = ["Scheduled end time must be later than start time."];
        }

        if (!Enum.TryParse<ConsultationMode>(request.ConsultationMode, true, out var consultationMode))
        {
            errors["consultationMode"] = ["Consultation mode is invalid."];
        }

        var selectedWindow = await dbContext.DoctorAvailabilityWindows.SingleOrDefaultAsync(
            x => x.Id == request.AvailabilityWindowId && x.DoctorProfileId == request.DoctorProfileId,
            cancellationToken);

        if (selectedWindow is null)
        {
            errors["availabilityWindowId"] = ["Selected availability window was not found."];
        }
        else if (request.ScheduledStartsAt < selectedWindow.StartsAt || request.ScheduledEndsAt > selectedWindow.EndsAt)
        {
            errors["availabilityWindowId"] = ["Selected booking time must fit inside the doctor's availability window."];
        }

        var conflictingBookingExists = await dbContext.ConsultationBookings.AnyAsync(
            x => x.DoctorProfileId == request.DoctorProfileId
                && x.Status == ConsultationBookingStatus.Confirmed
                && x.ScheduledStartsAt < request.ScheduledEndsAt
                && x.ScheduledEndsAt > request.ScheduledStartsAt,
            cancellationToken);

        if (conflictingBookingExists)
        {
            errors["scheduledStartsAt"] = ["The selected slot is no longer available."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var now = clock.UtcNow;
        var reservedSeconds = Math.Max(0, (long)Math.Ceiling((request.ScheduledEndsAt - request.ScheduledStartsAt).TotalSeconds));
        var booking = new ConsultationBooking(
            Guid.NewGuid(),
            request.PatientAccountId,
            request.DoctorProfileId,
            doctor.PrimarySpecialty,
            consultationMode,
            BookingType.Scheduled,
            new Money(doctor.PricePerSecond.MinorUnits, doctor.PricePerSecond.Currency),
            reservedSeconds,
            request.ScheduledStartsAt,
            request.ScheduledEndsAt,
            now,
            now);

        booking.Confirm(now);
        dbContext.ConsultationBookings.Add(booking);
        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            request.PatientAccountId.ToString(),
            "Patient",
            "ConsultationBookingCreated",
            "ConsultationBooking",
            booking.Id.ToString(),
            now));

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(ToBookingResponse(booking, doctor.DisplayName));
    }

    private static async Task<Ok<PatientBookingsResponse>> GetPatientBookingsAsync(
        Guid patientId,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var doctors = await dbContext.DoctorProfiles.ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

        var bookings = await dbContext.ConsultationBookings
            .Where(x => x.PatientAccountId == patientId)
            .OrderByDescending(x => x.ScheduledStartsAt)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(new PatientBookingsResponse(
            bookings.Select(booking => ToBookingResponse(
                booking,
                doctors.TryGetValue(booking.DoctorProfileId, out var doctorName) ? doctorName : "Doctor"))
            .ToArray()));
    }

    private static BookingResponse ToBookingResponse(ConsultationBooking booking, string doctorDisplayName)
    {
        return new BookingResponse(
            booking.Id,
            booking.PatientAccountId,
            booking.DoctorProfileId,
            doctorDisplayName,
            booking.SpecialtyCode,
            booking.ConsultationMode.ToString(),
            booking.Status.ToString(),
            booking.ScheduledStartsAt,
            booking.ScheduledEndsAt,
            booking.PricePerSecond.MinorUnits,
            booking.PricePerSecond.Currency,
            booking.ReservedSeconds);
    }

    internal sealed record DoctorSearchResponse(DoctorSearchItem[] Doctors);

    internal sealed record DoctorSearchItem(
        Guid Id,
        string DisplayName,
        string PrimarySpecialty,
        string CountryCode,
        long DefaultPricePerSecondMinor,
        string Currency,
        string VerificationStatus,
        string[] Languages,
        DateTimeOffset? NextAvailableAt,
        DoctorAvailabilityPreview[] AvailabilityWindows);

    internal sealed record DoctorDetailResponse(
        Guid Id,
        string DisplayName,
        string PrimarySpecialty,
        string CountryCode,
        long DefaultPricePerSecondMinor,
        string Currency,
        string VerificationStatus,
        string[] Languages,
        string Biography,
        string Qualifications,
        DoctorAvailabilityPreview[] AvailabilityWindows);

    internal sealed record DoctorAvailabilityPreview(
        Guid Id,
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        string ConsultationMode);

    internal sealed record CreateBookingRequest(
        Guid PatientAccountId,
        Guid DoctorProfileId,
        Guid AvailabilityWindowId,
        DateTimeOffset ScheduledStartsAt,
        DateTimeOffset ScheduledEndsAt,
        string ConsultationMode);

    internal sealed record BookingResponse(
        Guid Id,
        Guid PatientAccountId,
        Guid DoctorProfileId,
        string DoctorDisplayName,
        string SpecialtyCode,
        string ConsultationMode,
        string Status,
        DateTimeOffset? ScheduledStartsAt,
        DateTimeOffset? ScheduledEndsAt,
        long PricePerSecondMinor,
        string Currency,
        long ReservedSeconds);

    internal sealed record PatientBookingsResponse(BookingResponse[] Bookings);
}
