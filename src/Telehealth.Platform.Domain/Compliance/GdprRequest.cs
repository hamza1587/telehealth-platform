using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Compliance;

public enum GdprRequestType
{
    DataAccess,
    DataRectification,
    DataErasure,
    DataPortability
}

public enum GdprRequestStatus
{
    Submitted,
    Processing,
    Completed,
    Rejected
}

public class GdprRequest : Entity<Guid>
{
    public Guid PatientAccountId { get; private set; }

    public GdprRequestType RequestType { get; private set; }

    public GdprRequestStatus Status { get; private set; } = GdprRequestStatus.Submitted;

    public DateTimeOffset SubmittedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public string? Details { get; private set; }

    public string? RejectionReason { get; private set; }

    public string? DownloadUrl { get; private set; }

    private GdprRequest(Guid id, Guid patientAccountId, GdprRequestType requestType) : base(id)
    {
        PatientAccountId = patientAccountId;
        RequestType = requestType;
        SubmittedAt = DateTimeOffset.UtcNow;
    }

    public static GdprRequest Create(Guid patientAccountId, GdprRequestType requestType, string? details = null)
    {
        return new GdprRequest(Guid.NewGuid(), patientAccountId, requestType)
        {
            Details = details
        };
    }

    public void StartProcessing()
    {
        Status = GdprRequestStatus.Processing;
    }

    public void Complete(string? downloadUrl = null)
    {
        Status = GdprRequestStatus.Completed;
        ProcessedAt = DateTimeOffset.UtcNow;
        DownloadUrl = downloadUrl;
    }

    public void Reject(string reason)
    {
        Status = GdprRequestStatus.Rejected;
        ProcessedAt = DateTimeOffset.UtcNow;
        RejectionReason = reason;
    }
}