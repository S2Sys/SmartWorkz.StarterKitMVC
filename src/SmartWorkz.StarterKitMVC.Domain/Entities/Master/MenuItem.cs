namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class MenuItem
{
    public int MenuItemId { get; set; }
    public int MenuId { get; set; }
    public string Title { get; set; }
    public string URL { get; set; }
    public string Icon { get; set; }
    public int DisplayOrder { get; set; }
    public string NodePath { get; set; }
    public string RequiredRole { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Menu Menu { get; set; }
    public Tenant Tenant { get; set; }
}
