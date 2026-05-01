using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Compliance;

/// <summary>
/// GDPR data export request and processing.
/// </summary>
public sealed class GdprDataExport : Entity<Guid>
{
    public GdprDataExport(
        Guid id,
        Guid userId,
        string userType,
        string email,
        string format,
        DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        UserType = userType;
        Email = email;
        Format = format;
        Status = GdprExportStatus.Pending;
        CreatedAt = createdAt;
        EstimatedCompletion = createdAt.AddHours(24);
    }

    public Guid UserId { get; }
    public string UserType { get; }
    public string Email { get; }
    public string Format { get; }
    public GdprExportStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset EstimatedCompletion { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? DownloadUrl { get; private set; }
    public string? DownloadToken { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public long? FileSize { get; private set; }
    public string? FailureReason { get; private set; }
    public List<string> DataCategories { get; private set; } = new();

    public void MarkAsProcessing(DateTimeOffset startedAt)
    {
        Status = GdprExportStatus.Processing;
        StartedAt = startedAt;
    }

    public void Complete(string downloadUrl, string downloadToken, long fileSize, DateTimeOffset completedAt)
    {
        Status = GdprExportStatus.Ready;
        DownloadUrl = downloadUrl;
        DownloadToken = downloadToken;
        FileSize = fileSize;
        CompletedAt = completedAt;
        ExpiresAt = completedAt.AddDays(30);
    }

    public void Fail(string reason, DateTimeOffset failedAt)
    {
        Status = GdprExportStatus.Failed;
        FailureReason = reason;
    }

    public void AddDataCategory(string category)
    {
        DataCategories.Add(category);
    }

    public bool IsExpired(DateTimeOffset now)
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < now;
    }

    public bool IsDownloadable()
    {
        return Status == GdprExportStatus.Ready && !IsExpired(DateTimeOffset.UtcNow);
    }
}

public enum GdprExportStatus
{
    Pending,
    Processing,
    Ready,
    Downloaded,
    Expired,
    Failed,
    Cancelled
}
