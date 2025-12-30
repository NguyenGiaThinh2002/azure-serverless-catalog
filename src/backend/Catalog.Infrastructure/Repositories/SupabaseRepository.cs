using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Microsoft.Extensions.Logging;
using Npgsql;
using Dapper;
using System.Linq;

namespace Catalog.Infrastructure.Repositories;
public class SupabaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    private readonly string _connectionString;
    private readonly ILogger<SupabaseRepository<T>> _logger;
    private readonly string _tableName;

    public SupabaseRepository(string connectionString, ILogger<SupabaseRepository<T>> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
        _tableName = GetTableName(typeof(T));
    }

    private string GetTableName(Type entityType)
    {
        // Convert entity type name to snake_case table name with proper pluralization
        var typeName = entityType.Name;
        var plural = typeName.EndsWith("y") 
            ? typeName.Substring(0, typeName.Length - 1) + "ies" 
            : typeName + "s";
        return plural.ToLower(); // Product -> products, Category -> categories
    }

    public async Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Use Dapper's default mapping which handles PascalCase to snake_case conversion
        // We need to map properties manually for INSERT
        var properties = typeof(T).GetProperties();
        var columnNames = string.Join(", ", properties.Select(p => ToSnakeCase(p.Name)));
        var parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));

        var sql = $@"INSERT INTO {_tableName} ({columnNames}) 
                     VALUES ({parameterNames}) 
                     RETURNING *";

        // Configure Dapper to map snake_case columns to PascalCase properties
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        var result = await connection.QueryFirstOrDefaultAsync<T>(sql, entity);
        return result ?? entity;
    }

    public async Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var properties = typeof(T).GetProperties().Where(p => p.Name != "Id");
        var setClause = string.Join(", ", properties.Select(p => $"{ToSnakeCase(p.Name)} = @{p.Name}"));

        var sql = $@"UPDATE {_tableName} 
                     SET {setClause} 
                     WHERE id = @Id";

        DefaultTypeMap.MatchNamesWithUnderscores = true;
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(string id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $@"DELETE FROM {_tableName} WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $@"SELECT * FROM {_tableName} WHERE type = @Type ORDER BY created_at DESC";
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        return await connection.QueryAsync<T>(sql, new { Type = typeof(T).Name });
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $@"SELECT * FROM {_tableName} WHERE id = @Id";
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    private string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }

        return result.ToString();
    }
}

