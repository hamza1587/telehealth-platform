namespace Telehealth.Platform.Domain.Payments;

public enum RefundStatus
{
    Initiated = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}