using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

public interface IUserRepository
{
    Task<User> GetByEmailAsync(string email, string tenantId);
    Task<User> GetByIdAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId, string tenantId);
    Task<List<string>> GetUserPermissionsAsync(string userId, string tenantId);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task<bool> UserExistsAsync(string email, string tenantId);
    Task<RefreshToken> GetRefreshTokenAsync(string tokenHash, string tenantId);
    Task CreateRefreshTokenAsync(RefreshToken refreshToken);
    Task RevokeRefreshTokenAsync(string userId, string refreshToken);
    Task<PasswordResetToken> GetPasswordResetTokenAsync(string userId, string token, string tenantId);
    Task CreatePasswordResetTokenAsync(PasswordResetToken resetToken);
    Task UpdatePasswordResetTokenAsync(PasswordResetToken resetToken);
    Task InvalidatePreviousPasswordResetTokensAsync(string userId);
    Task<EmailVerificationToken> GetEmailVerificationTokenAsync(string userId, string token, string tenantId);
    Task CreateEmailVerificationTokenAsync(EmailVerificationToken verificationToken);
    Task UpdateEmailVerificationTokenAsync(EmailVerificationToken verificationToken);
}
