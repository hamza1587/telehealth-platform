using System.Text.Json;

namespace Telehealth.Platform.Localization;

public class LocalizationService : ILocalizationService
{
    public string Translate(string key, string language = "en")
    {
        return Translate(key, null, language);
    }

    public string Translate(string key, Dictionary<string, string>? parameters, string language = "en")
    {
        var translations = GetDefaultTranslations(language);
        var value = translations.GetValueOrDefault(key) ?? key;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                value = value.Replace($"{{{param.Key}}}", param.Value);
            }
        }

        return value;
    }

    public Task<Dictionary<string, string>> GetLanguageAsync(string language)
    {
        return Task.FromResult(GetDefaultTranslations(language));
    }

    public Task<List<string>> GetSupportedLanguagesAsync()
    {
        return Task.FromResult(new List<string>
        {
            "en", "de", "fr", "es", "it", "nl",
            "pl", "pt", "sv", "da", "no", "fi",
            "el", "cs", "hu", "ro", "bg", "hr",
            "sl", "sk"
        });
    }

    public Task<bool> IsLanguageSupportedAsync(string language)
    {
        var supported = GetSupportedLanguagesAsync().Result;
        return Task.FromResult(supported.Contains(language.ToLowerInvariant()));
    }

    public Task AddTranslationAsync(string key, string language, string value)
    {
        return Task.CompletedTask;
    }

    private Dictionary<string, string> GetDefaultTranslations(string language)
    {
        return language.ToLowerInvariant() switch
        {
            "de" => GetGermanTranslations(),
            "fr" => GetFrenchTranslations(),
            "es" => GetSpanishTranslations(),
            "it" => GetItalianTranslations(),
            "nl" => GetDutchTranslations(),
            "pl" => GetPolishTranslations(),
            "pt" => GetPortugueseTranslations(),
            "sv" => GetSwedishTranslations(),
            "da" => GetDanishTranslations(),
            "no" => GetNorwegianTranslations(),
            "fi" => GetFinnishTranslations(),
            "el" => GetGreekTranslations(),
            "cs" => GetCzechTranslations(),
            "hu" => GetHungarianTranslations(),
            "ro" => GetRomanianTranslations(),
            "bg" => GetBulgarianTranslations(),
            "hr" => GetCroatianTranslations(),
            "sl" => GetSlovenianTranslations(),
            "sk" => GetSlovakTranslations(),
            _ => new Dictionary<string, string>
            {
                { "app.title", "Telehealth Platform" },
                { "app.welcome", "Welcome to the Telehealth Platform" },
                { "consultation.title", "Video Consultation" },
                { "consultation.join", "Join Consultation" },
                { "consultation.leave", "Leave Consultation" },
                { "fhir.patient", "Patient" },
                { "fhir.condition", "Condition" },
                { "fhir.medication", "Medication" },
                { "fhir.allergy", "Allergy" },
                { "fhir.immunization", "Immunization" },
                { "fhir.observation", "Observation" },
                { "fhir.procedure", "Procedure" },
                { "research.request", "Research Data Request" },
                { "research.export", "Export Data" },
                { "research.approve", "Approve Request" }
            }
        };
    }

    private Dictionary<string, string> GetGermanTranslations() => new()
    {
        { "app.title", "Telemedizin-Plattform" },
        { "app.welcome", "Willkommen bei der Telemedizin-Plattform" },
        { "consultation.title", "Videokonsultation" },
        { "consultation.join", "Konsultation beitreten" },
        { "consultation.leave", "Konsultation verlassen" }
    };

    private Dictionary<string, string> GetFrenchTranslations() => new()
    {
        { "app.title", "Plateforme de Télémédecine" },
        { "app.welcome", "Bienvenue sur la plateforme de télémédecine" },
        { "consultation.title", "Consultation Vidéo" },
        { "consultation.join", "Rejoindre la consultation" },
        { "consultation.leave", "Quitter la consultation" }
    };

    private Dictionary<string, string> GetSpanishTranslations() => new()
    {
        { "app.title", "Plataforma de Telemedicina" },
        { "app.welcome", "Bienvenido a la plataforma de telemedicina" },
        { "consultation.title", "Consulta por Video" },
        { "consultation.join", "Unirse a la consulta" },
        { "consultation.leave", "Dejar la consulta" }
    };

    private Dictionary<string, string> GetItalianTranslations() => new()
    {
        { "app.title", "Piattaforma di Telemedicina" },
        { "app.welcome", "Benvenuto nella piattaforma di telemedicina" },
        { "consultation.title", "Consulenza Video" },
        { "consultation.join", "Unisciti alla consulenza" },
        { "consultation.leave", "Lascia la consulenza" }
    };

    private Dictionary<string, string> GetDutchTranslations() => new()
    {
        { "app.title", "Telemedicijnplatform" },
        { "app.welcome", "Welkom bij het telemedicijnplatform" },
        { "consultation.title", "Videoconsultatie" },
        { "consultation.join", "Doe mee aan consultatie" },
        { "consultation.leave", "Verlaat consultatie" }
    };

    private Dictionary<string, string> GetPolishTranslations() => new()
    {
        { "app.title", "Platforma Telemedycyny" },
        { "app.welcome", "Witamy na platformie telemedycyny" },
        { "consultation.title", "Konsultacja wideo" },
        { "consultation.join", "Dołącz do konsultacji" },
        { "consultation.leave", "Opuść konsultację" }
    };

    private Dictionary<string, string> GetPortugueseTranslations() => new()
    {
        { "app.title", "Plataforma de Telemedicina" },
        { "app.welcome", "Bem-vindo à plataforma de telemedicina" },
        { "consultation.title", "Consulta por Vídeo" },
        { "consultation.join", "Juntar-se à consulta" },
        { "consultation.leave", "Sair da consulta" }
    };

    private Dictionary<string, string> GetSwedishTranslations() => new()
    {
        { "app.title", "Telemedicinplattform" },
        { "app.welcome", "Välkommen till telemedicinplattformen" },
        { "consultation.title", "Videokonsultation" },
        { "consultation.join", "Gå med i konsultationen" },
        { "consultation.leave", "Lämna konsultationen" }
    };

    private Dictionary<string, string> GetDanishTranslations() => new()
    {
        { "app.title", "Telemedicinplatform" },
        { "app.welcome", "Velkommen til telemedicinplatformen" },
        { "consultation.title", "Videokonsultation" },
        { "consultation.join", "Deltag i konsultation" },
        { "consultation.leave", "Forlad konsultation" }
    };

    private Dictionary<string, string> GetNorwegianTranslations() => new()
    {
        { "app.title", "Telemedisinplattform" },
        { "app.welcome", "Velkommen til telemedisinplattformen" },
        { "consultation.title", "Videokonsultasjon" },
        { "consultation.join", "Bli med i konsultasjon" },
        { "consultation.leave", "Forlat konsultasjon" }
    };

    private Dictionary<string, string> GetFinnishTranslations() => new()
    {
        { "app.title", "Telemdbonusalusta" },
        { "app.welcome", "Tervetuloa telemdbonusalustalle" },
        { "consultation.title", "Videoneuvottelu" },
        { "consultation.join", "Liity neuvotteluun" },
        { "consultation.leave", "Poistu neuvottelusta" }
    };

    private Dictionary<string, string> GetGreekTranslations() => new()
    {
        { "app.title", "Πλατφόρμα Τηλεδιαθεμαστικής" },
        { "app.welcome", "Καλώς ορίσατε στην πλατφόρμα τηλεδιαθεμαστικής" },
        { "consultation.title", "Βιντεοσυμβουλευτική" },
        { "consultation.join", "Συμμετοχή στη συμβουλευτική" },
        { "consultation.leave", "Αποχώρηση από τη συμβουλευτική" }
    };

    private Dictionary<string, string> GetCzechTranslations() => new()
    {
        { "app.title", "Telemedicínská platforma" },
        { "app.welcome", "Vítejte na telemedicínské platformě" },
        { "consultation.title", "Videa konzultace" },
        { "consultation.join", "Připojit se k konzultaci" },
        { "consultation.leave", "Opustit konzultaci" }
    };

    private Dictionary<string, string> GetHungarianTranslations() => new()
    {
        { "app.title", "Teleorvosi platform" },
        { "app.welcome", "Üdvözlettel a teleorvosi platformon" },
        { "consultation.title", "Videokonzultáció" },
        { "consultation.join", "Csatlakozás a konzultációhoz" },
        { "consultation.leave", "Elhagyás a konzultációból" }
    };

    private Dictionary<string, string> GetRomanianTranslations() => new()
    {
        { "app.title", "Platformă de Telemedicina" },
        { "app.welcome", "Bine ați venit pe platforma de telemedicina" },
        { "consultation.title", "Consultatie Video" },
        { "consultation.join", "Alătură-te consultatiei" },
        { "consultation.leave", "Părăsește consultatia" }
    };

    private Dictionary<string, string> GetBulgarianTranslations() => new()
    {
        { "app.title", "Телемедицинска платформа" },
        { "app.welcome", "Добре дошли в телемедицинската платформа" },
        { "consultation.title", "Видеоконсултация" },
        { "consultation.join", "Присъединяване към консултацията" },
        { "consultation.leave", "Покажи консултацията" }
    };

    private Dictionary<string, string> GetCroatianTranslations() => new()
    {
        { "app.title", "Telemedicinska platforma" },
        { "app.welcome", "Dobrodošli na telemedicinsku platformu" },
        { "consultation.title", "Video konzultacija" },
        { "consultation.join", "Pridruži se konzultaciji" },
        { "consultation.leave", "Napusti konzultaciju" }
    };

    private Dictionary<string, string> GetSlovenianTranslations() => new()
    {
        { "app.title", "Telemedicinska platforma" },
        { "app.welcome", "Dobrodošli na telemedicinsko platformo" },
        { "consultation.title", "Video konzultacija" },
        { "consultation.join", "Pridruži se konzultaciji" },
        { "consultation.leave", "Zapusti konzultacijo" }
    };

    private Dictionary<string, string> GetSlovakTranslations() => new()
    {
        { "app.title", "Telemedicínska platforma" },
        { "app.welcome", "Vitajte na telemedicínskej platforme" },
        { "consultation.title", "Video konzultácia" },
        { "consultation.join", "Pridať sa k konzultácii" },
        { "consultation.leave", "Opustiť konzultáciu" }
    };
}