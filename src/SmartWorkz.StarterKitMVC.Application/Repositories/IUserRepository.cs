using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

public interface IUserRepository
{
    // ── Queries ──────────────────────────────────────────────────────────────
    Task<User?>              GetByEmailAsync(string email, string tenantId);
    Task<User?>              GetByIdAsync(string userId);
    Task<List<string>>       GetUserRolesAsync(string userId, string tenantId);
    Task<List<string>>       GetUserPermissionsAsync(string userId, string tenantId);
    Task<bool>               UserExistsAsync(string email, string tenantId);

    /// <summary>Paged search for admin list pages.</summary>
    Task<(IEnumerable<User> Items, int Total)> SearchPagedAsync(
        string tenantId, string? search, string orderBy, bool descending,
        int page, int pageSize);

    // ── User write (simple — no child sync) ──────────────────────────────────
    /// <summary>
    /// INSERT or UPDATE the Users row only (no role/permission sync).
    /// Use for password change, lockout updates, profile edits.
    /// </summary>
    Task UpsertUserAsync(User user);

    // ── Aggregate write (root + children in one transaction) ─────────────────
    /// <summary>
    /// INSERT or UPDATE user + sync roles + sync direct permissions atomically.
    /// Pass null for roleIds / permissionIds to leave them unchanged.
    /// Pass empty list to remove all assignments.
    /// </summary>
    Task UpsertUserWithRolesAsync(User user, List<string>? roleIds, List<int>? permissionIds);

    // ── Refresh tokens ───────────────────────────────────────────────────────
    Task<RefreshToken?>      GetRefreshTokenAsync(string token, string tenantId);
    Task                     CreateRefreshTokenAsync(RefreshToken refreshToken);
    Task                     RevokeRefreshTokenAsync(string userId, string refreshToken);

    // ── Password reset ───────────────────────────────────────────────────────
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string userId, string token, string tenantId);
    Task                      CreatePasswordResetTokenAsync(PasswordResetToken resetToken);
    Task                      UsePasswordResetTokenAsync(int passwordResetTokenId);
    Task                      InvalidatePreviousPasswordResetTokensAsync(string userId);

    // ── Email verification ───────────────────────────────────────────────────
    Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string userId, string token, string tenantId);
    Task                          CreateEmailVerificationTokenAsync(EmailVerificationToken verificationToken);
    Task                          UseEmailVerificationTokenAsync(int emailVerificationTokenId);
}
