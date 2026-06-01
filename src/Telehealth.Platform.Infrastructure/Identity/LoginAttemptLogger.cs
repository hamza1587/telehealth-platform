using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Identity;

/// <summary>
/// Logs all authentication attempts for security auditing.
/// This provides an append-only audit trail that cannot be modified.
/// </summary>
public sealed class LoginAttemptLogger : ILoginAttemptLogger
{
    private readonly PlatformDbContext _dbContext;

    public LoginAttemptLogger(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogSuccessfulAttemptAsync(
        string userId,
        string email,
        string? ipAddress,
        string? userAgent,
        string? deviceId,
        string? mfaMethod)
    {
        var attempt = new LoginAttempt(
            Guid.NewGuid(),
            userId,
            email,
            ipAddress,
            userAgent,
            deviceId,
            LoginAttemptResult.Success,
            mfaMethodUsed: mfaMethod)
        {
            MfaSuccessful = !string.IsNullOrEmpty(mfaMethod)
        };

        _dbContext.LoginAttempts.Add(attempt);
        await _dbContext.SaveChangesAsync();
    }

    public async Task LogFailedAttemptAsync(
        string? userId,
        string? email,
        string? ipAddress,
        string? userAgent,
        string? deviceId,
        LoginAttemptResult result,
        string reason)
    {
        var attempt = new LoginAttempt(
            Guid.NewGuid(),
            userId,
            email,
            ipAddress,
            userAgent,
            deviceId,
            result,
            reason);

        _dbContext.LoginAttempts.Add(attempt);
        await _dbContext.SaveChangesAsync();
    }

    public async Task LogMfaRequiredAsync(
        string userId,
        string email,
        string ipAddress,
        string? userAgent,
        string? deviceId)
    {
        var attempt = new LoginAttempt(
            Guid.NewGuid(),
            userId,
            email,
            ipAddress,
            userAgent,
            deviceId,
            LoginAttemptResult.MfaRequired);

        _dbContext.LoginAttempts.Add(attempt);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets recent failed login attempts for a user or IP address.
    /// Used for detecting brute-force attacks and suspicious activity.
    /// </summary>
    public async Task<IReadOnlyList<LoginAttempt>> GetRecentFailedAttemptsAsync(
        string? email = null,
        string? ipAddress = null,
        TimeSpan? window = null)
    {
        var lookback = DateTimeOffset.UtcNow.Subtract(window ?? TimeSpan.FromHours(1));

        var query = _dbContext.LoginAttempts
            .Where(a => a.Result != LoginAttemptResult.Success && a.AttemptedAt >= lookback);

        if (!string.IsNullOrEmpty(email))
        {
            query = query.Where(a => a.Email == email);
        }

        if (!string.IsNullOrEmpty(ipAddress))
        {
            query = query.Where(a => a.IpAddress == ipAddress);
        }

        return await query.OrderByDescending(a => a.AttemptedAt).ToListAsync();
    }

    /// <summary>
    /// Checks if an IP address should be blocked due to too many failed attempts.
    /// </summary>
    public async Task<bool> IsIpBlockedAsync(string ipAddress, int maxAttempts = 10, TimeSpan? window = null)
    {
        var lookback = DateTimeOffset.UtcNow.Subtract(window ?? TimeSpan.FromMinutes(15));

        var failedAttempts = await _dbContext.LoginAttempts
            .CountAsync(a => a.IpAddress == ipAddress
                && a.Result != LoginAttemptResult.Success
                && a.AttemptedAt >= lookback);

        return failedAttempts >= maxAttempts;
    }

    /// <summary>
    /// Checks if an account should be locked due to too many failed attempts.
    /// </summary>
    public async Task<bool> ShouldLockAccountAsync(string email, int maxAttempts = 5, TimeSpan? window = null)
    {
        var lookback = DateTimeOffset.UtcNow.Subtract(window ?? TimeSpan.FromMinutes(15));

        var failedAttempts = await _dbContext.LoginAttempts
            .CountAsync(a => a.Email == email
                && a.Result != LoginAttemptResult.Success
                && a.AttemptedAt >= lookback);

        return failedAttempts >= maxAttempts;
    }
}
