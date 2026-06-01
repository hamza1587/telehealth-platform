using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Prescription.Services;

namespace Telehealth.Platform.Prescription.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PharmacyController : ControllerBase
{
    private readonly IPharmacyService _pharmacyService;
    private readonly ILogger<PharmacyController> _logger;

    public PharmacyController(IPharmacyService pharmacyService, ILogger<PharmacyController> logger)
    {
        _pharmacyService = pharmacyService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Pharmacy>>> GetNearbyPharmeries([FromQuery] string countryCode, [FromQuery] string? city = null)
    {
        try
        {
            var pharmacies = await _pharmacyService.GetNearbyPharmaciesAsync(countryCode, city);
            return Ok(pharmacies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nearby pharmacies");
            return StatusCode(500, new { error = "Failed to get pharmacies" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Pharmacy>> GetPharmacy(string id)
    {
        try
        {
            var pharmacy = await _pharmacyService.GetPharmacyAsync(id);
            if (pharmacy == null)
            {
                return NotFound();
            }
            return Ok(pharmacy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pharmacy");
            return StatusCode(500, new { error = "Failed to get pharmacy" });
        }
    }

    [HttpPost("send")]
    public async Task<ActionResult> SendToPharmacy([FromBody] SendToPharmacyRequest request)
    {
        try
        {
            var result = await _pharmacyService.SendToPharmacyAsync(request.PrescriptionId, request.PharmacyId);
            if (result)
            {
                return Ok(new { message = "Prescription sent to pharmacy successfully" });
            }
            return BadRequest(new { error = "Failed to send prescription to pharmacy" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending prescription to pharmacy");
            return StatusCode(500, new { error = "Failed to send prescription" });
        }
    }
}

public record SendToPharmacyRequest(string PrescriptionId, string PharmacyId);
