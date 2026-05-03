using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Analytics;
using Telehealth.Platform.Infrastructure.Analytics;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Tests.Analytics;

public class AnalyticsServiceTests
{
    private readonly PlatformDbContext _dbContext;
    private readonly AnalyticsService _service;

    public AnalyticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName: "AnalyticsTestDb")
            .Options;

        _dbContext = new PlatformDbContext(options);
        _service = new AnalyticsService(_dbContext);
    }

    [Fact]
    public async Task GetMetricAsync_ShouldReturnNull_WhenMetricNotFound()
    {
        var result = await _service.GetMetricAsync(
            "nonexistent",
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMetricAsync_ShouldReturnMetric_WhenExists()
    {
        var metric = DashboardMetrics.Create(
            "total_patients",
            100,
            "patients",
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow);

        _dbContext.DashboardMetrics.Add(metric);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetMetricAsync(
            "total_patients",
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow);

        Assert.NotNull(result);
        Assert.Equal("total_patients", result!.MetricType);
        Assert.Equal(100, result.Value);
    }

    [Fact]
    public async Task GetMetricsAsync_ShouldReturnMultipleMetrics()
    {
        _dbContext.DashboardMetrics.Add(DashboardMetrics.Create(
            "total_patients", 100, "patients", DateTimeOffset.UtcNow.AddDays(-7), DateTimeOffset.UtcNow));
        _dbContext.DashboardMetrics.Add(DashboardMetrics.Create(
            "total_patients", 120, "patients", DateTimeOffset.UtcNow.AddDays(-14), DateTimeOffset.UtcNow.AddDays(-7)));
        await _dbContext.SaveChangesAsync();

        var results = await _service.GetMetricsAsync(
            "total_patients",
            DateTimeOffset.UtcNow.AddDays(-14),
            DateTimeOffset.UtcNow);

        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task GenerateReportAsync_ShouldCreateReport()
    {
        var parameters = new Dictionary<string, object>
        {
            ["reportType"] = "consultation_summary",
            ["startDate"] = DateTimeOffset.UtcNow.AddDays(-30).ToString("o"),
            ["endDate"] = DateTimeOffset.UtcNow.ToString("o")
        };

        var report = await _service.GenerateReportAsync("consultation_summary", parameters);

        Assert.NotNull(report);
        Assert.Equal("consultation_summary", report.ReportType);
        Assert.Equal("Draft", report.Status);
    }

    [Fact]
    public async Task GetReportAsync_ShouldReturnReport_WhenExists()
    {
        var report = AnalyticsReport.Create(
            "test_report",
            "Test Report",
            "Test description",
            "json",
            "system");

        _dbContext.Reports.Add(report);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetReportAsync(report.Id);

        Assert.NotNull(result);
        Assert.Equal("test_report", result!.ReportType);
    }

    [Fact]
    public async Task GetConsultationAnalyticsAsync_ShouldReturnEmpty_WhenNoData()
    {
        var results = await _service.GetConsultationAnalyticsAsync(
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow);

        Assert.Empty(results);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}