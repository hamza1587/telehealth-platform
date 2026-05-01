namespace Telehealth.Platform.Domain.Billing;

public enum BillingSessionStatus
{
    Pending = 1,
    Finalized = 2,
    Failed = 3,
    Reversed = 4,
}
