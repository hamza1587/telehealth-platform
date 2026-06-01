using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Video.Domain;
using Telehealth.Platform.Video.Services;

namespace Telehealth.Platform.Video.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoRoomController : ControllerBase
{
    private readonly IVideoService _videoService;
    private readonly ILogger<VideoRoomController> _logger;

    public VideoRoomController(IVideoService videoService, ILogger<VideoRoomController> logger)
    {
        _videoService = videoService;
        _logger = logger;
    }

    [HttpPost("rooms")]
    public async Task<ActionResult<VideoRoom>> CreateRoom([FromBody] CreateRoomRequest request)
    {
        try
        {
            var room = await _videoService.CreateRoomAsync(request.RoomName, request.MaxParticipants);
            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating video room");
            return StatusCode(500, new { error = "Failed to create room" });
        }
    }

    [HttpGet("rooms/{id}")]
    public async Task<ActionResult<VideoRoom>> GetRoom(Guid id)
    {
        try
        {
            var room = await _videoService.GetRoomAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return Ok(room);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video room");
            return StatusCode(500, new { error = "Failed to get room" });
        }
    }

    [HttpPost("rooms/{id}/activate")]
    public async Task<ActionResult<VideoRoom>> ActivateRoom(Guid id)
    {
        try
        {
            var room = await _videoService.ActivateRoomAsync(id);
            return Ok(room);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating video room");
            return StatusCode(500, new { error = "Failed to activate room" });
        }
    }

    [HttpPost("rooms/{id}/end")]
    public async Task<ActionResult<VideoRoom>> EndRoom(Guid id)
    {
        try
        {
            var room = await _videoService.EndRoomAsync(id);
            return Ok(room);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending video room");
            return StatusCode(500, new { error = "Failed to end room" });
        }
    }

    [HttpPost("rooms/{id}/recording/enable")]
    public async Task<ActionResult<VideoRoom>> EnableRecording(Guid id)
    {
        try
        {
            var room = await _videoService.EnableRecordingAsync(id);
            return Ok(room);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling recording");
            return StatusCode(500, new { error = "Failed to enable recording" });
        }
    }

    [HttpPost("rooms/{id}/recording/disable")]
    public async Task<ActionResult<VideoRoom>> DisableRecording(Guid id)
    {
        try
        {
            var room = await _videoService.DisableRecordingAsync(id);
            return Ok(room);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling recording");
            return StatusCode(500, new { error = "Failed to disable recording" });
        }
    }

    [HttpPost("rooms/{id}/screen-share/enable")]
    public async Task<ActionResult<VideoRoom>> EnableScreenSharing(Guid id)
    {
        try
        {
            var room = await _videoService.EnableScreenSharingAsync(id);
            return Ok(room);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling screen sharing");
            return StatusCode(500, new { error = "Failed to enable screen sharing" });
        }
    }

    [HttpPost("rooms/{id}/screen-share/disable")]
    public async Task<ActionResult<VideoRoom>> DisableScreenSharing(Guid id)
    {
        try
        {
            var room = await _videoService.DisableScreenSharingAsync(id);
            return Ok(room);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling screen sharing");
            return StatusCode(500, new { error = "Failed to disable screen sharing" });
        }
    }

    [HttpPost("rooms/{id}/token")]
    public async Task<ActionResult<string>> GenerateToken(Guid id, [FromBody] TokenRequest request)
    {
        try
        {
            var token = await _videoService.GenerateRoomTokenAsync(id, request.ParticipantIdentity);
            return Ok(new { token });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token");
            return StatusCode(500, new { error = "Failed to generate token" });
        }
    }
}

public record CreateRoomRequest(string RoomName, int MaxParticipants);
public record TokenRequest(string ParticipantIdentity);
