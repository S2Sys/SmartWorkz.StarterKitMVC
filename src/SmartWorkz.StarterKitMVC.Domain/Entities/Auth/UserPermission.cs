namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

using SmartWorkz.Core.Entities;

public class UserPermission : AuditableEntity<int>
{
    public string UserId { get; set; }
    public int PermissionId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public User User { get; set; }
    public Permission Permission { get; set; }
}
