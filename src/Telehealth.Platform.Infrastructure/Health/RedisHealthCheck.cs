using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Telehealth.Platform.Infrastructure.Health;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IOptions<RedisOptions> _options;

    public RedisHealthCheck(
        IConnectionMultiplexer? redis,
        IOptions<RedisOptions> options)
    {
        _redis = redis;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            return HealthCheckResult.Unhealthy("Redis is not connected");
        }

        try
        {
            var db = _redis.GetDatabase();
            var pingResult = await db.PingAsync();
            
            if (pingResult <= TimeSpan.FromSeconds(1))
            {
                return HealthCheckResult.Healthy($"Redis connected, latency: {pingResult.TotalMilliseconds}ms");
            }
            
            return HealthCheckResult.Degraded($"Redis connected but slow response: {pingResult.TotalMilliseconds}ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis health check failed", ex);
        }
    }
}

public class RedisOptions
{
    public const string SectionName = "Redis";

    public string Configuration { get; set; } = "localhost:6379";
    public bool Enabled { get; set; } = true;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}