using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

public class DapperUserRepository : IUserRepository
{
    private readonly string _connectionString;

    public DapperUserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    private IDbConnection GetConnection() => new SqlConnection(_connectionString);

    // ── Queries ──────────────────────────────────────────────────────────────

    public async Task<User?> GetByEmailAsync(string email, string tenantId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(
            "Auth.sp_GetUserByEmail",
            new { Email = email, TenantId = tenantId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<User?> GetByIdAsync(string userId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(
            "Auth.sp_GetUserById",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<List<string>> GetUserRolesAsync(string userId, string tenantId)
    {
        using var connection = GetConnection();
        var roles = await connection.QueryAsync<string>(
            "Auth.sp_GetUserRoles",
            new { UserId = userId, TenantId = tenantId },
            commandType: CommandType.StoredProcedure);
        return roles.ToList();
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId, string tenantId)
    {
        using var connection = GetConnection();
        var permissions = await connection.QueryAsync<string>(
            "Auth.sp_GetUserPermissions",
            new { UserId = userId, TenantId = tenantId },
            commandType: CommandType.StoredProcedure);
        return permissions.ToList();
    }

    public async Task<bool> UserExistsAsync(string email, string tenantId)
    {
        using var connection = GetConnection();
        var count = await connection.QueryFirstOrDefaultAsync<int>(
            "Auth.sp_UserExists",
            new { Email = email, TenantId = tenantId },
            commandType: CommandType.StoredProcedure);
        return count > 0;
    }

    // ── User write (simple — no child sync) ──────────────────────────────────

    public async Task UpsertUserAsync(User user)
    {
        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Auth.sp_UpsertUser",
            new
            {
                user.UserId,
                UserName      = user.Username,
                user.Email,
                user.PasswordHash,
                user.DisplayName,
                user.PhoneNumber,
                user.AvatarUrl,
                user.Locale,
                TenantId      = user.TenantId,
                user.IsActive,
                user.EmailConfirmed,
                user.TwoFactorEnabled,
                user.LockoutEnabled,
                user.AccessFailedCount,
                UpdatedAt     = user.UpdatedAt ?? DateTime.UtcNow,
                user.UpdatedBy,
                Roles         = (string?)null,   // don't touch roles
                Permissions   = (string?)null    // don't touch permissions
            },
            commandType: CommandType.StoredProcedure);
    }

    // ── Aggregate write ───────────────────────────────────────────────────────

    public async Task UpsertUserWithRolesAsync(User user, List<string>? roleIds, List<int>? permissionIds)
    {
        var rolesTvp = roleIds == null ? null : ToRoleTvp(roleIds);
        var permsTvp = permissionIds == null ? null : ToPermissionTvp(permissionIds);

        var p = new DynamicParameters();
        p.Add("UserId",            user.UserId);
        p.Add("UserName",          user.Username);
        p.Add("Email",             user.Email);
        p.Add("PasswordHash",      user.PasswordHash);
        p.Add("DisplayName",       user.DisplayName);
        p.Add("PhoneNumber",       user.PhoneNumber);
        p.Add("AvatarUrl",         user.AvatarUrl);
        p.Add("Locale",            user.Locale);
        p.Add("TenantId",          user.TenantId);
        p.Add("IsActive",          user.IsActive);
        p.Add("EmailConfirmed",    user.EmailConfirmed);
        p.Add("TwoFactorEnabled",  user.TwoFactorEnabled);
        p.Add("LockoutEnabled",    user.LockoutEnabled);
        p.Add("AccessFailedCount", user.AccessFailedCount);
        p.Add("UpdatedAt",         user.UpdatedAt ?? DateTime.UtcNow);
        p.Add("UpdatedBy",         user.UpdatedBy);
        p.Add("Roles",        rolesTvp, DbType.Object);
        p.Add("Permissions",  permsTvp, DbType.Object);

        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Auth.sp_UpsertUser", p,
            commandType: CommandType.StoredProcedure);
    }

    // ── Paged search ─────────────────────────────────────────────────────────

    public async Task<(IEnumerable<User> Items, int Total)> SearchPagedAsync(
        string tenantId, string? search, string orderBy, bool descending,
        int page, int pageSize)
    {
        var safeOrder = orderBy switch
        {
            "Email"       => "Email",
            "Username"    => "Username",
            "DisplayName" => "DisplayName",
            _             => "CreatedAt"
        };
        var dir    = descending ? "DESC" : "ASC";
        var offset = (page - 1) * pageSize;
        var param  = new { TenantId = tenantId, Search = search, Offset = offset, PageSize = pageSize };

        var countSql = """
            SELECT COUNT(*)
            FROM   Auth.Users
            WHERE  TenantId = @TenantId
              AND  IsDeleted = 0
              AND  (@Search IS NULL
                    OR Email       LIKE '%' + @Search + '%'
                    OR Username    LIKE '%' + @Search + '%'
                    OR DisplayName LIKE '%' + @Search + '%')
            """;

        var dataSql = $"""
            SELECT UserId, Username, NormalizedUsername, Email, NormalizedEmail,
                   EmailConfirmed, DisplayName, AvatarUrl, Locale, TenantId,
                   IsActive, CreatedAt, UpdatedAt, IsDeleted, AccessFailedCount,
                   LockoutEnabled, LockoutEnd, TwoFactorEnabled, PhoneNumber,
                   PhoneNumberConfirmed, SecurityStamp, ConcurrencyStamp,
                   CreatedBy, UpdatedBy
            FROM   Auth.Users
            WHERE  TenantId = @TenantId
              AND  IsDeleted = 0
              AND  (@Search IS NULL
                    OR Email       LIKE '%' + @Search + '%'
                    OR Username    LIKE '%' + @Search + '%'
                    OR DisplayName LIKE '%' + @Search + '%')
            ORDER BY {safeOrder} {dir}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        using var connection = GetConnection();
        var total = await connection.ExecuteScalarAsync<int>(countSql, param);
        var items = await connection.QueryAsync<User>(dataSql, param);
        return (items, total);
    }

    // ── TVP helpers ───────────────────────────────────────────────────────────

    private static DataTable ToRoleTvp(List<string> roleIds)
    {
        var dt = new DataTable();
        dt.Columns.Add("RoleId", typeof(string));
        foreach (var id in roleIds) dt.Rows.Add(id);
        return dt;
    }

    private static DataTable ToPermissionTvp(List<int> permissionIds)
    {
        var dt = new DataTable();
        dt.Columns.Add("PermissionId", typeof(int));
        foreach (var id in permissionIds) dt.Rows.Add(id);
        return dt;
    }

    // ── Refresh tokens ───────────────────────────────────────────────────────

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, string tenantId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<RefreshToken>(
            "Auth.sp_GetRefreshToken",
            new { Token = token, TenantId = tenantId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
    {
        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Auth.sp_CreateRefreshToken",
            new
            {
                refreshToken.UserId,
                refreshToken.Token,
                refreshToken.ExpiresAt,
                refreshToken.TenantId,
                refreshToken.CreatedAt
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
    {
        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Auth.sp_RevokeRefreshToken",
            new { UserId = userId, Token = refreshToken },
            commandType: CommandType.StoredProcedure);
    }

    // ── Password reset ───────────────────────────────────────────────────────

    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string userId, string token, string tenantId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<PasswordResetToken>(
            "Auth.sp_GetPasswordResetToken",
            new { UserId = userId, Token = token, TenantId = tenantId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task CreatePasswordResetTokenAsync(PasswordResetToken resetToken)
    {
        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Auth.sp_CreatePasswordResetToken",
            new
            {
                resetToken.UserId,
                resetToken.Token,
                resetToken.ExpiresAt,
                resetToken.TenantId,
                resetToken.CreatedAt
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UsePasswordResetTokenAsync(int passwordResetTokenId)
    {
        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Auth.sp_UpdatePasswordResetToken",
            new { PasswordResetTokenId = passwordResetTokenId, UsedAt = DateTime.UtcNow },
            commandType: CommandType.StoredProcedure);
    }

    public async Task InvalidatePreviousPasswordResetTokensAsync(string userId)
    {
        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Auth.sp_InvalidatePreviousPasswordResetTokens",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
    }

    // ── Email verification ───────────────────────────────────────────────────

    public async Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string userId, string token, string tenantId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<EmailVerificationToken>(
            "Auth.sp_GetEmailVerificationToken",
            new { UserId = userId, Token = token, TenantId = tenantId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task CreateEmailVerificationTokenAsync(EmailVerificationToken verificationToken)
    {
        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Auth.sp_CreateEmailVerificationToken",
            new
            {
                verificationToken.UserId,
                verificationToken.Email,
                verificationToken.Token,
                verificationToken.ExpiresAt,
                verificationToken.TenantId,
                verificationToken.CreatedAt
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UseEmailVerificationTokenAsync(int emailVerificationTokenId)
    {
        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Auth.sp_UpdateEmailVerificationToken",
            new { EmailVerificationTokenId = emailVerificationTokenId, VerifiedAt = DateTime.UtcNow },
            commandType: CommandType.StoredProcedure);
    }
}
