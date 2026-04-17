using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using System.Text.Json;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Implementation of role management service with caching.
/// Manages role creation, updates, and permission assignment.
/// </summary>
public class RoleService : IRoleService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IDistributedCache cache,
        ILogger<RoleService> logger)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<RoleDto?> GetByIdAsync(string roleId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

        try
        {
            var role = await _roleRepository.GetByIdAsync(roleId, tenantId);
            _logger.LogDebug("Retrieved role: {RoleId}", roleId);
            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role: {RoleId}", roleId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RoleDto?> GetByNameAsync(string roleName, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be empty", nameof(roleName));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        try
        {
            var role = await _roleRepository.GetByNameAsync(roleName, tenantId);
            _logger.LogDebug("Retrieved role by name: {RoleName}", roleName);
            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role by name: {RoleName}", roleName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetAllAsync(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        var cacheKey = GenerateCacheKey(tenantId);

        try
        {
            // Try cache first
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Cache hit for roles: {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<List<RoleDto>>(cachedData) ?? new List<RoleDto>();
            }

            // Cache miss - fetch from repository
            _logger.LogDebug("Cache miss for roles: {CacheKey}", cacheKey);
            var roles = await _roleRepository.GetAllAsync(tenantId);
            var rolesList = roles.OrderBy(r => r.Name).ToList();

            // Store in cache
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            };

            var serialized = JsonSerializer.Serialize(rolesList);
            await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

            _logger.LogDebug("Cached {Count} roles for tenant {TenantId}", rolesList.Count, tenantId);

            return rolesList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RoleDto> CreateAsync(RoleDto role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));
        if (string.IsNullOrWhiteSpace(role.Name))
            throw new ArgumentException("Role name is required", nameof(role.Name));

        try
        {
            role.RoleId = Guid.NewGuid().ToString();
            role.CreatedAt = DateTime.UtcNow;

            await _roleRepository.UpsertAsync(new Repositories.RoleDto
            {
                RoleId = role.RoleId,
                Name = role.Name,
                DisplayName = role.DisplayName,
                Description = role.Description,
                TenantId = role.TenantId,
                CreatedAt = role.CreatedAt
            });

            // Invalidate cache
            await InvalidateRoleCache(role.TenantId);

            _logger.LogInformation(
                "Role created: {RoleId} ({Name}) for tenant {TenantId}",
                role.RoleId, role.Name, role.TenantId);

            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {Name}", role.Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RoleDto> UpdateAsync(RoleDto role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));
        if (string.IsNullOrWhiteSpace(role.RoleId))
            throw new ArgumentException("Role ID is required", nameof(role.RoleId));
        if (string.IsNullOrWhiteSpace(role.Name))
            throw new ArgumentException("Role name is required", nameof(role.Name));

        try
        {
            role.UpdatedAt = DateTime.UtcNow;

            await _roleRepository.UpsertAsync(new Repositories.RoleDto
            {
                RoleId = role.RoleId,
                Name = role.Name,
                DisplayName = role.DisplayName,
                Description = role.Description,
                TenantId = role.TenantId,
                UpdatedAt = role.UpdatedAt
            });

            // Invalidate cache
            await InvalidateRoleCache(role.TenantId);

            _logger.LogInformation(
                "Role updated: {RoleId} ({Name})",
                role.RoleId, role.Name);

            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", role.RoleId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

        try
        {
            var role = await _roleRepository.GetByIdAsync(roleId, "");
            if (role == null)
                return false;

            var result = await _roleRepository.DeleteAsync(roleId);

            if (result)
            {
                // Invalidate cache
                await InvalidateRoleCache(role.TenantId);

                _logger.LogInformation("Role deleted: {RoleId}", roleId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", roleId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> AssignPermissionsAsync(string roleId, IEnumerable<string> permissionIds)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

        try
        {
            var permissionIdList = permissionIds?.ToList() ?? new List<string>();

            // Get the role to determine tenant
            var role = await _roleRepository.GetByIdAsync(roleId, "");
            if (role == null)
                return false;

            // Remove existing permissions
            var existingPermissions = await _permissionRepository.GetByRoleAsync(roleId);
            foreach (var existingPerm in existingPermissions)
            {
                await _permissionRepository.RemoveRolePermissionAsync(roleId, existingPerm.PermissionId);
            }

            // Assign new permissions
            foreach (var permissionId in permissionIdList)
            {
                await _permissionRepository.AssignToRoleAsync(roleId, permissionId);
            }

            // Invalidate cache
            await InvalidateRoleCache(role.TenantId);

            _logger.LogInformation(
                "Assigned {PermissionCount} permissions to role {RoleId}",
                permissionIdList.Count, roleId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permissions to role: {RoleId}", roleId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PermissionDto>> GetPermissionsAsync(string roleId)
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
            _logger.LogError(ex, "Error retrieving permissions for role: {RoleId}", roleId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemovePermissionAsync(string roleId, string permissionId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            throw new ArgumentException("Role ID cannot be empty", nameof(roleId));
        if (string.IsNullOrWhiteSpace(permissionId))
            throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

        try
        {
            var role = await _roleRepository.GetByIdAsync(roleId, "");
            if (role == null)
                return false;

            var result = await _permissionRepository.RemoveRolePermissionAsync(roleId, permissionId);

            if (result)
            {
                // Invalidate cache
                await InvalidateRoleCache(role.TenantId);

                _logger.LogDebug("Permission {PermissionId} removed from role {RoleId}",
                    permissionId, roleId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission from role: {RoleId}", roleId);
            return false;
        }
    }

    /// <summary>
    /// Invalidates cache for roles in a tenant.
    /// </summary>
    private async Task InvalidateRoleCache(string tenantId)
    {
        var cacheKey = GenerateCacheKey(tenantId);
        await _cache.RemoveAsync(cacheKey);
        _logger.LogDebug("Cache invalidated for roles: {CacheKey}", cacheKey);
    }

    /// <summary>
    /// Generates a cache key for role list.
    /// Format: roles_{tenantId}
    /// </summary>
    private static string GenerateCacheKey(string tenantId)
        => $"roles_{tenantId}";
}
