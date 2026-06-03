using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Video.Data;
using Telehealth.Platform.Video.Domain;
using Telehealth.Platform.Video.Services;

namespace Telehealth.Platform.Video.Controllers;

[ApiController]
[Route("api/rtc")]
public class SignalingController : ControllerBase
{
    private readonly IVideoService _videoService;
    private readonly ILiveKitTokenService _tokenService;
    private readonly VideoDbContext _context;
    private readonly ILogger<SignalingController> _logger;

    public SignalingController(
        IVideoService videoService,
        ILiveKitTokenService tokenService,
        VideoDbContext context,
        ILogger<SignalingController> logger)
    {
        _videoService = videoService;
        _tokenService = tokenService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Exchange a consultation ID for a LiveKit room token.
    /// The frontend calls this once before connecting to LiveKit.
    /// </summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(SignalingTokenResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<SignalingTokenResponse>> GetRoomToken([FromBody] SignalingTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ParticipantIdentity))
        {
            return BadRequest(new { error = "ParticipantIdentity is required" });
        }

        try
        {
            var session = await _context.CallSessions
                .FirstOrDefaultAsync(s =>
                    s.ConsultationId == request.ConsultationId &&
                    s.Status != CallSessionStatus.Ended &&
                    s.Status != CallSessionStatus.Failed);

            if (session == null)
            {
                return NotFound(new { error = "No active call session found for this consultation" });
            }

            var room = await _videoService.GetRoomAsync(session.RoomId);
            if (room == null)
            {
                return NotFound(new { error = "Video room not found" });
            }

            var token = _tokenService.GenerateToken(
                request.ParticipantIdentity,
                room.RoomName,
                canPublish: true,
                canSubscribe: true);

            _logger.LogInformation(
                "Issued LiveKit token for participant {Identity} in room {Room} (consultation {ConsultationId})",
                request.ParticipantIdentity, room.RoomName, request.ConsultationId);

            return Ok(new SignalingTokenResponse(
                token,
                _tokenService.GetServerUrl(),
                room.RoomName,
                session.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating room token for consultation {ConsultationId}", request.ConsultationId);
            return StatusCode(500, new { error = "Failed to generate room token" });
        }
    }
}

public record SignalingTokenRequest(Guid ConsultationId, string ParticipantIdentity);

public record SignalingTokenResponse(
    string Token,
    string ServerUrl,
    string RoomName,
    Guid CallSessionId);
