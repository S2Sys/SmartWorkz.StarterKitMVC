namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class Category : AuditableEntity<int>
{
    public int? ParentCategoryId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public string NodePath { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; }
    public Category ParentCategory { get; set; }
    public ICollection<Category> ChildCategories { get; set; }
    public ICollection<Product> Products { get; set; }
}
