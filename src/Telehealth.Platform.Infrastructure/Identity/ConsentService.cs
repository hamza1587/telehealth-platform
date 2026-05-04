using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telehealth.Platform.Application.Abstractions.Consent;
using Telehealth.Platform.Domain.Consent;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Identity;

public class ConsentService : IConsentService
{
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<ConsentService> _logger;

    public ConsentService(
        PlatformDbContext dbContext,
        ILogger<ConsentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ConsentCaptureResult> CaptureConsentAsync(
        Guid userId,
        Guid templateId,
        string consentText,
        bool isAccepted,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _dbContext.ConsentTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive, cancellationToken);

            if (template == null)
            {
                return new ConsentCaptureResult
                {
                    Success = false,
                    ErrorMessage = "Consent template not found or inactive"
                };
            }

            var consent = new PatientConsent(
                id: Guid.NewGuid(),
                patientAccountId: userId,
                consentTemplateId: templateId,
                consentType: template.ConsentType,
                version: template.Version,
                contentHash: Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(consentText))),
                isAccepted: isAccepted,
                ipAddress: ipAddress,
                userAgent: userAgent,
                capturedAt: DateTimeOffset.UtcNow);

            _dbContext.PatientConsents.Add(consent);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Consent captured for user {UserId}, template {TemplateId}", userId, templateId);

            return new ConsentCaptureResult
            {
                Success = true,
                ConsentId = consent.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing consent for user {UserId}", userId);
            return new ConsentCaptureResult
            {
                Success = false,
                ErrorMessage = "Failed to capture consent"
            };
        }
    }

    public async Task<IEnumerable<PatientConsent>> GetUserConsentsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PatientConsents
            .Where(c => c.PatientAccountId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasConsentAsync(
        Guid userId,
        ConsentType consentType,
        CancellationToken cancellationToken = default)
    {
        var latestConsent = await _dbContext.PatientConsents
            .Where(c => c.PatientAccountId == userId && c.ConsentType == consentType.ToString() && c.IsCurrentlyActive())
            .OrderByDescending(c => c.CapturedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return latestConsent != null;
    }

    public async Task<bool> WithdrawConsentAsync(
        Guid userId,
        Guid consentId,
        CancellationToken cancellationToken = default)
    {
        var consent = await _dbContext.PatientConsents
            .FirstOrDefaultAsync(c => c.Id == consentId && c.PatientAccountId == userId, cancellationToken);

        if (consent == null)
            return false;

        consent.Withdraw(null, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RevokeConsentAsync(
        Guid userId,
        Guid consentId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var consent = await _dbContext.PatientConsents
            .FirstOrDefaultAsync(c => c.Id == consentId && c.PatientAccountId == userId, cancellationToken);

        if (consent == null)
            return false;

        consent.Withdraw(reason, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ConsentTemplate?> GetLatestTemplateAsync(
        ConsentType consentType,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConsentTemplates
            .Where(t => t.ConsentType == consentType.ToString() && t.IsActive)
            .OrderByDescending(t => t.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ConsentAnalytics>> GetConsentAnalyticsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var consentTypes = Enum.GetValues<ConsentType>();
        var results = new List<ConsentAnalytics>();

        foreach (var consentType in consentTypes)
        {
            var consents = await _dbContext.PatientConsents
                .Where(c => c.CapturedAt >= from && c.CapturedAt <= to && c.ConsentType == consentType.ToString())
                .ToListAsync(cancellationToken);

            var totalCaptured = consents.Count;
            var accepted = consents.Count(c => c.IsCurrentlyActive());
            var withdrawn = consents.Count(c => c.WithdrawnAt.HasValue);

            results.Add(new ConsentAnalytics
            {
                ConsentType = consentType,
                TotalCaptured = totalCaptured,
                Accepted = accepted,
                Withdrawn = withdrawn,
                AcceptanceRate = totalCaptured > 0 ? (double)accepted / totalCaptured * 100 : 0
            });
        }

        return results;
    }
}