using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Billing;
using Telehealth.Platform.Domain.Billing;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Billing;

public class InsuranceProviderService : IInsuranceProviderService
{
    private readonly PlatformDbContext _dbContext;

    public InsuranceProviderService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InsuranceProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InsuranceProviders.FindAsync([id], cancellationToken);
    }

    public async Task<InsuranceProvider?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InsuranceProviders.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<InsuranceProvider>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.InsuranceProviders.ToListAsync(cancellationToken);
    }

    public async Task<InsuranceProvider> CreateAsync(
        string name,
        string code,
        string countryCode,
        string? phoneNumber = null,
        string? email = null,
        string? website = null,
        CancellationToken cancellationToken = default)
    {
        var provider = InsuranceProvider.Create(name, code, countryCode, phoneNumber, email, website);
        await _dbContext.InsuranceProviders.AddAsync(provider, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return provider;
    }

    public async Task UpdateAsync(
        Guid id,
        string name,
        string? phoneNumber,
        string? email,
        string? website,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var provider = await _dbContext.InsuranceProviders.FindAsync([id], cancellationToken);
        if (provider != null)
        {
            provider.Update(name, phoneNumber, email, website, isActive);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _dbContext.InsuranceProviders.FindAsync([id], cancellationToken);
        if (provider != null)
        {
            _dbContext.InsuranceProviders.Remove(provider);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}