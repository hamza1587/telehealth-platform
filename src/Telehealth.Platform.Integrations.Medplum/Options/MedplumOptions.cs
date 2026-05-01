namespace Telehealth.Platform.Integrations.Medplum.Options;

public sealed class MedplumOptions
{
    public const string SectionName = "Medplum";

    public string BaseUrl { get; init; } = "http://localhost:8103/";

    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }

    public int TimeoutSeconds { get; init; } = 30;
}
