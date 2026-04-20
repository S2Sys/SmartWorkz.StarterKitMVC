namespace SmartWorkz.Sample.ECommerce.Application.DTOs;

public record OrderItemDto(int Id, int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
