using Telehealth.Platform.Domain.Billing;

namespace Telehealth.Platform.Application.Abstractions.Billing;

public interface IInsuranceProviderService
{
    Task<InsuranceProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InsuranceProvider?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<InsuranceProvider>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InsuranceProvider> CreateAsync(string name, string code, string countryCode, string? phoneNumber = null, string? email = null, string? website = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, string name, string? phoneNumber, string? email, string? website, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}