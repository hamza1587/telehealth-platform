using Microsoft.Extensions.Logging;
using Telehealth.Platform.Application.Abstractions.Payments;
using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Payments;

namespace Telehealth.Platform.Application.Payments;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }

    public Task<Payment> CreatePaymentAsync(
        Guid patientAccountId,
        Guid? paymentMethodId,
        long amountMinor,
        CurrencyCode currency,
        string description,
        CancellationToken cancellationToken = default)
    {
        var payment = Payment.Create(
            patientAccountId,
            paymentMethodId,
            amountMinor,
            currency,
            description);

        _logger.LogInformation(
            "Payment created: {PaymentId} for patient {PatientId} with amount {Amount} {Currency}",
            payment.Id,
            patientAccountId,
            amountMinor,
            currency);

        return Task.FromResult(payment);
    }

    public Task<Payment> GetPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Payment>> GetPaymentsByPatientAsync(Guid patientAccountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaymentResult> ProcessPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = Guid.NewGuid().ToString("N")
        });
    }

    public Task<bool> CancelPaymentAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Refund> CreateRefundAsync(
        Guid paymentId,
        long amountMinor,
        string currency,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var refund = Refund.Create(paymentId, amountMinor, currency, reason);

        _logger.LogInformation(
            "Refund created: {RefundId} for payment {PaymentId} with amount {Amount}",
            refund.Id,
            paymentId,
            amountMinor);

        return Task.FromResult(refund);
    }

    public Task<Dispute> CreateDisputeAsync(
        Guid paymentId,
        DisputeReason reason,
        string reasonDescription,
        string evidence,
        CancellationToken cancellationToken = default)
    {
        var dispute = Dispute.Create(paymentId, reason, reasonDescription, evidence);

        _logger.LogInformation(
            "Dispute created: {DisputeId} for payment {PaymentId} with reason {Reason}",
            dispute.Id,
            paymentId,
            reason);

        return Task.FromResult(dispute);
    }

    public Task<PaymentMethod> AddPaymentMethodAsync(
        Guid patientAccountId,
        PaymentMethodType type,
        string provider,
        string token,
        string lastFour,
        string brand,
        DateTimeOffset expiryMonth,
        bool isDefault,
        CancellationToken cancellationToken = default)
    {
        PaymentMethod paymentMethod;

        if (type == PaymentMethodType.CreditCard || type == PaymentMethodType.DebitCard)
        {
            paymentMethod = PaymentMethod.CreateCard(
                patientAccountId,
                type,
                provider,
                token,
                lastFour,
                brand,
                expiryMonth,
                isDefault);
        }
        else if (type == PaymentMethodType.BankTransfer)
        {
            paymentMethod = PaymentMethod.CreateBankTransfer(
                patientAccountId,
                provider,
                token,
                lastFour);
        }
        else
        {
            paymentMethod = PaymentMethod.CreateWallet(
                patientAccountId,
                type,
                provider,
                token);
        }

        _logger.LogInformation(
            "Payment method added: {PaymentMethodId} for patient {PatientId} with type {Type}",
            paymentMethod.Id,
            patientAccountId,
            type);

        return Task.FromResult(paymentMethod);
    }

    public Task<PaymentMethod> GetDefaultPaymentMethodAsync(Guid patientAccountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(Guid patientAccountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemovePaymentMethodAsync(Guid paymentMethodId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<decimal> GetBalanceAsync(Guid patientAccountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}