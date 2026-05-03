using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Clinical;

public class ClinicalRecord : Entity<Guid>
{
    public Guid PatientAccountId { get; private set; }
    public string RecordType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset RecordedAt { get; private set; }
    public string RecordedBy { get; private set; } = string.Empty;
    public string? MedplumResourceId { get; private set; }
    public ClinicalRecordStatus Status { get; private set; } = ClinicalRecordStatus.Active;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private ClinicalRecord(
        Guid id,
        Guid patientAccountId,
        string recordType,
        string title,
        string content,
        string recordedBy) : base(id)
    {
        PatientAccountId = patientAccountId;
        RecordType = recordType;
        Title = title;
        Content = content;
        RecordedBy = recordedBy;
        RecordedAt = DateTimeOffset.UtcNow;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static ClinicalRecord Create(
        Guid patientAccountId,
        string recordType,
        string title,
        string content,
        string recordedBy,
        string? medplumResourceId = null)
    {
        return new ClinicalRecord(
            Guid.NewGuid(),
            patientAccountId,
            recordType,
            title,
            content,
            recordedBy)
        {
            MedplumResourceId = medplumResourceId
        };
    }

    public void UpdateContent(string content)
    {
        Content = content;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetStatus(ClinicalRecordStatus status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public enum ClinicalRecordStatus
{
    Active = 1,
    Archived = 2,
    Deleted = 3
}

public static class ClinicalRecordTypes
{
    public const string Diagnosis = "Diagnosis";
    public const string Treatment = "Treatment";
    public const string Prescription = "Prescription";
    public const string LabResult = "LabResult";
    public const string VitalSign = "VitalSign";
    public const string Immunization = "Immunization";
    public const string Procedure = "Procedure";
    public const string Note = "Note";
}