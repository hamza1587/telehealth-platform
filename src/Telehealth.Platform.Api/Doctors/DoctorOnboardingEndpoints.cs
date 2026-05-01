using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Doctors;

namespace Telehealth.Platform.Api.Doctors;

/// <summary>
/// Doctor onboarding and verification API endpoints.
/// </summary>
public static class DoctorOnboardingEndpoints
{
    public static IEndpointRouteBuilder MapDoctorOnboardingEndpoints(this IEndpointRouteBuilder app)
    {
        var onboarding = app.MapGroup("/doctors/onboarding").WithTags("Doctor Onboarding");

        // Doctor endpoints
        onboarding.MapPost("/submit", SubmitOnboardingApplicationAsync).RequireAuthorization("RequireDoctor");
        onboarding.MapGet("/status", GetOnboardingStatusAsync).RequireAuthorization("RequireDoctor");
        onboarding.MapPut("/update", UpdateOnboardingApplicationAsync).RequireAuthorization("RequireDoctor");
        onboarding.MapPost("/documents", UploadDocumentAsync).RequireAuthorization("RequireDoctor");
        onboarding.MapGet("/documents", GetMyDocumentsAsync).RequireAuthorization("RequireDoctor");
        onboarding.MapDelete("/documents/{documentId}", DeleteDocumentAsync).RequireAuthorization("RequireDoctor");
        onboarding.MapGet("/requirements", GetOnboardingRequirementsAsync).RequireAuthorization("RequireDoctor");

        // Admin/Compliance endpoints
        onboarding.MapGet("/admin/pending", GetPendingApplicationsAsync).RequireAuthorization("RequireComplianceOfficer");
        onboarding.MapGet("/admin/{doctorProfileId}", GetApplicationDetailsAsync).RequireAuthorization("RequireComplianceOfficer");
        onboarding.MapPost("/admin/{doctorProfileId}/verify", VerifyApplicationAsync).RequireAuthorization("RequireComplianceOfficer");
        onboarding.MapPost("/admin/{doctorProfileId}/request-changes", RequestChangesAsync).RequireAuthorization("RequireComplianceOfficer");
        onboarding.MapPost("/admin/{doctorProfileId}/reject", RejectApplicationAsync).RequireAuthorization("RequireComplianceOfficer");
        onboarding.MapPost("/admin/documents/{documentId}/review", ReviewDocumentAsync).RequireAuthorization("RequireComplianceOfficer");
        onboarding.MapGet("/admin/verification-checks/{doctorProfileId}", GetVerificationChecksAsync).RequireAuthorization("RequireComplianceOfficer");
        onboarding.MapPost("/admin/verification-checks", AddVerificationCheckAsync).RequireAuthorization("RequireComplianceOfficer");
        onboarding.MapPut("/admin/verification-checks/{checkId}", UpdateVerificationCheckAsync).RequireAuthorization("RequireComplianceOfficer");

        return app;
    }

    private static async Task<IResult> SubmitOnboardingApplicationAsync(
        SubmitOnboardingRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var response = new OnboardingStatusDto(
            Guid.NewGuid(),
            DoctorVerificationStatus.PendingVerification,
            DateTimeOffset.UtcNow,
            null,
            new List<RequiredDocumentDto>
            {
                new("MedicalLicense", false),
                new("BoardCertification", false),
                new("IdentityDocument", false),
                new("MalpracticeInsurance", false)
            },
            new List<VerificationCheckDto>
            {
                new(Guid.NewGuid(), "IdentityVerification", "Pending", null, null, null)
            });

        return Results.Ok(new { Message = "Application submitted successfully", Status = response });
    }

