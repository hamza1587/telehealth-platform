using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Reviews;
using Telehealth.Platform.Domain.Reviews;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Reviews;

public class ReviewService : IReviewService
{
    private readonly PlatformDbContext _dbContext;

    public ReviewService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Review> CreateReviewAsync(
        Guid patientAccountId,
        Guid doctorProfileId,
        int rating,
        Guid? consultationBookingId,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        var review = Review.Create(patientAccountId, doctorProfileId, rating, consultationBookingId, comment);
        await _dbContext.Reviews.AddAsync(review, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return review;
    }

    public async Task<Review?> GetReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
    }

    public async Task<IEnumerable<Review>> GetDoctorReviewsAsync(
        Guid doctorProfileId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reviews
            .Where(r => r.DoctorProfileId == doctorProfileId && r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Review>> GetPatientReviewsAsync(
        Guid patientAccountId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reviews
            .Where(r => r.PatientAccountId == patientAccountId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Review> ApproveReviewAsync(
        Guid reviewId,
        string reviewedBy,
        CancellationToken cancellationToken = default)
    {
        var review = await _dbContext.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken)
            ?? throw new InvalidOperationException($"Review with ID {reviewId} not found");

        review.Approve(reviewedBy);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return review;
    }

    public async Task<Review> RejectReviewAsync(
        Guid reviewId,
        string reviewedBy,
        CancellationToken cancellationToken = default)
    {
        var review = await _dbContext.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken)
            ?? throw new InvalidOperationException($"Review with ID {reviewId} not found");

        review.Reject(reviewedBy);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return review;
    }

    public async Task<double> GetAverageRatingAsync(
        Guid doctorProfileId,
        CancellationToken cancellationToken = default)
    {
        var reviews = await _dbContext.Reviews
            .Where(r => r.DoctorProfileId == doctorProfileId && r.Status == ReviewStatus.Approved)
            .ToListAsync(cancellationToken);

        if (!reviews.Any())
            return 0;

        return reviews.Average(r => r.Rating);
    }
}