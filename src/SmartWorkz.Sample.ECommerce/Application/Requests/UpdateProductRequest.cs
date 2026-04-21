namespace SmartWorkz.Sample.ECommerce.Application.Requests;

public record UpdateProductRequest(
    string? Name = null,
    string? Slug = null,
    string? Description = null,
    decimal? Price = null,
    string? Currency = null,
    int? Stock = null,
    bool? IsActive = null,
    int? CategoryId = null);
