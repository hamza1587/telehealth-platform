using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Telehealth.Platform.Localization;

public class TimezoneMiddleware
{
    private readonly Func<IDictionary<string, object>, Task> _next;
    private readonly ILogger<TimezoneMiddleware> _logger;

    public TimezoneMiddleware(Func<IDictionary<string, object>, Task> next, ILogger<TimezoneMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(IDictionary<string, object> context)
    {
        if (context.TryGetValue("Headers", out var headersObj) && headersObj is IDictionary<string, string> headers)
        {
            var timezoneHeader = headers.ContainsKey("X-Timezone") ? headers["X-Timezone"].ToString() : string.Empty;

            if (!string.IsNullOrEmpty(timezoneHeader))
            {
                try
                {
                    var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneHeader);
                    context["Timezone"] = timezoneInfo;
                }
                catch (TimeZoneNotFoundException)
                {
                    _logger.LogWarning("Invalid timezone provided: {Timezone}", timezoneHeader);
                }
            }
        }

        await _next(context);
    }
}

public static class TimezoneExtensions
{
    public static TimeZoneInfo? GetRequestTimezone(this IDictionary<string, object> context)
    {
        return context["Timezone"] as TimeZoneInfo;
    }

    public static DateTimeOffset ToRequestTimezone(this DateTimeOffset dateTime, IDictionary<string, object> context)
    {
        var timezone = context.GetRequestTimezone();
        if (timezone == null)
            return dateTime;

        return TimeZoneInfo.ConvertTime(dateTime, timezone);
    }
}