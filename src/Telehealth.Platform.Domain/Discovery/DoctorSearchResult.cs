using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Discovery;

/// <summary>
/// Search result for doctor discovery.
/// </summary>
public sealed class DoctorSearchResult
{
    public DoctorSearchResult(
        Guid doctorProfileId,
        string displayName,
        string primarySpecialty,
        List<string> specialties,
        List<string> languages,
        string countryCode,
        int yearsOfExperience,
        decimal pricePerSecond,
        string currency,
        double rating,
        int reviewCount,
        bool isAvailableNow,
        bool isInstantAvailable,
        string? nextAvailableSlot,
        string? photoUrl)
    {
        DoctorProfileId = doctorProfileId;
        DisplayName = displayName;
        PrimarySpecialty = primarySpecialty;
        Specialties = specialties;
        Languages = languages;
        CountryCode = countryCode;
        YearsOfExperience = yearsOfExperience;
        PricePerSecond = pricePerSecond;
        Currency = currency;
        Rating = rating;
        ReviewCount = reviewCount;
        IsAvailableNow = isAvailableNow;
        IsInstantAvailable = isInstantAvailable;
        NextAvailableSlot = nextAvailableSlot;
        PhotoUrl = photoUrl;
    }

    public Guid DoctorProfileId { get; }
    public string DisplayName { get; }
    public string PrimarySpecialty { get; }
    public List<string> Specialties { get; }
    public List<string> Languages { get; }
    public string CountryCode { get; }
    public int YearsOfExperience { get; }
    public decimal PricePerSecond { get; }
    public string Currency { get; }
    public double Rating { get; }
    public int ReviewCount { get; }
    public bool IsAvailableNow { get; }
    public bool IsInstantAvailable { get; }
    public string? NextAvailableSlot { get; }
    public string? PhotoUrl { get; }

    public decimal CalculatePricePerMinute()
    {
        return PricePerSecond * 60;
    }
}

/// <summary>
/// Search filters for doctor discovery.
/// </summary>
public sealed class DoctorSearchFilters
{
    public string? Specialty { get; set; }
    public string? Language { get; set; }
    public string? CountryCode { get; set; }
    public decimal? MaxPricePerSecond { get; set; }
    public int? MinExperienceYears { get; set; }
    public bool? IsAvailableNow { get; set; }
    public bool? IsInstantAvailable { get; set; }
    public string? ConsultationMode { get; set; }
    public DateTimeOffset? AvailableFrom { get; set; }
    public DateTimeOffset? AvailableTo { get; set; }
    public string? Gender { get; set; }
    public double? MinRating { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}

/// <summary>
/// Paginated search results.
/// </summary>
public sealed class DoctorSearchResults
{
    public DoctorSearchResults(
        List<DoctorSearchResult> items,
        int totalCount,
        int page,
        int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public List<DoctorSearchResult> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
