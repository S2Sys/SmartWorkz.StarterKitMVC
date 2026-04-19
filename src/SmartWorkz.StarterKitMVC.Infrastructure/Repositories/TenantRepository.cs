using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Tenant repository supporting both EF Core and Dapper operations
/// </summary>
public class TenantRepository : Repository<Tenant>, ITenantRepository, IDapperRepository<TenantDto>
{
    private readonly MasterDbContext _masterContext;
    private readonly IDbConnection _connection;
    private readonly ILogger<TenantRepository> _logger;

    public TenantRepository(MasterDbContext context) : base(context)
    {
        _masterContext = context;
    }

    public TenantRepository(MasterDbContext context, IDbConnection connection, ILogger<TenantRepository> logger)
        : base(context)
    {
        _masterContext = context;
        _connection = connection;
        _logger = logger;
    }

    public async Task<Tenant> GetByNameAsync(string name)
    {
        return await _masterContext.Tenants
            .FirstOrDefaultAsync(t => t.Name == name && !t.IsDeleted);
    }

    public async Task<List<Tenant>> GetActiveTenantAsync()
    {
        return await _masterContext.Tenants
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync();
    }

    // IDapperRepository<TenantDto> implementations
    async Task<TenantDto?> IDapperRepository<TenantDto>.GetByIdAsync(object id)
    {
        const string sql = """
            SELECT * FROM [Shared].[Tenant]
            WHERE TenantId = @Id AND IsDeleted = 0
            """;
        return await _connection.QueryFirstOrDefaultAsync<TenantDto>(sql, new { Id = id });
    }

    async Task<IEnumerable<TenantDto>> IDapperRepository<TenantDto>.GetAllAsync(string tenantId)
    {
        const string sql = """
            SELECT * FROM [Shared].[Tenant]
            WHERE IsDeleted = 0
            ORDER BY Name
            """;
        return await _connection.QueryAsync<TenantDto>(sql);
    }

    async Task<IEnumerable<TenantDto>> IDapperRepository<TenantDto>.FindAsync(object filters)
    {
        const string sql = """
            SELECT * FROM [Shared].[Tenant]
            WHERE IsDeleted = 0
            ORDER BY Name
            """;
        return await _connection.QueryAsync<TenantDto>(sql);
    }

    async Task<TenantDto?> IDapperRepository<TenantDto>.FirstOrDefaultAsync(object filters)
    {
        const string sql = """
            SELECT TOP 1 * FROM [Shared].[Tenant]
            WHERE IsDeleted = 0
            ORDER BY Name
            """;
        return await _connection.QueryFirstOrDefaultAsync<TenantDto>(sql);
    }

    async Task IDapperRepository<TenantDto>.UpsertAsync(TenantDto entity)
    {
        const string sql = """
            MERGE INTO [Shared].[Tenant] AS target
            USING (SELECT @TenantId) AS source(TenantId)
            ON target.TenantId = source.TenantId
            WHEN MATCHED THEN
                UPDATE SET Name = @Name, DisplayName = @DisplayName, IsActive = @IsActive, UpdatedAt = @UpdatedAt
            WHEN NOT MATCHED THEN
                INSERT (TenantId, Code, Name, DisplayName, IsActive, CreatedAt, CreatedBy)
                VALUES (@TenantId, @Code, @Name, @DisplayName, @IsActive, @CreatedAt, @CreatedBy);
            """;
        await _connection.ExecuteAsync(sql, entity);
    }

    async Task IDapperRepository<TenantDto>.SoftDeleteAsync(object id)
    {
        const string sql = """
            UPDATE [Shared].[Tenant]
            SET IsDeleted = 1, UpdatedAt = @UpdatedAt
            WHERE TenantId = @Id
            """;
        await _connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
    }

    async Task IDapperRepository<TenantDto>.DeleteAsync(object id)
    {
        const string sql = "DELETE FROM [Shared].[Tenant] WHERE TenantId = @Id";
        await _connection.ExecuteAsync(sql, new { Id = id });
    }

    async Task<bool> IDapperRepository<TenantDto>.ExistsAsync(object filters)
    {
        const string sql = """
            SELECT CAST(CASE WHEN EXISTS(
                SELECT 1 FROM [Shared].[Tenant] WHERE IsDeleted = 0
            ) THEN 1 ELSE 0 END AS BIT)
            """;
        return await _connection.QueryFirstAsync<bool>(sql);
    }

    async Task<int> IDapperRepository<TenantDto>.CountAsync(object filters)
    {
        const string sql = "SELECT COUNT(*) FROM [Shared].[Tenant] WHERE IsDeleted = 0";
        return await _connection.QueryFirstAsync<int>(sql);
    }

    async Task<(IEnumerable<TenantDto> Items, int Total)> IDapperRepository<TenantDto>.GetPagedAsync(
        object filters, string orderBy, bool descending, int pageNumber, int pageSize)
    {
        var direction = descending ? "DESC" : "ASC";
        var offset = (pageNumber - 1) * pageSize;

        const string countSql = "SELECT COUNT(*) FROM [Shared].[Tenant] WHERE IsDeleted = 0";
        var dataSql = $"""
            SELECT * FROM [Shared].[Tenant]
            WHERE IsDeleted = 0
            ORDER BY [{orderBy}] {direction}
            OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY
            """;

        var total = await _connection.QueryFirstAsync<int>(countSql);
        var items = await _connection.QueryAsync<TenantDto>(dataSql);
        return (items, total);
    }

    public async Task<TenantDto?> GetByCodeAsync(string code)
    {
        const string sql = """
            SELECT * FROM [Shared].[Tenant]
            WHERE Code = @Code AND IsDeleted = 0
            """;
        return await _connection.QueryFirstOrDefaultAsync<TenantDto>(sql, new { Code = code });
    }

    public async Task<bool> IsActiveAsync(string tenantId)
    {
        const string sql = """
            SELECT CAST(CASE WHEN EXISTS(
                SELECT 1 FROM [Shared].[Tenant]
                WHERE TenantId = @TenantId AND IsActive = 1 AND IsDeleted = 0
            ) THEN 1 ELSE 0 END AS BIT)
            """;
        return await _connection.QueryFirstAsync<bool>(sql, new { TenantId = tenantId });
    }

    public async Task<TenantDto?> GetWithDetailsAsync(string tenantId)
    {
        const string sql = """
            SELECT * FROM [Shared].[Tenant]
            WHERE TenantId = @TenantId AND IsDeleted = 0
            """;
        return await _connection.QueryFirstOrDefaultAsync<TenantDto>(sql, new { TenantId = tenantId });
    }
}
