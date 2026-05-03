using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Consultations;

public class VideoRoom : Entity<Guid>
{
    public Guid BookingId { get; private set; }
    public string RoomId { get; private set; } = string.Empty;
    public string SessionId { get; private set; } = string.Empty;
    public VideoRoomStatus Status { get; private set; } = VideoRoomStatus.Waiting;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? JoinedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private VideoRoom(
        Guid id,
        Guid bookingId,
        string roomId,
        string createdBy) : base(id)
    {
        BookingId = bookingId;
        RoomId = roomId;
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static VideoRoom Create(Guid bookingId, string roomId, string createdBy)
    {
        return new VideoRoom(Guid.NewGuid(), bookingId, roomId, createdBy);
    }

    public void MarkJoined()
    {
        JoinedAt = DateTimeOffset.UtcNow;
        Status = VideoRoomStatus.InProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void End()
    {
        EndedAt = DateTimeOffset.UtcNow;
        Status = VideoRoomStatus.Ended;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsActive => Status == VideoRoomStatus.InProgress && !EndedAt.HasValue;
}

public enum VideoRoomStatus
{
    Waiting = 1,
    InProgress = 2,
    Ended = 3,
    Failed = 4
}