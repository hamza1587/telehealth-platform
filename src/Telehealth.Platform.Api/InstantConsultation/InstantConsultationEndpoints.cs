using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Domain.InstantConsultation;

namespace Telehealth.Platform.Api.InstantConsultation;

/// <summary>
/// Instant consultation queue API endpoints.
/// </summary>
public static class InstantConsultationEndpoints
{
    public static IEndpointRouteBuilder MapInstantConsultationEndpoints(this IEndpointRouteBuilder app)
    {
        var instant = app.MapGroup("/instant-consultation").WithTags("Instant Consultation");

        // Patient endpoints
        instant.MapPost("/queue/join", JoinQueueAsync).RequireAuthorization("RequirePatient");
        instant.MapGet("/queue/status", GetQueueStatusAsync).RequireAuthorization("RequirePatient");
        instant.MapPost("/queue/leave", LeaveQueueAsync).RequireAuthorization("RequirePatient");
        instant.MapPost("/match/accept", AcceptMatchAsync).RequireAuthorization("RequirePatient");
        instant.MapPost("/match/decline", DeclineMatchAsync).RequireAuthorization("RequirePatient");

        // Doctor endpoints
        instant.MapPost("/availability", SetInstantAvailabilityAsync).RequireAuthorization("RequireDoctor");
        instant.MapGet("/availability", GetInstantAvailabilityAsync).RequireAuthorization("RequireDoctor");
        instant.MapPost("/accept-patient", AcceptQueuedPatientAsync).RequireAuthorization("RequireDoctor");
        instant.MapGet("/queue/next", GetNextPatientAsync).RequireAuthorization("RequireDoctor");
        instant.MapPost("/availability/toggle", ToggleAvailabilityAsync).RequireAuthorization("RequireDoctor");

        return app;
    }

    private static async Task<IResult> JoinQueueAsync(
        JoinQueueRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var queueEntry = new QueueEntryDto(
            Guid.NewGuid(),
            request.SpecialtyCode,
            request.PreferredMode,
            request.MaxPricePerSecond,
            request.Currency,
            QueueStatus.Waiting,
            0,
            DateTimeOffset.UtcNow,
            null,
            null,
            DateTimeOffset.UtcNow.AddMinutes(15));

        return Results.Ok(new
        {
            Message = "Joined instant consultation queue",
            Position = 1,
            EstimatedWaitMinutes = 5,
            Entry = queueEntry
        });
    }

    private static async Task<IResult> GetQueueStatusAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var status = new QueueStatusDto(
            true,
            Guid.NewGuid(),
            QueueStatus.Matched,
            0,
            5,
            DateTimeOffset.UtcNow.AddMinutes(2),
            new DoctorMatchDto(
                Guid.NewGuid(),
                "Dr. Jane Smith",
                "Cardiology",
                new List<string> { "English", "Spanish" },
                4.8,
                127,
                0.45m,
                "EUR"));

