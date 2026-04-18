-- ============================================
-- Phase 1B: Cache Management Procedures
-- Purpose: Cache invalidation and TTL management
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Master
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- PROCEDURE: Invalidate Configuration Cache
-- ============================================

IF OBJECT_ID('Master.spInvalidateCacheByKey', 'P') IS NOT NULL
    DROP PROCEDURE Master.spInvalidateCacheByKey;

GO

CREATE PROCEDURE Master.spInvalidateCacheByKey
    @CacheKey NVARCHAR(256),
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DeletedCount INT = 0;

    -- Mark cache entries as deleted (soft delete)
    UPDATE Master.CacheEntries
    SET IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE CacheKey = @CacheKey
    AND (@TenantId IS NULL OR TenantId = @TenantId)
    AND IsDeleted = 0;

    SELECT @DeletedCount = @@ROWCOUNT;

    PRINT '✅ Cache invalidated: ' + @CacheKey + ' (Entries: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ')';
END;

GO

-- ============================================
-- PROCEDURE: Invalidate Tenant Cache
-- ============================================

IF OBJECT_ID('Master.spInvalidateTenantCache', 'P') IS NOT NULL
    DROP PROCEDURE Master.spInvalidateTenantCache;

GO

CREATE PROCEDURE Master.spInvalidateTenantCache
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DeletedCount INT = 0;

    -- Invalidate all cache entries for tenant
    UPDATE Master.CacheEntries
    SET IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE TenantId = @TenantId
    AND IsDeleted = 0;

    SELECT @DeletedCount = @@ROWCOUNT;

    PRINT '✅ Invalidated all cache for tenant: ' + @TenantId + ' (Entries: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ')';
END;

GO

-- ============================================
-- PROCEDURE: Invalidate All Cache
-- ============================================

