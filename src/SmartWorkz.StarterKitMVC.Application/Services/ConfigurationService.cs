using SmartWorkz.StarterKitMVC.Shared.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using System.Text.Json;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Implementation of configuration management service with caching.
/// Provides type-safe access to application settings with 1-hour cache TTL.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private readonly IConfigurationRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(
        IConfigurationRepository repository,
        IDistributedCache cache,
        ILogger<ConfigurationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ConfigurationDto?> GetByKeyAsync(string key, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be empty", nameof(key));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        var cacheKey = GenerateCacheKey(key, tenantId);

        try
        {
            // Try cache first
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Cache hit for configuration: {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<ConfigurationDto>(cachedData);
            }

            // Cache miss - fetch from repository
            _logger.LogDebug("Cache miss for configuration: {CacheKey}", cacheKey);
            var config = await _repository.GetByKeyAsync(key, tenantId);

            if (config != null)
            {
                // Store in cache
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheDuration
                };

                var serialized = JsonSerializer.Serialize(config);
                await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

                _logger.LogDebug("Cached configuration: {Key}", key);
            }

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving configuration: {Key}, tenant: {TenantId}",
                key, tenantId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetValueAsync(string key, string tenantId, string? defaultValue = null)
    {
        try
        {
            var config = await GetByKeyAsync(key, tenantId);
            return config?.Value ?? defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration value: {Key}", key);
            return defaultValue;
        }
    }

    /// <inheritdoc />
    public async Task<T?> GetValueAsync<T>(string key, string tenantId, T? defaultValue = default)
        where T : class
    {
        try
        {
            var stringValue = await GetValueAsync(key, tenantId);
            if (string.IsNullOrEmpty(stringValue))
                return defaultValue;

            var targetType = typeof(T);

            // Handle primitive types
            if (targetType == typeof(string))
            {
                return (stringValue as T)!;
            }

            if (targetType == typeof(int))
            {
                if (int.TryParse(stringValue, out var intValue))
                    return (intValue as T)!;
                return defaultValue;
            }

            if (targetType == typeof(bool))
            {
                if (bool.TryParse(stringValue, out var boolValue))
                    return (boolValue as T)!;
                return defaultValue;
            }

            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(stringValue, out var dateValue))
                    return (dateValue as T)!;
                return defaultValue;
            }

            if (targetType == typeof(double))
            {
                if (double.TryParse(stringValue, out var doubleValue))
                    return (doubleValue as T)!;
                return defaultValue;
            }

            // Handle JSON deserialization for complex types
            try
            {
                var result = JsonSerializer.Deserialize<T>(stringValue);
                return result ?? defaultValue;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex,
                    "Failed to deserialize configuration value as {Type}: {Key}",
                    targetType.Name, key);
                return defaultValue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving typed configuration value: {Key}, Type: {Type}",
                key, typeof(T).Name);
            return defaultValue;
        }
    }

    /// <inheritdoc />
    public async Task<ConfigurationDto> SaveAsync(ConfigurationDto config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(config.Key))
            throw new ArgumentException("Configuration key cannot be empty", nameof(config.Key));

        try
        {
            // Save to repository
            await _repository.UpsertAsync(config);

            // Invalidate cache
            var cacheKey = GenerateCacheKey(config.Key, config.TenantId);
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation(
                "Configuration saved and cache invalidated: {Key} for tenant {TenantId}",
                config.Key, config.TenantId);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error saving configuration: {Key} for tenant {TenantId}",
                config.Key, config.TenantId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> GetAllForTenantAsync(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        try
        {
            var configurations = await _repository.GetAllForTenantAsync(tenantId);

            var result = configurations
                .Where(c => !c.IsDeleted && !string.IsNullOrEmpty(c.Value))
                .ToDictionary(c => c.Key, c => c.Value);

            _logger.LogDebug("Retrieved {Count} configurations for tenant {TenantId}",
                result.Count, tenantId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving all configurations for tenant: {TenantId}",
                tenantId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string key, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be empty", nameof(key));

        try
        {
            // Find the config first to verify it exists
            var config = await _repository.FirstOrDefaultAsync(new { Key = key, TenantId = tenantId });
            if (config == null)
                return false;

            // Delete by key (assumes Key is the primary key or composite key includes it)
            await _repository.DeleteAsync(key);

            // Invalidate cache
            var cacheKey = GenerateCacheKey(key, tenantId);
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation(
                "Configuration deleted and cache invalidated: {Key}",
                key);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting configuration: {Key} for tenant {TenantId}",
                key, tenantId);
            throw;
        }
    }

    /// <summary>
    /// Generates a cache key for configuration.
    /// Format: config_{key}_{tenantId}
    /// </summary>
    private static string GenerateCacheKey(string key, string tenantId)
        => $"config_{key.ToLowerInvariant()}_{tenantId}";
}
