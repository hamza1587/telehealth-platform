using Telehealth.Platform.Domain.Reviews;

namespace Telehealth.Platform.Application.Abstractions.Reviews;

public interface IReviewService
{
    Task<Review> CreateReviewAsync(
        Guid patientAccountId,
        Guid doctorProfileId,
        int rating,
        Guid? consultationBookingId,
        string? comment,
        CancellationToken cancellationToken = default);

    Task<Review?> GetReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Review>> GetDoctorReviewsAsync(
        Guid doctorProfileId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Review>> GetPatientReviewsAsync(Guid patientAccountId, CancellationToken cancellationToken = default);

    Task<Review> ApproveReviewAsync(Guid reviewId, string reviewedBy, CancellationToken cancellationToken = default);

    Task<Review> RejectReviewAsync(Guid reviewId, string reviewedBy, CancellationToken cancellationToken = default);

    Task<double> GetAverageRatingAsync(Guid doctorProfileId, CancellationToken cancellationToken = default);
}