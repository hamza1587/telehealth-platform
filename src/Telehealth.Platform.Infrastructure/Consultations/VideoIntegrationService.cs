using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Telehealth.Platform.Application.Abstractions.Consultations;
using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Infrastructure.Consultations;

namespace Telehealth.Platform.Infrastructure.Consultations;

public class JitsiVideoIntegrationService : IVideoIntegrationService
{
    private readonly string _jitsiDomain;
    private readonly string _appId;
    private readonly string _appSecret;
    private readonly IVideoRoomService _videoRoomService;

    public JitsiVideoIntegrationService(
        IOptions<JitsiOptions> jitsiOptions,
        IVideoRoomService videoRoomService)
    {
        _jitsiDomain = jitsiOptions.Value.Domain;
        _appId = jitsiOptions.Value.AppId;
        _appSecret = jitsiOptions.Value.AppSecret;
        _videoRoomService = videoRoomService;
    }

    public async Task<string> GenerateRoomIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var existingRoom = await _videoRoomService.GetByBookingIdAsync(bookingId, cancellationToken);
        if (existingRoom != null && !string.IsNullOrEmpty(existingRoom.RoomId))
        {
            return existingRoom.RoomId;
        }

        var roomId = GenerateRoomId();
        return roomId;
    }

    public async Task<string> GenerateJwtTokenAsync(string roomId, string displayName, CancellationToken cancellationToken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("room", roomId),
                new System.Security.Claims.Claim("displayName", displayName),
                new System.Security.Claims.Claim("app_id", _appId)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<VideoRoomInfo> GetRoomInfoAsync(string roomId, CancellationToken cancellationToken = default)
    {
        return new VideoRoomInfo
        {
            RoomId = roomId,
            DisplayName = "Telehealth Consultation",
            JoinUrl = $"https://{_jitsiDomain}/{roomId}",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2),
            MaxParticipants = 2
        };
    }

    public async Task<bool> ValidateRoomAccessAsync(string roomId, string displayName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(displayName))
            return false;

        return await _videoRoomService.GetByIdAsync(Guid.Parse(roomId), cancellationToken) != null;
    }

    private static string GenerateRoomId()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var result = new StringBuilder();
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[12];
        rng.GetBytes(bytes);
        foreach (var b in bytes)
        {
            result.Append(chars[b % chars.Length]);
        }
        return $"telehealth-{result.ToString().ToLower()}";
    }
}

public class JitsiOptions
{
    public const string SectionName = "Jitsi";

    public string Domain { get; set; } = "meet.jit.si";
    public string AppId { get; set; } = "telehealth-platform";
    public string AppSecret { get; set; } = string.Empty;
}