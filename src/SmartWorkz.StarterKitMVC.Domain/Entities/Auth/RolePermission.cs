namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

using SmartWorkz.Core.Entities;

public class RolePermission : AuditableEntity<int>
{
    public string RoleId { get; set; }
    public int PermissionId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public Role Role { get; set; }
    public Permission Permission { get; set; }
}
