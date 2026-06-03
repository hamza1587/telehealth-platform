using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telehealth.Platform.Video.Services;

public interface ILiveKitTokenService
{
    string GenerateToken(string identity, string roomName, bool canPublish = true, bool canSubscribe = true);
    string GetServerUrl();
}

public class LiveKitTokenService : ILiveKitTokenService
{
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _serverUrl;
    private readonly int _ttlSeconds;

    public LiveKitTokenService(IConfiguration configuration)
    {
        _apiKey = configuration["LiveKit:ApiKey"] ?? throw new InvalidOperationException("LiveKit:ApiKey is not configured");
        _apiSecret = configuration["LiveKit:ApiSecret"] ?? throw new InvalidOperationException("LiveKit:ApiSecret is not configured");
        _serverUrl = configuration["LiveKit:ServerUrl"] ?? "wss://localhost:7880";
        _ttlSeconds = int.TryParse(configuration["LiveKit:TokenTtlSeconds"], out var ttl) ? ttl : 21600;
    }

    public string GenerateToken(string identity, string roomName, bool canPublish = true, bool canSubscribe = true)
    {
        var headerJson = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" });
        var header = Base64UrlEncode(headerJson);

        var grant = new VideoGrant
        {
            Room = roomName,
            RoomJoin = true,
            CanPublish = canPublish,
            CanSubscribe = canSubscribe,
            CanPublishData = true,
        };

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payloadJson = JsonSerializer.Serialize(new LiveKitClaims
        {
            Issuer = _apiKey,
            Subject = identity,
            JwtId = Guid.NewGuid().ToString("N"),
            IssuedAt = now,
            NotBefore = now,
            Expires = now + _ttlSeconds,
            Video = grant,
        });
        var payload = Base64UrlEncode(payloadJson);

        var signature = SignHS256($"{header}.{payload}", _apiSecret);
        return $"{header}.{payload}.{signature}";
    }

    public string GetServerUrl() => _serverUrl;

    private static string SignHS256(string data, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var hash = HMACSHA256.HashData(keyBytes, dataBytes);
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(string input) =>
        Base64UrlEncode(Encoding.UTF8.GetBytes(input));

    private static string Base64UrlEncode(byte[] input) =>
        Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}

internal class LiveKitClaims
{
    [JsonPropertyName("iss")] public string Issuer { get; set; } = string.Empty;
    [JsonPropertyName("sub")] public string Subject { get; set; } = string.Empty;
    [JsonPropertyName("jti")] public string JwtId { get; set; } = string.Empty;
    [JsonPropertyName("iat")] public long IssuedAt { get; set; }
    [JsonPropertyName("nbf")] public long NotBefore { get; set; }
    [JsonPropertyName("exp")] public long Expires { get; set; }
    [JsonPropertyName("video")] public VideoGrant Video { get; set; } = new();
}

public class VideoGrant
{
    [JsonPropertyName("room")] public string Room { get; set; } = string.Empty;
    [JsonPropertyName("roomJoin")] public bool RoomJoin { get; set; }
    [JsonPropertyName("canPublish")] public bool CanPublish { get; set; }
    [JsonPropertyName("canSubscribe")] public bool CanSubscribe { get; set; }
    [JsonPropertyName("canPublishData")] public bool CanPublishData { get; set; }
}
