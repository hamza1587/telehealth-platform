namespace Telehealth.Platform.Application.Abstractions.AI;

public interface IDiagnosticService
{
    Task<DiagnosticResult> AnalyzeSymptomsAsync(
        string symptoms,
        Guid? modelId,
        CancellationToken cancellationToken = default);

    Task<DiagnosticResult> AnalyzeWithImageAsync(
        string symptoms,
        byte[] imageData,
        Guid? modelId,
        CancellationToken cancellationToken = default);
}

public class DiagnosticResult
{
    public string Diagnosis { get; set; } = null!;
    public decimal ConfidenceScore { get; set; }
    public List<Recommendation> Recommendations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string ModelUsed { get; set; } = null!;
}

public class Recommendation
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
}