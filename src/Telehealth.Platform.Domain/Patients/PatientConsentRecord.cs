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

    public Guid PatientAccountId { get; private set; }

    public string ConsentType { get; private set; }

    public string Version { get; private set; }

    public string TextSnapshot { get; private set; }

    public string TextHash { get; private set; }

    public string Language { get; private set; }

    public string LegalBasis { get; private set; }

    public bool IsAccepted { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public DateTimeOffset CapturedAt { get; private set; }

    public DateTimeOffset? WithdrawnAt { get; private set; }
}
