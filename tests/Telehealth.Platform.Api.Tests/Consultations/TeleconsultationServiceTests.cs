using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Infrastructure.Consultations;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Tests.Consultations;

public class TeleconsultationServiceTests
{
    private readonly PlatformDbContext _dbContext;
    private readonly TeleconsultationService _service;
    private readonly Guid _bookingId;

    public TeleconsultationServiceTests()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName: "TeleconsultationTestDb")
            .Options;

        _dbContext = new PlatformDbContext(options);
        _service = new TeleconsultationService(_dbContext);
        _bookingId = Guid.NewGuid();
    }

    [Fact]
    public async Task StartSessionAsync_ShouldCreateSession()
    {
        var session = await _service.StartSessionAsync(
            _bookingId,
            "jitsi",
            "room-123");

        Assert.NotNull(session);
        Assert.Equal(_bookingId, session.ConsultationBookingId);
        Assert.Equal("jitsi", session.VideoProvider);
        Assert.Equal("room-123", session.VideoRoomId);
        Assert.Equal(ConsultationSessionStatus.Created, session.Status);
    }

    [Fact]
    public async Task GetSessionAsync_ShouldReturnSession_WhenExists()
    {
        var createdSession = await _service.StartSessionAsync(_bookingId, "jitsi", "room-123");

        var session = await _service.GetSessionAsync(createdSession.Id);

        Assert.NotNull(session);
        Assert.Equal(createdSession.Id, session.Id);
    }

    [Fact]
    public async Task GetSessionAsync_ShouldThrow_WhenNotFound()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetSessionAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RecordParticipantEventAsync_ShouldCreateEvent()
    {
        var session = await _service.StartSessionAsync(_bookingId, "jitsi", "room-123");

        var participantEvent = await _service.RecordParticipantEventAsync(
            session.Id,
            ParticipantEventType.Joined,
            "patient",
            "patient-123",
            new Dictionary<string, object>());

        Assert.NotNull(participantEvent);
        Assert.Equal(session.Id, participantEvent.ConsultationSessionId);
        Assert.Equal(ParticipantEventType.Joined, participantEvent.EventType);
    }

    [Fact]
    public async Task GetActiveSessionsAsync_ShouldReturnOnlyActiveSessions()
    {
        var session1 = await _service.StartSessionAsync(_bookingId, "jitsi", "room-123");
        session1.StartSession();
        await _dbContext.SaveChangesAsync();

        var session2 = await _service.StartSessionAsync(Guid.NewGuid(), "jitsi", "room-456");
        session2.FailSession();
        await _dbContext.SaveChangesAsync();

        var activeSessions = await _service.GetActiveSessionsAsync();

        Assert.Single(activeSessions);
        Assert.Equal(session1.Id, activeSessions.First().Id);
    }

    [Fact]
    public async Task EndSessionAsync_ShouldUpdateSession()
    {
        var session = await _service.StartSessionAsync(_bookingId, "jitsi", "room-123");

        await _service.EndSessionAsync(session.Id, 300);

        var updatedSession = await _service.GetSessionAsync(session.Id);
        Assert.Equal(ConsultationSessionStatus.Completed, updatedSession.Status);
        Assert.Equal(300, updatedSession.BillableSeconds);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}