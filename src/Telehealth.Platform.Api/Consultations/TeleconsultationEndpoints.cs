using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Telehealth.Platform.Application.Abstractions.Consultations;
using Telehealth.Platform.Domain.Consultations;

namespace Telehealth.Platform.Api.Consultations;

public static class TeleconsultationEndpoints
{
    public static RouteGroupBuilder MapTeleconsultationEndpoints(this RouteGroupBuilder builder)
    {
        var teleconsultation = builder.MapGroup("/teleconsultation").WithTags("Teleconsultation");

        teleconsultation.MapPost("/sessions", StartSession)
            .RequireAuthorization("RequireDoctor")
            .WithName("StartSession")
            .WithDescription("Start a new consultation session");

        teleconsultation.MapGet("/sessions/{sessionId}", GetSession)
            .RequireAuthorization()
            .WithName("GetSession")
            .WithDescription("Get a consultation session by ID");

        teleconsultation.MapGet("/sessions/booking/{bookingId}", GetSessionByBooking)
            .RequireAuthorization()
            .WithName("GetSessionByBooking")
            .WithDescription("Get a consultation session by booking ID");

        teleconsultation.MapPost("/sessions/{sessionId}/events", RecordParticipantEvent)
            .RequireAuthorization()
            .WithName("RecordParticipantEvent")
            .WithDescription("Record a participant event");

        teleconsultation.MapPost("/sessions/{sessionId}/end", EndSession)
            .RequireAuthorization("RequireDoctor")
            .WithName("EndSession")
            .WithDescription("End a consultation session");

        teleconsultation.MapGet("/sessions/active", GetActiveSessions)
            .RequireAuthorization()
            .WithName("GetActiveSessions")
            .WithDescription("Get all active consultation sessions");

        return builder;
    }

    private static async Task<IResult> StartSession(
        StartSessionRequest request,
        ITeleconsultationService teleconsultationService,
        CancellationToken cancellationToken)
    {
        var session = await teleconsultationService.StartSessionAsync(
            request.BookingId,
            request.VideoProvider,
            request.VideoRoomId,
            cancellationToken);

        return Results.Ok(session);
    }

    private static async Task<IResult> GetSession(
        Guid sessionId,
        ITeleconsultationService teleconsultationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await teleconsultationService.GetSessionAsync(sessionId, cancellationToken);
            return Results.Ok(session);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GetSessionByBooking(
        Guid bookingId,
        ITeleconsultationService teleconsultationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await teleconsultationService.GetSessionByBookingAsync(bookingId, cancellationToken);
            return Results.Ok(session);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> RecordParticipantEvent(
        Guid sessionId,
        ParticipantEventRequest request,
        ITeleconsultationService teleconsultationService,
        CancellationToken cancellationToken)
    {
        var participantEvent = await teleconsultationService.RecordParticipantEventAsync(
            sessionId,
            request.EventType,
            request.ParticipantType,
            request.ParticipantId,
            request.Metadata,
            cancellationToken);

        return Results.Ok(participantEvent);
    }

    private static async Task<IResult> EndSession(
        Guid sessionId,
        EndSessionRequest request,
        ITeleconsultationService teleconsultationService,
        CancellationToken cancellationToken)
    {
        try
        {
            await teleconsultationService.EndSessionAsync(sessionId, request.BillableSeconds, cancellationToken);
            return Results.Ok();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GetActiveSessions(
        ITeleconsultationService teleconsultationService,
        CancellationToken cancellationToken)
    {
        var sessions = await teleconsultationService.GetActiveSessionsAsync(cancellationToken);
        return Results.Ok(sessions);
    }
}

public record StartSessionRequest(Guid BookingId, string VideoProvider, string VideoRoomId);

public record ParticipantEventRequest(
    ParticipantEventType EventType,
    string? ParticipantType,
    string? ParticipantId,
    Dictionary<string, object>? Metadata);

public record EndSessionRequest(long BillableSeconds);