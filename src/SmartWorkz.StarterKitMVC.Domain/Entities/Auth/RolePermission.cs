namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

public class RolePermission
{
    public int RolePermissionId { get; set; }
    public string RoleId { get; set; }
    public int PermissionId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Role Role { get; set; }
    public Permission Permission { get; set; }
}
