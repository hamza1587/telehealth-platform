namespace Telehealth.Platform.Application.Abstractions.EHR;

public interface IFhirService
{
    Task<FhirPatient?> GetPatientAsync(string patientId, CancellationToken cancellationToken = default);
    Task<FhirEncounter?> GetEncounterAsync(string encounterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FhirMedication>> GetMedicationsAsync(string patientId, CancellationToken cancellationToken = default);
    Task<bool> SyncPatientAsync(FhirPatient patient, CancellationToken cancellationToken = default);
    Task<string> CreateEncounterAsync(FhirEncounter encounter, CancellationToken cancellationToken = default);
}

public class FhirPatient
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTimeOffset BirthDate { get; set; }
    public string Gender { get; set; } = null!;
    public List<FhirAddress> Addresses { get; set; } = new();
    public List<FhirPhone> Phones { get; set; } = new();
}

public class FhirEncounter
{
    public string Id { get; set; } = null!;
    public string PatientId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Class { get; set; } = null!;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public List<FhirDiagnosis> Diagnoses { get; set; } = new();
}

public class FhirMedication
{
    public string Id { get; set; } = null!;
    public string PatientId { get; set; } = null!;
    public string MedicationCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTimeOffset AuthoredOn { get; set; }
}

public class FhirAddress
{
    public string Line { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}

public class FhirPhone
{
    public string System { get; set; } = null!;
    public string Value { get; set; } = null!;
    public bool IsPrimary { get; set; }
}

public class FhirDiagnosis
{
    public string Code { get; set; } = null!;
    public string Display { get; set; } = null!;
    public DateTimeOffset RecordedDate { get; set; }
}