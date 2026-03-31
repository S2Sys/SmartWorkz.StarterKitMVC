namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class Configuration
{
    public int ConfigId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string ConfigType { get; set; }
    public string Description { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Tenant Tenant { get; set; }
}
