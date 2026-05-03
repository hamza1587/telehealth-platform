using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Identity;

public class UserSessionService : IUserSessionService
{
    private readonly PlatformDbContext _dbContext;
    private readonly IUserDeviceService _deviceService;

    public UserSessionService(PlatformDbContext dbContext, IUserDeviceService deviceService)
    {
        _dbContext = dbContext;
        _deviceService = deviceService;
    }

    public async Task<UserSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSession> CreateSessionAsync(
        Guid userId,
        string sessionId,
        string deviceId,
        string ipAddress,
        string userAgent,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var session = UserSession.Create(userId, sessionId, deviceId, ipAddress, userAgent, ttl);
        await _dbContext.UserSessions.AddAsync(session, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task UpdateSessionActivityAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
        
        if (session != null && session.IsActive)
        {
            session.UpdateActivity();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task EndSessionAsync(string sessionId, string reason, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
        
        if (session != null)
        {
            session.End(reason);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task EndAllUserSessionsAsync(Guid userId, string reason, CancellationToken cancellationToken = default)
    {
        var sessions = await _dbContext.UserSessions
            .Where(s => s.UserId == userId && s.Status == SessionStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.End(reason);
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task EndOtherUserSessionsAsync(Guid userId, string currentSessionId, string reason, CancellationToken cancellationToken = default)
    {
        var sessions = await _dbContext.UserSessions
            .Where(s => s.UserId == userId && s.SessionId != currentSessionId && s.Status == SessionStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.End(reason);
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CleanExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        var expiredSessions = await _dbContext.UserSessions
            .Where(s => s.Status == SessionStatus.Active && s.ExpiresAt <= DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var session in expiredSessions)
        {
            session.End("Expired");
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return expiredSessions.Count;
    }

    public async Task<bool> IsSessionValidAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
        
        return session?.IsActive ?? false;
    }

    public async Task<int> GetActiveSessionCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserSessions
            .CountAsync(s => s.UserId == userId && s.Status == SessionStatus.Active, cancellationToken);
    }
}