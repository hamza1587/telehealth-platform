using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Domain.Entities;
using Telehealth.Platform.EHDS.Services;

namespace Telehealth.Platform.EHDS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportAuditController : ControllerBase
{
    private readonly IResearchExportService _researchService;
    private readonly ILogger<ExportAuditController> _logger;

    public ExportAuditController(IResearchExportService researchService, ILogger<ExportAuditController> logger)
    {
        _researchService = researchService;
        _logger = logger;
    }

    [HttpGet("{exportId}/audit-trail")]
    public async Task<ActionResult<AuditTrailResponse>> GetAuditTrail(Guid exportId)
    {
        var request = await _researchService.GetExportRequestAsync(exportId);
        if (request == null)
            return NotFound();

        var auditTrail = new List<AuditEvent>
        {
            new AuditEvent
            {
                Timestamp = request.RequestedAt,
                Action = "EXPORT_REQUESTED",
                Actor = request.RequesterId.ToString(),
                Details = $"Research purpose: {request.ResearchPurpose}"
            }
        };

        if (request.ApprovedAt.HasValue)
        {
            auditTrail.Add(new AuditEvent
            {
                Timestamp = request.ApprovedAt.Value,
                Action = "EXPORT_APPROVED",
                Actor = request.ApprovedBy ?? "Unknown",
                Details = "Request approved for processing"
            });
        }

        if (request.CompletedAt.HasValue)
        {
            auditTrail.Add(new AuditEvent
            {
                Timestamp = request.CompletedAt.Value,
                Action = "EXPORT_COMPLETED",
                Actor = "SYSTEM",
                Details = $"Export available at: {request.ExportUrl}"
            });
        }

        if (request.Status == ExportStatus.Failed)
        {
            auditTrail.Add(new AuditEvent
            {
                Timestamp = DateTimeOffset.UtcNow,
                Action = "EXPORT_FAILED",
                Actor = "SYSTEM",
                Details = request.RejectionReason ?? "Unknown error"
            });
        }

        return Ok(new AuditTrailResponse
        {
            ExportId = exportId,
            Events = auditTrail.OrderByDescending(e => e.Timestamp).ToList()
        });
    }

    [HttpGet("audit-log")]
    public async Task<ActionResult<List<AuditTrailResponse>>> GetAuditLogs(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var requests = await _researchService.GetPendingRequestsAsync();
        var auditLogs = requests.Select(r => new AuditTrailResponse
        {
            ExportId = r.Id,
            Events = new List<AuditEvent>
            {
                new AuditEvent
                {
                    Timestamp = r.RequestedAt,
                    Action = "EXPORT_REQUESTED",
                    Actor = r.RequesterId.ToString()
                }
            }
        }).ToList();

        return Ok(auditLogs.Skip((page - 1) * pageSize).Take(pageSize));
    }

    [HttpPost("{exportId}/track-access")]
    public async Task<ActionResult> TrackAccess(Guid exportId, [FromBody] AccessTrackingRequest request)
    {
        _logger.LogInformation(
            "Researcher {ResearcherId} accessed export {ExportId} from {IpAddress}",
            request.ResearcherId,
            exportId,
            request.IpAddress);

        return Ok(new { tracked = true });
    }
}

public record AuditTrailResponse
{
    public Guid ExportId { get; init; }
    public List<AuditEvent> Events { get; init; }
}

public record AuditEvent
{
    public DateTimeOffset Timestamp { get; init; }
    public string Action { get; init; }
    public string Actor { get; init; }
    public string? Details { get; init; }
}

public record AccessTrackingRequest
{
    public string ResearcherId { get; init; }
    public string IpAddress { get; init; }
}