using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Analytics.Services;

namespace Telehealth.Platform.Analytics.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictionController : ControllerBase
{
    private readonly IPredictiveModelingService _predictiveService;
    private readonly ILogger<PredictionController> _logger;

    public PredictionController(IPredictiveModelingService predictiveService, ILogger<PredictionController> logger)
    {
        _predictiveService = predictiveService;
        _logger = logger;
    }

    [HttpPost("predict")]
    public async Task<ActionResult<PredictionResult>> PredictMetric([FromBody] PredictRequest request)
    {
        try
        {
            var prediction = await _predictiveService.PredictMetricAsync(request.MetricName, request.DaysAhead);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting metric");
            return StatusCode(500, new { error = "Failed to predict metric" });
        }
    }

    [HttpPost("batch-predict")]
    public async Task<ActionResult<List<PredictionResult>>> BatchPredict([FromBody] BatchPredictRequest request)
    {
        try
        {
            var predictions = await _predictiveService.BatchPredictAsync(request.Metrics, request.DaysAhead);
            return Ok(predictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch predicting metrics");
            return StatusCode(500, new { error = "Failed to batch predict metrics" });
        }
    }

    [HttpPost("train/{metricName}")]
    public async Task<ActionResult> TrainModel(string metricName)
    {
        try
        {
            var result = await _predictiveService.TrainModelAsync(metricName);
            if (result)
            {
                return Ok(new { message = $"Model trained successfully for {metricName}" });
            }
            return BadRequest(new { error = "Failed to train model" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training model");
            return StatusCode(500, new { error = "Failed to train model" });
        }
    }
}

public record PredictRequest(string MetricName, int DaysAhead);
public record BatchPredictRequest(List<string> Metrics, int DaysAhead);
