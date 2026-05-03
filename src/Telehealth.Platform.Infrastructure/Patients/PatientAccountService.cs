using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Patients;
using Telehealth.Platform.Domain.Patients;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Patients;

public class PatientAccountService : IPatientAccountService
{
    private readonly PlatformDbContext _dbContext;

    public PatientAccountService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PatientAccount?> GetByMedplumIdAsync(Guid medplumPatientId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PatientAccounts
            .FirstOrDefaultAsync(p => p.MedplumPatientId == medplumPatientId, cancellationToken);
    }

    public async Task<PatientAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PatientAccounts
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PatientAccount> CreateAsync(
        Guid medplumPatientId,
        string displayName,
        string email,
        string countryCode,
        string preferredLanguage,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByMedplumIdAsync(medplumPatientId, cancellationToken);
        if (existing != null)
            return existing;

        var patient = PatientAccount.Create(medplumPatientId, displayName, email, countryCode, preferredLanguage);
        await _dbContext.PatientAccounts.AddAsync(patient, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public async Task UpdateProfileAsync(
        Guid id,
        string displayName,
        string email,
        string countryCode,
        string preferredLanguage,
        CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.PatientAccounts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (patient != null)
        {
            patient.UpdateProfile(displayName, email, countryCode, preferredLanguage);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.PatientAccounts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        patient?.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.PatientAccounts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        patient?.Activate();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}