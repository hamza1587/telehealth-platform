namespace Telehealth.Platform.Domain.Payments;

public enum PaymentTransactionStatus
{
    Pending = 1,
    Authorized = 2,
    Completed = 3,
    Failed = 4,
    Refunded = 5,
    Chargeback = 6,
    Disputed = 7
}