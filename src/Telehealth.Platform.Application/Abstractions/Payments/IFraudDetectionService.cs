namespace Telehealth.Platform.Application.Abstractions.Payments;

public interface IFraudDetectionService
{
    Task<FraudCheckResult> CheckFraudAsync(
        Guid patientAccountId,
        long amountMinor,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default);

    Task ReportSuspiciousActivityAsync(
        Guid patientAccountId,
        string reason,
        string ipAddress,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<FraudRule>> GetActiveFraudRulesAsync(CancellationToken cancellationToken = default);

    Task<bool> IsRateLimitedAsync(
        string ipAddress,
        string endpoint,
        CancellationToken cancellationToken = default);
}

public class FraudCheckResult
{
    public bool IsFraudulent { get; set; }
    public decimal RiskScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public IEnumerable<string> TriggeredRules { get; set; } = [];
}

public class FraudRule
{
    public string RuleId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Threshold { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
}