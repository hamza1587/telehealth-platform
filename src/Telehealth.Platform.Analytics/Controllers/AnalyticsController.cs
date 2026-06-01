using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Analytics.Domain.Models;
using Telehealth.Platform.Analytics.Services;

namespace Telehealth.Platform.Analytics.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<List<DashboardMetrics>>> GetCurrentMetrics()
    {
        try
        {
            var metrics = await _analyticsService.GetCurrentMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current metrics");
            return StatusCode(500, new { error = "Failed to get metrics" });
        }
    }

    [HttpGet("metrics/category/{category}")]
    public async Task<ActionResult<List<DashboardMetrics>>> GetMetricsByCategory(string category)
    {
        try
        {
            var metrics = await _analyticsService.GetMetricsByCategoryAsync(category);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics by category");
            return StatusCode(500, new { error = "Failed to get metrics" });
        }
    }

    [HttpPost("reports")]
    public async Task<ActionResult<AnalyticsReport>> GenerateReport([FromBody] GenerateReportRequest request)
    {
        try
        {
            var report = await _analyticsService.GenerateReportAsync(request.ReportName, request.QueryDefinition, request.CreatedBy);
            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report");
            return StatusCode(500, new { error = "Failed to generate report" });
        }
    }

    [HttpGet("reports/{id}")]
    public async Task<ActionResult<AnalyticsReport>> GetReport(Guid id)
    {
        try
        {
            var report = await _analyticsService.GetReportAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report");
            return StatusCode(500, new { error = "Failed to get report" });
        }
    }

    [HttpGet("reports/user/{userId}")]
    public async Task<ActionResult<List<AnalyticsReport>>> GetUserReports(Guid userId)
    {
        try
        {
            var reports = await _analyticsService.GetUserReportsAsync(userId);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user reports");
            return StatusCode(500, new { error = "Failed to get reports" });
        }
    }
}

public record GenerateReportRequest(string ReportName, string QueryDefinition, Guid CreatedBy);
