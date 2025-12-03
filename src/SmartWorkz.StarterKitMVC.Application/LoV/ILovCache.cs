using SmartWorkz.StarterKitMVC.Domain.LoV;

namespace SmartWorkz.StarterKitMVC.Application.LoV;

/// <summary>
/// Cache abstraction for LoV data to improve performance.
/// </summary>
/// <example>
/// <code>
/// // Inject ILovCache via DI
/// var cached = await _cache.GetCategoriesAsync("lov:categories:en-US");
/// if (cached is null)
/// {
///     var categories = await LoadFromDatabaseAsync();
///     await _cache.SetCategoriesAsync("lov:categories:en-US", categories);
/// }
/// </code>
/// </example>
public interface ILovCache
{
    /// <summary>Gets cached categories by key.</summary>
    Task<IReadOnlyCollection<Category>?> GetCategoriesAsync(string cacheKey, CancellationToken ct = default);
    
    /// <summary>Caches categories with the specified key.</summary>
    Task SetCategoriesAsync(string cacheKey, IReadOnlyCollection<Category> categories, CancellationToken ct = default);
    
    /// <summary>Gets cached LoV items by key.</summary>
    Task<IReadOnlyCollection<LovItem>?> GetItemsAsync(string cacheKey, CancellationToken ct = default);
    
    /// <summary>Caches LoV items with the specified key.</summary>
    Task SetItemsAsync(string cacheKey, IReadOnlyCollection<LovItem> items, CancellationToken ct = default);
    
    /// <summary>Invalidates all cache entries matching the prefix.</summary>
    Task InvalidateAsync(string cacheKeyPrefix, CancellationToken ct = default);
}
