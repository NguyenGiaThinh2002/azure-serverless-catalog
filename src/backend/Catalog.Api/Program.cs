// Catalog.Api/Program.cs
using Microsoft.Azure.Functions.Worker;
using Catalog.Core;
using Catalog.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Catalog.Api.HealthChecks;
using Catalog.Api.Middleware;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseWhen<SwaggerUIMiddleware>(context =>
            context.GetHttpContext()?.Request.Path.StartsWithSegments("/swagger") == true);
    })
    .ConfigureOpenApi()
    .ConfigureServices(services =>
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices();

        services.AddHealthChecks()
                .AddCheck<BasicHealthCheck>("basic_health");

        // Add Swagger services
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Catalog API", Version = "v1" });
        });
    })
    .Build();

host.Run();

