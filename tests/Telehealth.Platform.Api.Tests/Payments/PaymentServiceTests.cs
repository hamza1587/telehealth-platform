using Xunit;
using FluentAssertions;
using Telehealth.Platform.Domain.Payments;
using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Api.Tests.Payments;

public class PaymentServiceTests
{
    [Fact]
    public void Payment_Create_InitializesCorrectly()
    {
        var patientAccountId = Guid.NewGuid();
        var amountMinor = 1000L;
        var currency = CurrencyCode.USD;
        var description = "Test payment";

        var payment = Payment.Create(
            patientAccountId,
            null,
            amountMinor,
            currency,
            description);

        payment.PatientAccountId.Should().Be(patientAccountId);
        payment.AmountMinor.Should().Be(amountMinor);
        payment.Currency.Should().Be(currency);
        payment.Description.Should().Be(description);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Payment_SetCompleted_UpdatesStatus()
    {
        var payment = Payment.Create(
            Guid.NewGuid(),
            null,
            1000L,
            CurrencyCode.USD,
            "Test");

        payment.SetCompleted("ext-123");

        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.ExternalPaymentId.Should().Be("ext-123");
        payment.CompletedAt.Should().HaveValue();
    }

    [Fact]
    public void Payment_SetFailed_UpdatesStatus()
    {
        var payment = Payment.Create(
            Guid.NewGuid(),
            null,
            1000L,
            CurrencyCode.USD,
            "Test");

        payment.SetFailed("Insufficient funds");

        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.StatusDetails.Should().Be("Insufficient funds");
        payment.FailedAt.Should().HaveValue();
    }

    [Fact]
    public void Refund_Create_InitializesCorrectly()
    {
        var paymentId = Guid.NewGuid();
        var amountMinor = 500L;
        var currency = "USD";
        var reason = "Customer requested";

        var refund = Refund.Create(paymentId, amountMinor, currency, reason);

        refund.PaymentId.Should().Be(paymentId);
        refund.AmountMinor.Should().Be(amountMinor);
        refund.Currency.Should().Be(currency);
        refund.Reason.Should().Be(reason);
        refund.Status.Should().Be(RefundStatus.Initiated);
    }

    [Fact]
    public void Refund_MarkCompleted_UpdatesStatus()
    {
        var refund = Refund.Create(Guid.NewGuid(), 500L, "USD", "Test");

        refund.MarkCompleted("refund-ext-123");

        refund.Status.Should().Be(RefundStatus.Completed);
        refund.ExternalRefundId.Should().Be("refund-ext-123");
        refund.CompletedAt.Should().HaveValue();
    }

    [Fact]
    public void PaymentMethod_CreateCard_InitializesCorrectly()
    {
        var patientAccountId = Guid.NewGuid();
        var token = "tok_visa_1234";
        var lastFour = "1234";
        var brand = "Visa";
        var expiryMonth = new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero);

        var paymentMethod = PaymentMethod.CreateCard(
            patientAccountId,
            PaymentMethodType.CreditCard,
            "Visa",
            token,
            lastFour,
            brand,
            expiryMonth);

        paymentMethod.PatientAccountId.Should().Be(patientAccountId);
        paymentMethod.Type.Should().Be(PaymentMethodType.CreditCard);
        paymentMethod.Token.Should().Be(token);
        paymentMethod.LastFour.Should().Be(lastFour);
        paymentMethod.Brand.Should().Be(brand);
        paymentMethod.IsVerified.Should().BeFalse();
    }

    [Fact]
    public void PaymentMethod_CreateWallet_InitializesCorrectly()
    {
        var patientAccountId = Guid.NewGuid();
        var token = "wallet-apple-123";

        var paymentMethod = PaymentMethod.CreateWallet(
            patientAccountId,
            PaymentMethodType.ApplePay,
            "ApplePay",
            token);

        paymentMethod.PatientAccountId.Should().Be(patientAccountId);
        paymentMethod.Type.Should().Be(PaymentMethodType.ApplePay);
        paymentMethod.Token.Should().Be(token);
        paymentMethod.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void Dispute_Create_InitializesCorrectly()
    {
        var paymentId = Guid.NewGuid();
        var reason = DisputeReason.Fraudulent;
        var reasonDescription = "Unauthorized transaction";

        var dispute = Dispute.Create(paymentId, reason, reasonDescription);

        dispute.PaymentId.Should().Be(paymentId);
        dispute.Reason.Should().Be(reason);
        dispute.ReasonDescription.Should().Be(reasonDescription);
        dispute.Status.Should().Be(DisputeStatus.Open);
    }

    [Fact]
    public void PaymentTransaction_Create_InitializesCorrectly()
    {
        var paymentId = Guid.NewGuid();
        var patientAccountId = Guid.NewGuid();
        var amount = new Money(1000L, "USD");

        var transaction = PaymentTransaction.Create(
            paymentId,
            patientAccountId,
            "Stripe",
            "txn_123",
            1000L,
            amount);

        transaction.PaymentId.Should().Be(paymentId);
        transaction.PatientAccountId.Should().Be(patientAccountId);
        transaction.Provider.Should().Be("Stripe");
        transaction.ProviderTransactionId.Should().Be("txn_123");
        transaction.AmountMinor.Should().Be(1000L);
        transaction.Status.Should().Be(PaymentTransactionStatus.Pending);
    }
}