namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public record ProductDto(
    int ProductId,
    string TenantId,
    int CategoryId,
    string Name,
    string Description,
    string SKU,
    string Slug,
    decimal Price,
    int Stock,
    bool IsFeatured,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateProductDto(
    int CategoryId,
    string Name,
    string Description,
    string SKU,
    string Slug,
    decimal Price,
    int Stock,
    bool IsFeatured
);

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Slug,
    bool IsFeatured
);

public record ProductSearchResultDto(
    int ProductId,
    string Name,
    string Slug,
    decimal Price,
    bool IsFeatured
);
