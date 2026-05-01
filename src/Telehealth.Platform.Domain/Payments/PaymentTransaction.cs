using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Payments;

public sealed class PaymentTransaction : Entity<Guid>
{
    public PaymentTransaction(
        Guid id,
        Guid patientAccountId,
        string provider,
        string providerTransactionId,
        Money amount)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        Provider = provider;
        ProviderTransactionId = providerTransactionId;
        Amount = amount;
        Status = PaymentTransactionStatus.Pending;
    }

    public Guid PatientAccountId { get; }

    public string Provider { get; }

    public string ProviderTransactionId { get; }

    public Money Amount { get; }

    public PaymentTransactionStatus Status { get; private set; }
}
