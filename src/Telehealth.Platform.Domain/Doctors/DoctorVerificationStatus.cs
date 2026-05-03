namespace Telehealth.Platform.Domain.Doctors;

public enum DoctorVerificationStatus
{
    Draft = 1,
    PendingVerification = 2,
    Submitted = 3,
    InReview = 4,
    MoreInformationRequired = 5,
    Verified = 6,
    Rejected = 7,
    Expired = 8,
    Suspended = 9,
}
