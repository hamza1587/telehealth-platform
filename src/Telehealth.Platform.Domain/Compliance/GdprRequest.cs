using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Compliance;

public sealed class GdprRequest : Entity<Guid>
{
    public GdprRequest(Guid id, Guid requesterAccountId, GdprRequestType requestType, DateTimeOffset dueAt)
        : base(id)
    {
        RequesterAccountId = requesterAccountId;
        RequestType = requestType;
        DueAt = dueAt;
        Status = GdprRequestStatus.Submitted;
    }

    public Guid RequesterAccountId { get; }

    public GdprRequestType RequestType { get; }

    public GdprRequestStatus Status { get; private set; }

    public DateTimeOffset DueAt { get; }
}
