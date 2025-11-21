using Catalog.Core.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Catalog.Core.Repositories;
public class CosmosRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    private readonly CosmosClient _client;
    private readonly Container _container;
    private readonly ILogger<CosmosRepository<T>> _logger;

    public CosmosRepository(CosmosClient client, ILogger<CosmosRepository<T>> logger)
    {
        _client = client;
        _logger = logger;
        _container = _client.GetContainer("CatalogDb", "catalog");
    }

    public async Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        var response = await _container.CreateItemAsync(entity, new PartitionKey(entity.PartitionKey));
        return response.Resource;
    }
    public async Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _container.UpsertItemAsync(entity, new PartitionKey(entity.PartitionKey));
    }

    public async Task DeleteAsync(string id)
    {
        var pk = new PartitionKey($"{typeof(T).Name}|{id}");
        await _container.DeleteItemAsync<T>(id, pk);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = new QueryDefinition($"SELECT * FROM c WHERE c.type = '{typeof(T).Name}'");
        var iterator = _container.GetItemQueryIterator<T>(query);
        var results = new List<T>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<T> GetByIdAsync(string id)
    {
        try
        {
            var pk = new PartitionKey($"{typeof(T).Name}|{id}");
            var response = await _container.ReadItemAsync<T>(id, pk);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }


}

