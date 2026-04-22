using SmartWorkz.Core;

namespace SmartWorkz.Sample.ECommerce.Domain.Entities;

public class Product : AuditEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Money? Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
