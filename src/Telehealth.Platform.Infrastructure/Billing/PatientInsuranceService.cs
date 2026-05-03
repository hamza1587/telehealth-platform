using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Billing;
using Telehealth.Platform.Domain.Billing;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Billing;

public class PatientInsuranceService : IPatientInsuranceService
{
    private readonly PlatformDbContext _dbContext;

    public PatientInsuranceService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PatientInsurance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PatientInsurances.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<PatientInsurance>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PatientInsurances
            .Where(pi => pi.PatientAccountId == patientId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PatientInsurance> CreateAsync(
        Guid patientId,
        Guid providerId,
        string memberId,
        string? groupId = null,
        string? planName = null,
        CancellationToken cancellationToken = default)
    {
        var insurance = PatientInsurance.Create(patientId, providerId, memberId, groupId, planName);
        await _dbContext.PatientInsurances.AddAsync(insurance, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return insurance;
    }

    public async Task UpdateAsync(
        Guid id,
        string memberId,
        string? groupId,
        string? planName,
        DateTimeOffset? effectiveDate,
        DateTimeOffset? expirationDate,
        bool isPrimary,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var insurance = await _dbContext.PatientInsurances.FindAsync([id], cancellationToken);
        if (insurance != null)
        {
            insurance.Update(memberId, groupId, planName, effectiveDate, expirationDate, isPrimary, isActive);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var insurance = await _dbContext.PatientInsurances.FindAsync([id], cancellationToken);
        if (insurance != null)
        {
            _dbContext.PatientInsurances.Remove(insurance);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<PatientInsurance>> GetActiveByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PatientInsurances
            .Where(pi => pi.PatientAccountId == patientId && pi.IsActive && !pi.IsExpired)
            .ToListAsync(cancellationToken);
    }
}