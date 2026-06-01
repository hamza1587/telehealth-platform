using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.Localization;

public class RegulatoryRuleEngine : IRegulatoryRuleEngine
{
    public RegulatoryRules GetRulesForCountry(string countryCode)
    {
        return countryCode.ToUpperInvariant() switch
        {
            "DE" => new RegulatoryRules("DE", true, 3650, true, true, 5, 0.5, new List<string> { "PatientId", "Conditions", "Medications" }),
            "FR" => new RegulatoryRules("FR", true, 3650, true, true, 5, 0.5, new List<string> { "PatientId", "Conditions", "Allergies" }),
            "IT" => new RegulatoryRules("IT", true, 3650, true, true, 3, 1.0, new List<string> { "PatientId", "Conditions" }),
            "ES" => new RegulatoryRules("ES", true, 2555, true, true, 3, 1.0, new List<string> { "PatientId", "Conditions" }),
            "NL" => new RegulatoryRules("NL", true, 3650, true, true, 5, 0.5, new List<string> { "PatientId", "Conditions", "Medications" }),
            "PL" => new RegulatoryRules("PL", true, 3650, true, true, 3, 1.0, new List<string> { "PatientId", "Conditions" }),
            _ => throw new NotSupportedException($"Country code {countryCode} not supported")
        };
    }

    public bool ValidateExportAgainstRules(ResearchExportRequest request, string countryCode)
    {
        var rules = GetRulesForCountry(countryCode);

        if (request.KAnonymityLevel < rules.MinKAnonymityLevel)
            return false;

        if (request.Epsilon > rules.MaxEpsilon)
            return false;

        return true;
    }

    public ComplianceCheckResult CheckCompliance(string countryCode, object data)
    {
        var rules = GetRulesForCountry(countryCode);
        var issues = new List<string>();

        return new ComplianceCheckResult(issues.Count == 0, issues, rules);
    }
}

public interface IRegulatoryRuleEngine
{
    RegulatoryRules GetRulesForCountry(string countryCode);
    bool ValidateExportAgainstRules(ResearchExportRequest request, string countryCode);
    ComplianceCheckResult CheckCompliance(string countryCode, object data);
}

public record RegulatoryRules(
    string CountryCode,
    bool RequiresExplicitConsent,
    int DataRetentionDays,
    bool CrossBorderRequiresApproval,
    bool ResearchRequiresEthicsReview,
    int MinKAnonymityLevel,
    double MaxEpsilon,
    List<string> MandatoryFields
);

public record ComplianceCheckResult(
    bool IsCompliant,
    List<string> Issues,
    RegulatoryRules RulesApplied
);