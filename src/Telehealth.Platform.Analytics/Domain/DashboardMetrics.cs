namespace Telehealth.Platform.Analytics.Domain.Models;

public class DashboardMetrics
{
    public Guid Id { get; private set; }
    public string MetricName { get; private set; } = string.Empty;
    public double Value { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string Dimensions { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    public DashboardMetrics()
    {
    }

    public DashboardMetrics(string metricName, double value, string category, string? dimensions = null)
    {
        Id = Guid.NewGuid();
        MetricName = metricName;
        Value = value;
        Category = category;
        Dimensions = dimensions ?? string.Empty;
        Timestamp = DateTimeOffset.UtcNow;
    }
}
