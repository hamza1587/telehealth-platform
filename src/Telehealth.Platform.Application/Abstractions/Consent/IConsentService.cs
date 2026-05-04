using Telehealth.Platform.Domain.Consent;

namespace Telehealth.Platform.Application.Abstractions.Consent;

public interface IConsentService
{
    Task<ConsentCaptureResult> CaptureConsentAsync(
        Guid userId,
        Guid templateId,
        string consentText,
        bool isAccepted,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PatientConsent>> GetUserConsentsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> HasConsentAsync(
        Guid userId,
        ConsentType consentType,
        CancellationToken cancellationToken = default);

    Task<bool> WithdrawConsentAsync(
        Guid userId,
        Guid consentId,
        CancellationToken cancellationToken = default);

    Task<bool> RevokeConsentAsync(
        Guid userId,
        Guid consentId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<ConsentTemplate?> GetLatestTemplateAsync(
        ConsentType consentType,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ConsentAnalytics>> GetConsentAnalyticsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}

public class ConsentCaptureResult
{
    public bool Success { get; set; }
    public Guid? ConsentId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ConsentAnalytics
{
    public ConsentType ConsentType { get; set; }
    public int TotalCaptured { get; set; }
    public int Accepted { get; set; }
    public int Withdrawn { get; set; }
    public double AcceptanceRate { get; set; }
}