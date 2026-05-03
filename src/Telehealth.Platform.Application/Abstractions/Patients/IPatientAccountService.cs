using Telehealth.Platform.Domain.Patients;

namespace Telehealth.Platform.Application.Abstractions.Patients;

public interface IPatientAccountService
{
    Task<PatientAccount?> GetByMedplumIdAsync(Guid medplumPatientId, CancellationToken cancellationToken = default);
    Task<PatientAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PatientAccount> CreateAsync(Guid medplumPatientId, string displayName, string email, string countryCode, string preferredLanguage, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(Guid id, string displayName, string email, string countryCode, string preferredLanguage, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
}