using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for roles (Auth.Role table)
/// Provides role management and permission assignment operations
/// </summary>
public class RoleRepository : DapperRepository<RoleDto>, IRoleRepository
{
    public RoleRepository(IDbConnection connection, ILogger<RoleRepository> logger)
        : base(connection, logger)
    {
        TableName = "Role";
        Schema = "Auth";
        IdColumn = "RoleId";
    }

    /// <summary>Get role by name</summary>
    public async Task<RoleDto?> GetByNameAsync(string name, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Auth].[Role]
            WHERE [Name] = @Name
              AND TenantId = @TenantId
              AND IsDeleted = 0
            """;

        return await Connection.QueryFirstOrDefaultAsync<RoleDto>(
            sql,
            new { Name = name, TenantId = tenantId });
    }

    /// <summary>Get all roles for a tenant with pagination</summary>
    public async Task<(IEnumerable<RoleDto> Items, int Total)> GetPagedAsync(
        string tenantId, int pageNumber, int pageSize)
    {
        const string countSql = """
            SELECT COUNT(*) FROM [Auth].[Role]
            WHERE TenantId = @TenantId AND IsDeleted = 0
            """;

        const string dataSql = """
            SELECT * FROM [Auth].[Role]
            WHERE TenantId = @TenantId AND IsDeleted = 0
            ORDER BY [Name]
            OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new { TenantId = tenantId, PageNumber = pageNumber, PageSize = pageSize };
        var total = await Connection.QueryFirstAsync<int>(countSql, param);
        var items = await Connection.QueryAsync<RoleDto>(dataSql, param);

        return (items, total);
    }

    /// <summary>Get permissions assigned to a role</summary>
    public async Task<IEnumerable<PermissionDto>> GetPermissionsAsync(Guid roleId, string tenantId)
    {
        const string sql = """
            SELECT p.* FROM [Auth].[Permission] p
            INNER JOIN [Auth].[RolePermission] rp ON p.PermissionId = rp.PermissionId
            WHERE rp.RoleId = @RoleId
              AND p.TenantId = @TenantId
              AND p.IsDeleted = 0
            """;

        return await Connection.QueryAsync<PermissionDto>(sql, new { RoleId = roleId, TenantId = tenantId });
    }

    /// <summary>Assign permissions to a role</summary>
    public async Task AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds, string tenantId)
    {
        // Use stored procedure if available, otherwise implement directly
        // Note: This procedure doesn't exist - the code falls back to direct SQL
        const string spName = "[Auth].[sp_Role_AssignPermissions]";

        try
        {
            // Create TVP for permissions
            var dt = new DataTable();
            dt.Columns.Add("PermissionId", typeof(Guid));
            foreach (var id in permissionIds)
            {
                dt.Rows.Add(id);
            }

            var param = new DynamicParameters();
            param.Add("@RoleId", roleId);
            param.Add("@TenantId", tenantId);
            param.Add("@PermissionIds", dt, DbType.Object);

            await ExecuteStoredProcedureNonQueryAsync(spName, param);
        }
        catch
        {
            // Fallback: delete existing and insert new
            const string deleteSql = "DELETE FROM [Auth].[RolePermission] WHERE RoleId = @RoleId";
            await Connection.ExecuteAsync(deleteSql, new { RoleId = roleId });

            const string insertSql = """
                INSERT INTO [Auth].[RolePermission] (RolePermissionId, RoleId, PermissionId, CreatedAt)
                VALUES (@RolePermissionId, @RoleId, @PermissionId, @CreatedAt)
                """;

            foreach (var permissionId in permissionIds)
            {
                await Connection.ExecuteAsync(insertSql, new
                {
                    RolePermissionId = Guid.NewGuid(),
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    /// <summary>Remove all permissions from a role</summary>
    public async Task RemoveAllPermissionsAsync(Guid roleId)
    {
        const string sql = "DELETE FROM [Auth].[RolePermission] WHERE RoleId = @RoleId";
        await Connection.ExecuteAsync(sql, new { RoleId = roleId });
    }
}
