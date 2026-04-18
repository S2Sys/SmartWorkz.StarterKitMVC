-- ============================================
-- Phase 1C: API Rate Limiting
-- Purpose: Rate limit tracking and enforcement
-- Database: SQL Server (Boilerplate v3)
-- Schemas: dbo, Auth
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- TABLE: API Rate Limit Configuration
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RateLimitPolicies' AND schema_id = SCHEMA_ID('Auth'))
BEGIN
    CREATE TABLE Auth.RateLimitPolicies (
        RateLimitPolicyId INT PRIMARY KEY IDENTITY(1,1),
        ApiKeyId NVARCHAR(128) NOT NULL,
        PolicyName NVARCHAR(256),
        RequestsPerMinute INT NOT NULL DEFAULT 60,
        RequestsPerHour INT NOT NULL DEFAULT 1000,
        RequestsPerDay INT NOT NULL DEFAULT 10000,
        BurstSize INT NOT NULL DEFAULT 10,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        IsDeleted BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_RateLimitPolicies_ApiKey ON Auth.RateLimitPolicies(ApiKeyId);
    PRINT '✅ Created RateLimitPolicies table';
END

-- ============================================
-- TABLE: Rate Limit Tracking
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RateLimitTracking' AND schema_id = SCHEMA_ID('Auth'))
BEGIN
    CREATE TABLE Auth.RateLimitTracking (
        RateLimitTrackingId BIGINT PRIMARY KEY IDENTITY(1,1),
        ApiKeyId NVARCHAR(128) NOT NULL,
        IpAddress NVARCHAR(45) NOT NULL,
        Endpoint NVARCHAR(256) NOT NULL,
        RequestCount INT NOT NULL DEFAULT 1,
        WindowStart DATETIME2 NOT NULL,
        WindowEnd DATETIME2 NOT NULL,
        IsThrottled BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2
    );

    CREATE INDEX IX_RateLimitTracking_ApiKey ON Auth.RateLimitTracking(ApiKeyId);
    CREATE INDEX IX_RateLimitTracking_Window ON Auth.RateLimitTracking(WindowStart, WindowEnd);
    PRINT '✅ Created RateLimitTracking table';
END

GO

-- ============================================
-- PROCEDURE: Check Rate Limit
-- ============================================

IF OBJECT_ID('Auth.spCheckRateLimit', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spCheckRateLimit;

GO

CREATE PROCEDURE Auth.spCheckRateLimit
    @ApiKeyId NVARCHAR(128),
    @IpAddress NVARCHAR(45),
    @Endpoint NVARCHAR(256),
    @IsAllowed BIT OUTPUT,
    @RequestsRemaining INT OUTPUT,
    @ResetTime DATETIME2 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentTime DATETIME2 = GETUTCDATE();
    DECLARE @WindowStart DATETIME2 = DATEADD(MINUTE, -1, @CurrentTime);
    DECLARE @WindowEnd DATETIME2 = @CurrentTime;
    DECLARE @RequestsPerMinute INT = 60;
    DECLARE @CurrentCount INT = 0;

    -- Get rate limit policy
    SELECT @RequestsPerMinute = RequestsPerMinute
    FROM Auth.RateLimitPolicies
    WHERE ApiKeyId = @ApiKeyId
    AND IsActive = 1
    AND IsDeleted = 0;

    IF @RequestsPerMinute IS NULL
        SET @RequestsPerMinute = 60; -- Default limit

    -- Count requests in current window
    SELECT @CurrentCount = ISNULL(SUM(RequestCount), 0)
    FROM Auth.RateLimitTracking
    WHERE ApiKeyId = @ApiKeyId
    AND IpAddress = @IpAddress
    AND Endpoint = @Endpoint
    AND WindowStart >= @WindowStart
    AND WindowEnd <= @WindowEnd;

    -- Determine if request is allowed
    IF @CurrentCount >= @RequestsPerMinute
    BEGIN
        SET @IsAllowed = 0;
        SET @RequestsRemaining = 0;
        SET @ResetTime = @WindowEnd;

        -- Mark as throttled
        UPDATE Auth.RateLimitTracking
        SET IsThrottled = 1
        WHERE ApiKeyId = @ApiKeyId
        AND IpAddress = @IpAddress
        AND Endpoint = @Endpoint
        AND WindowStart >= @WindowStart;

        PRINT '🚫 Rate limit exceeded for: ' + @ApiKeyId + ' (' + @IpAddress + ')';
    END
    ELSE
    BEGIN
        SET @IsAllowed = 1;
        SET @RequestsRemaining = @RequestsPerMinute - @CurrentCount - 1;
        SET @ResetTime = @WindowEnd;

        -- Increment request count
        IF EXISTS (
            SELECT 1
            FROM Auth.RateLimitTracking
            WHERE ApiKeyId = @ApiKeyId
            AND IpAddress = @IpAddress
            AND Endpoint = @Endpoint
            AND WindowStart >= @WindowStart
        )
        BEGIN
            UPDATE Auth.RateLimitTracking
            SET RequestCount = RequestCount + 1,
                UpdatedAt = @CurrentTime
            WHERE ApiKeyId = @ApiKeyId
            AND IpAddress = @IpAddress
            AND Endpoint = @Endpoint
            AND WindowStart >= @WindowStart;
        END
        ELSE
        BEGIN
            INSERT INTO Auth.RateLimitTracking (
                ApiKeyId,
                IpAddress,
                Endpoint,
                RequestCount,
                WindowStart,
                WindowEnd,
                CreatedAt
            ) VALUES (
                @ApiKeyId,
                @IpAddress,
                @Endpoint,
                1,
                @WindowStart,
                @WindowEnd,
                @CurrentTime
            );
        END

        PRINT '✅ Request allowed: ' + @ApiKeyId + ' (Remaining: ' + CAST(@RequestsRemaining AS NVARCHAR(10)) + ')';
    END
END;

GO

-- ============================================
-- PROCEDURE: Create Rate Limit Policy
-- ============================================

IF OBJECT_ID('Auth.spCreateRateLimitPolicy', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spCreateRateLimitPolicy;

GO

CREATE PROCEDURE Auth.spCreateRateLimitPolicy
    @ApiKeyId NVARCHAR(128),
    @PolicyName NVARCHAR(256),
    @RequestsPerMinute INT = 60,
    @RequestsPerHour INT = 1000,
    @RequestsPerDay INT = 10000,
    @BurstSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Auth.RateLimitPolicies (
        ApiKeyId,
        PolicyName,
        RequestsPerMinute,
        RequestsPerHour,
        RequestsPerDay,
        BurstSize,
        IsActive,
        CreatedAt,
        IsDeleted
    ) VALUES (
        @ApiKeyId,
        @PolicyName,
        @RequestsPerMinute,
        @RequestsPerHour,
        @RequestsPerDay,
        @BurstSize,
        1,
        GETUTCDATE(),
        0
    );

    PRINT '✅ Rate limit policy created: ' + @PolicyName + ' for ' + @ApiKeyId;
END;

GO

-- ============================================
-- PROCEDURE: Update Rate Limit Policy
-- ============================================

IF OBJECT_ID('Auth.spUpdateRateLimitPolicy', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spUpdateRateLimitPolicy;

GO

CREATE PROCEDURE Auth.spUpdateRateLimitPolicy
    @ApiKeyId NVARCHAR(128),
    @RequestsPerMinute INT = NULL,
    @RequestsPerHour INT = NULL,
    @RequestsPerDay INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Auth.RateLimitPolicies
    SET RequestsPerMinute = ISNULL(@RequestsPerMinute, RequestsPerMinute),
        RequestsPerHour = ISNULL(@RequestsPerHour, RequestsPerHour),
        RequestsPerDay = ISNULL(@RequestsPerDay, RequestsPerDay),
        UpdatedAt = GETUTCDATE()
    WHERE ApiKeyId = @ApiKeyId
    AND IsDeleted = 0;

    PRINT '✅ Rate limit policy updated: ' + @ApiKeyId;
END;

GO

-- ============================================
-- PROCEDURE: Get Rate Limit Status
-- ============================================

IF OBJECT_ID('Auth.spGetRateLimitStatus', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spGetRateLimitStatus;

GO

CREATE PROCEDURE Auth.spGetRateLimitStatus
    @ApiKeyId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentTime DATETIME2 = GETUTCDATE();

    SELECT
        ApiKeyId,
        PolicyName,
        RequestsPerMinute,
        RequestsPerHour,
        RequestsPerDay,
        IsActive,
        (SELECT COUNT(*) FROM Auth.RateLimitTracking
         WHERE ApiKeyId = @ApiKeyId
         AND WindowStart >= DATEADD(MINUTE, -1, @CurrentTime)) AS RequestsLastMinute,
        (SELECT COUNT(*) FROM Auth.RateLimitTracking
         WHERE ApiKeyId = @ApiKeyId
         AND WindowStart >= DATEADD(HOUR, -1, @CurrentTime)) AS RequestsLastHour,
        (SELECT COUNT(*) FROM Auth.RateLimitTracking
         WHERE ApiKeyId = @ApiKeyId
         AND WindowStart >= DATEADD(DAY, -1, @CurrentTime)) AS RequestsLastDay
    FROM Auth.RateLimitPolicies
    WHERE ApiKeyId = @ApiKeyId
    AND IsDeleted = 0;

    PRINT '✅ Rate limit status retrieved for: ' + @ApiKeyId;
END;

GO

-- ============================================
-- PROCEDURE: Clean Old Rate Limit Records
-- ============================================

IF OBJECT_ID('Auth.spCleanOldRateLimitRecords', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spCleanOldRateLimitRecords;

GO

CREATE PROCEDURE Auth.spCleanOldRateLimitRecords
    @DaysToKeep INT = 7,
    @DryRun BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@DaysToKeep, GETUTCDATE());
    DECLARE @DeletedCount INT = 0;

    SELECT @DeletedCount = COUNT(*)
    FROM Auth.RateLimitTracking
    WHERE CreatedAt < @CutoffDate;

    IF @DryRun = 1
    BEGIN
        PRINT '🔍 DRY RUN: Old Rate Limit Records Cleanup';
        PRINT 'Cutoff Date: ' + CONVERT(NVARCHAR(10), @CutoffDate, 121);
        PRINT 'Records to delete: ' + CAST(@DeletedCount AS NVARCHAR(10));
        RETURN;
    END

    DELETE FROM Auth.RateLimitTracking
    WHERE CreatedAt < @CutoffDate;

    PRINT '🧹 Cleaned old rate limit records: ' + CAST(@DeletedCount AS NVARCHAR(10));
END;

GO

-- ============================================
-- PROCEDURE: Rate Limiting Report
-- ============================================

IF OBJECT_ID('Report.spRateLimitingReport', 'P') IS NOT NULL
    DROP PROCEDURE Report.spRateLimitingReport;

GO

CREATE PROCEDURE Report.spRateLimitingReport
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'RATE LIMITING REPORT';
    PRINT '═══════════════════════════════════════════';

    -- 1. Active policies
    PRINT '';
    PRINT '📋 ACTIVE RATE LIMIT POLICIES:';
    SELECT
        ApiKeyId,
        PolicyName,
        RequestsPerMinute,
        RequestsPerHour,
        RequestsPerDay,
        IsActive
    FROM Auth.RateLimitPolicies
    WHERE IsActive = 1
    AND IsDeleted = 0;

    -- 2. Throttled requests
    PRINT '';
    PRINT '🚫 THROTTLED REQUESTS (Last 24h):';
    SELECT
        ApiKeyId,
        IpAddress,
        COUNT(*) AS ThrottledCount,
        MAX(UpdatedAt) AS LastThrottle
    FROM Auth.RateLimitTracking
    WHERE IsThrottled = 1
    AND CreatedAt >= DATEADD(DAY, -1, GETUTCDATE())
    GROUP BY ApiKeyId, IpAddress
    ORDER BY ThrottledCount DESC;

    -- 3. Top consumers
    PRINT '';
    PRINT '📊 TOP API CONSUMERS (Last hour):';
    SELECT TOP 10
        ApiKeyId,
        IpAddress,
        Endpoint,
        SUM(RequestCount) AS RequestCount
    FROM Auth.RateLimitTracking
    WHERE CreatedAt >= DATEADD(HOUR, -1, GETUTCDATE())
    GROUP BY ApiKeyId, IpAddress, Endpoint
    ORDER BY RequestCount DESC;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1C: Rate Limiting successfully configured';
PRINT 'Total objects created:';
PRINT '  - 2 rate limit management tables';
PRINT '  - 6 rate limit procedures (check, create, update, status, cleanup, report)';
PRINT 'Status: API rate limiting ready for integration';

GO
