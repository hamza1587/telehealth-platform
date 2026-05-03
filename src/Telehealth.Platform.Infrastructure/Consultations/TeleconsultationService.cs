using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Consultations;
using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Consultations;

public class TeleconsultationService : ITeleconsultationService
{
    private readonly PlatformDbContext _dbContext;

    public TeleconsultationService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ConsultationSession> StartSessionAsync(
        Guid bookingId,
        string videoProvider,
        string videoRoomId,
        CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid();
        var session = new ConsultationSession(sessionId, bookingId, videoProvider, videoRoomId);

        await _dbContext.ConsultationSessions.AddAsync(session, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return session;
    }

    public async Task<ConsultationSession> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConsultationSessions
            .Include(s => s.ParticipantEvents)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session with ID {sessionId} not found");
    }

    public async Task<ConsultationSession> GetSessionByBookingAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConsultationSessions
            .Include(s => s.ParticipantEvents)
            .FirstOrDefaultAsync(s => s.ConsultationBookingId == bookingId, cancellationToken)
            ?? throw new InvalidOperationException($"No session found for booking ID {bookingId}");
    }

    public async Task<ParticipantEvent> RecordParticipantEventAsync(
        Guid sessionId,
        ParticipantEventType eventType,
        string? participantType,
        string? participantId,
        Dictionary<string, object>? metadata,
        CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.ConsultationSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session with ID {sessionId} not found");

        var participantEvent = new ParticipantEvent(
            Guid.NewGuid(),
            sessionId,
            eventType,
            DateTimeOffset.UtcNow);

        participantEvent.SetParticipant(participantType, participantId);

        if (metadata != null)
        {
            participantEvent.SetMetadata(metadata);
        }

        await _dbContext.ParticipantEvents.AddAsync(participantEvent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return participantEvent;
    }

    public async Task<ConsultationSession> UpdateSessionStatusAsync(
        Guid sessionId,
        ConsultationSessionStatus status,
        CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.ConsultationSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session with ID {sessionId} not found");

        throw new NotImplementedException("Status update logic needs to be implemented based on session state machine");
    }

    public async Task EndSessionAsync(
        Guid sessionId,
        long billableSeconds,
        CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.ConsultationSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session with ID {sessionId} not found");

        session.EndSession(billableSeconds);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ConsultationSession>> GetActiveSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConsultationSessions
            .Where(s => s.Status == ConsultationSessionStatus.InProgress)
            .ToListAsync(cancellationToken);
    }
}