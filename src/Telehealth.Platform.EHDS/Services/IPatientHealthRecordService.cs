using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.EHDS.Services;

public interface IPatientHealthRecordService
{
    Task<PatientHealthRecord> CreateRecordAsync(Guid patientId, string dataOrigin, bool isCrossBorderAccessible = false);
    Task<PatientHealthRecord?> GetRecordAsync(Guid id);
    Task<PatientHealthRecord?> GetRecordByPatientIdAsync(Guid patientId);
    Task<List<PatientHealthRecord>> GetAllRecordsAsync();
    Task<PatientHealthRecord> AddConditionAsync(Guid recordId, Condition condition);
    Task<PatientHealthRecord> AddMedicationAsync(Guid recordId, Medication medication);
    Task<PatientHealthRecord> SetCrossBorderAccessAsync(Guid recordId, bool accessible);
    Task<string> ExportToFhirAsync(Guid recordId);
    Task<string> ImportFromFhirAsync(string fhirJson);
}
