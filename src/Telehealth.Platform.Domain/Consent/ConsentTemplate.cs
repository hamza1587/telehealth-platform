using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Consent;

/// <summary>
/// Template for consent forms that patients must accept.
/// Supports versioning and multi-language support.
/// </summary>
public sealed class ConsentTemplate : Entity<Guid>
{
    public ConsentTemplate(
        Guid id,
        string consentType,
        string version,
        string title,
        string content,
        string language,
        string legalBasis,
        bool isRequired,
        DateTimeOffset effectiveFrom,
        DateTimeOffset createdAt)
        : base(id)
    {
        ConsentType = consentType;
        Version = version;
        Title = title;
        Content = content;
        Language = language;
        LegalBasis = legalBasis;
        IsRequired = isRequired;
        EffectiveFrom = effectiveFrom;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public string ConsentType { get; private set; }
    public string Version { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public string Language { get; private set; }
    public string LegalBasis { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Deactivate(DateTimeOffset effectiveTo, DateTimeOffset updatedAt)
    {
        IsActive = false;
        EffectiveTo = effectiveTo;
        UpdatedAt = updatedAt;
    }

    public void UpdateContent(string title, string content, DateTimeOffset updatedAt)
    {
        Title = title;
        Content = content;
        UpdatedAt = updatedAt;
    }
}

public enum ConsentType
{
    TermsOfService,
    PrivacyPolicy,
    TelehealthConsent,
    DataProcessingConsent,
    MarketingConsent,
    CookieConsent,
    ResearchConsent,
    MinorConsent
}
