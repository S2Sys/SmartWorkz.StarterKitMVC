using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Application.Repositories;
namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for managing user notifications.
/// Supports multiple notification types: in-app, email, and SMS.
/// Includes read/unread tracking and user preferences.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a user.
    /// Respects user notification preferences (channels, do not disturb).
    /// </summary>
    Task<bool> SendNotificationAsync(NotificationRequest request);

    /// <summary>
    /// Gets all unread notifications for a user.
    /// </summary>
    Task<IEnumerable<NotificationDto>> GetUnreadAsync(string userId, string tenantId);

    /// <summary>
    /// Gets all notifications for a user with pagination.
    /// </summary>
    Task<(IEnumerable<NotificationDto> Notifications, int Total)> GetAllAsync(
        string userId, string tenantId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    Task<bool> MarkAsReadAsync(Guid notificationId);

    /// <summary>
    /// Marks all notifications for a user as read.
    /// </summary>
    Task<bool> MarkAllAsReadAsync(string userId, string tenantId);

    /// <summary>
    /// Gets the count of unread notifications for a user.
    /// </summary>
    Task<int> GetUnreadCountAsync(string userId, string tenantId);

    /// <summary>
    /// Deletes a notification by ID.
    /// </summary>
    Task<bool> DeleteAsync(Guid notificationId);

    /// <summary>
    /// Deletes all notifications for a user.
    /// </summary>
    Task<bool> DeleteAllAsync(string userId, string tenantId);
}

/// <summary>Request to send a notification</summary>
public class NotificationRequest
{
    public string UserId { get; set; }
    public string TenantId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string? ActionUrl { get; set; }
    public string Type { get; set; } = "InApp";
    public Dictionary<string, string>? Data { get; set; }
}
