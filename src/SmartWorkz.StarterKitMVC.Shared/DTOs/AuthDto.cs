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

/// <summary>
/// Data transfer object representing a user's profile information.
/// Contains user identity, authentication state, and authorization details.
/// </summary>
/// <param name="UserId">The unique identifier for the user.</param>
/// <param name="Email">The user's email address (typically used for login and notifications).</param>
/// <param name="Username">The user's login username (alternative to email for authentication).</param>
/// <param name="DisplayName">The user's full name or display name for UI presentation.</param>
/// <param name="AvatarUrl">URL to the user's profile avatar/picture image.</param>
/// <param name="TenantId">The tenant ID for multi-tenant support (user's organization/workspace).</param>
/// <param name="EmailConfirmed">Flag indicating whether the user's email address has been verified.</param>
/// <param name="TwoFactorEnabled">Flag indicating whether two-factor authentication is enabled for this account.</param>
/// <remarks>
/// This record is used throughout the application for passing user information between API layers,
/// UI components, and services. It includes both authentication state (EmailConfirmed, TwoFactorEnabled)
/// and authorization information (Roles, Permissions).
///
/// The Roles list contains role names assigned to the user (e.g., "Admin", "User", "Manager").
/// The Permissions list contains specific permission strings that override or supplement role-based permissions.
/// </remarks>
/// <example>
/// <code>
/// var userProfile = new UserProfileDto(
///     userId: "usr-12345",
///     email: "john@example.com",
///     username: "johnsmith",
///     displayName: "John Smith",
///     avatarUrl: "https://example.com/avatars/john.jpg",
///     tenantId: "tenant-abc",
///     emailConfirmed: true,
///     twoFactorEnabled: false)
/// {
///     Roles = ["User", "Manager"],
///     Permissions = ["view:reports", "edit:data"]
/// };
/// </code>
/// </example>
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
    /// <summary>
    /// Gets or sets the list of role names assigned to the user.
    /// Roles are used for role-based access control (RBAC) throughout the application.
    /// </summary>
    public List<string> Roles { get; init; } = new();

    /// <summary>
    /// Gets or sets the list of specific permission strings granted to the user.
    /// Permissions can supplement or override role-based permissions for fine-grained access control.
    /// </summary>
    public List<string> Permissions { get; init; } = new();
}
