using Stripe;

namespace Telehealth.Platform.Api.Payouts;

/// <summary>
/// Manages Stripe Connect Express accounts for doctor payouts.
/// Stripe.net is used for account creation, onboarding links, and payouts.
/// </summary>
public sealed class StripeConnectService(IConfiguration config, ILogger<StripeConnectService> log)
{
    private readonly string _returnUrl = config["Stripe:ConnectReturnUrl"] ?? "https://app.telehealth.eu/settings/payouts";
    private readonly string _refreshUrl = config["Stripe:ConnectRefreshUrl"] ?? "https://app.telehealth.eu/settings/payouts/refresh";

    static StripeConnectService()
    {
        StripeConfiguration.ApiKey = null; // set per-call from config to avoid static mutation in tests
    }

    private void SetApiKey() =>
        StripeConfiguration.ApiKey = config["PaymentGateways:Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe secret key is not configured.");

    /// <summary>Creates a Stripe Express account for the doctor and returns the account ID.</summary>
    public async Task<string> CreateConnectedAccountAsync(string doctorId, string email, CancellationToken ct)
    {
        SetApiKey();
        var service = new AccountService();
        var account = await service.CreateAsync(new AccountCreateOptions
        {
            Type = "express",
            Email = email,
            Metadata = new Dictionary<string, string> { ["doctor_id"] = doctorId },
            Capabilities = new AccountCapabilitiesOptions
            {
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
            },
        }, cancellationToken: ct);

        log.LogInformation("Created Stripe Connect account {StripeAccountId} for doctor {DoctorId}",
            account.Id, doctorId);
        return account.Id;
    }

    /// <summary>Returns the Stripe onboarding URL for the doctor.</summary>
    public async Task<string> CreateOnboardingLinkAsync(string stripeAccountId, CancellationToken ct)
    {
        SetApiKey();
        var service = new AccountLinkService();
        var link = await service.CreateAsync(new AccountLinkCreateOptions
        {
            Account = stripeAccountId,
            ReturnUrl = _returnUrl,
            RefreshUrl = _refreshUrl,
            Type = "account_onboarding",
        }, cancellationToken: ct);
        return link.Url;
    }

    /// <summary>Returns the Stripe account status for a doctor.</summary>
    public async Task<ConnectAccountStatus> GetAccountStatusAsync(string stripeAccountId, CancellationToken ct)
    {
        SetApiKey();
        var service = new AccountService();
        var account = await service.GetAsync(stripeAccountId, cancellationToken: ct);

        return new ConnectAccountStatus(
            stripeAccountId,
            account.ChargesEnabled,
            account.PayoutsEnabled,
            account.DetailsSubmitted);
    }

    /// <summary>Creates a payout (transfer) to the doctor's Stripe Express account.</summary>
    public async Task<string> CreatePayoutAsync(
        string stripeAccountId,
        long amountMinor,
        string currency,
        string description,
        CancellationToken ct)
    {
        SetApiKey();
        var service = new TransferService();
        var transfer = await service.CreateAsync(new TransferCreateOptions
        {
            Amount = amountMinor,
            Currency = currency.ToLowerInvariant(),
            Destination = stripeAccountId,
            Description = description,
        }, cancellationToken: ct);

        log.LogInformation("Created Stripe transfer {TransferId} of {Amount} {Currency} to {AccountId}",
            transfer.Id, amountMinor, currency, stripeAccountId);
        return transfer.Id;
    }
}

public record ConnectAccountStatus(
    string AccountId,
    bool ChargesEnabled,
    bool PayoutsEnabled,
    bool DetailsSubmitted)
{
    public string Status =>
        !DetailsSubmitted ? "not_connected" :
        !ChargesEnabled || !PayoutsEnabled ? "pending" :
        "active";
}
