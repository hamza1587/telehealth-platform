using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Telehealth.Platform.Api.Admin;

/// <summary>
/// Admin operations API endpoints.
/// </summary>
public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/admin").WithTags("Admin Operations");

        // User management
        admin.MapGet("/users", GetAllUsersAsync).RequireAuthorization("RequireAdmin");
        admin.MapGet("/users/{userId}", GetUserDetailsAsync).RequireAuthorization("RequireAdmin");
        admin.MapPost("/users/{userId}/suspend", SuspendUserAsync).RequireAuthorization("RequireAdmin");
        admin.MapPost("/users/{userId}/activate", ActivateUserAsync).RequireAuthorization("RequireAdmin");
        admin.MapPost("/users/{userId}/impersonate", ImpersonateUserAsync).RequireAuthorization("RequireAdmin");

        // Doctor management
        admin.MapGet("/doctors", GetAllDoctorsAsync).RequireAuthorization("RequireAdmin");
        admin.MapPost("/doctors/{doctorId}/verify", VerifyDoctorAsync).RequireAuthorization("RequireAdmin");
        admin.MapPost("/doctors/{doctorId}/suspend", SuspendDoctorAsync).RequireAuthorization("RequireAdmin");

        // System settings
        admin.MapGet("/settings", GetSystemSettingsAsync).RequireAuthorization("RequireAdmin");
        admin.MapPut("/settings/{key}", UpdateSettingAsync).RequireAuthorization("RequireAdmin");

        // Reports and analytics
        admin.MapGet("/reports/dashboard", GetDashboardStatsAsync).RequireAuthorization("RequireAdmin");
        admin.MapGet("/reports/consultations", GetConsultationReportAsync).RequireAuthorization("RequireAdmin");
        admin.MapGet("/reports/revenue", GetRevenueReportAsync).RequireAuthorization("RequireAdmin");

        // Audit log
        admin.MapGet("/audit-log", GetAuditLogAsync).RequireAuthorization("RequireAdmin");

        return app;
    }

    private static async Task<IResult> GetAllUsersAsync(
        string? search,
        string? type,
        string? status,
        int? page,
        int? pageSize)
    {
        var users = new List<AdminUserDto>
        {
            new(
                Guid.NewGuid(),
                "john.doe@example.com",
                "John Doe",
                "Patient",
                "Active",
                DateTimeOffset.UtcNow.AddYears(-1),
                DateTimeOffset.UtcNow.AddDays(-1))
        };

        return Results.Ok(new { Items = users, TotalCount = users.Count });
    }

    private static async Task<IResult> GetUserDetailsAsync(
        Guid userId)
    {
        var user = new AdminUserDetailDto(
            userId,
            "john.doe@example.com",
            "John Doe",
            "+1234567890",
            "United States",
            "Patient",
            "Active",
            true,
            true,
            true,
            DateTimeOffset.UtcNow.AddYears(-1),
            DateTimeOffset.UtcNow.AddDays(-1),
            null,
            15,
            3,
            new List<AdminUserAuditDto>
            {
                new("Login", DateTimeOffset.UtcNow.AddDays(-1), "Web", "Success")
            });

        return Results.Ok(user);
    }

    private static async Task<IResult> SuspendUserAsync(
        Guid userId,
        SuspendUserRequestDto request)
    {
        return Results.Ok(new
        {
            UserId = userId,
            Status = "Suspended",
            Reason = request.Reason,
            SuspendedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> ActivateUserAsync(
        Guid userId)
    {
        return Results.Ok(new
        {
            UserId = userId,
            Status = "Active",
            ActivatedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> ImpersonateUserAsync(
        Guid userId,
        ClaimsPrincipal user)
    {
        return Results.Ok(new
        {
            UserId = userId,
            ImpersonationToken = "imp_token_12345",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Message = "Impersonation session started"
        });
    }

    private static async Task<IResult> GetAllDoctorsAsync(
        string? verificationStatus,
        string? search,
        int? page,
        int? pageSize)
    {
        var doctors = new List<AdminDoctorDto>
        {
            new(
                Guid.NewGuid(),
                "Dr. Jane Smith",
                "jane.smith@example.com",
                "Cardiology",
                "Verified",
                "Active",
                127,
                4.8,
                DateTimeOffset.UtcNow.AddYears(-2))
        };

        return Results.Ok(new { Items = doctors, TotalCount = doctors.Count });
    }

    private static async Task<IResult> VerifyDoctorAsync(
        Guid doctorId,
        VerifyDoctorRequestDto request)
    {
        return Results.Ok(new
        {
            DoctorId = doctorId,
            Status = "Verified",
            VerifiedBy = request.AdminId,
            Notes = request.Notes,
            VerifiedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> SuspendDoctorAsync(
        Guid doctorId,
        SuspendDoctorRequestDto request)
    {
        return Results.Ok(new
        {
            DoctorId = doctorId,
            Status = "Suspended",
            Reason = request.Reason,
            SuspendedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> GetSystemSettingsAsync(
        string? category)
    {
        var settings = new List<SystemSettingDto>
        {
            new(
                "consultation.max_duration",
                "3600",
                "int",
                "Maximum consultation duration in seconds",
                "Consultation",
                true),
            new(
                "payment.currency",
                "EUR",
                "string",
                "Default currency for payments",
                "Payment",
                true),
            new(
                "queue.max_wait_minutes",
                "15",
                "int",
                "Maximum wait time for instant queue",
                "Queue",
                true)
        };

        if (!string.IsNullOrEmpty(category))
        {
            settings = settings.Where(s => s.Category == category).ToList();
        }

        return Results.Ok(settings);
    }

    private static async Task<IResult> UpdateSettingAsync(
        string key,
        UpdateSettingRequestDto request)
    {
        return Results.Ok(new
        {
            Key = key,
            Value = request.Value,
            UpdatedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> GetDashboardStatsAsync(
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var stats = new DashboardStatsDto(
            1250,
            347,
            48,
            new MoneyDto(45850m, "EUR"),
            new MoneyDto(18340m, "EUR"),
            156,
            12,
            4.7);

        return Results.Ok(stats);
    }

    private static async Task<IResult> GetConsultationReportAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        string? groupBy)
    {
        var report = new ConsultationReportDto(
            1250,
            11800,
            450,
            180,
            new List<SpecialtyStatsDto>
            {
                new("Cardiology", 280, 2520),
                new("General Practice", 450, 3600),
                new("Mental Health", 180, 1440)
            });

        return Results.Ok(report);
    }

    private static async Task<IResult> GetRevenueReportAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        string? groupBy)
    {
        var report = new RevenueReportDto(
            45850m,
            9170m,
            36680m,
            27410m,
            new List<DailyRevenueDto>
            {
                new(DateTimeOffset.UtcNow.AddDays(-6), 6500m, 1300m),
                new(DateTimeOffset.UtcNow.AddDays(-5), 7200m, 1440m),
                new(DateTimeOffset.UtcNow.AddDays(-4), 6800m, 1360m),
                new(DateTimeOffset.UtcNow.AddDays(-3), 7500m, 1500m),
                new(DateTimeOffset.UtcNow.AddDays(-2), 8100m, 1620m),
                new(DateTimeOffset.UtcNow.AddDays(-1), 8850m, 1770m),
                new(DateTimeOffset.UtcNow, 900m, 180m)
            });

        return Results.Ok(report);
    }

    private static async Task<IResult> GetAuditLogAsync(
        string? operationType,
        Guid? adminId,
        string? targetType,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int? page,
        int? pageSize)
    {
        var entries = new List<AuditLogEntryDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Admin User",
                "SuspendUser",
                "User",
                Guid.NewGuid().ToString(),
                "Violation of terms",
                true,
                DateTimeOffset.UtcNow.AddHours(-2))
        };

        return Results.Ok(new { Items = entries, TotalCount = entries.Count });
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
public record MoneyDto(decimal Amount, string Currency);

public record AdminUserDto(
    Guid Id,
    string Email,
    string Name,
    string UserType,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastLoginAt);

public record AdminUserDetailDto(
    Guid Id,
    string Email,
    string Name,
    string? Phone,
    string Country,
    string UserType,
    string Status,
    bool EmailVerified,
    bool PhoneVerified,
    bool TwoFactorEnabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastLoginAt,
    DateTimeOffset? LastPasswordChangeAt,
    int TotalConsultations,
    int TotalTickets,
    List<AdminUserAuditDto> RecentActivity);

public record AdminUserAuditDto(
    string Action,
    DateTimeOffset Timestamp,
    string? IpAddress,
    string Status);

public record SuspendUserRequestDto(string Reason, int? SuspensionDays);

public record AdminDoctorDto(
    Guid Id,
    string DisplayName,
    string Email,
    string PrimarySpecialty,
    string VerificationStatus,
    string MarketplaceStatus,
    int ConsultationsCompleted,
    double Rating,
    DateTimeOffset JoinedAt);

public record VerifyDoctorRequestDto(Guid AdminId, string Notes);

public record SuspendDoctorRequestDto(string Reason);

public record SystemSettingDto(
    string Key,
    string Value,
    string ValueType,
    string? Description,
    string Category,
    bool IsActive);

public record UpdateSettingRequestDto(string Value, string? Reason);

public record DashboardStatsDto(
    int TotalUsers,
    int TotalConsultations,
    int ActiveDoctors,
    MoneyDto TotalRevenue,
    MoneyDto TotalPayouts,
    int OpenTickets,
    int PendingVerifications,
    double AverageRating);

public record ConsultationReportDto(
    int TotalConsultations,
    int TotalMinutes,
    int UniquePatients,
    int UniqueDoctors,
    List<SpecialtyStatsDto> BySpecialty);

public record SpecialtyStatsDto(
    string Specialty,
    int ConsultationCount,
    int TotalMinutes);

public record RevenueReportDto(
    decimal TotalRevenue,
    decimal TotalPlatformFees,
    decimal TotalPayouts,
    decimal NetRevenue,
    List<DailyRevenueDto> DailyBreakdown);

public record DailyRevenueDto(
    DateTimeOffset Date,
    decimal Revenue,
    decimal PlatformFees);

public record AuditLogEntryDto(
    Guid Id,
    Guid AdminId,
    string AdminName,
    string OperationType,
    string TargetType,
    string TargetId,
    string? Reason,
    bool Success,
    DateTimeOffset Timestamp);
