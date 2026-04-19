namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

public class TenantUser
{
    public int TenantUserId { get; set; }
    public string TenantId { get; set; }
    public string UserId { get; set; }
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public User User { get; set; }
}
