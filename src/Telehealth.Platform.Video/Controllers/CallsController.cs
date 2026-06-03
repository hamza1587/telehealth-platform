using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Video.Data;
using Telehealth.Platform.Video.Domain;
using Telehealth.Platform.Video.Services;

namespace Telehealth.Platform.Video.Controllers;

[ApiController]
[Route("api/calls")]
public class CallsController : ControllerBase
{
    private readonly IVideoService _videoService;
    private readonly VideoDbContext _context;
    private readonly ILogger<CallsController> _logger;

    public CallsController(
        IVideoService videoService,
        VideoDbContext context,
        ILogger<CallsController> logger)
    {
        _videoService = videoService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a call session for a consultation, provisioning the video room.
    /// Idempotent — returns existing session if one is already waiting/active.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CallSessionDto), 201)]
    [ProducesResponseType(typeof(CallSessionDto), 200)]
    public async Task<ActionResult<CallSessionDto>> CreateCall([FromBody] CreateCallRequest request)
    {
        try
        {
            // Return existing session if already open for this consultation
            var existing = await _context.CallSessions
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s =>
                    s.ConsultationId == request.ConsultationId &&
                    (s.Status == CallSessionStatus.Waiting || s.Status == CallSessionStatus.Active));

            if (existing != null)
            {
                return Ok(ToDto(existing));
            }

            var roomName = $"consultation-{request.ConsultationId:N}";
            var maxParticipants = request.MaxParticipants ?? 2;

            var room = await _videoService.CreateRoomAsync(roomName, maxParticipants);
            await _videoService.ActivateRoomAsync(room.Id);

            var session = new CallSession(request.ConsultationId, room.Id, request.InitiatedBy);
            _context.CallSessions.Add(session);
            await _context.SaveChangesAsync();

            // Reload with navigation property
            await _context.Entry(session).Reference(s => s.Room).LoadAsync();

            _logger.LogInformation(
                "Created call session {SessionId} for consultation {ConsultationId}, room {RoomName}",
                session.Id, request.ConsultationId, roomName);

            return CreatedAtAction(nameof(GetCall), new { id = session.Id }, ToDto(session));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating call session for consultation {ConsultationId}", request.ConsultationId);
            return StatusCode(500, new { error = "Failed to create call session" });
        }
    }

    /// <summary>
    /// Retrieve call session metadata: status, participant count, recording state.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CallSessionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CallSessionDto>> GetCall(Guid id)
    {
        try
        {
            var session = await _context.CallSessions
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null) return NotFound();

            return Ok(ToDto(session));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting call session {Id}", id);
            return StatusCode(500, new { error = "Failed to get call session" });
        }
    }

    /// <summary>
    /// End an active call session and close the underlying video room.
    /// </summary>
    [HttpPost("{id:guid}/end")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> EndCall(Guid id)
    {
        try
        {
            var session = await _context.CallSessions.FindAsync(id);
            if (session == null) return NotFound();

            session.End();
            await _videoService.EndRoomAsync(session.RoomId);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ended call session {SessionId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending call session {Id}", id);
            return StatusCode(500, new { error = "Failed to end call session" });
        }
    }

    private static CallSessionDto ToDto(CallSession s) => new(
        s.Id,
        s.ConsultationId,
        s.RoomId,
        s.Room?.RoomName ?? string.Empty,
        s.Status.ToString(),
        s.Room?.CurrentParticipants ?? 0,
        s.Room?.MaxParticipants ?? 0,
        s.Room?.IsRecording ?? false,
        s.Room?.HasScreenSharing ?? false,
        s.CreatedAt,
        s.StartedAt,
        s.EndedAt);
}

public record CreateCallRequest(
    Guid ConsultationId,
    string InitiatedBy,
    int? MaxParticipants = null);

public record CallSessionDto(
    Guid Id,
    Guid ConsultationId,
    Guid RoomId,
    string RoomName,
    string Status,
    int CurrentParticipants,
    int MaxParticipants,
    bool IsRecording,
    bool HasScreenSharing,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndedAt);
