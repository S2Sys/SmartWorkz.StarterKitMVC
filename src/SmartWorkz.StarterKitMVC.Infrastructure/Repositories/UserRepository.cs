using System.Data;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

public class UserRepository : CachedDapperRepository, IUserRepository
{
    public UserRepository(IDbConnection connection, IMemoryCache cache, ILogger<UserRepository> logger)
        : base(connection, cache, logger)
    {
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    public async Task<User?> GetByEmailAsync(string email, string tenantId)
    {
        return await QuerySingleSpAsync<User>(
            "[Auth].[spGetUser]",
            new { Email = email, TenantId = tenantId });
    }

    public async Task<User?> GetByIdAsync(string userId)
    {
        return await QuerySingleSpAsync<User>(
            "[Auth].[spGetUser]",
            new { UserId = userId });
    }

    public async Task<List<string>> GetUserRolesAsync(string userId, string tenantId)
    {
        var roles = await QuerySpAsync<string>(
            "[Auth].[spGetUserRole]",
            new { UserId = userId, TenantId = tenantId });
        return roles.ToList();
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId, string tenantId)
    {
        var permissions = await QuerySpAsync<string>(
            "[Auth].[spGetUserPermission]",
            new { UserId = userId, TenantId = tenantId });
        return permissions.ToList();
    }

    public async Task<bool> UserExistsAsync(string email, string tenantId)
    {
        var count = await QuerySingleSpAsync<int?>(
            "[Auth].[spUserExists]",
            new { Email = email, TenantId = tenantId });
        return (count ?? 0) > 0;
    }

    // ── User write (simple — no child sync) ──────────────────────────────────

    public async Task UpsertUserAsync(User user)
    {
        await ExecuteSpAsync(
            "[Auth].[spUpsertUser]",
            new
            {
                user.UserId,
                UserName      = user.Username,
                NormalizedUserName = user.NormalizedUsername,
                user.Email,
                NormalizedEmail = user.NormalizedEmail,
                user.PasswordHash,
                user.DisplayName,
                user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                user.AvatarUrl,
                user.Locale,
                TenantId      = user.TenantId,
                user.IsActive,
                user.EmailConfirmed,
                user.TwoFactorEnabled,
                user.LockoutEnabled,
                user.LockoutEnd,
                user.AccessFailedCount,
                SecurityStamp = user.SecurityStamp,
                ConcurrencyStamp = user.ConcurrencyStamp,
                UpdatedAt     = user.UpdatedAt ?? DateTime.UtcNow,
                user.UpdatedBy
            });
    }

    // ── Aggregate write ───────────────────────────────────────────────────────

    public async Task UpsertUserWithRolesAsync(User user, List<string>? roleIds, List<int>? permissionIds)
    {
        var rolesTvp = roleIds == null ? null : ToRoleTvp(roleIds);
        var permsTvp = permissionIds == null ? null : ToPermissionTvp(permissionIds);

        var p = new DynamicParameters();
        p.Add("UserId",            user.UserId);
        p.Add("UserName",          user.Username);
        p.Add("NormalizedUserName", user.NormalizedUsername);
        p.Add("Email",             user.Email);
        p.Add("NormalizedEmail",   user.NormalizedEmail);
        p.Add("PasswordHash",      user.PasswordHash);
        p.Add("DisplayName",       user.DisplayName);
        p.Add("PhoneNumber",       user.PhoneNumber);
        p.Add("PhoneNumberConfirmed", user.PhoneNumberConfirmed);
        p.Add("AvatarUrl",         user.AvatarUrl);
        p.Add("Locale",            user.Locale);
        p.Add("TenantId",          user.TenantId);
        p.Add("IsActive",          user.IsActive);
        p.Add("EmailConfirmed",    user.EmailConfirmed);
        p.Add("TwoFactorEnabled",  user.TwoFactorEnabled);
        p.Add("LockoutEnabled",    user.LockoutEnabled);
        p.Add("LockoutEnd",        user.LockoutEnd);
        p.Add("AccessFailedCount", user.AccessFailedCount);
        p.Add("SecurityStamp",     user.SecurityStamp);
        p.Add("ConcurrencyStamp",  user.ConcurrencyStamp);
        p.Add("UpdatedAt",         user.UpdatedAt ?? DateTime.UtcNow);
        p.Add("UpdatedBy",         user.UpdatedBy);
        p.Add("Roles",        rolesTvp, DbType.Object);
        p.Add("Permissions",  permsTvp, DbType.Object);

        await ExecuteSpAsync("[Auth].[spUpsertUser]", p);
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

        var total = await Connection.ExecuteScalarAsync<int?>(countSql, param);
        var items = await Connection.QueryAsync<User>(dataSql, param);
        return (items, total ?? 0);
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
        return await QuerySingleSpAsync<RefreshToken>(
            "[Auth].[spGetAuthToken]",
            new { Token = token, TokenType = "RefreshToken", TenantId = tenantId });
    }

    public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
    {
        await ExecuteSpAsync(
            "[Auth].[spUpsertAuthToken]",
            new
            {
                AuthTokenId = 0,
                refreshToken.UserId,
                refreshToken.Token,
                TokenType = "RefreshToken",
                TokenSubType = (string?)null,
                refreshToken.ExpiresAt,
                UsedAt = (DateTime?)null,
                VerifiedAt = (DateTime?)null,
                RevokedAt = (DateTime?)null,
                Attempts = 0,
                refreshToken.TenantId
            });
    }

    public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
    {
        await ExecuteSpAsync(
            "[Auth].[spUpsertAuthToken]",
            new
            {
                AuthTokenId = 0,
                UserId = userId,
                Token = refreshToken,
                TokenType = "RefreshToken",
                TokenSubType = (string?)null,
                ExpiresAt = DateTime.UtcNow,
                UsedAt = (DateTime?)null,
                VerifiedAt = (DateTime?)null,
                RevokedAt = DateTime.UtcNow,
                Attempts = 0,
                TenantId = (string?)null
            });
    }

    // ── Password reset ───────────────────────────────────────────────────────

    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string userId, string token, string tenantId)
    {
        return await QuerySingleSpAsync<PasswordResetToken>(
            "[Auth].[spGetAuthToken]",
            new { UserId = userId, Token = token, TokenType = "PasswordReset", TenantId = tenantId });
    }

