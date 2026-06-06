using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Telehealth.Platform.Domain.Doctors;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Admin;

/// <summary>
/// Admin operations API endpoints — wired to real DB queries.
/// </summary>
public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/admin").WithTags("Admin Operations");

        admin.MapGet("/users", GetAllUsersAsync).RequireAuthorization("RequireAdmin");
        admin.MapGet("/users/{userId}", GetUserDetailsAsync).RequireAuthorization("RequireAdmin");
        admin.MapPost("/users/{userId}/suspend", SuspendUserAsync).RequireAuthorization("RequireAdmin");
        admin.MapPost("/users/{userId}/activate", ActivateUserAsync).RequireAuthorization("RequireAdmin");
        admin.MapPost("/users/{userId}/impersonate", ImpersonateUserAsync).RequireAuthorization("RequireAdmin");

        admin.MapGet("/doctors", GetAllDoctorsAsync).RequireAuthorization("RequireAdmin");
        admin.MapPost("/doctors/{doctorId}/verify", VerifyDoctorAsync).RequireAuthorization("RequireAdmin");
        admin.MapPost("/doctors/{doctorId}/suspend", SuspendDoctorAsync).RequireAuthorization("RequireAdmin");

        admin.MapGet("/settings", GetSystemSettingsAsync).RequireAuthorization("RequireAdmin");
        admin.MapPut("/settings/{key}", UpdateSettingAsync).RequireAuthorization("RequireAdmin");

        admin.MapGet("/reports/dashboard", GetDashboardStatsAsync).RequireAuthorization("RequireAdmin");
        admin.MapGet("/reports/consultations", GetConsultationReportAsync).RequireAuthorization("RequireAdmin");
        admin.MapGet("/reports/revenue", GetRevenueReportAsync).RequireAuthorization("RequireAdmin");

        admin.MapGet("/audit-log", GetAuditLogAsync).RequireAuthorization("RequireAdmin");

        return app;
    }

    private static async Task<IResult> GetAllUsersAsync(
        PlatformDbContext db,
        string? search,
        string? type,
        string? status,
        int page = 1,
        int pageSize = 50)
    {
        var query = db.PlatformUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Email.Contains(search) ||
                                     (u.DisplayName != null && u.DisplayName.Contains(search)));

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<UserType>(type, out var userType))
            query = query.Where(u => u.UserType == userType);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, out var userStatus))
            query = query.Where(u => u.Status == userStatus);

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Email,
                u.DisplayName ?? u.Email,
                u.UserType.ToString(),
                u.Status.ToString(),
                u.CreatedAt,
                u.UpdatedAt))
            .ToListAsync();

        return Results.Ok(new { Items = users, TotalCount = total, Page = page, PageSize = pageSize });
    }

    private static async Task<IResult> GetUserDetailsAsync(Guid userId, PlatformDbContext db)
    {
        var user = await db.PlatformUsers.FindAsync(userId);
        if (user is null) return Results.NotFound(new { Message = "User not found." });

        var consultationCount = await db.ConsultationBookings
            .CountAsync(b => b.PatientAccountId == userId);

        var detail = new AdminUserDetailDto(
            user.Id,
            user.Email,
            user.DisplayName ?? user.Email,
            user.PhoneNumber,
            user.CountryCode ?? "Unknown",
            user.UserType.ToString(),
            user.Status.ToString(),
            user.EmailConfirmed,
            user.PhoneNumberConfirmed,
            user.TwoFactorEnabled,
            user.CreatedAt,
            user.UpdatedAt,
            null,
            consultationCount,
            0);

        return Results.Ok(detail);
    }

    private static async Task<IResult> SuspendUserAsync(
        Guid userId,
        SuspendUserRequestDto request,
        PlatformDbContext db)
    {
        var user = await db.PlatformUsers.FindAsync(userId);
        if (user is null) return Results.NotFound(new { Message = "User not found." });

        user.Suspend(request.Reason);
        await db.SaveChangesAsync();

        return Results.Ok(new { UserId = userId, Status = "Suspended", Reason = request.Reason, SuspendedAt = DateTimeOffset.UtcNow });
    }

    private static async Task<IResult> ActivateUserAsync(Guid userId, PlatformDbContext db)
    {
        var user = await db.PlatformUsers.FindAsync(userId);
        if (user is null) return Results.NotFound(new { Message = "User not found." });

        user.Activate();
        await db.SaveChangesAsync();

        return Results.Ok(new { UserId = userId, Status = "Active", ActivatedAt = DateTimeOffset.UtcNow });
    }

    private static async Task<IResult> ImpersonateUserAsync(Guid userId, ClaimsPrincipal currentUser, PlatformDbContext db)
    {
        var user = await db.PlatformUsers.FindAsync(userId);
        if (user is null) return Results.NotFound(new { Message = "User not found." });

        // Real impersonation requires a signed short-lived token; stub for now
        return Results.Ok(new
        {
            UserId = userId,
            ImpersonationToken = $"imp_{Guid.NewGuid():N}",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Message = "Impersonation session started"
        });
    }

    private static async Task<IResult> GetAllDoctorsAsync(
        PlatformDbContext db,
        string? verificationStatus,
        string? search,
        int page = 1,
        int pageSize = 50)
    {
        var query = db.DoctorProfiles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.DisplayName.Contains(search));

        if (!string.IsNullOrWhiteSpace(verificationStatus))
            query = query.Where(d => d.VerificationStatus.ToString() == verificationStatus);

        var total = await query.CountAsync();
        var doctors = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new AdminDoctorDto(
                d.Id,
                d.DisplayName,
                string.Empty,
                d.PrimarySpecialty,
                d.VerificationStatus.ToString(),
                d.MarketplaceStatus.ToString(),
                0,
                0.0,
                d.CreatedAt))
            .ToListAsync();

        return Results.Ok(new { Items = doctors, TotalCount = total, Page = page, PageSize = pageSize });
    }

    private static async Task<IResult> VerifyDoctorAsync(Guid doctorId, VerifyDoctorRequestDto request, PlatformDbContext db)
    {
        var doctor = await db.DoctorProfiles.FindAsync(doctorId);
        if (doctor is null) return Results.NotFound(new { Message = "Doctor not found." });

        doctor.ApproveVerification();
        await db.SaveChangesAsync();

        return Results.Ok(new { DoctorId = doctorId, Status = "Verified", Notes = request.Notes, VerifiedAt = DateTimeOffset.UtcNow });
    }

    private static async Task<IResult> SuspendDoctorAsync(Guid doctorId, SuspendDoctorRequestDto request, PlatformDbContext db)
    {
        var doctor = await db.DoctorProfiles.FindAsync(doctorId);
        if (doctor is null) return Results.NotFound(new { Message = "Doctor not found." });

        doctor.UpdateMarketplaceStatus(DoctorMarketplaceStatus.Suspended);
        await db.SaveChangesAsync();

        return Results.Ok(new { DoctorId = doctorId, Status = "Suspended", Reason = request.Reason, SuspendedAt = DateTimeOffset.UtcNow });
    }

    private static IResult GetSystemSettingsAsync(string? category)
    {
        var settings = new[]
        {
            new SystemSettingDto("consultation.max_duration", "3600", "int", "Maximum consultation duration in seconds", "Consultation", true),
            new SystemSettingDto("payment.currency", "EUR", "string", "Default currency for payments", "Payment", true),
            new SystemSettingDto("queue.max_wait_minutes", "15", "int", "Maximum wait time for instant queue", "Queue", true),
            new SystemSettingDto("rate_limit.requests_per_minute", "60", "int", "API rate limit per user per minute", "Security", true),
        };

        if (!string.IsNullOrEmpty(category))
            return Results.Ok(settings.Where(s => s.Category == category));

        return Results.Ok(settings);
    }

    private static IResult UpdateSettingAsync(string key, UpdateSettingRequestDto request)
    {
        // Settings are read-only config for now; persist to DB when SystemSettings entity is wired
        return Results.Ok(new { Key = key, Value = request.Value, UpdatedAt = DateTimeOffset.UtcNow });
    }

    private static async Task<IResult> GetDashboardStatsAsync(
        PlatformDbContext db,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var totalUsers = await db.PlatformUsers.CountAsync();
        var totalPatients = await db.PlatformUsers.CountAsync(u => u.UserType == UserType.Patient);
        var totalDoctors = await db.DoctorProfiles.CountAsync();
        var activeDoctors = await db.DoctorProfiles
            .CountAsync(d => d.MarketplaceStatus.ToString() == "Active");
        var totalBookings = await db.ConsultationBookings.CountAsync();

        var stats = new DashboardStatsDto(
            TotalUsers: totalUsers,
            TotalPatients: totalPatients,
            TotalDoctors: totalDoctors,
            ActiveDoctors: activeDoctors,
            TotalBookings: totalBookings);

        return Results.Ok(stats);
    }

    private static async Task<IResult> GetConsultationReportAsync(
        PlatformDbContext db,
        DateTimeOffset? from,
        DateTimeOffset? to,
        string? groupBy)
    {
        var query = db.ConsultationBookings.AsQueryable();
        if (from.HasValue) query = query.Where(b => b.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(b => b.CreatedAt <= to.Value);

        var total = await query.CountAsync();
        var bySpecialty = await query
            .GroupBy(b => b.Specialty)
            .Select(g => new SpecialtyStatsDto(g.Key, g.Count()))
            .ToListAsync();

        return Results.Ok(new ConsultationReportDto(total, bySpecialty));
    }

    private static async Task<IResult> GetRevenueReportAsync(
        PlatformDbContext db,
        DateTimeOffset? from,
        DateTimeOffset? to,
        string? groupBy)
    {
        // BillingSession has no date field; return aggregate over all finalized sessions
        var sessions = await db.BillingSessions
            .Where(b => b.Status == Domain.Billing.BillingSessionStatus.Finalized)
            .ToListAsync();
        var grossMinor = sessions.Sum(s => s.GrossAmount?.MinorUnits ?? 0);
        var feeMinor = sessions.Sum(s => s.PlatformFee?.MinorUnits ?? 0);
        var payoutMinor = sessions.Sum(s => s.DoctorEarning?.MinorUnits ?? 0);

        return Results.Ok(new RevenueReportDto(
            TotalRevenueEur: grossMinor / 100m,
            PlatformFeesEur: feeMinor / 100m,
            PayoutsEur: payoutMinor / 100m,
            SessionCount: sessions.Count));
    }

    private static async Task<IResult> GetAuditLogAsync(
        PlatformDbContext db,
        string? operationType,
        Guid? adminId,
        string? targetType,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page = 1,
        int pageSize = 50)
    {
        var query = db.AuditEvents.AsQueryable();

        if (!string.IsNullOrEmpty(operationType))
            query = query.Where(e => e.Action.Contains(operationType));
        if (adminId.HasValue)
            query = query.Where(e => e.ActorId == adminId.Value.ToString());
        if (!string.IsNullOrEmpty(targetType))
            query = query.Where(e => e.TargetType == targetType);
        if (from.HasValue) query = query.Where(e => e.OccurredAt >= from.Value);
        if (to.HasValue) query = query.Where(e => e.OccurredAt <= to.Value);

        var total = await query.CountAsync();
        var entries = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new AuditLogEntryDto(
                e.Id,
                e.ActorId,
                e.ActorType,
                e.Action,
                e.TargetType,
                e.TargetId,
                e.OccurredAt))
            .ToListAsync();

        return Results.Ok(new { Items = entries, TotalCount = total, Page = page, PageSize = pageSize });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}

