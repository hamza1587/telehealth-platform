namespace Telehealth.Platform.Analytics.Services;

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public record Alert(
    Guid Id,
    string MetricName,
    double Threshold,
    double CurrentValue,
    AlertSeverity Severity,
    string Message,
    DateTimeOffset TriggeredAt,
    bool IsResolved,
    DateTimeOffset? ResolvedAt
);

public interface IAlertingService
{
    Task<Alert> CreateAlertAsync(string metricName, double threshold, AlertSeverity severity);
    Task<List<Alert>> GetActiveAlertsAsync();
    Task<Alert> ResolveAlertAsync(Guid alertId);
    Task<bool> CheckThresholdsAsync(Dictionary<string, double> currentMetrics);
}