    public async Task CreatePasswordResetTokenAsync(PasswordResetToken resetToken)
    {
        await ExecuteSpAsync(
            "[Auth].[spUpsertAuthToken]",
            new
            {
                AuthTokenId = 0,
                resetToken.UserId,
                resetToken.Token,
                TokenType = "PasswordReset",
                TokenSubType = (string?)null,
                resetToken.ExpiresAt,
                UsedAt = (DateTime?)null,
                VerifiedAt = (DateTime?)null,
                RevokedAt = (DateTime?)null,
                Attempts = 0,
                resetToken.TenantId
            });
    }

    public async Task UsePasswordResetTokenAsync(int passwordResetTokenId)
    {
        await ExecuteSpAsync(
            "[Auth].[spUpsertAuthToken]",
            new
            {
                AuthTokenId = passwordResetTokenId,
                UserId = (string?)null,
                Token = (string?)null,
                TokenType = (string?)null,
                TokenSubType = (string?)null,
                ExpiresAt = DateTime.UtcNow,
                UsedAt = DateTime.UtcNow,
                VerifiedAt = (DateTime?)null,
                RevokedAt = (DateTime?)null,
                Attempts = 0,
                TenantId = (string?)null
            });
    }

    public async Task InvalidatePreviousPasswordResetTokensAsync(string userId)
    {
        var sql = """
            UPDATE Auth.AuthTokens
            SET RevokedAt = @RevokedAt, UpdatedAt = GETUTCDATE()
            WHERE UserId = @UserId
              AND TokenType = 'PasswordReset'
              AND IsDeleted = 0
              AND ExpiresAt > GETUTCDATE()
            """;

        await Connection.ExecuteAsync(sql, new { UserId = userId, RevokedAt = DateTime.UtcNow });
    }

    // ── Email verification ───────────────────────────────────────────────────

    public async Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string userId, string token, string tenantId)
    {
        return await QuerySingleSpAsync<EmailVerificationToken>(
            "[Auth].[spGetAuthToken]",
            new { UserId = userId, Token = token, TokenType = "EmailVerification", TenantId = tenantId });
    }

    public async Task CreateEmailVerificationTokenAsync(EmailVerificationToken verificationToken)
    {
        await ExecuteSpAsync(
            "[Auth].[spUpsertAuthToken]",
            new
            {
                AuthTokenId = 0,
                verificationToken.UserId,
                verificationToken.Token,
                TokenType = "EmailVerification",
                TokenSubType = verificationToken.Email,
                verificationToken.ExpiresAt,
                UsedAt = (DateTime?)null,
                VerifiedAt = (DateTime?)null,
                RevokedAt = (DateTime?)null,
                Attempts = 0,
                verificationToken.TenantId
            });
    }

    public async Task UseEmailVerificationTokenAsync(int emailVerificationTokenId)
    {
        await ExecuteSpAsync(
            "[Auth].[spUpsertAuthToken]",
            new
            {
                AuthTokenId = emailVerificationTokenId,
                UserId = (string?)null,
                Token = (string?)null,
                TokenType = (string?)null,
                TokenSubType = (string?)null,
                ExpiresAt = DateTime.UtcNow,
                UsedAt = (DateTime?)null,
                VerifiedAt = DateTime.UtcNow,
                RevokedAt = (DateTime?)null,
                Attempts = 0,
                TenantId = (string?)null
            });
    }
}
