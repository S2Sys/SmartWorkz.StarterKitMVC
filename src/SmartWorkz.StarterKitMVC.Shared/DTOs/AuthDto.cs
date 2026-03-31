namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public record LoginRequest(string Email, string Password, string TenantId);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserProfileDto User
);

public record RegisterRequest(
    string Email,
    string Username,
    string Password,
    string DisplayName,
    string TenantId
);

public record RefreshTokenRequest(string AccessToken, string RefreshToken);

public record ForgotPasswordRequest(string Email, string TenantId);

public record ResetPasswordRequest(
    string Token,
    string Email,
    string NewPassword,
    string TenantId
);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record VerifyEmailRequest(string Token, string Email, string TenantId);

public record UserProfileDto(
    string UserId,
    string Email,
    string Username,
    string DisplayName,
    string AvatarUrl,
    string TenantId,
    bool EmailConfirmed,
    bool TwoFactorEnabled
)
{
    public List<string> Roles { get; init; } = new();
    public List<string> Permissions { get; init; } = new();
}
