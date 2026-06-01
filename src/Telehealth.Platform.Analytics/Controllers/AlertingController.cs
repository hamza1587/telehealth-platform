using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Analytics.Services;

namespace Telehealth.Platform.Analytics.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertingController : ControllerBase
{
    private readonly IAlertingService _alertingService;
    private readonly ILogger<AlertingController> _logger;

    public AlertingController(IAlertingService alertingService, ILogger<AlertingController> logger)
    {
        _alertingService = alertingService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Alert>> CreateAlert([FromBody] CreateAlertRequest request)
    {
        try
        {
            var alert = await _alertingService.CreateAlertAsync(request.MetricName, request.Threshold, request.Severity);
            return CreatedAtAction(nameof(GetActiveAlerts), new { id = alert.Id }, alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alert");
            return StatusCode(500, new { error = "Failed to create alert" });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<Alert>>> GetActiveAlerts()
    {
        try
        {
            var alerts = await _alertingService.GetActiveAlertsAsync();
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active alerts");
            return StatusCode(500, new { error = "Failed to get alerts" });
        }
    }

    [HttpPost("{id}/resolve")]
    public async Task<ActionResult<Alert>> ResolveAlert(Guid id)
    {
        try
        {
            var alert = await _alertingService.ResolveAlertAsync(id);
            return Ok(alert);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving alert");
            return StatusCode(500, new { error = "Failed to resolve alert" });
        }
    }

    [HttpPost("check-thresholds")]
    public async Task<ActionResult> CheckThresholds([FromBody] Dictionary<string, double> currentMetrics)
    {
        try
        {
            var alertsTriggered = await _alertingService.CheckThresholdsAsync(currentMetrics);
            return Ok(new { alertsTriggered });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking thresholds");
            return StatusCode(500, new { error = "Failed to check thresholds" });
        }
    }
}

public record CreateAlertRequest(string MetricName, double Threshold, AlertSeverity Severity);
