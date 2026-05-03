using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Consultations;

namespace Telehealth.Platform.Api.Consultations;

/// <summary>
/// Voice and video consultation API endpoints.
/// </summary>
public static class ConsultationEndpoints
{
    public static IEndpointRouteBuilder MapConsultationEndpoints(this IEndpointRouteBuilder app)
    {
        var consultations = app.MapGroup("/consultations").WithTags("Consultations");

        consultations.MapGet("/{consultationId}", GetConsultationAsync).RequireAuthorization();
        consultations.MapPost("/{consultationId}/start", StartConsultationAsync).RequireAuthorization();
        consultations.MapPost("/{consultationId}/join", JoinConsultationAsync).RequireAuthorization();
        consultations.MapPost("/{consultationId}/leave", LeaveConsultationAsync).RequireAuthorization();
        consultations.MapPost("/{consultationId}/end", EndConsultationAsync).RequireAuthorization();
        consultations.MapPost("/{consultationId}/extend", ExtendConsultationAsync).RequireAuthorization();

        // Signaling for WebRTC
        consultations.MapPost("/{consultationId}/signal/offer", SendOfferAsync).RequireAuthorization();
        consultations.MapPost("/{consultationId}/signal/answer", SendAnswerAsync).RequireAuthorization();
        consultations.MapPost("/{consultationId}/signal/ice", SendIceCandidateAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetConsultationAsync(
        Guid consultationId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var consultation = new ConsultationDetailDto(
            consultationId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Dr. Jane Smith",
            "John Doe",
            ConsultationMode.Video,
            ConsultationStatus.InProgress,
            DateTimeOffset.UtcNow.AddMinutes(-10),
            null,
            600,
            new MoneyDto(0.50m, "EUR"),
            new MoneyDto(300m, "EUR"),
            "secure-room-token-12345",
            DateTimeOffset.UtcNow.AddMinutes(20));

        return Results.Ok(consultation);
    }

    private static async Task<IResult> StartConsultationAsync(
        Guid consultationId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Consultation started",
            ConsultationId = consultationId,
            StartedAt = DateTimeOffset.UtcNow,
            RoomToken = "secure-room-token-12345"
        });
    }

    private static async Task<IResult> JoinConsultationAsync(
        Guid consultationId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Joined consultation",
            ConsultationId = consultationId,
            JoinedAt = DateTimeOffset.UtcNow,
            RoomToken = "secure-room-token-12345"
        });
    }

    private static async Task<IResult> LeaveConsultationAsync(
        Guid consultationId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Left consultation",
            ConsultationId = consultationId,
            LeftAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> EndConsultationAsync(
        Guid consultationId,
        EndConsultationRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Consultation ended",
            ConsultationId = consultationId,
            EndedAt = DateTimeOffset.UtcNow,
            DurationSeconds = 1800,
            FinalCharge = new MoneyDto(450m, "EUR"),
            Reason = request.Reason
        });
    }

    private static async Task<IResult> ExtendConsultationAsync(
        Guid consultationId,
        ExtendConsultationRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Consultation extended",
            ConsultationId = consultationId,
            ExtendedBySeconds = request.ExtendBySeconds,
            NewEstimatedEnd = DateTimeOffset.UtcNow.AddSeconds(request.ExtendBySeconds)
        });
    }

    private static async Task<IResult> SendOfferAsync(
        Guid consultationId,
        SignalRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Offer sent" });
    }

    private static async Task<IResult> SendAnswerAsync(
        Guid consultationId,
        SignalRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Answer sent" });
    }

    private static async Task<IResult> SendIceCandidateAsync(
        Guid consultationId,
        SignalRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "ICE candidate sent" });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}

public enum ConsultationStatus
{
    Scheduled,
    Waiting,
    InProgress,
    Paused,
    Extended,
    Ended,
    Cancelled,
    NoShow
}

public record ConsultationDetailDto(
    Guid Id,
    Guid PatientAccountId,
    Guid DoctorProfileId,
    string DoctorName,
    string PatientName,
    ConsultationMode Mode,
    ConsultationStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    long DurationSeconds,
    MoneyDto PricePerSecond,
    MoneyDto CurrentCharge,
    string RoomToken,
    DateTimeOffset? ExpiresAt);

public record MoneyDto(decimal Amount, string Currency);

public record EndConsultationRequestDto(
    string? Reason,
    bool? IsTechnicalIssue);

public record ExtendConsultationRequestDto(
    int ExtendBySeconds,
    string? PatientConsent);

public record SignalRequestDto(
    string Type,
    object Payload,
    string? TargetUserId);
