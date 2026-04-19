using SmartWorkz.StarterKitMVC.Shared.DTOs;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for countries (Master.Country table)
/// Provides reference data for country lookups
/// </summary>
public class CountryRepository : DapperRepository<CountryDto>, ICountryRepository
{
    public CountryRepository(IDbConnection connection, ILogger<CountryRepository> logger)
        : base(connection, logger)
    {
        TableName = "Country";
        Schema = "Master";
        IdColumn = "CountryId";
    }

    /// <summary>Get country by ISO code</summary>
    public async Task<CountryDto?> GetByCodeAsync(string code, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[Country]
            WHERE Code = @Code
              AND TenantId = @TenantId
              AND IsDeleted = 0
            """;

        return await Connection.QueryFirstOrDefaultAsync<CountryDto>(
            sql,
            new { Code = code, TenantId = tenantId });
    }

    /// <summary>Get country by name</summary>
    public async Task<CountryDto?> GetByNameAsync(string name, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[Country]
            WHERE [Name] = @Name
              AND TenantId = @TenantId
              AND IsDeleted = 0
            """;

        return await Connection.QueryFirstOrDefaultAsync<CountryDto>(
            sql,
            new { Name = name, TenantId = tenantId });
    }

    /// <summary>Get all active countries</summary>
    public async Task<IEnumerable<CountryDto>> GetAllActiveAsync(string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[Country]
            WHERE TenantId = @TenantId
              AND IsActive = 1
              AND IsDeleted = 0
            ORDER BY DisplayName
            """;

        return await ExecuteQueryAsync(sql, new { TenantId = tenantId });
    }

    /// <summary>Search countries by code or name</summary>
    public async Task<IEnumerable<CountryDto>> SearchAsync(string searchTerm, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[Country]
            WHERE TenantId = @TenantId
              AND IsDeleted = 0
              AND (
                Code LIKE '%' + @SearchTerm + '%'
                OR [Name] LIKE '%' + @SearchTerm + '%'
                OR DisplayName LIKE '%' + @SearchTerm + '%'
              )
            ORDER BY DisplayName
            """;

        return await ExecuteQueryAsync(sql, new
        {
            SearchTerm = searchTerm,
            TenantId = tenantId
        });
    }
}
