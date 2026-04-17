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
}

/// <summary>DTO for Configuration entity</summary>
public class ConfigurationDto
{
    public Guid ConfigurationId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public string ConfigType { get; set; } // String, Integer, Boolean, DateTime, Json
    public string TenantId { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsEditable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
