using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Billing;

/// <summary>
/// Platform fee configuration and calculation.
/// </summary>
public sealed class PlatformFeeStructure : Entity<Guid>
{
    public PlatformFeeStructure(
        Guid id,
        string name,
        decimal basePlatformFeePercent,
        decimal? paymentProcessingFeePercent,
        Money? fixedFeePerConsultation,
        string? countryCode,
        string? doctorTier,
        DateTimeOffset effectiveFrom,
        DateTimeOffset createdAt)
        : base(id)
    {
        Name = name;
        BasePlatformFeePercent = basePlatformFeePercent;
        PaymentProcessingFeePercent = paymentProcessingFeePercent;
        FixedFeePerConsultation = fixedFeePerConsultation;
        CountryCode = countryCode;
        DoctorTier = doctorTier;
        EffectiveFrom = effectiveFrom;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public string Name { get; private set; }
    public decimal BasePlatformFeePercent { get; private set; }
    public decimal? PaymentProcessingFeePercent { get; private set; }
    public Money? FixedFeePerConsultation { get; private set; }
    public string? CountryCode { get; private set; }
    public string? DoctorTier { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string name,
        decimal basePlatformFeePercent,
        decimal? paymentProcessingFeePercent,
        Money? fixedFeePerConsultation,
        DateTimeOffset updatedAt)
    {
        Name = name;
        BasePlatformFeePercent = basePlatformFeePercent;
        PaymentProcessingFeePercent = paymentProcessingFeePercent;
        FixedFeePerConsultation = fixedFeePerConsultation;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset effectiveTo, DateTimeOffset updatedAt)
    {
        IsActive = false;
        EffectiveTo = effectiveTo;
        UpdatedAt = updatedAt;
    }

    public FeeCalculation CalculateFees(Money baseAmount, string paymentMethod)
    {
        var platformFee = new Money(baseAmount.Amount * BasePlatformFeePercent / 100, baseAmount.Currency);

        var processingFee = PaymentProcessingFeePercent.HasValue
            ? new Money(baseAmount.Amount * PaymentProcessingFeePercent.Value / 100, baseAmount.Currency)
            : new Money(0, baseAmount.Currency);

        var fixedFee = FixedFeePerConsultation ?? new Money(0, baseAmount.Currency);

        var totalPlatformFees = new Money(platformFee.Amount + processingFee.Amount + fixedFee.Amount, baseAmount.Currency);
        var doctorPayout = new Money(baseAmount.Amount - totalPlatformFees.Amount, baseAmount.Currency);

        return new FeeCalculation(
            baseAmount,
            platformFee,
            processingFee,
            fixedFee,
            totalPlatformFees,
            doctorPayout);
    }
}

public readonly record struct FeeCalculation(
    Money BaseAmount,
    Money PlatformFee,
    Money ProcessingFee,
    Money FixedFee,
    Money TotalPlatformFees,
    Money DoctorPayout);

public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0, currency);
}
