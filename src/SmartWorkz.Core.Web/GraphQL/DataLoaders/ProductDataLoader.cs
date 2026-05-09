namespace SmartWorkz.Core.Web.GraphQL.DataLoaders;

/// <summary>
/// DataLoader for batching Product queries to prevent N+1 query problems.
/// Groups product lookups by ID and loads them in a single batch operation.
/// Note: Full DataLoader implementation requires HotChocolate.DataLoader package
/// and would integrate with a repository pattern for actual data loading.
/// </summary>
public class ProductDataLoader
{
    /// <summary>
    /// Configuration class for DataLoader batch operations.
    /// Enables tracking of batched requests for efficient data fetching.
    /// </summary>
    public class DataLoaderConfig
    {
        public int BatchSize { get; set; } = 100;
        public int CacheDurationMs { get; set; } = 60000;
    }

    private readonly DataLoaderConfig _config;

    public ProductDataLoader(DataLoaderConfig? config = null)
    {
        _config = config ?? new DataLoaderConfig();
    }

    /// <summary>
    /// Gets the configuration for this DataLoader.
    /// </summary>
    public DataLoaderConfig GetConfig() => _config;

    /// <summary>
    /// Simulates batch loading of products.
    /// In a real implementation, this would fetch from a repository.
    /// </summary>
    public async Task<Dictionary<string, object>> LoadProductsByIdsAsync(IEnumerable<string> ids)
    {
        // Placeholder: In production, would batch fetch from repository
        var result = new Dictionary<string, object>();
        foreach (var id in ids)
        {
            result[id] = new { id, name = $"Product {id}", price = 99.99 };
        }
        return result;
    }
}
