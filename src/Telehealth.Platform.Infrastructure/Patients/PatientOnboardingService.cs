using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Telehealth.Platform.Application.Abstractions.Patients;
using Telehealth.Platform.Domain.Patients;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Patients;

/// <summary>
/// Implementation of patient onboarding service.
/// </summary>
internal sealed class PatientOnboardingService : IPatientOnboardingService
{
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<PatientOnboardingService> _logger;

    public PatientOnboardingService(
        PlatformDbContext dbContext,
        ILogger<PatientOnboardingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CreateMedicalProfileResult> CreateMedicalProfileAsync(
        Guid patientAccountId,
        CreateMedicalProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if profile already exists
            var existingProfile = await _dbContext.PatientMedicalProfiles
                .FirstOrDefaultAsync(p => p.PatientAccountId == patientAccountId, cancellationToken);

            if (existingProfile != null)
            {
                existingProfile.Update(
                    request.DateOfBirth,
                    request.SexAtBirth,
                    request.PhoneNumber,
                    request.CountryCode,
                    request.City,
                    request.TimeZone,
                    request.EmergencyContactName,
                    request.EmergencyContactPhone,
                    request.EmergencyContactRelationship,
                    request.ChiefConcern,
                    request.Symptoms,
                    request.SymptomDuration,
                    request.CurrentMedications,
                    request.Allergies,
                    request.KnownConditions,
                    request.PastSurgeries,
                    request.PregnancyStatus,
                    request.LifestyleFactors,
                    request.PreferredConsultationLanguage,
                    request.UrgencyLevel,
                    request.EmergencySymptoms,
                    request.MedicalDisclaimerAccepted,
                    DateTimeOffset.UtcNow
                );

                _dbContext.PatientMedicalProfiles.Update(existingProfile);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated medical profile for patient account {PatientAccountId}", patientAccountId);

                return new CreateMedicalProfileResult
                {
                    Success = true,
                    Profile = MapToDto(existingProfile)
                };
            }

            // Create new profile
            var profile = new PatientMedicalProfile(
                id: Guid.NewGuid(),
                patientAccountId: patientAccountId,
                dateOfBirth: request.DateOfBirth,
                sexAtBirth: request.SexAtBirth,
                phoneNumber: request.PhoneNumber,
                countryCode: request.CountryCode,
                city: request.City,
                timeZone: request.TimeZone,
                emergencyContactName: request.EmergencyContactName,
                emergencyContactPhone: request.EmergencyContactPhone,
                emergencyContactRelationship: request.EmergencyContactRelationship,
                chiefConcern: request.ChiefConcern,
                symptoms: request.Symptoms,
                symptomDuration: request.SymptomDuration,
                currentMedications: request.CurrentMedications,
                allergies: request.Allergies,
                knownConditions: request.KnownConditions,
                pastSurgeries: request.PastSurgeries,
                pregnancyStatus: request.PregnancyStatus,
                lifestyleFactors: request.LifestyleFactors,
                preferredConsultationLanguage: request.PreferredConsultationLanguage,
                urgencyLevel: request.UrgencyLevel,
                emergencySymptoms: request.EmergencySymptoms,
                medicalDisclaimerAccepted: request.MedicalDisclaimerAccepted,
                createdAt: DateTimeOffset.UtcNow,
                updatedAt: DateTimeOffset.UtcNow
            );

            _dbContext.PatientMedicalProfiles.Add(profile);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created medical profile for patient account {PatientAccountId}", patientAccountId);

            return new CreateMedicalProfileResult
            {
                Success = true,
                Profile = MapToDto(profile)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medical profile for patient account {PatientAccountId}", patientAccountId);
            return new CreateMedicalProfileResult
            {
                Success = false,
                Error = "Failed to create medical profile"
            };
        }
    }

    public async Task<MedicalProfileDto?> GetMedicalProfileAsync(
        Guid patientAccountId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.PatientMedicalProfiles
            .FirstOrDefaultAsync(p => p.PatientAccountId == patientAccountId, cancellationToken);

        return profile is null ? null : MapToDto(profile);
    }

    public async Task RecordConsentAsync(
        Guid patientAccountId,
        RecordConsentRequest request,
        CancellationToken cancellationToken = default)
    {
        var consentRecord = new PatientConsentRecord(
            id: Guid.NewGuid(),
            patientAccountId: patientAccountId,
            consentType: request.ConsentType,
            version: request.Version,
            textSnapshot: request.TextSnapshot,
            textHash: request.TextHash,
            language: request.Language,
            legalBasis: request.LegalBasis,
            isAccepted: request.IsAccepted,
            ipAddress: request.IpAddress,
            userAgent: request.UserAgent,
            capturedAt: DateTimeOffset.UtcNow
        );

        _dbContext.PatientConsentRecords.Add(consentRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recorded consent {ConsentType} for patient account {PatientAccountId}", 
            request.ConsentType, patientAccountId);
    }

    public async Task<List<ConsentRecordDto>> GetConsentRecordsAsync(
        Guid patientAccountId,
        CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.PatientConsentRecords
            .Where(c => c.PatientAccountId == patientAccountId)
            .OrderByDescending(c => c.CapturedAt)
            .ToListAsync(cancellationToken);

        return records.Select(MapToDto).ToList();
    }

    public async Task WithdrawConsentAsync(
        Guid patientAccountId,
        string consentType,
        CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.PatientConsentRecords
            .Where(c => c.PatientAccountId == patientAccountId && c.ConsentType == consentType && c.IsAccepted)
            .ToListAsync(cancellationToken);

        foreach (var record in records)
        {
            // Update the record to mark as withdrawn
            // Since the entity is immutable, we need to create a new record with withdrawal
            var withdrawalRecord = new PatientConsentRecord(
                id: Guid.NewGuid(),
                patientAccountId: patientAccountId,
                consentType: consentType,
                version: record.Version,
                textSnapshot: record.TextSnapshot,
                textHash: record.TextHash,
                language: record.Language,
                legalBasis: record.LegalBasis,
                isAccepted: false,
                ipAddress: null,
                userAgent: null,
                capturedAt: DateTimeOffset.UtcNow
            );

            _dbContext.PatientConsentRecords.Add(withdrawalRecord);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Withdrew consent {ConsentType} for patient account {PatientAccountId}", 
            consentType, patientAccountId);
    }

    private static MedicalProfileDto MapToDto(PatientMedicalProfile profile)
    {
        return new MedicalProfileDto
        {
            Id = profile.Id,
            PatientAccountId = profile.PatientAccountId,
            DateOfBirth = profile.DateOfBirth,
            SexAtBirth = profile.SexAtBirth,
            PhoneNumber = profile.PhoneNumber,
            CountryCode = profile.CountryCode,
            City = profile.City,
            TimeZone = profile.TimeZone,
            EmergencyContactName = profile.EmergencyContactName,
            EmergencyContactPhone = profile.EmergencyContactPhone,
            EmergencyContactRelationship = profile.EmergencyContactRelationship,
            ChiefConcern = profile.ChiefConcern,
            Symptoms = profile.Symptoms,
            SymptomDuration = profile.SymptomDuration,
            CurrentMedications = profile.CurrentMedications,
            Allergies = profile.Allergies,
            KnownConditions = profile.KnownConditions,
            PastSurgeries = profile.PastSurgeries,
            PregnancyStatus = profile.PregnancyStatus,
            LifestyleFactors = profile.LifestyleFactors,
            PreferredConsultationLanguage = profile.PreferredConsultationLanguage,
            UrgencyLevel = profile.UrgencyLevel,
            EmergencySymptoms = profile.EmergencySymptoms,
            MedicalDisclaimerAccepted = profile.MedicalDisclaimerAccepted,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    private static ConsentRecordDto MapToDto(PatientConsentRecord record)
    {
        return new ConsentRecordDto
        {
            Id = record.Id,
            PatientAccountId = record.PatientAccountId,
            ConsentType = record.ConsentType,
            Version = record.Version,
            TextSnapshot = record.TextSnapshot,
            TextHash = record.TextHash,
            Language = record.Language,
            LegalBasis = record.LegalBasis,
            IsAccepted = record.IsAccepted,
            IpAddress = record.IpAddress,
            UserAgent = record.UserAgent,
            CapturedAt = record.CapturedAt,
            WithdrawnAt = record.WithdrawnAt
        };
    }

    private static string ComputeHash(string text)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