IF OBJECT_ID('dbo.spInvalidateAllCache', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spInvalidateAllCache;

GO

CREATE PROCEDURE dbo.spInvalidateAllCache
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DeletedCount INT = 0;

    -- Invalidate all cache globally
    UPDATE Master.CacheEntries
    SET IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE IsDeleted = 0;

    SELECT @DeletedCount = @@ROWCOUNT;

    PRINT '🔥 PURGED ALL CACHE: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' entries';
END;

GO

-- ============================================
-- PROCEDURE: Get Cache Entry
-- ============================================

IF OBJECT_ID('Master.spGetCacheEntry', 'P') IS NOT NULL
    DROP PROCEDURE Master.spGetCacheEntry;

GO

CREATE PROCEDURE Master.spGetCacheEntry
    @CacheKey NVARCHAR(256),
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IsExpired BIT = 0;

    -- Check if cache entry exists and is still valid
    SELECT TOP 1
        @IsExpired = CASE
            WHEN IsDeleted = 1 THEN 1
            WHEN ExpiresAt IS NOT NULL AND ExpiresAt < GETUTCDATE() THEN 1
            ELSE 0
        END
    FROM Master.CacheEntries
    WHERE CacheKey = @CacheKey
    AND (@TenantId IS NULL OR TenantId = @TenantId);

    IF @IsExpired = 1
    BEGIN
        -- Mark as deleted if expired
        UPDATE Master.CacheEntries
        SET IsDeleted = 1,
            UpdatedAt = GETUTCDATE()
        WHERE CacheKey = @CacheKey
        AND (@TenantId IS NULL OR TenantId = @TenantId);

        PRINT '⏰ Cache expired: ' + @CacheKey;
        RETURN;
    END

    -- Return cache entry if valid
    SELECT
        CacheEntryId,
        CacheKey,
        CacheValue,
        TenantId,
        ExpiresAt,
        CreatedAt,
        DATEDIFF(SECOND, GETUTCDATE(), ExpiresAt) AS SecondsRemaining
    FROM Master.CacheEntries
    WHERE CacheKey = @CacheKey
    AND (@TenantId IS NULL OR TenantId = @TenantId)
    AND IsDeleted = 0
    AND (ExpiresAt IS NULL OR ExpiresAt > GETUTCDATE());

    PRINT '✅ Cache hit: ' + @CacheKey;
END;

GO

-- ============================================
-- PROCEDURE: Set Cache Entry
-- ============================================

IF OBJECT_ID('Master.spSetCacheEntry', 'P') IS NOT NULL
    DROP PROCEDURE Master.spSetCacheEntry;

GO

CREATE PROCEDURE Master.spSetCacheEntry
    @CacheKey NVARCHAR(256),
    @CacheValue NVARCHAR(MAX),
    @TenantId NVARCHAR(128) = NULL,
    @TtlSeconds INT = 3600
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ExpiresAt DATETIME2 = DATEADD(SECOND, @TtlSeconds, GETUTCDATE());
    DECLARE @CacheEntryId UNIQUEIDENTIFIER;

    -- Check if entry exists
    IF EXISTS (
        SELECT 1 FROM Master.CacheEntries
        WHERE CacheKey = @CacheKey
        AND (@TenantId IS NULL OR TenantId = @TenantId)
        AND IsDeleted = 0
    )
    BEGIN
        -- Update existing entry
        UPDATE Master.CacheEntries
        SET CacheValue = @CacheValue,
            ExpiresAt = @ExpiresAt,
            UpdatedAt = GETUTCDATE(),
            IsDeleted = 0
        WHERE CacheKey = @CacheKey
        AND (@TenantId IS NULL OR TenantId = @TenantId);

        PRINT '🔄 Cache updated: ' + @CacheKey + ' (TTL: ' + CAST(@TtlSeconds AS NVARCHAR(10)) + 's)';
    END
    ELSE
    BEGIN
        -- Insert new entry
        SET @CacheEntryId = NEWID();

        INSERT INTO Master.CacheEntries (
            CacheEntryId,
            CacheKey,
            CacheValue,
            TenantId,
            ExpiresAt,
            CreatedAt,
            CreatedBy,
            IsDeleted
        ) VALUES (
            @CacheEntryId,
            @CacheKey,
            @CacheValue,
            @TenantId,
            @ExpiresAt,
            GETUTCDATE(),
            'SYSTEM',
            0
        );

        PRINT '➕ Cache created: ' + @CacheKey + ' (TTL: ' + CAST(@TtlSeconds AS NVARCHAR(10)) + 's)';
    END
END;

GO

-- ============================================
-- PROCEDURE: Clean Expired Cache Entries
-- ============================================

IF OBJECT_ID('dbo.spCleanExpiredCache', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spCleanExpiredCache;

GO

CREATE PROCEDURE dbo.spCleanExpiredCache
    @DryRun BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ExpiredCount INT = 0;

    -- Count expired entries
    SELECT @ExpiredCount = COUNT(*)
    FROM Master.CacheEntries
    WHERE ExpiresAt IS NOT NULL
    AND ExpiresAt < GETUTCDATE()
    AND IsDeleted = 0;

    IF @DryRun = 1
    BEGIN
        PRINT '🔍 DRY RUN: Expired Cache Cleanup';
        PRINT 'Entries to clean: ' + CAST(@ExpiredCount AS NVARCHAR(10));
        RETURN;
    END

    -- Mark expired entries as deleted
    UPDATE Master.CacheEntries
    SET IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE ExpiresAt IS NOT NULL
    AND ExpiresAt < GETUTCDATE()
    AND IsDeleted = 0;

    PRINT '🧹 Cleaned expired cache entries: ' + CAST(@ExpiredCount AS NVARCHAR(10));
END;

GO

-- ============================================
-- PROCEDURE: Cache Statistics Report
-- ============================================

IF OBJECT_ID('dbo.spCacheStatistics', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spCacheStatistics;

GO

CREATE PROCEDURE dbo.spCacheStatistics
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'CACHE STATISTICS REPORT';
    PRINT '═══════════════════════════════════════════';

    -- 1. Overall cache stats
    PRINT '';
    PRINT '📊 OVERALL STATISTICS:';
    SELECT
        COUNT(*) AS TotalEntries,
        SUM(CASE WHEN IsDeleted = 0 AND (ExpiresAt IS NULL OR ExpiresAt > GETUTCDATE()) THEN 1 ELSE 0 END) AS ActiveEntries,
        SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) AS DeletedEntries,
        SUM(CASE WHEN ExpiresAt IS NOT NULL AND ExpiresAt < GETUTCDATE() THEN 1 ELSE 0 END) AS ExpiredEntries
    FROM Master.CacheEntries
    WHERE @TenantId IS NULL OR TenantId = @TenantId;

    -- 2. Cache by tenant
    PRINT '';
    PRINT '🏢 CACHE BY TENANT:';
    SELECT
        TenantId,
        COUNT(*) AS EntryCount,
        SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END) AS ActiveCount
    FROM Master.CacheEntries
    WHERE (@TenantId IS NULL OR TenantId = @TenantId)
    GROUP BY TenantId;

    -- 3. Top cached keys
    PRINT '';
    PRINT '🔑 TOP CACHED KEYS:';
    SELECT TOP 20
        CacheKey,
        COUNT(*) AS HitCount,
        MAX(CreatedAt) AS LastCreated,
        MIN(ExpiresAt) AS NextExpiry
    FROM Master.CacheEntries
    WHERE IsDeleted = 0
    AND (@TenantId IS NULL OR TenantId = @TenantId)
    GROUP BY CacheKey
    ORDER BY HitCount DESC;

    -- 4. TTL distribution
    PRINT '';
    PRINT '⏱️ TTL DISTRIBUTION:';
    SELECT
        CASE
            WHEN ExpiresAt IS NULL THEN 'Never'
            WHEN DATEDIFF(HOUR, GETUTCDATE(), ExpiresAt) < 1 THEN '< 1 hour'
            WHEN DATEDIFF(DAY, GETUTCDATE(), ExpiresAt) = 0 THEN '< 1 day'
            WHEN DATEDIFF(DAY, GETUTCDATE(), ExpiresAt) <= 7 THEN '< 1 week'
            ELSE '> 1 week'
        END AS TtlRange,
        COUNT(*) AS EntryCount
    FROM Master.CacheEntries
    WHERE IsDeleted = 0
    AND (@TenantId IS NULL OR TenantId = @TenantId)
    GROUP BY
        CASE
            WHEN ExpiresAt IS NULL THEN 'Never'
            WHEN DATEDIFF(HOUR, GETUTCDATE(), ExpiresAt) < 1 THEN '< 1 hour'
            WHEN DATEDIFF(DAY, GETUTCDATE(), ExpiresAt) = 0 THEN '< 1 day'
            WHEN DATEDIFF(DAY, GETUTCDATE(), ExpiresAt) <= 7 THEN '< 1 week'
            ELSE '> 1 week'
        END;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1B: Cache Management successfully configured';
PRINT 'Total objects created:';
PRINT '  - 8 cache management procedures';
PRINT '  - Cache invalidation (by key, by tenant, all)';
PRINT '  - TTL-based expiration';
PRINT '  - Cache statistics and monitoring';
PRINT 'Status: Cache management ready for implementation';

GO
