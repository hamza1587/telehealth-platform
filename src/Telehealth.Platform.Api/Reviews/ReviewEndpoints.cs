using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Telehealth.Platform.Application.Abstractions.Reviews;
using Telehealth.Platform.Domain.Reviews;

namespace Telehealth.Platform.Api.Reviews;

internal static class ReviewEndpoints
{
    public static IEndpointRouteBuilder MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var reviews = app.MapGroup("/platform/reviews").WithTags("Reviews");

        reviews.MapPost("/", CreateReviewAsync).RequireAuthorization("RequirePatient");
        reviews.MapGet("/doctor/{doctorId:guid}", GetDoctorReviewsAsync).RequireAuthorization();
        reviews.MapGet("/patient", GetPatientReviewsAsync).RequireAuthorization("RequirePatient");
        reviews.MapPut("/{reviewId:guid}/approve", ApproveReviewAsync).RequireAuthorization("RequireAdmin");
        reviews.MapPut("/{reviewId:guid}/reject", RejectReviewAsync).RequireAuthorization("RequireAdmin");

        return app;
    }

    private static async Task<Results<Ok<ReviewResponse>, ValidationProblem>> CreateReviewAsync(
        ClaimsPrincipal user,
        CreateReviewRequest request,
        IReviewService reviewService,
        CancellationToken cancellationToken)
    {
        var patientAccountId = GetAccountId(user);
        if (!patientAccountId.HasValue)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Authentication"] = ["User is not authenticated as patient."]
            });

        if (request.Rating < 1 || request.Rating > 5)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Rating"] = ["Rating must be between 1 and 5."]
            });
        }

        var review = await reviewService.CreateReviewAsync(
            patientAccountId.Value,
            request.DoctorProfileId,
            request.Rating,
            request.ConsultationBookingId,
            request.Comment,
            cancellationToken);

        return TypedResults.Ok(new ReviewResponse
        {
            Id = review.Id,
            PatientAccountId = review.PatientAccountId,
            DoctorProfileId = review.DoctorProfileId,
            Rating = review.Rating,
            Comment = review.Comment,
            Status = review.Status.ToString(),
            CreatedAt = review.CreatedAt
        });
    }

    private static async Task<Ok<IEnumerable<ReviewResponse>>> GetDoctorReviewsAsync(
        Guid doctorId,
        IReviewService reviewService,
        CancellationToken cancellationToken)
    {
        var reviews = await reviewService.GetDoctorReviewsAsync(doctorId, cancellationToken);

        var response = reviews.Select(r => new ReviewResponse
        {
            Id = r.Id,
            PatientAccountId = r.PatientAccountId,
            DoctorProfileId = r.DoctorProfileId,
            Rating = r.Rating,
            Comment = r.Comment,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt
        });

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<IEnumerable<ReviewResponse>>, UnauthorizedHttpResult>> GetPatientReviewsAsync(
        ClaimsPrincipal user,
        IReviewService reviewService,
        CancellationToken cancellationToken)
    {
        var patientAccountId = GetAccountId(user);
        if (!patientAccountId.HasValue)
            return TypedResults.Unauthorized();

        var reviews = await reviewService.GetPatientReviewsAsync(patientAccountId.Value, cancellationToken);

        var response = reviews.Select(r => new ReviewResponse
        {
            Id = r.Id,
            PatientAccountId = r.PatientAccountId,
            DoctorProfileId = r.DoctorProfileId,
            Rating = r.Rating,
            Comment = r.Comment,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt
        });

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<ReviewResponse>, NotFound>> ApproveReviewAsync(
        Guid reviewId,
        IReviewService reviewService,
        CancellationToken cancellationToken)
    {
        try
        {
            var review = await reviewService.ApproveReviewAsync(reviewId, "admin", cancellationToken);
            return TypedResults.Ok(new ReviewResponse
            {
                Id = review.Id,
                PatientAccountId = review.PatientAccountId,
                DoctorProfileId = review.DoctorProfileId,
                Rating = review.Rating,
                Comment = review.Comment,
                Status = review.Status.ToString(),
                CreatedAt = review.CreatedAt
            });
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<Ok<ReviewResponse>, NotFound>> RejectReviewAsync(
        Guid reviewId,
        IReviewService reviewService,
        CancellationToken cancellationToken)
    {
        try
        {
            var review = await reviewService.RejectReviewAsync(reviewId, "admin", cancellationToken);
            return TypedResults.Ok(new ReviewResponse
            {
                Id = review.Id,
                PatientAccountId = review.PatientAccountId,
                DoctorProfileId = review.DoctorProfileId,
                Rating = review.Rating,
                Comment = review.Comment,
                Status = review.Status.ToString(),
                CreatedAt = review.CreatedAt
            });
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }

    private static Guid? GetAccountId(ClaimsPrincipal user)
    {
        var accountClaim = user.FindFirst("account_id")?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(accountClaim, out var accountId) ? accountId : null;
    }
}

public class CreateReviewRequest
{
    public Guid DoctorProfileId { get; set; }
    public int Rating { get; set; }
    public Guid? ConsultationBookingId { get; set; }
    public string? Comment { get; set; }
}

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid PatientAccountId { get; set; }
    public Guid DoctorProfileId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}