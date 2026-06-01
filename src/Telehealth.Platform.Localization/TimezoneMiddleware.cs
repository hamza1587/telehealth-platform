using System.Text.Json;

namespace Telehealth.Platform.Localization;

public class TimezoneMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimezoneMiddleware> _logger;

    public TimezoneMiddleware(RequestDelegate next, ILogger<TimezoneMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var timezoneHeader = context.Request.Headers["X-Timezone"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(timezoneHeader))
        {
            try
            {
                var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneHeader);
                context.Items["Timezone"] = timezoneInfo;
            }
            catch (TimeZoneNotFoundException)
            {
                _logger.LogWarning("Invalid timezone provided: {Timezone}", timezoneHeader);
            }
        }

        await _next(context);
    }
}

public static class TimezoneExtensions
{
    public static TimeZoneInfo? GetRequestTimezone(this HttpContext context)
    {
        return context.Items["Timezone"] as TimeZoneInfo;
    }

    public static DateTimeOffset ToRequestTimezone(this DateTimeOffset dateTime, HttpContext context)
    {
        var timezone = context.GetRequestTimezone();
        if (timezone == null)
            return dateTime;

        return TimeZoneInfo.ConvertTime(dateTime, timezone);
    }
}