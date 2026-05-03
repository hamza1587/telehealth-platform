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

    public async Task<DoctorProfile?> GetByMedplumIdAsync(Guid medplumPractitionerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DoctorProfiles
            .FirstOrDefaultAsync(d => d.MedplumPractitionerId == medplumPractitionerId, cancellationToken);
    }

    public async Task<DoctorProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DoctorProfiles
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<DoctorProfile> CreateAsync(
        Guid medplumPractitionerId,
        string displayName,
        string countryCode,
        string primarySpecialty,
        int defaultPricePerSecondMinor,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByMedplumIdAsync(medplumPractitionerId, cancellationToken);
        if (existing != null)
            return existing;

        var doctor = DoctorProfile.Create(medplumPractitionerId, displayName, countryCode, primarySpecialty, defaultPricePerSecondMinor, currency);
        await _dbContext.DoctorProfiles.AddAsync(doctor, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return doctor;
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
        var doctor = await _dbContext.DoctorProfiles.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doctor != null)
        {
            doctor.UpdateProfile(displayName, countryCode, primarySpecialty, defaultPricePerSecondMinor, currency);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SubmitForVerificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var doctor = await _dbContext.DoctorProfiles.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        doctor?.SubmitForVerification();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveVerificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var doctor = await _dbContext.DoctorProfiles.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        doctor?.ApproveVerification();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectVerificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var doctor = await _dbContext.DoctorProfiles.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        doctor?.RejectVerification();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMarketplaceStatusAsync(Guid id, DoctorMarketplaceStatus status, CancellationToken cancellationToken = default)
    {
        var doctor = await _dbContext.DoctorProfiles.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        doctor?.UpdateMarketplaceStatus(status);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<DoctorProfile>> SearchAsync(
        string? specialty,
        string? countryCode,
        DoctorMarketplaceStatus status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DoctorProfiles.AsQueryable();

        if (!string.IsNullOrEmpty(specialty))
            query = query.Where(d => d.PrimarySpecialty == specialty);

        if (!string.IsNullOrEmpty(countryCode))
            query = query.Where(d => d.CountryCode == countryCode);

        query = query.Where(d => d.MarketplaceStatus == status);

        return await query.ToListAsync(cancellationToken);
    }
}