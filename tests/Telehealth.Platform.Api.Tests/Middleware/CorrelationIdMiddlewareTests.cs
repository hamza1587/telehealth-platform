using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Telehealth.Platform.Api.Middleware;

namespace Telehealth.Platform.Api.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_SetsCorrelationId_WhenNotPresent()
    {
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(next: (ctx) => Task.CompletedTask, logger);

        var context = new DefaultHttpContext();
        await middleware.InvokeAsync(context);

        Assert.NotNull(context.Items["CorrelationId"]);
        Assert.NotNull(context.Response.Headers["X-Correlation-ID"]);
    }

    [Fact]
    public async Task InvokeAsync_UsesExistingCorrelationId_FromHeader()
    {
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(next: (ctx) => Task.CompletedTask, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "test-correlation-123";

        await middleware.InvokeAsync(context);

        Assert.Equal("test-correlation-123", context.Items["CorrelationId"]);
        Assert.Equal("test-correlation-123", context.Response.Headers["X-Correlation-ID"]);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextDelegate()
    {
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(next: (ctx) => { nextCalled = true; return Task.CompletedTask; }, logger);

        var context = new DefaultHttpContext();
        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }
}