namespace Telehealth.Platform.Analytics.Services;

public record PredictionResult(
    string MetricName,
    double PredictedValue,
    double Confidence,
    DateTimeOffset PredictedFor,
    Dictionary<string, double> Features
);

public interface IPredictiveModelingService
{
    Task<PredictionResult> PredictMetricAsync(string metricName, int daysAhead);
    Task<List<PredictionResult>> BatchPredictAsync(List<string> metrics, int daysAhead);
    Task<bool> TrainModelAsync(string metricName);
}
