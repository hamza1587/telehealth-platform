using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Consultations;

namespace Telehealth.Platform.Domain.InstantConsultation;

/// <summary>
/// Patient entry in the instant consultation queue.
/// </summary>
public sealed class InstantQueueEntry : Entity<Guid>
{
    public InstantQueueEntry(
        Guid id,
        Guid patientAccountId,
        string specialtyCode,
        ConsultationMode preferredMode,
        decimal maxPricePerSecond,
        string currency,
        DateTimeOffset queuedAt)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        SpecialtyCode = specialtyCode;
        PreferredMode = preferredMode;
        MaxPricePerSecond = maxPricePerSecond;
        Currency = currency;
        Status = QueueStatus.Waiting;
        QueuedAt = queuedAt;
        Priority = 0;
    }

    public Guid PatientAccountId { get; }
    public string SpecialtyCode { get; }
    public ConsultationMode PreferredMode { get; }
    public decimal MaxPricePerSecond { get; }
    public string Currency { get; }
    public QueueStatus Status { get; private set; }
    public int Priority { get; private set; }
    public DateTimeOffset QueuedAt { get; }
    public DateTimeOffset? MatchedAt { get; private set; }
    public Guid? MatchedDoctorId { get; private set; }
    public Guid? ResultingBookingId { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public void Match(Guid doctorId, Guid bookingId, DateTimeOffset matchedAt, DateTimeOffset expiresAt)
    {
        Status = QueueStatus.Matched;
        MatchedDoctorId = doctorId;
        ResultingBookingId = bookingId;
        MatchedAt = matchedAt;
        ExpiresAt = expiresAt;
    }

    public void AcceptMatch(DateTimeOffset acceptedAt)
    {
        Status = QueueStatus.Accepted;
    }

    public void DeclineMatch(string reason, DateTimeOffset declinedAt)
    {
        Status = QueueStatus.Waiting;
        MatchedDoctorId = null;
        ResultingBookingId = null;
        MatchedAt = null;
        ExpiresAt = null;
    }

    public void Cancel(string reason, DateTimeOffset cancelledAt)
    {
        Status = QueueStatus.Cancelled;
        CancellationReason = reason;
    }

    public void Expire(DateTimeOffset expiredAt)
    {
        Status = QueueStatus.Expired;
    }

    public void BoostPriority(int priority)
    {
        Priority = priority;
    }

    public bool IsExpired(DateTimeOffset now)
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < now;
    }
}

public enum QueueStatus
{
    Waiting,
    Matched,
    Accepted,
    Cancelled,
    Expired,
    NoDoctorAvailable
}
