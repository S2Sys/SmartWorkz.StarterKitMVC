using SmartWorkz.StarterKitMVC.Shared.DTOs;
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

    /// <summary>Create a notification</summary>
    public async Task CreateAsync(NotificationDto notification)
    {
        const string sql = """
            INSERT INTO [Shared].[Notification] (
                NotificationId, UserId, Title, Message, NotificationType, ActionUrl,
                IsRead, ReadAt, TenantId, CreatedAt, ExpiresAt, IsDeleted
            ) VALUES (
                @NotificationId, @UserId, @Title, @Message, @NotificationType, @ActionUrl,
                @IsRead, @ReadAt, @TenantId, @CreatedAt, @ExpiresAt, @IsDeleted
            )
            """;

        await Connection.ExecuteAsync(sql, notification);
    }

    /// <summary>Update a notification</summary>
    public async Task UpdateAsync(NotificationDto notification)
    {
        const string sql = """
            UPDATE [Shared].[Notification]
            SET UserId = @UserId, Title = @Title, Message = @Message,
                NotificationType = @NotificationType, ActionUrl = @ActionUrl,
                IsRead = @IsRead, ReadAt = @ReadAt, TenantId = @TenantId,
                CreatedAt = @CreatedAt, ExpiresAt = @ExpiresAt, IsDeleted = @IsDeleted
            WHERE NotificationId = @NotificationId
            """;

        await Connection.ExecuteAsync(sql, notification);
    }

    /// <summary>Delete all notifications for a user</summary>
    public async Task DeleteAllAsync(string userId, string tenantId)
    {
        const string sql = """
            UPDATE [Shared].[Notification]
            SET IsDeleted = 1
            WHERE UserId = @UserId
              AND TenantId = @TenantId
            """;

        await Connection.ExecuteAsync(sql, new { UserId = userId, TenantId = tenantId });
    }

    /// <summary>Get paginated notifications</summary>
    public async Task<(IEnumerable<NotificationDto> Items, int Total)> GetPagedAsync(
        string userId, string tenantId, int pageNumber, int pageSize)
    {
        const string countSql = """
            SELECT COUNT(*) FROM [Shared].[Notification]
            WHERE UserId = @UserId
              AND TenantId = @TenantId
              AND IsDeleted = 0
            """;

        const string dataSql = """
            SELECT * FROM [Shared].[Notification]
            WHERE UserId = @UserId
              AND TenantId = @TenantId
              AND IsDeleted = 0
            ORDER BY CreatedAt DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new { UserId = userId, TenantId = tenantId, PageNumber = pageNumber, PageSize = pageSize };
        var total = await Connection.QueryFirstAsync<int>(countSql, param);
        var items = await Connection.QueryAsync<NotificationDto>(dataSql, param);

        return (items, total);
    }
}
