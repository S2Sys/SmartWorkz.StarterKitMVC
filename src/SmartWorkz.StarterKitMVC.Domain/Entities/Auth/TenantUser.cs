namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

using SmartWorkz.Core.Entities;

public class TenantUser : AuditableEntity<int>
{
    public string TenantId { get; set; }
    public string UserId { get; set; }
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public string Status { get; set; } = "Active";

    public User User { get; set; }
}
