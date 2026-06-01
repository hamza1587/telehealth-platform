namespace Telehealth.Platform.Localization;

public interface ILocalizationService
{
    string Translate(string key, string language = "en");
    string Translate(string key, Dictionary<string, string> parameters, string language = "en");
    Task<Dictionary<string, string>> GetLanguageAsync(string language);
    Task<List<string>> GetSupportedLanguagesAsync();
    Task<bool> IsLanguageSupportedAsync(string language);
    Task AddTranslationAsync(string key, string language, string value);
}

public enum SupportedLanguage
{
    English,
    German,
    French,
    Spanish,
    Italian,
    Dutch,
    Polish,
    Portuguese,
    Swedish,
    Danish,
    Norwegian,
    Finnish,
    Greek,
    Czech,
    Hungarian,
    Romanian,
    Bulgarian,
    Croatian,
    Slovenian,
    Slovak
}
