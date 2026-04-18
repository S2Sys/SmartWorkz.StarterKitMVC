using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for configuration key-value pairs (Master.Configuration table)
/// </summary>
public interface IConfigurationRepository : IDapperRepository<ConfigurationDto>
{
    /// <summary>Get configuration by key</summary>
    Task<ConfigurationDto?> GetByKeyAsync(string key, string tenantId);

    /// <summary>Get configuration value by key</summary>
    Task<string?> GetValueAsync(string key, string tenantId, string? defaultValue = null);

    /// <summary>Set configuration value by key</summary>
    Task SetValueAsync(string key, string value, string tenantId, string updatedBy);

    /// <summary>Get all configuration keys for a tenant</summary>
    Task<IEnumerable<ConfigurationDto>> GetAllForTenantAsync(string tenantId);

    /// <summary>Delete configuration by key</summary>
    Task<bool> DeleteAsync(string key, string tenantId);
}

