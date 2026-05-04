using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Pharmacy;

public class DrugInteraction : Entity<Guid>
{
    public string MedicationAId { get; private set; } = string.Empty;
    public string MedicationBId { get; private set; } = string.Empty;
    public string MedicationAName { get; private set; } = string.Empty;
    public string MedicationBName { get; private set; } = string.Empty;
    public InteractionSeverity Severity { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Recommendation { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    private DrugInteraction(
        Guid id,
        string medicationAId,
        string medicationBId,
        string medicationAName,
        string medicationBName) : base(id)
    {
        MedicationAId = medicationAId;
        MedicationBId = medicationBId;
        MedicationAName = medicationAName;
        MedicationBName = medicationBName;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static DrugInteraction Create(
        string medicationAId,
        string medicationBId,
        string medicationAName,
        string medicationBName,
        InteractionSeverity severity,
        string description,
        string recommendation)
    {
        var interaction = new DrugInteraction(
            Guid.NewGuid(),
            medicationAId,
            medicationBId,
            medicationAName,
            medicationBName)
        {
            Severity = severity,
            Description = description,
            Recommendation = recommendation
        };

        return interaction;
    }
}

public enum InteractionSeverity
{
    Minor = 1,
    Moderate = 2,
    Major = 3,
    Critical = 4
}

public class DrugInteractionResult
{
    public bool HasInteractions { get; set; }
    public List<DrugInteraction> Interactions { get; set; } = new();
}