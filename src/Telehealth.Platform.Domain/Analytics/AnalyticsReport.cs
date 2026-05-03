using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Analytics;

public class AnalyticsReport : Entity<Guid>
{
    public string ReportType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Format { get; private set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public long? RecordCount { get; private set; }
    public string Status { get; private set; } = "Draft";
    public string CreatedBy { get; private set; } = string.Empty;

    public void SetRecordCount(long? recordCount)
    {
        RecordCount = recordCount;
    }
    public Dictionary<string, object> Parameters { get; private set; } = new();
    public string ResultLocation { get; private set; } = string.Empty;

    private AnalyticsReport(
        Guid id,
        string reportType,
        string title,
        string description,
        string format,
        string createdBy) : base(id)
    {
        ReportType = reportType;
        Title = title;
        Description = description;
        Format = format;
        CreatedBy = createdBy;
        GeneratedAt = DateTimeOffset.UtcNow;
        Status = "Draft";
    }

    public static AnalyticsReport Create(
        string reportType,
        string title,
        string description,
        string format,
        string createdBy,
        Dictionary<string, object>? parameters = null)
    {
        var report = new AnalyticsReport(
            Guid.NewGuid(),
            reportType,
            title,
            description,
            format,
            createdBy);

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                report.Parameters[param.Key] = param.Value;
            }
        }

        return report;
    }

    public void StartProcessing()
    {
        Status = "Processing";
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Complete(long? recordCount = null)
    {
        Status = "Completed";
        CompletedAt = DateTimeOffset.UtcNow;
        RecordCount = recordCount;
    }

    public void Fail(string error)
    {
        Status = "Failed";
        CompletedAt = DateTimeOffset.UtcNow;
        Parameters["error"] = error;
    }

    public void SetResultLocation(string location)
    {
        ResultLocation = location;
    }
}

public static class ReportTypes
{
    public const string ConsultationSummary = "consultation_summary";
    public const string RevenueReport = "revenue_report";
    public const string PatientEngagement = "patient_engagement";
    public const string DoctorPerformance = "doctor_performance";
    public const string PlatformUsage = "platform_usage";
}