        return Results.Ok(status);
    }

    private static async Task<IResult> LeaveQueueAsync(
        ClaimsPrincipal user,
        string? reason)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Left queue successfully", Reason = reason });
    }

    private static async Task<IResult> AcceptMatchAsync(
        AcceptMatchRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Match accepted",
            BookingId = Guid.NewGuid(),
            ConsultationUrl = $"/consultation/{Guid.NewGuid()}",
            AcceptedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> DeclineMatchAsync(
        DeclineMatchRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Match declined",
            Reason = request.Reason,
            Requeued = true,
            NewPosition = 1
        });
    }

    private static async Task<IResult> SetInstantAvailabilityAsync(
        SetInstantAvailabilityRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var availability = new InstantAvailabilityDto(
            Guid.NewGuid(),
            true,
            request.MaxQueueSize,
            0,
            request.MinPricePerSecond,
            request.Currency,
            request.AvailableSpecialties,
            request.AvailableModes,
            DateTimeOffset.UtcNow);

        return Results.Ok(new { Message = "Instant availability configured", Availability = availability });
    }

    private static async Task<IResult> GetInstantAvailabilityAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var availability = new InstantAvailabilityDto(
            Guid.NewGuid(),
            true,
            5,
            2,
            0.40m,
            "EUR",
            new List<string> { "Cardiology", "Internal Medicine" },
            new List<string> { "Video", "Phone" },
            DateTimeOffset.UtcNow);

        return Results.Ok(availability);
    }

    private static async Task<IResult> AcceptQueuedPatientAsync(
        AcceptPatientRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Patient accepted",
            BookingId = Guid.NewGuid(),
            ConsultationUrl = $"/consultation/{Guid.NewGuid()}",
            PatientId = request.QueueEntryId,
            AcceptedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> GetNextPatientAsync(
        string? preferredSpecialty,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var patient = new QueuedPatientDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "John Doe",
            35,
            "Male",
            "Cardiology",
            ConsultationMode.Video,
            0.50m,
            "EUR",
            1,
            DateTimeOffset.UtcNow.AddMinutes(-2),
            "First consultation for chest pain");

        return Results.Ok(patient);
    }

    private static async Task<IResult> ToggleAvailabilityAsync(
        ToggleAvailabilityRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            IsAvailable = request.IsAvailable,
            ToggledAt = DateTimeOffset.UtcNow,
            Message = request.IsAvailable ? "You are now available for instant consultations" : "You are now unavailable for instant consultations"
        });
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

// DTOs
public record JoinQueueRequestDto(
    string SpecialtyCode,
    ConsultationMode PreferredMode,
    decimal MaxPricePerSecond,
    string Currency,
    string? Reason);

public record QueueEntryDto(
    Guid Id,
    string SpecialtyCode,
    ConsultationMode PreferredMode,
    decimal MaxPricePerSecond,
    string Currency,
    QueueStatus Status,
    int Priority,
    DateTimeOffset QueuedAt,
    DateTimeOffset? MatchedAt,
    Guid? MatchedDoctorId,
    DateTimeOffset? ExpiresAt);

public record QueueStatusDto(
    bool IsInQueue,
    Guid? QueueEntryId,
    QueueStatus Status,
    int Position,
    int? EstimatedWaitMinutes,
    DateTimeOffset? MatchExpiresAt,
    DoctorMatchDto? MatchedDoctor);

public record DoctorMatchDto(
    Guid DoctorProfileId,
    string DisplayName,
    string PrimarySpecialty,
    List<string> Languages,
    double Rating,
    int ConsultationsCompleted,
    decimal PricePerSecond,
    string Currency);

public record AcceptMatchRequestDto(Guid QueueEntryId);

public record DeclineMatchRequestDto(Guid QueueEntryId, string? Reason);

public record SetInstantAvailabilityRequestDto(
    bool IsAvailable,
    int? MaxQueueSize,
    decimal? MinPricePerSecond,
    string Currency,
    List<string> AvailableSpecialties,
    List<string> AvailableModes);

public record InstantAvailabilityDto(
    Guid Id,
    bool IsAvailable,
    int? MaxQueueSize,
    int CurrentQueueSize,
    decimal? MinPricePerSecond,
    string Currency,
    List<string> AvailableSpecialties,
    List<string> AvailableModes,
    DateTimeOffset LastUpdated);

public record AcceptPatientRequestDto(Guid QueueEntryId);

public record QueuedPatientDto(
    Guid QueueEntryId,
    Guid PatientAccountId,
    string PatientName,
    int? Age,
    string? Gender,
    string SpecialtyCode,
    ConsultationMode PreferredMode,
    decimal MaxPricePerSecond,
    string Currency,
    int Priority,
    DateTimeOffset QueuedAt,
    string? Reason);

public record ToggleAvailabilityRequestDto(bool IsAvailable);
