using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.Integrations.Healthcare.Services;

public interface ILabIntegrationService
{
    Task<LabResult> SubmitLabResultAsync(LabResult labResult);
    Task<LabResult?> GetLabResultAsync(Guid id);
    Task<List<LabResult>> GetLabResultsByPatientAsync(Guid patientId);
    Task<List<LabResult>> GetLabResultsByLabSystemAsync(string labSystemId);
    Task<string> SyncWithLabSystemAsync(string labSystemId);
}
