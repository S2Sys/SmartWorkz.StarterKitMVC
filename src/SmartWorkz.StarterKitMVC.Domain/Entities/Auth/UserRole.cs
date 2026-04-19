namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

using SmartWorkz.Core.Entities;

public class UserRole : AuditableEntity<int>
{
    public string UserId { get; set; }
    public string RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; }
    public Role Role { get; set; }
}
