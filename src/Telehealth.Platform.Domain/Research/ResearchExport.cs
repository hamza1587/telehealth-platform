using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Research;

/// <summary>
/// Research data export with anonymization.
/// </summary>
public sealed class ResearchExport : Entity<Guid>
{
    public ResearchExport(
        Guid id,
        Guid requestedBy,
        string researchProject,
        string? institution,
        string? ethicsApproval,
        string dataType,
        string anonymizationLevel,
        string format,
        DateTimeOffset? dateRangeFrom,
        DateTimeOffset? dateRangeTo,
        string? filters,
        DateTimeOffset createdAt)
        : base(id)
    {
        RequestedBy = requestedBy;
        ResearchProject = researchProject;
        Institution = institution;
        EthicsApproval = ethicsApproval;
        DataType = dataType;
        AnonymizationLevel = anonymizationLevel;
        Format = format;
        DateRangeFrom = dateRangeFrom;
        DateRangeTo = dateRangeTo;
        Filters = filters;
        Status = ResearchExportStatus.PendingApproval;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid RequestedBy { get; }
    public string ResearchProject { get; }
    public string? Institution { get; }
    public string? EthicsApproval { get; }
    public string DataType { get; }
    public string AnonymizationLevel { get; }
    public string Format { get; }
    public DateTimeOffset? DateRangeFrom { get; }
    public DateTimeOffset? DateRangeTo { get; }
    public string? Filters { get; }
    public ResearchExportStatus Status { get; private set; }
    public string? ApprovedBy { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? DownloadUrl { get; private set; }
    public long? RecordCount { get; private set; }
    public long? FileSize { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public void Approve(string approvedBy, string? conditions, DateTimeOffset approvedAt)
    {
        Status = ResearchExportStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = approvedAt;
        UpdatedAt = approvedAt;
    }

    public void Reject(string reason, DateTimeOffset rejectedAt)
    {
        Status = ResearchExportStatus.Rejected;
        RejectionReason = reason;
        UpdatedAt = rejectedAt;
    }

    public void Complete(string downloadUrl, long recordCount, long fileSize, DateTimeOffset completedAt)
    {
        Status = ResearchExportStatus.Ready;
        DownloadUrl = downloadUrl;
        RecordCount = recordCount;
        FileSize = fileSize;
        CompletedAt = completedAt;
        ExpiresAt = completedAt.AddDays(90);
        UpdatedAt = completedAt;
    }

    public void Fail(string reason, DateTimeOffset failedAt)
    {
        Status = ResearchExportStatus.Failed;
        RejectionReason = reason;
        UpdatedAt = failedAt;
    }

    public bool IsExpired(DateTimeOffset now)
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < now;
    }

    public bool IsDownloadable()
    {
        return Status == ResearchExportStatus.Ready && !IsExpired(DateTimeOffset.UtcNow);
    }
}

public enum ResearchExportStatus
{
    PendingApproval,
    UnderReview,
    Approved,
    Rejected,
    Processing,
    Ready,
    Downloaded,
    Expired,
    Failed
}

public enum AnonymizationLevel
{
    Pseudonymized,
    Anonymized,
    Aggregated,
    DifferentialPrivacy
}
