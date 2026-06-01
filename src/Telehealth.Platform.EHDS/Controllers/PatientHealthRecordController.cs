using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Telehealth.Platform.Domain.Entities;
using Telehealth.Platform.EHDS.Services;

namespace Telehealth.Platform.EHDS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientHealthRecordController : ControllerBase
{
    private readonly IPatientHealthRecordService _service;

    public PatientHealthRecordController(IPatientHealthRecordService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<PatientHealthRecord>> CreateRecord([FromBody] CreateRecordRequest request)
    {
        var record = await _service.CreateRecordAsync(request.PatientId, request.DataOrigin, request.IsCrossBorderAccessible);
        return CreatedAtAction(nameof(GetRecord), new { id = record.Id }, record);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PatientHealthRecord>> GetRecord(Guid id)
    {
        var record = await _service.GetRecordAsync(id);
        if (record == null)
            return NotFound();
        return Ok(record);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<PatientHealthRecord>> GetRecordByPatientId(Guid patientId)
    {
        var record = await _service.GetRecordByPatientIdAsync(patientId);
        if (record == null)
            return NotFound();
        return Ok(record);
    }

    [HttpGet]
    public async Task<ActionResult<List<PatientHealthRecord>>> GetAllRecords()
    {
        var records = await _service.GetAllRecordsAsync();
        return Ok(records);
    }

    [HttpPost("{id}/conditions")]
    public async Task<ActionResult<PatientHealthRecord>> AddCondition(Guid id, [FromBody] Telehealth.Platform.Domain.Entities.Condition condition)
    {
        var record = await _service.AddConditionAsync(id, condition);
        return Ok(record);
    }

    [HttpPost("{id}/medications")]
    public async Task<ActionResult<PatientHealthRecord>> AddMedication(Guid id, [FromBody] Medication medication)
    {
        var record = await _service.AddMedicationAsync(id, medication);
        return Ok(record);
    }

    [HttpPut("{id}/cross-border-access")]
    public async Task<ActionResult<PatientHealthRecord>> SetCrossBorderAccess(Guid id, [FromBody] SetCrossBorderAccessRequest request)
    {
        var record = await _service.SetCrossBorderAccessAsync(id, request.Accessible);
        return Ok(record);
    }

    [HttpGet("{id}/fhir")]
    public async Task<ActionResult<string>> ExportToFhir(Guid id)
    {
        try
        {
            var fhirJson = await _service.ExportToFhirAsync(id);
            return Content(fhirJson, "application/fhir+json");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to export record: {ex.Message}");
        }
    }

    [HttpPost("import/fhir")]
    public async Task<ActionResult<string>> ImportFromFhir([FromBody] string fhirJson)
    {
        try
        {
            var recordId = await _service.ImportFromFhirAsync(fhirJson);
            return Ok(new { recordId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"Invalid FHIR data: {ex.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to import FHIR data: {ex.Message}");
        }
    }
}

public record CreateRecordRequest(Guid PatientId, string DataOrigin, bool IsCrossBorderAccessible = false);
public record SetCrossBorderAccessRequest(bool Accessible);
