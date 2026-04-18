-- ============================================
-- Phase 1C: Advanced Monitoring & Backup Recovery
-- Purpose: Query performance monitoring and backup procedures
-- Database: SQL Server (Boilerplate v3)
-- Schemas: dbo, Report
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- TABLE: Query Performance Tracking
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'QueryPerformanceLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.QueryPerformanceLog (
        QueryPerformanceLogId BIGINT PRIMARY KEY IDENTITY(1,1),
        QueryHash NVARCHAR(64),
        QueryText NVARCHAR(MAX),
        ExecutionCount INT NOT NULL DEFAULT 1,
        TotalElapsedTimeMs BIGINT,
        AvgElapsedTimeMs BIGINT,
        MinElapsedTimeMs INT,
        MaxElapsedTimeMs INT,
        TotalLogicalReads BIGINT,
        TotalPhysicalReads BIGINT,
        IsSlowQuery BIT NOT NULL DEFAULT 0,
        RecordedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_QueryPerformanceLog_Hash ON dbo.QueryPerformanceLog(QueryHash);
    CREATE INDEX IX_QueryPerformanceLog_Slow ON dbo.QueryPerformanceLog(IsSlowQuery);
    PRINT '✅ Created QueryPerformanceLog table';
END

GO

-- ============================================
-- PROCEDURE: Log Query Performance
-- ============================================

