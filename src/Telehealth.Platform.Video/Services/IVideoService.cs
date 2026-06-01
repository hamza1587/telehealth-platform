using Telehealth.Platform.Video.Domain;

namespace Telehealth.Platform.Video.Services;

public interface IVideoService
{
    Task<VideoRoom> CreateRoomAsync(string roomName, int maxParticipants, string countryCode = "US");
    Task<VideoRoom?> GetRoomAsync(Guid roomId);
    Task<List<VideoRoom>> GetAllRoomsAsync();
    Task<VideoRoom> ActivateRoomAsync(Guid roomId);
    Task<VideoRoom> EndRoomAsync(Guid roomId);
    Task<VideoRoom> EnableRecordingAsync(Guid roomId);
    Task<VideoRoom> DisableRecordingAsync(Guid roomId);
    Task<VideoRoom> EnableScreenSharingAsync(Guid roomId);
    Task<VideoRoom> DisableScreenSharingAsync(Guid roomId);
    Task<string> GenerateRoomTokenAsync(Guid roomId, string participantIdentity);
}
