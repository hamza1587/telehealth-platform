using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Compliance;

namespace Telehealth.Platform.Api.Compliance;

/// <summary>
/// GDPR data rights API endpoints.
/// </summary>
public static class GdprEndpoints
{
    public static IEndpointRouteBuilder MapGdprEndpoints(this IEndpointRouteBuilder app)
    {
        var gdpr = app.MapGroup("/gdpr").WithTags("GDPR Data Rights");

        // User data export
        gdpr.MapPost("/export-request", RequestDataExportAsync).RequireAuthorization();
        gdpr.MapGet("/export-requests", GetExportRequestsAsync).RequireAuthorization();
        gdpr.MapGet("/export/{requestId}/download", DownloadExportAsync).RequireAuthorization();

        // Data deletion
        gdpr.MapPost("/delete-request", RequestDataDeletionAsync).RequireAuthorization();
        gdpr.MapGet("/delete-requests", GetDeletionRequestsAsync).RequireAuthorization();

        // Data access
        gdpr.MapGet("/my-data-summary", GetMyDataSummaryAsync).RequireAuthorization();

        // Admin endpoints
        gdpr.MapGet("/admin/export-requests", GetAllExportRequestsAsync).RequireAuthorization("RequireComplianceOfficer");
        gdpr.MapGet("/admin/delete-requests", GetAllDeletionRequestsAsync).RequireAuthorization("RequireComplianceOfficer");
        gdpr.MapPost("/admin/export/{requestId}/process", ProcessExportRequestAsync).RequireAuthorization("RequireComplianceOfficer");
        gdpr.MapPost("/admin/delete/{requestId}/process", ProcessDeletionRequestAsync).RequireAuthorization("RequireComplianceOfficer");

        return app;
    }

    private static async Task<IResult> RequestDataExportAsync(
        DataExportRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var exportRequest = new GdprExportRequestDto(
            Guid.NewGuid(),
            request.Format,
            GdprExportStatus.Pending,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(24),
            request.DataCategories);

        return Results.Accepted($"/gdpr/export/{exportRequest.Id}", new
        {
            Message = "Data export request submitted",
            Request = exportRequest
        });
    }

    private static async Task<IResult> GetExportRequestsAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var requests = new List<GdprExportRequestDto>
        {
            new(
                Guid.NewGuid(),
                "JSON",
                GdprExportStatus.Ready,
                DateTimeOffset.UtcNow.AddDays(-5),
                DateTimeOffset.UtcNow.AddDays(-4),
                new List<string> { "Profile", "Consultations", "Billing" }),
            new(
                Guid.NewGuid(),
                "PDF",
                GdprExportStatus.Expired,
                DateTimeOffset.UtcNow.AddDays(-40),
                DateTimeOffset.UtcNow.AddDays(-39),
                new List<string> { "Profile", "Consultations" })
        };

