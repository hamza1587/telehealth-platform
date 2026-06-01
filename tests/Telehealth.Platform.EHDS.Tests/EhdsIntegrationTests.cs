using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telehealth.Platform.EHDS;

namespace Telehealth.Platform.EHDS.Tests;

public class EHDSIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EHDSIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Fhir_CapabilityStatement_ReturnsValidStatement()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/fhir");
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        Assert.True(json.RootElement.TryGetProperty("fhirVersion", out var fhirVersion));
        Assert.Equal("4.0.1", fhirVersion.GetString());
    }

    [Fact]
    public async Task CrossBorder_SupportedCountries_ReturnsEhdsCompliantCountries()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/crossborder/supported-countries");
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var countries = JsonSerializer.Deserialize<List<CountryResponse>>(content);
        
        Assert.NotNull(countries);
        Assert.Contains(countries, c => c.Code == "DE" && c.IsEhdsCompliant);
        Assert.Contains(countries, c => c.Code == "FR" && c.IsEhdsCompliant);
    }

    [Fact]
    public async Task ResearcherPortal_Login_RequiresValidInstitution()
    {
        var client = _factory.CreateClient();
        
        var loginRequest = new { InstitutionEmail = "invalid@example.com" };
        var content = StringContent(JsonSerializer.Serialize(loginRequest), "application/json");
        
        var response = await client.PostAsync("/api/researcher/login", content);
        
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeIdentification_PrivacyMetrics_ReturnsMetrics()
    {
        var client = _factory.CreateClient();
        
        var request = new { kAnonymityLevel = 5, epsilon = 1.0 };
        var content = StringContent(JsonSerializer.Serialize(request), "application/json");
        
        var response = await client.PostAsync("/api/deidentification/privacy-metrics", content);
        
        response.EnsureSuccessStatusCode();
    }

    private StringContent StringContent(string content, string mediaType)
    {
        return new StringContent(content, System.Text.Encoding.UTF8, mediaType);
    }
}

public record CountryResponse(string Code, string Name, bool IsEhdsCompliant);