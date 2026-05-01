using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Clinical;

namespace Telehealth.Platform.Api.Clinical;

/// <summary>
/// Clinical notes API endpoints.
/// </summary>
public static class ClinicalNotesEndpoints
{
    public static IEndpointRouteBuilder MapClinicalNotesEndpoints(this IEndpointRouteBuilder app)
    {
        var notes = app.MapGroup("/clinical-notes").WithTags("Clinical Notes");

        // Doctor endpoints
        notes.MapGet("/consultation/{consultationId}", GetNotesByConsultationAsync).RequireAuthorization("RequireDoctor");
        notes.MapPost("/consultation/{consultationId}", CreateNoteAsync).RequireAuthorization("RequireDoctor");
        notes.MapPut("/{noteId}", UpdateNoteAsync).RequireAuthorization("RequireDoctor");
        notes.MapPost("/{noteId}/finalize", FinalizeNoteAsync).RequireAuthorization("RequireDoctor");
        notes.MapPost("/{noteId}/lock", LockNoteAsync).RequireAuthorization("RequireDoctor");

        // Patient endpoints (view only their own notes)
        notes.MapGet("/my-notes", GetMyNotesAsync).RequireAuthorization("RequirePatient");
        notes.MapGet("/{noteId}/view", GetNoteAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetNotesByConsultationAsync(
        Guid consultationId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var notes = new List<ClinicalNoteDto>
        {
            new(
                Guid.NewGuid(),
                consultationId,
                "ConsultationSummary",
                "Cardiology Consultation Summary",
                "Patient presented with chest pain...",
                false,
                false,
                DateTimeOffset.UtcNow.AddHours(-1),
                DateTimeOffset.UtcNow.AddHours(-1))
        };

        return Results.Ok(notes);
    }

    private static async Task<IResult> CreateNoteAsync(
        Guid consultationId,
        CreateNoteRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var note = new ClinicalNoteDto(
            Guid.NewGuid(),
            consultationId,
            request.NoteType,
            request.Title,
            request.Content,
            true,
            false,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        return Results.Created($"/clinical-notes/{note.Id}", note);
    }

    private static async Task<IResult> UpdateNoteAsync(
        Guid noteId,
        UpdateNoteRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Note updated", NoteId = noteId });
    }

    private static async Task<IResult> FinalizeNoteAsync(
        Guid noteId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Note finalized", NoteId = noteId, FinalizedAt = DateTimeOffset.UtcNow });
    }

    private static async Task<IResult> LockNoteAsync(
        Guid noteId,
        LockNoteRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Note locked", NoteId = noteId, LockedAt = DateTimeOffset.UtcNow });
    }

    private static async Task<IResult> GetMyNotesAsync(
        ClaimsPrincipal user,
        string? type,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var notes = new List<PatientNoteSummaryDto>
        {
            new(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddDays(-5),
                "Dr. Jane Smith",
                "Cardiology",
                "Consultation Summary",
                false)
        };

        return Results.Ok(new { Items = notes, TotalCount = notes.Count });
    }

    private static async Task<IResult> GetNoteAsync(
        Guid noteId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var note = new ClinicalNoteDetailDto(
            noteId,
            Guid.NewGuid(),
            "ConsultationSummary",
            "Cardiology Consultation Summary",
            "Patient presented with mild chest pain... [full content]",
            "Dr. Jane Smith",
            "Cardiology",
            false,
            false,
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-5),
            null);

        return Results.Ok(note);
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
public record ClinicalNoteDto(
    Guid Id,
    Guid ConsultationId,
    string NoteType,
    string Title,
    string Content,
    bool IsDraft,
    bool IsLocked,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record ClinicalNoteDetailDto(
    Guid Id,
    Guid ConsultationId,
    string NoteType,
    string Title,
    string Content,
    string DoctorName,
    string Specialty,
    bool IsDraft,
    bool IsLocked,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LockedAt);

public record PatientNoteSummaryDto(
    Guid Id,
    DateTimeOffset Date,
    string DoctorName,
    string Specialty,
    string NoteType,
    bool IsDraft);

public record CreateNoteRequestDto(
    string NoteType,
    string Title,
    string Content);

public record UpdateNoteRequestDto(
    string Title,
    string Content);

public record LockNoteRequestDto(string Reason);
