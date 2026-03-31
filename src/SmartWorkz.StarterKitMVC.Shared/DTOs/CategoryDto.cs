namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public record CategoryDto(
    int CategoryId,
    string TenantId,
    string Name,
    string Description,
    string Slug,
    int? ParentCategoryId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateCategoryDto(
    string Name,
    string Description,
    string Slug,
    int? ParentCategoryId
);

public record UpdateCategoryDto(
    string Name,
    string Description,
    string Slug,
    int? ParentCategoryId
);

public record CategoryTreeDto(
    int CategoryId,
    string Name,
    string Slug
)
{
    public List<CategoryTreeDto> Children { get; init; } = new();
}
