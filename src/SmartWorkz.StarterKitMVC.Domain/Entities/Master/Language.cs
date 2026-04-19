namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class Language
{
    public int LanguageId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string NativeName { get; set; }
    public bool IsDefault { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Tenant Tenant { get; set; }
}
