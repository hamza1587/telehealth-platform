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
    }

    public Guid ConsultationBookingId { get; }

    public string? MedplumEncounterId { get; private set; }

    public string VideoProvider { get; }

    public string VideoRoomId { get; }

    public ConsultationSessionStatus Status { get; private set; }

    public long BillableSeconds { get; private set; }

    public void LinkMedplumEncounter(string medplumEncounterId)
    {
        MedplumEncounterId = medplumEncounterId;
    }
}
