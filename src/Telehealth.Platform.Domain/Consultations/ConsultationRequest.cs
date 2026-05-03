using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Consultations;

public enum ConsultationRequestStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled,
    Completed
}

public class ConsultationRequest : Entity<Guid>
{
    public Guid PatientAccountId { get; private set; }

    public Guid DoctorProfileId { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? ScheduledAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public ConsultationMode Mode { get; private set; }

    public ConsultationRequestStatus Status { get; private set; } = ConsultationRequestStatus.Pending;

    public string? Reason { get; private set; }

    public string? RejectionReason { get; private set; }

    public string? MedplumEncounterId { get; private set; }

    private ConsultationRequest(
        Guid id,
        Guid patientAccountId,
        Guid doctorProfileId,
        ConsultationMode mode) : base(id)
    {
        PatientAccountId = patientAccountId;
        DoctorProfileId = doctorProfileId;
        Mode = mode;
        RequestedAt = DateTimeOffset.UtcNow;
    }

    public static ConsultationRequest Create(
        Guid patientAccountId,
        Guid doctorProfileId,
        ConsultationMode mode,
        string? reason = null)
    {
        var request = new ConsultationRequest(
            Guid.NewGuid(),
            patientAccountId,
            doctorProfileId,
            mode)
        {
            Reason = reason
        };

        return request;
    }

    public void Schedule(DateTimeOffset scheduledAt)
    {
        ScheduledAt = scheduledAt;
        Status = ConsultationRequestStatus.Approved;
    }

    public void Reject(string rejectionReason)
    {
        Status = ConsultationRequestStatus.Rejected;
        RejectionReason = rejectionReason;
    }

    public void Cancel()
    {
        Status = ConsultationRequestStatus.Cancelled;
    }

    public void Complete(string medplumEncounterId)
    {
        Status = ConsultationRequestStatus.Completed;
        MedplumEncounterId = medplumEncounterId;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}