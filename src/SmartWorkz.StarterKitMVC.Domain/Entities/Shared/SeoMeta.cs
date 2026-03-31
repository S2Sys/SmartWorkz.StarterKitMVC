namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

public class SeoMeta
{
    public int SeoMetaId { get; set; }
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Keywords { get; set; }
    public string Slug { get; set; }
    public string OgTitle { get; set; }
    public string OgDescription { get; set; }
    public string OgImageUrl { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
