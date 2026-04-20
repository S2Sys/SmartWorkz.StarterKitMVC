namespace SmartWorkz.Sample.ECommerce.Application.DTOs;

public record CartItemDto(int ProductId, string ProductName, string Slug, int Quantity, decimal UnitPrice);

public record CartDto(List<CartItemDto> Items)
{
    public decimal Total => Items.Sum(i => i.Quantity * i.UnitPrice);
    public int ItemCount => Items.Sum(i => i.Quantity);
}
