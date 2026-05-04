using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Research;

public enum ResearchExportStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public class ResearchExport : Entity<Guid>
{
    public Guid RequesterId { get; private set; }

    public string RequesterType { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public ResearchExportStatus Status { get; private set; } = ResearchExportStatus.Pending;

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public long? RecordCount { get; private set; }

    public string? DownloadUrl { get; private set; }

    public string? FailureReason { get; private set; }

    public Dictionary<string, object>? Parameters { get; private set; } = new();

    private ResearchExport(
        Guid id,
        Guid requesterId,
        string title,
        string description) : base(id)
    {
        RequesterId = requesterId;
        Title = title;
        Description = description;
        RequestedAt = DateTimeOffset.UtcNow;
    }

    public static ResearchExport Create(
        Guid requesterId,
        string title,
        string description,
        string requesterType,
        Dictionary<string, object>? parameters = null)
    {
        var export = new ResearchExport(Guid.NewGuid(), requesterId, title, description)
        {
            RequesterType = requesterType
        };

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                if (export.Parameters != null)
                {
                    export.Parameters[param.Key] = param.Value;
                }
            }
        }

        return export;
    }

    public void StartProcessing()
    {
        Status = ResearchExportStatus.Processing;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Complete(long? recordCount = null, string? downloadUrl = null)
    {
        Status = ResearchExportStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        RecordCount = recordCount;
        DownloadUrl = downloadUrl;
    }

    public void Fail(string reason)
    {
        Status = ResearchExportStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        FailureReason = reason;
    }
}