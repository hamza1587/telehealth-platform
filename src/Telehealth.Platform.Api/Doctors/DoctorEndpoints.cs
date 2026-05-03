using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Time;
using Telehealth.Platform.Domain.Common;
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

        return app;
    }

    private static async Task<Results<Ok<DoctorResponse>, ValidationProblem>> CreateDoctorAsync(
        SaveDoctorProfileRequest request,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var doctorProfile = DoctorProfile.Create(
            Guid.NewGuid(),
            request.DisplayName.Trim(),
            request.CountryCode.Trim().ToUpperInvariant(),
            request.PrimarySpecialty.Trim(),
            request.DefaultPricePerSecondMinor,
            request.Currency.Trim().ToUpperInvariant());

        dbContext.DoctorProfiles.Add(doctorProfile);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(await BuildDoctorResponseAsync(doctorProfile.Id, dbContext, cancellationToken));
    }

    private static async Task<Results<Ok<DoctorResponse>, NotFound>> GetDoctorAsync(
        Guid doctorId,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var doctorProfile = await dbContext.DoctorProfiles.SingleOrDefaultAsync(x => x.Id == doctorId, cancellationToken);
        if (doctorProfile is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(await BuildDoctorResponseAsync(doctorId, dbContext, cancellationToken));
    }

    private static async Task<Results<Ok<DoctorResponse>, NotFound, ValidationProblem>> UpdateDoctorProfileAsync(
        Guid doctorId,
        SaveDoctorProfileRequest request,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var doctorProfile = await dbContext.DoctorProfiles.SingleOrDefaultAsync(x => x.Id == doctorId, cancellationToken);
        if (doctorProfile is null)
        {
            return TypedResults.NotFound();
        }

        doctorProfile.UpdateProfile(
            request.DisplayName.Trim(),
            request.CountryCode.Trim().ToUpperInvariant(),
            request.PrimarySpecialty.Trim(),
            request.DefaultPricePerSecondMinor,
            request.Currency.Trim().ToUpperInvariant());

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(await BuildDoctorResponseAsync(doctorId, dbContext, cancellationToken));
    }

    private static async Task<Results<Ok<DoctorResponse>, NotFound, ValidationProblem>> UpdateVerificationAsync(
        Guid doctorId,
        UpdateDoctorVerificationRequest request,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var doctorProfile = await dbContext.DoctorProfiles.SingleOrDefaultAsync(x => x.Id == doctorId, cancellationToken);
        if (doctorProfile is null)
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

        doctorProfile.UpdateVerificationStatus(verificationStatus);
        doctorProfile.UpdateMarketplaceStatus(marketplaceStatus);

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(await BuildDoctorResponseAsync(doctorId, dbContext, cancellationToken));
    }

    private static async Task<DoctorResponse> BuildDoctorResponseAsync(
        Guid doctorId,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var doctor = await dbContext.DoctorProfiles.FindAsync([doctorId], cancellationToken);
        return new DoctorResponse(
            doctor?.Id ?? doctorId,
            doctor?.DisplayName ?? string.Empty,
            doctor?.CountryCode ?? string.Empty,
            doctor?.PrimarySpecialty ?? string.Empty,
            doctor?.DefaultPricePerSecondMinor ?? 0,
            doctor?.Currency ?? "USD",
            doctor?.VerificationStatus ?? DoctorVerificationStatus.Draft,
            doctor?.MarketplaceStatus ?? DoctorMarketplaceStatus.Hidden,
            doctor?.CreatedAt ?? DateTimeOffset.UtcNow,
            doctor?.UpdatedAt ?? DateTimeOffset.UtcNow);
    }
}

public record SaveDoctorProfileRequest(
    string DisplayName,
    string CountryCode,
    string PrimarySpecialty,
    int DefaultPricePerSecondMinor,
    string Currency);

public record UpdateDoctorVerificationRequest(
    string VerificationStatus,
    string? ReviewerId,
    string? ReviewNotes);

public record DoctorResponse(
    Guid Id,
    string DisplayName,
    string CountryCode,
    string PrimarySpecialty,
    int DefaultPricePerSecondMinor,
    string Currency,
    DoctorVerificationStatus VerificationStatus,
    DoctorMarketplaceStatus MarketplaceStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);