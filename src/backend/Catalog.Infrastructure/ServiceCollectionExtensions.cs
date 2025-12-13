using Catalog.Core.Repositories;
using Catalog.Infrastructure.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.CosmosDb;
using Microsoft.Extensions.DependencyInjection;

using Catalog.Infrastructure.HealthChecks;
namespace Catalog.Infrastructure;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<CosmosClient>(sp =>
        {
            var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING")
                ?? throw new InvalidOperationException("COSMOS_CONNECTION_STRING not found");

            return new CosmosClient(connectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        });
        services.AddScoped(typeof(IBaseRepository<>), typeof(CosmosRepository<>));

        //Health check for Cosmos DB
        services.AddHealthChecks()
                .AddCheck<CosmosDbHealthCheck>("Infrastructure-cosmosdb");

        // Future: Cosmos DB, Repositories, External APIs
        return services;
    }
}
