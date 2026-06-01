using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.EHDS.Services;

public class ResearchExportService : IResearchExportService
{
    private readonly EhdsDbContext _context;

    public ResearchExportService(EhdsDbContext context)
    {
        _context = context;
    }

    public async Task<ResearchExportRequest> CreateExportRequestAsync(
        Guid requesterId,
        string researchPurpose,
        List<string> dataDomains,
        string deidentificationMethod = "k_anonymity",
        int kAnonymityLevel = 5,
        double epsilon = 1.0)
    {
        var request = new ResearchExportRequest(
            requesterId,
            researchPurpose,
            dataDomains,
            deidentificationMethod,
            kAnonymityLevel,
            epsilon);

        _context.ResearchExportRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<ResearchExportRequest?> GetExportRequestAsync(Guid id)
    {
        return await _context.ResearchExportRequests.FindAsync(id);
    }

    public async Task<List<ResearchExportRequest>> GetPendingRequestsAsync()
    {
        return await _context.ResearchExportRequests
            .Where(r => r.Status == ExportStatus.Pending)
            .ToListAsync();
    }

    public async Task<ResearchExportRequest> ApproveRequestAsync(Guid id, string approverId)
    {
        var request = await _context.ResearchExportRequests.FindAsync(id);
        if (request == null)
            throw new ArgumentException("Export request not found", nameof(id));

        request.Approve(approverId);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<ResearchExportRequest> RejectRequestAsync(Guid id, string approverId, string reason)
    {
        var request = await _context.ResearchExportRequests.FindAsync(id);
        if (request == null)
            throw new ArgumentException("Export request not found", nameof(id));

        request.Reject(approverId, reason);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<ResearchExportRequest> StartProcessingAsync(Guid id)
    {
        var request = await _context.ResearchExportRequests.FindAsync(id);
        if (request == null)
            throw new ArgumentException("Export request not found", nameof(id));

        request.StartProcessing();
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<ResearchExportRequest> CompleteExportAsync(Guid id, string exportUrl)
    {
        var request = await _context.ResearchExportRequests.FindAsync(id);
        if (request == null)
            throw new ArgumentException("Export request not found", nameof(id));

        request.Complete(exportUrl);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<ResearchExportRequest> FailExportAsync(Guid id, string errorMessage)
    {
        var request = await _context.ResearchExportRequests.FindAsync(id);
        if (request == null)
            throw new ArgumentException("Export request not found", nameof(id));

        request.Fail(errorMessage);
        await _context.SaveChangesAsync();
        return request;
    }
}
