namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

using SmartWorkz.Core.Entities;

public class PasswordResetToken : AuditableEntity<int>
{
    public string UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public User User { get; set; }
}
