namespace Telehealth.Platform.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        var (statusCode, title, safeDetail) = exception switch
        {
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized", "You are not authorized to perform this action."),
            InvalidOperationException ioe when ioe.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) =>
                (StatusCodes.Status404NotFound, "Not Found", ioe.Message),
            InvalidOperationException ioe when ioe.Message.Contains("Insufficient", StringComparison.OrdinalIgnoreCase) =>
                (StatusCodes.Status422UnprocessableEntity, "Insufficient Resources", ioe.Message),
            ArgumentException ae =>
                (StatusCodes.Status400BadRequest, "Bad Request", ae.Message),
            OperationCanceledException =>
                (StatusCodes.Status499ClientClosedRequest, "Request Cancelled", "The request was cancelled."),
            _ =>
                (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred. Please try again.")
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception,
                "[{CorrelationId}] Unhandled {ExceptionType}: {Message}",
                correlationId, exception.GetType().Name, exception.Message);
        }
        else
        {
            _logger.LogWarning(exception,
                "[{CorrelationId}] {StatusCode} {Title}: {Message}",
                correlationId, statusCode, title, exception.Message);
        }

        if (context.Response.HasStarted) return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            detail = safeDetail,
            correlationId,
            traceId = context.TraceIdentifier
        });
    }
}
