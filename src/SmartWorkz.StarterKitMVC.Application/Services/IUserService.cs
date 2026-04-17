using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for managing user accounts and profiles.
/// Handles user CRUD, password management, and two-factor authentication.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    Task<UserProfileDto?> GetByIdAsync(string userId, string tenantId);

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    Task<UserProfileDto?> GetByEmailAsync(string email, string tenantId);

    /// <summary>
    /// Gets all users for a tenant with pagination.
    /// </summary>
    Task<(IEnumerable<UserProfileDto> Users, int Total)> GetAllAsync(
        string tenantId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Searches users by email or display name.
    /// </summary>
    Task<IEnumerable<UserProfileDto>> SearchAsync(string searchTerm, string tenantId);

    /// <summary>
    /// Creates a new user account.
    /// </summary>
    Task<(bool Success, UserProfileDto? User, string? Error)> CreateAsync(UserProfileDto user, string password);

    /// <summary>
    /// Updates user profile information.
    /// </summary>
    Task<(bool Success, UserProfileDto? User, string? Error)> UpdateAsync(UserProfileDto user);

    /// <summary>
    /// Deletes a user account.
    /// </summary>
    Task<bool> DeleteAsync(string userId);

    /// <summary>
    /// Changes a user's password with validation of current password.
    /// </summary>
    Task<(bool Success, string? Error)> ChangePasswordAsync(
        string userId, string currentPassword, string newPassword);

    /// <summary>
    /// Enables two-factor authentication for a user.
    /// </summary>
    Task<(bool Success, string? Secret, string? Error)> EnableTwoFactorAsync(string userId);

    /// <summary>
    /// Disables two-factor authentication for a user.
    /// </summary>
    Task<(bool Success, string? Error)> DisableTwoFactorAsync(string userId);

    /// <summary>
    /// Locks a user account temporarily.
    /// </summary>
    Task<bool> LockAsync(string userId, TimeSpan duration);

    /// <summary>
    /// Unlocks a user account.
    /// </summary>
    Task<bool> UnlockAsync(string userId);

    /// <summary>
    /// Gets user's roles.
    /// </summary>
    Task<List<string>> GetRolesAsync(string userId, string tenantId);

    /// <summary>
    /// Assigns roles to a user.
    /// </summary>
    Task<bool> AssignRolesAsync(string userId, IEnumerable<string> roleIds);
}
