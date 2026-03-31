namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

public class User
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string NormalizedUserName { get; set; }
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
    public bool LockoutEnabled { get; set; } = true;
    public int AccessFailedCount { get; set; }
    public string DisplayName { get; set; }
    public string AvatarUrl { get; set; }
    public string Locale { get; set; } = "en-US";
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<UserPermission> UserPermissions { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public ICollection<LoginAttempt> LoginAttempts { get; set; }
}
