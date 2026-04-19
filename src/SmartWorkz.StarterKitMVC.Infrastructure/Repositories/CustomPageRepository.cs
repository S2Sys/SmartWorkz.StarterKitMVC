using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for custom pages (Master.CustomPage table)
/// Manages CMS pages and static content
/// </summary>
public class CustomPageRepository : DapperRepository<CustomPageDto>, ICustomPageRepository
{
    public CustomPageRepository(IDbConnection connection, ILogger<CustomPageRepository> logger)
        : base(connection, logger)
    {
        TableName = "CustomPage";
        Schema = "Master";
        IdColumn = "CustomPageId";
    }

    /// <summary>Get custom page by slug</summary>
    public async Task<CustomPageDto?> GetBySlugAsync(string slug, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[CustomPage]
            WHERE Slug = @Slug
              AND TenantId = @TenantId
              AND IsDeleted = 0
            """;

        return await Connection.QueryFirstOrDefaultAsync<CustomPageDto>(
            sql,
            new { Slug = slug, TenantId = tenantId });
    }

    /// <summary>Get custom page by name</summary>
    public async Task<CustomPageDto?> GetByNameAsync(string name, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[CustomPage]
            WHERE [Name] = @Name
              AND TenantId = @TenantId
              AND IsDeleted = 0
            """;

        return await Connection.QueryFirstOrDefaultAsync<CustomPageDto>(
            sql,
            new { Name = name, TenantId = tenantId });
    }

    /// <summary>Get all published pages</summary>
    public async Task<IEnumerable<CustomPageDto>> GetPublishedAsync(string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[CustomPage]
            WHERE TenantId = @TenantId
              AND IsPublished = 1
              AND IsDeleted = 0
            ORDER BY [Name]
            """;

        return await ExecuteQueryAsync(sql, new { TenantId = tenantId });
    }

    /// <summary>Search custom pages</summary>
    public async Task<IEnumerable<CustomPageDto>> SearchAsync(string searchTerm, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[CustomPage]
            WHERE TenantId = @TenantId
              AND IsDeleted = 0
              AND (
                [Name] LIKE '%' + @SearchTerm + '%'
                OR Title LIKE '%' + @SearchTerm + '%'
                OR Content LIKE '%' + @SearchTerm + '%'
                OR MetaKeywords LIKE '%' + @SearchTerm + '%'
              )
            ORDER BY [Name]
            """;

        return await ExecuteQueryAsync(sql, new
        {
            SearchTerm = searchTerm,
            TenantId = tenantId
        });
    }
}
