using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Video.Data;
using Telehealth.Platform.Video.Domain;

namespace Telehealth.Platform.Video.Services;

public class VideoService : IVideoService
{
    private readonly VideoDbContext _context;
    private readonly string _accountSid;
    private readonly string _apiKey;
    private readonly string _apiSecret;

    public VideoService(VideoDbContext context, IConfiguration configuration)
    {
        _context = context;
        _accountSid = configuration["Twilio:AccountSid"] ?? throw new ArgumentNullException("Twilio:AccountSid");
        _apiKey = configuration["Twilio:ApiKey"] ?? throw new ArgumentNullException("Twilio:ApiKey");
        _apiSecret = configuration["Twilio:ApiSecret"] ?? throw new ArgumentNullException("Twilio:ApiSecret");
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

        room.Activate($"TWILIO-{Guid.NewGuid()}");
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

        // TODO: Implement proper Twilio JWT token generation when credentials are configured
        // This requires Twilio.JWT package or manual JWT construction
        var token = $"token-{Guid.NewGuid()}-{participantIdentity}-{room.TwilioRoomSid}";
        return await Task.FromResult(token);
    }
}
