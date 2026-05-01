namespace Telehealth.Platform.Domain.Doctors;

public sealed class DoctorLanguage
{
    public DoctorLanguage(Guid doctorProfileId, string languageCode)
    {
        DoctorProfileId = doctorProfileId;
        LanguageCode = languageCode;
    }

    public Guid DoctorProfileId { get; }

    public string LanguageCode { get; }
}
