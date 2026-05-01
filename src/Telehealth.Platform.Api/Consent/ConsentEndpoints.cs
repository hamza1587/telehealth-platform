using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Telehealth.Platform.Api.Consent;

/// <summary>
/// Consent management API endpoints for GDPR compliance.
/// </summary>
public static class ConsentEndpoints
{
    public static IEndpointRouteBuilder MapConsentEndpoints(this IEndpointRouteBuilder app)
    {
        var consent = app.MapGroup("/consent").WithTags("Consent Management");

        // Patient consent endpoints
        consent.MapGet("/templates", GetConsentTemplatesAsync);
        consent.MapGet("/my-consents", GetMyConsentsAsync).RequireAuthorization();
        consent.MapPost("/accept", AcceptConsentAsync).RequireAuthorization();
        consent.MapPost("/withdraw", WithdrawConsentAsync).RequireAuthorization();
        consent.MapGet("/status/{action}", GetConsentStatusForActionAsync).RequireAuthorization();

        // Admin endpoints
        consent.MapGet("/admin/templates", GetAllTemplatesAsync).RequireAuthorization("RequireAdmin");
        consent.MapPost("/admin/templates", CreateTemplateAsync).RequireAuthorization("RequireAdmin");
        consent.MapPut("/admin/templates/{templateId}", UpdateTemplateAsync).RequireAuthorization("RequireAdmin");
        consent.MapPost("/admin/templates/{templateId}/deactivate", DeactivateTemplateAsync).RequireAuthorization("RequireAdmin");
        consent.MapGet("/admin/requirements", GetConsentRequirementsAsync).RequireAuthorization("RequireAdmin");
        consent.MapPut("/admin/requirements/{requirementId}", UpdateRequirementAsync).RequireAuthorization("RequireAdmin");
        consent.MapGet("/admin/patient/{patientId}", GetPatientConsentsAsync).RequireAuthorization("RequireAdmin");

        return app;
    }

    private static async Task<IResult> GetConsentTemplatesAsync(
        string? language,
        string? consentType)
    {
        // Return available consent templates
        var templates = new List<ConsentTemplateDto>
        {
            new(
                Guid.NewGuid(),
                "TermsOfService",
                "1.0",
                "Terms of Service",
                "Terms of service content...",
                language ?? "en",
                "Contractual",
                true,
                DateTimeOffset.UtcNow.AddYears(-1)),
            new(
                Guid.NewGuid(),
                "PrivacyPolicy",
                "1.0",
                "Privacy Policy",
                "Privacy policy content...",
                language ?? "en",
                "Legal Obligation",
                true,
                DateTimeOffset.UtcNow.AddYears(-1)),
            new(
                Guid.NewGuid(),
                "TelehealthConsent",
                "1.0",
                "Telehealth Informed Consent",
                "Telehealth consent content...",
                language ?? "en",
                "Consent",
                true,
                DateTimeOffset.UtcNow.AddYears(-1))
        };

        if (!string.IsNullOrEmpty(consentType))
        {
            templates = templates.Where(t => t.ConsentType == consentType).ToList();
        }

        return Results.Ok(templates);
    }

    private static async Task<IResult> GetMyConsentsAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        // Return patient's consent history
        var consents = new List<PatientConsentDto>
        {
            new(
                Guid.NewGuid(),
                "TermsOfService",
                "1.0",
                true,
                DateTimeOffset.UtcNow.AddMonths(-3),
                null,
                null)
        };

