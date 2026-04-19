namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

using SmartWorkz.Core.Entities;

public class TwoFactorToken : AuditableEntity<int>
{
    public string UserId { get; set; }
    public string Token { get; set; }
    public string TokenType { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int Attempts { get; set; }

    public User User { get; set; }
}
