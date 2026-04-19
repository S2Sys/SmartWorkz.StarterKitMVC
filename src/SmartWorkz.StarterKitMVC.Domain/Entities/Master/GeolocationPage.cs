namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class GeolocationPage : AuditableEntity<int>
{
    public int GeoId { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; }
}
