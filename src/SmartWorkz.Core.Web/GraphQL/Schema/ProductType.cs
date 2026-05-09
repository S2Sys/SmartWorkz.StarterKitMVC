namespace SmartWorkz.Core.Web.GraphQL.Schema;

/// <summary>
/// GraphQL type for Product entity.
/// Exposes product information for catalog queries.
/// </summary>
public class ProductType
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? Sku { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;
}
