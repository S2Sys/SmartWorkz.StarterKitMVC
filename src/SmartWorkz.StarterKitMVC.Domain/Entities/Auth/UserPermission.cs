namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

public class UserPermission
{
    public int UserPermissionId { get; set; }
    public string UserId { get; set; }
    public int PermissionId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public User User { get; set; }
    public Permission Permission { get; set; }
}
