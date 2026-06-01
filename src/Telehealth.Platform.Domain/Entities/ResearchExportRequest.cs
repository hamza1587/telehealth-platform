namespace Telehealth.Platform.Domain.Entities;

public class ResearchExportRequest
{
    public Guid Id { get; private set; }
    public Guid RequesterId { get; private set; }
    public string ResearchPurpose { get; private set; }
    public List<string> DataDomains { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public ExportStatus Status { get; private set; }
    public string DeidentificationMethod { get; private set; }
    public int KAnonymityLevel { get; private set; }
    public double Epsilon { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public string? ExportUrl { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public List<string> AuditTrail { get; private set; }
    public string? RejectionReason { get; private set; } = null;

    public ResearchExportRequest(
        Guid requesterId,
        string researchPurpose,
        List<string> dataDomains,
        string deidentificationMethod = "k_anonymity",
        int kAnonymityLevel = 5,
        double epsilon = 1.0)
    {
        Id = Guid.NewGuid();
        RequesterId = requesterId;
        ResearchPurpose = researchPurpose;
        DataDomains = dataDomains;
        DeidentificationMethod = deidentificationMethod;
        KAnonymityLevel = kAnonymityLevel;
        Epsilon = epsilon;
        RequestedAt = DateTimeOffset.UtcNow;
        Status = ExportStatus.Pending;
        AuditTrail = new List<string>
        {
            $"[{DateTimeOffset.UtcNow}] Export request created by {requesterId}"
        };
    }

    public void Approve(string approverId)
    {
        Status = ExportStatus.Approved;
        ApprovedBy = approverId;
        ApprovedAt = DateTimeOffset.UtcNow;
        AuditTrail.Add($"[{DateTimeOffset.UtcNow}] Export approved by {approverId}");
    }

    public void Reject(string approverId, string reason)
    {
        Status = ExportStatus.Rejected;
        ApprovedBy = approverId;
        ApprovedAt = DateTimeOffset.UtcNow;
        AuditTrail.Add($"[{DateTimeOffset.UtcNow}] Export rejected by {approverId}: {reason}");
    }

    public void StartProcessing()
    {
        Status = ExportStatus.Processing;
        AuditTrail.Add($"[{DateTimeOffset.UtcNow}] Export processing started");
    }

    public void Complete(string exportUrl)
    {
        Status = ExportStatus.Completed;
        ExportUrl = exportUrl;
        CompletedAt = DateTimeOffset.UtcNow;
        AuditTrail.Add($"[{DateTimeOffset.UtcNow}] Export completed. URL: {exportUrl}");
    }

    public void Fail(string errorMessage)
    {
        Status = ExportStatus.Failed;
        AuditTrail.Add($"[{DateTimeOffset.UtcNow}] Export failed: {errorMessage}");
    }
}

public enum ExportStatus
{
    Pending,
    Approved,
    Rejected,
    Processing,
    Completed,
    Failed
}
