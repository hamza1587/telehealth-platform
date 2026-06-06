using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Telehealth.Platform.Api.Payouts;

[ApiController]
[Route("api/v1/payouts")]
[Authorize]
public class PayoutEndpoints(
    StripeConnectService stripeConnect,
    IPayoutRepository payoutRepo,
    ILogger<PayoutEndpoints> log) : ControllerBase
{
    private string DoctorId =>
        User.FindFirst("doctor_id")?.Value
        ?? throw new UnauthorizedAccessException("doctor_id claim missing.");

    // ─── Earnings ───────────────────────────────────────────────────────────

    /// <summary>Returns lifetime earnings summary for the authenticated doctor.</summary>
    [HttpGet("earnings")]
    public async Task<IActionResult> GetEarnings(CancellationToken ct)
    {
        var summary = await payoutRepo.GetEarningsSummaryAsync(DoctorId, ct);
        return Ok(summary);
    }

    /// <summary>Returns payout history (paginated).</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var history = await payoutRepo.GetPayoutHistoryAsync(DoctorId, page, pageSize, ct);
        return Ok(history);
    }

    // ─── Stripe Connect ──────────────────────────────────────────────────────

    /// <summary>Returns the Stripe Connect account status for the doctor.</summary>
    [HttpGet("connect/status")]
    public async Task<IActionResult> GetConnectStatus(CancellationToken ct)
    {
        var stripeAccountId = await payoutRepo.GetStripeAccountIdAsync(DoctorId, ct);
        if (stripeAccountId is null)
            return Ok(new { status = "not_connected" });

        try
        {
            var status = await stripeConnect.GetAccountStatusAsync(stripeAccountId, ct);
            return Ok(new { status = status.Status });
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to fetch Stripe account status for doctor {DoctorId}", DoctorId);
            return Ok(new { status = "not_connected" });
        }
    }

    /// <summary>
    /// Creates a Stripe Express account (if needed) and returns the onboarding URL.
    /// </summary>
    [HttpPost("connect/onboard")]
    public async Task<IActionResult> Onboard(CancellationToken ct)
    {
        var email = User.FindFirst("email")?.Value
            ?? throw new InvalidOperationException("email claim missing.");

        var stripeAccountId = await payoutRepo.GetStripeAccountIdAsync(DoctorId, ct);

        if (stripeAccountId is null)
        {
            stripeAccountId = await stripeConnect.CreateConnectedAccountAsync(DoctorId, email, ct);
            await payoutRepo.SaveStripeAccountIdAsync(DoctorId, stripeAccountId, ct);
        }

        var url = await stripeConnect.CreateOnboardingLinkAsync(stripeAccountId, ct);
        return Ok(new { url });
    }

    // ─── Withdrawals ─────────────────────────────────────────────────────────

    /// <summary>
    /// Requests a withdrawal of the specified amount to the doctor's Stripe account.
    /// </summary>
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request, CancellationToken ct)
    {
        if (request.AmountMinor <= 0)
            return BadRequest(new { error = "Amount must be positive." });

        var stripeAccountId = await payoutRepo.GetStripeAccountIdAsync(DoctorId, ct);
        if (stripeAccountId is null)
            return BadRequest(new { error = "Stripe account not connected." });

        // Verify the doctor has sufficient available balance
        var summary = await payoutRepo.GetEarningsSummaryAsync(DoctorId, ct);
        if (request.AmountMinor > summary.PendingMinor)
            return UnprocessableEntity(new { error = "Insufficient available balance." });

        var transferId = await stripeConnect.CreatePayoutAsync(
            stripeAccountId,
            request.AmountMinor,
            summary.Currency,
            $"Payout for doctor {DoctorId}",
            ct);

        await payoutRepo.RecordPayoutAsync(DoctorId, request.AmountMinor, summary.Currency, transferId, ct);

        return Ok(new { transferId, amountMinor = request.AmountMinor, currency = summary.Currency });
    }
}

public record WithdrawRequest(long AmountMinor);

// ─── Payout repository abstraction ──────────────────────────────────────────

public interface IPayoutRepository
{
    Task<EarningsSummary> GetEarningsSummaryAsync(string doctorId, CancellationToken ct);
    Task<PayoutHistoryPage> GetPayoutHistoryAsync(string doctorId, int page, int pageSize, CancellationToken ct);
    Task<string?> GetStripeAccountIdAsync(string doctorId, CancellationToken ct);
    Task SaveStripeAccountIdAsync(string doctorId, string stripeAccountId, CancellationToken ct);
    Task RecordPayoutAsync(string doctorId, long amountMinor, string currency, string transferId, CancellationToken ct);
}

public record EarningsSummary(
    long TotalEarningsMinor,
    long PendingMinor,
    long PaidOutMinor,
    int TotalConsultations,
    double AvgRating,
    string Currency);

public record PayoutHistoryPage(
    IReadOnlyList<PayoutRecord> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record PayoutRecord(
    string Id,
    long AmountMinor,
    string Currency,
    string Status,
    string? TransferId,
    DateTimeOffset CreatedAt);
