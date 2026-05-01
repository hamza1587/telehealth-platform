using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Patients;

public sealed class PatientConsentRecord : Entity<Guid>
{
    public PatientConsentRecord(
        Guid id,
        Guid patientAccountId,
        string consentType,
        string version,
        string textSnapshot,
        string textHash,
        string language,
        string legalBasis,
        bool isAccepted,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset capturedAt)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        ConsentType = consentType;
        Version = version;
        TextSnapshot = textSnapshot;
        TextHash = textHash;
        Language = language;
        LegalBasis = legalBasis;
        IsAccepted = isAccepted;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CapturedAt = capturedAt;
    }

    public Guid PatientAccountId { get; }

    public string ConsentType { get; }

    public string Version { get; }

    public string TextSnapshot { get; }

    public string TextHash { get; }

    public string Language { get; }

    public string LegalBasis { get; }

    public bool IsAccepted { get; }

    public string? IpAddress { get; }

    public string? UserAgent { get; }

    public DateTimeOffset CapturedAt { get; }

    public DateTimeOffset? WithdrawnAt { get; private set; }
}
