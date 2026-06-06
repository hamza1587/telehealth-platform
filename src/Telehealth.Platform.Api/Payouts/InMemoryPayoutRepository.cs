namespace Telehealth.Platform.Api.Payouts;

/// <summary>
/// Temporary in-memory implementation. Replace with EF Core + PostgreSQL in the Infrastructure layer.
/// </summary>
internal sealed class InMemoryPayoutRepository : IPayoutRepository
{
    private readonly Dictionary<string, string> _stripeAccounts = [];
    private readonly Dictionary<string, List<PayoutRecord>> _history = [];

    public Task<EarningsSummary> GetEarningsSummaryAsync(string doctorId, CancellationToken ct)
    {
        // In production, sum from billing_sessions / consultation_billing tables.
        var summary = new EarningsSummary(0, 0, 0, 0, 0.0, "EUR");
        return Task.FromResult(summary);
    }

    public Task<PayoutHistoryPage> GetPayoutHistoryAsync(string doctorId, int page, int pageSize, CancellationToken ct)
    {
        _history.TryGetValue(doctorId, out var records);
        var items = (records ?? [])
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(new PayoutHistoryPage(items, records?.Count ?? 0, page, pageSize));
    }

    public Task<string?> GetStripeAccountIdAsync(string doctorId, CancellationToken ct)
    {
        _stripeAccounts.TryGetValue(doctorId, out var id);
        return Task.FromResult<string?>(id);
    }

    public Task SaveStripeAccountIdAsync(string doctorId, string stripeAccountId, CancellationToken ct)
    {
        _stripeAccounts[doctorId] = stripeAccountId;
        return Task.CompletedTask;
    }

    public Task RecordPayoutAsync(string doctorId, long amountMinor, string currency, string transferId, CancellationToken ct)
    {
        if (!_history.TryGetValue(doctorId, out var list))
        {
            list = [];
            _history[doctorId] = list;
        }
        list.Add(new PayoutRecord(
            Guid.NewGuid().ToString(),
            amountMinor,
            currency,
            "completed",
            transferId,
            DateTimeOffset.UtcNow));
        return Task.CompletedTask;
    }
}
