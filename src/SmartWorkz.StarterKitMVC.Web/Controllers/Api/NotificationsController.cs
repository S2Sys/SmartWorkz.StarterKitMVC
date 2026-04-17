using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Primitives;
using SmartWorkz.StarterKitMVC.Web.Middleware;
using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

/// <summary>
/// Notifications API endpoints for managing user notifications.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all unread notifications for the current user.
    /// </summary>
    /// <returns>List of unread notifications</returns>
    [HttpGet("unread")]
    [ProducesResponseType(typeof(IEnumerable<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUnread()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetUnread called without valid user identity");
            return Unauthorized();
        }

        _logger.LogInformation("Retrieving unread notifications for user: {UserId}", userId);
        var result = await _notificationService.GetUnreadNotificationsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Get count of unread notifications for the current user.
    /// </summary>
    /// <returns>Count of unread notifications</returns>
    [HttpGet("unread/count")]
    [ProducesResponseType(typeof(UnreadCountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetUnreadCount called without valid user identity");
            return Unauthorized();
        }

        _logger.LogInformation("Retrieving unread notification count for user: {UserId}", userId);
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new UnreadCountResponse(count));
    }

    /// <summary>
    /// Mark a specific notification as read.
    /// </summary>
    /// <param name="id">The notification ID</param>
    /// <returns>Success message</returns>
    [HttpPost("{id}/mark-as-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("MarkAsRead called with invalid ID: {Id}", id);
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Valid notification ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID must be greater than 0" } },
                Request.Path));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("MarkAsRead called without valid user identity");
            return Unauthorized();
        }

        _logger.LogInformation("Marking notification as read: {NotificationId} for user: {UserId}", id, userId);

        var result = await _notificationService.MarkAsReadAsync(id, userId);

        if (!result)
        {
            _logger.LogWarning("Notification not found or access denied: {Id} for user: {UserId}", id, userId);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Notification with ID {id} not found or access denied",
                Request.Path));
        }

        return Ok(new { message = "Notification marked as read" });
    }

    /// <summary>
    /// Mark all notifications as read for the current user.
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("mark-all-as-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("MarkAllAsRead called without valid user identity");
            return Unauthorized();
        }

        _logger.LogInformation("Marking all notifications as read for user: {UserId}", userId);

        var count = await _notificationService.MarkAllAsReadAsync(userId);

        return Ok(new { message = $"{count} notifications marked as read", count });
    }
}

/// <summary>
/// Response model for unread notification count.
/// </summary>
public record UnreadCountResponse(int Count);

/// <summary>
/// Response model for notification information.
/// </summary>
public record NotificationDto(
    int Id,
    string UserId,
    string Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt,
    Dictionary<string, string>? Metadata = null);
