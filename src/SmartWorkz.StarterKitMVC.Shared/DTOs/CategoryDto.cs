namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string TenantId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Slug { get; set; }
    public int? ParentCategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Slug { get; set; }
    public int? ParentCategoryId { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Slug { get; set; }
    public int? ParentCategoryId { get; set; }
}

public class CategoryTreeDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public List<CategoryTreeDto> Children { get; set; } = new();
}
