using HotChocolate.Types;

namespace SmartWorkz.Core.Web.GraphQL;

/// <summary>
/// GraphQL type representing a product in the system.
/// </summary>
[ObjectType]
public class ProductType
{
    /// <summary>
    /// Unique identifier for the product.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Product description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Product SKU (Stock Keeping Unit).
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// When the product was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the product was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Whether the product is active/available.
    /// </summary>
    public bool IsActive { get; set; }
}
