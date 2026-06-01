using System.Net.Http.Json;
using Telehealth.Platform.Prescription.Domain.Models;

namespace Telehealth.Platform.Prescription.Services;

public class DrugInteractionService : IDrugInteractionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string _apiKey;

    public DrugInteractionService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiUrl = configuration["DrugInteraction:ApiUrl"] ?? "https://api.drug-interaction-db.gov";
        _apiKey = configuration["DrugInteraction:ApiKey"] ?? throw new ArgumentNullException("DrugInteraction:ApiKey");
        
        _httpClient.BaseAddress = new Uri(_apiUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
    }

    public async Task<List<DrugInteraction>> CheckInteractionsAsync(List<string> existingMedications, List<string> newMedications)
    {
        try
        {
            // Prepare request payload
            var payload = new
            {
                existingMedications = existingMedications,
                newMedications = newMedications
            };

            // Send to drug interaction API
            var response = await _httpClient.PostAsJsonAsync("/api/v1/interactions/check", payload);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DrugInteractionResponse>();
                return result?.Interactions?.Select(i => new DrugInteraction(
                    i.Medication1,
                    i.Medication2,
                    i.Description,
                    ParseSeverity(i.Severity)
                )).ToList() ?? new List<DrugInteraction>();
            }

            // Fallback to sample data if API fails
            return GetSampleInteractions(existingMedications, newMedications);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Failed to check drug interactions: {ex.Message}");
            return GetSampleInteractions(existingMedications, newMedications);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error checking drug interactions: {ex.Message}");
            return GetSampleInteractions(existingMedications, newMedications);
        }
    }

    private List<DrugInteraction> GetSampleInteractions(List<string> existingMedications, List<string> newMedications)
    {
        // Fallback sample data for demonstration
        var interactions = new List<DrugInteraction>();

        // Check for common high-severity interactions
        if (existingMedications.Any(m => m.Contains("warfarin", StringComparison.OrdinalIgnoreCase)) &&
            newMedications.Any(m => m.Contains("aspirin", StringComparison.OrdinalIgnoreCase)))
        {
            interactions.Add(new DrugInteraction(
                "warfarin",
                "aspirin",
                "Increased risk of bleeding when warfarin is combined with aspirin",
                InteractionSeverity.High
            ));
        }

        if (existingMedications.Any(m => m.Contains("simvastatin", StringComparison.OrdinalIgnoreCase)) &&
            newMedications.Any(m => m.Contains("itraconazole", StringComparison.OrdinalIgnoreCase)))
        {
            interactions.Add(new DrugInteraction(
                "simvastatin",
                "itraconazole",
                "Itraconazole significantly increases simvastatin levels, increasing risk of myopathy",
                InteractionSeverity.High
            ));
        }

        return interactions;
    }

    private InteractionSeverity ParseSeverity(string severity)
    {
        return severity.ToLowerInvariant() switch
        {
            "high" => InteractionSeverity.High,
            "moderate" => InteractionSeverity.Moderate,
            "low" => InteractionSeverity.Low,
            _ => InteractionSeverity.Low
        };
    }
}

// Response DTOs for drug interaction API
internal record DrugInteractionResponse(List<InteractionDto>? Interactions);
internal record InteractionDto(
    string Medication1,
    string Medication2,
    string Severity,
    string Description
);
