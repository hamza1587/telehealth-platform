using System.Security.Cryptography;

namespace Telehealth.Platform.Domain.Services;

public class DifferentialPrivacyService
{
    private readonly Random _random;

    public DifferentialPrivacyService()
    {
        _random = new Random();
    }

    /// <summary>
    /// Adds Laplace noise to a value for differential privacy
    /// </summary>
    public double AddLaplaceNoise(double value, double sensitivity, double epsilon)
    {
        var noise = SampleLaplace(0, sensitivity / epsilon);
        return value + noise;
    }

    /// <summary>
    /// Adds Laplace noise to an integer count
    /// </summary>
    public int AddLaplaceNoise(int value, double sensitivity, double epsilon)
    {
        var noise = SampleLaplace(0, sensitivity / epsilon);
        return (int)Math.Round(value + noise);
    }

    /// <summary>
    /// Computes a differentially private mean
    /// </summary>
    public double ComputePrivateMean(List<double> values, double epsilon)
    {
        if (values.Count == 0)
            return 0;

        var sensitivity = (values.Max() - values.Min()) / values.Count;
        var mean = values.Average();
        return AddLaplaceNoise(mean, sensitivity, epsilon);
    }

    /// <summary>
    /// Computes a differentially private count
    /// </summary>
    public int ComputePrivateCount(int count, double epsilon)
    {
        return AddLaplaceNoise(count, 1, epsilon);
    }

    /// <summary>
    /// Computes a differentially private sum
    /// </summary>
    public double ComputePrivateSum(List<double> values, double sensitivity, double epsilon)
    {
        var sum = values.Sum();
        return AddLaplaceNoise(sum, sensitivity, epsilon);
    }

    /// <summary>
    /// Applies differential privacy to a histogram
    /// </summary>
    public Dictionary<string, int> ApplyPrivacyToHistogram(
        Dictionary<string, int> histogram,
        double epsilon)
    {
        var privateHistogram = new Dictionary<string, int>();
        
        foreach (var (key, value) in histogram)
        {
            privateHistogram[key] = ComputePrivateCount(value, epsilon);
        }
        
        return privateHistogram;
    }

    /// <summary>
    /// Samples from Laplace distribution
    /// </summary>
    private double SampleLaplace(double location, double scale)
    {
        var u = _random.NextDouble() - 0.5;
        return location - scale * Math.Sign(u) * Math.Log(1 - 2 * Math.Abs(u));
    }

    /// <summary>
    /// Calculates the privacy budget usage
    /// </summary>
    public double CalculatePrivacyBudget(double epsilon, double delta, int queries)
    {
        // Advanced composition theorem
        return Math.Sqrt(2 * queries * Math.Log(1 / delta)) * epsilon;
    }

    /// <summary>
    /// Checks if privacy budget is sufficient
    /// </summary>
    public bool IsPrivacyBudgetSufficient(double usedBudget, double totalBudget)
    {
        return usedBudget <= totalBudget;
    }

    /// <summary>
    /// Calculates the optimal epsilon for a given privacy budget
    /// </summary>
    public double CalculateOptimalEpsilon(double totalBudget, int queries, double delta = 1e-5)
    {
        return totalBudget / Math.Sqrt(2 * queries * Math.Log(1 / delta));
    }
}
