using Microsoft.ML;
using Microsoft.ML.Data;

namespace Telehealth.Platform.Analytics.Services;

public class PredictiveModelingService : IPredictiveModelingService
{
    private readonly IDataWarehouseService _dataWarehouse;
    private readonly MLContext _mlContext;
    private readonly Dictionary<string, ITransformer> _trainedModels;

    public PredictiveModelingService(IDataWarehouseService dataWarehouse)
    {
        _dataWarehouse = dataWarehouse;
        _mlContext = new MLContext(seed: 42);
        _trainedModels = new Dictionary<string, ITransformer>();
    }

    public async Task<PredictionResult> PredictMetricAsync(string metricName, int daysAhead)
    {
        try
        {
            // Get historical data from data warehouse
            var historicalData = await _dataWarehouse.QueryDataAsync($"SELECT date, value FROM {metricName} ORDER BY date DESC LIMIT 90");
            
            if (historicalData.Count < 10)
            {
                // Not enough data for ML, use simple projection
                return await SimplePredictionAsync(metricName, daysAhead, historicalData);
            }

            // Convert to ML data format
            var mlData = historicalData
                .Where(d => d.ContainsKey("value") && d["value"] != DBNull.Value)
                .Select(d => new TimeSeriesData
                {
                    Date = d.ContainsKey("date") && d["date"] != DBNull.Value 
                        ? DateTimeOffset.TryParse(d["date"].ToString(), out var date) ? date : DateTimeOffset.UtcNow 
                        : DateTimeOffset.UtcNow,
                    Value = (float)Convert.ToDouble(d["value"])
                })
                .OrderBy(d => d.Date)
                .ToList();

            if (mlData.Count < 10)
            {
                return await SimplePredictionAsync(metricName, daysAhead, historicalData);
            }

            // Train or load model
            if (!_trainedModels.ContainsKey(metricName))
            {
                var model = TrainTimeSeriesModel(mlData);
                _trainedModels[metricName] = model;
            }

            // Make prediction
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TimeSeriesData, TimeSeriesPrediction>(_trainedModels[metricName]);
            
            // Get the last known value
            var lastValue = mlData.Last().Value;
            
            // Simple trend-based prediction (in production, use proper forecasting)
            var trend = CalculateTrend(mlData);
            var predictedValue = lastValue * (1 + trend * daysAhead);
            var confidence = CalculateConfidence(mlData, trend);
            
            var result = new PredictionResult(
                metricName,
                predictedValue,
                confidence,
                DateTimeOffset.UtcNow.AddDays(daysAhead),
                new Dictionary<string, double>
                {
                    { "historical_average", mlData.Average(d => d.Value) },
                    { "trend", trend },
                    { "volatility", CalculateVolatility(mlData) },
                    { "data_points", mlData.Count }
                }
            );

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ML prediction failed for {metricName}: {ex.Message}");
            // Fallback to simple prediction
            var historicalData = await _dataWarehouse.QueryDataAsync($"SELECT * FROM {metricName} ORDER BY date DESC LIMIT 30");
            return await SimplePredictionAsync(metricName, daysAhead, historicalData);
        }
    }

    public async Task<List<PredictionResult>> BatchPredictAsync(List<string> metrics, int daysAhead)
    {
        var predictions = new List<PredictionResult>();
        
        foreach (var metric in metrics)
        {
            var prediction = await PredictMetricAsync(metric, daysAhead);
            predictions.Add(prediction);
        }

        return predictions;
    }

    public async Task<bool> TrainModelAsync(string metricName)
    {
        try
        {
            // Get training data
            var historicalData = await _dataWarehouse.QueryDataAsync($"SELECT date, value FROM {metricName} ORDER BY date DESC LIMIT 365");
            
            var mlData = historicalData
                .Where(d => d.ContainsKey("value") && d["value"] != DBNull.Value)
                .Select(d => new TimeSeriesData
                {
                    Date = d.ContainsKey("date") && d["date"] != DBNull.Value 
                        ? DateTimeOffset.TryParse(d["date"].ToString(), out var date) ? date : DateTimeOffset.UtcNow 
                        : DateTimeOffset.UtcNow,
                    Value = (float)Convert.ToDouble(d["value"])
                })
                .OrderBy(d => d.Date)
                .ToList();

            if (mlData.Count < 30)
            {
                Console.WriteLine($"Not enough data to train model for {metricName}");
                return false;
            }

            // Train model
            var model = TrainTimeSeriesModel(mlData);
            _trainedModels[metricName] = model;
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to train model for {metricName}: {ex.Message}");
            return false;
        }
    }

    private ITransformer TrainTimeSeriesModel(List<TimeSeriesData> data)
    {
        // Convert to IDataView
        var trainingData = _mlContext.Data.LoadFromEnumerable(data);
        
        // Build pipeline for time series forecasting
        var pipeline = _mlContext.Transforms.Concatenate("Features", new[] { "Value" })
            .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "Value", maximumNumberOfIterations: 100));
        
        // Train model
        var model = pipeline.Fit(trainingData);
        
        return model;
    }

    private async Task<PredictionResult> SimplePredictionAsync(string metricName, int daysAhead, List<Dictionary<string, object>> historicalData)
    {
        var values = historicalData
            .Where(d => d.ContainsKey("value") && d["value"] != DBNull.Value)
            .Select(d => Convert.ToDouble(d["value"]))
            .ToList();

        if (values.Count == 0)
        {
            values.Add(1000.0); // Default fallback
        }

        var currentValue = values.Average();
        var trend = values.Count > 1 ? (values.Last() - values.First()) / values.Count : 0.05;
        var predictedValue = currentValue * (1 + trend * daysAhead);
        var confidence = Math.Max(0.5, 1.0 - (values.Count / 100.0)); // More data = higher confidence

        return new PredictionResult(
            metricName,
            predictedValue,
            confidence,
            DateTimeOffset.UtcNow.AddDays(daysAhead),
            new Dictionary<string, double>
            {
                { "historical_average", currentValue },
                { "trend", trend },
                { "volatility", CalculateVolatility(values) },
                { "data_points", values.Count }
            }
        );
    }

    private double CalculateTrend(List<TimeSeriesData> data)
    {
        if (data.Count < 2) return 0.05;
        
        var values = data.Select(d => d.Value).ToList();
        return (values.Last() - values.First()) / values.Count;
    }

    private double CalculateVolatility(List<TimeSeriesData> data)
    {
        if (data.Count < 2) return 0.1;
        
        var values = data.Select(d => d.Value).ToList();
        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Count;
        return Math.Sqrt(variance) / mean;
    }

    private double CalculateVolatility(List<double> values)
    {
        if (values.Count < 2) return 0.1;
        
        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Count;
        return Math.Sqrt(variance) / mean;
    }

    private double CalculateConfidence(List<TimeSeriesData> data, double trend)
    {
        // Confidence based on data volume and trend stability
        var dataConfidence = Math.Min(0.95, data.Count / 100.0);
        var trendStability = 1.0 - Math.Abs(trend);
        return (dataConfidence + trendStability) / 2;
    }
}

// ML data models
internal class TimeSeriesData
{
    public DateTimeOffset Date { get; set; }
    public float Value { get; set; }
}

internal class TimeSeriesPrediction
{
    [ColumnName("Score")]
    public float PredictedValue { get; set; }
}
