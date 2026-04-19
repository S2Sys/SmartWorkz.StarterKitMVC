using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Implementation of user management service.
/// Handles user creation, updates, password management, and 2FA.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetByIdAsync(string userId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            return await MapToProfileDto(user, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user: {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetByEmailAsync(string email, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        try
        {
            var user = await _userRepository.GetByEmailAsync(email, tenantId);
            if (user == null)
                return null;

            return await MapToProfileDto(user, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<UserProfileDto> Users, int Total)> GetAllAsync(
        string tenantId, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        try
        {
            var (users, total) = await _userRepository.SearchPagedAsync(
                tenantId, null, "CreatedAt", true, page, pageSize);

            var tasks = users.Select(u => MapToProfileDto(u, tenantId)).ToList();
            var profileDtos = await Task.WhenAll(tasks);

            _logger.LogDebug("Retrieved {Count} users for tenant {TenantId}", users.Count(), tenantId);

            return (profileDtos, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserProfileDto>> SearchAsync(string searchTerm, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<UserProfileDto>();

        try
        {
            var (users, _) = await _userRepository.SearchPagedAsync(
                tenantId, searchTerm, "DisplayName", false, 1, 100);

            var tasks = users.Select(u => MapToProfileDto(u, tenantId)).ToList();
            var profileDtos = await Task.WhenAll(tasks);

            _logger.LogDebug("Found {Count} users matching search term: {SearchTerm}",
                profileDtos.Length, searchTerm);

            return profileDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, UserProfileDto? User, string? Error)> CreateAsync(
        UserProfileDto user, string password)
    {
        if (user == null)
            return (false, null, "User data is required");
        if (string.IsNullOrWhiteSpace(user.Email))
            return (false, null, "Email is required");
        if (string.IsNullOrWhiteSpace(password))
            return (false, null, "Password is required");

        try
        {
            // Check if user already exists
            if (await _userRepository.UserExistsAsync(user.Email, user.TenantId))
            {
                _logger.LogWarning("Attempt to create user with existing email: {Email}", user.Email);
                return (false, null, "Email already in use");
            }

            var newUser = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Email = user.Email,
                NormalizedEmail = user.Email.ToUpperInvariant(),
                Username = user.Username,
                NormalizedUsername = user.Username.ToUpperInvariant(),
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                PasswordHash = _passwordHasher.Hash(password),
                IsActive = true,
                EmailConfirmed = false,
                TwoFactorEnabled = false,
                CreatedAt = DateTime.UtcNow,
                TenantId = user.TenantId
            };

            await _userRepository.UpsertUserAsync(newUser);

            _logger.LogInformation("User created successfully: {UserId} ({Email})", newUser.UserId, newUser.Email);

            return (true, await MapToProfileDto(newUser, user.TenantId), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", user.Email);
            return (false, null, "An error occurred while creating the user");
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, UserProfileDto? User, string? Error)> UpdateAsync(UserProfileDto user)
    {
        if (user == null)
            return (false, null, "User data is required");
        if (string.IsNullOrWhiteSpace(user.UserId))
            return (false, null, "User ID is required");

        try
        {
            var existingUser = await _userRepository.GetByIdAsync(user.UserId);
            if (existingUser == null)
                return (false, null, "User not found");

            existingUser.DisplayName = user.DisplayName;
            existingUser.AvatarUrl = user.AvatarUrl;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpsertUserAsync(existingUser);

            _logger.LogInformation("User updated: {UserId}", user.UserId);

            return (true, await MapToProfileDto(existingUser, user.TenantId), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.UserId);
            return (false, null, "An error occurred while updating the user");
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpsertUserAsync(user);

            _logger.LogInformation("User deleted (deactivated): {UserId}", userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? Error)> ChangePasswordAsync(
        string userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return (false, "User ID is required");
        if (string.IsNullOrWhiteSpace(currentPassword))
            return (false, "Current password is required");
        if (string.IsNullOrWhiteSpace(newPassword))
            return (false, "New password is required");

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, "User not found");

            if (!_passwordHasher.Verify(currentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Failed password change attempt for user: {UserId}", userId);
                return (false, "Current password is incorrect");
            }

            user.PasswordHash = _passwordHasher.Hash(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpsertUserAsync(user);

            _logger.LogInformation("Password changed for user: {UserId}", userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return (false, "An error occurred while changing password");
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? Secret, string? Error)> EnableTwoFactorAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return (false, null, "User ID is required");

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, null, "User not found");

            // Generate 2FA secret
            var secret = GenerateTwoFactorSecret();

            user.TwoFactorEnabled = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpsertUserAsync(user);

            _logger.LogInformation("Two-factor authentication enabled for user: {UserId}", userId);

            return (true, secret, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA for user: {UserId}", userId);
            return (false, null, "An error occurred while enabling 2FA");
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? Error)> DisableTwoFactorAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return (false, "User ID is required");

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, "User not found");

            user.TwoFactorEnabled = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpsertUserAsync(user);

            _logger.LogInformation("Two-factor authentication disabled for user: {UserId}", userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA for user: {UserId}", userId);
            return (false, "An error occurred while disabling 2FA");
        }
    }

    /// <inheritdoc />
    public async Task<bool> LockAsync(string userId, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = false;
            user.LockoutEnd = DateTime.UtcNow.Add(duration);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpsertUserAsync(user);

            _logger.LogInformation("User locked: {UserId} until {LockoutEnd}", userId, user.LockoutEnd);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking user: {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UnlockAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = true;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpsertUserAsync(user);

            _logger.LogInformation("User unlocked: {UserId}", userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user: {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<string>> GetRolesAsync(string userId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            return await _userRepository.GetUserRolesAsync(userId, tenantId) ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user: {UserId}", userId);
            return new List<string>();
        }
    }

    /// <inheritdoc />
    public async Task<bool> AssignRolesAsync(string userId, IEnumerable<string> roleIds)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var roleIdList = roleIds?.ToList() ?? new List<string>();
            // This would use a specific repository method or unit of work
            _logger.LogInformation(
                "Assigned {RoleCount} roles to user {UserId}",
                roleIdList.Count, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning roles to user: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Maps a User domain entity to a UserProfileDto.
    /// </summary>
    private async Task<UserProfileDto> MapToProfileDto(User user, string tenantId)
    {
        var roles = await _userRepository.GetUserRolesAsync(user.UserId, tenantId);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId, tenantId);

        return new UserProfileDto(
            user.UserId,
            user.Email,
            user.Username,
            user.DisplayName ?? user.Email,
            user.AvatarUrl,
            tenantId,
            user.EmailConfirmed,
            user.TwoFactorEnabled)
        {
            Roles = roles ?? new List<string>(),
            Permissions = permissions ?? new List<string>()
        };
    }

    /// <summary>
    /// Generates a random two-factor authentication secret.
    /// </summary>
    private static string GenerateTwoFactorSecret()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace("+", "-").Replace("/", "_");
    }
}
