using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Consent;

/// <summary>
/// Records a patient's consent to a specific consent template.
/// Immutable record with withdrawal support.
/// </summary>
public sealed class PatientConsent : Entity<Guid>
{
    public PatientConsent(
        Guid id,
        Guid patientAccountId,
        Guid consentTemplateId,
        string consentType,
        string version,
        string contentHash,
        bool isAccepted,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset capturedAt)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        ConsentTemplateId = consentTemplateId;
        ConsentType = consentType;
        Version = version;
        ContentHash = contentHash;
        IsAccepted = isAccepted;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CapturedAt = capturedAt;
    }

    public Guid PatientAccountId { get; private set; }
    public Guid ConsentTemplateId { get; private set; }
    public string ConsentType { get; private set; }
    public string Version { get; private set; }
    public string ContentHash { get; private set; }
    public bool IsAccepted { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset CapturedAt { get; private set; }
    public DateTimeOffset? WithdrawnAt { get; private set; }
    public string? WithdrawalReason { get; private set; }

    public void Withdraw(string? reason, DateTimeOffset withdrawnAt)
    {
        WithdrawnAt = withdrawnAt;
        WithdrawalReason = reason;
    }

    public bool IsCurrentlyActive()
    {
        return IsAccepted && !WithdrawnAt.HasValue;
    }
}
