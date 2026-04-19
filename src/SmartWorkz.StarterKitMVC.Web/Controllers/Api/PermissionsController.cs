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
/// Permissions API endpoints for managing application permissions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available permissions.
    /// </summary>
    /// <returns>List of all permissions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Retrieving all permissions");
        var tenantId = User.GetTenantId() ?? "DEFAULT";
        var result = await _permissionService.GetAllAsync(tenantId);
        return Ok(result);
    }

    // TODO: Implement GetById once IPermissionService.GetByIdAsync is available
    // Currently commented out as service doesn't have a GetByIdAsync method
    /*
    /// <summary>
    /// Get a specific permission by ID.
    /// </summary>
    /// <param name="id">The permission ID</param>
    /// <returns>The permission details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
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
                "Permission ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Retrieving permission: {Id}", id);
        var result = await _permissionService.GetPermissionByIdAsync(id);

        if (result == null)
        {
            _logger.LogWarning("Permission not found: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Permission with ID {id} not found",
                Request.Path));
        }

        return Ok(result);
    }
    */
}

/// <summary>
/// Response model for permission information.
/// </summary>
public record PermissionDto(
    string Id,
    string Name,
    string? Description = null,
    string? Category = null)
{
    public DateTime CreatedAt { get; init; }
}
