using Telehealth.Platform.Analytics.Domain.Models;

namespace Telehealth.Platform.Analytics.Services;

public interface IAnalyticsService
{
    Task<List<DashboardMetrics>> GetCurrentMetricsAsync();
    Task<List<DashboardMetrics>> GetMetricsByCategoryAsync(string category);
    Task<AnalyticsReport> GenerateReportAsync(string reportName, string queryDefinition, Guid createdBy);
    Task<AnalyticsReport?> GetReportAsync(Guid reportId);
    Task<List<AnalyticsReport>> GetUserReportsAsync(Guid userId);
}
