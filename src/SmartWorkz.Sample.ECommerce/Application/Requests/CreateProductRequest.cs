namespace SmartWorkz.Sample.ECommerce.Application.Requests;

public record CreateProductRequest(
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    string Currency = "USD",
    int Stock = default,
    bool IsActive = true,
    int CategoryId = default);