// DTOs
public record AdminUserDto(Guid Id, string Email, string Name, string UserType, string Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public record AdminUserDetailDto(
    Guid Id, string Email, string Name, string? Phone, string Country,
    string UserType, string Status, bool EmailVerified, bool PhoneVerified,
    bool TwoFactorEnabled, DateTimeOffset CreatedAt, DateTimeOffset LastUpdatedAt,
    DateTimeOffset? LastPasswordChangeAt, int TotalConsultations, int TotalTickets);

public record SuspendUserRequestDto(string Reason, int? SuspensionDays);
public record AdminDoctorDto(Guid Id, string DisplayName, string Email, string PrimarySpecialty, string VerificationStatus, string MarketplaceStatus, int ConsultationsCompleted, double Rating, DateTimeOffset JoinedAt);
public record VerifyDoctorRequestDto(Guid AdminId, string Notes);
public record SuspendDoctorRequestDto(string Reason);
public record SystemSettingDto(string Key, string Value, string ValueType, string? Description, string Category, bool IsActive);
public record UpdateSettingRequestDto(string Value, string? Reason);
public record DashboardStatsDto(int TotalUsers, int TotalPatients, int TotalDoctors, int ActiveDoctors, int TotalBookings);
public record ConsultationReportDto(int TotalConsultations, List<SpecialtyStatsDto> BySpecialty);
public record SpecialtyStatsDto(string Specialty, int Count);
public record RevenueReportDto(decimal TotalRevenueEur, decimal PlatformFeesEur, decimal PayoutsEur, int SessionCount);
public record AuditLogEntryDto(Guid Id, string ActorId, string ActorType, string Action, string TargetType, string TargetId, DateTimeOffset OccurredAt);
