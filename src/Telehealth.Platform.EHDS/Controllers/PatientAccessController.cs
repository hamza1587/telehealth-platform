using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.EHDS.Services;

namespace Telehealth.Platform.EHDS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientAccessController : ControllerBase
{
    private readonly IPatientHealthRecordService _patientRecordService;

    public PatientAccessController(IPatientHealthRecordService patientRecordService)
    {
        _patientRecordService = patientRecordService;
    }

    [HttpGet("{patientId}/record")]
    public async Task<ActionResult<PatientHealthRecordSummary>> GetPatientRecord([FromRoute] Guid patientId)
    {
        var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
        if (record == null)
            return NotFound($"Patient record not found for ID: {patientId}");

        return Ok(new PatientHealthRecordSummary
        {
            PatientId = record.PatientId,
            Conditions = record.Conditions.Count,
            Medications = record.Medications.Count,
            Allergies = record.Allergies.Count,
            Immunizations = record.Immunizations.Count,
            Procedures = record.Procedures.Count,
            Observations = record.Observations.Count,
            LastUpdated = record.LastUpdated,
            DataOrigin = record.DataOrigin,
            IsCrossBorderAccessible = record.IsCrossBorderAccessible
        });
    }

    [HttpGet("{patientId}/conditions")]
    public async Task<ActionResult<List<ConditionSummary>>> GetConditions([FromRoute] Guid patientId)
    {
        var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
        if (record == null)
            return NotFound($"Patient record not found for ID: {patientId}");

        var conditions = record.Conditions.Select(c => new ConditionSummary
        {
            Id = c.Id,
            Code = c.Code,
            Display = c.Display,
            ClinicalStatus = c.ClinicalStatus,
            VerificationStatus = c.VerificationStatus,
            OnsetDateTime = c.OnsetDateTime
        }).ToList();

        return Ok(conditions);
    }

    [HttpGet("{patientId}/medications")]
    public async Task<ActionResult<List<MedicationSummary>>> GetMedications([FromRoute] Guid patientId)
    {
        var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
        if (record == null)
            return NotFound($"Patient record not found for ID: {patientId}");

        var medications = record.Medications.Select(m => new MedicationSummary
        {
            Id = m.Id,
            Code = m.Code,
            Display = m.Display,
            Dosage = m.Dosage,
            Frequency = m.Frequency,
            StartDate = m.StartDate
        }).ToList();

        return Ok(medications);
    }

    [HttpGet("{patientId}/allergies")]
    public async Task<ActionResult<List<AllergySummary>>> GetAllergies([FromRoute] Guid patientId)
    {
        var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
        if (record == null)
            return NotFound($"Patient record not found for ID: {patientId}");

        var allergies = record.Allergies.Select(a => new AllergySummary
        {
            Id = a.Id,
            Code = a.Code,
            Display = a.Display,
            ClinicalStatus = a.ClinicalStatus,
            Criticality = a.Criticality
        }).ToList();

        return Ok(allergies);
    }

    [HttpGet("{patientId}/immunizations")]
    public async Task<ActionResult<List<ImmunizationSummary>>> GetImmunizations([FromRoute] Guid patientId)
    {
        var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
        if (record == null)
            return NotFound($"Patient record not found for ID: {patientId}");

        var immunizations = record.Immunizations.Select(i => new ImmunizationSummary
        {
            Id = i.Id,
            VaccineCode = i.VaccineCode,
            Display = i.Display,
            AdministrationDate = i.AdministrationDate,
            LotNumber = i.LotNumber
        }).ToList();

        return Ok(immunizations);
    }

    [HttpGet("{patientId}/observations")]
    public async Task<ActionResult<List<ObservationSummary>>> GetObservations([FromRoute] Guid patientId)
    {
        var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
        if (record == null)
            return NotFound($"Patient record not found for ID: {patientId}");

        var observations = record.Observations.Select(o => new ObservationSummary
        {
            Id = o.Id,
            Code = o.Code,
            Display = o.Display,
            Value = o.Value,
            Unit = o.Unit,
            EffectiveDateTime = o.EffectiveDateTime
        }).ToList();

        return Ok(observations);
    }

    [HttpGet("{patientId}/fhir")]
    public async Task<ActionResult<string>> GetFhirData([FromRoute] Guid patientId)
    {
        var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
        if (record == null)
            return NotFound($"Patient record not found for ID: {patientId}");

        var fhirJson = await _patientRecordService.ExportToFhirAsync(record.Id);
        return Content(fhirJson, "application/fhir+json");
    }

    [HttpPost("{patientId}/cross-border-consent")]
    public async Task<ActionResult> UpdateCrossBorderConsent([FromRoute] Guid patientId, [FromBody] ConsentRequest request)
    {
        var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
        if (record == null)
            return NotFound($"Patient record not found for ID: {patientId}");

        var updated = await _patientRecordService.SetCrossBorderAccessAsync(record.Id, request.IsAllowed);
        return Ok(new { PatientId = patientId, IsAllowed = request.IsAllowed });
    }
}

public record PatientHealthRecordSummary(
    Guid PatientId,
    int Conditions,
    int Medications,
    int Allergies,
    int Immunizations,
    int Procedures,
    int Observations,
    DateTimeOffset LastUpdated,
    string DataOrigin,
    bool IsCrossBorderAccessible);

public record ConditionSummary(
    Guid Id,
    string Code,
    string Display,
    string ClinicalStatus,
    string VerificationStatus,
    DateTimeOffset OnsetDateTime);

public record MedicationSummary(
    Guid Id,
    string Code,
    string Display,
    string Dosage,
    string Frequency,
    DateTimeOffset StartDate);

public record AllergySummary(
    Guid Id,
    string Code,
    string Display,
    string ClinicalStatus,
    string Criticality);

public record ImmunizationSummary(
    Guid Id,
    string VaccineCode,
    string Display,
    DateTimeOffset AdministrationDate,
    string? LotNumber);

public record ObservationSummary(
    Guid Id,
    string Code,
    string Display,
    string Value,
    string Unit,
    DateTimeOffset EffectiveDateTime);

public record ConsentRequest(bool IsAllowed);