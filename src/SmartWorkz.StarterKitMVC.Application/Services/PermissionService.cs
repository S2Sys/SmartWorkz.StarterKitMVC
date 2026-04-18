using SmartWorkz.StarterKitMVC.Shared.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using System.Text.Json;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Implementation of permission authorization service.
/// Evaluates user permissions with caching for performance.
/// </summary>
public class PermissionService : IPermissionService
{
    private static readonly TimeSpan PermissionCacheDuration = TimeSpan.FromMinutes(15);

    private readonly IUserRepository _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        IUserRepository userRepository,
        IPermissionRepository permissionRepository,
        IDistributedCache cache,
        ILogger<PermissionService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<bool> UserHasPermissionAsync(string userId, string permissionName, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(permissionName))
            throw new ArgumentException("Permission name cannot be empty", nameof(permissionName));

        try
        {
            var permissions = await GetUserPermissionsAsync(userId, tenantId);
            var hasPermission = permissions.Contains(permissionName, StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug(
                "Permission check for user {UserId}: {PermissionName} = {Result}",
                userId, permissionName, hasPermission);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking permission {PermissionName} for user {UserId}",
                permissionName, userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UserHasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (permissionNames == null || !permissionNames.Any())
            return false;

        try
        {
            var userPermissions = await GetUserPermissionsAsync(userId, tenantId);

            var hasAny = permissionNames.Any(perm =>
                userPermissions.Contains(perm, StringComparer.OrdinalIgnoreCase));

            _logger.LogDebug(
                "Any permission check for user {UserId}: {Result}",
                userId, hasAny);

            return hasAny;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking any permissions for user {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UserHasAllPermissionsAsync(string userId, IEnumerable<string> permissionNames, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (permissionNames == null || !permissionNames.Any())
            return true;

        try
        {
            var userPermissions = await GetUserPermissionsAsync(userId, tenantId);

            var hasAll = permissionNames.All(perm =>
                userPermissions.Contains(perm, StringComparer.OrdinalIgnoreCase));

            _logger.LogDebug(
                "All permissions check for user {UserId}: {Result}",
                userId, hasAll);

            return hasAll;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking all permissions for user {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        var cacheKey = GenerateUserPermissionsCacheKey(userId, tenantId);

        try
        {
            // Try cache first
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Cache hit for user permissions: {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<List<string>>(cachedData) ?? new List<string>();
            }

            // Cache miss - fetch from repositories
            _logger.LogDebug("Cache miss for user permissions: {CacheKey}", cacheKey);

            // Get user's roles and their permissions
            var userRoles = await _userRepository.GetUserRolesAsync(userId, tenantId);
            var rolePermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (userRoles != null && userRoles.Any())
            {
                foreach (var roleId in userRoles)
                {
                    var rolePerms = await _permissionRepository.GetByRoleAsync(roleId);
                    foreach (var perm in rolePerms)
                    {
                        rolePermissions.Add(perm.Name);
                    }
                }
            }

            // Get user's direct permissions
            var directPermissions = await _userRepository.GetUserPermissionsAsync(userId, tenantId);
            if (directPermissions != null)
            {
                foreach (var perm in directPermissions)
                {
                    rolePermissions.Add(perm);
                }
            }

            var permissionsList = rolePermissions.ToList();

            // Store in cache
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PermissionCacheDuration
            };

            var serialized = JsonSerializer.Serialize(permissionsList);
            await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

            _logger.LogDebug("Cached {Count} permissions for user {UserId}",
                permissionsList.Count, userId);

            return permissionsList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving permissions for user {UserId}",
                userId);
            return new List<string>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PermissionDto>> GetByRoleAsync(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

        try
        {
            var permissions = await _permissionRepository.GetByRoleAsync(roleId);
            _logger.LogDebug("Retrieved {Count} permissions for role {RoleId}",
                permissions.Count(), roleId);
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for role {RoleId}", roleId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RoleHasPermissionAsync(string roleId, string permissionName)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            throw new ArgumentException("Role ID cannot be empty", nameof(roleId));
        if (string.IsNullOrWhiteSpace(permissionName))
            throw new ArgumentException("Permission name cannot be empty", nameof(permissionName));

        try
        {
            var permissions = await _permissionRepository.GetByRoleAsync(roleId);
            var hasPermission = permissions.Any(p =>
                p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));

            _logger.LogDebug(
                "Permission check for role {RoleId}: {PermissionName} = {Result}",
                roleId, permissionName, hasPermission);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking permission {PermissionName} for role {RoleId}",
                permissionName, roleId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PermissionDto>> GetAllAsync(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        try
        {
            var permissions = await _permissionRepository.GetAllAsync(tenantId);
            _logger.LogDebug("Retrieved {Count} permissions for tenant {TenantId}",
                permissions.Count(), tenantId);
            return permissions.OrderBy(p => p.Category).ThenBy(p => p.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all permissions for tenant {TenantId}", tenantId);
            throw;
        }
    }

    /// <summary>
    /// Invalidates permission cache for a user.
    /// Called when role or permission assignments change.
    /// </summary>
    public async Task InvalidateUserPermissionsCacheAsync(string userId, string tenantId)
    {
        var cacheKey = GenerateUserPermissionsCacheKey(userId, tenantId);
        await _cache.RemoveAsync(cacheKey);
        _logger.LogDebug("Cache invalidated for user permissions: {CacheKey}", cacheKey);
    }

    /// <summary>
    /// Generates a cache key for user permissions.
    /// Format: permissions_{userId}_{tenantId}
    /// </summary>
    private static string GenerateUserPermissionsCacheKey(string userId, string tenantId)
        => $"permissions_{userId}_{tenantId}";
}
