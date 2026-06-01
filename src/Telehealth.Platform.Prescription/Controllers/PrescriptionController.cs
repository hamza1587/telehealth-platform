using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Prescription.Domain.Models;
using Telehealth.Platform.Prescription.Services;

namespace Telehealth.Platform.Prescription.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly ILogger<PrescriptionController> _logger;

    public PrescriptionController(IPrescriptionService prescriptionService, ILogger<PrescriptionController> logger)
    {
        _prescriptionService = prescriptionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Domain.Models.Prescription>> CreatePrescription([FromBody] CreatePrescriptionRequest request)
    {
        try
        {
            var prescription = await _prescriptionService.CreatePrescriptionAsync(
                request.ConsultationId,
                request.DoctorId,
                request.PatientId,
                request.CountryCode
            );
            return CreatedAtAction(nameof(GetPrescription), new { id = prescription.Id }, prescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prescription");
            return StatusCode(500, new { error = "Failed to create prescription" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Domain.Models.Prescription>> GetPrescription(Guid id)
    {
        try
        {
            var prescription = await _prescriptionService.GetPrescriptionAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }
            return Ok(prescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prescription");
            return StatusCode(500, new { error = "Failed to get prescription" });
        }
    }

    [HttpPost("{id}/items")]
    public async Task<ActionResult<Domain.Models.Prescription>> AddItem(Guid id, [FromBody] AddItemRequest request)
    {
        try
        {
            var item = new Domain.Models.PrescriptionItem(
                request.MedicationCode,
                request.MedicationName,
                request.Dosage,
                request.Frequency,
                request.DurationDays,
                request.Instructions,
                request.Quantity,
                id,
                request.Refills,
                request.IsControlledSubstance
            );

            var prescription = await _prescriptionService.AddItemAsync(id, item);
            return Ok(prescription);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to prescription");
            return StatusCode(500, new { error = "Failed to add item" });
        }
    }

    [HttpPost("{id}/sign")]
    public async Task<ActionResult<Domain.Models.Prescription>> SignPrescription(Guid id, [FromBody] SignRequest request)
    {
        try
        {
            var prescription = await _prescriptionService.SignPrescriptionAsync(id, request.DigitalSignature);
            return Ok(prescription);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing prescription");
            return StatusCode(500, new { error = "Failed to sign prescription" });
        }
    }

    [HttpPost("{id}/send")]
    public async Task<ActionResult<Domain.Models.Prescription>> SendPrescription(Guid id, [FromBody] SendRequest request)
    {
        try
        {
            var prescription = await _prescriptionService.SendPrescriptionAsync(id, request.PharmacyId);
            return Ok(prescription);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending prescription");
            return StatusCode(500, new { error = "Failed to send prescription" });
        }
    }

    [HttpPost("{id}/dispense")]
    public async Task<ActionResult<Domain.Models.Prescription>> DispensePrescription(Guid id)
    {
        try
        {
            var prescription = await _prescriptionService.DispensePrescriptionAsync(id);
            return Ok(prescription);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispensing prescription");
            return StatusCode(500, new { error = "Failed to dispense prescription" });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<Domain.Models.Prescription>> CancelPrescription(Guid id)
    {
        try
        {
            var prescription = await _prescriptionService.CancelPrescriptionAsync(id);
            return Ok(prescription);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling prescription");
            return StatusCode(500, new { error = "Failed to cancel prescription" });
        }
    }

    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<List<Domain.Models.Prescription>>> GetPatientPrescriptions(Guid patientId)
    {
        try
        {
            var prescriptions = await _prescriptionService.GetPatientPrescriptionsAsync(patientId);
            return Ok(prescriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient prescriptions");
            return StatusCode(500, new { error = "Failed to get prescriptions" });
        }
    }
}

public record CreatePrescriptionRequest(Guid ConsultationId, Guid DoctorId, Guid PatientId, string CountryCode);
public record AddItemRequest(
    string MedicationCode,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string Instructions,
    int Quantity,
    string? Refills,
    bool IsControlledSubstance
);
public record SignRequest(string DigitalSignature);
public record SendRequest(string? PharmacyId);
