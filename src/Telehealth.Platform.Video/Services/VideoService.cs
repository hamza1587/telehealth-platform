using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Video.Data;
using Telehealth.Platform.Video.Domain;

namespace Telehealth.Platform.Video.Services;

public class VideoService : IVideoService
{
    private readonly VideoDbContext _context;
    private readonly ILiveKitTokenService _tokenService;

    public VideoService(VideoDbContext context, ILiveKitTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<VideoRoom> CreateRoomAsync(string roomName, int maxParticipants, string countryCode = "US")
    {
        var room = new VideoRoom(roomName, maxParticipants, countryCode);
        _context.VideoRooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<VideoRoom?> GetRoomAsync(Guid roomId)
    {
        return await _context.VideoRooms.FindAsync(roomId);
    }

    public async Task<List<VideoRoom>> GetAllRoomsAsync()
    {
        return await _context.VideoRooms.ToListAsync();
    }

    public async Task<VideoRoom> ActivateRoomAsync(Guid roomId)
    {
        var room = await _context.VideoRooms.FindAsync(roomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found", nameof(roomId));
        }

        room.Activate($"LIVEKIT-{Guid.NewGuid():N}");
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<VideoRoom> EndRoomAsync(Guid roomId)
    {
        var room = await _context.VideoRooms.FindAsync(roomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found", nameof(roomId));
        }

        room.End();
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<VideoRoom> EnableRecordingAsync(Guid roomId)
    {
        var room = await _context.VideoRooms.FindAsync(roomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found", nameof(roomId));
        }

        room.EnableRecording();
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<VideoRoom> DisableRecordingAsync(Guid roomId)
    {
        var room = await _context.VideoRooms.FindAsync(roomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found", nameof(roomId));
        }

        room.DisableRecording();
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<VideoRoom> EnableScreenSharingAsync(Guid roomId)
    {
        var room = await _context.VideoRooms.FindAsync(roomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found", nameof(roomId));
        }

        room.EnableScreenSharing();
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<VideoRoom> DisableScreenSharingAsync(Guid roomId)
    {
        var room = await _context.VideoRooms.FindAsync(roomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found", nameof(roomId));
        }

        room.DisableScreenSharing();
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<string> GenerateRoomTokenAsync(Guid roomId, string participantIdentity)
    {
        var room = await _context.VideoRooms.FindAsync(roomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found", nameof(roomId));
        }

        return await Task.FromResult(_tokenService.GenerateToken(participantIdentity, room.RoomName));
    }
}
