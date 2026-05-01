using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

/// <summary>
/// Doctor's consultation pricing configuration.
/// </summary>
public sealed class DoctorPricing : Entity<Guid>
{
    public DoctorPricing(
        Guid id,
        Guid doctorProfileId,
        Money pricePerSecond,
        Money? videoCallSurcharge,
        Money? phoneCallSurcharge,
        Money? chatSurcharge,
        bool isInstantConsultationEnabled,
        Money? instantConsultationPremium,
        DateTimeOffset createdAt)
        : base(id)
    {
        DoctorProfileId = doctorProfileId;
        PricePerSecond = pricePerSecond;
        VideoCallSurcharge = videoCallSurcharge;
        PhoneCallSurcharge = phoneCallSurcharge;
        ChatSurcharge = chatSurcharge;
        IsInstantConsultationEnabled = isInstantConsultationEnabled;
        InstantConsultationPremium = instantConsultationPremium;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid DoctorProfileId { get; }
    public Money PricePerSecond { get; private set; }
    public Money? VideoCallSurcharge { get; private set; }
    public Money? PhoneCallSurcharge { get; private set; }
    public Money? ChatSurcharge { get; private set; }
    public bool IsInstantConsultationEnabled { get; private set; }
    public Money? InstantConsultationPremium { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void UpdatePricing(
        Money pricePerSecond,
        Money? videoCallSurcharge,
        Money? phoneCallSurcharge,
        Money? chatSurcharge,
        DateTimeOffset updatedAt)
    {
        PricePerSecond = pricePerSecond;
        VideoCallSurcharge = videoCallSurcharge;
        PhoneCallSurcharge = phoneCallSurcharge;
        ChatSurcharge = chatSurcharge;
        UpdatedAt = updatedAt;
    }

    public void UpdateInstantConsultation(
        bool isEnabled,
        Money? premium,
        DateTimeOffset updatedAt)
    {
        IsInstantConsultationEnabled = isEnabled;
        InstantConsultationPremium = premium;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        IsActive = false;
        UpdatedAt = updatedAt;
    }

    public Money CalculateTotalPrice(ConsultationMode mode, bool isInstant)
    {
        var basePrice = PricePerSecond;

        var surcharge = mode switch
        {
            ConsultationMode.Video => VideoCallSurcharge,
            ConsultationMode.Phone => PhoneCallSurcharge,
            ConsultationMode.Text => ChatSurcharge,
            _ => null
        };

        var total = surcharge.HasValue
            ? new Money(basePrice.Amount + surcharge.Value.Amount, basePrice.Currency)
            : basePrice;

        if (isInstant && IsInstantConsultationEnabled && InstantConsultationPremium.HasValue)
        {
            total = new Money(total.Amount + InstantConsultationPremium.Value.Amount, total.Currency);
        }

        return total;
    }
}

public enum ConsultationMode
{
    Video,
    Phone,
    Text,
    InPerson
}

public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0, currency);
    public static Money EUR(decimal amount) => new(amount, "EUR");
}
