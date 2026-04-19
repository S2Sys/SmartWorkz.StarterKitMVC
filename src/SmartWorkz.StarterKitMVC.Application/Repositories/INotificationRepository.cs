using SmartWorkz.StarterKitMVC.Shared.DTOs;
namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for notifications (Shared.Notification table)
/// </summary>
public interface INotificationRepository : IDapperRepository<NotificationDto>
{
    /// <summary>Get notifications for a user</summary>
    Task<IEnumerable<NotificationDto>> GetByUserAsync(string userId, string tenantId);

    /// <summary>Get unread notifications for a user</summary>
    Task<IEnumerable<NotificationDto>> GetUnreadAsync(string userId, string tenantId);

    /// <summary>Mark notification as read</summary>
    Task MarkAsReadAsync(Guid notificationId, string userId);

    /// <summary>Mark all notifications as read for a user</summary>
    Task MarkAllAsReadAsync(string userId, string tenantId);

    /// <summary>Get unread count for a user</summary>
    Task<int> GetUnreadCountAsync(string userId, string tenantId);

    /// <summary>Delete old notifications (cleanup)</summary>
    Task DeleteOlderThanAsync(DateTime beforeDate, string tenantId);

    /// <summary>Create a notification</summary>
    Task CreateAsync(NotificationDto notification);

    /// <summary>Update a notification</summary>
    Task UpdateAsync(NotificationDto notification);

    /// <summary>Delete all notifications for a user</summary>
    Task DeleteAllAsync(string userId, string tenantId);

    /// <summary>Get paginated notifications with filtering</summary>
    Task<(IEnumerable<NotificationDto> Items, int Total)> GetPagedAsync(string userId, string tenantId, int pageNumber, int pageSize);
}

