namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

public class UserRole
{
    public int UserRoleId { get; set; }
    public string UserId { get; set; }
    public string RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public User User { get; set; }
    public Role Role { get; set; }
}