    private static async Task<IResult> GetOnboardingStatusAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var status = new OnboardingStatusDto(
            Guid.NewGuid(),
            DoctorVerificationStatus.PendingVerification,
            DateTimeOffset.UtcNow.AddDays(-5),
            null,
            new List<RequiredDocumentDto>
            {
                new("MedicalLicense", true),
                new("BoardCertification", true),
                new("IdentityDocument", true),
                new("MalpracticeInsurance", false)
            },
            new List<VerificationCheckDto>
            {
                new(Guid.NewGuid(), "IdentityVerification", "Passed", null, null, DateTimeOffset.UtcNow.AddDays(-2)),
                new(Guid.NewGuid(), "LicenseVerification", "InProgress", null, null, null),
                new(Guid.NewGuid(), "BackgroundCheck", "Pending", null, null, null)
            });

        return Results.Ok(status);
    }

    private static async Task<IResult> UpdateOnboardingApplicationAsync(
        UpdateOnboardingRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Application updated successfully" });
    }

    private static async Task<IResult> UploadDocumentAsync(
        UploadDocumentRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var document = new DocumentDto(
            Guid.NewGuid(),
            request.DocumentType,
            request.FileName,
            $"/documents/{Guid.NewGuid()}/{request.FileName}",
            request.FileSize,
            "Pending",
            null,
            DateTimeOffset.UtcNow);

        return Results.Ok(new { Message = "Document uploaded successfully", Document = document });
    }

    private static async Task<IResult> GetMyDocumentsAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var documents = new List<DocumentDto>
        {
            new(Guid.NewGuid(), "MedicalLicense", "license.pdf", "/docs/license.pdf", 1024000, "Approved", null, DateTimeOffset.UtcNow.AddDays(-5)),
            new(Guid.NewGuid(), "IdentityDocument", "passport.pdf", "/docs/passport.pdf", 512000, "Approved", null, DateTimeOffset.UtcNow.AddDays(-5)),
            new(Guid.NewGuid(), "MalpracticeInsurance", "insurance.pdf", "/docs/insurance.pdf", 768000, "Pending", null, DateTimeOffset.UtcNow.AddDays(-1))
        };

        return Results.Ok(documents);
    }

    private static async Task<IResult> DeleteDocumentAsync(
        Guid documentId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { Message = "Document deleted", DocumentId = documentId });
    }

    private static async Task<IResult> GetOnboardingRequirementsAsync()
    {
        var requirements = new OnboardingRequirementsDto(
            new List<DocumentRequirementDto>
            {
                new("MedicalLicense", "Medical License", "Current medical license from your country of practice", true, new List<string> { ".pdf", ".jpg", ".png" }, 5242880),
                new("BoardCertification", "Board Certification", "Relevant board certification documents", true, new List<string> { ".pdf", ".jpg", ".png" }, 5242880),
                new("IdentityDocument", "Identity Document", "Government-issued photo ID", true, new List<string> { ".pdf", ".jpg", ".png" }, 5242880),
                new("MalpracticeInsurance", "Malpractice Insurance", "Current malpractice insurance certificate", true, new List<string> { ".pdf" }, 5242880),
                new("CurriculumVitae", "Curriculum Vitae", "Your professional CV", false, new List<string> { ".pdf", ".doc", ".docx" }, 5242880),
                new("ProfessionalPhoto", "Professional Photo", "Recent professional headshot", false, new List<string> { ".jpg", ".png" }, 2097152)
            },
            new List<VerificationRequirementDto>
            {
                new("IdentityVerification", "Identity Verification", true),
                new("LicenseVerification", "License Verification", true),
                new("BackgroundCheck", "Background Check", true),
                new("ReferenceCheck", "Reference Check", true),
                new("EducationVerification", "Education Verification", false),
                new("PlatformTraining", "Platform Training", true)
            });

        return Results.Ok(requirements);
    }

    private static async Task<IResult> GetPendingApplicationsAsync(
        int? page,
        int? pageSize)
    {
        var applications = new List<PendingApplicationDto>
        {
            new(
                Guid.NewGuid(),
                "Dr. Jane Smith",
                "jane.smith@example.com",
                "United States",
                "Cardiology",
                DoctorVerificationStatus.PendingVerification,
                DateTimeOffset.UtcNow.AddDays(-3),
                3)
        };

        return Results.Ok(new { Items = applications, TotalCount = 1 });
    }

    private static async Task<IResult> GetApplicationDetailsAsync(
        Guid doctorProfileId)
    {
        var application = new ApplicationDetailsDto(
            doctorProfileId,
            "Dr. Jane Smith",
            "jane.smith@example.com",
            "+1234567890",
            "United States",
            "MD123456",
            "American Medical Association",
            "MD, FACC",
            15,
            "Board-certified cardiologist with 15 years of experience...",
            "Medical Protective Company",
            "MP-123456-789",
            new DateOnly(2025, 12, 31),
            DoctorVerificationStatus.PendingVerification,
            DateTimeOffset.UtcNow.AddDays(-3),
            null,
            new List<DocumentDto>
            {
                new(Guid.NewGuid(), "MedicalLicense", "license.pdf", "/docs/license.pdf", 1024000, "Approved", null, DateTimeOffset.UtcNow.AddDays(-3)),
                new(Guid.NewGuid(), "IdentityDocument", "passport.pdf", "/docs/passport.pdf", 512000, "Approved", null, DateTimeOffset.UtcNow.AddDays(-3))
            },
            new List<VerificationCheckDto>
            {
                new(Guid.NewGuid(), "IdentityVerification", "Passed", "Verified via passport", "admin1", DateTimeOffset.UtcNow.AddDays(-2)),
                new(Guid.NewGuid(), "LicenseVerification", "InProgress", "Checking with licensing authority", null, null)
            });

        return Results.Ok(application);
    }

    private static async Task<IResult> VerifyApplicationAsync(
        Guid doctorProfileId,
        VerifyApplicationRequestDto request)
    {
        return Results.Ok(new
        {
            Message = "Doctor verified successfully",
            DoctorProfileId = doctorProfileId,
            VerifiedAt = DateTimeOffset.UtcNow,
            VerifiedBy = request.ReviewerId
        });
    }

    private static async Task<IResult> RequestChangesAsync(
        Guid doctorProfileId,
        RequestChangesRequestDto request)
    {
        return Results.Ok(new
        {
            Message = "Changes requested",
            DoctorProfileId = doctorProfileId,
            RequestedAt = DateTimeOffset.UtcNow,
            Categories = request.Categories
        });
    }

    private static async Task<IResult> RejectApplicationAsync(
        Guid doctorProfileId,
        RejectApplicationRequestDto request)
    {
        return Results.Ok(new
        {
            Message = "Application rejected",
            DoctorProfileId = doctorProfileId,
            RejectedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });
    }

    private static async Task<IResult> ReviewDocumentAsync(
        Guid documentId,
        ReviewDocumentRequestDto request)
    {
        return Results.Ok(new
        {
            DocumentId = documentId,
            Status = request.Status,
            ReviewedAt = DateTimeOffset.UtcNow,
            ReviewedBy = request.ReviewerId
        });
    }

    private static async Task<IResult> GetVerificationChecksAsync(
        Guid doctorProfileId)
    {
        var checks = new List<VerificationCheckDto>
        {
            new(Guid.NewGuid(), "IdentityVerification", "Passed", "Verified", "admin1", DateTimeOffset.UtcNow.AddDays(-2)),
            new(Guid.NewGuid(), "LicenseVerification", "InProgress", null, null, null),
            new(Guid.NewGuid(), "BackgroundCheck", "Pending", null, null, null)
        };

        return Results.Ok(checks);
    }

    private static async Task<IResult> AddVerificationCheckAsync(
        AddVerificationCheckRequestDto request)
    {
        var check = new VerificationCheckDto(
            Guid.NewGuid(),
            request.CheckType,
            "Pending",
            null,
            null,
            null);

        return Results.Ok(check);
    }

    private static async Task<IResult> UpdateVerificationCheckAsync(
        Guid checkId,
        UpdateVerificationCheckRequestDto request)
    {
        return Results.Ok(new
        {
            CheckId = checkId,
            Status = request.Status,
            Notes = request.Notes,
            UpdatedAt = DateTimeOffset.UtcNow
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
public record SubmitOnboardingRequestDto(
    string LegalName,
    string Email,
    string PhoneNumber,
    string CountryOfPractice,
    string LicenseNumber,
    string LicensingAuthority,
    string Qualifications,
    int YearsOfExperience,
    string Biography,
    string InsuranceProvider,
    string InsurancePolicyNumber,
    DateOnly? LicenseExpiryDate);

public record UpdateOnboardingRequestDto(
    string LegalName,
    string Email,
    string PhoneNumber,
    string CountryOfPractice,
    string LicenseNumber,
    string LicensingAuthority,
    string Qualifications,
    int YearsOfExperience,
    string Biography,
    string InsuranceProvider,
    string InsurancePolicyNumber,
    DateOnly? LicenseExpiryDate);

public record UploadDocumentRequestDto(
    string DocumentType,
    string FileName,
    long FileSize,
    string ContentBase64);

public record OnboardingStatusDto(
    Guid DoctorProfileId,
    DoctorVerificationStatus Status,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? VerifiedAt,
    List<RequiredDocumentDto> RequiredDocuments,
    List<VerificationCheckDto> VerificationChecks);

public record RequiredDocumentDto(
    string DocumentType,
    bool IsUploaded);

public record VerificationCheckDto(
    Guid Id,
    string CheckType,
    string Status,
    string? Notes,
    string? VerifiedBy,
    DateTimeOffset? CompletedAt);

public record DocumentDto(
    Guid Id,
    string DocumentType,
    string FileName,
    string FileUrl,
    long FileSize,
    string Status,
    string? RejectionReason,
    DateTimeOffset UploadedAt);

public record OnboardingRequirementsDto(
    List<DocumentRequirementDto> DocumentRequirements,
    List<VerificationRequirementDto> VerificationRequirements);

public record DocumentRequirementDto(
    string DocumentType,
    string DisplayName,
    string Description,
    bool IsRequired,
    List<string> AllowedFormats,
    long MaxFileSize);

public record VerificationRequirementDto(
    string CheckType,
    string DisplayName,
    bool IsRequired);

public record PendingApplicationDto(
    Guid DoctorProfileId,
    string LegalName,
    string Email,
    string CountryOfPractice,
    string? Specialties,
    DoctorVerificationStatus Status,
    DateTimeOffset SubmittedAt,
    int DocumentsCount);

public record ApplicationDetailsDto(
    Guid DoctorProfileId,
    string LegalName,
    string Email,
    string PhoneNumber,
    string CountryOfPractice,
    string LicenseNumber,
    string LicensingAuthority,
    string Qualifications,
    int YearsOfExperience,
    string Biography,
    string InsuranceProvider,
    string InsurancePolicyNumber,
    DateOnly? LicenseExpiryDate,
    DoctorVerificationStatus Status,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? VerifiedAt,
    List<DocumentDto> Documents,
    List<VerificationCheckDto> VerificationChecks);

public record VerifyApplicationRequestDto(
    string ReviewerId,
    string? Notes);

public record RequestChangesRequestDto(
    List<string> Categories,
    string Message);

public record RejectApplicationRequestDto(
    string Reason,
    bool AllowReapply);

public record ReviewDocumentRequestDto(
    string Status,
    string ReviewerId,
    string? RejectionReason);

public record AddVerificationCheckRequestDto(
    Guid DoctorProfileId,
    string CheckType);

public record UpdateVerificationCheckRequestDto(
    string Status,
    string? Notes,
    string? ExternalReference);
