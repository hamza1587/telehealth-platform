using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Research;

namespace Telehealth.Platform.Api.Research;

/// <summary>
/// Research export and anonymization API endpoints.
/// </summary>
public static class ResearchExportEndpoints
{
    public static IEndpointRouteBuilder MapResearchExportEndpoints(this IEndpointRouteBuilder app)
    {
        var research = app.MapGroup("/research").WithTags("Research Export");

        // Researcher endpoints
        research.MapPost("/export-requests", RequestExportAsync).RequireAuthorization("RequireResearchReviewer");
        research.MapGet("/my-exports", GetMyExportsAsync).RequireAuthorization("RequireResearchReviewer");
        research.MapGet("/export/{exportId}", GetExportAsync).RequireAuthorization("RequireResearchReviewer");
        research.MapGet("/export/{exportId}/download", DownloadExportAsync).RequireAuthorization("RequireResearchReviewer");

        // Admin/Compliance endpoints
        research.MapGet("/admin/export-requests", GetAllExportRequestsAsync).RequireAuthorization("RequireComplianceOfficer");
        research.MapPost("/admin/export/{exportId}/approve", ApproveExportAsync).RequireAuthorization("RequireComplianceOfficer");
        research.MapPost("/admin/export/{exportId}/reject", RejectExportAsync).RequireAuthorization("RequireComplianceOfficer");
        research.MapPost("/admin/export/{exportId}/generate", GenerateExportAsync).RequireAuthorization("RequireComplianceOfficer");

        // Available data types
        research.MapGet("/data-types", GetAvailableDataTypesAsync).RequireAuthorization("RequireResearchReviewer");

        return app;
    }

    private static async Task<IResult> RequestExportAsync(
        ResearchExportRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var export = new ResearchExportDto(
            Guid.NewGuid(),
            request.ResearchProject,
            request.Institution,
            request.EthicsApproval,
            request.DataType,
            request.AnonymizationLevel,
            request.Format,
            request.DateRangeFrom,
            request.DateRangeTo,
            ResearchExportStatus.PendingApproval,
            DateTimeOffset.UtcNow,
            null,
            null);

        return Results.Created($"/research/export/{export.Id}", new
        {
            Message = "Research export request submitted for approval",
            Export = export
        });
    }

    private static async Task<IResult> GetMyExportsAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var exports = new List<ResearchExportSummaryDto>
        {
            new(
                Guid.NewGuid(),
                "Telehealth Usage Patterns",
                "Consultations",
                "Anonymized",
                ResearchExportStatus.Approved,
                15420,
                DateTimeOffset.UtcNow.AddDays(-5))
        };

