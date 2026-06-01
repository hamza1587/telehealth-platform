using Telehealth.Platform.Domain.Entities;
using Telehealth.Platform.Domain.Services;

namespace Telehealth.Platform.EHDS.Services;

public class DeIdentificationService : IDeIdentificationService
{
    private readonly KAnonymityService _kAnonymityService;
    private readonly DifferentialPrivacyService _differentialPrivacyService;

    public DeIdentificationService()
    {
        _kAnonymityService = new KAnonymityService();
        _differentialPrivacyService = new DifferentialPrivacyService();
    }

    /// <summary>
    /// De-identifies patient health records for research use
    /// </summary>
    public List<Dictionary<string, object>> DeIdentifyRecords(
        List<PatientHealthRecord> records,
        int kAnonymityLevel = 5,
        double epsilon = 1.0)
    {
        // Convert records to dictionary format
        var data = records.Select(r => new Dictionary<string, object>
        {
            { "PatientId", HashValue(r.PatientId.ToString()) },
            { "Conditions", r.Conditions.Count },
            { "Medications", r.Medications.Count },
            { "Allergies", r.Allergies.Count },
            { "Immunizations", r.Immunizations.Count },
            { "Procedures", r.Procedures.Count },
            { "Observations", r.Observations.Count },
            { "LastUpdated", r.LastUpdated.ToString("yyyy-MM") },
            { "DataOrigin", r.DataOrigin }
        }).ToList();

        // Apply k-anonymity
        var quasiIdentifiers = new List<string> { "LastUpdated", "DataOrigin" };
        var kAnonymizedData = _kAnonymityService.ApplyKAnonymity(data, quasiIdentifiers, kAnonymityLevel);

        // Apply differential privacy to numeric values
        var privateData = kAnonymizedData.Select(record => new Dictionary<string, object>
        {
            { "PatientId", record["PatientId"] },
            { "Conditions", _differentialPrivacyService.ComputePrivateCount((int)record["Conditions"], epsilon) },
            { "Medications", _differentialPrivacyService.ComputePrivateCount((int)record["Medications"], epsilon) },
            { "Allergies", _differentialPrivacyService.ComputePrivateCount((int)record["Allergies"], epsilon) },
            { "Immunizations", _differentialPrivacyService.ComputePrivateCount((int)record["Immunizations"], epsilon) },
            { "Procedures", _differentialPrivacyService.ComputePrivateCount((int)record["Procedures"], epsilon) },
            { "Observations", _differentialPrivacyService.ComputePrivateCount((int)record["Observations"], epsilon) },
            { "LastUpdated", record["LastUpdated"] },
            { "DataOrigin", record["DataOrigin"] }
        }).ToList();

        return privateData;
    }

    /// <summary>
    /// De-identifies a single patient health record
    /// </summary>
    public Dictionary<string, object> DeIdentifyRecord(
        PatientHealthRecord record,
        int kAnonymityLevel = 5,
        double epsilon = 1.0)
    {
        return DeIdentifyRecords(new List<PatientHealthRecord> { record }, kAnonymityLevel, epsilon).First();
    }

    /// <summary>
    /// Validates that de-identified data meets privacy requirements
    /// </summary>
    public bool ValidatePrivacy(
        List<Dictionary<string, object>> data,
        int kAnonymityLevel,
        double epsilon)
    {
        var quasiIdentifiers = new List<string> { "LastUpdated", "DataOrigin" };
        var satisfiesKAnonymity = _kAnonymityService.CheckKAnonymity(data, quasiIdentifiers, kAnonymityLevel);
        
        return satisfiesKAnonymity;
    }

    /// <summary>
    /// Calculates the current k-anonymity level of the dataset
    /// </summary>
    public int CalculateCurrentKAnonymity(List<Dictionary<string, object>> data)
    {
        var quasiIdentifiers = new List<string> { "LastUpdated", "DataOrigin" };
        return _kAnonymityService.CalculateKAnonymityLevel(data, quasiIdentifiers);
    }

    /// <summary>
    /// Hashes a value for additional privacy
    /// </summary>
    private string HashValue(string value)
    {
        return _kAnonymityService.HashValue(value);
    }

    /// <summary>
    /// Removes direct identifiers from a record
    /// </summary>
    public Dictionary<string, object> RemoveDirectIdentifiers(Dictionary<string, object> record)
    {
        var directIdentifiers = new List<string> { "PatientId", "PatientName", "SSN", "Email", "Phone" };
        var sanitizedRecord = new Dictionary<string, object>(record);
        
        foreach (var identifier in directIdentifiers)
        {
            if (sanitizedRecord.ContainsKey(identifier))
            {
                sanitizedRecord.Remove(identifier);
            }
        }
        
        return sanitizedRecord;
    }
}
