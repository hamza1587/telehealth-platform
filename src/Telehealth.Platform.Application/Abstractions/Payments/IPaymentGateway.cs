using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Payments;

namespace Telehealth.Platform.Application.Abstractions.Payments;

public interface IPaymentGateway
{
    string ProviderName { get; }

    Task<PaymentTransaction> AuthorizePaymentAsync(
        Payment payment,
        PaymentMethod paymentMethod,
        CancellationToken cancellationToken = default);

    Task<PaymentTransaction> CapturePaymentAsync(
        Payment payment,
        CancellationToken cancellationToken = default);

    Task<PaymentTransaction> RefundPaymentAsync(
        Refund refund,
        Payment payment,
        CancellationToken cancellationToken = default);

    Task<PaymentTransaction> VoidPaymentAsync(
        Payment payment,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateWebhookAsync(
        string signature,
        string payload,
        CancellationToken cancellationToken = default);

    Task<PaymentWebhookEvent> ProcessWebhookAsync(
        string signature,
        string payload,
        CancellationToken cancellationToken = default);
}

public class PaymentWebhookEvent
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public string ProviderTransactionId { get; set; } = string.Empty;
    public long AmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentTransactionStatus Status { get; set; }
    public string StatusDetails { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}