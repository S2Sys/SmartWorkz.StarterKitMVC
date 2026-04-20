using Dapper;
using SmartWorkz.Core.Shared.Data;
using SmartWorkz.Core.Shared.Guards;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using System.Data;

namespace SmartWorkz.Sample.ECommerce.Application.Services;

public class CatalogSearchService(IDbConnection connection)
{
    public async Task<IReadOnlyList<ProductDto>> SearchAsync(string query)
    {
        Guard.NotEmpty(query, nameof(query));
        const string sql = """
            SELECT p.Id, p.Name, p.Slug, p.Description,
                   p.Price_Amount AS Price, p.Price_Currency AS Currency,
                   p.Stock, p.IsActive, p.CategoryId, c.Name AS CategoryName
            FROM Products p
            LEFT JOIN Categories c ON c.Id = p.CategoryId
            WHERE p.IsActive = 1
              AND (p.Name LIKE @q OR p.Description LIKE @q OR c.Name LIKE @q)
            LIMIT 50
            """;
        var result = await connection.DapperQueryAsync<ProductDto>(sql, new { q = $"%{query}%" });
        return result.Succeeded ? result.Data!.AsReadOnly() : new List<ProductDto>().AsReadOnly();
    }
}
