using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for user authentication with JWT token generation and refresh token management.
/// Handles login, registration, password reset, and token validation.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// Returns JWT access token, refresh token, and user profile on success.
    /// </summary>
    Task<(bool Success, LoginResponse? Response, string? Error)> LoginAsync(
        string email, string password, string tenantId);

    /// <summary>
    /// Validates user credentials without generating tokens.
    /// Used for sensitive operations requiring re-authentication.
    /// </summary>
    Task<bool> ValidateCredentialsAsync(string email, string password, string tenantId);

    /// <summary>
    /// Registers a new user account.
    /// Returns JWT tokens and user profile on success.
    /// </summary>
    Task<(bool Success, LoginResponse? Response, string? Error)> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Initiates password reset flow by sending email with reset token.
    /// </summary>
    Task<(bool Success, string? Error)> SendPasswordResetEmailAsync(string email, string tenantId);

    /// <summary>
    /// Completes password reset with token and new password.
    /// </summary>
    Task<(bool Success, string? Error)> ResetPasswordAsync(string token, string newPassword, string tenantId);

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// Token includes claims for sub, email, name, tenant, roles, and permissions.
    /// </summary>
    Task<string> GenerateJwtTokenAsync(UserProfileDto user);

    /// <summary>
    /// Generates a refresh token that can be used to obtain new access tokens.
    /// Refresh tokens have extended expiration (7 days default).
    /// </summary>
    Task<string> GenerateRefreshTokenAsync(string userId, string tenantId);

    /// <summary>
    /// Validates a refresh token and checks if it's still valid and not revoked.
    /// </summary>
    Task<bool> ValidateRefreshTokenAsync(string token, string userId, string tenantId);

    /// <summary>
    /// Refreshes an access token using a valid refresh token.
    /// Returns new access token if refresh token is valid.
    /// </summary>
    Task<(bool Success, LoginResponse? Response, string? Error)> RefreshAccessTokenAsync(
        string refreshToken, string tenantId);

    /// <summary>
    /// Revokes a refresh token, preventing further use.
    /// </summary>
    Task<bool> RevokeRefreshTokenAsync(string userId, string refreshToken, string tenantId);
}
