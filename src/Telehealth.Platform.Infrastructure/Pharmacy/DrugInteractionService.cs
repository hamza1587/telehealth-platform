using Telehealth.Platform.Application.Abstractions.Pharmacy;
using Telehealth.Platform.Domain.Pharmacy;

namespace Telehealth.Platform.Infrastructure.Pharmacy;

public class DrugInteractionService : IDrugInteractionService
{
    private readonly List<DrugInteraction> _interactions = new()
    {
        DrugInteraction.Create(
            "aspirin", "ibuprofen", "Aspirin", "Ibuprofen",
            InteractionSeverity.Major,
            "Increased risk of bleeding",
            "Monitor for bleeding signs; consider alternative pain relief"),
        DrugInteraction.Create(
            "warfarin", "aspirin", "Warfarin", "Aspirin",
            InteractionSeverity.Critical,
            "Increased bleeding risk with anticoagulation",
            "Close INR monitoring required; consider dose adjustment")
    };

    public Task<DrugInteractionResult> CheckInteractionsAsync(
        List<string> medicationIds,
        CancellationToken cancellationToken = default)
    {
        var results = new List<DrugInteraction>();

        for (int i = 0; i < medicationIds.Count; i++)
        {
            for (int j = i + 1; j < medicationIds.Count; j++)
            {
                var interaction = _interactions.FirstOrDefault(x =>
                    (x.MedicationAId == medicationIds[i] && x.MedicationBId == medicationIds[j]) ||
                    (x.MedicationAId == medicationIds[j] && x.MedicationBId == medicationIds[i]));

                if (interaction != null)
                {
                    results.Add(interaction);
                }
            }
        }

        return Task.FromResult(new DrugInteractionResult
        {
            HasInteractions = results.Any(),
            Interactions = results
        });
    }

    public Task<DrugInteractionResult> CheckInteractionsByNamesAsync(
        List<string> medicationNames,
        CancellationToken cancellationToken = default)
    {
        var results = new List<DrugInteraction>();

        for (int i = 0; i < medicationNames.Count; i++)
        {
            for (int j = i + 1; j < medicationNames.Count; j++)
            {
                var interaction = _interactions.FirstOrDefault(x =>
                    (x.MedicationAName.Equals(medicationNames[i], StringComparison.OrdinalIgnoreCase) &&
                     x.MedicationBName.Equals(medicationNames[j], StringComparison.OrdinalIgnoreCase)) ||
                    (x.MedicationAName.Equals(medicationNames[j], StringComparison.OrdinalIgnoreCase) &&
                     x.MedicationBName.Equals(medicationNames[i], StringComparison.OrdinalIgnoreCase)));

                if (interaction != null)
                {
                    results.Add(interaction);
                }
            }
        }

        return Task.FromResult(new DrugInteractionResult
        {
            HasInteractions = results.Any(),
            Interactions = results
        });
    }
}