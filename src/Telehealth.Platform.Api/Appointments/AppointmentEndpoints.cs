using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Application.Abstractions.Consultations;
using Telehealth.Platform.Application.Consultations;
using Telehealth.Platform.Domain.Consultations;

namespace Telehealth.Platform.Api.Appointments;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var appointments = app.MapGroup("/appointments").WithTags("Appointments");

        appointments.MapGet("/my-appointments", GetMyAppointmentsAsync).RequireAuthorization("RequirePatient");
        appointments.MapPost("/book", BookAppointmentAsync).RequireAuthorization("RequirePatient");
        appointments.MapPost("/{appointmentId}/cancel", CancelAppointmentAsync).RequireAuthorization("RequirePatient");
        appointments.MapPost("/{appointmentId}/reschedule", RescheduleAppointmentAsync).RequireAuthorization("RequirePatient");
        appointments.MapGet("/{appointmentId}", GetAppointmentAsync).RequireAuthorization();

        appointments.MapGet("/doctor/schedule", GetDoctorScheduleAsync).RequireAuthorization("RequireDoctor");
        appointments.MapPost("/{appointmentId}/confirm", ConfirmAppointmentAsync).RequireAuthorization("RequireDoctor");
        appointments.MapPost("/{appointmentId}/reject", RejectAppointmentAsync).RequireAuthorization("RequireDoctor");
        appointments.MapGet("/doctor/upcoming", GetDoctorUpcomingAppointmentsAsync).RequireAuthorization("RequireDoctor");

        appointments.MapGet("/doctor/availability", GetDoctorAvailabilitySlotsAsync).RequireAuthorization("RequireDoctor");
        appointments.MapPost("/doctor/availability", SetAvailabilitySlotsAsync).RequireAuthorization("RequireDoctor");
        appointments.MapDelete("/doctor/availability/{slotId}", RemoveAvailabilitySlotAsync).RequireAuthorization("RequireDoctor");

        return app;
    }

    private static async Task<IResult> GetMyAppointmentsAsync(
        ClaimsPrincipal user,
        string? status,
        DateTimeOffset? from,
        DateTimeOffset? to,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var appointments = await teleconsultationService.GetPatientAppointmentsAsync(userId.Value, status, from, to);
        var appointmentsList = appointments.ToList();
        return Results.Ok(new { Items = appointmentsList, TotalCount = appointmentsList.Count });
    }

    private static async Task<IResult> BookAppointmentAsync(
        BookAppointmentRequestDto request,
        ClaimsPrincipal user,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var appointment = await teleconsultationService.BookAppointmentAsync(userId.Value, request);
        return Results.Ok(new { Message = "Appointment booked", Appointment = appointment });
    }

    private static async Task<IResult> CancelAppointmentAsync(
        Guid appointmentId,
        CancelAppointmentRequestDto request,
        ClaimsPrincipal user,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        await teleconsultationService.CancelAppointmentAsync(appointmentId, userId.Value, request.Reason);
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
        ClaimsPrincipal user,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var appointment = await teleconsultationService.RescheduleAppointmentAsync(appointmentId, userId.Value, request);
        return Results.Ok(new
        {
            Message = "Appointment rescheduled",
            AppointmentId = appointmentId,
            NewStartsAt = appointment.ScheduledStartsAt,
            NewEndsAt = appointment.ScheduledEndsAt
        });
    }

    private static async Task<IResult> GetAppointmentAsync(
        Guid appointmentId,
        ClaimsPrincipal user,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var appointment = await teleconsultationService.GetAppointmentAsync(appointmentId, userId.Value);
        return Results.Ok(appointment);
    }

    private static async Task<IResult> GetDoctorScheduleAsync(
        ClaimsPrincipal user,
        DateTimeOffset? from,
        DateTimeOffset? to,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var schedule = await teleconsultationService.GetDoctorScheduleAsync(userId.Value, from, to);
        return Results.Ok(schedule);
    }

    private static async Task<IResult> ConfirmAppointmentAsync(
        Guid appointmentId,
        ClaimsPrincipal user,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        await teleconsultationService.ConfirmAppointmentAsync(appointmentId, userId.Value);
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
        ClaimsPrincipal user,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        await teleconsultationService.RejectAppointmentAsync(appointmentId, userId.Value, request.Reason);
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
        int? daysAhead,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var appointments = await teleconsultationService.GetDoctorUpcomingAppointmentsAsync(userId.Value, daysAhead ?? 7);
        var appointmentsList = appointments.ToList();
        return Results.Ok(new { Items = appointmentsList, TotalCount = appointmentsList.Count });
    }

    private static async Task<IResult> GetDoctorAvailabilitySlotsAsync(
        ClaimsPrincipal user,
        DateTimeOffset? from,
        DateTimeOffset? to,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var slots = await teleconsultationService.GetDoctorAvailabilitySlotsAsync(userId.Value, from, to);
        return Results.Ok(slots);
    }

    private static async Task<IResult> SetAvailabilitySlotsAsync(
        SetAvailabilityRequestDto request,
        ClaimsPrincipal user,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        await teleconsultationService.SetDoctorAvailabilitySlotsAsync(userId.Value, request);
        return Results.Ok(new { Message = "Availability slots set" });
    }

    private static async Task<IResult> RemoveAvailabilitySlotAsync(
        Guid slotId,
        ClaimsPrincipal user,
        ITeleconsultationService teleconsultationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        await teleconsultationService.RemoveDoctorAvailabilitySlotAsync(userId.Value, slotId);
        return Results.Ok(new { Message = "Availability slot removed" });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("sub") ?? user.FindFirst("user_id");
        return Guid.TryParse(userIdClaim?.Value, out var userId) ? userId : null;
    }
}