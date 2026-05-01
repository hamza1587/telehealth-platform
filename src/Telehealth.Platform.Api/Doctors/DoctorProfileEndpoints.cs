using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Doctors;

namespace Telehealth.Platform.Api.Doctors;

/// <summary>
/// Doctor profile and availability management API endpoints.
/// </summary>
public static class DoctorProfileEndpoints
{
    public static IEndpointRouteBuilder MapDoctorProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var profile = app.MapGroup("/doctors/profile").WithTags("Doctor Profile");

        // Profile management
        profile.MapGet("/me", GetMyProfileAsync).RequireAuthorization("RequireDoctor");
        profile.MapPut("/me", UpdateMyProfileAsync).RequireAuthorization("RequireDoctor");
        profile.MapGet("/{doctorProfileId}", GetDoctorProfileAsync);
        profile.MapGet("/{doctorProfileId}/public", GetPublicProfileAsync);

        // Specialties
        profile.MapGet("/me/specialties", GetMySpecialtiesAsync).RequireAuthorization("RequireDoctor");
        profile.MapPost("/me/specialties", AddSpecialtyAsync).RequireAuthorization("RequireDoctor");
        profile.MapPut("/me/specialties/{specialtyId}", UpdateSpecialtyAsync).RequireAuthorization("RequireDoctor");
        profile.MapDelete("/me/specialties/{specialtyId}", RemoveSpecialtyAsync).RequireAuthorization("RequireDoctor");
        profile.MapPost("/me/specialties/{specialtyId}/set-primary", SetPrimarySpecialtyAsync).RequireAuthorization("RequireDoctor");

        // Workplaces
        profile.MapGet("/me/workplaces", GetMyWorkplacesAsync).RequireAuthorization("RequireDoctor");
        profile.MapPost("/me/workplaces", AddWorkplaceAsync).RequireAuthorization("RequireDoctor");
        profile.MapPut("/me/workplaces/{workplaceId}", UpdateWorkplaceAsync).RequireAuthorization("RequireDoctor");
        profile.MapDelete("/me/workplaces/{workplaceId}", RemoveWorkplaceAsync).RequireAuthorization("RequireDoctor");

        // Education
        profile.MapGet("/me/education", GetMyEducationAsync).RequireAuthorization("RequireDoctor");
        profile.MapPost("/me/education", AddEducationAsync).RequireAuthorization("RequireDoctor");
        profile.MapPut("/me/education/{educationId}", UpdateEducationAsync).RequireAuthorization("RequireDoctor");
        profile.MapDelete("/me/education/{educationId}", RemoveEducationAsync).RequireAuthorization("RequireDoctor");

        // Pricing
        profile.MapGet("/me/pricing", GetMyPricingAsync).RequireAuthorization("RequireDoctor");
        profile.MapPut("/me/pricing", UpdatePricingAsync).RequireAuthorization("RequireDoctor");

        // Availability
        profile.MapGet("/me/availability", GetMyAvailabilityAsync).RequireAuthorization("RequireDoctor");
        profile.MapPost("/me/availability", AddAvailabilityAsync).RequireAuthorization("RequireDoctor");
        profile.MapDelete("/me/availability/{availabilityId}", RemoveAvailabilityAsync).RequireAuthorization("RequireDoctor");
        profile.MapGet("/me/availability/exceptions", GetScheduleExceptionsAsync).RequireAuthorization("RequireDoctor");
        profile.MapPost("/me/availability/exceptions", AddScheduleExceptionAsync).RequireAuthorization("RequireDoctor");
        profile.MapDelete("/me/availability/exceptions/{exceptionId}", RemoveScheduleExceptionAsync).RequireAuthorization("RequireDoctor");

        // Language preferences
        profile.MapGet("/me/languages", GetMyLanguagesAsync).RequireAuthorization("RequireDoctor");
        profile.MapPost("/me/languages", AddLanguageAsync).RequireAuthorization("RequireDoctor");
        profile.MapDelete("/me/languages/{languageCode}", RemoveLanguageAsync).RequireAuthorization("RequireDoctor");

