namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

using SmartWorkz.Core.Entities;

public class SeoMeta : AuditableEntity<int>
{
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Keywords { get; set; }
    public string Slug { get; set; }
    public string OgTitle { get; set; }
    public string OgDescription { get; set; }
    public string OgImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
