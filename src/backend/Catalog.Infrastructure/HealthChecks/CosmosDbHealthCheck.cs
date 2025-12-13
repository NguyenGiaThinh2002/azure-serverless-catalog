using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Catalog.Infrastructure.HealthChecks;

public class CosmosDbHealthCheck : IHealthCheck
{
    private readonly CosmosClient _cosmosClient;

    public CosmosDbHealthCheck(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cosmosClient.ReadAccountAsync();
            return HealthCheckResult.Healthy("Cosmos DB is reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cosmos DB is not reachable", ex);
        }
    }
}