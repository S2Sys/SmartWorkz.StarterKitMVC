namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class Category
{
    public int CategoryId { get; set; }
    public int? ParentCategoryId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public string NodePath { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Tenant Tenant { get; set; }
    public Category ParentCategory { get; set; }
    public ICollection<Category> ChildCategories { get; set; }
    public ICollection<Product> Products { get; set; }
}
