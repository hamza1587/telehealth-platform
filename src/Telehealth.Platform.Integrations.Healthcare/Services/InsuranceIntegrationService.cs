using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.Integrations.Healthcare.Services;

public class InsuranceIntegrationService : IInsuranceIntegrationService
{
    private readonly HealthcareIntegrationsDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public InsuranceIntegrationService(
        HealthcareIntegrationsDbContext context,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<InsuranceClaim> SubmitClaimAsync(InsuranceClaim claim)
    {
        _context.InsuranceClaims.Add(claim);
        await _context.SaveChangesAsync();
        return claim;
    }

    public async Task<InsuranceClaim?> GetClaimAsync(Guid id)
    {
        return await _context.InsuranceClaims.FindAsync(id);
    }

    public async Task<List<InsuranceClaim>> GetClaimsByPatientAsync(Guid patientId)
    {
        return await _context.InsuranceClaims
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.SubmittedDate)
            .ToListAsync();
    }

    public async Task<List<InsuranceClaim>> GetClaimsByProviderAsync(string insuranceProviderId)
    {
        return await _context.InsuranceClaims
            .Where(c => c.InsuranceProviderId == insuranceProviderId)
            .OrderByDescending(c => c.SubmittedDate)
            .ToListAsync();
    }

    public async Task<InsuranceClaim> ApproveClaimAsync(Guid id, string claimNumber)
    {
        var claim = await _context.InsuranceClaims.FindAsync(id);
        if (claim == null)
            throw new ArgumentException("Claim not found", nameof(id));

        claim.Approve(claimNumber);
        await _context.SaveChangesAsync();
        return claim;
    }

    public async Task<InsuranceClaim> RejectClaimAsync(Guid id, string reason)
    {
        var claim = await _context.InsuranceClaims.FindAsync(id);
        if (claim == null)
            throw new ArgumentException("Claim not found", nameof(id));

        claim.Reject(reason);
        await _context.SaveChangesAsync();
        return claim;
    }

    public async Task<string> SyncWithInsuranceProviderAsync(string insuranceProviderId)
    {
        try
        {
            // In production, this would connect to the actual insurance provider API
            var insuranceApiUrl = _configuration[$"InsuranceProviders:{insuranceProviderId}:ApiUrl"];
            var insuranceApiKey = _configuration[$"InsuranceProviders:{insuranceProviderId}:ApiKey"];

            if (string.IsNullOrEmpty(insuranceApiUrl))
            {
                return $"Insurance provider {insuranceProviderId} not configured";
            }

            _httpClient.DefaultRequestHeaders.Add("X-API-Key", insuranceApiKey);
            
            // Placeholder for actual insurance API sync
            // var response = await _httpClient.PostAsync($"{insuranceApiUrl}/claims/sync", null);
            // Process claim status updates

            return $"Synced with insurance provider {insuranceProviderId}";
        }
        catch (Exception ex)
        {
            return $"Failed to sync with insurance provider {insuranceProviderId}: {ex.Message}";
        }
    }
}
