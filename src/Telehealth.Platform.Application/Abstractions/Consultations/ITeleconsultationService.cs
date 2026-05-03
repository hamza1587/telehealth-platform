using Telehealth.Platform.Domain.Consultations;

namespace Telehealth.Platform.Application.Abstractions.Consultations;

public interface ITeleconsultationService
{
    Task<ConsultationSession> StartSessionAsync(
        Guid bookingId,
        string videoProvider,
        string videoRoomId,
        CancellationToken cancellationToken = default);

    Task<ConsultationSession> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<ConsultationSession> GetSessionByBookingAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<ParticipantEvent> RecordParticipantEventAsync(
        Guid sessionId,
        ParticipantEventType eventType,
        string? participantType,
        string? participantId,
        Dictionary<string, object>? metadata,
        CancellationToken cancellationToken = default);

    Task<ConsultationSession> UpdateSessionStatusAsync(
        Guid sessionId,
        ConsultationSessionStatus status,
        CancellationToken cancellationToken = default);

    Task EndSessionAsync(
        Guid sessionId,
        long billableSeconds,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ConsultationSession>> GetActiveSessionsAsync(
        CancellationToken cancellationToken = default);
}