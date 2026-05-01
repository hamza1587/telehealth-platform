using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Consent;

/// <summary>
/// Defines which consent types are required for different user actions.
/// </summary>
public sealed class ConsentRequirement : Entity<Guid>
{
    public ConsentRequirement(
        Guid id,
        string action,
        string consentType,
        bool isRequired,
        int minimumAge,
        DateTimeOffset createdAt)
        : base(id)
    {
        Action = action;
        ConsentType = consentType;
        IsRequired = isRequired;
        MinimumAge = minimumAge;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public string Action { get; private set; }
    public string ConsentType { get; private set; }
    public bool IsRequired { get; private set; }
    public int MinimumAge { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(bool isRequired, int minimumAge, DateTimeOffset updatedAt)
    {
        IsRequired = isRequired;
        MinimumAge = minimumAge;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        IsActive = false;
        UpdatedAt = updatedAt;
    }
}

public enum RequiredAction
{
    Registration,
    BookingConsultation,
    VideoCall,
    PrescriptionRequest,
    DataExport,
    ResearchParticipation,
    ProfileUpdate,
    PaymentProcessing
}
