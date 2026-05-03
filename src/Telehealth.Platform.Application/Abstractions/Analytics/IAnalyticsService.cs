using Telehealth.Platform.Domain.Analytics;

namespace Telehealth.Platform.Application.Abstractions.Analytics;

public interface IAnalyticsService
{
    Task<DashboardMetrics?> GetMetricAsync(string metricType, DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken cancellationToken = default);
    Task<IEnumerable<DashboardMetrics>> GetMetricsAsync(string metricType, DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken cancellationToken = default);
    Task<AnalyticsReport> GenerateReportAsync(string reportType, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
    Task<AnalyticsReport?> GetReportAsync(Guid reportId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConsultationAnalytics>> GetConsultationAnalyticsAsync(DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken cancellationToken = default);
}