namespace SmartWorkz.StarterKitMVC.Application.Identity;

public sealed record TokenResult(string AccessToken, string RefreshToken, DateTime ExpiresAt);
