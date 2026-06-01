using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Domain.Entities;
using Telehealth.Platform.EHDS.Services;

namespace Telehealth.Platform.EHDS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResearcherPortalController : ControllerBase
{
    private readonly IResearchExportService _researchService;
    private readonly IDeIdentificationService _deIdentificationService;
    private readonly IPatientHealthRecordService _recordService;

    public ResearcherPortalController(
        IResearchExportService researchService,
        IDeIdentificationService deIdentificationService,
        IPatientHealthRecordService recordService)
    {
        _researchService = researchService;
        _deIdentificationService = deIdentificationService;
        _recordService = recordService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ResearcherAuthResponse>> Login([FromBody] ResearcherLoginRequest request)
    {
        // In production, verify researcher credentials against research institute database
        // This is a placeholder implementation
        var isValid = !string.IsNullOrEmpty(request.InstitutionEmail) && 
                      request.InstitutionEmail.Contains(".edu") || 
                      request.InstitutionEmail.Contains("research");
        
        if (!isValid)
            return Unauthorized("Invalid researcher credentials");
        
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        
        return Ok(new ResearcherAuthResponse
        {
            Token = token,
            ResearcherId = Guid.NewGuid(),
            Institution = request.InstitutionEmail.Split('@')[1],
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        });
    }

    [HttpPost("request-export")]
    public async Task<ActionResult<ResearchExportRequest>> RequestDataExport([FromBody] DataExportRequest request)
    {
        try
        {
            var exportRequest = await _researchService.CreateExportRequestAsync(
                request.ResearcherId,
                request.ResearchPurpose,
                request.DataDomains,
                request.DeidentificationMethod,
                request.KAnonymityLevel,
                request.Epsilon);

            return CreatedAtAction(nameof(GetExportRequest), new { id = exportRequest.Id }, exportRequest);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create export request: {ex.Message}");
        }
    }

    [HttpGet("request/{id}")]
    public async Task<ActionResult<ResearchExportRequest>> GetExportRequest(Guid id)
    {
        var request = await _researchService.GetExportRequestAsync(id);
        if (request == null)
            return NotFound();
        return Ok(request);
    }

    [HttpGet("requests/pending")]
    public async Task<ActionResult<List<ResearchExportRequest>>> GetPendingRequests()
    {
        var requests = await _researchService.GetPendingRequestsAsync();
        return Ok(requests);
    }

    [HttpPost("approve-request/{id}")]
    public async Task<ActionResult<ResearchExportRequest>> ApproveRequest(Guid id, [FromBody] ApproveResearchRequest request)
    {
        try
        {
            var exportRequest = await _researchService.ApproveRequestAsync(id, request.ApproverId);
            return Ok(exportRequest);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("download/{exportId}")]
    public async Task<ActionResult> DownloadExport(Guid exportId)
    {
        var request = await _researchService.GetExportRequestAsync(exportId);
        if (request == null)
            return NotFound();

        if (request.Status != ExportStatus.Completed)
            return BadRequest("Export not completed");

        if (string.IsNullOrEmpty(request.ExportUrl))
            return BadRequest("No export available");

        // In production, this would download from secure storage
        var fileName = $"research-export-{exportId}.zip";
        
        return Ok(new DownloadResponse
        {
            DownloadUrl = request.ExportUrl,
            FileName = fileName,
            Size = "1.2GB", // Placeholder
            RecordCount = 10000 // Placeholder
        });
    }

    [HttpPost("privacy-check")]
    public async Task<ActionResult<PrivacyCheckResponse>> CheckPrivacy([FromBody] PrivacyCheckRequest request)
    {
        var records = await _recordService.GetAllRecordsAsync();
        var sample = records.Take(100).ToList(); // Sample for performance
        
        var kLevel = _deIdentificationService.CalculateCurrentKAnonymity(
            sample.Select(r => new Dictionary<string, object>()).ToList()
        );
        
        var isValid = _deIdentificationService.ValidatePrivacy(
            sample.Select(r => new Dictionary<string, object>()).ToList(),
            request.KAnonymityLevel,
            request.Epsilon
        );

        return Ok(new PrivacyCheckResponse
        {
            SatisfiesPrivacy = isValid,
            CurrentKAnonymity = kLevel,
            RequestedKAnonymity = request.KAnonymityLevel,
            Epsilon = request.Epsilon,
            Recommendations = isValid 
                ? new List<string>() 
                : new List<string> { "Increase k-anonymity level or reduce quasi-identifiers" }
        });
    }
}

public record ResearcherLoginRequest(string InstitutionEmail, string? ApiKey);
public record ResearcherAuthResponse(
    string Token, 
    Guid ResearcherId, 
    string Institution, 
    DateTimeOffset ExpiresAt
);
public record DataExportRequest(
    Guid ResearcherId,
    string ResearchPurpose,
    List<string> DataDomains,
    string DeidentificationMethod = "k_anonymity",
    int KAnonymityLevel = 5,
    double Epsilon = 1.0
);
public record ApproveResearchRequest(string ApproverId);
public record DownloadResponse(
    string DownloadUrl,
    string FileName,
    string Size,
    int RecordCount
);
public record PrivacyCheckRequest(int KAnonymityLevel = 5, double Epsilon = 1.0);
public record PrivacyCheckResponse(
    bool SatisfiesPrivacy,
    int CurrentKAnonymity,
    int RequestedKAnonymity,
    double Epsilon,
    List<string> Recommendations
);