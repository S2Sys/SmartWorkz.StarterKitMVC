using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for lookup values (Master.Lookup table)
/// Handles hierarchical lookup data like currencies, languages, timezones
/// </summary>
public class LookupRepository : DapperRepository<LookupDto>, ILookupRepository
{
    public LookupRepository(IDbConnection connection, ILogger<LookupRepository> logger)
        : base(connection, logger)
    {
        TableName = "Lookup";
        Schema = "Master";
        IdColumn = "LookupId";
    }

    /// <summary>Get lookup values by category key</summary>
    public async Task<IEnumerable<LookupDto>> GetByCategoryAsync(string categoryKey, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[Lookup]
            WHERE CategoryKey = @CategoryKey
              AND TenantId = @TenantId
              AND IsDeleted = 0
            ORDER BY SortOrder, DisplayName
            """;

        return await ExecuteQueryAsync(sql, new { CategoryKey = categoryKey, TenantId = tenantId });
    }

    /// <summary>Get all currency lookups</summary>
    public async Task<IEnumerable<LookupDto>> GetCurrenciesAsync(string tenantId)
    {
        return await GetByCategoryAsync("CURRENCY", tenantId);
    }

    /// <summary>Get all language lookups</summary>
    public async Task<IEnumerable<LookupDto>> GetLanguagesAsync(string tenantId)
    {
        return await GetByCategoryAsync("LANGUAGE", tenantId);
    }

    /// <summary>Get all timezone lookups</summary>
    public async Task<IEnumerable<LookupDto>> GetTimeZonesAsync(string tenantId)
    {
        return await GetByCategoryAsync("TIMEZONE", tenantId);
    }

    /// <summary>Get lookup ID by category and key</summary>
    public async Task<Guid?> GetIdByCategoryAndKeyAsync(string categoryKey, string key, string tenantId)
    {
        const string sql = """
            SELECT LookupId FROM [Master].[Lookup]
            WHERE CategoryKey = @CategoryKey
              AND [Key] = @Key
              AND TenantId = @TenantId
              AND IsDeleted = 0
            """;

        return await Connection.QueryFirstOrDefaultAsync<Guid?>(
            sql,
            new { CategoryKey = categoryKey, Key = key, TenantId = tenantId });
    }
}
