namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

public class PasswordResetToken
{
    public int PasswordResetTokenId { get; set; }
    public string UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public User User { get; set; }
}
