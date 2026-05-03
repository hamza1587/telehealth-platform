using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Analytics;
using Telehealth.Platform.Domain.Analytics;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly PlatformDbContext _dbContext;

    public AnalyticsService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardMetrics?> GetMetricAsync(
        string metricType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DashboardMetrics
            .FirstOrDefaultAsync(m =>
                m.MetricType == metricType &&
                m.PeriodStart >= periodStart &&
                m.PeriodEnd <= periodEnd,
                cancellationToken);
    }

    public async Task<IEnumerable<DashboardMetrics>> GetMetricsAsync(
        string metricType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DashboardMetrics
            .Where(m =>
                m.MetricType == metricType &&
                m.PeriodStart >= periodStart &&
                m.PeriodEnd <= periodEnd)
            .OrderBy(m => m.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<AnalyticsReport> GenerateReportAsync(
        string reportType,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        var report = AnalyticsReport.Create(
            reportType,
            $"Generated Report - {reportType}",
            $"Analytics report for {reportType}",
            "json",
            "system",
            parameters);

        report.StartProcessing();

        await _dbContext.Reports.AddAsync(report, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            switch (reportType)
            {
                case "consultation_summary":
                    await GenerateConsultationSummaryReportAsync(report, parameters, cancellationToken);
                    break;
                case "revenue_report":
                    await GenerateRevenueReportAsync(report, parameters, cancellationToken);
                    break;
                case "patient_engagement":
                    await GeneratePatientEngagementReportAsync(report, parameters, cancellationToken);
                    break;
                case "doctor_performance":
                    await GenerateDoctorPerformanceReportAsync(report, parameters, cancellationToken);
                    break;
                case "platform_usage":
                    await GeneratePlatformUsageReportAsync(report, parameters, cancellationToken);
                    break;
                default:
                    report.Fail($"Unknown report type: {reportType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            report.Fail(ex.Message);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return report;
    }

    public async Task<AnalyticsReport?> GetReportAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reports
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);
    }

    public async Task<IEnumerable<ConsultationAnalytics>> GetConsultationAnalyticsAsync(
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConsultationAnalytics
            .Where(ca => ca.Date >= periodStart && ca.Date <= periodEnd)
            .OrderBy(ca => ca.Date)
            .ToListAsync(cancellationToken);
    }

    private async Task GenerateConsultationSummaryReportAsync(
        AnalyticsReport report,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var startDate = parameters.TryGetValue("startDate", out var sd) ? DateTimeOffset.Parse(sd.ToString()!) : DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = parameters.TryGetValue("endDate", out var ed) ? DateTimeOffset.Parse(ed.ToString()!) : DateTimeOffset.UtcNow;

        var analytics = await GetConsultationAnalyticsAsync(startDate, endDate, cancellationToken);
        var count = (long)analytics.Count();
        report.SetRecordCount(count);
        report.SetResultLocation($"reports/{report.Id}/consultation-summary");
        report.Complete(count);
    }

    private async Task GenerateRevenueReportAsync(
        AnalyticsReport report,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var startDate = parameters.TryGetValue("startDate", out var sd) ? DateTimeOffset.Parse(sd.ToString()!) : DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = parameters.TryGetValue("endDate", out var ed) ? DateTimeOffset.Parse(ed.ToString()!) : DateTimeOffset.UtcNow;

        var analytics = await GetConsultationAnalyticsAsync(startDate, endDate, cancellationToken);
        var count = (long)analytics.Count();
        report.SetRecordCount(count);
        report.SetResultLocation($"reports/{report.Id}/revenue-report");
        report.Complete(count);
    }

    private async Task GeneratePatientEngagementReportAsync(
        AnalyticsReport report,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var startDate = parameters.TryGetValue("startDate", out var sd) ? DateTimeOffset.Parse(sd.ToString()!) : DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = parameters.TryGetValue("endDate", out var ed) ? DateTimeOffset.Parse(ed.ToString()!) : DateTimeOffset.UtcNow;

        var analytics = await GetConsultationAnalyticsAsync(startDate, endDate, cancellationToken);
        var count = (long)analytics.Count();
        report.SetRecordCount(count);
        report.SetResultLocation($"reports/{report.Id}/patient-engagement");
        report.Complete(count);
    }

    private async Task GenerateDoctorPerformanceReportAsync(
        AnalyticsReport report,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var startDate = parameters.TryGetValue("startDate", out var sd) ? DateTimeOffset.Parse(sd.ToString()!) : DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = parameters.TryGetValue("endDate", out var ed) ? DateTimeOffset.Parse(ed.ToString()!) : DateTimeOffset.UtcNow;

        var analytics = await GetConsultationAnalyticsAsync(startDate, endDate, cancellationToken);
        var count = (long)analytics.Count();
        report.SetRecordCount(count);
        report.SetResultLocation($"reports/{report.Id}/doctor-performance");
        report.Complete(count);
    }

    private async Task GeneratePlatformUsageReportAsync(
        AnalyticsReport report,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var startDate = parameters.TryGetValue("startDate", out var sd) ? DateTimeOffset.Parse(sd.ToString()!) : DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = parameters.TryGetValue("endDate", out var ed) ? DateTimeOffset.Parse(ed.ToString()!) : DateTimeOffset.UtcNow;

        var analytics = await GetConsultationAnalyticsAsync(startDate, endDate, cancellationToken);
        var count = (long)analytics.Count();
        report.SetRecordCount(count);
        report.SetResultLocation($"reports/{report.Id}/platform-usage");
        report.Complete(count);
    }
}