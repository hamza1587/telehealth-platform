using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Clinical;

/// <summary>
/// Diagnosis recorded for a patient.
/// </summary>
public sealed class Diagnosis : Entity<Guid>
{
    public Diagnosis(
        Guid id,
        Guid patientAccountId,
        Guid? consultationSessionId,
        string icd10Code,
        string name,
        string? description,
        DiagnosisType type,
        DiagnosisStatus status,
        DateOnly? onsetDate,
        DateOnly? resolutionDate,
        string? severity,
        Guid doctorProfileId,
        DateTimeOffset createdAt)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        ConsultationSessionId = consultationSessionId;
        Icd10Code = icd10Code;
        Name = name;
        Description = description;
        Type = type;
        Status = status;
        OnsetDate = onsetDate;
        ResolutionDate = resolutionDate;
        Severity = severity;
        DoctorProfileId = doctorProfileId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid PatientAccountId { get; private set; }
    public Guid? ConsultationSessionId { get; private set; }
    public string Icd10Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DiagnosisType Type { get; private set; }
    public DiagnosisStatus Status { get; private set; }
    public DateOnly? OnsetDate { get; private set; }
    public DateOnly? ResolutionDate { get; private set; }
    public string? Severity { get; private set; }
    public Guid DoctorProfileId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string name,
        string? description,
        DiagnosisStatus status,
        DateOnly? resolutionDate,
        string? severity,
        DateTimeOffset updatedAt)
    {
        Name = name;
        Description = description;
        Status = status;
        ResolutionDate = resolutionDate;
        Severity = severity;
        UpdatedAt = updatedAt;
    }

    public void Resolve(DateOnly resolutionDate, DateTimeOffset updatedAt)
    {
        Status = DiagnosisStatus.Resolved;
        ResolutionDate = resolutionDate;
        UpdatedAt = updatedAt;
    }
}

public enum DiagnosisType
{
    Primary,
    Secondary,
    Differential,
    RuleOut,
    History
}

public enum DiagnosisStatus
{
    Active,
    Resolved,
    Chronic,
    InRemission,
    RuledOut
}
