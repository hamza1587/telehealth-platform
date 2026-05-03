using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Reviews;

public class Review : Entity<Guid>
{
    public Guid PatientAccountId { get; private set; }

    public Guid DoctorProfileId { get; private set; }

    public Guid? ConsultationBookingId { get; private set; }

    public int Rating { get; private set; }

    public string? Comment { get; private set; }

    public ReviewStatus Status { get; private set; } = ReviewStatus.Pending;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public string? ReviewedBy { get; private set; }

    private Review(
        Guid id,
        Guid patientAccountId,
        Guid doctorProfileId,
        int rating) : base(id)
    {
        PatientAccountId = patientAccountId;
        DoctorProfileId = doctorProfileId;
        Rating = rating;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Review Create(
        Guid patientAccountId,
        Guid doctorProfileId,
        int rating,
        Guid? consultationBookingId = null,
        string? comment = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

        var review = new Review(Guid.NewGuid(), patientAccountId, doctorProfileId, rating)
        {
            ConsultationBookingId = consultationBookingId,
            Comment = comment
        };

        return review;
    }

    public void Approve(string reviewedBy)
    {
        Status = ReviewStatus.Approved;
        ReviewedAt = DateTimeOffset.UtcNow;
        ReviewedBy = reviewedBy;
    }

    public void Reject(string reviewedBy)
    {
        Status = ReviewStatus.Rejected;
        ReviewedAt = DateTimeOffset.UtcNow;
        ReviewedBy = reviewedBy;
    }
}