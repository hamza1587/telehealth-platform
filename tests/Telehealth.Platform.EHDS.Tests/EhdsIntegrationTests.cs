using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

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
}