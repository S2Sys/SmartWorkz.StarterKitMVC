namespace SmartWorkz.Sample.ECommerce.Application.DTOs;

public record CategoryWithProductsDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    List<ProductDto> Products
);
