using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Domain.Entities;
using Telehealth.Platform.EHDS.Services;

namespace Telehealth.Platform.EHDS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeIdentificationController : ControllerBase
{
    private readonly IDeIdentificationService _service;
    private readonly IPatientHealthRecordService _recordService;

    public DeIdentificationController(IDeIdentificationService service, IPatientHealthRecordService recordService)
    {
        _service = service;
        _recordService = recordService;
    }

    [HttpPost("records")]
    public async Task<ActionResult<List<Dictionary<string, object>>>> DeIdentifyRecords(
        [FromBody] DeIdentifyRequest request)
    {
        var records = await _recordService.GetAllRecordsAsync();
        var deIdentifiedData = _service.DeIdentifyRecords(records, request.KAnonymityLevel, request.Epsilon);
        return Ok(deIdentifiedData);
    }

    [HttpPost("record/{id}")]
    public async Task<ActionResult<Dictionary<string, object>>> DeIdentifyRecord(
        Guid id,
        [FromBody] DeIdentifyRequest request)
    {
        var record = await _recordService.GetRecordAsync(id);
        if (record == null)
            return NotFound();

        var deIdentifiedData = _service.DeIdentifyRecord(record, request.KAnonymityLevel, request.Epsilon);
        return Ok(deIdentifiedData);
    }

    [HttpPost("validate")]
    public ActionResult<bool> ValidatePrivacy([FromBody] ValidatePrivacyRequest request)
    {
        var isValid = _service.ValidatePrivacy(request.Data, request.KAnonymityLevel, request.Epsilon);
        return Ok(isValid);
    }

    [HttpGet("k-anonymity")]
    public ActionResult<int> CalculateCurrentKAnonymity([FromBody] CalculateKAnonymityRequest request)
    {
        var kLevel = _service.CalculateCurrentKAnonymity(request.Data);
        return Ok(kLevel);
    }
}

public record DeIdentifyRequest(int KAnonymityLevel = 5, double Epsilon = 1.0);
public record ValidatePrivacyRequest(List<Dictionary<string, object>> Data, int KAnonymityLevel, double Epsilon);
public record CalculateKAnonymityRequest(List<Dictionary<string, object>> Data);
