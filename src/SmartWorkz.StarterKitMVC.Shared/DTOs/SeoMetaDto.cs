namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public class SeoMetaDto
{
    public int SeoMetaId { get; set; }
    public string TenantId { get; set; }
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Keywords { get; set; }
    public string Slug { get; set; }
    public string OgImage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateSeoMetaDto
{
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Keywords { get; set; }
    public string Slug { get; set; }
    public string OgImage { get; set; }
}

public class UpdateSeoMetaDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Keywords { get; set; }
    public string Slug { get; set; }
    public string OgImage { get; set; }
}
