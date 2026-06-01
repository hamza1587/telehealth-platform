using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Analytics.Data;
using Telehealth.Platform.Analytics.Domain.Models;

namespace Telehealth.Platform.Analytics.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AnalyticsDbContext _context;
    private readonly IDataWarehouseService _dataWarehouse;

    public AnalyticsService(AnalyticsDbContext context, IDataWarehouseService dataWarehouse)
    {
        _context = context;
        _dataWarehouse = dataWarehouse;
    }

    public async Task<List<DashboardMetrics>> GetCurrentMetricsAsync()
    {
        return await _context.DashboardMetrics.ToListAsync();
    }

    public async Task<List<DashboardMetrics>> GetMetricsByCategoryAsync(string category)
    {
        var filtered = await _context.DashboardMetrics
            .Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();
        return filtered;
    }

    public async Task<AnalyticsReport> GenerateReportAsync(string reportName, string queryDefinition, Guid createdBy)
    {
        var report = new AnalyticsReport(reportName, queryDefinition, createdBy);
        report.StartGenerating();
        _context.AnalyticsReports.Add(report);
        await _context.SaveChangesAsync();

        try
        {
            // Generate report using data warehouse
            var downloadUrl = await _dataWarehouse.ExecuteQueryAsync(queryDefinition);
            report.Complete(downloadUrl);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            report.Fail(ex.Message);
            await _context.SaveChangesAsync();
        }

        return report;
    }

    public async Task<AnalyticsReport?> GetReportAsync(Guid reportId)
    {
        return await _context.AnalyticsReports.FindAsync(reportId);
    }

    public async Task<List<AnalyticsReport>> GetUserReportsAsync(Guid userId)
    {
        var userReports = await _context.AnalyticsReports
            .Where(r => r.CreatedBy == userId)
            .ToListAsync();
        return userReports;
    }
}
