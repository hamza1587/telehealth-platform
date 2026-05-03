using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Application.Abstractions.Identity;

public interface IUserSessionService
{
    Task<UserSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSession>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSession> CreateSessionAsync(
        Guid userId,
        string sessionId,
        string deviceId,
        string ipAddress,
        string userAgent,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);
    Task UpdateSessionActivityAsync(string sessionId, CancellationToken cancellationToken = default);
    Task EndSessionAsync(string sessionId, string reason, CancellationToken cancellationToken = default);
    Task EndAllUserSessionsAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
    Task EndOtherUserSessionsAsync(Guid userId, string currentSessionId, string reason, CancellationToken cancellationToken = default);
    Task<int> CleanExpiredSessionsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsSessionValidAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<int> GetActiveSessionCountAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class SessionOptions
{
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromHours(8);
    public int MaxConcurrentSessions { get; set; } = 5;
    public TimeSpan InactivityTimeout { get; set; } = TimeSpan.FromHours(2);
}