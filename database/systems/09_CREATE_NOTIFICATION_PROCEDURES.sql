-- ============================================
-- Phase 1B: Notification Management Procedures
-- Purpose: User notification creation, delivery, and lifecycle
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Shared
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- PROCEDURE: Create Notification
-- ============================================

IF OBJECT_ID('Shared.spCreateNotification', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spCreateNotification;

GO

CREATE PROCEDURE Shared.spCreateNotification
    @UserId NVARCHAR(128),
    @TenantId NVARCHAR(128),
    @Title NVARCHAR(256),
    @Message NVARCHAR(MAX),
    @Type NVARCHAR(50) = 'Info',
    @RelatedEntityType NVARCHAR(128) = NULL,
    @RelatedEntityId NVARCHAR(128) = NULL,
    @NotificationId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @NotificationId = NEWID();

    -- Validate type
    IF @Type NOT IN ('Info', 'Warning', 'Error', 'Success', 'System')
        SET @Type = 'Info';

    INSERT INTO Shared.Notifications (
        NotificationId,
        UserId,
        TenantId,
        Title,
        Message,
        Type,
        Status,
        RelatedEntityType,
        RelatedEntityId,
        CreatedAt,
        CreatedBy,
        IsDeleted
    ) VALUES (
        @NotificationId,
        @UserId,
        @TenantId,
        @Title,
        @Message,
        @Type,
        'Pending',
        @RelatedEntityType,
        @RelatedEntityId,
        GETUTCDATE(),
        'SYSTEM',
        0
    );

    PRINT '✅ Notification created: ' + @Title + ' for user ' + @UserId;
END;

GO

-- ============================================
-- PROCEDURE: Get User Notifications
-- ============================================

IF OBJECT_ID('Shared.spGetUserNotifications', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spGetUserNotifications;

GO

CREATE PROCEDURE Shared.spGetUserNotifications
    @UserId NVARCHAR(128),
    @TenantId NVARCHAR(128),
    @UnreadOnly BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT
        NotificationId,
        UserId,
        TenantId,
        Title,
        Message,
        Type,
        Status,
        RelatedEntityType,
        RelatedEntityId,
        ReadAt,
        CreatedAt,
        ROW_NUMBER() OVER (ORDER BY CreatedAt DESC) AS RowNumber
    FROM Shared.Notifications
    WHERE UserId = @UserId
    AND TenantId = @TenantId
    AND IsDeleted = 0
    AND (@UnreadOnly = 0 OR Status = 'Pending')
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    PRINT '✅ Retrieved notifications for user: ' + @UserId;
END;

GO

-- ============================================
-- PROCEDURE: Mark Notification as Read
-- ============================================

IF OBJECT_ID('Shared.spMarkNotificationAsRead', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spMarkNotificationAsRead;

GO

CREATE PROCEDURE Shared.spMarkNotificationAsRead
    @NotificationId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Shared.Notifications
    SET Status = 'Read',
        ReadAt = GETUTCDATE(),
        UpdatedAt = GETUTCDATE()
    WHERE NotificationId = @NotificationId
    AND IsDeleted = 0;

    IF @@ROWCOUNT = 0
    BEGIN
        PRINT '⚠️ Notification not found: ' + CAST(@NotificationId AS NVARCHAR(36));
        RETURN;
    END

    PRINT '✅ Marked as read: ' + CAST(@NotificationId AS NVARCHAR(36));
END;

GO

-- ============================================
-- PROCEDURE: Mark All Notifications as Read
-- ============================================

IF OBJECT_ID('Shared.spMarkAllNotificationsAsRead', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spMarkAllNotificationsAsRead;

GO

CREATE PROCEDURE Shared.spMarkAllNotificationsAsRead
    @UserId NVARCHAR(128),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UpdatedCount INT;

    UPDATE Shared.Notifications
    SET Status = 'Read',
        ReadAt = GETUTCDATE(),
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
    AND TenantId = @TenantId
    AND Status = 'Pending'
    AND IsDeleted = 0;

    SELECT @UpdatedCount = @@ROWCOUNT;

    PRINT '✅ Marked ' + CAST(@UpdatedCount AS NVARCHAR(10)) + ' notifications as read for user: ' + @UserId;
END;

GO

-- ============================================
-- PROCEDURE: Archive Old Notifications
-- ============================================

IF OBJECT_ID('Shared.spArchiveOldNotifications', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spArchiveOldNotifications;

GO

CREATE PROCEDURE Shared.spArchiveOldNotifications
    @DaysToKeep INT = 30,
    @DryRun BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@DaysToKeep, GETUTCDATE());
    DECLARE @ArchivedCount INT = 0;

    -- Count old notifications
    SELECT @ArchivedCount = COUNT(*)
    FROM Shared.Notifications
    WHERE CreatedAt < @CutoffDate
    AND Status IN ('Read', 'Archived')
    AND IsDeleted = 0;

    IF @DryRun = 1
    BEGIN
        PRINT '🔍 DRY RUN: Old Notification Cleanup';
        PRINT 'Cutoff Date: ' + CONVERT(NVARCHAR(10), @CutoffDate, 121);
        PRINT 'Notifications to archive: ' + CAST(@ArchivedCount AS NVARCHAR(10));
        RETURN;
    END

    -- Archive old notifications (soft delete)
    UPDATE Shared.Notifications
    SET IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE CreatedAt < @CutoffDate
    AND Status IN ('Read', 'Archived')
    AND IsDeleted = 0;

    PRINT '🧹 Archived old notifications: ' + CAST(@ArchivedCount AS NVARCHAR(10));
END;

GO

-- ============================================
-- PROCEDURE: Get Unread Notification Count
-- ============================================

IF OBJECT_ID('Shared.spGetUnreadNotificationCount', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spGetUnreadNotificationCount;

GO

CREATE PROCEDURE Shared.spGetUnreadNotificationCount
    @UserId NVARCHAR(128),
    @TenantId NVARCHAR(128),
    @UnreadCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @UnreadCount = COUNT(*)
    FROM Shared.Notifications
    WHERE UserId = @UserId
    AND TenantId = @TenantId
    AND Status = 'Pending'
    AND IsDeleted = 0;

    PRINT '📬 Unread notifications for user ' + @UserId + ': ' + CAST(@UnreadCount AS NVARCHAR(10));
END;

GO

-- ============================================
-- PROCEDURE: Broadcast System Notification
-- ============================================

IF OBJECT_ID('Shared.spBroadcastSystemNotification', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spBroadcastSystemNotification;

GO

CREATE PROCEDURE Shared.spBroadcastSystemNotification
    @TenantId NVARCHAR(128),
    @Title NVARCHAR(256),
    @Message NVARCHAR(MAX),
    @Type NVARCHAR(50) = 'System',
    @TargetRole NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NotificationId UNIQUEIDENTIFIER;
    DECLARE @UserCursor CURSOR;
    DECLARE @UserId NVARCHAR(128);
    DECLARE @CreatedCount INT = 0;

    -- Get users to notify
    CREATE TABLE #UsersToNotify (UserId NVARCHAR(128));

    IF @TargetRole IS NOT NULL
    BEGIN
        -- Notify users with specific role
        INSERT INTO #UsersToNotify
        SELECT DISTINCT u.UserId
        FROM Auth.Users u
        INNER JOIN Auth.UserRoles ur ON u.UserId = ur.UserId
        INNER JOIN Auth.Roles r ON ur.RoleId = r.RoleId
        WHERE u.TenantId = @TenantId
        AND r.Name = @TargetRole
        AND u.IsDeleted = 0
        AND u.IsActive = 1;
    END
    ELSE
    BEGIN
        -- Notify all active users
        INSERT INTO #UsersToNotify
        SELECT UserId
        FROM Auth.Users
        WHERE TenantId = @TenantId
        AND IsDeleted = 0
        AND IsActive = 1;
    END

    -- Create notification for each user
    DECLARE @UserCursor CURSOR;
    SET @UserCursor = CURSOR FOR SELECT UserId FROM #UsersToNotify;

    OPEN @UserCursor;
    FETCH NEXT FROM @UserCursor INTO @UserId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC Shared.spCreateNotification
            @UserId = @UserId,
            @TenantId = @TenantId,
            @Title = @Title,
            @Message = @Message,
            @Type = @Type,
            @NotificationId = @NotificationId OUTPUT;

        SET @CreatedCount = @CreatedCount + 1;
        FETCH NEXT FROM @UserCursor INTO @UserId;
    END

    CLOSE @UserCursor;
    DEALLOCATE @UserCursor;
    DROP TABLE #UsersToNotify;

    PRINT '📢 Broadcast notification created: ' + @Title + ' (Delivered to: ' + CAST(@CreatedCount AS NVARCHAR(10)) + ' users)';
END;

GO

-- ============================================
-- PROCEDURE: Notification Statistics
-- ============================================

IF OBJECT_ID('Report.spNotificationStatistics', 'P') IS NOT NULL
    DROP PROCEDURE Report.spNotificationStatistics;

GO

CREATE PROCEDURE Report.spNotificationStatistics
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'NOTIFICATION STATISTICS';
    PRINT '═══════════════════════════════════════════';

    -- 1. Overall stats
    PRINT '';
    PRINT '📊 OVERALL STATISTICS:';
    SELECT
        COUNT(*) AS TotalNotifications,
        SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) AS UnreadNotifications,
        SUM(CASE WHEN Status = 'Read' THEN 1 ELSE 0 END) AS ReadNotifications,
        CAST(SUM(CASE WHEN Status = 'Read' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5, 2)) AS ReadPercentage
    FROM Shared.Notifications
    WHERE TenantId = @TenantId
    AND IsDeleted = 0;

    -- 2. By notification type
    PRINT '';
    PRINT '📋 BY NOTIFICATION TYPE:';
    SELECT
        Type,
        COUNT(*) AS Count,
        SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) AS Unread
    FROM Shared.Notifications
    WHERE TenantId = @TenantId
    AND IsDeleted = 0
    GROUP BY Type
    ORDER BY Count DESC;

    -- 3. User notification activity
    PRINT '';
    PRINT '👥 TOP USERS BY NOTIFICATION COUNT:';
    SELECT TOP 10
        u.UserId,
        u.Email,
        COUNT(n.NotificationId) AS NotificationCount,
        SUM(CASE WHEN n.Status = 'Pending' THEN 1 ELSE 0 END) AS UnreadCount
    FROM Auth.Users u
    LEFT JOIN Shared.Notifications n ON u.UserId = n.UserId
        AND n.TenantId = @TenantId
        AND n.IsDeleted = 0
    WHERE u.TenantId = @TenantId
    AND u.IsDeleted = 0
    GROUP BY u.UserId, u.Email
    ORDER BY NotificationCount DESC;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1B: Notification Management successfully configured';
PRINT 'Total objects created:';
PRINT '  - 8 notification management procedures';
PRINT '  - Create, read, archive notifications';
PRINT '  - Broadcast system notifications';
PRINT '  - Notification statistics & reporting';
PRINT 'Status: Notification system ready for use';

GO
