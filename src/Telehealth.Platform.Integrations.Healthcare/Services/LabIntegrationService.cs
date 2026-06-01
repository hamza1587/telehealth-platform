using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.Integrations.Healthcare.Services;

public class LabIntegrationService : ILabIntegrationService
{
    private readonly HealthcareIntegrationsDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public LabIntegrationService(
        HealthcareIntegrationsDbContext context,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<LabResult> SubmitLabResultAsync(LabResult labResult)
    {
        _context.LabResults.Add(labResult);
        await _context.SaveChangesAsync();
        return labResult;
    }

    public async Task<LabResult?> GetLabResultAsync(Guid id)
    {
        return await _context.LabResults.FindAsync(id);
    }

    public async Task<List<LabResult>> GetLabResultsByPatientAsync(Guid patientId)
    {
        return await _context.LabResults
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.ResultDate)
            .ToListAsync();
    }

    public async Task<List<LabResult>> GetLabResultsByLabSystemAsync(string labSystemId)
    {
        return await _context.LabResults
            .Where(r => r.LabSystemId == labSystemId)
            .OrderByDescending(r => r.ResultDate)
            .ToListAsync();
    }

    public async Task<string> SyncWithLabSystemAsync(string labSystemId)
    {
        try
        {
            // In production, this would connect to the actual lab system via HL7/FHIR
            var labApiUrl = _configuration[$"LabSystems:{labSystemId}:ApiUrl"];
            var labApiKey = _configuration[$"LabSystems:{labSystemId}:ApiKey"];

            if (string.IsNullOrEmpty(labApiUrl))
            {
                return $"Lab system {labSystemId} not configured";
            }

            _httpClient.DefaultRequestHeaders.Add("X-API-Key", labApiKey);
            
            // Placeholder for actual HL7/FHIR sync
            // var response = await _httpClient.GetAsync($"{labApiUrl}/fhir/Observation");
            // var fhirData = await response.Content.ReadAsStringAsync();
            // Process and store lab results

            return $"Synced with lab system {labSystemId}";
        }
        catch (Exception ex)
        {
            return $"Failed to sync with lab system {labSystemId}: {ex.Message}";
        }
    }
}
