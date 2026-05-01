using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Time;
using Telehealth.Platform.Domain.Auditing;
using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Domain.Doctors;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Doctors;

internal static class DoctorEndpoints
{
    public static IEndpointRouteBuilder MapDoctorEndpoints(this IEndpointRouteBuilder app)
    {
        var doctors = app.MapGroup("/platform/doctors").WithTags("Doctors");

        doctors.MapPost("/onboarding", CreateDoctorAsync);
        doctors.MapGet("/{doctorId:guid}", GetDoctorAsync);
        doctors.MapPut("/{doctorId:guid}/profile", UpdateDoctorProfileAsync);
        doctors.MapPut("/{doctorId:guid}/verification", UpdateVerificationAsync);
        doctors.MapPut("/{doctorId:guid}/availability", ReplaceAvailabilityAsync);

        return app;
    }

    private static async Task<Results<Ok<DoctorResponse>, ValidationProblem>> CreateDoctorAsync(
        SaveDoctorProfileRequest request,
        PlatformDbContext dbContext,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var errors = ValidateDoctorRequest(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var now = clock.UtcNow;
        var doctorId = Guid.NewGuid();

        var doctorProfile = new DoctorProfile(
            doctorId,
            $"pending-{Guid.NewGuid():N}",
            request.DisplayName.Trim(),
            request.CountryCode.Trim().ToUpperInvariant(),
            request.PrimarySpecialty.Trim(),
            new Money(request.DefaultPricePerSecondMinor, request.Currency.Trim().ToUpperInvariant()),
            DoctorVerificationStatus.Submitted,
            DoctorMarketplaceStatus.Hidden,
            now,
            now);

        var onboardingRecord = new DoctorOnboardingRecord(
            Guid.NewGuid(),
            doctorId,
            request.LegalName.Trim(),
            request.Email.Trim().ToLowerInvariant(),
            request.PhoneNumber.Trim(),
            request.CountryOfPractice.Trim().ToUpperInvariant(),
            request.LicenseNumber.Trim(),
            request.LicensingAuthority.Trim(),
            request.Qualifications.Trim(),
            request.YearsOfExperience,
            request.Biography.Trim(),
            request.InsuranceProvider.Trim(),
            request.InsurancePolicyNumber.Trim(),
            request.LicenseExpiryDate,
            null,
            null,
            now,
            now);

        dbContext.DoctorProfiles.Add(doctorProfile);
        dbContext.DoctorOnboardingRecords.Add(onboardingRecord);
        dbContext.DoctorLanguages.AddRange(
            request.Languages
                .Where(language => !string.IsNullOrWhiteSpace(language))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(language => new DoctorLanguage(doctorId, language.Trim().ToLowerInvariant())));

        dbContext.DoctorAvailabilityWindows.AddRange(ToAvailabilityWindows(doctorId, request.AvailabilityWindows, now));

        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            doctorId.ToString(),
            "Doctor",
            "DoctorOnboardingSubmitted",
            "DoctorProfile",
            doctorId.ToString(),
            now));

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(await BuildDoctorResponseAsync(doctorId, dbContext, cancellationToken));
    }

    private static async Task<Results<Ok<DoctorResponse>, NotFound, ValidationProblem>> UpdateDoctorProfileAsync(
        Guid doctorId,
        SaveDoctorProfileRequest request,
        PlatformDbContext dbContext,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var doctorProfile = await dbContext.DoctorProfiles.SingleOrDefaultAsync(x => x.Id == doctorId, cancellationToken);
        var onboardingRecord = await dbContext.DoctorOnboardingRecords.SingleOrDefaultAsync(x => x.DoctorProfileId == doctorId, cancellationToken);

        if (doctorProfile is null || onboardingRecord is null)
        {
            return TypedResults.NotFound();
        }

        var errors = ValidateDoctorRequest(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var now = clock.UtcNow;
        doctorProfile.UpdateProfile(
            request.DisplayName.Trim(),
            request.CountryCode.Trim().ToUpperInvariant(),
            request.PrimarySpecialty.Trim(),
            new Money(request.DefaultPricePerSecondMinor, request.Currency.Trim().ToUpperInvariant()),
            now);

        onboardingRecord.UpdateDetails(
            request.LegalName.Trim(),
            request.Email.Trim().ToLowerInvariant(),
            request.PhoneNumber.Trim(),
            request.CountryOfPractice.Trim().ToUpperInvariant(),
            request.LicenseNumber.Trim(),
            request.LicensingAuthority.Trim(),
            request.Qualifications.Trim(),
            request.YearsOfExperience,
            request.Biography.Trim(),
            request.InsuranceProvider.Trim(),
            request.InsurancePolicyNumber.Trim(),
            request.LicenseExpiryDate,
            now);

        var existingLanguages = await dbContext.DoctorLanguages.Where(x => x.DoctorProfileId == doctorId).ToListAsync(cancellationToken);
        if (existingLanguages.Count > 0)
        {
            dbContext.DoctorLanguages.RemoveRange(existingLanguages);
        }

        dbContext.DoctorLanguages.AddRange(
            request.Languages
                .Where(language => !string.IsNullOrWhiteSpace(language))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(language => new DoctorLanguage(doctorId, language.Trim().ToLowerInvariant())));

        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            doctorId.ToString(),
            "Doctor",
            "DoctorProfileUpdated",
            "DoctorProfile",
            doctorId.ToString(),
            now));

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(await BuildDoctorResponseAsync(doctorId, dbContext, cancellationToken));
    }

    private static async Task<Results<Ok<DoctorResponse>, NotFound, ValidationProblem>> UpdateVerificationAsync(
        Guid doctorId,
        UpdateDoctorVerificationRequest request,
        PlatformDbContext dbContext,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var doctorProfile = await dbContext.DoctorProfiles.SingleOrDefaultAsync(x => x.Id == doctorId, cancellationToken);
        var onboardingRecord = await dbContext.DoctorOnboardingRecords.SingleOrDefaultAsync(x => x.DoctorProfileId == doctorId, cancellationToken);

        if (doctorProfile is null || onboardingRecord is null)
        {
            return TypedResults.NotFound();
        }

        if (!Enum.TryParse<DoctorVerificationStatus>(request.VerificationStatus, true, out var verificationStatus))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["verificationStatus"] = ["Verification status is invalid."]
            });
        }

        var marketplaceStatus = verificationStatus == DoctorVerificationStatus.Verified
            ? DoctorMarketplaceStatus.Available
            : DoctorMarketplaceStatus.Hidden;

        var now = clock.UtcNow;
        doctorProfile.UpdateVerificationStatus(verificationStatus, marketplaceStatus, now);
        onboardingRecord.UpdateReview(request.ReviewerId?.Trim(), request.ReviewNotes?.Trim(), now);

        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            request.ReviewerId?.Trim() ?? "admin-reviewer",
            "Admin",
            "DoctorVerificationUpdated",
            "DoctorProfile",
            doctorId.ToString(),
            now));

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(await BuildDoctorResponseAsync(doctorId, dbContext, cancellationToken));
    }

    private static async Task<Results<Ok<DoctorResponse>, NotFound, ValidationProblem>> ReplaceAvailabilityAsync(
        Guid doctorId,
        ReplaceDoctorAvailabilityRequest request,
        PlatformDbContext dbContext,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var doctorProfile = await dbContext.DoctorProfiles.SingleOrDefaultAsync(x => x.Id == doctorId, cancellationToken);
        if (doctorProfile is null)
        {
            return TypedResults.NotFound();
        }

        var errors = new Dictionary<string, string[]>();
        foreach (var window in request.AvailabilityWindows)
        {
            if (window.EndsAt <= window.StartsAt)
            {
                errors["availabilityWindows"] = ["Availability end time must be later than start time."];
                break;
            }
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var now = clock.UtcNow;
        var existingWindows = await dbContext.DoctorAvailabilityWindows.Where(x => x.DoctorProfileId == doctorId).ToListAsync(cancellationToken);
        if (existingWindows.Count > 0)
        {
            dbContext.DoctorAvailabilityWindows.RemoveRange(existingWindows);
        }

        dbContext.DoctorAvailabilityWindows.AddRange(ToAvailabilityWindows(doctorId, request.AvailabilityWindows, now));
        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            doctorId.ToString(),
            "Doctor",
            "DoctorAvailabilityUpdated",
            "DoctorProfile",
            doctorId.ToString(),
            now));

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(await BuildDoctorResponseAsync(doctorId, dbContext, cancellationToken));
    }

    private static async Task<Results<Ok<DoctorResponse>, NotFound>> GetDoctorAsync(
        Guid doctorId,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var doctor = await dbContext.DoctorProfiles.AnyAsync(x => x.Id == doctorId, cancellationToken);
        if (!doctor)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(await BuildDoctorResponseAsync(doctorId, dbContext, cancellationToken));
    }

    private static async Task<DoctorResponse> BuildDoctorResponseAsync(
        Guid doctorId,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var doctorProfile = await dbContext.DoctorProfiles.SingleAsync(x => x.Id == doctorId, cancellationToken);
        var onboardingRecord = await dbContext.DoctorOnboardingRecords.SingleAsync(x => x.DoctorProfileId == doctorId, cancellationToken);
        var languages = await dbContext.DoctorLanguages
            .Where(x => x.DoctorProfileId == doctorId)
            .Select(x => x.LanguageCode)
            .OrderBy(x => x)
            .ToArrayAsync(cancellationToken);

        var availability = await dbContext.DoctorAvailabilityWindows
            .Where(x => x.DoctorProfileId == doctorId)
            .OrderBy(x => x.StartsAt)
            .Select(x => new DoctorAvailabilityWindowResponse(x.Id, x.StartsAt, x.EndsAt, x.ConsultationMode.ToString(), x.IsInstantEnabled))
            .ToArrayAsync(cancellationToken);

        return new DoctorResponse(
            doctorProfile.Id,
            doctorProfile.DisplayName,
            doctorProfile.CountryCode,
            doctorProfile.PrimarySpecialty,
            doctorProfile.PricePerSecond.MinorUnits,
            doctorProfile.PricePerSecond.Currency,
            doctorProfile.VerificationStatus.ToString(),
            doctorProfile.MarketplaceStatus.ToString(),
            languages,
            new DoctorOnboardingResponse(
                onboardingRecord.LegalName,
                onboardingRecord.Email,
                onboardingRecord.PhoneNumber,
                onboardingRecord.CountryOfPractice,
                onboardingRecord.LicenseNumber,
                onboardingRecord.LicensingAuthority,
                onboardingRecord.Qualifications,
                onboardingRecord.YearsOfExperience,
                onboardingRecord.Biography,
                onboardingRecord.InsuranceProvider,
                onboardingRecord.InsurancePolicyNumber,
                onboardingRecord.LicenseExpiryDate,
                onboardingRecord.ReviewerId,
                onboardingRecord.ReviewNotes),
            availability);
    }

    private static List<DoctorAvailabilityWindow> ToAvailabilityWindows(
        Guid doctorId,
        IReadOnlyCollection<DoctorAvailabilityWindowRequest> windows,
        DateTimeOffset createdAt)
    {
        return windows.Select(window => new DoctorAvailabilityWindow(
            Guid.NewGuid(),
            doctorId,
            window.StartsAt,
            window.EndsAt,
            Enum.TryParse<ConsultationMode>(window.ConsultationMode, true, out var mode) ? mode : ConsultationMode.Video,
            window.IsInstantEnabled,
            createdAt)).ToList();
    }

    private static Dictionary<string, string[]> ValidateDoctorRequest(SaveDoctorProfileRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors["displayName"] = ["Display name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.LegalName))
        {
            errors["legalName"] = ["Legal name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.CountryCode) || request.CountryCode.Trim().Length != 2)
        {
            errors["countryCode"] = ["Country code must be a 2-letter ISO code."];
        }

        if (string.IsNullOrWhiteSpace(request.PrimarySpecialty))
        {
            errors["primarySpecialty"] = ["Primary specialty is required."];
        }

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
        {
            errors["licenseNumber"] = ["License number is required."];
        }

        if (string.IsNullOrWhiteSpace(request.LicensingAuthority))
        {
            errors["licensingAuthority"] = ["Licensing authority is required."];
        }

        if (request.YearsOfExperience < 0)
        {
            errors["yearsOfExperience"] = ["Years of experience cannot be negative."];
        }

        if (request.DefaultPricePerSecondMinor < 0)
        {
            errors["defaultPricePerSecondMinor"] = ["Default price per second must be non-negative."];
        }

        return errors;
    }

    internal sealed record SaveDoctorProfileRequest(
        string DisplayName,
        string LegalName,
        string Email,
        string PhoneNumber,
        string CountryCode,
        string CountryOfPractice,
        string PrimarySpecialty,
        string Qualifications,
        int YearsOfExperience,
        string Biography,
        string LicenseNumber,
        string LicensingAuthority,
        string InsuranceProvider,
        string InsurancePolicyNumber,
        DateOnly? LicenseExpiryDate,
        long DefaultPricePerSecondMinor,
        string Currency,
        string[] Languages,
        DoctorAvailabilityWindowRequest[] AvailabilityWindows);

    internal sealed record UpdateDoctorVerificationRequest(
        string VerificationStatus,
        string? ReviewerId,
        string? ReviewNotes);

    internal sealed record ReplaceDoctorAvailabilityRequest(
        DoctorAvailabilityWindowRequest[] AvailabilityWindows);

    internal sealed record DoctorAvailabilityWindowRequest(
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        string ConsultationMode,
        bool IsInstantEnabled);

    internal sealed record DoctorResponse(
        Guid Id,
        string DisplayName,
        string CountryCode,
        string PrimarySpecialty,
        long DefaultPricePerSecondMinor,
        string Currency,
        string VerificationStatus,
        string MarketplaceStatus,
        string[] Languages,
        DoctorOnboardingResponse Onboarding,
        DoctorAvailabilityWindowResponse[] AvailabilityWindows);

    internal sealed record DoctorOnboardingResponse(
        string LegalName,
        string Email,
        string PhoneNumber,
        string CountryOfPractice,
        string LicenseNumber,
        string LicensingAuthority,
        string Qualifications,
        int YearsOfExperience,
        string Biography,
        string InsuranceProvider,
        string InsurancePolicyNumber,
        DateOnly? LicenseExpiryDate,
        string? ReviewerId,
        string? ReviewNotes);

    internal sealed record DoctorAvailabilityWindowResponse(
        Guid Id,
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        string ConsultationMode,
        bool IsInstantEnabled);
}
