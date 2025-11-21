using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
namespace Catalog.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Catalog.Infrastructure.Repositories;
//using Catalog.Core.Repositories;

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
        services.AddScoped(typeof(IBaseRepository<>)

        // Future: Cosmos DB, Repositories, External APIs
        return services;
    }
}
