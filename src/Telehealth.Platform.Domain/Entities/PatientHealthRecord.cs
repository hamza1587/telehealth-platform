namespace Telehealth.Platform.Domain.Entities;

public class PatientHealthRecord
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public List<Condition> Conditions { get; private set; }
    public List<Medication> Medications { get; private set; }
    public List<AllergyIntolerance> Allergies { get; private set; }
    public List<Immunization> Immunizations { get; private set; }
    public List<Procedure> Procedures { get; private set; }
    public List<Observation> Observations { get; private set; }
    public DateTimeOffset LastUpdated { get; private set; }
    public string DataOrigin { get; private set; }
    public bool IsCrossBorderAccessible { get; private set; }

    public PatientHealthRecord(
        Guid patientId,
        string dataOrigin,
        bool isCrossBorderAccessible = false)
    {
        Id = Guid.NewGuid();
        PatientId = patientId;
        DataOrigin = dataOrigin;
        IsCrossBorderAccessible = isCrossBorderAccessible;
        LastUpdated = DateTimeOffset.UtcNow;
        Conditions = new List<Condition>();
        Medications = new List<Medication>();
        Allergies = new List<AllergyIntolerance>();
        Immunizations = new List<Immunization>();
        Procedures = new List<Procedure>();
        Observations = new List<Observation>();
    }

    public void AddCondition(Condition condition)
    {
        Conditions.Add(condition);
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void AddMedication(Medication medication)
    {
        Medications.Add(medication);
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void SetCrossBorderAccess(bool accessible)
    {
        IsCrossBorderAccessible = accessible;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}

public class Condition
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string System { get; set; }
    public string Display { get; set; }
    public string ClinicalStatus { get; set; }
    public string VerificationStatus { get; set; }
    public DateTimeOffset OnsetDateTime { get; set; }
    public string? Notes { get; set; }
}

public class Medication
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string System { get; set; }
    public string Display { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? Prescriber { get; set; }
}

public class AllergyIntolerance
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string System { get; set; }
    public string Display { get; set; }
    public string ClinicalStatus { get; set; }
    public string Criticality { get; set; }
    public string? Reaction { get; set; }
}

public class Immunization
{
    public Guid Id { get; set; }
    public string VaccineCode { get; set; }
    public string System { get; set; }
    public string Display { get; set; }
    public DateTimeOffset AdministrationDate { get; set; }
    public string? LotNumber { get; set; }
    public string? Site { get; set; }
}

public class Procedure
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string System { get; set; }
    public string Display { get; set; }
    public DateTimeOffset PerformedDate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}

public class Observation
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string System { get; set; }
    public string Display { get; set; }
    public string Value { get; set; }
    public string Unit { get; set; }
    public DateTimeOffset EffectiveDateTime { get; set; }
    public string? ReferenceRange { get; set; }
}
