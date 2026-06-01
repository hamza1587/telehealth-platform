using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.EHDS.Services;

public interface IResearchExportService
{
    Task<ResearchExportRequest> CreateExportRequestAsync(
        Guid requesterId,
        string researchPurpose,
        List<string> dataDomains,
        string deidentificationMethod = "k_anonymity",
        int kAnonymityLevel = 5,
        double epsilon = 1.0);
    Task<ResearchExportRequest?> GetExportRequestAsync(Guid id);
    Task<List<ResearchExportRequest>> GetPendingRequestsAsync();
    Task<ResearchExportRequest> ApproveRequestAsync(Guid id, string approverId);
    Task<ResearchExportRequest> RejectRequestAsync(Guid id, string approverId, string reason);
    Task<ResearchExportRequest> StartProcessingAsync(Guid id);
    Task<ResearchExportRequest> CompleteExportAsync(Guid id, string exportUrl);
    Task<ResearchExportRequest> FailExportAsync(Guid id, string errorMessage);
}
