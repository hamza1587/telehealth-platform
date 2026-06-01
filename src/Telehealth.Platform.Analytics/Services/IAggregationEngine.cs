namespace Telehealth.Platform.Analytics.Services;

public interface IAggregationEngine
{
    Task<Dictionary<string, double>> AggregateMetricsAsync(string metricType, string timeRange);
    Task<List<Dictionary<string, object>>> GroupByAsync(string field, string metric);
    Task<double> CalculateTrendAsync(string metric, int periods);
}
