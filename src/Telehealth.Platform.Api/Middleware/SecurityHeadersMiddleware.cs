namespace Telehealth.Platform.Api.Middleware;

/// <summary>
/// Adds OWASP-recommended security response headers to every HTTP response.
/// Must be registered before CORS so headers are present on preflight responses too.
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next, IConfiguration cfg, IWebHostEnvironment env)
{
    // For a pure JSON API, block all resource loading — only frame-ancestors matters
    private static readonly string ApiCsp =
        "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Content-Security-Policy
        var csp = cfg["Security:ContentSecurityPolicy"] ?? ApiCsp;
        headers.Append("Content-Security-Policy", csp);

        // Prevent MIME-type sniffing
        headers.Append("X-Content-Type-Options", "nosniff");

        // Block this API from being embedded in any frame
        headers.Append("X-Frame-Options", "DENY");

        // Legacy XSS filter (still respected by older browsers)
        headers.Append("X-XSS-Protection", "1; mode=block");

        // Limit referer leakage
        headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Grant only camera + microphone (needed for video consultations); deny everything else
        headers.Append("Permissions-Policy",
            "camera=(self), microphone=(self), geolocation=(), payment=(), usb=(), " +
            "display-capture=(self), screen-wake-lock=()");

        // HSTS — enforce HTTPS for 1 year; add to preload list once domains are final
        if (context.Request.IsHttps || env.IsProduction())
            headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");

        // Remove server fingerprinting headers
        headers.Remove("Server");
        headers.Remove("X-Powered-By");
        headers.Remove("X-AspNet-Version");
        headers.Remove("X-AspNetMvc-Version");

        await next(context);
    }
}
