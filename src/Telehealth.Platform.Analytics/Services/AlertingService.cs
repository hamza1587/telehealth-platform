namespace Telehealth.Platform.Analytics.Services;

public class AlertingService : IAlertingService
{
    private readonly Dictionary<Guid, Alert> _alerts;
    private readonly Dictionary<string, (double threshold, AlertSeverity severity)> _thresholds;

    public AlertingService()
    {
        _alerts = new Dictionary<Guid, Alert>();
        _thresholds = new Dictionary<string, (double, AlertSeverity)>();
        
        // Initialize default thresholds
        _thresholds["Video Call Success Rate"] = (95.0, AlertSeverity.Warning);
        _thresholds["Prescription Success Rate"] = (95.0, AlertSeverity.Warning);
        _thresholds["Average Wait Time"] = (10.0, AlertSeverity.Critical);
        _thresholds["Patient Satisfaction"] = (3.5, AlertSeverity.Warning);
    }

    public async Task<Alert> CreateAlertAsync(string metricName, double threshold, AlertSeverity severity)
    {
        var alert = new Alert(
            Guid.NewGuid(),
            metricName,
            threshold,
            0, // Will be updated when triggered
            severity,
            $"Alert created for {metricName} with threshold {threshold}",
            DateTimeOffset.UtcNow,
            false,
            null
        );

        _alerts[alert.Id] = alert;
        return await Task.FromResult(alert);
    }

    public async Task<List<Alert>> GetActiveAlertsAsync()
    {
        var activeAlerts = _alerts.Values.Where(a => !a.IsResolved).ToList();
        return await Task.FromResult(activeAlerts);
    }

    public async Task<Alert> ResolveAlertAsync(Guid alertId)
    {
        if (!_alerts.ContainsKey(alertId))
        {
            throw new ArgumentException("Alert not found", nameof(alertId));
        }

        var alert = _alerts[alertId];
        alert = alert with { IsResolved = true, ResolvedAt = DateTimeOffset.UtcNow };
        _alerts[alertId] = alert;

        return await Task.FromResult(alert);
    }

    public async Task<bool> CheckThresholdsAsync(Dictionary<string, double> currentMetrics)
    {
        var alertsTriggered = false;

        foreach (var metric in currentMetrics)
        {
            if (_thresholds.TryGetValue(metric.Key, out var thresholdConfig))
            {
                var (threshold, severity) = thresholdConfig;
                
                // Check if threshold is breached
                var isBreach = metric.Key.Contains("Rate") || metric.Key.Contains("Satisfaction")
                    ? metric.Value < threshold
                    : metric.Value > threshold;

                if (isBreach)
                {
                    var alert = new Alert(
                        Guid.NewGuid(),
                        metric.Key,
                        threshold,
                        metric.Value,
                        severity,
                        $"{metric.Key} threshold breached: {metric.Value} vs {threshold}",
                        DateTimeOffset.UtcNow,
                        false,
                        null
                    );

                    _alerts[alert.Id] = alert;
                    alertsTriggered = true;
                }
            }
        }

        return await Task.FromResult(alertsTriggered);
    }
}
