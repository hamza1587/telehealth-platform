namespace Telehealth.Platform.Prescription.Services;

public enum InteractionSeverity
{
    Low,
    Moderate,
    High,
    Critical
}

public record DrugInteraction(
    string Medication1,
    string Medication2,
    string Description,
    InteractionSeverity Severity
);

public interface IDrugInteractionService
{
    Task<List<DrugInteraction>> CheckInteractionsAsync(List<string> currentMedications, List<string> newMedications);
}
