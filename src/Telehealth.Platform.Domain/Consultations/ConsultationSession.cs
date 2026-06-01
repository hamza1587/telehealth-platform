using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Consultations;

public sealed class ConsultationSession : Entity<Guid>
{
    public ConsultationSession(Guid id, Guid consultationBookingId, string videoProvider, string videoRoomId)
        : base(id)
    {
        ConsultationBookingId = consultationBookingId;
        VideoProvider = videoProvider;
        VideoRoomId = videoRoomId;
        Status = ConsultationSessionStatus.Created;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid ConsultationBookingId { get; private set; }

    public string? MedplumEncounterId { get; private set; }

    public string VideoProvider { get; private set; }

    public string VideoRoomId { get; private set; }

    public ConsultationSessionStatus Status { get; private set; }

    public long BillableSeconds { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<ParticipantEvent> ParticipantEvents { get; private set; } = new List<ParticipantEvent>();

    public void LinkMedplumEncounter(string medplumEncounterId)
    {
        MedplumEncounterId = medplumEncounterId;
    }

    public void StartSession()
    {
        Status = ConsultationSessionStatus.InProgress;
    }

    public void EndSession(long billableSeconds)
    {
        Status = ConsultationSessionStatus.Completed;
        BillableSeconds = billableSeconds;
    }

    public void FailSession()
    {
        Status = ConsultationSessionStatus.Failed;
    }
}
