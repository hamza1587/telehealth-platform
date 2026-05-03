using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Billing;

public class PatientInsurance : Entity<Guid>
{
    public Guid PatientAccountId { get; private set; }
    public Guid InsuranceProviderId { get; private set; }
    public string MemberId { get; private set; } = string.Empty;
    public string? GroupId { get; private set; }
    public string? PlanName { get; private set; }
    public DateTimeOffset? EffectiveDate { get; private set; }
    public DateTimeOffset? ExpirationDate { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private PatientInsurance(
        Guid id,
        Guid patientAccountId,
        Guid insuranceProviderId,
        string memberId) : base(id)
    {
        PatientAccountId = patientAccountId;
        InsuranceProviderId = insuranceProviderId;
        MemberId = memberId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static PatientInsurance Create(
        Guid patientAccountId,
        Guid insuranceProviderId,
        string memberId,
        string? groupId = null,
        string? planName = null)
    {
        return new PatientInsurance(
            Guid.NewGuid(),
            patientAccountId,
            insuranceProviderId,
            memberId)
        {
            GroupId = groupId,
            PlanName = planName
        };
    }

    public void Update(
        string memberId,
        string? groupId,
        string? planName,
        DateTimeOffset? effectiveDate,
        DateTimeOffset? expirationDate,
        bool isPrimary,
        bool isActive)
    {
        MemberId = memberId;
        GroupId = groupId;
        PlanName = planName;
        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
        IsPrimary = isPrimary;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTimeOffset.UtcNow;
}