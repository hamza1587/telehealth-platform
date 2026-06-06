using System.Text.RegularExpressions;

namespace Telehealth.Platform.Api.Middleware;

/// <summary>
/// Rejects requests whose query-string values contain known attack patterns.
/// This is a defence-in-depth layer — EF Core parameterized queries already prevent
/// SQL injection, and model binding handles XSS at the output layer. This stops
/// clearly malicious requests before they reach any handler.
/// </summary>
public partial class InputSanitizationMiddleware(RequestDelegate next, ILogger<InputSanitizationMiddleware> logger)
{
    // Covers common XSS vectors, SQL injection fragments, and path traversal
    [GeneratedRegex(
        @"<script[\s>]|javascript\s*:|vbscript\s*:|on\w+\s*=|expression\s*\(|" +
        @";\s*(?:DROP|ALTER|TRUNCATE|INSERT|DELETE|UPDATE)\s|UNION\s+(?:ALL\s+)?SELECT|" +
        @"(?:\.\.[\\/]){2,}|%2e%2e%2f|%2e%2e/|\.\.%2f",
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex DangerousInputPattern();

    public async Task InvokeAsync(HttpContext context)
    {
        foreach (var param in context.Request.Query)
        {
            foreach (var value in param.Value)
            {
                if (value is not null && DangerousInputPattern().IsMatch(value))
                {
                    logger.LogWarning(
                        "Rejected request with suspicious query param '{Param}' from {IP}",
                        param.Key,
                        context.Connection.RemoteIpAddress);

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "Request contains invalid characters.",
                    });
                    return;
                }
            }
        }

        await next(context);
    }
}
