using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Extensions;
using SmartWorkz.StarterKitMVC.Shared.Primitives;
using SmartWorkz.StarterKitMVC.Web.Middleware;
using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

/// <summary>
/// User API endpoints for managing user profiles, passwords, and 2FA.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get a user profile by ID.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user profile</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetById called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "User ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        // Users can only view their own profile unless they are admins
        if (currentUserId != id && !User.IsInRole("Admin"))
        {
            _logger.LogWarning("User {UserId} attempted to access profile of user {TargetId}", currentUserId, id);
            return Forbid();
        }

        _logger.LogInformation("Retrieving user profile: {Id}", id);
        var tenantId = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) ?? "DEFAULT";
        var result = await _userService.GetByIdAsync(id, tenantId);

        if (result == null)
        {
            _logger.LogWarning("User not found: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"User with ID {id} not found",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Get the current authenticated user's profile.
    /// </summary>
    /// <returns>The current user's profile</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetProfile called without valid user identity");
            return Unauthorized();
        }

        _logger.LogInformation("Retrieving profile for user: {UserId}", userId);
        var result = await _userService.GetByIdAsync(userId, "DEFAULT");

        if (result == null)
        {
            _logger.LogWarning("Profile not found for user: {UserId}", userId);
            return NotFound(ProblemDetailsResponse.NotFound(
                "User profile not found",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Update a user profile (Admin only).
    /// </summary>
    /// <param name="id">The user ID to update</param>
    /// <param name="request">The user update request</param>
    /// <returns>The updated user profile</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("Update user called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "User ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        if (request == null)
        {
            _logger.LogWarning("Update user called with null request");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Request body is required",
                new Dictionary<string, string[]> { ["body"] = new[] { "Request cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Updating user: {Id}", id);

        var tenantId = User.GetTenantId() ?? "DEFAULT";
        var existingUser = await _userService.GetByIdAsync(id, tenantId);
        if (existingUser == null)
        {
            _logger.LogWarning("User not found for update: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"User with ID {id} not found",
                Request.Path));
        }

        var updatedUser = new UserProfileDto(
            existingUser.UserId,
            existingUser.Email,
            existingUser.Username,
            request.DisplayName ?? existingUser.DisplayName,
            request.AvatarUrl ?? existingUser.AvatarUrl,
            existingUser.TenantId,
            existingUser.EmailConfirmed,
            existingUser.TwoFactorEnabled);

        var (success, user, error) = await _userService.UpdateAsync(updatedUser);

        if (!success || user == null)
        {
            _logger.LogWarning("User update failed for: {Id}: {Error}", id, error);
            return BadRequest(ProblemDetailsResponse.ValidationError(
                error ?? "Failed to update user",
                new Dictionary<string, string[]> { ["user"] = new[] { error ?? "Update failed" } },
                Request.Path));
        }

        return Ok(user);
    }

    /// <summary>
    /// Change the current user's password.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="request">The change password request</param>
    /// <returns>Success message</returns>
    [HttpPost("{id}/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("ChangePassword called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "User ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        // Users can only change their own password unless they are admins
        if (currentUserId != id && !User.IsInRole("Admin"))
        {
            _logger.LogWarning("User {UserId} attempted to change password for user {TargetId}", currentUserId, id);
            return Forbid();
        }

        if (request == null || string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            _logger.LogWarning("ChangePassword called with invalid request");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Current password and new password are required",
                new Dictionary<string, string[]>
                {
                    ["currentPassword"] = new[] { "Current password is required" },
                    ["newPassword"] = new[] { "New password is required" }
                },
                Request.Path));
        }

        _logger.LogInformation("Changing password for user: {Id}", id);

        var (success, error) = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);

        if (!success)
        {
            _logger.LogWarning("Password change failed for user: {Id}: {Error}", id, error);
            return BadRequest(ProblemDetailsResponse.ValidationError(
                error ?? "Password change failed. Verify your current password is correct.",
                new Dictionary<string, string[]> { ["currentPassword"] = new[] { error ?? "Invalid current password" } },
                Request.Path));
        }

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Enable two-factor authentication for a user.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>2FA setup information</returns>
    [HttpPost("{id}/enable-2fa")]
    [ProducesResponseType(typeof(Enable2FaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Enable2FA(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("Enable2FA called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "User ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        // Users can only enable 2FA for their own account unless they are admins
        if (currentUserId != id && !User.IsInRole("Admin"))
        {
            _logger.LogWarning("User {UserId} attempted to enable 2FA for user {TargetId}", currentUserId, id);
            return Forbid();
        }

        _logger.LogInformation("Enabling 2FA for user: {Id}", id);

        var (success, secret, error) = await _userService.EnableTwoFactorAsync(id);

        if (!success)
        {
            _logger.LogWarning("2FA setup failed for user: {Id}: {Error}", id, error);
            return BadRequest(ProblemDetailsResponse.ValidationError(
                error ?? "Failed to enable 2FA",
                new Dictionary<string, string[]> { ["2fa"] = new[] { error ?? "Setup failed" } },
                Request.Path));
        }

        return Ok(new { message = "2FA enabled successfully", secret });
    }

    /// <summary>
    /// Delete a user account (Admin only).
    /// </summary>
    /// <param name="id">The user ID to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("Delete user called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "User ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Deleting user: {Id}", id);

        var result = await _userService.DeleteAsync(id);

        if (!result)
        {
            _logger.LogWarning("User not found for deletion: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"User with ID {id} not found",
                Request.Path));
        }

        return NoContent();
    }
}

/// <summary>
/// Request model for updating a user.
/// </summary>
public record UpdateUserRequest(
    string? DisplayName = null,
    string? AvatarUrl = null,
    Dictionary<string, string>? Attributes = null);

/// <summary>
/// Request model for changing a password.
/// </summary>
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>
/// Response model for 2FA setup.
/// </summary>
public record Enable2FaResponse(
    string QrCodeUrl,
    List<string> RecoveryCodes,
    string ManualEntryKey);
