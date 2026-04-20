namespace SmartWorkz.Sample.ECommerce.Application.DTOs;

public record ProductDto(int Id, string Name, string Slug, string? Description, decimal Price, string Currency, int Stock, bool IsActive, int CategoryId, string CategoryName);
