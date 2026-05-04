using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.AI;

public class DiagnosticSession : Entity<Guid>
{
    public Guid PatientId { get; private set; }
    public Guid? DoctorId { get; private set; }
    public Guid ModelId { get; private set; }
    public string Symptoms { get; private set; } = string.Empty;
    public string Diagnosis { get; private set; } = string.Empty;
    public decimal ConfidenceScore { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private DiagnosticSession(
        Guid id,
        Guid patientId,
        Guid modelId,
        string symptoms) : base(id)
    {
        PatientId = patientId;
        ModelId = modelId;
        Symptoms = symptoms;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static DiagnosticSession Create(
        Guid patientId,
        Guid modelId,
        string symptoms)
    {
        return new DiagnosticSession(Guid.NewGuid(), patientId, modelId, symptoms);
    }

    public void CompleteSession(string diagnosis, decimal confidenceScore)
    {
        Diagnosis = diagnosis;
        ConfidenceScore = confidenceScore;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void AssignDoctor(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}