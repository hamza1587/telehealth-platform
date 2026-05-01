namespace Telehealth.Platform.Domain.Compliance;

public enum GdprRequestStatus
{
    Submitted = 1,
    IdentityVerificationRequired = 2,
    InReview = 3,
    Approved = 4,
    PartiallyApproved = 5,
    Rejected = 6,
    Completed = 7,
}
