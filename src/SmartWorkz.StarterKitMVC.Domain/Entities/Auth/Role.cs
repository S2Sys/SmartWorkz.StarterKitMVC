namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

public class Role
{
    public string RoleId { get; set; }
    public string Name { get; set; }
    public string NormalizedName { get; set; }
    public string Description { get; set; }
    public string TenantId { get; set; }
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}
