// Catalog.Api/Program.cs
using Microsoft.Azure.Functions.Worker;
using Catalog.Core;
using Catalog.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Catalog.Api.HealthChecks;
using Catalog.Api.Middleware;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Catalog.Infrastructure.HealthChecks;
using Catalog.Api.Middleware;
using Microsoft.Extensions.DependencyInjection;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureFunctionsWebApplication(builder =>
    {
        // Add JWT authentication middleware
        builder.UseMiddleware<JwtAuthMiddleware>();
        
        builder.UseWhen<SwaggerUIMiddleware>(context =>
            context.GetHttpContext()?.Request.Path.StartsWithSegments("/swagger") == true);
    })
    .ConfigureOpenApi()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices(context.Configuration);

        services.AddHealthChecks()
                .AddCheck<BasicHealthCheck>("basic_health")
                .AddCheck<SupabaseHealthCheck>("catalog_supabase");

        // Add Swagger services
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Catalog API", Version = "v1" });
        });
    })
    .Build();

host.Run();

//// src/backend/Catalog.Api/Program.cs
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Extensions.Hosting;
//using Catalog.Core;
//using Catalog.Infrastructure;
//using Catalog.Api.HealthChecks;
//using Microsoft.Extensions.Diagnostics.HealthChecks;
//using Catalog.Api.Middleware;
//using Microsoft.Azure.Functions.Worker.Extensions.OpenApi;

//var host = Host.CreateDefaultBuilder(args)
//    .ConfigureFunctionsWebApplication(worker =>
//    {
//        // Swagger UI only in development (cleanest .NET 8 way)
//        worker.UseMiddleware<SwaggerUIMiddleware>();
//    })
//    .ConfigureServices(services =>
//    {
//        // Core + Infrastructure registration
//        services.AddApplicationServices();
//        services.AddInfrastructureServices();

//        services.AddHealthChecks()
//                .AddCheck<BasicHealthCheck>("basic_health")
//                .AddAzureCosmosDB(                       // built-in health check from Microsoft.Azure.Cosmos
//                    //cosmosClientProvider: sp => sp.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>(),
//                    name: "cosmosdb",
//                    tags: new[] { "db", "cosmos" });

//        // OpenAPI / Swagger (official .NET 8 isolated package)
//        services.AddOpenApi();   // comes from Microsoft.Azure.Functions.Worker.Extensions.OpenApi
//    })
//    .Build();
 
//host.Run();
