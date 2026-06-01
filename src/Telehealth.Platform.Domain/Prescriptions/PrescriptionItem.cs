using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Prescriptions;

/// <summary>
/// Individual item within a prescription.
/// </summary>
public sealed class PrescriptionItem : Entity<Guid>
{
    public PrescriptionItem(
        Guid id,
        Guid prescriptionId,
        string medicationCode,
        string medicationName,
        string dosage,
        string frequency,
        string route,
        string? instructions,
        int quantity,
        string? unit,
        int durationDays,
        bool isAsNeeded,
        string? asNeededReason,
        DateTimeOffset createdAt)
        : base(id)
    {
        PrescriptionId = prescriptionId;
        MedicationCode = medicationCode;
        MedicationName = medicationName;
        Dosage = dosage;
        Frequency = frequency;
        Route = route;
        Instructions = instructions;
        Quantity = quantity;
        Unit = unit;
        DurationDays = durationDays;
        IsAsNeeded = isAsNeeded;
        AsNeededReason = asNeededReason;
        IsSubstitutable = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid PrescriptionId { get; private set; }
    public string MedicationCode { get; private set; }
    public string MedicationName { get; private set; }
    public string Dosage { get; private set; }
    public string Frequency { get; private set; }
    public string Route { get; private set; }
    public string? Instructions { get; private set; }
    public int Quantity { get; private set; }
    public string? Unit { get; private set; }
    public int DurationDays { get; private set; }
    public bool IsAsNeeded { get; private set; }
    public string? AsNeededReason { get; private set; }
    public bool IsSubstitutable { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string dosage,
        string frequency,
        string? instructions,
        int quantity,
        DateTimeOffset updatedAt)
    {
        Dosage = dosage;
        Frequency = frequency;
        Instructions = instructions;
        Quantity = quantity;
        UpdatedAt = updatedAt;
    }

    public void SetSubstitutable(bool isSubstitutable, DateTimeOffset updatedAt)
    {
        IsSubstitutable = isSubstitutable;
        UpdatedAt = updatedAt;
    }

    public string GetDisplayText()
    {
        var asNeededText = IsAsNeeded ? " as needed" : "";
        var routeText = string.IsNullOrEmpty(Route) ? "" : $" via {Route}";
        return $"{MedicationName} {Dosage}{routeText}, {Frequency}{asNeededText} for {DurationDays} days";
    }
}
