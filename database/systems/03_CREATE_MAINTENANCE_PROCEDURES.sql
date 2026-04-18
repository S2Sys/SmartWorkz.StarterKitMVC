-- ============================================
-- Phase 1A: Maintenance Procedures
-- Purpose: Data cleanup, archiving, and consistency checks
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Auth, Shared
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- PROCEDURE: Clean Expired AuthTokens
-- ============================================

IF OBJECT_ID('Auth.spCleanExpiredTokens', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spCleanExpiredTokens;

GO

CREATE PROCEDURE Auth.spCleanExpiredTokens
    @DaysToKeep INT = 90,
    @DryRun BIT = 1,
    @DeletedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@DaysToKeep, GETUTCDATE());
    DECLARE @Count INT = 0;

    -- Count tokens to delete
    SELECT @Count = COUNT(*)
    FROM Auth.AuthTokens
    WHERE ExpiresAt < @CutoffDate
    AND IsRevoked = 0;

    IF @DryRun = 1
    BEGIN
        PRINT '🔍 DRY RUN: Expired Token Cleanup';
        PRINT 'Cutoff Date: ' + CONVERT(NVARCHAR(19), @CutoffDate, 121);
        PRINT 'Tokens to clean: ' + CAST(@Count AS NVARCHAR(10));
        SET @DeletedCount = 0;
        RETURN;
    END

    -- Soft delete expired tokens
    UPDATE Auth.AuthTokens
    SET IsRevoked = 1,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = 'SYSTEM'
    WHERE ExpiresAt < @CutoffDate
    AND IsRevoked = 0;

    SET @DeletedCount = @Count;

    PRINT '✅ Expired tokens revoked: ' + CAST(@DeletedCount AS NVARCHAR(10));
END;

GO

-- ============================================
-- PROCEDURE: Archive Old AuditLogs
-- ============================================

