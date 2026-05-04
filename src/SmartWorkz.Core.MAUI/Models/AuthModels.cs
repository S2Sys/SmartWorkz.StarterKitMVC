namespace SmartWorkz.Mobile;

internal sealed record LoginRequest(string Email, string Password);

internal sealed record RefreshRequest(string RefreshToken);

internal sealed record AuthTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
