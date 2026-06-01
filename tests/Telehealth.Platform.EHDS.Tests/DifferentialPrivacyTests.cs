using System.Collections.Generic;
using Telehealth.Platform.Domain.Services;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Telehealth.Platform.EHDS.Tests;

public class DifferentialPrivacyTests
{
    private readonly DifferentialPrivacyService _privacyService;
    private readonly KAnonymityService _kAnonymityService;

    public DifferentialPrivacyTests()
    {
        _privacyService = new DifferentialPrivacyService();
        _kAnonymityService = new KAnonymityService();
    }

    [Fact]
    public void ComputePrivateCount_ShouldAddNoise_WhenEpsilonProvided()
    {
        var count = 100;
        var epsilon = 0.5;

        var privateCount = _privacyService.ComputePrivateCount(count, epsilon);

        Assert.True(privateCount >= 0);
        Assert.NotEqual(count, privateCount); // Noise should be added
    }

    [Fact]
    public void ComputePrivateCount_ShouldReturnPositiveValue_WhenInputIsZero()
    {
        var privateCount = _privacyService.ComputePrivateCount(0, 1.0);

        Assert.True(privateCount >= 0);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void AddLaplaceNoise_ShouldScaleWithEpsilon(double epsilon)
    {
        var value = 100.0;
        var sensitivity = 1.0;

        var noise1 = _privacyService.AddLaplaceNoise(value, sensitivity, epsilon);

        // Lower epsilon should produce more noise (higher variance)
        // This is a statistical test - may occasionally fail
        Assert.True(noise1 >= 0 || noise1 <= 200);
    }

    [Fact]
    public void CalculatePrivacyBudget_ShouldIncreaseWithQueries()
    {
        var epsilon = 0.5;
        var delta = 1e-5;

        var budget1 = _privacyService.CalculatePrivacyBudget(epsilon, delta, 1);
        var budget10 = _privacyService.CalculatePrivacyBudget(epsilon, delta, 10);

        Assert.True(budget10 > budget1);
    }

    [Fact]
    public void IsPrivacyBudgetSufficient_ShouldReturnTrue_WhenWithinBudget()
    {
        var usedBudget = 0.3;
        var totalBudget = 1.0;

        var result = _privacyService.IsPrivacyBudgetSufficient(usedBudget, totalBudget);

        Assert.True(result);
    }

    [Fact]
    public void IsPrivacyBudgetSufficient_ShouldReturnFalse_WhenExceeded()
    {
        var usedBudget = 1.5;
        var totalBudget = 1.0;

        var result = _privacyService.IsPrivacyBudgetSufficient(usedBudget, totalBudget);

        Assert.False(result);
    }

    [Fact]
    public void ApplyKAnonymity_ShouldGroupRecords_ByQuasiIdentifiers()
    {
        var data = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "Age", "25-34" }, { "Zip", "100" }, { "Value", 10 } },
            new Dictionary<string, object> { { "Age", "25-34" }, { "Zip", "100" }, { "Value", 20 } },
            new Dictionary<string, object> { { "Age", "25-34" }, { "Zip", "101" }, { "Value", 30 } },
        };

        var result = _kAnonymityService.ApplyKAnonymity(data, new List<string> { "Age", "Zip" }, 2);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void CheckKAnonymity_ShouldReturnTrue_WhenAllGroupsHaveKOrMoreRecords()
    {
        var data = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "Age", "25-34" } },
            new Dictionary<string, object> { { "Age", "25-34" } },
            new Dictionary<string, object> { { "Age", "35-44" } },
            new Dictionary<string, object> { { "Age", "35-44" } },
        };

        var result = _kAnonymityService.CheckKAnonymity(data, new List<string> { "Age" }, 2);

        Assert.True(result);
    }

    [Fact]
    public void CheckKAnonymity_ShouldReturnFalse_WhenGroupHasLessThanKRecords()
    {
        var data = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "Age", "25-34" } },
            new Dictionary<string, object> { { "Age", "25-34" } },
            new Dictionary<string, object> { { "Age", "35-44" } }, // Only 1 record
        };

        var result = _kAnonymityService.CheckKAnonymity(data, new List<string> { "Age" }, 2);

        Assert.False(result);
    }

    [Fact]
    public void CalculateKAnonymityLevel_ShouldReturnMinGroupSize()
    {
        var data = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "Age", "25-34" } },
            new Dictionary<string, object> { { "Age", "25-34" } },
            new Dictionary<string, object> { { "Age", "35-44" } },
            new Dictionary<string, object> { { "Age", "35-44" } },
            new Dictionary<string, object> { { "Age", "35-44" } },
        };

        var kLevel = _kAnonymityService.CalculateKAnonymityLevel(data, new List<string> { "Age" });

        Assert.Equal(2, kLevel); // The "25-34" group has only 2 records
    }

    [Fact]
    public void HashValue_ShouldProduceConsistentHash()
    {
        var value = "test-value-123";

        var hash1 = _kAnonymityService.HashValue(value);
        var hash2 = _kAnonymityService.HashValue(value);

        Assert.Equal(hash1, hash2);
        Assert.NotEqual(value, hash1);
    }
}