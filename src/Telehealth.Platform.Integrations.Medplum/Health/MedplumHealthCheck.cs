using Microsoft.Extensions.Diagnostics.HealthChecks;
using Telehealth.Platform.Application.Abstractions.ClinicalRecords;

namespace Telehealth.Platform.Integrations.Medplum.Health;

public class MedplumHealthCheck : IHealthCheck
{
    private readonly IClinicalRecordGateway _gateway;

    public MedplumHealthCheck(IClinicalRecordGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isAvailable = await _gateway.IsAvailableAsync(cancellationToken);
            return isAvailable 
                ? HealthCheckResult.Healthy("Medplum is available") 
                : HealthCheckResult.Degraded("Medplum is not responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Medplum health check failed", ex);
        }
    }
}