using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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

        return app;
    }

    private static async Task<Ok<DoctorSearchResponse>> SearchDoctorsAsync(
        string? specialty,
        string? country,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var doctors = await dbContext.DoctorProfiles
            .Where(x => x.VerificationStatus == DoctorVerificationStatus.Verified)
            .Where(x => x.MarketplaceStatus == DoctorMarketplaceStatus.Available)
            .ToListAsync(cancellationToken);

        var results = doctors
            .Where(d => string.IsNullOrWhiteSpace(specialty) || d.PrimarySpecialty.Contains(specialty, StringComparison.OrdinalIgnoreCase))
            .Where(d => string.IsNullOrWhiteSpace(country) || d.CountryCode.Equals(country, StringComparison.OrdinalIgnoreCase))
            .Select(d => new DoctorSearchItem(
                d.Id,
                d.DisplayName,
                d.PrimarySpecialty,
                d.CountryCode,
                d.DefaultPricePerSecondMinor,
                d.Currency))
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

        return TypedResults.Ok(new DoctorDetailResponse(
            doctor.Id,
            doctor.DisplayName,
            doctor.PrimarySpecialty,
            doctor.CountryCode,
            doctor.DefaultPricePerSecondMinor,
            doctor.Currency,
            doctor.VerificationStatus.ToString()));
    }
}

public record DoctorSearchItem(
    Guid Id,
    string DisplayName,
    string PrimarySpecialty,
    string CountryCode,
    int PricePerSecondMinor,
    string Currency);

public record DoctorSearchResponse(IReadOnlyList<DoctorSearchItem> Doctors);

public record DoctorDetailResponse(
    Guid Id,
    string DisplayName,
    string PrimarySpecialty,
    string CountryCode,
    int PricePerSecondMinor,
    string Currency,
    string VerificationStatus);