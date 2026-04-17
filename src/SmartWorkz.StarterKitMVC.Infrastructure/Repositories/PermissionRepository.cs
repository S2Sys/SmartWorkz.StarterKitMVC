using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for permissions (Auth.Permission table)
/// Manages permission data and checks
/// </summary>
public class PermissionRepository : DapperRepository<PermissionDto>, IPermissionRepository
{
    public PermissionRepository(IDbConnection connection, ILogger<PermissionRepository> logger)
        : base(connection, logger)
    {
        TableName = "Permissions";
        Schema = "Auth";
        IdColumn = "PermissionId";
    }

    /// <summary>Get permission by name</summary>
    public async Task<PermissionDto?> GetByNameAsync(string name, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Auth].[Permissions]
            WHERE [Name] = @Name
              AND TenantId = @TenantId
              AND IsDeleted = 0
            """;

        return await Connection.QueryFirstOrDefaultAsync<PermissionDto>(
            sql,
            new { Name = name, TenantId = tenantId });
    }

    /// <summary>Get all permissions for a tenant</summary>
    public async Task<IEnumerable<PermissionDto>> GetAllForTenantAsync(string tenantId)
    {
        const string sql = """
            SELECT * FROM [Auth].[Permissions]
            WHERE TenantId = @TenantId
              AND IsDeleted = 0
            ORDER BY Category, [Name]
            """;

        return await ExecuteQueryAsync(sql, new { TenantId = tenantId });
    }

    /// <summary>Get permissions for a role</summary>
    public async Task<IEnumerable<PermissionDto>> GetByRoleAsync(Guid roleId, string tenantId)
    {
        const string sql = """
            SELECT p.* FROM [Auth].[Permissions] p
            INNER JOIN [Auth].[RolePermissions] rp ON p.PermissionId = rp.PermissionId
            WHERE rp.RoleId = @RoleId
              AND p.TenantId = @TenantId
              AND p.IsDeleted = 0
            ORDER BY p.Category, p.[Name]
            """;

        return await ExecuteQueryAsync(sql, new { RoleId = roleId, TenantId = tenantId });
    }

    /// <summary>Get permissions for a user (direct + via roles)</summary>
    public async Task<IEnumerable<PermissionDto>> GetByUserAsync(Guid userId, string tenantId)
    {
        const string sql = """
            SELECT DISTINCT p.* FROM [Auth].[Permissions] p
            WHERE TenantId = @TenantId
              AND IsDeleted = 0
              AND (
                -- Direct user permissions
                EXISTS (
                    SELECT 1 FROM [Auth].[UserPermissions] up
                    WHERE up.UserId = @UserId AND up.PermissionId = p.PermissionId
                )
                -- Permissions via roles
                OR EXISTS (
                    SELECT 1 FROM [Auth].[RolePermissions] rp
                    INNER JOIN [Auth].[UserRoles] ur ON rp.RoleId = ur.RoleId
                    WHERE ur.UserId = @UserId AND rp.PermissionId = p.PermissionId
                )
              )
            ORDER BY p.Category, p.[Name]
            """;

        return await ExecuteQueryAsync(sql, new { UserId = userId, TenantId = tenantId });
    }

    /// <summary>Check if user has a specific permission</summary>
    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName, string tenantId)
    {
        const string sql = """
            SELECT CAST(CASE WHEN EXISTS(
                SELECT 1 FROM [Auth].[Permissions] p
                WHERE p.[Name] = @PermissionName
                  AND p.TenantId = @TenantId
                  AND p.IsDeleted = 0
                  AND (
                    -- Direct permission
                    EXISTS (
                        SELECT 1 FROM [Auth].[UserPermissions] up
                        WHERE up.UserId = @UserId AND up.PermissionId = p.PermissionId
                    )
                    -- Permission via role
                    OR EXISTS (
                        SELECT 1 FROM [Auth].[RolePermissions] rp
                        INNER JOIN [Auth].[UserRoles] ur ON rp.RoleId = ur.RoleId
                        WHERE ur.UserId = @UserId AND rp.PermissionId = p.PermissionId
                    )
                  )
            ) THEN 1 ELSE 0 END AS BIT)
            """;

        return await Connection.QueryFirstAsync<bool>(sql, new
        {
            UserId = userId,
            PermissionName = permissionName,
            TenantId = tenantId
        });
    }

    /// <summary>Check if role has a specific permission</summary>
    public async Task<bool> RoleHasPermissionAsync(Guid roleId, string permissionName, string tenantId)
    {
        const string sql = """
            SELECT CAST(CASE WHEN EXISTS(
                SELECT 1 FROM [Auth].[Permissions] p
                INNER JOIN [Auth].[RolePermissions] rp ON p.PermissionId = rp.PermissionId
                WHERE rp.RoleId = @RoleId
                  AND p.[Name] = @PermissionName
                  AND p.TenantId = @TenantId
                  AND p.IsDeleted = 0
            ) THEN 1 ELSE 0 END AS BIT)
            """;

        return await Connection.QueryFirstAsync<bool>(sql, new
        {
            RoleId = roleId,
            PermissionName = permissionName,
            TenantId = tenantId
        });
    }

    /// <summary>Assign a permission to a role</summary>
    public async Task AssignToRoleAsync(object roleId, object permissionId)
    {
        await Connection.ExecuteAsync(
            "[Auth].[spUpsertRolePermission]",
            new
            {
                RolePermissionId = 0,
                RoleId = roleId,
                PermissionId = permissionId,
                TenantId = (string?)null,
                CreatedAt = DateTime.UtcNow
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    /// <summary>Remove a permission from a role</summary>
    public async Task RemoveRolePermissionAsync(object roleId, object permissionId)
    {
        const string sql = """
            DELETE FROM [Auth].[RolePermissions]
            WHERE RoleId = @RoleId AND PermissionId = @PermissionId
            """;

        await Connection.ExecuteAsync(sql, new { RoleId = roleId, PermissionId = permissionId });
    }
}
