namespace SmartWorkz.Sample.ECommerce.Application.DTOs;

public record LoginResponseDto(string Token, DateTime ExpiresAt, string Email, string FullName);
