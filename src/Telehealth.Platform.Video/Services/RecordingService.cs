using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Video.Data;
using Telehealth.Platform.Video.Domain;

namespace Telehealth.Platform.Video.Services;

public class RecordingService : IRecordingService
{
    private readonly VideoDbContext _context;
    private readonly IVideoService _videoService;
    private readonly string _accountSid;
    private readonly string _apiKey;
    private readonly string _apiSecret;

    public RecordingService(VideoDbContext context, IVideoService videoService, IConfiguration configuration)
    {
        _context = context;
        _videoService = videoService;
        _accountSid = configuration["Twilio:AccountSid"] ?? string.Empty;
        _apiKey = configuration["Twilio:ApiKey"] ?? string.Empty;
        _apiSecret = configuration["Twilio:ApiSecret"] ?? string.Empty;
    }

    public async Task<RecordingSession> StartRecordingAsync(Guid roomId, bool requiresConsent)
    {
        var room = await _videoService.GetRoomAsync(roomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found", nameof(roomId));
        }

        var recordingSession = new RecordingSession(roomId, requiresConsent);

        if (recordingSession.ConsentObtained && room.TwilioRoomSid != null)
        {
            // Start Twilio recording
            try
            {
                if (!string.IsNullOrEmpty(_accountSid) && !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiSecret))
                {
                    // TODO: Implement actual Twilio recording API call
                    // This would use Twilio.Rest.Video.V1.Room.RoomRecordingResource.CreateAsync
                    var recordingSid = $"RT-{Guid.NewGuid()}";
                    recordingSession.StartRecording(recordingSid);
                }
                else
                {
                    // Fallback to placeholder
                    recordingSession.StartRecording($"recording-{Guid.NewGuid()}");
                }
            }
            catch
            {
                // Fallback to placeholder if Twilio API fails
                recordingSession.StartRecording($"recording-{Guid.NewGuid()}");
            }
        }

        _context.RecordingSessions.Add(recordingSession);
        await _context.SaveChangesAsync();
        return recordingSession;
    }

    public async Task<RecordingSession?> GetRecordingSessionAsync(Guid recordingId)
    {
        return await _context.RecordingSessions.FindAsync(recordingId);
    }

    public async Task<RecordingSession> StopRecordingAsync(Guid recordingId)
    {
        var session = await _context.RecordingSessions.FindAsync(recordingId);
        if (session == null)
        {
            throw new ArgumentException("Recording session not found", nameof(recordingId));
        }

        session.EndRecording();

        // Stop Twilio recording and get URL
        if (session.TwilioRecordingSid != null)
        {
            try
            {
                if (!string.IsNullOrEmpty(_accountSid) && !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiSecret))
                {
                    // TODO: Implement actual Twilio recording stop API call
                    // This would use Twilio.Rest.Video.V1.Room.RoomRecordingResource.UpdateAsync
                    var recordingUrl = $"https://media.twiliocdn.com/{session.TwilioRecordingSid}";
                    session.CompleteRecording(recordingUrl);
                }
                else
                {
                    // Fallback to placeholder
                    session.CompleteRecording($"https://storage.example.com/recordings/{session.TwilioRecordingSid}");
                }
            }
            catch
            {
                // Fallback to placeholder if Twilio API fails
                session.CompleteRecording($"https://storage.example.com/recordings/{session.TwilioRecordingSid}");
            }
        }

        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<RecordingSession> GrantConsentAsync(Guid recordingId)
    {
        var session = await _context.RecordingSessions.FindAsync(recordingId);
        if (session == null)
        {
            throw new ArgumentException("Recording session not found", nameof(recordingId));
        }

        session.GrantConsent();

        // Start recording if consent was just granted
        if (session.Status == RecordingStatus.Pending)
        {
            var room = await _videoService.GetRoomAsync(session.RoomId);
            if (room != null && room.TwilioRoomSid != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(_accountSid) && !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiSecret))
                    {
                        // TODO: Implement actual Twilio recording API call
                        var recordingSid = $"RT-{Guid.NewGuid()}";
                        session.StartRecording(recordingSid);
                    }
                    else
                    {
                        session.StartRecording($"recording-{Guid.NewGuid()}");
                    }
                }
                catch
                {
                    session.StartRecording($"recording-{Guid.NewGuid()}");
                }
            }
        }

        await _context.SaveChangesAsync();
        return session;
    }
}
