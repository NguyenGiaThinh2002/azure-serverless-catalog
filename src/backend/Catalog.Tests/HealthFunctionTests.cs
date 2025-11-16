// Catalog.Tests/HealthFunctionTests.cs
using Catalog.Api.HealthChecks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Catalog.Tests;

public class HealthFunctionTests
{
    [Fact]
    public void DI_Resolves_HealthCheckService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureServices(s => s.AddHealthChecks().AddCheck<BasicHealthCheck>("basic_health"))
            .Build();

        var service = host.Services.GetRequiredService<HealthCheckService>();
        Assert.NotNull(service);
    }
}