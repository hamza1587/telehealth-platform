using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Domain.Entities;
using Telehealth.Platform.Prescription.Services;

namespace Telehealth.Platform.Prescription.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientMedicationController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly INationalPrescriptionGateway _prescriptionGateway;

    public PatientMedicationController(
        IPrescriptionService prescriptionService,
        INationalPrescriptionGateway prescriptionGateway)
    {
        _prescriptionService = prescriptionService;
        _prescriptionGateway = prescriptionGateway;
    }

    [HttpGet("{patientId}/history")]
    public async Task<ActionResult<List<PatientMedicationHistory>>> GetMedicationHistory(Guid patientId)
    {
        var prescriptions = await _prescriptionService.GetPatientPrescriptionsAsync(patientId);
        
        var history = prescriptions.Select(p => new PatientMedicationHistory
        {
            PrescriptionId = p.Id,
            Medications = p.Items.Select(i => new MedicationHistoryItem
            {
                Code = i.MedicationCode,
                Name = i.MedicationName,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                StartDate = p.CreatedAt,
                IsDiscontinued = p.Status == Domain.Models.PrescriptionStatus.Dispensed || p.Status == Domain.Models.PrescriptionStatus.Cancelled,
                Source = p.NationalPrescriptionId != null ? "National Prescription System" : "Platform Prescription"
            }).ToList(),
            LastUpdated = p.UpdatedAt
        }).ToList();

        return Ok(history);
    }

    [HttpPost("sync/{patientId}")]
    public async Task<ActionResult<MedicationSyncResult>> SyncMedicationHistory(Guid patientId, [FromBody] MedicationSyncRequest request)
    {
        try
        {
            // Trigger sync with national prescription gateway
            var result = await _prescriptionGateway.SyncWithPrescriptionSystemAsync(request.CountryCode);
            
            // Get updated history
            var prescriptions = await _prescriptionService.GetPatientPrescriptionsAsync(patientId);
            var syncedCount = prescriptions.Count(p => p.NationalPrescriptionId != null);
            
            return Ok(new MedicationSyncResult
            {
                PatientId = patientId,
                SyncedCount = syncedCount,
                Message = result,
                Timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to sync medication history: {ex.Message}");
        }
    }

    [HttpGet("{patientId}/interactions")]
    public async Task<ActionResult<List<DrugInteraction>>> CheckDrugInteractions(Guid patientId, [FromQuery] string[] medications)
    {
        // In production, this would check interactions via the DrugInteractionService
        var interactions = new List<DrugInteraction>();
        
        // Placeholder for actual interaction checking
        foreach (var med in medications)
        {
            var interaction = new DrugInteraction
            {
                Medication1 = med,
                Medication2 = medications.FirstOrDefault(m => m != med) ?? string.Empty,
                Severity = "Moderate",
                Description = $"Potential interaction between {med} and other medications",
                Recommendation = "Monitor patient closely"
            };
            interactions.Add(interaction);
        }
        
        return Ok(interactions);
    }
}

public record PatientMedicationHistory(
    Guid PrescriptionId,
    List<MedicationHistoryItem> Medications,
    DateTimeOffset LastUpdated
);

public record MedicationHistoryItem(
    string Code,
    string Name,
    string Dosage,
    string Frequency,
    DateTimeOffset StartDate,
    bool IsDiscontinued,
    string Source
);

public record MedicationSyncRequest(string CountryCode);

public record MedicationSyncResult(
    Guid PatientId,
    int SyncedCount,
    string Message,
    DateTimeOffset Timestamp
);

public record DrugInteraction(
    string Medication1,
    string Medication2,
    string Severity,
    string Description,
    string Recommendation
);