using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.Integrations.Healthcare.Services;

public interface IInsuranceIntegrationService
{
    Task<InsuranceClaim> SubmitClaimAsync(InsuranceClaim claim);
    Task<InsuranceClaim?> GetClaimAsync(Guid id);
    Task<List<InsuranceClaim>> GetClaimsByPatientAsync(Guid patientId);
    Task<List<InsuranceClaim>> GetClaimsByProviderAsync(string insuranceProviderId);
    Task<InsuranceClaim> ApproveClaimAsync(Guid id, string claimNumber);
    Task<InsuranceClaim> RejectClaimAsync(Guid id, string reason);
    Task<string> SyncWithInsuranceProviderAsync(string insuranceProviderId);
}
