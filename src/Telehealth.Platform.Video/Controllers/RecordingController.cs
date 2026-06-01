using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Video.Domain;
using Telehealth.Platform.Video.Services;

namespace Telehealth.Platform.Video.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecordingController : ControllerBase
{
    private readonly IRecordingService _recordingService;
    private readonly ILogger<RecordingController> _logger;

    public RecordingController(IRecordingService recordingService, ILogger<RecordingController> logger)
    {
        _recordingService = recordingService;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<ActionResult<RecordingSession>> StartRecording([FromBody] StartRecordingRequest request)
    {
        try
        {
            var session = await _recordingService.StartRecordingAsync(request.RoomId, request.RequiresConsent);
            return CreatedAtAction(nameof(GetRecordingSession), new { id = session.Id }, session);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting recording");
            return StatusCode(500, new { error = "Failed to start recording" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecordingSession>> GetRecordingSession(Guid id)
    {
        try
        {
            var session = await _recordingService.GetRecordingSessionAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recording session");
            return StatusCode(500, new { error = "Failed to get recording session" });
        }
    }

    [HttpPost("{id}/stop")]
    public async Task<ActionResult<RecordingSession>> StopRecording(Guid id)
    {
        try
        {
            var session = await _recordingService.StopRecordingAsync(id);
            return Ok(session);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording");
            return StatusCode(500, new { error = "Failed to stop recording" });
        }
    }

    [HttpPost("{id}/consent")]
    public async Task<ActionResult<RecordingSession>> GrantConsent(Guid id)
    {
        try
        {
            var session = await _recordingService.GrantConsentAsync(id);
            return Ok(session);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting consent");
            return StatusCode(500, new { error = "Failed to grant consent" });
        }
    }
}

public record StartRecordingRequest(Guid RoomId, bool RequiresConsent);
