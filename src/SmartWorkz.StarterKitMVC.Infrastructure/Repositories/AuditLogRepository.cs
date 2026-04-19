using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for audit logs (Auth.AuditTrail table)
/// Provides audit trail and compliance tracking
/// </summary>
public class AuditLogRepository : DapperRepository<AuditLogDto>, IAuditLogRepository
{
    public AuditLogRepository(IDbConnection connection, ILogger<AuditLogRepository> logger)
        : base(connection, logger)
    {
        TableName = "AuditTrail";
        Schema = "Auth";
        IdColumn = "AuditTrailId";
    }

    /// <summary>Get audit logs for a specific entity</summary>
    public async Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityType, object entityId, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Auth].[AuditTrail]
            WHERE EntityType = @EntityType
              AND EntityId = @EntityId
              AND TenantId = @TenantId
            ORDER BY CreatedAt DESC
            """;

        return await ExecuteQueryAsync(sql, new
        {
            EntityType = entityType,
            EntityId = entityId,
            TenantId = tenantId
        });
    }

    /// <summary>Get audit logs for a specific user</summary>
    public async Task<IEnumerable<AuditLogDto>> GetByUserAsync(string userId, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Auth].[AuditTrail]
            WHERE UserId = @UserId
              AND TenantId = @TenantId
            ORDER BY CreatedAt DESC
            """;

        return await ExecuteQueryAsync(sql, new
        {
            UserId = userId,
            TenantId = tenantId
        });
    }

    /// <summary>Get audit logs within a date range</summary>
    public async Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime from, DateTime to, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Auth].[AuditTrail]
            WHERE TenantId = @TenantId
              AND CreatedAt >= @FromDate
              AND CreatedAt <= @ToDate
            ORDER BY CreatedAt DESC
            """;

        return await ExecuteQueryAsync(sql, new
        {
            TenantId = tenantId,
            FromDate = from,
            ToDate = to
        });
    }

    /// <summary>Get paged audit logs</summary>
    public async Task<(IEnumerable<AuditLogDto> Items, int Total)> GetPagedAsync(
        string tenantId, string? entityType = null, int pageNumber = 1, int pageSize = 20)
    {
        var filter = !string.IsNullOrEmpty(entityType) ? "AND EntityType = @EntityType" : "";

        var countSql = $"""
            SELECT COUNT(*) FROM [Auth].[AuditTrail]
            WHERE TenantId = @TenantId
            {filter}
            """;

        var dataSql = $"""
            SELECT * FROM [Auth].[AuditTrail]
            WHERE TenantId = @TenantId
            {filter}
            ORDER BY CreatedAt DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new DynamicParameters();
        param.Add("@TenantId", tenantId);
        param.Add("@PageNumber", pageNumber);
        param.Add("@PageSize", pageSize);
        if (!string.IsNullOrEmpty(entityType))
            param.Add("@EntityType", entityType);

        var total = await Connection.QueryFirstAsync<int>(countSql, param);
        var items = await Connection.QueryAsync<AuditLogDto>(dataSql, param);

        return (items, total);
    }

    /// <summary>Log an entity change</summary>
    public async Task LogChangeAsync(
        string entityType, object entityId, string action, string changes,
        string userId, string tenantId)
    {
        const string sql = """
            INSERT INTO [Auth].[AuditTrail] (
                AuditTrailId, UserId, EntityType, EntityId, Action,
                NewValues, TenantId, CreatedAt
            ) VALUES (
                @AuditTrailId, @UserId, @EntityType, @EntityId, @Action,
                @NewValues, @TenantId, @CreatedAt
            )
            """;

        await Connection.ExecuteAsync(sql, new
        {
            AuditTrailId = Guid.NewGuid(),
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId.ToString(),
            Action = action,
            NewValues = changes,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        });
    }

    /// <summary>Delete old audit logs (cleanup)</summary>
    public async Task DeleteOlderThanAsync(DateTime beforeDate, string tenantId)
    {
        const string sql = """
            DELETE FROM [Auth].[AuditTrail]
            WHERE TenantId = @TenantId
              AND CreatedAt < @BeforeDate
            """;

        await Connection.ExecuteAsync(sql, new
        {
            TenantId = tenantId,
            BeforeDate = beforeDate
        });
    }
}
