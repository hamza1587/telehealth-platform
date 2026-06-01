using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Prescription.Services;

namespace Telehealth.Platform.Prescription.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrugInteractionController : ControllerBase
{
    private readonly IDrugInteractionService _drugInteractionService;
    private readonly ILogger<DrugInteractionController> _logger;

    public DrugInteractionController(IDrugInteractionService drugInteractionService, ILogger<DrugInteractionController> logger)
    {
        _drugInteractionService = drugInteractionService;
        _logger = logger;
    }

    [HttpPost("check")]
    public async Task<ActionResult<List<DrugInteraction>>> CheckInteractions([FromBody] CheckInteractionsRequest request)
    {
        try
        {
            var interactions = await _drugInteractionService.CheckInteractionsAsync(
                request.CurrentMedications,
                request.NewMedications
            );
            return Ok(interactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking drug interactions");
            return StatusCode(500, new { error = "Failed to check interactions" });
        }
    }
}

public record CheckInteractionsRequest(List<string> CurrentMedications, List<string> NewMedications);
