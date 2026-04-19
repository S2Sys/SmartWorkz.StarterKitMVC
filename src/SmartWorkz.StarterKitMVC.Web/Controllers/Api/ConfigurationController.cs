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
/// Configuration API endpoints for managing system configuration settings.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(IConfigurationService configurationService, ILogger<ConfigurationController> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all configuration entries.
    /// </summary>
    /// <returns>List of all configuration entries</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = User.GetTenantId() ?? "DEFAULT";
        _logger.LogInformation("Retrieving all configurations");
        var result = await _configurationService.GetAllForTenantAsync(tenantId);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific configuration entry by key.
    /// </summary>
    /// <param name="key">The configuration key</param>
    /// <returns>The configuration entry</returns>
    [HttpGet("{key}")]
    [ProducesResponseType(typeof(ConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Get configuration called with empty key");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Configuration key is required",
                new Dictionary<string, string[]> { ["key"] = new[] { "Key cannot be empty" } },
                Request.Path));
        }

        var tenantId = User.GetTenantId() ?? "DEFAULT";
        _logger.LogInformation("Retrieving configuration: {Key}", key);
        var result = await _configurationService.GetByKeyAsync(key, tenantId);

        if (result == null)
        {
            _logger.LogWarning("Configuration not found: {Key}", key);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Configuration key '{key}' not found",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new configuration entry.
    /// </summary>
    /// <param name="request">The configuration creation request</param>
    /// <returns>The created configuration entry</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ConfigurationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateConfigurationRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Create configuration called with null request");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Request body is required",
                new Dictionary<string, string[]> { ["body"] = new[] { "Request cannot be empty" } },
                Request.Path));
        }

        if (string.IsNullOrWhiteSpace(request.Key))
        {
            _logger.LogWarning("Create configuration called with empty key");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Configuration key is required",
                new Dictionary<string, string[]> { ["key"] = new[] { "Key cannot be empty" } },
                Request.Path));
        }

        var userId = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) ?? "system";
        var tenantId = User.GetTenantId() ?? "DEFAULT";
        _logger.LogInformation("Creating configuration: {Key}", request.Key);

        var config = new ConfigurationDto
        {
            ConfigurationId = Guid.NewGuid(),
            Key = request.Key,
            Value = request.Value,
            Description = request.Description,
            IsEncrypted = request.IsEncrypted,
            TenantId = tenantId,
            CreatedBy = userId
        };
        var result = await _configurationService.SaveAsync(config);
        return CreatedAtAction(nameof(Get), new { key = request.Key }, result);
    }

    /// <summary>
    /// Update an existing configuration entry.
    /// </summary>
    /// <param name="key">The configuration key to update</param>
    /// <param name="request">The configuration update request</param>
    /// <returns>The updated configuration entry</returns>
    [HttpPut("{key}")]
    [ProducesResponseType(typeof(ConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateConfigurationRequest request)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Update configuration called with empty key");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Configuration key is required",
                new Dictionary<string, string[]> { ["key"] = new[] { "Key cannot be empty" } },
                Request.Path));
        }

        if (request == null)
        {
            _logger.LogWarning("Update configuration called with null request");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Request body is required",
                new Dictionary<string, string[]> { ["body"] = new[] { "Request cannot be empty" } },
                Request.Path));
        }

        var userId = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) ?? "system";
        var tenantId = User.GetTenantId() ?? "DEFAULT";
        _logger.LogInformation("Updating configuration: {Key}", key);

        var existing = await _configurationService.GetByKeyAsync(key, tenantId);
        if (existing == null)
        {
            _logger.LogWarning("Configuration not found for update: {Key}", key);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Configuration key '{key}' not found",
                Request.Path));
        }

        var config = new ConfigurationDto
        {
            ConfigurationId = existing.ConfigurationId,
            Key = key,
            Value = request.Value ?? existing.Value,
            Description = request.Description ?? existing.Description,
            IsEncrypted = existing.IsEncrypted,
            TenantId = tenantId,
            CreatedBy = existing.CreatedBy,
            UpdatedBy = userId,
            UpdatedAt = DateTime.UtcNow
        };
        var result = await _configurationService.SaveAsync(config);

        if (result == null)
        {
            _logger.LogWarning("Configuration not found for update: {Key}", key);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Configuration key '{key}' not found",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a configuration entry.
    /// </summary>
    /// <param name="key">The configuration key to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Delete configuration called with empty key");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Configuration key is required",
                new Dictionary<string, string[]> { ["key"] = new[] { "Key cannot be empty" } },
                Request.Path));
        }

        var tenantId = User.GetTenantId() ?? "DEFAULT";
        _logger.LogInformation("Deleting configuration: {Key}", key);

        var result = await _configurationService.DeleteAsync(key, tenantId);

        if (!result)
        {
            _logger.LogWarning("Configuration not found for deletion: {Key}", key);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Configuration key '{key}' not found",
                Request.Path));
        }

        return NoContent();
    }
}

/// <summary>
/// Request model for creating a configuration entry.
/// </summary>
public record CreateConfigurationRequest(
    string Key,
    string Value,
    string Description,
    bool IsEncrypted = false);

/// <summary>
/// Request model for updating a configuration entry.
/// </summary>
public record UpdateConfigurationRequest(
    string? Value = null,
    string? Description = null);
