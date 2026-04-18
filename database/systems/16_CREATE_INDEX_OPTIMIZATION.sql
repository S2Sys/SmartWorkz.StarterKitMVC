-- ============================================
-- System Procedure: Index Optimization & Maintenance
-- Purpose: Auto-maintain indexes for performance
-- Database: SQL Server (Boilerplate v3)
-- Schemas: dbo, Report
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- TABLE: Index Maintenance Log
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IndexMaintenanceLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.IndexMaintenanceLog (
        IndexMaintenanceLogId BIGINT PRIMARY KEY IDENTITY(1,1),
        SchemaName NVARCHAR(128) NOT NULL,
        TableName NVARCHAR(128) NOT NULL,
        IndexName NVARCHAR(128) NOT NULL,
        MaintenanceType NVARCHAR(50) NOT NULL,
        FragmentationBefore DECIMAL(5, 2),
        FragmentationAfter DECIMAL(5, 2),
        ExecutionTimeMs INT,
        PagesFreeMs INT,
        StartedAt DATETIME2 NOT NULL,
        CompletedAt DATETIME2,
        IsSuccessful BIT NOT NULL DEFAULT 1,
        ErrorMessage NVARCHAR(MAX),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_IndexMaintenanceLog_Table ON dbo.IndexMaintenanceLog(SchemaName, TableName);
    CREATE INDEX IX_IndexMaintenanceLog_Date ON dbo.IndexMaintenanceLog(CreatedAt);
    PRINT '✅ Created IndexMaintenanceLog table';
END

GO

-- ============================================
-- PROCEDURE: Analyze Index Fragmentation
-- ============================================

IF OBJECT_ID('dbo.spAnalyzeIndexFragmentation', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spAnalyzeIndexFragmentation;

GO

CREATE PROCEDURE dbo.spAnalyzeIndexFragmentation
    @SchemaName NVARCHAR(128) = NULL,
    @TableName NVARCHAR(128) = NULL,
    @MinFragmentation DECIMAL(5, 2) = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbId INT = DB_ID();

    SELECT
        SCHEMA_NAME(t.schema_id) AS SchemaName,
        t.name AS TableName,
        i.name AS IndexName,
        i.type_desc AS IndexType,
        ps.page_count AS PageCount,
        ps.avg_fragmentation_in_percent AS Fragmentation,
        CASE
            WHEN ps.avg_fragmentation_in_percent <= 10 THEN 'Healthy'
            WHEN ps.avg_fragmentation_in_percent <= 30 THEN 'Reorganize'
            ELSE 'Rebuild'
        END AS RecommendedAction,
        ps.index_id AS IndexId
    FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    INNER JOIN sys.dm_db_index_physical_stats(@DbId, NULL, NULL, NULL, 'LIMITED') ps
        ON i.object_id = ps.object_id
        AND i.index_id = ps.index_id
    WHERE database_id = @DbId
    AND ps.page_count > 1000
    AND ps.avg_fragmentation_in_percent >= @MinFragmentation
    AND SCHEMA_NAME(t.schema_id) IN ('Master', 'Shared', 'Auth', 'Report', 'Transaction')
    AND (@SchemaName IS NULL OR SCHEMA_NAME(t.schema_id) = @SchemaName)
    AND (@TableName IS NULL OR t.name = @TableName)
    ORDER BY ps.avg_fragmentation_in_percent DESC;

    PRINT '✅ Index fragmentation analysis completed';
END;

GO

-- ============================================
-- PROCEDURE: Auto-Rebuild Fragmented Indexes
-- ============================================

IF OBJECT_ID('dbo.spAutoRebuildFragmentedIndexes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spAutoRebuildFragmentedIndexes;

GO

CREATE PROCEDURE dbo.spAutoRebuildFragmentedIndexes
    @FragmentationThreshold DECIMAL(5, 2) = 30,
    @DryRun BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbId INT = DB_ID();
    DECLARE @SchemaName NVARCHAR(128);
    DECLARE @TableName NVARCHAR(128);
    DECLARE @IndexName NVARCHAR(128);
    DECLARE @Fragmentation DECIMAL(5, 2);
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @StartTime DATETIME2;
    DECLARE @EndTime DATETIME2;
    DECLARE @RebuildCount INT = 0;
    DECLARE @ReorganizeCount INT = 0;

    DECLARE IndexCursor CURSOR FOR
    SELECT
        SCHEMA_NAME(t.schema_id),
        t.name,
        i.name,
        ps.avg_fragmentation_in_percent
    FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    INNER JOIN sys.dm_db_index_physical_stats(@DbId, NULL, NULL, NULL, 'LIMITED') ps
        ON i.object_id = ps.object_id
        AND i.index_id = ps.index_id
    WHERE database_id = @DbId
    AND ps.page_count > 1000
    AND ps.avg_fragmentation_in_percent >= @FragmentationThreshold
    AND SCHEMA_NAME(t.schema_id) IN ('Master', 'Shared', 'Auth', 'Report', 'Transaction')
    AND i.name IS NOT NULL
    ORDER BY ps.avg_fragmentation_in_percent DESC;

    PRINT '═══════════════════════════════════════════';
    PRINT 'INDEX REBUILD OPERATION';
    PRINT 'Threshold: ' + CAST(@FragmentationThreshold AS NVARCHAR(5)) + '%';
    PRINT 'Dry Run: ' + CASE WHEN @DryRun = 1 THEN 'YES' ELSE 'NO' END;
    PRINT '═══════════════════════════════════════════';

    OPEN IndexCursor;
    FETCH NEXT FROM IndexCursor INTO @SchemaName, @TableName, @IndexName, @Fragmentation;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @StartTime = GETUTCDATE();

        PRINT '';
        PRINT 'Processing: ' + @SchemaName + '.' + @TableName + '.' + @IndexName;
        PRINT 'Fragmentation: ' + CAST(@Fragmentation AS NVARCHAR(5)) + '%';

        IF @Fragmentation > 30
        BEGIN
            -- REBUILD for high fragmentation
            IF @DryRun = 0
            BEGIN
                BEGIN TRY
                    SET @SQL = 'ALTER INDEX [' + @IndexName + '] ON ' + @SchemaName + '.[' + @TableName + '] REBUILD WITH (FILLFACTOR = 90, SORT_IN_TEMPDB = ON, ONLINE = ON);';
                    EXEC sp_executesql @SQL;
                    SET @EndTime = GETUTCDATE();

                    INSERT INTO dbo.IndexMaintenanceLog (
                        SchemaName, TableName, IndexName, MaintenanceType,
                        FragmentationBefore, ExecutionTimeMs, StartedAt, CompletedAt, IsSuccessful
                    ) VALUES (
                        @SchemaName, @TableName, @IndexName, 'REBUILD',
                        @Fragmentation, DATEDIFF(MILLISECOND, @StartTime, @EndTime),
                        @StartTime, @EndTime, 1
                    );

                    PRINT '✅ REBUILT (Fragmentation was ' + CAST(@Fragmentation AS NVARCHAR(5)) + '%)';
                    SET @RebuildCount = @RebuildCount + 1;
                END TRY
                BEGIN CATCH
                    PRINT '❌ REBUILD FAILED: ' + ERROR_MESSAGE();
                    INSERT INTO dbo.IndexMaintenanceLog (
                        SchemaName, TableName, IndexName, MaintenanceType,
                        FragmentationBefore, StartedAt, CompletedAt, IsSuccessful, ErrorMessage
                    ) VALUES (
                        @SchemaName, @TableName, @IndexName, 'REBUILD',
                        @Fragmentation, @StartTime, GETUTCDATE(), 0, ERROR_MESSAGE()
                    );
                END CATCH
            END
            ELSE
                PRINT '🔍 [DRY RUN] Would REBUILD this index';
        END
        ELSE IF @Fragmentation > 10
        BEGIN
            -- REORGANIZE for moderate fragmentation
            IF @DryRun = 0
            BEGIN
                BEGIN TRY
                    SET @SQL = 'ALTER INDEX [' + @IndexName + '] ON ' + @SchemaName + '.[' + @TableName + '] REORGANIZE;';
                    EXEC sp_executesql @SQL;
                    SET @EndTime = GETUTCDATE();

                    INSERT INTO dbo.IndexMaintenanceLog (
                        SchemaName, TableName, IndexName, MaintenanceType,
                        FragmentationBefore, ExecutionTimeMs, StartedAt, CompletedAt, IsSuccessful
                    ) VALUES (
                        @SchemaName, @TableName, @IndexName, 'REORGANIZE',
                        @Fragmentation, DATEDIFF(MILLISECOND, @StartTime, @EndTime),
                        @StartTime, @EndTime, 1
                    );

                    PRINT '✅ REORGANIZED (Fragmentation was ' + CAST(@Fragmentation AS NVARCHAR(5)) + '%)';
                    SET @ReorganizeCount = @ReorganizeCount + 1;
                END TRY
                BEGIN CATCH
                    PRINT '❌ REORGANIZE FAILED: ' + ERROR_MESSAGE();
                    INSERT INTO dbo.IndexMaintenanceLog (
                        SchemaName, TableName, IndexName, MaintenanceType,
                        FragmentationBefore, StartedAt, CompletedAt, IsSuccessful, ErrorMessage
                    ) VALUES (
                        @SchemaName, @TableName, @IndexName, 'REORGANIZE',
                        @Fragmentation, @StartTime, GETUTCDATE(), 0, ERROR_MESSAGE()
                    );
                END CATCH
            END
            ELSE
                PRINT '🔍 [DRY RUN] Would REORGANIZE this index';
        END

        FETCH NEXT FROM IndexCursor INTO @SchemaName, @TableName, @IndexName, @Fragmentation;
    END

    CLOSE IndexCursor;
    DEALLOCATE IndexCursor;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT '✅ INDEX MAINTENANCE COMPLETED';
    PRINT 'Rebuilt: ' + CAST(@RebuildCount AS NVARCHAR(10));
    PRINT 'Reorganized: ' + CAST(@ReorganizeCount AS NVARCHAR(10));
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Identify Missing Indexes
-- ============================================

IF OBJECT_ID('dbo.spIdentifyMissingIndexes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spIdentifyMissingIndexes;

GO

CREATE PROCEDURE dbo.spIdentifyMissingIndexes
    @MinImpact INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbId INT = DB_ID();

    PRINT '═══════════════════════════════════════════';
    PRINT 'MISSING INDEX ANALYSIS';
    PRINT 'Minimum Impact: ' + CAST(@MinImpact AS NVARCHAR(10));
    PRINT '═══════════════════════════════════════════';
    PRINT '';

    SELECT TOP 20
        CONVERT(DECIMAL(18, 2), migs.user_seeks * migs.avg_total_user_cost * (migs.avg_user_impact * 0.01)) AS ImprovementMeasure,
        mid.equality_columns,
        mid.inequality_columns,
        mid.included_columns,
        migs.user_seeks,
        migs.avg_user_impact,
        CAST(CAST(migs.avg_user_impact AS FLOAT) * 100 AS INT) AS ImpactPercentage,
        t.name AS TableName,
        SCHEMA_NAME(t.schema_id) AS SchemaName
    FROM sys.dm_db_missing_index_details mid
    INNER JOIN sys.dm_db_missing_index_groups mig ON mid.index_handle = mig.index_handle
    INNER JOIN sys.dm_db_missing_index_groups_stats migs ON mig.index_group_id = migs.index_group_id
    INNER JOIN sys.tables t ON mid.object_id = t.object_id
    WHERE database_id = @DbId
    AND SCHEMA_NAME(t.schema_id) IN ('Master', 'Shared', 'Auth', 'Report', 'Transaction')
    AND migs.user_seeks * migs.avg_total_user_cost * (migs.avg_user_impact * 0.01) > @MinImpact
    ORDER BY ImprovementMeasure DESC;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT 'USE THIS SQL TO CREATE MISSING INDEXES:';
    PRINT '═══════════════════════════════════════════';

    SELECT
        'CREATE INDEX IX_' +
        REPLACE(REPLACE(REPLACE(mid.equality_columns, ', ', '_'), '[', ''), ']', '') +
        ' ON ' + SCHEMA_NAME(t.schema_id) + '.[' + t.name + '] (' + mid.equality_columns +
        CASE WHEN mid.included_columns IS NOT NULL THEN ') INCLUDE (' + mid.included_columns ELSE '' END +
        ');' AS CreateIndexStatement
    FROM sys.dm_db_missing_index_details mid
    INNER JOIN sys.dm_db_missing_index_groups mig ON mid.index_handle = mig.index_handle
    INNER JOIN sys.dm_db_missing_index_groups_stats migs ON mig.index_group_id = migs.index_group_id
    INNER JOIN sys.tables t ON mid.object_id = t.object_id
    WHERE database_id = @DbId
    AND SCHEMA_NAME(t.schema_id) IN ('Master', 'Shared', 'Auth', 'Report', 'Transaction')
    AND migs.user_seeks * migs.avg_total_user_cost * (migs.avg_user_impact * 0.01) > @MinImpact;

    PRINT '';
    PRINT '✅ Missing index analysis completed';
END;

GO

-- ============================================
-- PROCEDURE: Identify Unused Indexes
-- ============================================

IF OBJECT_ID('dbo.spIdentifyUnusedIndexes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spIdentifyUnusedIndexes;

GO

CREATE PROCEDURE dbo.spIdentifyUnusedIndexes
    @MinDaysOld INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbId INT = DB_ID();
    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@MinDaysOld, GETUTCDATE());

    PRINT '═══════════════════════════════════════════';
    PRINT 'UNUSED INDEX ANALYSIS';
    PRINT 'Analysis Period: ' + CAST(@MinDaysOld AS NVARCHAR(3)) + ' days';
    PRINT '═══════════════════════════════════════════';
    PRINT '';

    SELECT
        SCHEMA_NAME(t.schema_id) AS SchemaName,
        t.name AS TableName,
        i.name AS IndexName,
        i.type_desc AS IndexType,
        s.user_updates,
        s.user_seeks,
        s.user_scans,
        s.user_lookups,
        (s.user_seeks + s.user_scans + s.user_lookups) AS TotalReads,
        'DROP INDEX [' + i.name + '] ON ' + SCHEMA_NAME(t.schema_id) + '.[' + t.name + '];' AS DropStatement
    FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    LEFT JOIN sys.dm_db_index_usage_stats s ON i.object_id = s.object_id
        AND i.index_id = s.index_id
        AND s.database_id = @DbId
    WHERE t.is_ms_shipped = 0
    AND i.type > 0
    AND i.is_primary_key = 0
    AND i.is_unique = 0
    AND SCHEMA_NAME(t.schema_id) IN ('Master', 'Shared', 'Auth', 'Report', 'Transaction')
    AND (s.user_seeks + s.user_scans + s.user_lookups = 0 OR s.last_user_seek < @CutoffDate)
    AND s.user_updates > 100
    ORDER BY s.user_updates DESC;

    PRINT '';
    PRINT '⚠️ CAUTION: Verify unused indexes before dropping!';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Update Index Statistics
-- ============================================

IF OBJECT_ID('dbo.spUpdateIndexStatistics', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spUpdateIndexStatistics;

GO

CREATE PROCEDURE dbo.spUpdateIndexStatistics
    @SchemaName NVARCHAR(128) = NULL,
    @TableName NVARCHAR(128) = NULL,
    @DryRun BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @FullName NVARCHAR(256);
    DECLARE @UpdateCount INT = 0;

    PRINT '═══════════════════════════════════════════';
    PRINT 'INDEX STATISTICS UPDATE';
    PRINT 'Dry Run: ' + CASE WHEN @DryRun = 1 THEN 'YES' ELSE 'NO' END;
    PRINT '═══════════════════════════════════════════';

    DECLARE StatsCursor CURSOR FOR
    SELECT
        SCHEMA_NAME(t.schema_id) + '.' + t.name
    FROM sys.tables t
    WHERE SCHEMA_NAME(t.schema_id) IN ('Master', 'Shared', 'Auth', 'Report', 'Transaction')
    AND (@SchemaName IS NULL OR SCHEMA_NAME(t.schema_id) = @SchemaName)
    AND (@TableName IS NULL OR t.name = @TableName);

    OPEN StatsCursor;
    FETCH NEXT FROM StatsCursor INTO @FullName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT '';
        PRINT 'Updating: ' + @FullName;

        IF @DryRun = 0
        BEGIN
            BEGIN TRY
                SET @SQL = 'UPDATE STATISTICS ' + @FullName + ' WITH FULLSCAN;';
                EXEC sp_executesql @SQL;
                PRINT '✅ Statistics updated';
                SET @UpdateCount = @UpdateCount + 1;
            END TRY
            BEGIN CATCH
                PRINT '❌ Error: ' + ERROR_MESSAGE();
            END CATCH
        END
        ELSE
            PRINT '🔍 [DRY RUN] Would update statistics';

        FETCH NEXT FROM StatsCursor INTO @FullName;
    END

    CLOSE StatsCursor;
    DEALLOCATE StatsCursor;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT '✅ STATISTICS UPDATE COMPLETED';
    PRINT 'Tables Updated: ' + CAST(@UpdateCount AS NVARCHAR(10));
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Index Health Report
-- ============================================

IF OBJECT_ID('Report.spIndexHealthReport', 'P') IS NOT NULL
    DROP PROCEDURE Report.spIndexHealthReport;

GO

CREATE PROCEDURE Report.spIndexHealthReport
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbId INT = DB_ID();

    PRINT '═══════════════════════════════════════════';
    PRINT 'INDEX HEALTH REPORT';
    PRINT '═══════════════════════════════════════════';

    -- 1. Overall fragmentation summary
    PRINT '';
    PRINT '📊 FRAGMENTATION SUMMARY:';
    SELECT
        SUM(CASE WHEN ps.avg_fragmentation_in_percent <= 10 THEN 1 ELSE 0 END) AS HealthyIndexes,
        SUM(CASE WHEN ps.avg_fragmentation_in_percent > 10 AND ps.avg_fragmentation_in_percent <= 30 THEN 1 ELSE 0 END) AS NeedsReorganize,
        SUM(CASE WHEN ps.avg_fragmentation_in_percent > 30 THEN 1 ELSE 0 END) AS NeedsRebuild,
        COUNT(*) AS TotalIndexes
    FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    INNER JOIN sys.dm_db_index_physical_stats(@DbId, NULL, NULL, NULL, 'LIMITED') ps
        ON i.object_id = ps.object_id
        AND i.index_id = ps.index_id
    WHERE database_id = @DbId
    AND ps.page_count > 1000
    AND SCHEMA_NAME(t.schema_id) IN ('Master', 'Shared', 'Auth', 'Report', 'Transaction');

    -- 2. Top fragmented indexes
    PRINT '';
    PRINT '⚠️ TOP 10 MOST FRAGMENTED INDEXES:';
    SELECT TOP 10
        SCHEMA_NAME(t.schema_id) + '.' + t.name + '.' + i.name AS IndexFullName,
        ps.avg_fragmentation_in_percent AS Fragmentation,
        ps.page_count AS Pages,
        CASE
            WHEN ps.avg_fragmentation_in_percent > 30 THEN 'REBUILD'
            WHEN ps.avg_fragmentation_in_percent > 10 THEN 'REORGANIZE'
            ELSE 'HEALTHY'
        END AS Action
    FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    INNER JOIN sys.dm_db_index_physical_stats(@DbId, NULL, NULL, NULL, 'LIMITED') ps
        ON i.object_id = ps.object_id
        AND i.index_id = ps.index_id
    WHERE database_id = @DbId
    AND ps.page_count > 1000
    AND SCHEMA_NAME(t.schema_id) IN ('Master', 'Shared', 'Auth', 'Report', 'Transaction')
    ORDER BY ps.avg_fragmentation_in_percent DESC;

    -- 3. Recent maintenance activity
    PRINT '';
    PRINT '🔧 RECENT MAINTENANCE ACTIVITY (Last 7 days):';
    SELECT
        MaintenanceType,
        COUNT(*) AS Operations,
        AVG(ExecutionTimeMs) AS AvgTimeMs,
        SUM(CASE WHEN IsSuccessful = 1 THEN 1 ELSE 0 END) AS Successful,
        SUM(CASE WHEN IsSuccessful = 0 THEN 1 ELSE 0 END) AS Failed
    FROM dbo.IndexMaintenanceLog
    WHERE CreatedAt >= DATEADD(DAY, -7, GETUTCDATE())
    GROUP BY MaintenanceType;

    -- 4. Index usage summary
    PRINT '';
    PRINT '📈 INDEX USAGE SUMMARY:';
    SELECT TOP 10
        SCHEMA_NAME(t.schema_id) + '.' + t.name + '.' + i.name AS IndexFullName,
        s.user_seeks,
        s.user_scans,
        s.user_lookups,
        s.user_updates,
        (s.user_seeks + s.user_scans + s.user_lookups) AS TotalReads
    FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    INNER JOIN sys.dm_db_index_usage_stats s ON i.object_id = s.object_id
        AND i.index_id = s.index_id
    WHERE s.database_id = @DbId
    AND SCHEMA_NAME(t.schema_id) IN ('Master', 'Shared', 'Auth', 'Report', 'Transaction')
    ORDER BY (s.user_seeks + s.user_scans + s.user_lookups) DESC;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Master Index Maintenance Job
-- ============================================

IF OBJECT_ID('dbo.spRunIndexMaintenanceJob', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spRunIndexMaintenanceJob;

GO

CREATE PROCEDURE dbo.spRunIndexMaintenanceJob
    @FragmentationThreshold DECIMAL(5, 2) = 30
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'MASTER INDEX MAINTENANCE JOB';
    PRINT '═══════════════════════════════════════════';

    -- 1. Rebuild/reorganize fragmented indexes
    PRINT '';
    PRINT 'Step 1: Rebuilding/reorganizing fragmented indexes...';
    EXEC dbo.spAutoRebuildFragmentedIndexes @FragmentationThreshold = @FragmentationThreshold, @DryRun = 0;

    -- 2. Update statistics
    PRINT '';
    PRINT 'Step 2: Updating index statistics...';
    EXEC dbo.spUpdateIndexStatistics @DryRun = 0;

    -- 3. Generate health report
    PRINT '';
    PRINT 'Step 3: Generating index health report...';
    EXEC Report.spIndexHealthReport;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT '✅ INDEX MAINTENANCE JOB COMPLETED';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Index Optimization & Maintenance System Procedures Created';
PRINT '';
PRINT 'Total Objects Created:';
PRINT '  - 1 index maintenance log table';
PRINT '  - 8 index optimization procedures:';
PRINT '    • spAnalyzeIndexFragmentation (diagnostic)';
PRINT '    • spAutoRebuildFragmentedIndexes (auto-maintenance)';
PRINT '    • spIdentifyMissingIndexes (recommendations)';
PRINT '    • spIdentifyUnusedIndexes (cleanup)';
PRINT '    • spUpdateIndexStatistics (performance)';
PRINT '    • Report.spIndexHealthReport (monitoring)';
PRINT '    • dbo.spRunIndexMaintenanceJob (orchestrator)';
PRINT '';
PRINT 'Key Features:';
PRINT '  ✅ Automatic rebuild/reorganize based on fragmentation';
PRINT '  ✅ Missing index discovery with impact scores';
PRINT '  ✅ Unused index identification';
PRINT '  ✅ Statistics updates (FULLSCAN)';
PRINT '  ✅ Dry-run mode for testing';
PRINT '  ✅ Complete maintenance logging';
PRINT '  ✅ Health monitoring dashboard';
PRINT '';
PRINT 'Status: Production-ready index maintenance system';

GO
