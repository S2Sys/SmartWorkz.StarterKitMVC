using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Extensions;
using SmartWorkz.StarterKitMVC.Shared.Primitives;
using SmartWorkz.StarterKitMVC.Web.Middleware;
using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

/// <summary>
/// Roles API endpoints for managing application roles and permissions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available roles.
    /// </summary>
    /// <returns>List of all roles</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = User.GetTenantId() ?? "DEFAULT";
        _logger.LogInformation("Retrieving all roles");
        var result = await _roleService.GetAllAsync(tenantId);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific role by ID.
    /// </summary>
    /// <param name="id">The role ID</param>
    /// <returns>The role details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
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
                "Role ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        var tenantId = User.GetTenantId() ?? "DEFAULT";
        _logger.LogInformation("Retrieving role: {Id}", id);
        var result = await _roleService.GetByIdAsync(id, tenantId);

        if (result == null)
        {
            _logger.LogWarning("Role not found: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Role with ID {id} not found",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new role.
    /// </summary>
    /// <param name="request">The role creation request</param>
    /// <returns>The created role</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Create role called with null request");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Request body is required",
                new Dictionary<string, string[]> { ["body"] = new[] { "Request cannot be empty" } },
                Request.Path));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Create role called with empty name");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Role name is required",
                new Dictionary<string, string[]> { ["name"] = new[] { "Name cannot be empty" } },
                Request.Path));
        }

        var userId = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) ?? "system";
        var tenantId = User.GetTenantId() ?? "DEFAULT";
        _logger.LogInformation("Creating role: {Name}", request.Name);

        var role = new RoleDto
        {
            RoleId = Guid.NewGuid(),
            Name = request.Name,
            DisplayName = request.Name,
            Description = request.Description,
            TenantId = tenantId,
            CreatedBy = userId
        };
        var result = await _roleService.CreateAsync(role);
        return CreatedAtAction(nameof(GetById), new { id = result.RoleId.ToString() }, result);
    }

    /// <summary>
    /// Update an existing role.
    /// </summary>
    /// <param name="id">The role ID to update</param>
    /// <param name="request">The role update request</param>
    /// <returns>The updated role</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("Update role called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Role ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        if (request == null)
        {
            _logger.LogWarning("Update role called with null request");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Request body is required",
                new Dictionary<string, string[]> { ["body"] = new[] { "Request cannot be empty" } },
                Request.Path));
        }

        var userId = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) ?? "system";
        _logger.LogInformation("Updating role: {Id}", id);

        var role = new RoleDto
        {
            RoleId = Guid.Parse(id),
            Name = request.Name,
            DisplayName = request.Name,
            Description = request.Description,
            UpdatedBy = userId,
            UpdatedAt = DateTime.UtcNow
        };
        var result = await _roleService.UpdateAsync(role);

        if (result == null)
        {
            _logger.LogWarning("Role not found for update: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Role with ID {id} not found",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a role.
    /// </summary>
    /// <param name="id">The role ID to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
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
            _logger.LogWarning("Delete role called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Role ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Deleting role: {Id}", id);

        var result = await _roleService.DeleteAsync(id);

        if (!result)
        {
            _logger.LogWarning("Role not found for deletion: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Role with ID {id} not found",
                Request.Path));
        }

        return NoContent();
    }

    /// <summary>
    /// Assign permissions to a role.
    /// </summary>
    /// <param name="id">The role ID</param>
    /// <param name="request">The permissions assignment request</param>
    /// <returns>The updated role with permissions</returns>
    [HttpPost("{id}/permissions")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignPermissions(string id, [FromBody] AssignPermissionsRequest request)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("AssignPermissions called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Role ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        if (request == null || request.PermissionIds == null || !request.PermissionIds.Any())
        {
            _logger.LogWarning("AssignPermissions called with empty permissions");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "At least one permission ID is required",
                new Dictionary<string, string[]> { ["permissionIds"] = new[] { "Permission list cannot be empty" } },
                Request.Path));
        }

        var tenantId = User.GetTenantId() ?? "DEFAULT";
        _logger.LogInformation("Assigning permissions to role: {Id}", id);

        var result = await _roleService.AssignPermissionsAsync(id, request.PermissionIds, tenantId);

        if (result == null)
        {
            _logger.LogWarning("Role not found for permission assignment: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Role with ID {id} not found",
                Request.Path));
        }

        return Ok(result);
    }
}

/// <summary>
/// Request model for creating a role.
/// </summary>
public record CreateRoleRequest(
    string Name,
    string? Description = null);

/// <summary>
/// Request model for updating a role.
/// </summary>
public record UpdateRoleRequest(
    string? Name = null,
    string? Description = null);

/// <summary>
/// Request model for assigning permissions to a role.
/// </summary>
public record AssignPermissionsRequest(List<string> PermissionIds);

