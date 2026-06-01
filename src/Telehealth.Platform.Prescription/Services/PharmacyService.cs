using System.Net.Http.Json;

namespace Telehealth.Platform.Prescription.Services;

public class PharmacyService : IPharmacyService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string _apiKey;

    public PharmacyService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiUrl = configuration["PharmacyNetwork:ApiUrl"] ?? "https://api.pharmacy-network.gov";
        _apiKey = configuration["PharmacyNetwork:ApiKey"] ?? throw new ArgumentNullException("PharmacyNetwork:ApiKey");
        
        _httpClient.BaseAddress = new Uri(_apiUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
    }

    public async Task<List<Pharmacy>> GetNearbyPharmaciesAsync(string countryCode, string? city = null)
    {
        try
        {
            // Build query parameters
            var queryParams = $"?countryCode={countryCode}";
            if (!string.IsNullOrEmpty(city))
            {
                queryParams += $"&city={Uri.EscapeDataString(city)}";
            }

            // Query pharmacy network API
            var response = await _httpClient.GetAsync($"/api/v1/pharmacies/nearby{queryParams}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PharmacyResponse>();
                return result?.Pharmacies?.Select(p => new Pharmacy(
                    p.Id,
                    p.Name,
                    p.Address,
                    p.City,
                    p.CountryCode,
                    p.IsOnline,
                    p.AcceptsEPrescriptions
                )).ToList() ?? new List<Pharmacy>();
            }

            // Fallback to sample data if API fails
            return GetSamplePharmacies(countryCode, city);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Failed to get nearby pharmacies: {ex.Message}");
            return GetSamplePharmacies(countryCode, city);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error getting nearby pharmacies: {ex.Message}");
            return GetSamplePharmacies(countryCode, city);
        }
    }

    public async Task<Pharmacy?> GetPharmacyAsync(string pharmacyId)
    {
        try
        {
            // Query pharmacy network API
            var response = await _httpClient.GetAsync($"/api/v1/pharmacies/{pharmacyId}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PharmacyDetail>();
                return new Pharmacy(
                    result?.Id ?? pharmacyId,
                    result?.Name ?? string.Empty,
                    result?.Address ?? string.Empty,
                    result?.City ?? string.Empty,
                    result?.CountryCode ?? string.Empty,
                    result?.IsOnline ?? false,
                    result?.AcceptsEPrescriptions ?? false
                );
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Failed to get pharmacy: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error getting pharmacy: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SendToPharmacyAsync(string prescriptionId, string pharmacyId)
    {
        try
        {
            // Prepare request payload
            var payload = new
            {
                prescriptionId = prescriptionId,
                pharmacyId = pharmacyId
            };

            // Send to pharmacy network API
            var response = await _httpClient.PostAsJsonAsync("/api/v1/prescriptions/send", payload);
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Failed to send prescription to pharmacy: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error sending prescription to pharmacy: {ex.Message}");
            return false;
        }
    }

    private List<Pharmacy> GetSamplePharmacies(string countryCode, string? city = null)
    {
        // Fallback sample data for demonstration
        var pharmacies = new List<Pharmacy>
        {
            new Pharmacy("PH-001", "Central Pharmacy", "123 Main St", "Berlin", "DE", true, true),
            new Pharmacy("PH-002", "Health Plus", "456 Oak Ave", "Munich", "DE", true, true),
            new Pharmacy("PH-003", "MediCare", "789 Pine Rd", "Hamburg", "DE", true, true),
            new Pharmacy("PH-004", "City Drugs", "321 Elm St", "Frankfurt", "DE", true, true),
            new Pharmacy("PH-005", "Wellness Pharmacy", "654 Maple Dr", "Cologne", "DE", true, true)
        };

        pharmacies = pharmacies
            .Where(p => p.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase))
            .Where(p => p.IsOnline && p.AcceptsEPrescriptions)
            .ToList();

        if (!string.IsNullOrEmpty(city))
        {
            pharmacies = pharmacies
                .Where(p => p.City.Equals(city, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return pharmacies;
    }
}

// Response DTOs for pharmacy network API
internal record PharmacyResponse(List<PharmacyDto>? Pharmacies);
internal record PharmacyDto(
    string Id,
    string Name,
    string Address,
    string City,
    string CountryCode,
    bool IsOnline,
    bool AcceptsEPrescriptions
);
internal record PharmacyDetail(
    string Id,
    string Name,
    string Address,
    string City,
    string CountryCode,
    bool IsOnline,
    bool AcceptsEPrescriptions
);
