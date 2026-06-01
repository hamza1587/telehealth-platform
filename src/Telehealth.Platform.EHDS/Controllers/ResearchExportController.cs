using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Domain.Entities;
using Telehealth.Platform.EHDS.Services;

namespace Telehealth.Platform.EHDS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResearchExportController : ControllerBase
{
    private readonly IResearchExportService _service;

    public ResearchExportController(IResearchExportService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ResearchExportRequest>> CreateExportRequest([FromBody] CreateExportRequest request)
    {
        var exportRequest = await _service.CreateExportRequestAsync(
            request.RequesterId,
            request.ResearchPurpose,
            request.DataDomains,
            request.DeidentificationMethod,
            request.KAnonymityLevel,
            request.Epsilon);
        return CreatedAtAction(nameof(GetExportRequest), new { id = exportRequest.Id }, exportRequest);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResearchExportRequest>> GetExportRequest(Guid id)
    {
        var request = await _service.GetExportRequestAsync(id);
        if (request == null)
            return NotFound();
        return Ok(request);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<ResearchExportRequest>>> GetPendingRequests()
    {
        var requests = await _service.GetPendingRequestsAsync();
        return Ok(requests);
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ResearchExportRequest>> ApproveRequest(Guid id, [FromBody] ApproveRequest request)
    {
        var exportRequest = await _service.ApproveRequestAsync(id, request.ApproverId);
        return Ok(exportRequest);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<ResearchExportRequest>> RejectRequest(Guid id, [FromBody] RejectRequest request)
    {
        var exportRequest = await _service.RejectRequestAsync(id, request.ApproverId, request.Reason);
        return Ok(exportRequest);
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult<ResearchExportRequest>> StartProcessing(Guid id)
    {
        var exportRequest = await _service.StartProcessingAsync(id);
        return Ok(exportRequest);
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<ResearchExportRequest>> CompleteExport(Guid id, [FromBody] CompleteExportRequest request)
    {
        var exportRequest = await _service.CompleteExportAsync(id, request.ExportUrl);
        return Ok(exportRequest);
    }

    [HttpPost("{id}/fail")]
    public async Task<ActionResult<ResearchExportRequest>> FailExport(Guid id, [FromBody] FailExportRequest request)
    {
        var exportRequest = await _service.FailExportAsync(id, request.ErrorMessage);
        return Ok(exportRequest);
    }
}

public record CreateExportRequest(
    Guid RequesterId,
    string ResearchPurpose,
    List<string> DataDomains,
    string DeidentificationMethod = "k_anonymity",
    int KAnonymityLevel = 5,
    double Epsilon = 1.0);

public record ApproveRequest(string ApproverId);
public record RejectRequest(string ApproverId, string Reason);
public record CompleteExportRequest(string ExportUrl);
public record FailExportRequest(string ErrorMessage);
