using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for notifications (Shared.Notification table)
/// Handles user notifications and messaging
/// </summary>
public class NotificationRepository : DapperRepository<NotificationDto>, INotificationRepository
{
    public NotificationRepository(IDbConnection connection, ILogger<NotificationRepository> logger)
        : base(connection, logger)
    {
        TableName = "Notification";
        Schema = "Shared";
        IdColumn = "NotificationId";
    }

    /// <summary>Get notifications for a user</summary>
    public async Task<IEnumerable<NotificationDto>> GetByUserAsync(string userId, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Shared].[Notification]
            WHERE UserId = @UserId
              AND TenantId = @TenantId
              AND IsDeleted = 0
            ORDER BY CreatedAt DESC
            """;

        return await ExecuteQueryAsync(sql, new { UserId = userId, TenantId = tenantId });
    }

    /// <summary>Get unread notifications for a user</summary>
    public async Task<IEnumerable<NotificationDto>> GetUnreadAsync(string userId, string tenantId)
    {
        const string sql = """
            SELECT * FROM [Shared].[Notification]
            WHERE UserId = @UserId
              AND TenantId = @TenantId
              AND IsRead = 0
              AND IsDeleted = 0
            ORDER BY CreatedAt DESC
            """;

        return await ExecuteQueryAsync(sql, new { UserId = userId, TenantId = tenantId });
    }

    /// <summary>Mark notification as read</summary>
    public async Task MarkAsReadAsync(Guid notificationId, string userId)
    {
        const string sql = """
            UPDATE [Shared].[Notification]
            SET IsRead = 1, ReadAt = @ReadAt
            WHERE NotificationId = @NotificationId AND UserId = @UserId
            """;

        await Connection.ExecuteAsync(sql, new
        {
            NotificationId = notificationId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        });
    }

    /// <summary>Mark all notifications as read for a user</summary>
    public async Task MarkAllAsReadAsync(string userId, string tenantId)
    {
        const string sql = """
            UPDATE [Shared].[Notification]
            SET IsRead = 1, ReadAt = @ReadAt
            WHERE UserId = @UserId
              AND TenantId = @TenantId
              AND IsRead = 0
            """;

        await Connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            TenantId = tenantId,
            ReadAt = DateTime.UtcNow
        });
    }

    /// <summary>Get unread count for a user</summary>
    public async Task<int> GetUnreadCountAsync(string userId, string tenantId)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Shared].[Notification]
            WHERE UserId = @UserId
              AND TenantId = @TenantId
              AND IsRead = 0
              AND IsDeleted = 0
            """;

        return await Connection.QueryFirstAsync<int>(sql, new
        {
            UserId = userId,
            TenantId = tenantId
        });
    }

    /// <summary>Delete old notifications (cleanup)</summary>
    public async Task DeleteOlderThanAsync(DateTime beforeDate, string tenantId)
    {
        const string sql = """
            UPDATE [Shared].[Notification]
            SET IsDeleted = 1
            WHERE TenantId = @TenantId
              AND CreatedAt < @BeforeDate
              AND IsDeleted = 0
            """;

        await Connection.ExecuteAsync(sql, new
        {
            TenantId = tenantId,
            BeforeDate = beforeDate
        });
    }
}
