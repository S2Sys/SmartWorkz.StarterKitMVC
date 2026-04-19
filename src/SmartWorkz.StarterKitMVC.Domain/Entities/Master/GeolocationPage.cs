namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class GeolocationPage
{
    public int GeoPageId { get; set; }
    public int GeoId { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Tenant Tenant { get; set; }
}
