using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Consultations;
using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Consultations;

public class VideoRoomService : IVideoRoomService
{
    private readonly PlatformDbContext _dbContext;

    public VideoRoomService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VideoRoom?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.VideoRooms
            .FirstOrDefaultAsync(r => r.BookingId == bookingId, cancellationToken);
    }

    public async Task<VideoRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.VideoRooms.FindAsync([id], cancellationToken);
    }

    public async Task<VideoRoom> CreateAsync(Guid bookingId, string roomId, string createdBy, CancellationToken cancellationToken = default)
    {
        var room = VideoRoom.Create(bookingId, roomId, createdBy);
        await _dbContext.VideoRooms.AddAsync(room, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task<VideoRoom?> MarkJoinedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await _dbContext.VideoRooms.FindAsync([id], cancellationToken);
        if (room == null) return null;

        room.MarkJoined();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task<VideoRoom?> EndAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await _dbContext.VideoRooms.FindAsync([id], cancellationToken);
        if (room == null) return null;

        room.End();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task<IEnumerable<VideoRoom>> GetActiveRoomsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.VideoRooms
            .Where(r => r.Status == VideoRoomStatus.InProgress)
            .ToListAsync(cancellationToken);
    }
}