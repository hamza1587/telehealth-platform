using Telehealth.Platform.Domain.Billing;

namespace Telehealth.Platform.Application.Abstractions.Billing;

public interface IPatientInsuranceService
{
    Task<PatientInsurance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PatientInsurance>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<PatientInsurance> CreateAsync(Guid patientId, Guid providerId, string memberId, string? groupId = null, string? planName = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, string memberId, string? groupId, string? planName, DateTimeOffset? effectiveDate, DateTimeOffset? expirationDate, bool isPrimary, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PatientInsurance>> GetActiveByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
}