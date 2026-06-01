using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Clinical;

/// <summary>
/// Patient allergy or intolerance record.
/// </summary>
public sealed class AllergyIntolerance : Entity<Guid>
{
    public AllergyIntolerance(
        Guid id,
        Guid patientAccountId,
        string substance,
        string? substanceCode,
        AllergyCategory category,
        AllergyCriticality criticality,
        string? reaction,
        string? onset,
        string? note,
        DateTimeOffset createdAt)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        Substance = substance;
        SubstanceCode = substanceCode;
        Category = category;
        Criticality = criticality;
        Reaction = reaction;
        Onset = onset;
        Note = note;
        Status = AllergyStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid PatientAccountId { get; private set; }
    public string Substance { get; private set; }
    public string? SubstanceCode { get; private set; }
    public AllergyCategory Category { get; private set; }
    public AllergyCriticality Criticality { get; private set; }
    public string? Reaction { get; private set; }
    public string? Onset { get; private set; }
    public string? Note { get; private set; }
    public AllergyStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string substance,
        AllergyCategory category,
        AllergyCriticality criticality,
        string? reaction,
        string? note,
        DateTimeOffset updatedAt)
    {
        Substance = substance;
        Category = category;
        Criticality = criticality;
        Reaction = reaction;
        Note = note;
        UpdatedAt = updatedAt;
    }

    public void Inactivate(DateTimeOffset updatedAt)
    {
        Status = AllergyStatus.Inactive;
        UpdatedAt = updatedAt;
    }

    public void Resolve(DateTimeOffset updatedAt)
    {
        Status = AllergyStatus.Resolved;
        UpdatedAt = updatedAt;
    }
}

public enum AllergyCategory
{
    Food,
    Medication,
    Environment,
    Biologic
}

public enum AllergyCriticality
{
    Low,
    High,
    UnableToAssess
}

public enum AllergyStatus
{
    Active,
    Inactive,
    Resolved
}
