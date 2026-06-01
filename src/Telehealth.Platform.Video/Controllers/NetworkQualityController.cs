using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Video.Services;

namespace Telehealth.Platform.Video.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NetworkQualityController : ControllerBase
{
    private readonly INetworkQualityService _networkQualityService;
    private readonly ILogger<NetworkQualityController> _logger;

    public NetworkQualityController(INetworkQualityService networkQualityService, ILogger<NetworkQualityController> logger)
    {
        _networkQualityService = networkQualityService;
        _logger = logger;
    }

    [HttpGet("rooms/{roomId}/metrics")]
    public async Task<ActionResult<NetworkQualityMetrics>> GetQualityMetrics(Guid roomId)
    {
        try
        {
            var metrics = await _networkQualityService.GetQualityMetricsAsync(roomId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting network quality metrics");
            return StatusCode(500, new { error = "Failed to get metrics" });
        }
    }

    [HttpPost("rooms/{roomId}/metrics")]
    public async Task<ActionResult> RecordQualityMetrics(Guid roomId, [FromBody] NetworkQualityMetrics metrics)
    {
        try
        {
            await _networkQualityService.RecordQualityMetricsAsync(roomId, metrics);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording network quality metrics");
            return StatusCode(500, new { error = "Failed to record metrics" });
        }
    }
}
