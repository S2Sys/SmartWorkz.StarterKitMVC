namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

using SmartWorkz.Core.Entities;

public class Permission : AuditableEntity<int>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string PermissionType { get; set; }
    public string ResourceType { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<RolePermission> RolePermissions { get; set; }
    public ICollection<UserPermission> UserPermissions { get; set; }
}
