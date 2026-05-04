using Telehealth.Platform.Domain.Pharmacy;

namespace Telehealth.Platform.Application.Abstractions.Pharmacy;

public interface IDrugInteractionService
{
    Task<DrugInteractionResult> CheckInteractionsAsync(
        List<string> medicationIds,
        CancellationToken cancellationToken = default);

    Task<DrugInteractionResult> CheckInteractionsByNamesAsync(
        List<string> medicationNames,
        CancellationToken cancellationToken = default);
}