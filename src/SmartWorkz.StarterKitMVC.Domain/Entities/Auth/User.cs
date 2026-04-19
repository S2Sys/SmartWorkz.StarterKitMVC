namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

using SmartWorkz.Core.Entities;

public class User : AuditableEntity<string>
{
    public string Username { get; set; }
    public string NormalizedUsername { get; set; }
    public string Email { get; set; }
    public string NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string PasswordHash { get; set; }
    public string SecurityStamp { get; set; }
    public string ConcurrencyStamp { get; set; }
    public string PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LockoutEndAt => LockoutEnd;
    public bool LockoutEnabled { get; set; } = true;
    public int AccessFailedCount { get; set; }
    public string DisplayName { get; set; }
    public string AvatarUrl { get; set; }
    public string Locale { get; set; } = "en-US";
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<UserPermission> UserPermissions { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public ICollection<LoginAttempt> LoginAttempts { get; set; }
    public ICollection<TenantUser> TenantUsers { get; set; }
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public ICollection<TwoFactorToken> TwoFactorTokens { get; set; }
}
