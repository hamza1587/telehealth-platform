using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.EHDS.Services;

namespace Telehealth.Platform.EHDS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrossBorderController : ControllerBase
{
    private readonly IPatientHealthRecordService _patientRecordService;
    private readonly IDeIdentificationService _deIdentificationService;

    public CrossBorderController(
        IPatientHealthRecordService patientRecordService,
        IDeIdentificationService deIdentificationService)
    {
        _patientRecordService = patientRecordService;
        _deIdentificationService = deIdentificationService;
    }

    [HttpPost("exchange")]
    public async Task<ActionResult<CrossBorderExchangeResponse>> ExchangeData([FromBody] CrossBorderExchangeRequest request)
    {
        try
        {
            // Verify patient record exists and is cross-border accessible
            var record = await _patientRecordService.GetRecordByPatientIdAsync(request.PatientId);
            if (record == null)
                return NotFound($"Patient record not found for ID: {request.PatientId}");

            if (!record.IsCrossBorderAccessible)
                return Forbid("Patient record not marked for cross-border access");

            // Export to FHIR format for interoperability
            var fhirData = await _patientRecordService.ExportToFhirAsync(record.Id);
            
            // If for research purposes, apply de-identification
            if (request.Purpose == "research")
            {
                var records = new List<Telehealth.Platform.Domain.Entities.PatientHealthRecord> { record };
                var deidentified = _deIdentificationService.DeIdentifyRecords(
                    records,
                    request.KAnonymityLevel,
                    request.Epsilon);
                
                return Ok(new CrossBorderExchangeResponse
                {
                    PatientId = request.PatientId,
                    RecipientCountry = request.RecipientCountry,
                    FhirData = fhirData,
                    DeidentifiedData = deidentified,
                    ExchangeId = Guid.NewGuid()
                });
            }

            return Ok(new CrossBorderExchangeResponse
            {
                PatientId = request.PatientId,
                RecipientCountry = request.RecipientCountry,
                FhirData = fhirData,
                ExchangeId = Guid.NewGuid()
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to exchange data: {ex.Message}");
        }
    }

    [HttpGet("supported-countries")]
    public ActionResult<List<SupportedCountry>> GetSupportedCountries()
    {
        var countries = Enum.GetValues<CountryCode>()
            .Select(code => new SupportedCountry
            {
                Code = code.ToString(),
                Name = code switch
                {
                    CountryCode.DE => "Germany",
                    CountryCode.FR => "France",
                    CountryCode.IT => "Italy",
                    CountryCode.ES => "Spain",
                    CountryCode.NL => "Netherlands",
                    CountryCode.PL => "Poland",
                    CountryCode.SE => "Sweden",
                    CountryCode.PT => "Portugal",
                    CountryCode.AT => "Austria",
                    CountryCode.BE => "Belgium",
                    _ => code.ToString()
                },
                IsEhdsCompliant = true
            })
            .ToList();

        return Ok(countries);
    }

    [HttpPost("validate-access")]
    public async Task<ActionResult<bool>> ValidateCrossBorderAccess([FromBody] ValidateAccessRequest request)
    {
        var record = await _patientRecordService.GetRecordByPatientIdAsync(request.PatientId);
        if (record == null)
            return NotFound($"Patient record not found for ID: {request.PatientId}");

        var hasAccess = record.IsCrossBorderAccessible;
        var canExchange = hasAccess && 
            Enum.TryParse<CountryCode>(request.RecipientCountry, out _) &&
            !string.IsNullOrEmpty(request.RecipientCountry);

        return Ok(new { PatientId = request.PatientId, HasAccess = hasAccess, CanExchange = canExchange });
    }
}

public record CrossBorderExchangeRequest(
    Guid PatientId,
    string RecipientCountry,
    string Purpose,
    int KAnonymityLevel = 5,
    double Epsilon = 1.0);

public record CrossBorderExchangeResponse(
    Guid PatientId,
    string RecipientCountry,
    string FhirData,
    List<Dictionary<string, object>>? DeidentifiedData,
    Guid ExchangeId);

public record SupportedCountry(string Code, string Name, bool IsEhdsCompliant);

public record ValidateAccessRequest(Guid PatientId, string RecipientCountry);

public enum CountryCode
{
    DE, FR, IT, ES, NL, PL, SE, PT, AT, BE
}