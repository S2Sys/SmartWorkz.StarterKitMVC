using SmartWorkz.Core.Shared.Guards;
using SmartWorkz.Core.Shared.Mapping;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Application.Mapping;

public class CategoryToCategoryDtoProfile : IMapperProfile<Category, CategoryDto>
{
    public Type SourceType => typeof(Category);
    public Type TargetType => typeof(CategoryDto);

    public CategoryDto Map(Category source)
    {
        source = Guard.NotNull(source, nameof(source));

        return new CategoryDto(
            source.Id,
            source.Name,
            source.Slug,
            source.Description,
            source.Products?.Count ?? 0);
    }

    public Task<CategoryDto> MapAsync(Category source, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Map(source));
    }
}

public class ProductToProductDtoProfile : IMapperProfile<Product, ProductDto>
{
    public Type SourceType => typeof(Product);
    public Type TargetType => typeof(ProductDto);

    public ProductDto Map(Product source)
    {
        source = Guard.NotNull(source, nameof(source));

        return new ProductDto(
            source.Id,
            source.Name,
            source.Slug,
            source.Description,
            source.Price?.Amount ?? 0,
            source.Price?.Currency ?? "USD",
            source.Stock,
            source.IsActive,
            source.CategoryId,
            source.Category?.Name ?? string.Empty);
    }

    public Task<ProductDto> MapAsync(Product source, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Map(source));
    }
}

public class CustomerToCustomerDtoProfile : IMapperProfile<Customer, CustomerDto>
{
    public Type SourceType => typeof(Customer);
    public Type TargetType => typeof(CustomerDto);

    public CustomerDto Map(Customer source)
    {
        source = Guard.NotNull(source, nameof(source));

        return new CustomerDto(
            source.Id,
            source.FirstName,
            source.LastName,
            source.Email.Value);
    }

    public Task<CustomerDto> MapAsync(Customer source, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Map(source));
    }
}

public class OrderItemToOrderItemDtoProfile : IMapperProfile<OrderItem, OrderItemDto>
{
    public Type SourceType => typeof(OrderItem);
    public Type TargetType => typeof(OrderItemDto);

    public OrderItemDto Map(OrderItem source)
    {
        source = Guard.NotNull(source, nameof(source));

        return new OrderItemDto(
            source.Id,
            source.ProductId,
            source.ProductName,
            source.Quantity,
            source.UnitPrice?.Amount ?? 0,
            source.GetLineTotal() ?? 0);
    }

    public Task<OrderItemDto> MapAsync(OrderItem source, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Map(source));
    }
}

public class OrderToOrderDtoProfile : IMapperProfile<Order, OrderDto>
{
    public Type SourceType => typeof(Order);
    public Type TargetType => typeof(OrderDto);

    public OrderDto Map(Order source)
    {
        source = Guard.NotNull(source, nameof(source));

        return new OrderDto(
            source.Id,
            source.CustomerId,
            source.Status.ToString(),
            source.Total?.Amount ?? 0,
            source.Total?.Currency ?? "USD",
            source.PlacedAt,
            source.Items
                .Select(i => new OrderItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice?.Amount ?? 0,
                    i.GetLineTotal() ?? 0))
                .ToList());
    }

    public Task<OrderDto> MapAsync(Order source, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Map(source));
    }
}
