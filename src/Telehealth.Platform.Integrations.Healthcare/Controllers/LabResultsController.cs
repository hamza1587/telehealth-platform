using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Domain.Entities;
using Telehealth.Platform.Integrations.Healthcare.Services;

namespace Telehealth.Platform.Integrations.Healthcare.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabResultsController : ControllerBase
{
    private readonly ILabIntegrationService _service;

    public LabResultsController(ILabIntegrationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<LabResult>> SubmitLabResult([FromBody] LabResult labResult)
    {
        var result = await _service.SubmitLabResultAsync(labResult);
        return CreatedAtAction(nameof(GetLabResult), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LabResult>> GetLabResult(Guid id)
    {
        var result = await _service.GetLabResultAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<List<LabResult>>> GetLabResultsByPatient(Guid patientId)
    {
        var results = await _service.GetLabResultsByPatientAsync(patientId);
        return Ok(results);
    }

    [HttpGet("lab-system/{labSystemId}")]
    public async Task<ActionResult<List<LabResult>>> GetLabResultsByLabSystem(string labSystemId)
    {
        var results = await _service.GetLabResultsByLabSystemAsync(labSystemId);
        return Ok(results);
    }

    [HttpPost("sync/{labSystemId}")]
    public async Task<ActionResult<string>> SyncWithLabSystem(string labSystemId)
    {
        var result = await _service.SyncWithLabSystemAsync(labSystemId);
        return Ok(new { message = result });
    }
}
