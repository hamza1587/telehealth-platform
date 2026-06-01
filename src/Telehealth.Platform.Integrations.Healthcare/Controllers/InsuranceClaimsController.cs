using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Domain.Entities;
using Telehealth.Platform.Integrations.Healthcare.Services;

namespace Telehealth.Platform.Integrations.Healthcare.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InsuranceClaimsController : ControllerBase
{
    private readonly IInsuranceIntegrationService _service;

    public InsuranceClaimsController(IInsuranceIntegrationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<InsuranceClaim>> SubmitClaim([FromBody] InsuranceClaim claim)
    {
        var result = await _service.SubmitClaimAsync(claim);
        return CreatedAtAction(nameof(GetClaim), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InsuranceClaim>> GetClaim(Guid id)
    {
        var claim = await _service.GetClaimAsync(id);
        if (claim == null)
            return NotFound();
        return Ok(claim);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<List<InsuranceClaim>>> GetClaimsByPatient(Guid patientId)
    {
        var claims = await _service.GetClaimsByPatientAsync(patientId);
        return Ok(claims);
    }

    [HttpGet("provider/{insuranceProviderId}")]
    public async Task<ActionResult<List<InsuranceClaim>>> GetClaimsByProvider(string insuranceProviderId)
    {
        var claims = await _service.GetClaimsByProviderAsync(insuranceProviderId);
        return Ok(claims);
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<InsuranceClaim>> ApproveClaim(Guid id, [FromBody] ApproveClaimRequest request)
    {
        var claim = await _service.ApproveClaimAsync(id, request.ClaimNumber);
        return Ok(claim);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<InsuranceClaim>> RejectClaim(Guid id, [FromBody] RejectClaimRequest request)
    {
        var claim = await _service.RejectClaimAsync(id, request.Reason);
        return Ok(claim);
    }

    [HttpPost("sync/{insuranceProviderId}")]
    public async Task<ActionResult<string>> SyncWithInsuranceProvider(string insuranceProviderId)
    {
        var result = await _service.SyncWithInsuranceProviderAsync(insuranceProviderId);
        return Ok(new { message = result });
    }
}

public record ApproveClaimRequest(string ClaimNumber);
public record RejectClaimRequest(string Reason);
