using Telehealth.Platform.Domain.Clinical;

namespace Telehealth.Platform.Application.Abstractions.Clinical;

public interface IClinicalRecordService
{
    Task<ClinicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClinicalRecord>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<ClinicalRecord> CreateAsync(
        Guid patientId,
        string recordType,
        string title,
        string content,
        string recordedBy,
        string? medplumResourceId = null,
        CancellationToken cancellationToken = default);
    Task UpdateContentAsync(Guid id, string content, CancellationToken cancellationToken = default);
    Task SetStatusAsync(Guid id, ClinicalRecordStatus status, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}