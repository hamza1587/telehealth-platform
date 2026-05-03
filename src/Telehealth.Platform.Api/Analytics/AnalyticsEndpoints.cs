using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Telehealth.Platform.Application.Abstractions.Analytics;
using Telehealth.Platform.Domain.Analytics;

namespace Telehealth.Platform.Api.Analytics;

public static class AnalyticsEndpoints
{
    public static RouteGroupBuilder MapAnalyticsEndpoints(this RouteGroupBuilder builder)
    {
        var analytics = builder.MapGroup("/analytics").WithTags("Analytics");

        analytics.MapGet("/metrics/{metricType}", GetMetric)
            .RequireAuthorization("RequireAdmin")
            .WithName("GetMetric")
            .WithDescription("Get a specific metric by type and period");

        analytics.MapGet("/metrics", GetMetrics)
            .RequireAuthorization("RequireAdmin")
            .WithName("GetMetrics")
            .WithDescription("Get all metrics for a specific type and period");

        analytics.MapPost("/reports", GenerateReport)
            .RequireAuthorization("RequireAdmin")
            .WithName("GenerateReport")
            .WithDescription("Generate a new analytics report");

        analytics.MapGet("/reports/{reportId}", GetReport)
            .RequireAuthorization("RequireAdmin")
            .WithName("GetReport")
            .WithDescription("Get a specific report by ID");

        analytics.MapGet("/consultation-analytics", GetConsultationAnalytics)
            .RequireAuthorization("RequireAdmin")
            .WithName("GetConsultationAnalytics")
            .WithDescription("Get consultation analytics for a period");

        return builder;
    }

    private static async Task<IResult> GetMetric(
        string metricType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var metric = await analyticsService.GetMetricAsync(metricType, periodStart, periodEnd, cancellationToken);
        return metric is not null ? Results.Ok(metric) : Results.NotFound();
    }

    private static async Task<IResult> GetMetrics(
        string metricType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        if (periodStart > periodEnd)
        {
            return Results.BadRequest("Start date must be before end date");
        }

        var metrics = await analyticsService.GetMetricsAsync(metricType, periodStart, periodEnd, cancellationToken);
        return Results.Ok(metrics);
    }

    private static async Task<IResult> GenerateReport(
        Dictionary<string, object> parameters,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        if (!parameters.ContainsKey("reportType"))
        {
            return Results.BadRequest("reportType parameter is required");
        }

        var reportType = parameters["reportType"].ToString() ?? string.Empty;
        var report = await analyticsService.GenerateReportAsync(reportType, parameters, cancellationToken);
        return Results.Ok(report);
    }

    private static async Task<IResult> GetReport(
        Guid reportId,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        var report = await analyticsService.GetReportAsync(reportId, cancellationToken);
        return report is not null ? Results.Ok(report) : Results.NotFound();
    }

    private static async Task<IResult> GetConsultationAnalytics(
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        IAnalyticsService analyticsService,
        CancellationToken cancellationToken)
    {
        if (periodStart > periodEnd)
        {
            return Results.BadRequest("Start date must be before end date");
        }

        var analytics = await analyticsService.GetConsultationAnalyticsAsync(periodStart, periodEnd, cancellationToken);
        return Results.Ok(analytics);
    }
}