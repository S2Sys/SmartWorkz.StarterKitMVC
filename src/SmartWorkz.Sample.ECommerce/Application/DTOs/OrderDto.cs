using SmartWorkz.Sample.ECommerce.Domain.Enums;

namespace SmartWorkz.Sample.ECommerce.Application.DTOs;

public record OrderDto(int Id, int CustomerId, string Status, decimal Total, string Currency, DateTime PlacedAt, List<OrderItemDto> Items);
