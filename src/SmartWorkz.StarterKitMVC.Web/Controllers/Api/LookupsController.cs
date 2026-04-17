using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Primitives;
using SmartWorkz.StarterKitMVC.Web.Middleware;
using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

/// <summary>
/// Lookups/LoV (List of Values) API endpoints for managing dropdown lists and system reference data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookupService;
    private readonly ILogger<LookupsController> _logger;

    public LookupsController(ILookupService lookupService, ILogger<LookupsController> logger)
    {
        _lookupService = lookupService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available currencies.
    /// </summary>
    /// <returns>List of currency lookups</returns>
    [HttpGet("currencies")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<LookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrencies()
    {
        _logger.LogInformation("Retrieving currencies");
        var result = await _lookupService.GetLookupsByCategoryAsync("Currency");
        return Ok(result);
    }

    /// <summary>
    /// Get all available languages.
    /// </summary>
    /// <returns>List of language lookups</returns>
    [HttpGet("languages")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<LookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLanguages()
    {
        _logger.LogInformation("Retrieving languages");
        var result = await _lookupService.GetLookupsByCategoryAsync("Language");
        return Ok(result);
    }

    /// <summary>
    /// Get all available time zones.
    /// </summary>
    /// <returns>List of time zone lookups</returns>
    [HttpGet("timezones")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<LookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTimeZones()
    {
        _logger.LogInformation("Retrieving time zones");
        var result = await _lookupService.GetLookupsByCategoryAsync("TimeZone");
        return Ok(result);
    }

    /// <summary>
    /// Get lookups by category key.
    /// </summary>
    /// <param name="categoryKey">The category key to retrieve lookups for</param>
    /// <returns>List of lookups in the specified category</returns>
    [HttpGet("{categoryKey}")]
    [ProducesResponseType(typeof(IEnumerable<LookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCategory(string categoryKey)
    {
        if (string.IsNullOrWhiteSpace(categoryKey))
        {
            _logger.LogWarning("GetByCategory called with empty category key");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Category key is required",
                new Dictionary<string, string[]> { ["categoryKey"] = new[] { "Category key cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Retrieving lookups for category: {CategoryKey}", categoryKey);
        var result = await _lookupService.GetLookupsByCategoryAsync(categoryKey);

        if (result == null || !result.Any())
        {
            _logger.LogWarning("No lookups found for category: {CategoryKey}", categoryKey);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"No lookups found for category: {categoryKey}",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new lookup entry (Admin only).
    /// </summary>
    /// <param name="request">The lookup creation request</param>
    /// <returns>The created lookup</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(LookupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateLookupRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Create lookup called with null request");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Request body is required",
                new Dictionary<string, string[]> { ["body"] = new[] { "Request cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Creating lookup: {Category} - {Code}", request.Category, request.Code);

        var result = await _lookupService.CreateLookupAsync(request);
        return CreatedAtAction(nameof(GetByCategory), new { categoryKey = request.Category }, result);
    }

    /// <summary>
    /// Update an existing lookup entry (Admin only).
    /// </summary>
    /// <param name="id">The lookup ID to update</param>
    /// <param name="request">The lookup update request</param>
    /// <returns>The updated lookup</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(LookupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLookupRequest request)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Update lookup called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Valid lookup ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        if (request == null)
        {
            _logger.LogWarning("Update lookup called with null request");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Request body is required",
                new Dictionary<string, string[]> { ["body"] = new[] { "Request cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Updating lookup with ID: {Id}", id);

        var result = await _lookupService.UpdateLookupAsync(id, request);

        if (result == null)
        {
            _logger.LogWarning("Lookup not found for update: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Lookup with ID {id} not found",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a lookup entry (Admin only).
    /// </summary>
    /// <param name="id">The lookup ID to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Delete lookup called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Valid lookup ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Deleting lookup with ID: {Id}", id);

        var result = await _lookupService.DeleteLookupAsync(id);

        if (!result)
        {
            _logger.LogWarning("Lookup not found for deletion: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Lookup with ID {id} not found",
                Request.Path));
        }

        return NoContent();
    }
}

/// <summary>
/// Request model for creating a lookup entry.
/// </summary>
public record CreateLookupRequest(
    string Category,
    string Code,
    string DisplayValue,
    string? Description = null,
    int? DisplayOrder = null,
    Dictionary<string, string>? Attributes = null);

/// <summary>
/// Request model for updating a lookup entry.
/// </summary>
public record UpdateLookupRequest(
    string? DisplayValue = null,
    string? Description = null,
    int? DisplayOrder = null,
    Dictionary<string, string>? Attributes = null);