IF OBJECT_ID('Shared.spArchiveOldAuditLogs', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spArchiveOldAuditLogs;

GO

CREATE PROCEDURE Shared.spArchiveOldAuditLogs
    @MonthsToKeep INT = 12,
    @DryRun BIT = 1,
    @ArchivedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CutoffDate DATETIME2 = DATEADD(MONTH, -@MonthsToKeep, GETUTCDATE());
    DECLARE @Count INT = 0;

    -- Count logs to archive
    SELECT @Count = COUNT(*)
    FROM Shared.AuditLogs
    WHERE CreatedAt < @CutoffDate
    AND IsDeleted = 0;

    IF @DryRun = 1
    BEGIN
        PRINT '🔍 DRY RUN: Audit Log Archival';
        PRINT 'Cutoff Date: ' + CONVERT(NVARCHAR(19), @CutoffDate, 121);
        PRINT 'Logs to archive: ' + CAST(@Count AS NVARCHAR(10));
        SET @ArchivedCount = 0;
        RETURN;
    END

    -- Soft delete old logs (archive)
    UPDATE Shared.AuditLogs
    SET IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE CreatedAt < @CutoffDate
    AND IsDeleted = 0;

    SET @ArchivedCount = @Count;

    PRINT '✅ Audit logs archived: ' + CAST(@ArchivedCount AS NVARCHAR(10));
END;

GO

-- ============================================
-- PROCEDURE: Detect Orphaned Records
-- ============================================

IF OBJECT_ID('dbo.spDetectOrphanedRecords', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spDetectOrphanedRecords;

GO

CREATE PROCEDURE dbo.spDetectOrphanedRecords
    @SchemaName NVARCHAR(128) = NULL,
    @TableName NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Report TABLE (
        SchemaName NVARCHAR(128),
        TableName NVARCHAR(128),
        ForeignKeyName NVARCHAR(128),
        OrphanedRecords INT,
        ReferencedTable NVARCHAR(128)
    );

    -- Check for orphaned records in Auth.UserRoles (invalid RoleId)
    INSERT INTO @Report
    SELECT
        'Auth' AS SchemaName,
        'UserRoles' AS TableName,
        'FK_UserRoles_Roles' AS ForeignKeyName,
        COUNT(*) AS OrphanedRecords,
        'Auth.Roles' AS ReferencedTable
    FROM Auth.UserRoles ur
    WHERE NOT EXISTS (SELECT 1 FROM Auth.Roles r WHERE r.RoleId = ur.RoleId)
    HAVING COUNT(*) > 0;

    -- Check for orphaned records in Auth.RolePermissions (invalid RoleId)
    INSERT INTO @Report
    SELECT
        'Auth' AS SchemaName,
        'RolePermissions' AS TableName,
        'FK_RolePermissions_Roles' AS ForeignKeyName,
        COUNT(*) AS OrphanedRecords,
        'Auth.Roles' AS ReferencedTable
    FROM Auth.RolePermissions rp
    WHERE NOT EXISTS (SELECT 1 FROM Auth.Roles r WHERE r.RoleId = rp.RoleId)
    HAVING COUNT(*) > 0;

    -- Check for orphaned records in Master.Categories (invalid ParentCategoryId)
    INSERT INTO @Report
    SELECT
        'Master' AS SchemaName,
        'Categories' AS TableName,
        'FK_Categories_ParentCategory' AS ForeignKeyName,
        COUNT(*) AS OrphanedRecords,
        'Master.Categories' AS ReferencedTable
    FROM Master.Categories c
    WHERE c.ParentCategoryId IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM Master.Categories p WHERE p.CategoryId = c.ParentCategoryId)
    HAVING COUNT(*) > 0;

    -- Display report
    IF (SELECT COUNT(*) FROM @Report) = 0
    BEGIN
        PRINT '✅ No orphaned records detected';
    END
    ELSE
    BEGIN
        PRINT '⚠️ Orphaned records detected:';
        SELECT * FROM @Report;
    END
END;

GO

-- ============================================
-- PROCEDURE: Data Consistency Check
-- ============================================

IF OBJECT_ID('dbo.spDataConsistencyCheck', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spDataConsistencyCheck;

GO

CREATE PROCEDURE dbo.spDataConsistencyCheck
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Issues TABLE (
        Issue NVARCHAR(MAX),
        Severity NVARCHAR(10),
        AffectedRecords INT
    );

    -- Check 1: Users with deleted tenants
    INSERT INTO @Issues
    SELECT
        'Users assigned to deleted tenants',
        'HIGH',
        COUNT(*)
    FROM Auth.Users u
    WHERE NOT EXISTS (SELECT 1 FROM Master.Tenants t WHERE t.TenantId = u.TenantId AND t.IsDeleted = 0)
    HAVING COUNT(*) > 0;

    -- Check 2: Active deleted records
    INSERT INTO @Issues
    SELECT
        'Records marked both Active (1) and Deleted (1)',
        'MEDIUM',
        COUNT(*)
    FROM Auth.Users
    WHERE IsActive = 1 AND IsDeleted = 1
    HAVING COUNT(*) > 0;

    -- Check 3: Future UpdatedAt dates
    INSERT INTO @Issues
    SELECT
        'Records with UpdatedAt in the future',
        'LOW',
        COUNT(*)
    FROM Auth.Users
    WHERE UpdatedAt > GETUTCDATE()
    HAVING COUNT(*) > 0;

    -- Check 4: CreatedAt after UpdatedAt
    INSERT INTO @Issues
    SELECT
        'Records where CreatedAt > UpdatedAt',
        'MEDIUM',
        COUNT(*)
    FROM Auth.Users
    WHERE CreatedAt > UpdatedAt
    HAVING COUNT(*) > 0;

    -- Check 5: Configuration with empty values
    INSERT INTO @Issues
    SELECT
        'Configuration keys with NULL or empty values',
        'LOW',
        COUNT(*)
    FROM Master.Configuration
    WHERE (Value IS NULL OR LEN(Value) = 0)
    AND IsDeleted = 0
    HAVING COUNT(*) > 0;

    -- Display report
    IF (SELECT COUNT(*) FROM @Issues) = 0
    BEGIN
        PRINT '✅ All consistency checks passed';
    END
    ELSE
    BEGIN
        PRINT '⚠️ Consistency issues found:';
        SELECT
            Issue,
            Severity,
            AffectedRecords
        FROM @Issues
        ORDER BY
            CASE Severity WHEN 'HIGH' THEN 1 WHEN 'MEDIUM' THEN 2 ELSE 3 END;
    END
END;

GO

-- ============================================
-- PROCEDURE: Cleanup Inactive Logins
-- ============================================

IF OBJECT_ID('Auth.spCleanupInactiveLogins', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spCleanupInactiveLogins;

GO

CREATE PROCEDURE Auth.spCleanupInactiveLogins
    @DaysInactive INT = 180,
    @DryRun BIT = 1,
    @UpdatedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@DaysInactive, GETUTCDATE());
    DECLARE @Count INT = 0;

    -- Count users not logged in
    SELECT @Count = COUNT(*)
    FROM Auth.Users
    WHERE (LastLoginAt IS NULL OR LastLoginAt < @CutoffDate)
    AND IsActive = 1
    AND IsDeleted = 0;

    IF @DryRun = 1
    BEGIN
        PRINT '🔍 DRY RUN: Inactive Login Cleanup';
        PRINT 'Cutoff Date: ' + CONVERT(NVARCHAR(19), @CutoffDate, 121);
        PRINT 'Users to deactivate: ' + CAST(@Count AS NVARCHAR(10));
        SET @UpdatedCount = 0;
        RETURN;
    END

    -- Deactivate inactive users
    UPDATE Auth.Users
    SET IsActive = 0,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = 'SYSTEM'
    WHERE (LastLoginAt IS NULL OR LastLoginAt < @CutoffDate)
    AND IsActive = 1
    AND IsDeleted = 0;

    SET @UpdatedCount = @Count;

    PRINT '✅ Inactive users deactivated: ' + CAST(@UpdatedCount AS NVARCHAR(10));
END;

GO

-- ============================================
-- PROCEDURE: Cleanup Failed Logins
-- ============================================

IF OBJECT_ID('Auth.spCleanupFailedLoginAttempts', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spCleanupFailedLoginAttempts;

GO

CREATE PROCEDURE Auth.spCleanupFailedLoginAttempts
    @DaysToKeep INT = 90,
    @DryRun BIT = 1,
    @DeletedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@DaysToKeep, GETUTCDATE());
    DECLARE @Count INT = 0;

    -- Count old login attempts
    SELECT @Count = COUNT(*)
    FROM Auth.LoginAttempts
    WHERE AttemptedAt < @CutoffDate
    AND IsDeleted = 0;

    IF @DryRun = 1
    BEGIN
        PRINT '🔍 DRY RUN: Failed Login Cleanup';
        PRINT 'Cutoff Date: ' + CONVERT(NVARCHAR(19), @CutoffDate, 121);
        PRINT 'Login attempts to archive: ' + CAST(@Count AS NVARCHAR(10));
        SET @DeletedCount = 0;
        RETURN;
    END

    -- Archive old login attempts
    UPDATE Auth.LoginAttempts
    SET IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE AttemptedAt < @CutoffDate
    AND IsDeleted = 0;

    SET @DeletedCount = @Count;

    PRINT '✅ Login attempts archived: ' + CAST(@DeletedCount AS NVARCHAR(10));
END;

GO

-- ============================================
-- MASTER CLEANUP PROCEDURE
-- ============================================

IF OBJECT_ID('dbo.spRunMaintenanceCycle', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spRunMaintenanceCycle;

GO

CREATE PROCEDURE dbo.spRunMaintenanceCycle
    @DryRun BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DeletedCount INT = 0;
    DECLARE @ArchivedCount INT = 0;
    DECLARE @UpdatedCount INT = 0;

    PRINT '═══════════════════════════════════════════';
    PRINT 'MAINTENANCE CYCLE STARTING';
    PRINT '═══════════════════════════════════════════';

    -- 1. Clean expired tokens
    EXEC Auth.spCleanExpiredTokens @DaysToKeep = 90, @DryRun = @DryRun, @DeletedCount = @DeletedCount OUTPUT;

    -- 2. Archive old audit logs
    EXEC Shared.spArchiveOldAuditLogs @MonthsToKeep = 12, @DryRun = @DryRun, @ArchivedCount = @ArchivedCount OUTPUT;

    -- 3. Cleanup inactive logins
    EXEC Auth.spCleanupInactiveLogins @DaysInactive = 180, @DryRun = @DryRun, @UpdatedCount = @UpdatedCount OUTPUT;

    -- 4. Cleanup failed login attempts
    EXEC Auth.spCleanupFailedLoginAttempts @DaysToKeep = 90, @DryRun = @DryRun, @DeletedCount = @DeletedCount OUTPUT;

    -- 5. Detect orphaned records
    PRINT '';
    EXEC dbo.spDetectOrphanedRecords;

    -- 6. Run consistency check
    PRINT '';
    EXEC dbo.spDataConsistencyCheck;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT 'MAINTENANCE CYCLE COMPLETE';
    PRINT '═══════════════════════════════════════════';

    IF @DryRun = 1
    BEGIN
        PRINT 'Mode: DRY RUN (no changes made)';
        PRINT 'To execute for real, call with @DryRun = 0';
    END
    ELSE
    BEGIN
        PRINT '✅ All maintenance tasks completed';
    END
END;

GO

PRINT '✅ Phase 1A: Maintenance Procedures successfully created';
PRINT 'Total procedures created:';
PRINT '  - spCleanExpiredTokens (revoke expired auth tokens)';
PRINT '  - spArchiveOldAuditLogs (archive historical logs)';
PRINT '  - spDetectOrphanedRecords (find orphaned data)';
PRINT '  - spDataConsistencyCheck (validate data integrity)';
PRINT '  - spCleanupInactiveLogins (deactivate inactive users)';
PRINT '  - spCleanupFailedLoginAttempts (archive old attempts)';
PRINT '  - spRunMaintenanceCycle (master cleanup)';
PRINT 'Status: Ready for security procedures (Phase 1A Step 4)';

GO
