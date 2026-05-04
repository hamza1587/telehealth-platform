using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Payments;

public enum DisputeStatus
{
    Open = 1,
    UnderReview = 2,
    EvidenceSubmitted = 3,
    Resolved = 4,
    Lost = 5,
    Won = 6
}

public enum DisputeReason
{
    Fraudulent = 1,
    Duplicate = 2,
    IncorrectAmount = 3,
    NotRecognized = 4,
    Unauthorized = 5,
    Cancelled = 6,
    Other = 7
}

public sealed class Dispute : Entity<Guid>
{
    private Dispute(
        Guid id,
        Guid paymentId,
        DisputeReason reason,
        string reasonDescription,
        DisputeStatus status) : base(id)
    {
        PaymentId = paymentId;
        Reason = reason;
        ReasonDescription = reasonDescription;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid PaymentId { get; private set; }
    public DisputeReason Reason { get; private set; }
    public string ReasonDescription { get; private set; } = string.Empty;
    public DisputeStatus Status { get; private set; } = DisputeStatus.Open;
    public string Evidence { get; private set; } = string.Empty;
    public string StatusDetails { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static Dispute Create(Guid paymentId, DisputeReason reason, string reasonDescription, string evidence = "")
    {
        return new Dispute(
            Guid.NewGuid(),
            paymentId,
            reason,
            reasonDescription,
            string.IsNullOrEmpty(evidence) ? DisputeStatus.Open : DisputeStatus.EvidenceSubmitted)
        {
            Evidence = evidence
        };
    }

    public void AddEvidence(string evidence)
    {
        Evidence = evidence;
        Status = DisputeStatus.EvidenceSubmitted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateStatus(DisputeStatus status, string details = "")
    {
        Status = status;
        StatusDetails = details;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Resolve(DisputeStatus finalStatus)
    {
        Status = finalStatus;
        ResolvedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkLost()
    {
        Status = DisputeStatus.Lost;
        ResolvedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkWon()
    {
        Status = DisputeStatus.Won;
        ResolvedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}