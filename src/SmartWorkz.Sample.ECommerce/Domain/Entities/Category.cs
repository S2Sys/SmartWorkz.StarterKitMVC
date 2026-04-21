using SmartWorkz.Core;

namespace SmartWorkz.Sample.ECommerce.Domain.Entities;

public class Category : AuditableEntity<int>, IEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
