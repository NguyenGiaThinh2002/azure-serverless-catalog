using Catalog.Core.Repositories;
using Catalog.Core.Services;
using Catalog.Infrastructure.Repositories;
using Catalog.Infrastructure.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Catalog.Infrastructure.HealthChecks;
using Supabase;
using System.Linq;

namespace Catalog.Infrastructure;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Store configuration for lazy evaluation (Azure Functions may load env vars after service registration)
        services.AddSingleton<IConfiguration?>(configuration);
        
        // Use factory pattern to read connection string lazily when first needed
        services.AddSingleton<string>(sp =>
        {
            // Azure Functions loads local.settings.json Values section as environment variables
            // Try multiple sources to be robust
            string? connectionString = null;
            
            // Try environment variable first (Azure Functions loads local.settings.json here)
            connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING");
            
            // If not found, try configuration (for other scenarios)
            if (string.IsNullOrEmpty(connectionString))
            {
                var config = sp.GetService<IConfiguration>();
                if (config != null)
                {
                    connectionString = config["SUPABASE_CONNECTION_STRING"] 
                        ?? config["Values:SUPABASE_CONNECTION_STRING"];
                }
            }
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "SUPABASE_CONNECTION_STRING not found. " +
                    "Please ensure it's set in local.settings.json (Values section) or as an environment variable. " +
                    "Current environment variables: " + string.Join(", ", Environment.GetEnvironmentVariables().Keys.Cast<string>().Where(k => k.Contains("SUPABASE"))));
            }
            
            return connectionString;
        });

        // Create a factory for NpgsqlConnection for health checks
        services.AddSingleton<NpgsqlConnection>(sp =>
        {
            var connString = sp.GetRequiredService<string>();
            return new NpgsqlConnection(connString);
        });

        services.AddScoped(typeof(IBaseRepository<>), typeof(SupabaseRepository<>));

        // Configure Supabase client for authentication
        services.AddSingleton<SupabaseClient>(sp =>
        {
            var config = sp.GetService<IConfiguration>();
            var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL")
                ?? config?["SUPABASE_URL"]
                ?? throw new InvalidOperationException("SUPABASE_URL not found");
            
            var supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY")
                ?? config?["SUPABASE_ANON_KEY"]
                ?? throw new InvalidOperationException("SUPABASE_ANON_KEY not found");

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };

            return new SupabaseClient(supabaseUrl, supabaseKey, options);
        });

        // Register authentication service
        services.AddScoped<IAuthService>(sp =>
        {
            var supabaseClient = sp.GetRequiredService<SupabaseClient>();
            var logger = sp.GetRequiredService<ILogger<AuthService>>();
            var config = sp.GetService<IConfiguration>();
            return new AuthService(supabaseClient, logger, config);
        });

        // Health check for Supabase/PostgreSQL
        services.AddHealthChecks()
                .AddCheck<SupabaseHealthCheck>("Infrastructure-supabase");

        return services;
    }
}
