using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Catalog.Infrastructure.HealthChecks;

public class SupabaseHealthCheck : IHealthCheck
{
    private readonly NpgsqlConnection _connection;

    public SupabaseHealthCheck(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                await _connection.OpenAsync(cancellationToken);
            }

            // Simple query to test connection
            var command = new NpgsqlCommand("SELECT 1", _connection);
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Supabase/PostgreSQL is reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Supabase/PostgreSQL is not reachable", ex);
        }
    }
}




