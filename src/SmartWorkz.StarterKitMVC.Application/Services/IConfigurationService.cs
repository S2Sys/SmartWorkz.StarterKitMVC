using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for managing application configuration with type-safe access and caching.
/// Supports multiple configuration types: String, Integer, Boolean, DateTime, Json.
/// Includes encryption support for sensitive values.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets a configuration value by key.
    /// Results are cached for 1 hour.
    /// </summary>
    Task<ConfigurationDto?> GetByKeyAsync(string key, string tenantId);

    /// <summary>
    /// Gets a configuration value as a string by key.
    /// </summary>
    Task<string?> GetValueAsync(string key, string tenantId, string? defaultValue = null);

    /// <summary>
    /// Gets a configuration value with automatic type conversion.
    /// Supports: string, int, bool, DateTime, and JSON deserialization.
    /// </summary>
    Task<T?> GetValueAsync<T>(string key, string tenantId, T? defaultValue = default)
        where T : class;

    /// <summary>
    /// Saves or updates a configuration value and invalidates cache.
    /// </summary>
    Task<ConfigurationDto> SaveAsync(ConfigurationDto config);

    /// <summary>
    /// Gets all configuration values for a specific tenant.
    /// </summary>
    Task<Dictionary<string, string>> GetAllForTenantAsync(string tenantId);

    /// <summary>
    /// Deletes a configuration value.
    /// </summary>
    Task<bool> DeleteAsync(string key, string tenantId);
}
