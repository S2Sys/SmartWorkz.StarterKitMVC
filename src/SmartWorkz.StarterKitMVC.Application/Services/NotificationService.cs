using SmartWorkz.StarterKitMVC.Shared.DTOs;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Implementation of notification management service.
/// Handles sending, tracking, and managing user notifications.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        ILogger<NotificationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<bool> SendNotificationAsync(NotificationRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new ArgumentException("User ID cannot be empty", nameof(request.UserId));
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Notification title is required", nameof(request.Title));

        try
        {
            var notification = new NotificationDto
            {
                NotificationId = Guid.NewGuid(),
                UserId = request.UserId,
                TenantId = request.TenantId,
                Title = request.Title,
                Message = request.Message,
                ActionUrl = request.ActionUrl,
                NotificationType = request.Type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            await _repository.UpsertAsync(notification);

            _logger.LogInformation(
                "Notification sent to user {UserId}: {Title}",
                request.UserId, request.Title);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending notification to user {UserId}",
                request.UserId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<NotificationDto>> GetUnreadAsync(string userId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        try
        {
            var notifications = await _repository.GetUnreadAsync(userId, tenantId);
            _logger.LogDebug("Retrieved {Count} unread notifications for user {UserId}",
                notifications.Count(), userId);
            return notifications.OrderByDescending(n => n.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving unread notifications for user {UserId}",
                userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<NotificationDto> Notifications, int Total)> GetAllAsync(
        string userId, string tenantId, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        try
        {
            var allNotifications = await _repository.GetByUserAsync(userId, tenantId);
            var paginatedNotifications = allNotifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
            var total = allNotifications.Count();

            _logger.LogDebug("Retrieved {Count} notifications for user {UserId}", paginatedNotifications.Count(), userId);
            return (paginatedNotifications, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving notifications for user {UserId}",
                userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> MarkAsReadAsync(Guid notificationId)
    {
        if (notificationId == Guid.Empty)
            throw new ArgumentException("Notification ID must be valid", nameof(notificationId));

        try
        {
            var notification = await _repository.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _repository.UpsertAsync(notification);

            _logger.LogDebug("Notification marked as read: {NotificationId}", notificationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error marking notification as read: {NotificationId}",
                notificationId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> MarkAllAsReadAsync(string userId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var unreadNotifications = await _repository.GetUnreadAsync(userId, tenantId);

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _repository.UpsertAsync(notification);
            }

            _logger.LogInformation(
                "Marked {Count} notifications as read for user {UserId}",
                unreadNotifications.Count(), userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error marking all notifications as read for user {UserId}",
                userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(string userId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var unreadNotifications = await _repository.GetUnreadAsync(userId, tenantId);
            var count = unreadNotifications.Count();

            _logger.LogDebug("Unread notification count for user {UserId}: {Count}", userId, count);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting unread count for user {UserId}",
                userId);
            return 0;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid notificationId)
    {
        if (notificationId == Guid.Empty)
            throw new ArgumentException("Notification ID must be valid", nameof(notificationId));

        try
        {
            await _repository.DeleteAsync(notificationId);
            _logger.LogDebug("Notification deleted: {NotificationId}", notificationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting notification: {NotificationId}",
                notificationId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAllAsync(string userId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var notifications = await _repository.FindAsync(new { UserId = userId, TenantId = tenantId });
            foreach (var notification in notifications)
            {
                await _repository.DeleteAsync(notification.NotificationId);
            }

            _logger.LogInformation("All notifications deleted for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting all notifications for user {UserId}",
                userId);
            return false;
        }
    }
}
