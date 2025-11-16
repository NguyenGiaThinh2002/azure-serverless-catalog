using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Catalog.Api.HealthChecks;

public class BasicHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("API is running"));
    }
}