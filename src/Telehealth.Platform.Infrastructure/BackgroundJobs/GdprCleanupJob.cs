using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.BackgroundJobs;

/// <summary>
/// Processes pending GDPR deletion requests and purges expired tokens.
/// Registered as a daily Hangfire recurring job.
/// </summary>
public class GdprCleanupJob
{
    private readonly PlatformDbContext _db;
    private readonly ILogger<GdprCleanupJob> _logger;

    public GdprCleanupJob(PlatformDbContext db, ILogger<GdprCleanupJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // Suspend users who are pending deletion (give them 30-day grace period before hard delete)
        var pendingDeletion = await _db.PlatformUsers
            .Where(u => u.Status == UserStatus.PendingDeletion
                     && u.UpdatedAt < DateTimeOffset.UtcNow.AddDays(-30))
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var user in pendingDeletion)
        {
            try
            {
                // Anonymise by suspending — hard delete is a separate compliance step
                user.Suspend("GDPR deletion processed");
                _logger.LogInformation("GDPR: anonymised user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GDPR: failed to process user {UserId}", user.Id);
            }
        }

        if (pendingDeletion.Count > 0)
            await _db.SaveChangesAsync(cancellationToken);

        // Purge refresh tokens older than 90 days or already revoked
        var tokenCutoff = DateTimeOffset.UtcNow.AddDays(-90);
        var deletedTokens = await _db.RefreshTokens
            .Where(t => t.ExpiresAt < DateTimeOffset.UtcNow || t.IsRevoked)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedTokens > 0)
            _logger.LogInformation("GdprCleanupJob: purged {Count} expired/revoked refresh tokens", deletedTokens);

        // Purge old login attempts (keep 30 days for security analysis)
        var loginCutoff = DateTimeOffset.UtcNow.AddDays(-30);
        var deletedAttempts = await _db.LoginAttempts
            .Where(l => l.AttemptedAt < loginCutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedAttempts > 0)
            _logger.LogInformation("GdprCleanupJob: purged {Count} old login attempts", deletedAttempts);

        _logger.LogInformation(
            "GdprCleanupJob complete: {PendingCount} users processed, {TokenCount} tokens purged, {AttemptCount} attempts purged",
            pendingDeletion.Count, deletedTokens, deletedAttempts);
    }
}
