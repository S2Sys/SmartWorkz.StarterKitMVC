using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Validation;
using SmartWorkz.StarterKitMVC.Web.Middleware;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

/// <summary>
/// Authentication endpoints for login, registration, token refresh, and password management.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and returns JWT access token and refresh token.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/login
    ///     {
    ///       "email": "user@example.com",
    ///       "password": "SecurePass123",
    ///       "tenantId": "tenant-id"
    ///     }
    ///
    /// Returns a LoginResponse with accessToken, refreshToken, and user profile.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!AuthValidators.IsValidLogin(request))
        {
            var errors = new List<ValidationFailure>();
            if (string.IsNullOrWhiteSpace(request.Email))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.Email), ErrorMessage = "Email is required" });
            if (string.IsNullOrWhiteSpace(request.Password))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.Password), ErrorMessage = "Password is required" });
            if (string.IsNullOrWhiteSpace(request.TenantId))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.TenantId), ErrorMessage = "Tenant ID is required" });
            throw new ValidationException(errors);
        }

        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!AuthValidators.IsValidRegister(request))
        {
            var errors = new List<ValidationFailure>();
            if (string.IsNullOrWhiteSpace(request.Email) || !AuthValidators.IsValidEmail(request.Email))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.Email), ErrorMessage = "Valid email is required" });
            if (string.IsNullOrWhiteSpace(request.Password) || !AuthValidators.IsValidPassword(request.Password))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.Password), ErrorMessage = "Password must be at least 8 characters with uppercase, lowercase, and digit" });
            if (string.IsNullOrWhiteSpace(request.TenantId))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.TenantId), ErrorMessage = "Tenant ID is required" });
            if (errors.Any()) throw new ValidationException(errors);
        }

        var result = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetProfile), result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (!AuthValidators.IsValidRefreshToken(request))
        {
            var errors = new List<ValidationFailure>();
            if (string.IsNullOrWhiteSpace(request.AccessToken))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.AccessToken), ErrorMessage = "Access token is required" });
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.RefreshToken), ErrorMessage = "Refresh token is required" });
            throw new ValidationException(errors);
        }

        var result = await _authService.RefreshTokenAsync(request);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Revoke([FromBody] string refreshToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        await _authService.RevokeTokenAsync(userId, refreshToken);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!AuthValidators.IsValidForgotPassword(request))
        {
            var errors = new List<ValidationFailure>();
            if (string.IsNullOrWhiteSpace(request.Email) || !AuthValidators.IsValidEmail(request.Email))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.Email), ErrorMessage = "Valid email is required" });
            if (string.IsNullOrWhiteSpace(request.TenantId))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.TenantId), ErrorMessage = "Tenant ID is required" });
            throw new ValidationException(errors);
        }

        await _authService.ForgotPasswordAsync(request);
        return Ok(new { message = "If the email exists, a reset link has been sent" });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!AuthValidators.IsValidResetPassword(request))
        {
            var errors = new List<ValidationFailure>();
            if (string.IsNullOrWhiteSpace(request.Email) || !AuthValidators.IsValidEmail(request.Email))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.Email), ErrorMessage = "Valid email is required" });
            if (string.IsNullOrWhiteSpace(request.Token))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.Token), ErrorMessage = "Reset token is required" });
            if (string.IsNullOrWhiteSpace(request.NewPassword) || !AuthValidators.IsValidPassword(request.NewPassword))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.NewPassword), ErrorMessage = "Password must be at least 8 characters with uppercase, lowercase, and digit" });
            if (string.IsNullOrWhiteSpace(request.TenantId))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.TenantId), ErrorMessage = "Tenant ID is required" });
            if (errors.Any()) throw new ValidationException(errors);
        }

        var success = await _authService.ResetPasswordAsync(request);
        if (!success)
            return BadRequest(new { message = "Invalid or expired reset token" });

        return Ok(new { message = "Password reset successfully" });
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!AuthValidators.IsValidChangePassword(request))
        {
            var errors = new List<ValidationFailure>();
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.CurrentPassword), ErrorMessage = "Current password is required" });
            if (string.IsNullOrWhiteSpace(request.NewPassword) || !AuthValidators.IsValidPassword(request.NewPassword))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.NewPassword), ErrorMessage = "New password must be at least 8 characters with uppercase, lowercase, and digit" });
            if (request.CurrentPassword == request.NewPassword)
                errors.Add(new ValidationFailure { PropertyName = nameof(request.NewPassword), ErrorMessage = "New password cannot be the same as current password" });
            throw new ValidationException(errors);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        var success = await _authService.ChangePasswordAsync(userId, request);
        if (!success)
            return BadRequest(new { message = "Current password is incorrect" });

        return Ok(new { message = "Password changed successfully" });
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        if (!AuthValidators.IsValidVerifyEmail(request))
        {
            var errors = new List<ValidationFailure>();
            if (string.IsNullOrWhiteSpace(request.Email) || !AuthValidators.IsValidEmail(request.Email))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.Email), ErrorMessage = "Valid email is required" });
            if (string.IsNullOrWhiteSpace(request.Token))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.Token), ErrorMessage = "Verification token is required" });
            if (string.IsNullOrWhiteSpace(request.TenantId))
                errors.Add(new ValidationFailure { PropertyName = nameof(request.TenantId), ErrorMessage = "Tenant ID is required" });
            throw new ValidationException(errors);
        }

        var success = await _authService.VerifyEmailAsync(request);
        if (!success)
            return BadRequest(new { message = "Invalid or expired verification token" });

        return Ok(new { message = "Email verified successfully" });
    }

    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var profile = await _authService.GetProfileAsync(userId);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }
}
