-- ============================================
-- Phase 1C: Dashboard Metrics & Views
-- Purpose: Pre-computed metrics for dashboard visualization
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Report
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- VIEW: Dashboard - User Metrics
-- ============================================

IF OBJECT_ID('Report.vw_DashboardUserMetrics', 'V') IS NOT NULL
    DROP VIEW Report.vw_DashboardUserMetrics;

GO

CREATE VIEW Report.vw_DashboardUserMetrics
AS
SELECT
    COUNT(*) AS TotalUsers,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveUsers,
    SUM(CASE WHEN IsLocked = 1 THEN 1 ELSE 0 END) AS LockedUsers,
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) AS DeletedUsers,
    COUNT(CASE WHEN LastLoginAt >= DATEADD(DAY, -7, GETUTCDATE()) THEN 1 END) AS UsersLastWeek,
    COUNT(CASE WHEN LastLoginAt >= DATEADD(DAY, -30, GETUTCDATE()) THEN 1 END) AS UsersLastMonth,
    CAST(
        COUNT(CASE WHEN IsActive = 1 THEN 1 END) * 100.0 /
        NULLIF(COUNT(*), 0)
        AS DECIMAL(5, 2)
    ) AS ActivePercentage
FROM Auth.Users
WHERE IsDeleted = 0;

GO

-- ============================================
-- VIEW: Dashboard - Authentication Metrics
-- ============================================

IF OBJECT_ID('Report.vw_DashboardAuthMetrics', 'V') IS NOT NULL
    DROP VIEW Report.vw_DashboardAuthMetrics;

GO

CREATE VIEW Report.vw_DashboardAuthMetrics
AS
SELECT
    COUNT(*) AS TotalLoginAttempts,
    SUM(CASE WHEN IsSuccessful = 1 THEN 1 ELSE 0 END) AS SuccessfulLogins,
    SUM(CASE WHEN IsSuccessful = 0 THEN 1 ELSE 0 END) AS FailedLogins,
    CAST(
        SUM(CASE WHEN IsSuccessful = 1 THEN 1 ELSE 0 END) * 100.0 /
        NULLIF(COUNT(*), 0)
        AS DECIMAL(5, 2)
    ) AS SuccessRate,
    COUNT(CASE WHEN AttemptedAt >= DATEADD(HOUR, -24, GETUTCDATE()) THEN 1 END) AS AttemptsLast24h,
    COUNT(CASE WHEN AttemptedAt >= DATEADD(HOUR, -1, GETUTCDATE()) THEN 1 END) AS AttemptsLastHour
FROM Auth.LoginAttempts
WHERE IsDeleted = 0;

GO

-- ============================================
-- VIEW: Dashboard - Content Metrics
-- ============================================

IF OBJECT_ID('Report.vw_DashboardContentMetrics', 'V') IS NOT NULL
    DROP VIEW Report.vw_DashboardContentMetrics;

GO

CREATE VIEW Report.vw_DashboardContentMetrics
AS
SELECT
    (SELECT COUNT(*) FROM Master.BlogPosts WHERE IsDeleted = 0) AS TotalBlogPosts,
    (SELECT COUNT(*) FROM Master.BlogPosts WHERE IsPublished = 1 AND IsDeleted = 0) AS PublishedBlogPosts,
    (SELECT COUNT(*) FROM Master.CustomPages WHERE IsPublished = 1 AND IsDeleted = 0) AS PublishedPages,
    (SELECT COUNT(*) FROM Master.Categories WHERE IsActive = 1 AND IsDeleted = 0) AS ActiveCategories,
    (SELECT SUM(ViewCount) FROM Master.BlogPosts WHERE IsDeleted = 0) AS TotalBlogViews,
    (SELECT SUM(ViewCount) FROM Master.CustomPages WHERE IsDeleted = 0) AS TotalPageViews;

GO

-- ============================================
-- VIEW: Dashboard - System Metrics
-- ============================================

IF OBJECT_ID('Report.vw_DashboardSystemMetrics', 'V') IS NOT NULL
    DROP VIEW Report.vw_DashboardSystemMetrics;

GO

