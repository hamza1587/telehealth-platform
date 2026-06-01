using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Doctors;
using Telehealth.Platform.Domain.Doctors;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Doctors;

public class DoctorProfileService : IDoctorProfileService
{
    private readonly PlatformDbContext _dbContext;

    public DoctorProfileService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    public async Task<DoctorProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DoctorProfiles
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<DoctorProfile?> GetByMedplumIdAsync(Guid medplumPractitionerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DoctorProfiles
            .FirstOrDefaultAsync(d => d.MedplumPractitionerId == medplumPractitionerId, cancellationToken);
    }

    public async Task<IEnumerable<DoctorProfile>> SearchAsync(
        string? specialty,
        string? countryCode,
        DoctorMarketplaceStatus status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DoctorProfiles
            .Where(d => d.MarketplaceStatus == status);

        if (!string.IsNullOrWhiteSpace(specialty))
            query = query.Where(d => d.PrimarySpecialty.Contains(specialty));

        if (!string.IsNullOrWhiteSpace(countryCode))
            query = query.Where(d => d.CountryCode == countryCode);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DoctorAvailabilityWindow>> GetAvailabilityAsync(
        Guid doctorProfileId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DoctorAvailabilityWindows
            .Where(w => w.DoctorProfileId == doctorProfileId
                     && w.StartsAt >= from
                     && w.EndsAt <= to)
            .ToListAsync(cancellationToken);
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    public async Task<DoctorProfile> CreateAsync(
        Guid medplumPractitionerId,
        string displayName,
        string countryCode,
        string primarySpecialty,
        int defaultPricePerSecondMinor,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var profile = DoctorProfile.Create(
            medplumPractitionerId,
            displayName,
            countryCode,
            primarySpecialty,
            defaultPricePerSecondMinor,
            currency);

        _dbContext.DoctorProfiles.Add(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task UpdateProfileAsync(
        Guid id,
        string displayName,
        string countryCode,
        string primarySpecialty,
        int defaultPricePerSecondMinor,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Doctor profile {id} not found.");

        profile.UpdateProfile(displayName, countryCode, primarySpecialty, defaultPricePerSecondMinor, currency);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SubmitForVerificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Doctor profile {id} not found.");

        profile.SubmitForVerification();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveVerificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Doctor profile {id} not found.");

        profile.ApproveVerification();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectVerificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Doctor profile {id} not found.");

        profile.RejectVerification();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMarketplaceStatusAsync(
        Guid id,
        DoctorMarketplaceStatus status,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Doctor profile {id} not found.");

        profile.UpdateMarketplaceStatus(status);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
