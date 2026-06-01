namespace Telehealth.Platform.Analytics.Domain.Models;

public enum ReportStatus
{
    Pending,
    Generating,
    Completed,
    Failed
}

public class AnalyticsReport
{
    public Guid Id { get; private set; }
    public string ReportName { get; private set; } = string.Empty;
    public string QueryDefinition { get; private set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; private set; }
    public string DownloadUrl { get; private set; } = string.Empty;
    public ReportStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public AnalyticsReport()
    {
    }

    public AnalyticsReport(string reportName, string queryDefinition, Guid createdBy)
    {
        Id = Guid.NewGuid();
        ReportName = reportName;
        QueryDefinition = queryDefinition;
        CreatedBy = createdBy;
        Status = ReportStatus.Pending;
        GeneratedAt = DateTimeOffset.UtcNow;
        DownloadUrl = string.Empty;
    }

    public void StartGenerating()
    {
        Status = ReportStatus.Generating;
    }

    public void Complete(string downloadUrl)
    {
        Status = ReportStatus.Completed;
        DownloadUrl = downloadUrl;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = ReportStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
