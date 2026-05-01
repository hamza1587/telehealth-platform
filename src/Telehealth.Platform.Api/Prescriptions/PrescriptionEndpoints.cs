using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Prescriptions;

namespace Telehealth.Platform.Api.Prescriptions;

/// <summary>
/// Prescription management API endpoints.
/// </summary>
public static class PrescriptionEndpoints
{
    public static IEndpointRouteBuilder MapPrescriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var prescriptions = app.MapGroup("/prescriptions").WithTags("Prescriptions");

        // Doctor endpoints
        prescriptions.MapGet("/consultation/{consultationId}", GetPrescriptionsByConsultationAsync).RequireAuthorization("RequireDoctor");
        prescriptions.MapPost("/consultation/{consultationId}", CreatePrescriptionAsync).RequireAuthorization("RequireDoctor");
        prescriptions.MapPut("/{prescriptionId}", UpdatePrescriptionAsync).RequireAuthorization("RequireDoctor");
        prescriptions.MapPost("/{prescriptionId}/issue", IssuePrescriptionAsync).RequireAuthorization("RequireDoctor");
        prescriptions.MapPost("/{prescriptionId}/cancel", CancelPrescriptionAsync).RequireAuthorization("RequireDoctor");
        prescriptions.MapPost("/{prescriptionId}/items", AddPrescriptionItemAsync).RequireAuthorization("RequireDoctor");
        prescriptions.MapPut("/{prescriptionId}/items/{itemId}", UpdatePrescriptionItemAsync).RequireAuthorization("RequireDoctor");

        // Patient endpoints
        prescriptions.MapGet("/my-prescriptions", GetMyPrescriptionsAsync).RequireAuthorization("RequirePatient");
        prescriptions.MapGet("/{prescriptionId}", GetPrescriptionAsync).RequireAuthorization();
        prescriptions.MapGet("/{prescriptionId}/download", DownloadPrescriptionAsync).RequireAuthorization("RequirePatient");

