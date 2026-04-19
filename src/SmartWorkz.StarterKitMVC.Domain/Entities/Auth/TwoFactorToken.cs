namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

public class TwoFactorToken
{
    public int TwoFactorTokenId { get; set; }
    public string UserId { get; set; }
    public string Token { get; set; }
    public string TokenType { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int Attempts { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public User User { get; set; }
}