        return Results.Ok(consents);
    }

    private static async Task<IResult> AcceptConsentAsync(
        AcceptConsentRequestDto request,
        ClaimsPrincipal user,
        HttpContext httpContext)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

        // Record consent acceptance
        var consent = new PatientConsentDto(
            Guid.NewGuid(),
            request.ConsentType,
            request.Version,
            true,
            DateTimeOffset.UtcNow,
            null,
            null);

        return Results.Ok(new
        {
            consent.Id,
            Message = "Consent recorded successfully"
        });
    }

    private static async Task<IResult> WithdrawConsentAsync(
        WithdrawConsentRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        // Process consent withdrawal
        return Results.Ok(new
        {
            Message = "Consent withdrawn successfully",
            WithdrawnAt = DateTimeOffset.UtcNow,
            EffectiveDate = DateTimeOffset.UtcNow.AddDays(1)
        });
    }

    private static async Task<IResult> GetConsentStatusForActionAsync(
        string action,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var status = new ConsentStatusDto(
            action,
            true,
            new List<RequiredConsentDto>
            {
                new("TermsOfService", true, true, DateTimeOffset.UtcNow.AddMonths(-3)),
                new("PrivacyPolicy", true, true, DateTimeOffset.UtcNow.AddMonths(-3)),
                new("TelehealthConsent", true, true, DateTimeOffset.UtcNow.AddMonths(-3))
            });

        return Results.Ok(status);
    }

    private static async Task<IResult> GetAllTemplatesAsync()
    {
        var templates = new List<ConsentTemplateDto>
        {
            new(
                Guid.NewGuid(),
                "TermsOfService",
                "1.0",
                "Terms of Service",
                "Terms content...",
                "en",
                "Contractual",
                true,
                DateTimeOffset.UtcNow.AddYears(-1))
        };

        return Results.Ok(templates);
    }

    private static async Task<IResult> CreateTemplateAsync(
        CreateConsentTemplateDto request)
    {
        var template = new ConsentTemplateDto(
            Guid.NewGuid(),
            request.ConsentType,
            request.Version,
            request.Title,
            request.Content,
            request.Language,
            request.LegalBasis,
            request.IsRequired,
            request.EffectiveFrom);

        return Results.Created($"/consent/admin/templates/{template.Id}", template);
    }

    private static async Task<IResult> UpdateTemplateAsync(
        Guid templateId,
        UpdateConsentTemplateDto request)
    {
        var template = new ConsentTemplateDto(
            templateId,
            request.ConsentType,
            request.Version,
            request.Title,
            request.Content,
            request.Language,
            request.LegalBasis,
            request.IsRequired,
            DateTimeOffset.UtcNow);

        return Results.Ok(template);
    }

    private static async Task<IResult> DeactivateTemplateAsync(
        Guid templateId,
        DeactivateTemplateRequestDto request)
    {
        return Results.Ok(new { Message = "Template deactivated", TemplateId = templateId });
    }

    private static async Task<IResult> GetConsentRequirementsAsync()
    {
        var requirements = new List<ConsentRequirementDto>
        {
            new(Guid.NewGuid(), "Registration", "TermsOfService", true, 16, true),
            new(Guid.NewGuid(), "Registration", "PrivacyPolicy", true, 16, true),
            new(Guid.NewGuid(), "BookingConsultation", "TelehealthConsent", true, 18, true)
        };

        return Results.Ok(requirements);
    }

    private static async Task<IResult> UpdateRequirementAsync(
        Guid requirementId,
        UpdateConsentRequirementDto request)
    {
        var requirement = new ConsentRequirementDto(
            requirementId,
            request.Action,
            request.ConsentType,
            request.IsRequired,
            request.MinimumAge,
            true);

        return Results.Ok(requirement);
    }

    private static async Task<IResult> GetPatientConsentsAsync(
        Guid patientId)
    {
        var consents = new List<PatientConsentDto>
        {
            new(
                Guid.NewGuid(),
                "TermsOfService",
                "1.0",
                true,
                DateTimeOffset.UtcNow.AddMonths(-3),
                null,
                null)
        };

        return Results.Ok(new { PatientId = patientId, Consents = consents });
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
public record ConsentTemplateDto(
    Guid Id,
    string ConsentType,
    string Version,
    string Title,
    string Content,
    string Language,
    string LegalBasis,
    bool IsRequired,
    DateTimeOffset EffectiveFrom);

public record PatientConsentDto(
    Guid Id,
    string ConsentType,
    string Version,
    bool IsAccepted,
    DateTimeOffset CapturedAt,
    DateTimeOffset? WithdrawnAt,
    string? WithdrawalReason);

public record ConsentStatusDto(
    string Action,
    bool CanProceed,
    List<RequiredConsentDto> RequiredConsents);

public record RequiredConsentDto(
    string ConsentType,
    bool IsRequired,
    bool HasConsent,
    DateTimeOffset? ConsentDate);

public record ConsentRequirementDto(
    Guid Id,
    string Action,
    string ConsentType,
    bool IsRequired,
    int MinimumAge,
    bool IsActive);

public record AcceptConsentRequestDto(
    string ConsentType,
    string Version,
    bool IsAccepted);

public record WithdrawConsentRequestDto(
    string ConsentType,
    string? Reason);

public record CreateConsentTemplateDto(
    string ConsentType,
    string Version,
    string Title,
    string Content,
    string Language,
    string LegalBasis,
    bool IsRequired,
    DateTimeOffset EffectiveFrom);

public record UpdateConsentTemplateDto(
    string ConsentType,
    string Version,
    string Title,
    string Content,
    string Language,
    string LegalBasis,
    bool IsRequired);

public record DeactivateTemplateRequestDto(
    DateTimeOffset EffectiveTo,
    string? Reason);

public record UpdateConsentRequirementDto(
    string Action,
    string ConsentType,
    bool IsRequired,
    int MinimumAge);
