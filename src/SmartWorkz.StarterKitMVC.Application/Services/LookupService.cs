using SmartWorkz.StarterKitMVC.Shared.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Abstractions;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using System.Text.Json;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Implementation of lookup management service with distributed caching.
/// Uses IDistributedCache for lookups with 24-hour TTL for stable data.
/// </summary>
public class LookupService : ILookupService
{
    private const string CurrencyCategoryKey = "CURRENCY";
    private const string LanguageCategoryKey = "LANGUAGE";
    private const string TimeZoneCategoryKey = "TIMEZONE";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    private readonly ILookupRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<LookupService> _logger;

    public LookupService(
        ILookupRepository repository,
        IDistributedCache cache,
        ILogger<LookupService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LookupDto>> GetCurrenciesAsync(string tenantId)
    {
        return await GetByCategoryInternalAsync(CurrencyCategoryKey, tenantId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LookupDto>> GetLanguagesAsync(string tenantId)
    {
        return await GetByCategoryInternalAsync(LanguageCategoryKey, tenantId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LookupDto>> GetTimeZonesAsync(string tenantId)
    {
        return await GetByCategoryInternalAsync(TimeZoneCategoryKey, tenantId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LookupDto>> GetByCategoryAsync(string categoryKey, string tenantId)
    {
        return await GetByCategoryInternalAsync(categoryKey, tenantId);
    }

    /// <inheritdoc />
    public async Task<LookupDto> UpsertAsync(LookupDto lookup)
    {
        if (lookup == null)
            throw new ArgumentNullException(nameof(lookup));

        try
        {
            // Perform repository upsert
            await _repository.UpsertAsync(lookup);

            // Invalidate category cache
            await InvalidateCategoryCache(lookup.CategoryKey, lookup.TenantId);

            _logger.LogInformation(
                "Lookup upserted: {CategoryKey} - {Key} for tenant {TenantId}",
                lookup.CategoryKey, lookup.Key, lookup.TenantId);

            return lookup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error upserting lookup: {CategoryKey} - {Key} for tenant {TenantId}",
                lookup.CategoryKey, lookup.Key, lookup.TenantId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            // Get the lookup first to know which cache to invalidate
            var lookup = await _repository.GetByIdAsync(id);
            if (lookup == null)
                return false;

            await _repository.DeleteAsync(id);

            // Invalidate category cache
            await InvalidateCategoryCache(lookup.CategoryKey, lookup.TenantId);

            _logger.LogInformation(
                "Lookup deleted: {LookupId} ({CategoryKey})",
                id, lookup.CategoryKey);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lookup: {LookupId}", id);
            throw;
        }
    }

    /// <summary>
    /// Internal method to get lookups by category with caching.
    /// Cache key: lookups_{categoryKey}_{tenantId}
    /// </summary>
    private async Task<IEnumerable<LookupDto>> GetByCategoryInternalAsync(string categoryKey, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(categoryKey))
            throw new ArgumentException("Category key cannot be empty", nameof(categoryKey));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        var cacheKey = GenerateCacheKey(categoryKey, tenantId);

        try
        {
            // Try to get from cache
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Cache hit for lookups: {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<List<LookupDto>>(cachedData) ?? new List<LookupDto>();
            }

            // Cache miss - fetch from repository
            _logger.LogDebug("Cache miss for lookups: {CacheKey}", cacheKey);
            var lookups = (await _repository.GetByCategoryAsync(categoryKey, tenantId))
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.DisplayName)
                .ToList();

            // Store in cache
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            };

            var serialized = JsonSerializer.Serialize(lookups);
            await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

            _logger.LogDebug("Cached {Count} lookups for category: {CategoryKey}", lookups.Count, categoryKey);

            return lookups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving lookups for category: {CategoryKey}, tenant: {TenantId}",
                categoryKey, tenantId);
            throw;
        }
    }

    /// <summary>
    /// Invalidates cache for a specific category and tenant.
    /// </summary>
    private async Task InvalidateCategoryCache(string categoryKey, string tenantId)
    {
        var cacheKey = GenerateCacheKey(categoryKey, tenantId);
        await _cache.RemoveAsync(cacheKey);
        _logger.LogDebug("Cache invalidated for: {CacheKey}", cacheKey);
    }

    /// <summary>
    /// Generates a cache key for lookup category.
    /// Format: lookups_{categoryKey}_{tenantId}
    /// </summary>
    private static string GenerateCacheKey(string categoryKey, string tenantId)
        => $"lookups_{categoryKey.ToLowerInvariant()}_{tenantId}";
}
