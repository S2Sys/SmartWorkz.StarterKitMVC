namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public class ProductDto
{
    public int ProductId { get; set; }
    public string TenantId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SKU { get; set; }
    public string Slug { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SKU { get; set; }
    public string Slug { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
}

public class UpdateProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Slug { get; set; }
    public bool IsFeatured { get; set; }
}

public class ProductSearchResultDto
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public decimal Price { get; set; }
    public bool IsFeatured { get; set; }
}