        return Results.Ok(new { Items = exports, TotalCount = exports.Count });
    }

    private static async Task<IResult> GetExportAsync(
        Guid exportId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var export = new ResearchExportDetailDto(
            exportId,
            "Telehealth Usage Patterns",
            "University Medical Center",
            "IRB-2024-001",
            "Consultations",
            "Anonymized",
            "CSV",
            DateTimeOffset.UtcNow.AddYears(-1),
            DateTimeOffset.UtcNow,
            ResearchExportStatus.Ready,
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-2),
            "Compliance Officer",
            15420,
            2548576,
            DateTimeOffset.UtcNow.AddDays(88));

        return Results.Ok(export);
    }

    private static async Task<IResult> DownloadExportAsync(
        Guid exportId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            ExportId = exportId,
            DownloadUrl = $"https://api.example.com/research/exports/{exportId}.zip",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            FileSize = 2548576,
            RecordCount = 15420
        });
    }

    private static async Task<IResult> GetAllExportRequestsAsync(
        string? status,
        int? page,
        int? pageSize)
    {
        var exports = new List<AdminResearchExportDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Dr. Researcher",
                "Telehealth Usage Patterns",
                "University Medical Center",
                "IRB-2024-001",
                "Consultations",
                "Anonymized",
                ResearchExportStatus.PendingApproval,
                DateTimeOffset.UtcNow.AddHours(-4),
                null,
                null)
        };

        return Results.Ok(new { Items = exports, TotalCount = exports.Count });
    }

    private static async Task<IResult> ApproveExportAsync(
        Guid exportId,
        ApproveExportRequestDto request)
    {
        return Results.Ok(new
        {
            ExportId = exportId,
            Status = ResearchExportStatus.Approved,
            ApprovedBy = request.ApprovedBy,
            ApprovedAt = DateTimeOffset.UtcNow,
            Conditions = request.Conditions
        });
    }

    private static async Task<IResult> RejectExportAsync(
        Guid exportId,
        RejectExportRequestDto request)
    {
        return Results.Ok(new
        {
            ExportId = exportId,
            Status = ResearchExportStatus.Rejected,
            Reason = request.Reason,
            RejectedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> GenerateExportAsync(
        Guid exportId,
        GenerateExportRequestDto request)
    {
        return Results.Ok(new
        {
            ExportId = exportId,
            Status = ResearchExportStatus.Processing,
            EstimatedCompletion = DateTimeOffset.UtcNow.AddHours(2),
            StartedBy = request.AdminId
        });
    }

    private static async Task<IResult> GetAvailableDataTypesAsync()
    {
        var dataTypes = new List<DataTypeInfoDto>
        {
            new(
                "Consultations",
                "Consultation metadata (anonymized)",
                new List<string> { "Duration", "Specialty", "Mode", "Outcome" },
                new List<string> { "Anonymized", "Aggregated" }),
            new(
                "Demographics",
                "Aggregated user demographics",
                new List<string> { "Age Group", "Country", "Language" },
                new List<string> { "Aggregated", "DifferentialPrivacy" }),
            new(
                "PlatformUsage",
                "Platform usage statistics",
                new List<string> { "Feature Usage", "Time of Day", "Device Type" },
                new List<string> { "Aggregated", "Anonymized" })
        };

        return Results.Ok(dataTypes);
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
public record ResearchExportRequestDto(
    string ResearchProject,
    string? Institution,
    string? EthicsApproval,
    string DataType,
    string AnonymizationLevel,
    string Format,
    DateTimeOffset? DateRangeFrom,
    DateTimeOffset? DateRangeTo,
    string? Filters);

public record ResearchExportDto(
    Guid Id,
    string ResearchProject,
    string? Institution,
    string? EthicsApproval,
    string DataType,
    string AnonymizationLevel,
    string Format,
    DateTimeOffset? DateRangeFrom,
    DateTimeOffset? DateRangeTo,
    ResearchExportStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset? CompletedAt);

public record ResearchExportSummaryDto(
    Guid Id,
    string ResearchProject,
    string DataType,
    string AnonymizationLevel,
    ResearchExportStatus Status,
    long? RecordCount,
    DateTimeOffset? CompletedAt);

public record ResearchExportDetailDto(
    Guid Id,
    string ResearchProject,
    string? Institution,
    string? EthicsApproval,
    string DataType,
    string AnonymizationLevel,
    string Format,
    DateTimeOffset? DateRangeFrom,
    DateTimeOffset? DateRangeTo,
    ResearchExportStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset? CompletedAt,
    string? ApprovedBy,
    long? RecordCount,
    long? FileSize,
    DateTimeOffset? ExpiresAt);

public record AdminResearchExportDto(
    Guid Id,
    Guid RequestedBy,
    string RequesterName,
    string ResearchProject,
    string? Institution,
    string? EthicsApproval,
    string DataType,
    string AnonymizationLevel,
    ResearchExportStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ApprovedAt,
    string? ApprovedBy);

public record ApproveExportRequestDto(
    string ApprovedBy,
    string? Conditions,
    DateTimeOffset? DataExpiryDate);

public record RejectExportRequestDto(string Reason);

public record GenerateExportRequestDto(
    string AdminId,
    bool ConfirmAnonymization,
    string? DataFilter);

public record DataTypeInfoDto(
    string Name,
    string Description,
    List<string> AvailableFields,
    List<string> SupportedAnonymizationLevels);
