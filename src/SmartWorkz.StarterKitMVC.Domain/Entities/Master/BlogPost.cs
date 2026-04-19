namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class BlogPost : AuditableEntity<int>
{
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public string Author { get; set; }
    public DateTime PublishedAt { get; set; }
    public int Views { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; }
}
