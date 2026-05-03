using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Consultations;

namespace Telehealth.Platform.Api.Appointments;

/// <summary>
/// Appointment scheduling and management API endpoints.
/// </summary>
public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var appointments = app.MapGroup("/appointments").WithTags("Appointments");

        // Patient endpoints
        appointments.MapGet("/my-appointments", GetMyAppointmentsAsync).RequireAuthorization("RequirePatient");
        appointments.MapPost("/book", BookAppointmentAsync).RequireAuthorization("RequirePatient");
        appointments.MapPost("/{appointmentId}/cancel", CancelAppointmentAsync).RequireAuthorization("RequirePatient");
        appointments.MapPost("/{appointmentId}/reschedule", RescheduleAppointmentAsync).RequireAuthorization("RequirePatient");
        appointments.MapGet("/{appointmentId}", GetAppointmentAsync).RequireAuthorization();

        // Doctor endpoints
        appointments.MapGet("/doctor/schedule", GetDoctorScheduleAsync).RequireAuthorization("RequireDoctor");
        appointments.MapPost("/{appointmentId}/confirm", ConfirmAppointmentAsync).RequireAuthorization("RequireDoctor");
        appointments.MapPost("/{appointmentId}/reject", RejectAppointmentAsync).RequireAuthorization("RequireDoctor");
        appointments.MapGet("/doctor/upcoming", GetDoctorUpcomingAppointmentsAsync).RequireAuthorization("RequireDoctor");

        // Availability management (Doctor)
        appointments.MapGet("/doctor/availability", GetDoctorAvailabilitySlotsAsync).RequireAuthorization("RequireDoctor");
        appointments.MapPost("/doctor/availability", SetAvailabilitySlotsAsync).RequireAuthorization("RequireDoctor");
        appointments.MapDelete("/doctor/availability/{slotId}", RemoveAvailabilitySlotAsync).RequireAuthorization("RequireDoctor");

        return app;
    }

    private static async Task<IResult> GetMyAppointmentsAsync(
        ClaimsPrincipal user,
        string? status,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var appointments = new List<AppointmentDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Dr. Jane Smith",
                "Cardiology",
                ConsultationMode.Video,
                AppointmentStatus.Confirmed,
                DateTimeOffset.UtcNow.AddDays(2),
                DateTimeOffset.UtcNow.AddDays(2).AddMinutes(30),
                new MoneyDto(0.50m, "EUR"),
                1800,
                DateTimeOffset.UtcNow.AddDays(-1))
        };

        return Results.Ok(new { Items = appointments, TotalCount = appointments.Count });
    }

    private static async Task<IResult> BookAppointmentAsync(
        BookAppointmentRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var appointment = new AppointmentDto(
            Guid.NewGuid(),
            request.DoctorProfileId,
            "Dr. Jane Smith",
            request.SpecialtyCode,
            request.ConsultationMode,
            AppointmentStatus.PendingConfirmation,
            request.ScheduledStartsAt,
            request.ScheduledEndsAt,
            new MoneyDto(0.50m, "EUR"),
            (long)(request.ScheduledEndsAt - request.ScheduledStartsAt).TotalSeconds,
            DateTimeOffset.UtcNow);

        return Results.Ok(new { Message = "Appointment booked", Appointment = appointment });
    }

    private static async Task<IResult> CancelAppointmentAsync(
        Guid appointmentId,
        CancelAppointmentRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Appointment cancelled",
            AppointmentId = appointmentId,
            CancelledAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });
    }

    private static async Task<IResult> RescheduleAppointmentAsync(
        Guid appointmentId,
        RescheduleAppointmentRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Appointment rescheduled",
            AppointmentId = appointmentId,
            NewStartsAt = request.NewStartsAt,
            NewEndsAt = request.NewEndsAt
        });
    }

    private static async Task<IResult> GetAppointmentAsync(
        Guid appointmentId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var appointment = new AppointmentDetailDto(
            appointmentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Dr. Jane Smith",
            "Cardiology",
            ConsultationMode.Video,
            AppointmentStatus.Confirmed,
            DateTimeOffset.UtcNow.AddDays(2),
            DateTimeOffset.UtcNow.AddDays(2).AddMinutes(30),
            new MoneyDto(0.50m, "EUR"),
            1800,
            DateTimeOffset.UtcNow.AddDays(-1),
            null,
            "Please bring your recent test results.",
            new List<AppointmentNoteDto>()
            {
                new(Guid.NewGuid(), "System", "Appointment created", DateTimeOffset.UtcNow.AddDays(-1))
            });

        return Results.Ok(appointment);
    }

    private static async Task<IResult> GetDoctorScheduleAsync(
        ClaimsPrincipal user,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var schedule = new DoctorScheduleDto(
            Guid.NewGuid(),
            new List<ScheduleSlotDto>
            {
                new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(9), DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(17), true),
                new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(2).Date.AddHours(9), DateTimeOffset.UtcNow.AddDays(2).Date.AddHours(17), true),
                new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(3).Date.AddHours(9), DateTimeOffset.UtcNow.AddDays(3).Date.AddHours(12), false)
            },
            new List<BookedSlotDto>
            {
                new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(10), DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(10).AddMinutes(30), "John Doe", AppointmentStatus.Confirmed),
                new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(14), DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(14).AddMinutes(30), "Jane Smith", AppointmentStatus.PendingConfirmation)
            },
            new List<ScheduleExceptionDto>
            {
                new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7), DateTimeOffset.UtcNow.AddDays(7).AddHours(24), "Conference", true)
            });

        return Results.Ok(schedule);
    }

    private static async Task<IResult> ConfirmAppointmentAsync(
        Guid appointmentId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Appointment confirmed",
            AppointmentId = appointmentId,
            ConfirmedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> RejectAppointmentAsync(
        Guid appointmentId,
        RejectAppointmentRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Appointment rejected",
            AppointmentId = appointmentId,
            Reason = request.Reason,
            RejectedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> GetDoctorUpcomingAppointmentsAsync(
        ClaimsPrincipal user,
        int? daysAhead)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var appointments = new List<DoctorAppointmentDto>
        {
            new(
                Guid.NewGuid(),
                "John Doe",
                "Cardiology consultation",
                ConsultationMode.Video,
                AppointmentStatus.Confirmed,
                DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(10),
                DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(10).AddMinutes(30),
                false,
                null)
        };

        return Results.Ok(new { Items = appointments, TotalCount = appointments.Count });
    }

    private static async Task<IResult> GetDoctorAvailabilitySlotsAsync(
        ClaimsPrincipal user,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var slots = new List<AvailabilitySlotDto>
        {
            new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(9), DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(17), ConsultationMode.Video, true),
            new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(2).Date.AddHours(9), DateTimeOffset.UtcNow.AddDays(2).Date.AddHours(17), ConsultationMode.Video, true),
            new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(3).Date.AddHours(9), DateTimeOffset.UtcNow.AddDays(3).Date.AddHours(12), ConsultationMode.Phone, false)
        };

        return Results.Ok(slots);
    }

    private static async Task<IResult> SetAvailabilitySlotsAsync(
        SetAvailabilityRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var slots = request.Slots.Select(s => new AvailabilitySlotDto(
            Guid.NewGuid(),
            s.StartsAt,
            s.EndsAt,
            s.ConsultationMode,
            s.IsInstantEnabled)).ToList();

        return Results.Ok(new { Message = "Availability set", Slots = slots });
    }

    private static async Task<IResult> RemoveAvailabilitySlotAsync(
        Guid slotId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Availability slot removed", SlotId = slotId });
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

// Enums
public enum AppointmentStatus
{
    Draft,
    PendingConfirmation,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    NoShow,
    Rejected,
    Rescheduled
}

// DTOs
public record AppointmentDto(
    Guid Id,
    Guid DoctorProfileId,
    string DoctorName,
    string SpecialtyCode,
    ConsultationMode ConsultationMode,
    AppointmentStatus Status,
    DateTimeOffset ScheduledStartsAt,
    DateTimeOffset ScheduledEndsAt,
    MoneyDto PricePerSecond,
    long ReservedSeconds,
    DateTimeOffset CreatedAt);

public record AppointmentDetailDto(
    Guid Id,
    Guid PatientAccountId,
    Guid DoctorProfileId,
    string DoctorName,
    string SpecialtyCode,
    ConsultationMode ConsultationMode,
    AppointmentStatus Status,
    DateTimeOffset ScheduledStartsAt,
    DateTimeOffset ScheduledEndsAt,
    MoneyDto PricePerSecond,
    long ReservedSeconds,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    string? Notes,
    List<AppointmentNoteDto> History);

public record AppointmentNoteDto(
    Guid Id,
    string Author,
    string Content,
    DateTimeOffset CreatedAt);

public record DoctorScheduleDto(
    Guid DoctorProfileId,
    List<ScheduleSlotDto> AvailableSlots,
    List<BookedSlotDto> BookedSlots,
    List<ScheduleExceptionDto> Exceptions);

public record ScheduleSlotDto(
    Guid Id,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool IsInstantEnabled);

public record BookedSlotDto(
    Guid AppointmentId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string PatientName,
    AppointmentStatus Status);

public record ScheduleExceptionDto(
    Guid Id,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Reason,
    bool IsAllDay);

public record DoctorAppointmentDto(
    Guid Id,
    string PatientName,
    string Reason,
    ConsultationMode ConsultationMode,
    AppointmentStatus Status,
    DateTimeOffset ScheduledStartsAt,
    DateTimeOffset ScheduledEndsAt,
    bool IsFirstVisit,
    string? Notes);

public record AvailabilitySlotDto(
    Guid Id,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    ConsultationMode ConsultationMode,
    bool IsInstantEnabled);

public record MoneyDto(decimal Amount, string Currency);

public record BookAppointmentRequestDto(
    Guid DoctorProfileId,
    string SpecialtyCode,
    ConsultationMode ConsultationMode,
    DateTimeOffset ScheduledStartsAt,
    DateTimeOffset ScheduledEndsAt,
    string? Notes);

public record CancelAppointmentRequestDto(
    string Reason,
    bool RequestRefund);

public record RescheduleAppointmentRequestDto(
    DateTimeOffset NewStartsAt,
    DateTimeOffset NewEndsAt,
    string? Reason);

public record RejectAppointmentRequestDto(
    string Reason);

public record SetAvailabilityRequestDto(
    List<SlotRequestDto> Slots);

public record SlotRequestDto(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    ConsultationMode ConsultationMode,
    bool IsInstantEnabled);