CREATE VIEW Report.vw_DashboardSystemMetrics
AS
SELECT
    (SELECT COUNT(*) FROM Shared.AuditLogs WHERE IsDeleted = 0) AS TotalAuditLogs,
    (SELECT COUNT(*) FROM Shared.AuditLogs WHERE CreatedAt >= DATEADD(DAY, -1, GETUTCDATE()) AND IsDeleted = 0) AS AuditLogsToday,
    (SELECT COUNT(*) FROM Shared.Notifications WHERE IsDeleted = 0) AS TotalNotifications,
    (SELECT COUNT(*) FROM Shared.Notifications WHERE Status = 'Pending' AND IsDeleted = 0) AS UnreadNotifications,
    (SELECT COUNT(*) FROM Master.FeatureFlags WHERE IsEnabled = 1 AND IsDeleted = 0) AS EnabledFeatures,
    (SELECT COUNT(*) FROM Master.Configuration WHERE IsActive = 1 AND IsDeleted = 0) AS ActiveConfigurations;

GO

-- ============================================
-- VIEW: Dashboard - Tenant Metrics
-- ============================================

IF OBJECT_ID('Report.vw_DashboardTenantMetrics', 'V') IS NOT NULL
    DROP VIEW Report.vw_DashboardTenantMetrics;

GO

CREATE VIEW Report.vw_DashboardTenantMetrics
AS
SELECT
    COUNT(*) AS TotalTenants,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveTenants,
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) AS DeletedTenants,
    SUM(CASE WHEN GETUTCDATE() > SubscriptionExpiresAt THEN 1 ELSE 0 END) AS ExpiredSubscriptions
FROM Master.Tenants
WHERE IsDeleted = 0;

GO

-- ============================================
-- PROCEDURE: Get Dashboard Summary
-- ============================================

IF OBJECT_ID('Report.spGetDashboardSummary', 'P') IS NOT NULL
    DROP PROCEDURE Report.spGetDashboardSummary;

GO

CREATE PROCEDURE Report.spGetDashboardSummary
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'DASHBOARD SUMMARY';
    PRINT '═══════════════════════════════════════════';

    -- 1. User Metrics
    PRINT '';
    PRINT '👥 USER METRICS:';
    SELECT * FROM Report.vw_DashboardUserMetrics;

    -- 2. Authentication Metrics
    PRINT '';
    PRINT '🔐 AUTHENTICATION METRICS:';
    SELECT * FROM Report.vw_DashboardAuthMetrics;

    -- 3. Content Metrics
    PRINT '';
    PRINT '📄 CONTENT METRICS:';
    SELECT * FROM Report.vw_DashboardContentMetrics;

    -- 4. System Metrics
    PRINT '';
    PRINT '⚙️ SYSTEM METRICS:';
    SELECT * FROM Report.vw_DashboardSystemMetrics;

    -- 5. Tenant Metrics
    PRINT '';
    PRINT '🏢 TENANT METRICS:';
    SELECT * FROM Report.vw_DashboardTenantMetrics;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Get Real-time Dashboard Tile
-- ============================================

IF OBJECT_ID('Report.spGetDashboardTile', 'P') IS NOT NULL
    DROP PROCEDURE Report.spGetDashboardTile;

GO

CREATE PROCEDURE Report.spGetDashboardTile
    @TileType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE WHEN @TileType = 'ActiveUsers' THEN 'Active Users'
                WHEN @TileType = 'FailedLogins' THEN 'Failed Logins (24h)'
                WHEN @TileType = 'PublishedContent' THEN 'Published Content'
                WHEN @TileType = 'UnreadNotifications' THEN 'Unread Notifications'
                ELSE 'Unknown'
           END AS TileTitle,
           CASE @TileType
                WHEN 'ActiveUsers' THEN (SELECT CAST(ActiveUsers AS NVARCHAR(10)) FROM Report.vw_DashboardUserMetrics)
                WHEN 'FailedLogins' THEN (SELECT CAST(FailedLogins AS NVARCHAR(10)) FROM Report.vw_DashboardAuthMetrics WHERE AttemptedAt >= DATEADD(HOUR, -24, GETUTCDATE()))
                WHEN 'PublishedContent' THEN (SELECT CAST(PublishedBlogPosts + PublishedPages AS NVARCHAR(10)) FROM Report.vw_DashboardContentMetrics)
                WHEN 'UnreadNotifications' THEN (SELECT CAST(UnreadNotifications AS NVARCHAR(10)) FROM Report.vw_DashboardSystemMetrics)
                ELSE '0'
           END AS TileValue,
           GETUTCDATE() AS LastUpdated;
END;

GO

PRINT '✅ Phase 1C: Dashboard Metrics successfully configured';
PRINT 'Total objects created:';
PRINT '  - 5 dashboard metric views (user, auth, content, system, tenant)';
PRINT '  - 2 dashboard procedures (summary, tile)';
PRINT 'Status: Real-time dashboard metrics ready';

GO