IF OBJECT_ID('dbo.spLogQueryPerformance', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spLogQueryPerformance;

GO

CREATE PROCEDURE dbo.spLogQueryPerformance
    @QueryText NVARCHAR(MAX),
    @ExecutionTimeMs INT,
    @LogicalReads BIGINT = 0,
    @PhysicalReads BIGINT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueryHash NVARCHAR(64);
    DECLARE @IsSlowQuery BIT = CASE WHEN @ExecutionTimeMs > 1000 THEN 1 ELSE 0 END;

    -- Calculate hash of query (first 64 chars)
    SET @QueryHash = SUBSTRING(@QueryText, 1, 64);

    -- Check if query already logged
    IF EXISTS (SELECT 1 FROM dbo.QueryPerformanceLog WHERE QueryHash = @QueryHash AND IsDeleted = 0)
    BEGIN
        UPDATE dbo.QueryPerformanceLog
        SET ExecutionCount = ExecutionCount + 1,
            TotalElapsedTimeMs = TotalElapsedTimeMs + @ExecutionTimeMs,
            AvgElapsedTimeMs = (TotalElapsedTimeMs + @ExecutionTimeMs) / (ExecutionCount + 1),
            MaxElapsedTimeMs = CASE WHEN @ExecutionTimeMs > MaxElapsedTimeMs THEN @ExecutionTimeMs ELSE MaxElapsedTimeMs END,
            MinElapsedTimeMs = CASE WHEN @ExecutionTimeMs < MinElapsedTimeMs THEN @ExecutionTimeMs ELSE MinElapsedTimeMs END,
            TotalLogicalReads = TotalLogicalReads + @LogicalReads,
            TotalPhysicalReads = TotalPhysicalReads + @PhysicalReads,
            IsSlowQuery = @IsSlowQuery
        WHERE QueryHash = @QueryHash
        AND IsDeleted = 0;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.QueryPerformanceLog (
            QueryHash,
            QueryText,
            ExecutionCount,
            TotalElapsedTimeMs,
            AvgElapsedTimeMs,
            MinElapsedTimeMs,
            MaxElapsedTimeMs,
            TotalLogicalReads,
            TotalPhysicalReads,
            IsSlowQuery,
            RecordedAt,
            IsDeleted
        ) VALUES (
            @QueryHash,
            @QueryText,
            1,
            @ExecutionTimeMs,
            @ExecutionTimeMs,
            @ExecutionTimeMs,
            @ExecutionTimeMs,
            @LogicalReads,
            @PhysicalReads,
            @IsSlowQuery,
            GETUTCDATE(),
            0
        );
    END

    IF @IsSlowQuery = 1
        PRINT '⚠️ Slow query logged: ' + CAST(@ExecutionTimeMs AS NVARCHAR(10)) + 'ms';
END;

GO

-- ============================================
-- PROCEDURE: Slow Query Analysis
-- ============================================

IF OBJECT_ID('Report.spSlowQueryAnalysis', 'P') IS NOT NULL
    DROP PROCEDURE Report.spSlowQueryAnalysis;

GO

CREATE PROCEDURE Report.spSlowQueryAnalysis
    @ThresholdMs INT = 1000,
    @TopCount INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'SLOW QUERY ANALYSIS';
    PRINT 'Threshold: ' + CAST(@ThresholdMs AS NVARCHAR(10)) + 'ms';
    PRINT '═══════════════════════════════════════════';

    -- 1. Slowest queries
    PRINT '';
    PRINT '🐢 SLOWEST QUERIES:';
    SELECT TOP @TopCount
        QueryHash,
        SUBSTRING(QueryText, 1, 80) AS QueryPreview,
        ExecutionCount,
        AvgElapsedTimeMs,
        MaxElapsedTimeMs,
        TotalLogicalReads,
        RecordedAt
    FROM dbo.QueryPerformanceLog
    WHERE IsSlowQuery = 1
    AND IsDeleted = 0
    ORDER BY AvgElapsedTimeMs DESC;

    -- 2. Most frequently slow
    PRINT '';
    PRINT '📊 MOST FREQUENTLY SLOW QUERIES:';
    SELECT TOP @TopCount
        QueryHash,
        SUBSTRING(QueryText, 1, 80) AS QueryPreview,
        ExecutionCount,
        AvgElapsedTimeMs
    FROM dbo.QueryPerformanceLog
    WHERE IsSlowQuery = 1
    AND IsDeleted = 0
    ORDER BY ExecutionCount DESC;

    -- 3. Query with most reads
    PRINT '';
    PRINT '📖 QUERIES WITH MOST I/O:';
    SELECT TOP @TopCount
        QueryHash,
        SUBSTRING(QueryText, 1, 80) AS QueryPreview,
        TotalLogicalReads,
        TotalPhysicalReads,
        AvgElapsedTimeMs
    FROM dbo.QueryPerformanceLog
    WHERE IsDeleted = 0
    AND (TotalLogicalReads > 0 OR TotalPhysicalReads > 0)
    ORDER BY TotalLogicalReads DESC;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Resource Usage Monitoring
-- ============================================

IF OBJECT_ID('Report.spResourceUsageMonitoring', 'P') IS NOT NULL
    DROP PROCEDURE Report.spResourceUsageMonitoring;

GO

CREATE PROCEDURE Report.spResourceUsageMonitoring
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'RESOURCE USAGE MONITORING';
    PRINT '═══════════════════════════════════════════';

    -- 1. Current connections
    PRINT '';
    PRINT '🔌 CURRENT CONNECTIONS:';
    SELECT
        @@SPID AS CurrentSessionId,
        (SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE database_id = DB_ID()) AS TotalConnections;

    -- 2. Memory usage
    PRINT '';
    PRINT '💾 MEMORY USAGE:';
    SELECT
        (SELECT COUNT(*) FROM sys.dm_exec_requests) AS ActiveRequests,
        (SELECT SUM(granted_memory_kb) FROM sys.dm_exec_query_memory_grants) AS GrantedMemoryMB;

    -- 3. Lock waits
    PRINT '';
    PRINT '🔒 LOCK WAITS:';
    SELECT TOP 10
        wait_type,
        waiting_task_count,
        wait_time_ms
    FROM sys.dm_os_wait_stats
    WHERE wait_type NOT LIKE '%SLEEP%'
    ORDER BY wait_time_ms DESC;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Database Backup Procedure
-- ============================================

IF OBJECT_ID('dbo.spCreateFullBackup', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spCreateFullBackup;

GO

CREATE PROCEDURE dbo.spCreateFullBackup
    @BackupPath NVARCHAR(260) = NULL,
    @BackupName NVARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatabaseName NVARCHAR(128) = DB_NAME();
    DECLARE @BackupFile NVARCHAR(260);
    DECLARE @Timestamp NVARCHAR(19);

    -- Generate timestamp for unique backup file
    SET @Timestamp = FORMAT(GETUTCDATE(), 'yyyyMMdd_HHmmss');

    -- Set default backup path if not provided
    IF @BackupPath IS NULL
        SET @BackupPath = 'C:\Backups\';

    -- Generate backup file name
    IF @BackupName IS NULL
        SET @BackupFile = @BackupPath + @DatabaseName + '_Full_' + @Timestamp + '.bak';
    ELSE
        SET @BackupFile = @BackupPath + @BackupName + '_' + @Timestamp + '.bak';

    PRINT '═══════════════════════════════════════════';
    PRINT 'DATABASE BACKUP PROCEDURE';
    PRINT 'Database: ' + @DatabaseName;
    PRINT 'Backup File: ' + @BackupFile;
    PRINT '═══════════════════════════════════════════';

    BEGIN TRY
        -- Perform full backup
        BACKUP DATABASE @DatabaseName
        TO DISK = @BackupFile
        WITH NOFORMAT, NOINIT, NAME = @DatabaseName + ' Full Backup', SKIP, NOREWIND, NOUNLOAD, COMPRESSION, CHECKSUM, STATS = 10;

        PRINT '';
        PRINT '✅ BACKUP COMPLETED SUCCESSFULLY';
        PRINT 'File: ' + @BackupFile;
        PRINT 'Time: ' + CONVERT(NVARCHAR(19), GETUTCDATE(), 121);
    END TRY
    BEGIN CATCH
        PRINT '';
        PRINT '❌ BACKUP FAILED';
        PRINT 'Error: ' + ERROR_MESSAGE();
    END CATCH

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Database Integrity Check
-- ============================================

IF OBJECT_ID('dbo.spCheckDatabaseIntegrity', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spCheckDatabaseIntegrity;

GO

CREATE PROCEDURE dbo.spCheckDatabaseIntegrity
    @RepairMode BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatabaseName NVARCHAR(128) = DB_NAME();
    DECLARE @Command NVARCHAR(500);

    PRINT '═══════════════════════════════════════════';
    PRINT 'DATABASE INTEGRITY CHECK';
    PRINT 'Database: ' + @DatabaseName;
    PRINT '═══════════════════════════════════════════';

    BEGIN TRY
        IF @RepairMode = 0
        BEGIN
            PRINT '';
            PRINT 'Running CHECKDB (No repair)...';
            DBCC CHECKDB(@DatabaseName, NOINDEX) WITH NO_INFOMSGS;
            PRINT '✅ Database integrity check completed';
        END
        ELSE
        BEGIN
            PRINT '';
            PRINT '⚠️ Running CHECKDB with REPAIR_ALLOW_DATA_LOSS...';
            DBCC CHECKDB(@DatabaseName, REPAIR_ALLOW_DATA_LOSS) WITH NO_INFOMSGS;
            PRINT '✅ Database repair completed';
        END
    END TRY
    BEGIN CATCH
        PRINT '❌ Error during integrity check: ' + ERROR_MESSAGE();
    END CATCH

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1C: Monitoring & Backup successfully configured';
PRINT 'Total objects created:';
PRINT '  - 1 query performance tracking table';
PRINT '  - 5 monitoring and backup procedures (log, analysis, resource, backup, integrity)';
PRINT 'Status: Advanced monitoring and backup ready';

GO
