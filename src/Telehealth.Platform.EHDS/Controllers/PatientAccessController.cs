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

public record PatientHealthRecordSummary
{
    public Guid PatientId { get; init; }
    public int Conditions { get; init; }
    public int Medications { get; init; }
    public int Allergies { get; init; }
    public int Immunizations { get; init; }
    public int Procedures { get; init; }
    public int Observations { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public string DataOrigin { get; init; } = string.Empty;
    public bool IsCrossBorderAccessible { get; init; }
}

public record ConditionSummary
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Display { get; init; } = string.Empty;
    public string ClinicalStatus { get; init; } = string.Empty;
    public string VerificationStatus { get; init; } = string.Empty;
    public DateTimeOffset OnsetDateTime { get; init; }
}

public record MedicationSummary
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Display { get; init; } = string.Empty;
    public string Dosage { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public DateTimeOffset StartDate { get; init; }
}

public record AllergySummary
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Display { get; init; } = string.Empty;
    public string ClinicalStatus { get; init; } = string.Empty;
    public string Criticality { get; init; } = string.Empty;
}

public record ImmunizationSummary
{
    public Guid Id { get; init; }
    public string VaccineCode { get; init; } = string.Empty;
    public string Display { get; init; } = string.Empty;
    public DateTimeOffset AdministrationDate { get; init; }
    public string? LotNumber { get; init; }
}

public record ObservationSummary
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Display { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public DateTimeOffset EffectiveDateTime { get; init; }
}

public record ConsentRequest(bool IsAllowed);