        return Results.Ok(new { Items = requests, TotalCount = requests.Count });
    }

    private static async Task<IResult> DownloadExportAsync(
        Guid requestId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            DownloadUrl = $"https://api.example.com/gdpr/exports/{requestId}.zip",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
            FileSize = 1548576
        });
    }

    private static async Task<IResult> RequestDataDeletionAsync(
        DataDeletionRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            RequestId = Guid.NewGuid(),
            Message = "Data deletion request submitted",
            Status = "PendingVerification",
            VerificationRequired = true,
            VerificationSentTo = request.Email
        });
    }

    private static async Task<IResult> GetDeletionRequestsAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var requests = new List<GdprDeletionRequestDto>
        {
            new(
                Guid.NewGuid(),
                GdprRequestStatus.Completed,
                DateTimeOffset.UtcNow.AddDays(-30),
                DateTimeOffset.UtcNow.AddDays(-28),
                "User request",
                new List<string> { "Profile", "Consultations", "Messages" })
        };

        return Results.Ok(new { Items = requests, TotalCount = requests.Count });
    }

    private static async Task<IResult> GetMyDataSummaryAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var summary = new DataSummaryDto(
            DateTimeOffset.UtcNow.AddYears(-1),
            15,
            3,
            245,
            new List<DataCategoryDto>
            {
                new("Profile", "Personal information and preferences", 12, DateTimeOffset.UtcNow.AddDays(-1)),
                new("Consultations", "Medical consultations and notes", 15, DateTimeOffset.UtcNow.AddDays(-2)),
                new("Billing", "Payment and billing records", 8, DateTimeOffset.UtcNow.AddDays(-5)),
                new("Messages", "Chat and communication history", 245, DateTimeOffset.UtcNow.AddDays(-1)),
                new("DeviceInfo", "Device and login information", 12, DateTimeOffset.UtcNow.AddDays(-1))
            });

        return Results.Ok(summary);
    }

    private static async Task<IResult> GetAllExportRequestsAsync(
        string? status,
        int? page,
        int? pageSize)
    {
        var requests = new List<AdminGdprExportDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "john.doe@example.com",
                "JSON",
                GdprExportStatus.Pending,
                DateTimeOffset.UtcNow.AddHours(-2),
                DateTimeOffset.UtcNow.AddHours(22),
                null,
                null)
        };

        return Results.Ok(new { Items = requests, TotalCount = requests.Count });
    }

    private static async Task<IResult> GetAllDeletionRequestsAsync(
        string? status,
        int? page,
        int? pageSize)
    {
        var requests = new List<AdminGdprDeletionDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "jane.smith@example.com",
                GdprRequestStatus.Pending,
                DateTimeOffset.UtcNow.AddHours(-4),
                null,
                "User request",
                new List<string> { "Profile", "Consultations", "Billing" })
        };

        return Results.Ok(new { Items = requests, TotalCount = requests.Count });
    }

    private static async Task<IResult> ProcessExportRequestAsync(
        Guid requestId,
        ProcessExportRequestDto request)
    {
        return Results.Ok(new
        {
            RequestId = requestId,
            Status = "Processing",
            EstimatedCompletion = DateTimeOffset.UtcNow.AddHours(4),
            ProcessedBy = request.AdminId
        });
    }

    private static async Task<IResult> ProcessDeletionRequestAsync(
        Guid requestId,
        ProcessDeletionRequestDto request)
    {
        return Results.Ok(new
        {
            RequestId = requestId,
            Status = "Completed",
            DeletedCategories = request.CategoriesToDelete,
            ProcessedBy = request.AdminId,
            CompletedAt = DateTimeOffset.UtcNow
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
public record DataExportRequestDto(
    string Format,
    List<string> DataCategories,
    string? Email);

public record GdprExportRequestDto(
    Guid Id,
    string Format,
    GdprExportStatus Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    List<string> DataCategories);

public record DataDeletionRequestDto(
    string Email,
    string Reason,
    List<string> CategoriesToDelete,
    bool ConfirmPermanentDeletion);

public record GdprDeletionRequestDto(
    Guid Id,
    GdprRequestStatus Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    string Reason,
    List<string> CategoriesDeleted);

public enum GdprRequestStatus
{
    Pending,
    PendingVerification,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public record DataSummaryDto(
    DateTimeOffset MemberSince,
    int ConsultationsCount,
    int PrescriptionsCount,
    int MessagesCount,
    List<DataCategoryDto> Categories);

public record DataCategoryDto(
    string Name,
    string Description,
    int RecordsCount,
    DateTimeOffset LastUpdated);

public record AdminGdprExportDto(
    Guid Id,
    Guid UserId,
    string Email,
    string Format,
    GdprExportStatus Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    string? DownloadUrl,
    long? FileSize);

public record AdminGdprDeletionDto(
    Guid Id,
    Guid UserId,
    string Email,
    GdprRequestStatus Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    string Reason,
    List<string> CategoriesToDelete);

public record ProcessExportRequestDto(Guid AdminId);

public record ProcessDeletionRequestDto(
    Guid AdminId,
    List<string> CategoriesToDelete,
    bool ConfirmAnonymize);
