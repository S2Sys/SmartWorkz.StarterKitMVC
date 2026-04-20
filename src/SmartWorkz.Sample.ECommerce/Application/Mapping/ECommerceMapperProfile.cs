using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Application.Mapping;

public static class CategoryMapper
{
    public static CategoryDto ToDto(Category src) => new(
        src.Id, src.Name, src.Slug, src.Description, src.Products?.Count ?? 0);
}

public static class ProductMapper
{
    public static ProductDto ToDto(Product src) => new(
        src.Id, src.Name, src.Slug, src.Description,
        src.Price?.Amount ?? 0, src.Price?.Currency ?? string.Empty,
        src.Stock, src.IsActive, src.CategoryId, src.Category?.Name ?? string.Empty);
}

public static class CustomerMapper
{
    public static CustomerDto ToDto(Customer src) => new(
        src.Id, src.FirstName, src.LastName, src.Email.Value);
}

public static class OrderItemMapper
{
    public static OrderItemDto ToDto(OrderItem src) => new(
        src.Id, src.ProductId, src.ProductName, src.Quantity,
        src.UnitPrice?.Amount ?? 0, src.GetLineTotal() ?? 0);
}

public static class OrderMapper
{
    public static OrderDto ToDto(Order src) => new(
        src.Id, src.CustomerId, src.Status.ToString(),
        src.Total?.Amount ?? 0, src.Total?.Currency ?? string.Empty,
        src.PlacedAt,
        src.Items.Select(OrderItemMapper.ToDto).ToList());
}
