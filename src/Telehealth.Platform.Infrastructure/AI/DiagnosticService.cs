using Telehealth.Platform.Application.Abstractions.AI;

namespace Telehealth.Platform.Infrastructure.AI;

public class DiagnosticService : IDiagnosticService
{
    private readonly IList<DiagnosticResult> _mockResults = new List<DiagnosticResult>
    {
        new DiagnosticResult
        {
            Diagnosis = "Common cold",
            ConfidenceScore = 0.85m,
            Recommendations = new List<Recommendation>
            {
                new Recommendation { Title = "Rest", Description = "Get adequate rest" },
                new Recommendation { Title = "Hydration", Description = "Drink plenty of fluids" }
            },
            Warnings = new List<string>(),
            ModelUsed = "symptom-analyzer-v1"
        }
    };

    public Task<DiagnosticResult> AnalyzeSymptomsAsync(
        string symptoms,
        Guid? modelId,
        CancellationToken cancellationToken = default)
    {
        var result = new DiagnosticResult
        {
            Diagnosis = "Analysis pending review",
            ConfidenceScore = 0.5m,
            Recommendations = new List<Recommendation>(),
            Warnings = new List<string> { "Requires physician review" },
            ModelUsed = modelId?.ToString() ?? "default-model"
        };

        return Task.FromResult(result);
    }

    public Task<DiagnosticResult> AnalyzeWithImageAsync(
        string symptoms,
        byte[] imageData,
        Guid? modelId,
        CancellationToken cancellationToken = default)
    {
        var result = new DiagnosticResult
        {
            Diagnosis = "Image analysis pending",
            ConfidenceScore = 0.6m,
            Recommendations = new List<Recommendation>(),
            Warnings = new List<string> { "Image requires specialist review" },
            ModelUsed = modelId?.ToString() ?? "image-analysis-model"
        };

        return Task.FromResult(result);
    }
}