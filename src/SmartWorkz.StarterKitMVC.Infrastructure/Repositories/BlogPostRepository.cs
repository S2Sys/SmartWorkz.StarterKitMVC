using SmartWorkz.StarterKitMVC.Shared.DTOs;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for blog posts (Master.BlogPost table)
/// Handles content management operations
/// </summary>
public class BlogPostRepository : DapperRepository<BlogPostDto>, IBlogPostRepository
{
    public BlogPostRepository(IDbConnection connection, ILogger<BlogPostRepository> logger)
        : base(connection, logger)
    {
        TableName = "BlogPost";
        Schema = "Master";
        IdColumn = "BlogPostId";
    }

    /// <summary>Get published blog posts for a tenant</summary>
    public async Task<IEnumerable<BlogPostDto>> GetPublishedAsync(string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[BlogPost]
            WHERE TenantId = @TenantId
              AND IsPublished = 1
              AND IsDeleted = 0
            ORDER BY PublishedAt DESC
            """;

        return await ExecuteQueryAsync(sql, new { TenantId = tenantId });
    }

    /// <summary>Get blog posts by author</summary>
    public async Task<IEnumerable<BlogPostDto>> GetByAuthorAsync(string authorId, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[BlogPost]
            WHERE AuthorId = @AuthorId
              AND TenantId = @TenantId
              AND IsDeleted = 0
            ORDER BY CreatedAt DESC
            """;

        return await ExecuteQueryAsync(sql, new { AuthorId = authorId, TenantId = tenantId });
    }

    /// <summary>Search blog posts by title or content</summary>
    public async Task<IEnumerable<BlogPostDto>> SearchAsync(string searchTerm, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[BlogPost]
            WHERE TenantId = @TenantId
              AND IsDeleted = 0
              AND (
                Title LIKE '%' + @SearchTerm + '%'
                OR Content LIKE '%' + @SearchTerm + '%'
                OR Summary LIKE '%' + @SearchTerm + '%'
                OR Tags LIKE '%' + @SearchTerm + '%'
              )
            ORDER BY CASE WHEN IsPublished = 1 THEN 0 ELSE 1 END, CreatedAt DESC
            """;

        return await ExecuteQueryAsync(sql, new { SearchTerm = searchTerm, TenantId = tenantId });
    }

    /// <summary>Get paged blog posts</summary>
    public async Task<(IEnumerable<BlogPostDto> Items, int Total)> GetPagedAsync(
        string tenantId, bool? published = null, int pageNumber = 1, int pageSize = 20)
    {
        var publishedFilter = published.HasValue ? "AND IsPublished = @Published" : "";

        var countSql = $"""
            SELECT COUNT(*) FROM [Master].[BlogPost]
            WHERE TenantId = @TenantId
              AND IsDeleted = 0
              {publishedFilter}
            """;

        var dataSql = $"""
            SELECT * FROM [Master].[BlogPost]
            WHERE TenantId = @TenantId
              AND IsDeleted = 0
              {publishedFilter}
            ORDER BY CASE WHEN IsPublished = 1 THEN PublishedAt ELSE CreatedAt END DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new DynamicParameters();
        param.Add("@TenantId", tenantId);
        param.Add("@PageNumber", pageNumber);
        param.Add("@PageSize", pageSize);
        if (published.HasValue)
            param.Add("@Published", published.Value);

        var total = await Connection.QueryFirstAsync<int>(countSql, param);
        var items = await Connection.QueryAsync<BlogPostDto>(dataSql, param);

        return (items, total);
    }

    /// <summary>Get blog post by slug</summary>
    public async Task<BlogPostDto?> GetBySlugAsync(string slug, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Master].[BlogPost]
            WHERE Slug = @Slug
              AND TenantId = @TenantId
              AND IsDeleted = 0
            """;

        return await Connection.QueryFirstOrDefaultAsync<BlogPostDto>(
            sql,
            new { Slug = slug, TenantId = tenantId });
    }
}
