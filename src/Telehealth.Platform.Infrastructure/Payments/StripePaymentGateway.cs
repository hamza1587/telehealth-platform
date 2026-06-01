using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Telehealth.Platform.Application.Abstractions.Payments;
using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Payments;

namespace Telehealth.Platform.Infrastructure.Payments;

public class StripePaymentGatewayOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public bool IsTestMode { get; set; } = true;
}

public class StripePaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<StripePaymentGatewayOptions> _options;
    private readonly ILogger<StripePaymentGateway> _logger;

    public string ProviderName => "Stripe";

    public StripePaymentGateway(
        HttpClient httpClient,
        IOptions<StripePaymentGatewayOptions> options,
        ILogger<StripePaymentGateway> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<PaymentTransaction> AuthorizePaymentAsync(
        Payment payment,
        PaymentMethod paymentMethod,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            amount = payment.AmountMinor,
            currency = payment.Currency.ToString().ToLower(),
            source = paymentMethod.Token,
            description = payment.Description,
            capture = false
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.Value.SecretKey);

        var response = await _httpClient.PostAsync("https://api.stripe.com/v1/charges", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Stripe authorization failed: {Error}", error);
            throw new InvalidOperationException($"Stripe authorization failed: {error}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var stripeResponse = JsonSerializer.Deserialize<StripeChargeResponse>(responseContent)
            ?? throw new InvalidOperationException("Stripe returned an empty authorization response.");

        return PaymentTransaction.Create(
            payment.Id,
            payment.PatientAccountId,
            ProviderName,
            stripeResponse.Id,
            payment.AmountMinor,
            new Money(stripeResponse.Amount, stripeResponse.Currency));
    }

    public async Task<PaymentTransaction> CapturePaymentAsync(
        Payment payment,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            amount_to_capture = payment.AmountMinor
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.Value.SecretKey);

        var response = await _httpClient.PostAsync(
            $"https://api.stripe.com/v1/charges/{payment.ExternalPaymentId}/capture",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Stripe capture failed: {Error}", error);
            throw new InvalidOperationException($"Stripe capture failed: {error}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var stripeResponse = JsonSerializer.Deserialize<StripeChargeResponse>(responseContent)
            ?? throw new InvalidOperationException("Stripe returned an empty capture response.");

        return PaymentTransaction.Create(
            payment.Id,
            payment.PatientAccountId,
            ProviderName,
            stripeResponse.Id,
            payment.AmountMinor,
            new Money(stripeResponse.Amount, stripeResponse.Currency));
    }

    public async Task<PaymentTransaction> RefundPaymentAsync(
        Refund refund,
        Payment payment,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            charge = payment.ExternalPaymentId,
            amount = refund.AmountMinor
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.Value.SecretKey);

        var response = await _httpClient.PostAsync("https://api.stripe.com/v1/refunds", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Stripe refund failed: {Error}", error);
            throw new InvalidOperationException($"Stripe refund failed: {error}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var stripeResponse = JsonSerializer.Deserialize<StripeRefundResponse>(responseContent)
            ?? throw new InvalidOperationException("Stripe returned an empty refund response.");

        return PaymentTransaction.Create(
            refund.PaymentId,
            Guid.Empty,
            ProviderName,
            stripeResponse.Id,
            refund.AmountMinor,
            new Money(refund.AmountMinor, refund.Currency));
    }

    public async Task<PaymentTransaction> VoidPaymentAsync(
        Payment payment,
        CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.Value.SecretKey);

        var response = await _httpClient.PostAsync(
            $"https://api.stripe.com/v1/charges/{payment.ExternalPaymentId}/refund",
            null,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Stripe void failed: {Error}", error);
            throw new InvalidOperationException($"Stripe void failed: {error}");
        }

        return PaymentTransaction.Create(
            payment.Id,
            payment.PatientAccountId,
            ProviderName,
            payment.ExternalPaymentId,
            payment.AmountMinor,
            new Money(payment.AmountMinor, payment.Currency.ToString()));
    }

    public async Task<bool> ValidateWebhookAsync(
        string signature,
        string payload,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(true);
    }

    public async Task<PaymentWebhookEvent> ProcessWebhookAsync(
        string signature,
        string payload,
        CancellationToken cancellationToken = default)
    {
        var webhook = JsonSerializer.Deserialize<StripeWebhookPayload>(payload);

        return new PaymentWebhookEvent
        {
            EventId = webhook?.Id ?? string.Empty,
            EventType = webhook?.Type ?? string.Empty,
            PaymentId = Guid.Empty,
            ProviderTransactionId = webhook?.Data?.Object?.Id ?? string.Empty,
            AmountMinor = webhook?.Data?.Object?.Amount ?? 0,
            Currency = webhook?.Data?.Object?.Currency ?? string.Empty,
            Status = MapStripeStatus(webhook?.Data?.Object?.Status),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static PaymentTransactionStatus MapStripeStatus(string? status)
    {
        return status?.ToLower() switch
        {
            "succeeded" => PaymentTransactionStatus.Completed,
            "pending" => PaymentTransactionStatus.Pending,
            "failed" => PaymentTransactionStatus.Failed,
            "refunded" => PaymentTransactionStatus.Refunded,
            _ => PaymentTransactionStatus.Pending
        };
    }
}

internal class StripeChargeResponse
{
    public string Id { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

internal class StripeRefundResponse
{
    public string Id { get; set; } = string.Empty;
}

internal class StripeWebhookPayload
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public StripeWebhookData Data { get; set; } = new();
}

internal class StripeWebhookData
{
    public StripeWebhookObject Object { get; set; } = new();
}

internal class StripeWebhookObject
{
    public string Id { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}