        return app;
    }

    private static async Task<IResult> GetMyProfileAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var profile = new DoctorProfileDetailDto(
            Guid.NewGuid(),
            "Dr. Jane Smith",
            "jane.smith@example.com",
            "United States",
            "Cardiology",
            "Board-certified cardiologist with 15 years of experience in interventional cardiology.",
            15,
            DoctorVerificationStatus.Verified,
            DoctorMarketplaceStatus.Active,
            new MoneyDto(0.50m, "EUR"),
            new List<SpecialtyDto>
            {
                new(Guid.NewGuid(), "Cardiology", "Cardiology", true),
                new(Guid.NewGuid(), "Internal Medicine", "Internal Medicine", false)
            },
            new List<LanguageDto>
            {
                new("en", "English", true),
                new("es", "Spanish", false)
            },
            DateTimeOffset.UtcNow.AddYears(-2));

        return Results.Ok(profile);
    }

    private static async Task<IResult> UpdateMyProfileAsync(
        UpdateProfileRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Profile updated successfully" });
    }

    private static async Task<IResult> GetDoctorProfileAsync(
        Guid doctorProfileId)
    {
        var profile = new DoctorProfileDetailDto(
            doctorProfileId,
            "Dr. Jane Smith",
            "jane.smith@example.com",
            "United States",
            "Cardiology",
            "Board-certified cardiologist with 15 years of experience.",
            15,
            DoctorVerificationStatus.Verified,
            DoctorMarketplaceStatus.Active,
            new MoneyDto(0.50m, "EUR"),
            new List<SpecialtyDto>
            {
                new(Guid.NewGuid(), "Cardiology", "Cardiology", true)
            },
            new List<LanguageDto>
            {
                new("en", "English", true)
            },
            DateTimeOffset.UtcNow.AddYears(-2));

        return Results.Ok(profile);
    }

    private static async Task<IResult> GetPublicProfileAsync(
        Guid doctorProfileId)
    {
        var profile = new DoctorPublicProfileDto(
            doctorProfileId,
            "Dr. Jane Smith",
            "United States",
            "Cardiology",
            "Board-certified cardiologist with 15 years of experience.",
            15,
            DoctorVerificationStatus.Verified,
            new MoneyDto(0.50m, "EUR"),
            new List<string> { "Cardiology", "Internal Medicine" },
            new List<string> { "English", "Spanish" },
            4.8,
            127,
            DateTimeOffset.UtcNow.AddYears(-2));

        return Results.Ok(profile);
    }

    private static async Task<IResult> GetMySpecialtiesAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var specialties = new List<SpecialtyDto>
        {
            new(Guid.NewGuid(), "Cardiology", "Cardiology", true),
            new(Guid.NewGuid(), "Internal Medicine", "Internal Medicine", false)
        };

        return Results.Ok(specialties);
    }

    private static async Task<IResult> AddSpecialtyAsync(
        AddSpecialtyRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var specialty = new SpecialtyDto(Guid.NewGuid(), request.SpecialtyCode, request.SpecialtyName, request.IsPrimary);
        return Results.Ok(specialty);
    }

    private static async Task<IResult> UpdateSpecialtyAsync(
        Guid specialtyId,
        UpdateSpecialtyRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Specialty updated" });
    }

    private static async Task<IResult> RemoveSpecialtyAsync(
        Guid specialtyId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Specialty removed" });
    }

    private static async Task<IResult> SetPrimarySpecialtyAsync(
        Guid specialtyId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Primary specialty set" });
    }

    private static async Task<IResult> GetMyWorkplacesAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var workplaces = new List<WorkplaceDto>
        {
            new(Guid.NewGuid(), "Central Hospital", "Cardiology Department", "123 Main St", "New York", "United States", true, true),
            new(Guid.NewGuid(), "Heart Care Center", null, "456 Park Ave", "New York", "United States", false, true)
        };

        return Results.Ok(workplaces);
    }

    private static async Task<IResult> AddWorkplaceAsync(
        AddWorkplaceRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var workplace = new WorkplaceDto(Guid.NewGuid(), request.Name, request.Department, request.Address, request.City, request.Country, request.IsPrimary, true);
        return Results.Ok(workplace);
    }

    private static async Task<IResult> UpdateWorkplaceAsync(
        Guid workplaceId,
        UpdateWorkplaceRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Workplace updated" });
    }

    private static async Task<IResult> RemoveWorkplaceAsync(
        Guid workplaceId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Workplace removed" });
    }

    private static async Task<IResult> GetMyEducationAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var education = new List<EducationDto>
        {
            new(Guid.NewGuid(), "Harvard Medical School", "MD", "Medicine", new DateOnly(2000, 9, 1), new DateOnly(2004, 6, 1), true),
            new(Guid.NewGuid(), "Johns Hopkins University", "Residency", "Internal Medicine", new DateOnly(2004, 7, 1), new DateOnly(2007, 6, 30), true)
        };

        return Results.Ok(education);
    }

    private static async Task<IResult> AddEducationAsync(
        AddEducationRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var education = new EducationDto(Guid.NewGuid(), request.Institution, request.Degree, request.FieldOfStudy, request.StartDate, request.EndDate, false);
        return Results.Ok(education);
    }

    private static async Task<IResult> UpdateEducationAsync(
        Guid educationId,
        UpdateEducationRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Education updated" });
    }

    private static async Task<IResult> RemoveEducationAsync(
        Guid educationId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Education removed" });
    }

    private static async Task<IResult> GetMyPricingAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var pricing = new PricingDto(
            Guid.NewGuid(),
            new MoneyDto(0.50m, "EUR"),
            new MoneyDto(0.10m, "EUR"),
            new MoneyDto(0.05m, "EUR"),
            new MoneyDto(0.02m, "EUR"),
            true,
            new MoneyDto(0.20m, "EUR"));

        return Results.Ok(pricing);
    }

    private static async Task<IResult> UpdatePricingAsync(
        UpdatePricingRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Pricing updated successfully" });
    }

    private static async Task<IResult> GetMyAvailabilityAsync(
        ClaimsPrincipal user,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var availability = new List<AvailabilityWindowDto>
        {
            new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(9), DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(17), "Video", true),
            new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(2).Date.AddHours(9), DateTimeOffset.UtcNow.AddDays(2).Date.AddHours(17), "Video", true),
            new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(3).Date.AddHours(9), DateTimeOffset.UtcNow.AddDays(3).Date.AddHours(12), "Phone", false)
        };

        return Results.Ok(availability);
    }

    private static async Task<IResult> AddAvailabilityAsync(
        AddAvailabilityRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var availability = new AvailabilityWindowDto(Guid.NewGuid(), request.StartsAt, request.EndsAt, request.ConsultationMode, request.IsInstantEnabled);
        return Results.Ok(availability);
    }

    private static async Task<IResult> RemoveAvailabilityAsync(
        Guid availabilityId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Availability removed" });
    }

    private static async Task<IResult> GetScheduleExceptionsAsync(
        ClaimsPrincipal user,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var exceptions = new List<ScheduleExceptionDto>
        {
            new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7), DateTimeOffset.UtcNow.AddDays(7).AddHours(24), "Conference", true)
        };

        return Results.Ok(exceptions);
    }

    private static async Task<IResult> AddScheduleExceptionAsync(
        AddScheduleExceptionRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var exception = new ScheduleExceptionDto(Guid.NewGuid(), request.StartsAt, request.EndsAt, request.Reason, request.IsAllDay);
        return Results.Ok(exception);
    }

    private static async Task<IResult> RemoveScheduleExceptionAsync(
        Guid exceptionId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Schedule exception removed" });
    }

    private static async Task<IResult> GetMyLanguagesAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var languages = new List<LanguageDto>
        {
            new("en", "English", true),
            new("es", "Spanish", false),
            new("fr", "French", false)
        };

        return Results.Ok(languages);
    }

    private static async Task<IResult> AddLanguageAsync(
        AddLanguageRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var language = new LanguageDto(request.LanguageCode, request.LanguageName, request.IsPrimary);
        return Results.Ok(language);
    }

    private static async Task<IResult> RemoveLanguageAsync(
        string languageCode,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Language removed" });
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
public record DoctorProfileDetailDto(
    Guid Id,
    string DisplayName,
    string Email,
    string CountryCode,
    string PrimarySpecialty,
    string Biography,
    int YearsOfExperience,
    DoctorVerificationStatus VerificationStatus,
    DoctorMarketplaceStatus MarketplaceStatus,
    MoneyDto PricePerSecond,
    List<SpecialtyDto> Specialties,
    List<LanguageDto> Languages,
    DateTimeOffset MemberSince);

public record DoctorPublicProfileDto(
    Guid Id,
    string DisplayName,
    string CountryCode,
    string PrimarySpecialty,
    string Biography,
    int YearsOfExperience,
    DoctorVerificationStatus VerificationStatus,
    MoneyDto PricePerSecond,
    List<string> Specialties,
    List<string> Languages,
    double Rating,
    int ConsultationsCompleted,
    DateTimeOffset MemberSince);

public record MoneyDto(decimal Amount, string Currency);

public record SpecialtyDto(
    Guid Id,
    string SpecialtyCode,
    string SpecialtyName,
    bool IsPrimary);

public record LanguageDto(
    string LanguageCode,
    string LanguageName,
    bool IsPrimary);

public record WorkplaceDto(
    Guid Id,
    string Name,
    string? Department,
    string? Address,
    string? City,
    string? Country,
    bool IsPrimary,
    bool IsActive);

public record EducationDto(
    Guid Id,
    string Institution,
    string Degree,
    string FieldOfStudy,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsVerified);

public record PricingDto(
    Guid Id,
    MoneyDto PricePerSecond,
    MoneyDto? VideoCallSurcharge,
    MoneyDto? PhoneCallSurcharge,
    MoneyDto? ChatSurcharge,
    bool IsInstantConsultationEnabled,
    MoneyDto? InstantConsultationPremium);

public record AvailabilityWindowDto(
    Guid Id,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string ConsultationMode,
    bool IsInstantEnabled);

public record ScheduleExceptionDto(
    Guid Id,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Reason,
    bool IsAllDay);

public record UpdateProfileRequestDto(
    string DisplayName,
    string Biography,
    int YearsOfExperience);

public record AddSpecialtyRequestDto(
    string SpecialtyCode,
    string SpecialtyName,
    bool IsPrimary,
    string? CertificationNumber,
    DateOnly? CertifiedDate,
    DateOnly? ExpiryDate);

public record UpdateSpecialtyRequestDto(
    string SpecialtyCode,
    string SpecialtyName,
    string? CertificationNumber,
    DateOnly? ExpiryDate);

public record AddWorkplaceRequestDto(
    string Name,
    string? Department,
    string? Address,
    string? City,
    string? Country,
    bool IsPrimary);

public record UpdateWorkplaceRequestDto(
    string Name,
    string? Department,
    string? Address,
    string? City,
    string? Country,
    bool IsPrimary);

public record AddEducationRequestDto(
    string Institution,
    string Degree,
    string FieldOfStudy,
    DateOnly? StartDate,
    DateOnly? EndDate);

public record UpdateEducationRequestDto(
    string Institution,
    string Degree,
    string FieldOfStudy,
    DateOnly? StartDate,
    DateOnly? EndDate);

public record UpdatePricingRequestDto(
    MoneyDto PricePerSecond,
    MoneyDto? VideoCallSurcharge,
    MoneyDto? PhoneCallSurcharge,
    MoneyDto? ChatSurcharge,
    bool IsInstantConsultationEnabled,
    MoneyDto? InstantConsultationPremium);

public record AddAvailabilityRequestDto(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string ConsultationMode,
    bool IsInstantEnabled);

public record AddScheduleExceptionRequestDto(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Reason,
    bool IsAllDay);

public record AddLanguageRequestDto(
    string LanguageCode,
    string LanguageName,
    bool IsPrimary);
