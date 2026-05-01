using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Time;
using Telehealth.Platform.Domain.Auditing;
using Telehealth.Platform.Domain.Patients;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Patients;

internal static class PatientEndpoints
{
    public static IEndpointRouteBuilder MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var patients = app.MapGroup("/platform/patients").WithTags("Patients");

        patients.MapPost("/register", RegisterPatientAsync);
        patients.MapPut("/{patientId:guid}/onboarding", SaveOnboardingAsync);
        patients.MapGet("/{patientId:guid}", GetPatientAsync);

        return app;
    }

    private static async Task<Results<Ok<PatientResponse>, ValidationProblem>> RegisterPatientAsync(
        RegisterPatientRequest request,
        PlatformDbContext dbContext,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors["displayName"] = ["Display name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.CountryCode) || request.CountryCode.Trim().Length != 2)
        {
            errors["countryCode"] = ["Country code must be a 2-letter ISO code."];
        }

        if (string.IsNullOrWhiteSpace(request.PreferredLanguage))
        {
            errors["preferredLanguage"] = ["Preferred language is required."];
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailTaken = await dbContext.PatientAccounts.AnyAsync(
            patient => patient.Email == normalizedEmail,
            cancellationToken);

        if (emailTaken)
        {
            errors["email"] = ["An account with this email already exists."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var now = clock.UtcNow;
        var patient = new PatientAccount(
            Guid.NewGuid(),
            request.DisplayName.Trim(),
            normalizedEmail,
            $"pending-{Guid.NewGuid():N}",
            request.CountryCode.Trim().ToUpperInvariant(),
            request.PreferredLanguage.Trim(),
            PatientAccountStatus.PendingOnboarding,
            now,
            now);

        dbContext.PatientAccounts.Add(patient);
        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            patient.Id.ToString(),
            "Patient",
            "PatientRegistered",
            "PatientAccount",
            patient.Id.ToString(),
            now));

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(ToResponse(patient, null, []));
    }

    private static async Task<Results<Ok<PatientResponse>, NotFound, ValidationProblem>> SaveOnboardingAsync(
        Guid patientId,
        SavePatientOnboardingRequest request,
        HttpContext httpContext,
        PlatformDbContext dbContext,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var patient = await dbContext.PatientAccounts.SingleOrDefaultAsync(
            x => x.Id == patientId,
            cancellationToken);

        if (patient is null)
        {
            return TypedResults.NotFound();
        }

        var errors = ValidateOnboardingRequest(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var now = clock.UtcNow;
        var profile = await dbContext.PatientMedicalProfiles.SingleOrDefaultAsync(
            x => x.PatientAccountId == patientId,
            cancellationToken);

        if (profile is not null)
        {
            dbContext.PatientMedicalProfiles.Remove(profile);
        }

        var createdProfile = new PatientMedicalProfile(
            Guid.NewGuid(),
            patientId,
            request.DateOfBirth,
            request.SexAtBirth.Trim(),
            request.PhoneNumber.Trim(),
            request.CountryCode.Trim().ToUpperInvariant(),
            request.City.Trim(),
            request.TimeZone.Trim(),
            request.EmergencyContactName.Trim(),
            request.EmergencyContactPhone.Trim(),
            request.EmergencyContactRelationship.Trim(),
            request.ChiefConcern.Trim(),
            request.Symptoms.Trim(),
            request.SymptomDuration.Trim(),
            request.CurrentMedications.Trim(),
            request.Allergies.Trim(),
            request.KnownConditions.Trim(),
            request.PastSurgeries.Trim(),
            request.PregnancyStatus.Trim(),
            request.LifestyleFactors.Trim(),
            request.PreferredConsultationLanguage.Trim(),
            request.UrgencyLevel.Trim(),
            request.EmergencySymptoms,
            request.MedicalDisclaimerAccepted,
            now,
            now);

        dbContext.PatientMedicalProfiles.Add(createdProfile);

        var existingConsents = await dbContext.PatientConsentRecords
            .Where(x => x.PatientAccountId == patientId)
            .ToListAsync(cancellationToken);

        if (existingConsents.Count > 0)
        {
            dbContext.PatientConsentRecords.RemoveRange(existingConsents);
        }

        var consentLanguage = request.ConsentLanguage.Trim();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

        var consentDefinitions = new[]
        {
            new ConsentDefinition(PatientConsentType.TermsOfService, request.TermsAccepted, "Contract", "Patient accepts the platform terms of service."),
            new ConsentDefinition(PatientConsentType.PrivacyPolicy, request.PrivacyAccepted, "LegalObligation", "Patient acknowledges the privacy policy and data rights notice."),
            new ConsentDefinition(PatientConsentType.Teleconsultation, request.TeleconsultationAccepted, "Consent", "Patient consents to remote teleconsultation care delivery."),
            new ConsentDefinition(PatientConsentType.HealthDataProcessing, request.HealthDataProcessingAccepted, "Consent", "Patient consents to processing of special-category health data for care delivery."),
            new ConsentDefinition(PatientConsentType.Research, request.ResearchAccepted, "Consent", "Patient grants optional secondary-use consent for approved research."),
            new ConsentDefinition(PatientConsentType.Marketing, request.MarketingAccepted, "Consent", "Patient grants optional marketing contact consent.")
        };

        foreach (var definition in consentDefinitions)
        {
            dbContext.PatientConsentRecords.Add(new PatientConsentRecord(
                Guid.NewGuid(),
                patientId,
                definition.ConsentType,
                request.ConsentVersion.Trim(),
                definition.TextSnapshot,
                ComputeSha256(definition.TextSnapshot),
                consentLanguage,
                definition.LegalBasis,
                definition.IsAccepted,
                ipAddress,
                userAgent,
                now));
        }

        patient.MarkProfileCompleted(
            patient.DisplayName,
            request.CountryCode.Trim().ToUpperInvariant(),
            request.PreferredConsultationLanguage.Trim(),
            now);

        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            patient.Id.ToString(),
            "Patient",
            "PatientOnboardingCompleted",
            "PatientAccount",
            patient.Id.ToString(),
            now));

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(ToResponse(patient, createdProfile, consentDefinitions));
    }

    private static async Task<Results<Ok<PatientResponse>, NotFound>> GetPatientAsync(
        Guid patientId,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var patient = await dbContext.PatientAccounts.SingleOrDefaultAsync(
            x => x.Id == patientId,
            cancellationToken);

        if (patient is null)
        {
            return TypedResults.NotFound();
        }

        var profile = await dbContext.PatientMedicalProfiles.SingleOrDefaultAsync(
            x => x.PatientAccountId == patientId,
            cancellationToken);

        var consents = await dbContext.PatientConsentRecords
            .Where(x => x.PatientAccountId == patientId)
            .Select(x => new ConsentDefinition(x.ConsentType, x.IsAccepted, x.LegalBasis, x.TextSnapshot))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(ToResponse(patient, profile, consents));
    }

    private static Dictionary<string, string[]> ValidateOnboardingRequest(SavePatientOnboardingRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (!request.TermsAccepted)
        {
            errors["termsAccepted"] = ["Terms of service must be accepted."];
        }

        if (!request.PrivacyAccepted)
        {
            errors["privacyAccepted"] = ["Privacy policy acknowledgement is required."];
        }

        if (!request.TeleconsultationAccepted)
        {
            errors["teleconsultationAccepted"] = ["Teleconsultation consent is required."];
        }

        if (!request.HealthDataProcessingAccepted)
        {
            errors["healthDataProcessingAccepted"] = ["Health data processing consent is required."];
        }

        if (!request.MedicalDisclaimerAccepted)
        {
            errors["medicalDisclaimerAccepted"] = ["Emergency disclaimer acknowledgement is required."];
        }

        if (string.IsNullOrWhiteSpace(request.ConsentVersion))
        {
            errors["consentVersion"] = ["Consent version is required."];
        }

        if (string.IsNullOrWhiteSpace(request.ConsentLanguage))
        {
            errors["consentLanguage"] = ["Consent language is required."];
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            errors["phoneNumber"] = ["Phone number is required."];
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            errors["city"] = ["City is required."];
        }

        if (string.IsNullOrWhiteSpace(request.TimeZone))
        {
            errors["timeZone"] = ["Time zone is required."];
        }

        if (string.IsNullOrWhiteSpace(request.EmergencyContactName))
        {
            errors["emergencyContactName"] = ["Emergency contact name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.EmergencyContactPhone))
        {
            errors["emergencyContactPhone"] = ["Emergency contact phone is required."];
        }

        if (string.IsNullOrWhiteSpace(request.EmergencyContactRelationship))
        {
            errors["emergencyContactRelationship"] = ["Emergency contact relationship is required."];
        }

        if (string.IsNullOrWhiteSpace(request.ChiefConcern))
        {
            errors["chiefConcern"] = ["Chief concern is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Symptoms))
        {
            errors["symptoms"] = ["Symptoms are required."];
        }

        if (string.IsNullOrWhiteSpace(request.SymptomDuration))
        {
            errors["symptomDuration"] = ["Symptom duration is required."];
        }

        if (string.IsNullOrWhiteSpace(request.PreferredConsultationLanguage))
        {
            errors["preferredConsultationLanguage"] = ["Preferred consultation language is required."];
        }

        if (string.IsNullOrWhiteSpace(request.UrgencyLevel))
        {
            errors["urgencyLevel"] = ["Urgency level is required."];
        }

        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            errors["dateOfBirth"] = ["Date of birth cannot be in the future."];
        }

        return errors;
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    private static PatientResponse ToResponse(
        PatientAccount patient,
        PatientMedicalProfile? profile,
        IEnumerable<ConsentDefinition> consents)
    {
        return new PatientResponse(
            patient.Id,
            patient.DisplayName,
            patient.Email,
            patient.CountryCode,
            patient.PreferredLanguage,
            patient.Status.ToString(),
            profile is not null,
            profile?.EmergencySymptoms ?? false,
            consents.Select(x => new ConsentResponse(x.ConsentType, x.IsAccepted, x.LegalBasis)).ToArray(),
            profile is null
                ? null
                : new MedicalProfileResponse(
                    profile.DateOfBirth,
                    profile.SexAtBirth,
                    profile.PhoneNumber,
                    profile.City,
                    profile.TimeZone,
                    profile.EmergencyContactName,
                    profile.EmergencyContactPhone,
                    profile.EmergencyContactRelationship,
                    profile.ChiefConcern,
                    profile.Symptoms,
                    profile.SymptomDuration,
                    profile.CurrentMedications,
                    profile.Allergies,
                    profile.KnownConditions,
                    profile.PastSurgeries,
                    profile.PregnancyStatus,
                    profile.LifestyleFactors,
                    profile.PreferredConsultationLanguage,
                    profile.UrgencyLevel,
                    profile.EmergencySymptoms,
                    profile.MedicalDisclaimerAccepted));
    }

    internal sealed record RegisterPatientRequest(
        string DisplayName,
        string Email,
        string CountryCode,
        string PreferredLanguage);

    internal sealed record SavePatientOnboardingRequest(
        string ConsentVersion,
        string ConsentLanguage,
        bool TermsAccepted,
        bool PrivacyAccepted,
        bool TeleconsultationAccepted,
        bool HealthDataProcessingAccepted,
        bool ResearchAccepted,
        bool MarketingAccepted,
        DateOnly DateOfBirth,
        string SexAtBirth,
        string PhoneNumber,
        string CountryCode,
        string City,
        string TimeZone,
        string EmergencyContactName,
        string EmergencyContactPhone,
        string EmergencyContactRelationship,
        string ChiefConcern,
        string Symptoms,
        string SymptomDuration,
        string CurrentMedications,
        string Allergies,
        string KnownConditions,
        string PastSurgeries,
        string PregnancyStatus,
        string LifestyleFactors,
        string PreferredConsultationLanguage,
        string UrgencyLevel,
        bool EmergencySymptoms,
        bool MedicalDisclaimerAccepted);

    internal sealed record PatientResponse(
        Guid Id,
        string DisplayName,
        string Email,
        string CountryCode,
        string PreferredLanguage,
        string Status,
        bool HasMedicalProfile,
        bool EmergencySymptoms,
        ConsentResponse[] Consents,
        MedicalProfileResponse? MedicalProfile);

    internal sealed record ConsentResponse(
        string ConsentType,
        bool IsAccepted,
        string LegalBasis);

    internal sealed record MedicalProfileResponse(
        DateOnly DateOfBirth,
        string SexAtBirth,
        string PhoneNumber,
        string City,
        string TimeZone,
        string EmergencyContactName,
        string EmergencyContactPhone,
        string EmergencyContactRelationship,
        string ChiefConcern,
        string Symptoms,
        string SymptomDuration,
        string CurrentMedications,
        string Allergies,
        string KnownConditions,
        string PastSurgeries,
        string PregnancyStatus,
        string LifestyleFactors,
        string PreferredConsultationLanguage,
        string UrgencyLevel,
        bool EmergencySymptoms,
        bool MedicalDisclaimerAccepted);

    private sealed record ConsentDefinition(
        string ConsentType,
        bool IsAccepted,
        string LegalBasis,
        string TextSnapshot);
}
