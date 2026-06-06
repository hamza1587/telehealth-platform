using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Telehealth.Platform.Api.Middleware;

public class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseCachingMiddleware> _logger;

    public ResponseCachingMiddleware(RequestDelegate next, ILogger<ResponseCachingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cacheableMethods = new[] { "GET" };
        var cacheablePaths = new[]
        {
            "/platform/info",
            "/health/live",
            "/health/ready"
        };

        if (!cacheableMethods.Contains(context.Request.Method) ||
            !cacheablePaths.Any(p => context.Request.Path.StartsWithSegments(p)))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        var responseBody = new HttpResponseBodyStream(context.Response);
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            if (context.Response.StatusCode == 200)
            {
                context.Response.Headers["Cache-Control"] = "public, max-age=300";
            }

            // Copy buffered response back to the original stream
            context.Response.Body = originalBody;
            await responseBody.CopyToAsync(context.Response);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private class HttpResponseBodyStream : Stream
    {
        private readonly HttpResponse _response;
        private readonly MemoryStream _memoryStream;

        public HttpResponseBodyStream(HttpResponse response)
        {
            _response = response;
            _memoryStream = new MemoryStream();
        }

        public override bool CanRead => _memoryStream.CanRead;
        public override bool CanSeek => _memoryStream.CanSeek;
        public override bool CanWrite => _memoryStream.CanWrite;
        public override long Length => _memoryStream.Length;
        public override long Position { get => _memoryStream.Position; set => _memoryStream.Position = value; }

        public override void Flush() => _memoryStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _memoryStream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _memoryStream.Seek(offset, origin);
        public override void SetLength(long value) => _memoryStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _memoryStream.Write(buffer, offset, count);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            _memoryStream.WriteAsync(buffer, offset, count, cancellationToken);

        public async Task CopyToAsync(HttpResponse response)
        {
            _memoryStream.Position = 0;
            await _memoryStream.CopyToAsync(response.Body);
        }
    }
}