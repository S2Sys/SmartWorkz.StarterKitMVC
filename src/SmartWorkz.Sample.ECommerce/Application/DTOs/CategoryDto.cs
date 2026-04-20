namespace SmartWorkz.Sample.ECommerce.Application.DTOs;

public record CategoryDto(int Id, string Name, string Slug, string? Description, int ProductCount);
