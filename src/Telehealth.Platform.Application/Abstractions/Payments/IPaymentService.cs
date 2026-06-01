using System.Net;
using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Payments;

namespace Telehealth.Platform.Application.Abstractions.Payments;

public interface IPaymentService
{
    Task<Payment> CreatePaymentAsync(
        Guid patientAccountId,
        Guid? paymentMethodId,
        long amountMinor,
        CurrencyCode currency,
        string description,
        CancellationToken cancellationToken = default);

    Task<Payment> GetPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Payment>> GetPaymentsByPatientAsync(Guid patientAccountId, CancellationToken cancellationToken = default);

    Task<PaymentResult> ProcessPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<bool> CancelPaymentAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default);

    Task<Refund> CreateRefundAsync(
        Guid paymentId,
        long amountMinor,
        string currency,
        string reason,
        CancellationToken cancellationToken = default);

    Task<Dispute> CreateDisputeAsync(
        Guid paymentId,
        DisputeReason reason,
        string reasonDescription,
        string evidence,
        CancellationToken cancellationToken = default);

    Task<PaymentMethod> AddPaymentMethodAsync(
        Guid patientAccountId,
        PaymentMethodType type,
        string provider,
        string token,
        string lastFour,
        string brand,
        DateTimeOffset expiryMonth,
        bool isDefault,
        CancellationToken cancellationToken = default);

    Task<PaymentMethod> GetDefaultPaymentMethodAsync(Guid patientAccountId, CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(Guid patientAccountId, CancellationToken cancellationToken = default);

    Task<bool> RemovePaymentMethodAsync(Guid paymentMethodId, CancellationToken cancellationToken = default);

    Task<decimal> GetBalanceAsync(Guid patientAccountId, CancellationToken cancellationToken = default);
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public IPAddress? ClientIp { get; set; }
}