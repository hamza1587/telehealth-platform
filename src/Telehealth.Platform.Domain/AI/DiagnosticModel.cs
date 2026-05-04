using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.AI;

public class DiagnosticModel : Entity<Guid>
{
    public string ModelName { get; private set; } = string.Empty;
    public string ModelVersion { get; private set; } = string.Empty;
    public string Specialty { get; private set; } = string.Empty;
    public decimal AccuracyScore { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private DiagnosticModel(
        Guid id,
        string modelName,
        string modelVersion,
        string specialty,
        decimal accuracyScore) : base(id)
    {
        ModelName = modelName;
        ModelVersion = modelVersion;
        Specialty = specialty;
        AccuracyScore = accuracyScore;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static DiagnosticModel Create(
        string modelName,
        string modelVersion,
        string specialty,
        decimal accuracyScore)
    {
        return new DiagnosticModel(
            Guid.NewGuid(),
            modelName,
            modelVersion,
            specialty,
            accuracyScore);
    }

    public void UpdateAccuracy(decimal accuracyScore)
    {
        AccuracyScore = accuracyScore;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}