        return app;
    }

    private static async Task<IResult> GetPrescriptionsByConsultationAsync(
        Guid consultationId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var prescriptions = new List<PrescriptionSummaryDto>
        {
            new(
                Guid.NewGuid(),
                consultationId,
                "Medication",
                3,
                PrescriptionStatus.Issued,
                DateTimeOffset.UtcNow.AddHours(-1),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)))
        };

        return Results.Ok(prescriptions);
    }

    private static async Task<IResult> CreatePrescriptionAsync(
        Guid consultationId,
        CreatePrescriptionRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var prescription = new PrescriptionDto(
            Guid.NewGuid(),
            consultationId,
            request.PrescriptionType,
            request.Notes,
            request.DurationDays,
            request.StartDate,
            request.EndDate,
            request.RefillsAllowed,
            PrescriptionStatus.Draft,
            new List<PrescriptionItemDto>(),
            DateTimeOffset.UtcNow);

        return Results.Created($"/prescriptions/{prescription.Id}", prescription);
    }

    private static async Task<IResult> UpdatePrescriptionAsync(
        Guid prescriptionId,
        UpdatePrescriptionRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Prescription updated", PrescriptionId = prescriptionId });
    }

    private static async Task<IResult> IssuePrescriptionAsync(
        Guid prescriptionId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Prescription issued",
            PrescriptionId = prescriptionId,
            IssuedAt = DateTimeOffset.UtcNow,
            PrescriptionNumber = "RX-2024-001234"
        });
    }

    private static async Task<IResult> CancelPrescriptionAsync(
        Guid prescriptionId,
        CancelPrescriptionRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Prescription cancelled",
            PrescriptionId = prescriptionId,
            Reason = request.Reason,
            CancelledAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> AddPrescriptionItemAsync(
        Guid prescriptionId,
        AddPrescriptionItemRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var item = new PrescriptionItemDto(
            Guid.NewGuid(),
            request.MedicationCode,
            request.MedicationName,
            request.Dosage,
            request.Frequency,
            request.Route,
            request.Instructions,
            request.Quantity,
            request.Unit,
            request.DurationDays,
            request.IsAsNeeded,
            request.AsNeededReason,
            true);

        return Results.Ok(new { Message = "Item added", Item = item });
    }

    private static async Task<IResult> UpdatePrescriptionItemAsync(
        Guid prescriptionId,
        Guid itemId,
        UpdatePrescriptionItemRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Item updated", ItemId = itemId });
    }

    private static async Task<IResult> GetMyPrescriptionsAsync(
        ClaimsPrincipal user,
        string? status)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var prescriptions = new List<PatientPrescriptionDto>
        {
            new(
                Guid.NewGuid(),
                "RX-2024-001234",
                "Dr. Jane Smith",
                "Cardiology",
                2,
                PrescriptionStatus.Issued,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                new List<PrescriptionItemSummaryDto>
                {
                    new(Guid.NewGuid(), "Aspirin", "81mg", "Once daily", 30, "tablet")
                })
        };

        return Results.Ok(new { Items = prescriptions, TotalCount = prescriptions.Count });
    }

    private static async Task<IResult> GetPrescriptionAsync(
        Guid prescriptionId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var prescription = new PrescriptionDetailDto(
            prescriptionId,
            Guid.NewGuid(),
            "RX-2024-001234",
            "Dr. Jane Smith",
            "Cardiology",
            "Medication",
            "Take as directed",
            7,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            2,
            0,
            PrescriptionStatus.Issued,
            DateTimeOffset.UtcNow.AddDays(-2),
            new List<PrescriptionItemDto>
            {
                new(Guid.NewGuid(), "ASP001", "Aspirin", "81mg", "Once daily", "Oral", "Take with food", 30, "tablet", 7, false, null, true),
                new(Guid.NewGuid(), "MET002", "Metoprolol", "50mg", "Twice daily", "Oral", null, 60, "tablet", 7, false, null, true)
            });

        return Results.Ok(prescription);
    }

    private static async Task<IResult> DownloadPrescriptionAsync(
        Guid prescriptionId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            PrescriptionId = prescriptionId,
            DownloadUrl = $"https://api.example.com/prescriptions/{prescriptionId}/download.pdf",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
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
public record PrescriptionDto(
    Guid Id,
    Guid ConsultationId,
    string PrescriptionType,
    string? Notes,
    int? DurationDays,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int? RefillsAllowed,
    PrescriptionStatus Status,
    List<PrescriptionItemDto> Items,
    DateTimeOffset CreatedAt);

public record PrescriptionSummaryDto(
    Guid Id,
    Guid ConsultationId,
    string PrescriptionType,
    int ItemsCount,
    PrescriptionStatus Status,
    DateTimeOffset CreatedAt,
    DateOnly? ExpiresOn);

public record PrescriptionDetailDto(
    Guid Id,
    Guid ConsultationId,
    string PrescriptionNumber,
    string DoctorName,
    string Specialty,
    string PrescriptionType,
    string? Notes,
    int? DurationDays,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int? RefillsAllowed,
    int RefillsUsed,
    PrescriptionStatus Status,
    DateTimeOffset CreatedAt,
    List<PrescriptionItemDto> Items);

public record PrescriptionItemDto(
    Guid Id,
    string MedicationCode,
    string MedicationName,
    string Dosage,
    string Frequency,
    string Route,
    string? Instructions,
    int Quantity,
    string? Unit,
    int DurationDays,
    bool IsAsNeeded,
    string? AsNeededReason,
    bool IsSubstitutable);

public record PrescriptionItemSummaryDto(
    Guid Id,
    string MedicationName,
    string Dosage,
    string Frequency,
    int Quantity,
    string? Unit);

public record PatientPrescriptionDto(
    Guid Id,
    string PrescriptionNumber,
    string DoctorName,
    string Specialty,
    int ItemsCount,
    PrescriptionStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    List<PrescriptionItemSummaryDto> Items);

public record CreatePrescriptionRequestDto(
    string PrescriptionType,
    string? Notes,
    int? DurationDays,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int? RefillsAllowed);

public record UpdatePrescriptionRequestDto(
    string? Notes,
    int? DurationDays,
    DateOnly? EndDate,
    int? RefillsAllowed);

public record CancelPrescriptionRequestDto(string Reason);

public record AddPrescriptionItemRequestDto(
    string MedicationCode,
    string MedicationName,
    string Dosage,
    string Frequency,
    string Route,
    string? Instructions,
    int Quantity,
    string? Unit,
    int DurationDays,
    bool IsAsNeeded,
    string? AsNeededReason);

public record UpdatePrescriptionItemRequestDto(
    string Dosage,
    string Frequency,
    string? Instructions,
    int Quantity);
