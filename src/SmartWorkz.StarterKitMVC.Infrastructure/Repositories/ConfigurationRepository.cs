using SmartWorkz.StarterKitMVC.Shared.DTOs;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for configuration key-value pairs (Master.Configuration table)
/// Provides convenient access to application settings
/// </summary>
public class ConfigurationRepository : DapperRepository<ConfigurationDto>, IConfigurationRepository
{
    public ConfigurationRepository(IDbConnection connection, ILogger<ConfigurationRepository> logger)
        : base(connection, logger)
    {
        TableName = "Configuration";
        Schema = "Master";
        IdColumn = "ConfigurationId";
    }

    /// <summary>Get configuration by key</summary>
    public async Task<ConfigurationDto?> GetByKeyAsync(string key, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[Configuration]
            WHERE [Key] = @Key
              AND TenantId = @TenantId
              AND IsDeleted = 0
            """;

        return await Connection.QueryFirstOrDefaultAsync<ConfigurationDto>(
            sql,
            new { Key = key, TenantId = tenantId });
    }

    /// <summary>Get configuration value by key</summary>
    public async Task<string?> GetValueAsync(string key, string tenantId, string? defaultValue = null)
    {
        var config = await GetByKeyAsync(key, tenantId);
        return config?.Value ?? defaultValue;
    }

    /// <summary>Set configuration value by key (upsert)</summary>
    public async Task SetValueAsync(string key, string value, string tenantId, string updatedBy)
    {
        var existing = await GetByKeyAsync(key, tenantId);

        if (existing != null)
        {
            // Update existing
            const string updateSql = """
                UPDATE [Master].[Configuration]
                SET [Value] = @Value, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy
                WHERE [Key] = @Key AND TenantId = @TenantId
                """;

            await Connection.ExecuteAsync(updateSql, new
            {
                Key = key,
                Value = value,
                TenantId = tenantId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy
            });
        }
        else
        {
            // Insert new
            var config = new ConfigurationDto
            {
                ConfigurationId = Guid.NewGuid(),
                Key = key,
                Value = value,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = updatedBy
            };

            await UpsertAsync(config);
        }
    }

    /// <summary>Get all configuration keys for a tenant</summary>
    public async Task<IEnumerable<ConfigurationDto>> GetAllForTenantAsync(string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[Configuration]
            WHERE TenantId = @TenantId
              AND IsDeleted = 0
            ORDER BY [Key]
            """;

        return await ExecuteQueryAsync(sql, new { TenantId = tenantId });
    }
}
