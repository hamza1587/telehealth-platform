using Telehealth.Platform.Application.Abstractions.EHR;

namespace Telehealth.Platform.Infrastructure.EHR;

public class FhirService : IFhirService
{
    private readonly Dictionary<string, FhirPatient> _patients = new();

    public Task<FhirPatient?> GetPatientAsync(string patientId, CancellationToken cancellationToken = default)
    {
        _patients.TryGetValue(patientId, out var patient);
        return Task.FromResult(patient);
    }

    public Task<FhirEncounter?> GetEncounterAsync(string encounterId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<FhirEncounter?>(null);
    }

    public Task<IEnumerable<FhirMedication>> GetMedicationsAsync(string patientId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Enumerable.Empty<FhirMedication>());
    }

    public Task<bool> SyncPatientAsync(FhirPatient patient, CancellationToken cancellationToken = default)
    {
        _patients[patient.Id] = patient;
        return Task.FromResult(true);
    }

    public Task<string> CreateEncounterAsync(FhirEncounter encounter, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }
}