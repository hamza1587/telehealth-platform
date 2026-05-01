using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Wallets;

/// <summary>
/// Day pass subscription for unlimited consultations within a period.
/// </summary>
public sealed class DayPass : Entity<Guid>
{
    public DayPass(
        Guid id,
        Guid patientAccountId,
        string passType,
        int durationHours,
        decimal price,
        string currency,
        int maxConsultations,
        bool isUnlimited,
        DateTimeOffset activatedAt,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        PassType = passType;
        DurationHours = durationHours;
        Price = price;
        Currency = currency;
        MaxConsultations = maxConsultations;
        IsUnlimited = isUnlimited;
        ActivatedAt = activatedAt;
        ExpiresAt = expiresAt;
        Status = DayPassStatus.Active;
        ConsultationsUsed = 0;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid PatientAccountId { get; }
    public string PassType { get; private set; }
    public int DurationHours { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; }
    public int MaxConsultations { get; private set; }
    public bool IsUnlimited { get; private set; }
    public int ConsultationsUsed { get; private set; }
    public DayPassStatus Status { get; private set; }
    public DateTimeOffset ActivatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool CanUse()
    {
        if (Status != DayPassStatus.Active)
            return false;

        if (DateTimeOffset.UtcNow > ExpiresAt)
        {
            Status = DayPassStatus.Expired;
            return false;
        }

        if (!IsUnlimited && ConsultationsUsed >= MaxConsultations)
            return false;

        return true;
    }

    public void UseConsultation(DateTimeOffset usedAt)
    {
        if (!CanUse())
            throw new InvalidOperationException("Day pass cannot be used");

        ConsultationsUsed++;
        UpdatedAt = usedAt;
    }

    public void Cancel(string reason, DateTimeOffset cancelledAt)
    {
        if (Status != DayPassStatus.Active)
            throw new InvalidOperationException("Only active passes can be cancelled");

        Status = DayPassStatus.Cancelled;
        CancelledAt = cancelledAt;
        CancellationReason = reason;
        UpdatedAt = cancelledAt;
    }

    public void Extend(int additionalHours, DateTimeOffset extendedAt)
    {
        ExpiresAt = ExpiresAt.AddHours(additionalHours);
        UpdatedAt = extendedAt;
    }

    public TimeSpan GetRemainingTime()
    {
        if (Status != DayPassStatus.Active)
            return TimeSpan.Zero;

        var remaining = ExpiresAt - DateTimeOffset.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    public int GetRemainingConsultations()
    {
        if (IsUnlimited)
            return int.MaxValue;

        return Math.Max(0, MaxConsultations - ConsultationsUsed);
    }
}

public enum DayPassStatus
{
    Active,
    Expired,
    Cancelled,
    Suspended,
    FullyUsed
}

public enum DayPassType
{
    Basic24Hour,
    Premium24Hour,
    WeekendPass,
    WeekPass,
    UnlimitedDay
}
