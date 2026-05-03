namespace Telehealth.Platform.Application.Abstractions.Consultations;

public interface IVideoIntegrationService
{
    Task<string> GenerateRoomIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

    Task<string> GenerateJwtTokenAsync(string roomId, string displayName, CancellationToken cancellationToken = default);

    Task<VideoRoomInfo> GetRoomInfoAsync(string roomId, CancellationToken cancellationToken = default);

    Task<bool> ValidateRoomAccessAsync(string roomId, string displayName, CancellationToken cancellationToken = default);
}

public class VideoRoomInfo
{
    public string RoomId { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string JoinUrl { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public int MaxParticipants { get; set; }
}