namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class Menu
{
    public int MenuId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string MenuType { get; set; }
    public int DisplayOrder { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Tenant Tenant { get; set; }
    public ICollection<MenuItem> MenuItems { get; set; }
}
