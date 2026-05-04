using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telehealth.Platform.Application.Abstractions.Payments;

namespace Telehealth.Platform.Application.Payments;

public class FraudDetectionServiceOptions
{
    public int MaxAttemptsPerHour { get; set; } = 10;
    public decimal MaxAmountThreshold { get; set; } = 1000m;
    public int RateLimitWindowMinutes { get; set; } = 60;
}

public class FraudDetectionService : IFraudDetectionService
{
    private readonly IOptions<FraudDetectionServiceOptions> _options;
    private readonly ILogger<FraudDetectionService> _logger;
    private readonly Dictionary<string, List<DateTime>> _attemptHistory = new();
    private readonly Dictionary<string, List<FraudCheckResult>> _riskHistory = new();

    public FraudDetectionService(
        IOptions<FraudDetectionServiceOptions> options,
        ILogger<FraudDetectionService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<FraudCheckResult> CheckFraudAsync(
        Guid patientAccountId,
        long amountMinor,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        var triggeredRules = new List<string>();
        var riskScore = 0m;

        var amount = amountMinor / 100m;
        if (amount > _options.Value.MaxAmountThreshold)
        {
            triggeredRules.Add("high_amount");
            riskScore += 0.4m;
        }

        if (await IsRateLimitedAsync(ipAddress, "payment", cancellationToken))
        {
            triggeredRules.Add("rate_limit");
            riskScore += 0.3m;
        }

        if (_attemptHistory.ContainsKey(ipAddress))
        {
            var recentAttempts = _attemptHistory[ipAddress]
                .Where(d => d > DateTime.UtcNow.AddHours(-1))
                .ToList();

            if (recentAttempts.Count >= _options.Value.MaxAttemptsPerHour)
            {
                triggeredRules.Add("too_many_attempts");
                riskScore += 0.3m;
            }
        }

        var isFraudulent = riskScore > 0.5m;

        var result = new FraudCheckResult
        {
            IsFraudulent = isFraudulent,
            RiskScore = riskScore,
            Reason = isFraudulent ? "Potential fraud detected" : "Transaction appears safe",
            TriggeredRules = triggeredRules
        };

        if (!_riskHistory.ContainsKey(ipAddress))
        {
            _riskHistory[ipAddress] = new List<FraudCheckResult>();
        }
        _riskHistory[ipAddress].Add(result);

        return await Task.FromResult(result);
    }

    public async Task ReportSuspiciousActivityAsync(
        Guid patientAccountId,
        string reason,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Suspicious activity reported for patient {PatientId} from IP {IpAddress}: {Reason}",
            patientAccountId,
            ipAddress,
            reason);

        await Task.CompletedTask;
    }

    public async Task<IEnumerable<FraudRule>> GetActiveFraudRulesAsync(CancellationToken cancellationToken = default)
    {
        var rules = new List<FraudRule>
        {
            new FraudRule
            {
                RuleId = "high_amount",
                Name = "High Amount Threshold",
                Description = "Transactions above threshold amount",
                Threshold = _options.Value.MaxAmountThreshold,
                IsActive = true,
                Priority = 1
            },
            new FraudRule
            {
                RuleId = "rate_limit",
                Name = "Rate Limiting",
                Description = "Too many requests from same IP",
                Threshold = _options.Value.MaxAttemptsPerHour,
                IsActive = true,
                Priority = 2
            }
        };

        return await Task.FromResult(rules);
    }

    public async Task<bool> IsRateLimitedAsync(
        string ipAddress,
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        if (!_attemptHistory.ContainsKey(ipAddress))
        {
            _attemptHistory[ipAddress] = new List<DateTime>();
        }

        var cutoff = DateTime.UtcNow.AddMinutes(-_options.Value.RateLimitWindowMinutes);
        var recentAttempts = _attemptHistory[ipAddress]
            .Where(d => d > cutoff)
            .ToList();

        if (recentAttempts.Count >= _options.Value.MaxAttemptsPerHour)
        {
            return true;
        }

        recentAttempts.Add(DateTime.UtcNow);
        return await Task.FromResult(false);
    }
}