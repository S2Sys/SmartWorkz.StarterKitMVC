using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;
using Microsoft.Extensions.Configuration;

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

    public async Task<User> GetByEmailAsync(string email, string tenantId)
    {
        using var connection = GetConnection();
        var user = await connection.QueryFirstOrDefaultAsync<User>(
            "Auth.sp_GetUserByEmail",
            new { Email = email, TenantId = tenantId },
            commandType: CommandType.StoredProcedure
        );
        return user;
    }

    public async Task<User> GetByIdAsync(string userId)
    {
        using var connection = GetConnection();
        const string sql = @"
            SELECT
                UserId, Email, NormalizedEmail, Username, NormalizedUsername,
                DisplayName, PasswordHash, SecurityStamp, ConcurrencyStamp,
                TenantId, EmailConfirmed, TwoFactorEnabled, LockoutEnabled,
                LockoutEnd, AccessFailedCount, IsActive, IsDeleted,
                CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, AvatarUrl
            FROM Auth.Users
            WHERE UserId = @UserId AND IsDeleted = 0";

        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
        return user;
    }

    public async Task<List<string>> GetUserRolesAsync(string userId, string tenantId)
    {
        using var connection = GetConnection();
        var roles = await connection.QueryAsync<string>(
            "Auth.sp_GetUserRoles",
            new { UserId = userId, TenantId = tenantId },
            commandType: CommandType.StoredProcedure
        );
        return roles.ToList();
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId, string tenantId)
    {
        using var connection = GetConnection();
        var permissions = await connection.QueryAsync<string>(
            "Auth.sp_GetUserPermissions",
            new { UserId = userId, TenantId = tenantId },
            commandType: CommandType.StoredProcedure
        );
        return permissions.ToList();
    }

    public async Task CreateUserAsync(User user)
    {
        using var connection = GetConnection();
        const string sql = @"
            INSERT INTO Auth.Users (
                UserId, Email, NormalizedEmail, Username, NormalizedUsername,
                DisplayName, PasswordHash, SecurityStamp, ConcurrencyStamp,
                TenantId, EmailConfirmed, TwoFactorEnabled, LockoutEnabled,
                LockoutEnd, AccessFailedCount, IsActive, IsDeleted,
                CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, AvatarUrl
            ) VALUES (
                @UserId, @Email, @NormalizedEmail, @Username, @NormalizedUsername,
                @DisplayName, @PasswordHash, @SecurityStamp, @ConcurrencyStamp,
                @TenantId, @EmailConfirmed, @TwoFactorEnabled, @LockoutEnabled,
                @LockoutEnd, @AccessFailedCount, @IsActive, @IsDeleted,
                @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy, @AvatarUrl
            )";

        await connection.ExecuteAsync(sql, user);
    }

    public async Task UpdateUserAsync(User user)
    {
        using var connection = GetConnection();
        const string sql = @"
            UPDATE Auth.Users
            SET Email = @Email,
                NormalizedEmail = @NormalizedEmail,
                Username = @Username,
                NormalizedUsername = @NormalizedUsername,
                DisplayName = @DisplayName,
                PasswordHash = @PasswordHash,
                SecurityStamp = @SecurityStamp,
                ConcurrencyStamp = @ConcurrencyStamp,
                EmailConfirmed = @EmailConfirmed,
                TwoFactorEnabled = @TwoFactorEnabled,
                LockoutEnabled = @LockoutEnabled,
                LockoutEnd = @LockoutEnd,
                AccessFailedCount = @AccessFailedCount,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy,
                AvatarUrl = @AvatarUrl
            WHERE UserId = @UserId AND TenantId = @TenantId";

        await connection.ExecuteAsync(sql, user);
    }

    public async Task<bool> UserExistsAsync(string email, string tenantId)
    {
        using var connection = GetConnection();
        const string sql = @"
            SELECT COUNT(*)
            FROM Auth.Users
            WHERE Email = @Email AND TenantId = @TenantId AND IsDeleted = 0";

        var count = await connection.QueryFirstOrDefaultAsync<int>(
            sql,
            new { Email = email, TenantId = tenantId }
        );
        return count > 0;
    }

    public async Task<RefreshToken> GetRefreshTokenAsync(string tokenHash, string tenantId)
    {
        using var connection = GetConnection();
        const string sql = @"
            SELECT RefreshTokenId, UserId, Token, ExpiresAt, RevokedAt, TenantId, CreatedAt, IsDeleted
            FROM Auth.RefreshTokens
            WHERE Token = @Token AND TenantId = @TenantId
              AND RevokedAt IS NULL AND ExpiresAt > GETUTCDATE() AND IsDeleted = 0";

        var token = await connection.QueryFirstOrDefaultAsync<RefreshToken>(
            sql,
            new { Token = tokenHash, TenantId = tenantId }
        );
        return token;
    }

    public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
    {
        using var connection = GetConnection();
        const string sql = @"
            INSERT INTO Auth.RefreshTokens (
                RefreshTokenId, UserId, Token, ExpiresAt, TenantId, CreatedAt, IsDeleted
            ) VALUES (
                @RefreshTokenId, @UserId, @Token, @ExpiresAt, @TenantId, @CreatedAt, 0
            )";

        await connection.ExecuteAsync(sql, refreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
    {
        using var connection = GetConnection();
        const string sql = @"
            UPDATE Auth.RefreshTokens
            SET RevokedAt = GETUTCDATE()
            WHERE UserId = @UserId AND Token = @Token AND RevokedAt IS NULL";

        await connection.ExecuteAsync(sql, new { UserId = userId, Token = refreshToken });
    }

    public async Task<PasswordResetToken> GetPasswordResetTokenAsync(string userId, string token, string tenantId)
    {
        using var connection = GetConnection();
        const string sql = @"
            SELECT PasswordResetTokenId, UserId, Token, ExpiresAt, UsedAt, TenantId, CreatedAt, IsDeleted
            FROM Auth.PasswordResetTokens
            WHERE UserId = @UserId AND Token = @Token AND TenantId = @TenantId
              AND UsedAt IS NULL AND ExpiresAt > GETUTCDATE() AND IsDeleted = 0";

        var resetToken = await connection.QueryFirstOrDefaultAsync<PasswordResetToken>(
            sql,
            new { UserId = userId, Token = token, TenantId = tenantId }
        );
        return resetToken;
    }

    public async Task CreatePasswordResetTokenAsync(PasswordResetToken resetToken)
    {
        using var connection = GetConnection();
        const string sql = @"
            INSERT INTO Auth.PasswordResetTokens (
                PasswordResetTokenId, UserId, Token, ExpiresAt, TenantId, CreatedAt, IsDeleted
            ) VALUES (
                @PasswordResetTokenId, @UserId, @Token, @ExpiresAt, @TenantId, @CreatedAt, 0
            )";

        await connection.ExecuteAsync(sql, resetToken);
    }

    public async Task UpdatePasswordResetTokenAsync(PasswordResetToken resetToken)
    {
        using var connection = GetConnection();
        const string sql = @"
            UPDATE Auth.PasswordResetTokens
            SET UsedAt = @UsedAt
            WHERE PasswordResetTokenId = @PasswordResetTokenId";

        await connection.ExecuteAsync(sql, resetToken);
    }

    public async Task InvalidatePreviousPasswordResetTokensAsync(string userId)
    {
        using var connection = GetConnection();
        const string sql = @"
            UPDATE Auth.PasswordResetTokens
            SET IsDeleted = 1
            WHERE UserId = @UserId AND UsedAt IS NULL AND IsDeleted = 0";

        await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task<EmailVerificationToken> GetEmailVerificationTokenAsync(string userId, string token, string tenantId)
    {
        using var connection = GetConnection();
        const string sql = @"
            SELECT EmailVerificationTokenId, UserId, Token, ExpiresAt, VerifiedAt, TenantId, CreatedAt, IsDeleted
            FROM Auth.EmailVerificationTokens
            WHERE UserId = @UserId AND Token = @Token AND TenantId = @TenantId
              AND VerifiedAt IS NULL AND ExpiresAt > GETUTCDATE() AND IsDeleted = 0";

        var verificationToken = await connection.QueryFirstOrDefaultAsync<EmailVerificationToken>(
            sql,
            new { UserId = userId, Token = token, TenantId = tenantId }
        );
        return verificationToken;
    }

    public async Task CreateEmailVerificationTokenAsync(EmailVerificationToken verificationToken)
    {
        using var connection = GetConnection();
        const string sql = @"
            INSERT INTO Auth.EmailVerificationTokens (
                EmailVerificationTokenId, UserId, Token, ExpiresAt, TenantId, CreatedAt, IsDeleted
            ) VALUES (
                @EmailVerificationTokenId, @UserId, @Token, @ExpiresAt, @TenantId, @CreatedAt, 0
            )";

        await connection.ExecuteAsync(sql, verificationToken);
    }

    public async Task UpdateEmailVerificationTokenAsync(EmailVerificationToken verificationToken)
    {
        using var connection = GetConnection();
        const string sql = @"
            UPDATE Auth.EmailVerificationTokens
            SET VerifiedAt = @VerifiedAt
            WHERE EmailVerificationTokenId = @EmailVerificationTokenId";

        await connection.ExecuteAsync(sql, verificationToken);
    }
}
