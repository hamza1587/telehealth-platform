using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Analytics.Services;

namespace Telehealth.Platform.Analytics.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AggregationController : ControllerBase
{
    private readonly IAggregationEngine _aggregationEngine;
    private readonly ILogger<AggregationController> _logger;

    public AggregationController(IAggregationEngine aggregationEngine, ILogger<AggregationController> logger)
    {
        _aggregationEngine = aggregationEngine;
        _logger = logger;
    }

    [HttpPost("aggregate")]
    public async Task<ActionResult<Dictionary<string, double>>> AggregateMetrics([FromBody] AggregateRequest request)
    {
        try
        {
            var result = await _aggregationEngine.AggregateMetricsAsync(request.MetricType, request.TimeRange);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating metrics");
            return StatusCode(500, new { error = "Failed to aggregate metrics" });
        }
    }

    [HttpPost("groupby")]
    public async Task<ActionResult<List<Dictionary<string, object>>>> GroupBy([FromBody] GroupByRequest request)
    {
        try
        {
            var result = await _aggregationEngine.GroupByAsync(request.Field, request.Metric);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error grouping data");
            return StatusCode(500, new { error = "Failed to group data" });
        }
    }

    [HttpGet("trend/{metric}/{periods}")]
    public async Task<ActionResult<double>> CalculateTrend(string metric, int periods)
    {
        try
        {
            var trend = await _aggregationEngine.CalculateTrendAsync(metric, periods);
            return Ok(new { metric, periods, trend });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating trend");
            return StatusCode(500, new { error = "Failed to calculate trend" });
        }
    }
}

public record AggregateRequest(string MetricType, string TimeRange);
public record GroupByRequest(string Field, string Metric);
