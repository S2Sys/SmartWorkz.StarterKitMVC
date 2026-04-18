-- ============================================
-- SmartWorkz v3 System Stored Procedures
-- Purpose: System utilities, metadata, maintenance
-- Database: SQL Server
-- Features: Table/View existence, dependencies, cleanup, index stats
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;
GO

-- ============================================
-- 1. TABLE EXISTS CHECK
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[spTableExists]
    @SchemaName NVARCHAR(128),
    @TableName NVARCHAR(128),
    @Exists BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Exists = CASE
        WHEN EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = @SchemaName
            AND TABLE_NAME = @TableName
            AND TABLE_TYPE = 'BASE TABLE'
        ) THEN 1
        ELSE 0
    END;

    SELECT @Exists AS TableExists;
END;
GO

-- ============================================
-- 2. VIEW EXISTS CHECK
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[spViewExists]
    @SchemaName NVARCHAR(128),
    @ViewName NVARCHAR(128),
    @Exists BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Exists = CASE
        WHEN EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.VIEWS
            WHERE TABLE_SCHEMA = @SchemaName
            AND TABLE_NAME = @ViewName
        ) THEN 1
        ELSE 0
    END;

    SELECT @Exists AS ViewExists;
END;
GO

-- ============================================
-- 3. STORED PROCEDURE EXISTS CHECK
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[spStoredProcedureExists]
    @SchemaName NVARCHAR(128),
    @ProcedureName NVARCHAR(128),
    @Exists BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Exists = CASE
        WHEN EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_SCHEMA = @SchemaName
            AND ROUTINE_NAME = @ProcedureName
            AND ROUTINE_TYPE = 'PROCEDURE'
        ) THEN 1
        ELSE 0
    END;

    SELECT @Exists AS ProcedureExists;
END;
GO

-- ============================================
-- 4. FIND TABLE DEPENDENCIES
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[spFindTableDependencies]
    @SchemaName NVARCHAR(128),
    @TableName NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    PRINT 'Finding dependencies for [' + @SchemaName + '].[' + @TableName + ']...';
    PRINT '';

    -- 1. Foreign Key Dependencies (tables that reference this table)
    PRINT '=== FOREIGN KEY DEPENDENCIES (Tables referencing this table) ===';
    SELECT
        fk.name AS ForeignKeyName,
        SCHEMA_NAME(t2.schema_id) AS ReferencingSchema,
        t2.name AS ReferencingTable,
        c2.name AS ReferencingColumn,
        SCHEMA_NAME(t1.schema_id) AS ReferencedSchema,
        t1.name AS ReferencedTable,
        c1.name AS ReferencedColumn
    FROM sys.foreign_keys fk
    INNER JOIN sys.tables t1 ON fk.referenced_object_id = t1.object_id
    INNER JOIN sys.tables t2 ON fk.parent_object_id = t2.object_id
    INNER JOIN sys.columns c1 ON fk.referenced_object_id = c1.object_id
        AND fk.referenced_column_id = c1.column_id
    INNER JOIN sys.columns c2 ON fk.parent_object_id = c2.object_id
        AND fk.parent_column_id = c2.column_id
    WHERE t1.name = @TableName
    AND SCHEMA_NAME(t1.schema_id) = @SchemaName;

    PRINT '';
    PRINT '=== STORED PROCEDURES REFERENCING THIS TABLE ===';
    SELECT
        SCHEMA_NAME(p.schema_id) AS SchemaName,
        p.name AS ProcedureName,
        OBJECT_DEFINITION(p.object_id) AS Definition
    FROM sys.procedures p
    WHERE OBJECT_DEFINITION(p.object_id) LIKE '%' + @TableName + '%'
    AND SCHEMA_NAME(p.schema_id) != 'sys';

    PRINT '';
    PRINT '=== VIEWS REFERENCING THIS TABLE ===';
    SELECT
        SCHEMA_NAME(v.schema_id) AS SchemaName,
        v.name AS ViewName,
        OBJECT_DEFINITION(v.object_id) AS Definition
    FROM sys.views v
    WHERE OBJECT_DEFINITION(v.object_id) LIKE '%' + @TableName + '%';

    PRINT '';
    PRINT '=== INDEXES ON THIS TABLE ===';
    SELECT
        i.name AS IndexName,
        i.type_desc AS IndexType,
        c.name AS ColumnName,
        ic.is_descending_key AS IsDescending
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id
        AND i.index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id
        AND ic.column_id = c.column_id
    WHERE i.object_id = OBJECT_ID(@SchemaName + '.' + @TableName);

    PRINT '';
    PRINT '=== CONSTRAINTS ON THIS TABLE ===';
    SELECT
        CONSTRAINT_NAME,
        CONSTRAINT_TYPE,
        TABLE_SCHEMA,
        TABLE_NAME,
        COLUMN_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = @SchemaName
    AND TABLE_NAME = @TableName;

END;
GO

-- ============================================
-- 5. CLEAN TABLE WITH DEPENDENCY CLEANUP
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[spCleanTableWithDependencies]
    @SchemaName NVARCHAR(128),
    @TableName NVARCHAR(128),
    @DeleteData BIT = 0,
    @DropForeignKeys BIT = 0,
    @DropIndexes BIT = 0,
    @DryRun BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TableFullName NVARCHAR(256) = '[' + @SchemaName + '].[' + @TableName + ']';

    PRINT '================================================';
    PRINT 'CLEANUP PLAN FOR ' + @TableFullName;
    IF @DryRun = 1 PRINT '(DRY RUN - No changes will be made)';
    PRINT '================================================';
    PRINT '';

    -- Step 1: Drop Foreign Keys
    IF @DropForeignKeys = 1
    BEGIN
        PRINT '--- Dropping Foreign Keys ---';

        DECLARE @FKName NVARCHAR(128);
        DECLARE fk_cursor CURSOR FOR
        SELECT name FROM sys.foreign_keys
        WHERE referenced_object_id = OBJECT_ID(@TableFullName);

        OPEN fk_cursor;
        FETCH NEXT FROM fk_cursor INTO @FKName;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @DropFK NVARCHAR(MAX) = 'ALTER TABLE ' + @TableFullName + ' DROP CONSTRAINT [' + @FKName + '];';
            PRINT 'WOULD EXECUTE: ' + @DropFK;

            IF @DryRun = 0
            BEGIN
                EXEC sp_executesql @DropFK;
                PRINT '✓ Dropped FK: ' + @FKName;
            END

            FETCH NEXT FROM fk_cursor INTO @FKName;
        END

        CLOSE fk_cursor;
        DEALLOCATE fk_cursor;
        PRINT '';
    END

    -- Step 2: Drop Indexes
    IF @DropIndexes = 1
    BEGIN
        PRINT '--- Dropping Indexes ---';

        DECLARE @IndexName NVARCHAR(128);
        DECLARE idx_cursor CURSOR FOR
        SELECT name FROM sys.indexes
        WHERE object_id = OBJECT_ID(@TableFullName)
        AND index_id > 0;  -- Skip clustered index

        OPEN idx_cursor;
        FETCH NEXT FROM idx_cursor INTO @IndexName;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @DropIdx NVARCHAR(MAX) = 'DROP INDEX [' + @IndexName + '] ON ' + @TableFullName + ';';
            PRINT 'WOULD EXECUTE: ' + @DropIdx;

            IF @DryRun = 0
            BEGIN
                EXEC sp_executesql @DropIdx;
                PRINT '✓ Dropped Index: ' + @IndexName;
            END

            FETCH NEXT FROM idx_cursor INTO @IndexName;
        END

        CLOSE idx_cursor;
        DEALLOCATE idx_cursor;
        PRINT '';
    END

    -- Step 3: Delete Data
    IF @DeleteData = 1
    BEGIN
        PRINT '--- Deleting Data ---';

        DECLARE @DeleteSQL NVARCHAR(MAX) = 'DELETE FROM ' + @TableFullName + ';';
        PRINT 'WOULD EXECUTE: ' + @DeleteSQL;

        IF @DryRun = 0
        BEGIN
            DECLARE @RowCount INT;
            EXEC sp_executesql @DeleteSQL;
            SET @RowCount = @@ROWCOUNT;
            PRINT '✓ Deleted ' + CAST(@RowCount AS NVARCHAR(20)) + ' rows';
        END

        PRINT '';
    END

    PRINT '================================================';
    IF @DryRun = 1
        PRINT 'DRY RUN COMPLETE - Execute with @DryRun = 0 to apply changes';
    ELSE
        PRINT 'CLEANUP COMPLETE';
    PRINT '================================================';
END;
GO

-- ============================================
-- 6. INDEX STATISTICS REPORT
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[spIndexStatisticsReport]
    @SchemaName NVARCHAR(128) = NULL,
    @TableName NVARCHAR(128) = NULL,
    @OrderBy NVARCHAR(20) = 'Fragmentation'  -- 'Fragmentation', 'Seeks', 'Scans', 'Size'
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '================================================';
    PRINT 'INDEX STATISTICS REPORT';
    IF @SchemaName IS NOT NULL PRINT 'Schema: ' + @SchemaName;
    IF @TableName IS NOT NULL PRINT 'Table: ' + @TableName;
    PRINT 'Generated: ' + CONVERT(NVARCHAR(30), GETUTCDATE(), 121);
    PRINT '================================================';
    PRINT '';

    SELECT
        SCHEMA_NAME(t.schema_id) AS SchemaName,
        t.name AS TableName,
        i.name AS IndexName,
        i.type_desc AS IndexType,
        ps.in_row_data_page_count AS Pages,
        ps.in_row_data_page_count * 8.0 / 1024 AS SizeMB,
        s.user_seeks AS Seeks,
        s.user_scans AS Scans,
        s.user_lookups AS Lookups,
        s.user_updates AS Updates,
        CAST(d.avg_fragmentation_in_percent AS DECIMAL(10, 2)) AS FragmentationPercent,
        d.page_count AS IndexPageCount,
        CAST(GETUTCDATE() - s.last_user_seek AS NVARCHAR(20)) AS DaysSinceLastSeek,
        CASE
            WHEN d.avg_fragmentation_in_percent < 10 THEN 'Good'
            WHEN d.avg_fragmentation_in_percent < 30 THEN 'Moderate'
            ELSE 'High Fragmentation'
        END AS HealthStatus
    FROM sys.tables t
    INNER JOIN sys.indexes i ON t.object_id = i.object_id
    LEFT JOIN sys.dm_db_index_usage_stats s ON i.object_id = s.object_id
        AND i.index_id = s.index_id
        AND s.database_id = DB_ID()
    LEFT JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') d
        ON i.object_id = d.object_id
        AND i.index_id = d.index_id
    LEFT JOIN sys.dm_db_partition_stats ps ON i.object_id = ps.object_id
        AND i.index_id = ps.index_id
    WHERE i.index_id > 0  -- Exclude heaps
    AND (@SchemaName IS NULL OR SCHEMA_NAME(t.schema_id) = @SchemaName)
    AND (@TableName IS NULL OR t.name = @TableName)
    ORDER BY
        CASE @OrderBy
            WHEN 'Seeks' THEN s.user_seeks
            WHEN 'Scans' THEN s.user_scans
            WHEN 'Size' THEN ps.in_row_data_page_count
            ELSE d.avg_fragmentation_in_percent
        END DESC;

    PRINT '';
    PRINT '================================================';
    PRINT 'LEGEND:';
    PRINT '  Seeks: Index seek operations (good - targeted access)';
    PRINT '  Scans: Index scan operations (full scan)';
    PRINT '  Lookups: RID/Key lookups (may indicate missing index)';
    PRINT '  Fragmentation < 10%: Good';
    PRINT '  Fragmentation 10-30%: Moderate (consider rebuild)';
    PRINT '  Fragmentation > 30%: High (rebuild recommended)';
    PRINT '================================================';
END;
GO

-- ============================================
-- 7. INDEX RECOMMENDATION REPORT
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[spIndexRecommendationReport]
    @MinFragmentation DECIMAL(10, 2) = 10,
    @MinSeeks INT = 100
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '================================================';
    PRINT 'INDEX OPTIMIZATION RECOMMENDATIONS';
    PRINT 'Min Fragmentation: ' + CAST(@MinFragmentation AS NVARCHAR(10)) + '%';
    PRINT 'Min Seeks: ' + CAST(@MinSeeks AS NVARCHAR(10));
    PRINT 'Generated: ' + CONVERT(NVARCHAR(30), GETUTCDATE(), 121);
    PRINT '================================================';
    PRINT '';

    PRINT '--- INDEXES WITH HIGH FRAGMENTATION ---';
    SELECT
        SCHEMA_NAME(t.schema_id) AS SchemaName,
        t.name AS TableName,
        i.name AS IndexName,
        CAST(d.avg_fragmentation_in_percent AS DECIMAL(10, 2)) AS FragmentationPercent,
        CASE
            WHEN d.avg_fragmentation_in_percent > 30 THEN 'REBUILD'
            ELSE 'REORGANIZE'
        END AS Action,
        'ALTER INDEX [' + i.name + '] ON [' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + '] ' +
        CASE
            WHEN d.avg_fragmentation_in_percent > 30 THEN 'REBUILD;'
            ELSE 'REORGANIZE;'
        END AS TSQLCommand
    FROM sys.tables t
    INNER JOIN sys.indexes i ON t.object_id = i.object_id
    INNER JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') d
        ON i.object_id = d.object_id
        AND i.index_id = d.index_id
    WHERE i.index_id > 0
    AND d.avg_fragmentation_in_percent >= @MinFragmentation
    AND d.page_count > 1000  -- Only report significant indexes
    ORDER BY d.avg_fragmentation_in_percent DESC;

    PRINT '';
    PRINT '--- UNUSED INDEXES (Candidates for Removal) ---';
    SELECT
        SCHEMA_NAME(t.schema_id) AS SchemaName,
        t.name AS TableName,
        i.name AS IndexName,
        i.type_desc AS IndexType,
        s.user_updates AS Updates,
        s.user_seeks + s.user_scans + s.user_lookups AS Reads,
        'DROP INDEX [' + i.name + '] ON [' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + '];' AS DropCommand
    FROM sys.tables t
    INNER JOIN sys.indexes i ON t.object_id = i.object_id
    LEFT JOIN sys.dm_db_index_usage_stats s ON i.object_id = s.object_id
        AND i.index_id = s.index_id
        AND s.database_id = DB_ID()
    WHERE i.index_id > 0
    AND i.is_primary_key = 0  -- Don't drop PK
    AND i.is_unique = 0  -- Don't drop unique
    AND (s.user_seeks IS NULL OR (s.user_seeks + s.user_scans + s.user_lookups = 0))
    AND s.user_updates > 0
    ORDER BY s.user_updates DESC;

    PRINT '';
    PRINT '--- MISSING INDEXES (High Impact Opportunity) ---';
    SELECT TOP 20
        CONVERT(DECIMAL(18, 2), migs.user_seeks * migs.avg_total_user_cost * (migs.avg_user_impact * 0.01)) AS Improvement,
        mid.equality_columns,
        mid.included_columns,
        mid.inequality_columns,
        migs.user_seeks,
        migs.avg_user_impact,
        'CREATE INDEX idx_' + REPLACE(REPLACE(REPLACE(mid.equality_columns, ', ', '_'), '[', ''), ']', '') +
        ' ON [' + mid.statement + '] (' + mid.equality_columns + ')' +
        CASE WHEN mid.included_columns IS NOT NULL THEN ' INCLUDE (' + mid.included_columns + ')' ELSE '' END + ';' AS TSQLCommand
    FROM sys.dm_db_missing_index_details mid
    INNER JOIN sys.dm_db_missing_index_groups mig ON mid.index_handle = mig.index_handle
    INNER JOIN sys.dm_db_missing_index_groups_stats migs ON mig.index_group_id = migs.index_group_id
    WHERE database_id = DB_ID()
    ORDER BY Improvement DESC;

    PRINT '';
    PRINT '================================================';
END;
GO

-- ============================================
-- 8. DATABASE OBJECT COUNT REPORT
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[spDatabaseObjectReport]
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '================================================';
    PRINT 'DATABASE OBJECT INVENTORY REPORT';
    PRINT 'Database: ' + DB_NAME();
    PRINT 'Generated: ' + CONVERT(NVARCHAR(30), GETUTCDATE(), 121);
    PRINT '================================================';
    PRINT '';

    PRINT '--- SCHEMAS ---';
    SELECT
        SCHEMA_NAME(schema_id) AS SchemaName,
        COUNT(*) AS ObjectCount
    FROM sys.objects
    GROUP BY schema_id
    ORDER BY SCHEMA_NAME(schema_id);

    PRINT '';
    PRINT '--- OBJECT TYPES ---';
    SELECT
        type_desc AS ObjectType,
        COUNT(*) AS Count
    FROM sys.objects
    WHERE is_ms_shipped = 0
    GROUP BY type_desc
    ORDER BY Count DESC;

    PRINT '';
    PRINT '--- TABLE STATISTICS ---';
    SELECT
        SCHEMA_NAME(t.schema_id) AS SchemaName,
        t.name AS TableName,
        (SELECT COUNT(*) FROM sys.columns WHERE object_id = t.object_id) AS ColumnCount,
        (SELECT COUNT(*) FROM sys.indexes WHERE object_id = t.object_id) AS IndexCount,
        (SELECT COUNT(*) FROM sys.foreign_keys WHERE parent_object_id = t.object_id) AS FKCount,
        CAST(SUM(p.rows) AS BIGINT) AS RowCount
    FROM sys.tables t
    LEFT JOIN sys.partitions p ON t.object_id = p.object_id
    GROUP BY t.schema_id, t.name
    ORDER BY SCHEMA_NAME(t.schema_id), t.name;

    PRINT '';
    PRINT '--- STORED PROCEDURE COUNT ---';
    SELECT
        SCHEMA_NAME(schema_id) AS SchemaName,
        COUNT(*) AS ProcedureCount
    FROM sys.procedures
    WHERE is_ms_shipped = 0
    GROUP BY schema_id
    ORDER BY SCHEMA_NAME(schema_id);

    PRINT '';
    PRINT '--- VIEW COUNT ---';
    SELECT
        SCHEMA_NAME(schema_id) AS SchemaName,
        COUNT(*) AS ViewCount
    FROM sys.views
    WHERE is_ms_shipped = 0
    GROUP BY schema_id
    ORDER BY SCHEMA_NAME(schema_id);

    PRINT '';
    PRINT '================================================';
    PRINT 'SUMMARY';
    SELECT
        COUNT(DISTINCT SCHEMA_NAME(schema_id)) AS TotalSchemas,
        SUM(CASE WHEN type_desc = 'USER_TABLE' THEN 1 ELSE 0 END) AS Tables,
        SUM(CASE WHEN type_desc = 'SQL_STORED_PROCEDURE' THEN 1 ELSE 0 END) AS StoredProcedures,
        SUM(CASE WHEN type_desc = 'VIEW' THEN 1 ELSE 0 END) AS Views,
        SUM(CASE WHEN type_desc = 'SQL_TRIGGER' THEN 1 ELSE 0 END) AS Triggers
    FROM sys.objects
    WHERE is_ms_shipped = 0;

    PRINT '================================================';
END;
GO

-- ============================================
-- 9. STORED PROCEDURE EXECUTION STATISTICS
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[spProcedureExecutionStats]
    @SchemaName NVARCHAR(128) = NULL,
    @OrderBy NVARCHAR(20) = 'ExecutionCount'  -- 'ExecutionCount', 'TotalTime', 'AvgTime'
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '================================================';
    PRINT 'STORED PROCEDURE EXECUTION STATISTICS';
    IF @SchemaName IS NOT NULL PRINT 'Schema: ' + @SchemaName;
    PRINT 'Generated: ' + CONVERT(NVARCHAR(30), GETUTCDATE(), 121);
    PRINT '================================================';
    PRINT '';

    SELECT
        SCHEMA_NAME(p.schema_id) AS SchemaName,
        p.name AS ProcedureName,
        stat.execution_count AS ExecutionCount,
        stat.total_elapsed_time / 1000 AS TotalTimeMs,
        CAST(stat.total_elapsed_time / 1000.0 / NULLIF(stat.execution_count, 0) AS DECIMAL(10, 2)) AS AvgTimeMs,
        stat.max_elapsed_time / 1000 AS MaxTimeMs,
        stat.total_physical_reads AS PhysicalReads,
        stat.total_logical_reads AS LogicalReads,
        stat.total_logical_writes AS LogicalWrites,
        GETDATE() AS LastExecutionTime
    FROM sys.procedures p
    INNER JOIN sys.dm_exec_procedure_stats stat ON p.object_id = stat.object_id
    WHERE stat.database_id = DB_ID()
    AND p.is_ms_shipped = 0
    AND (@SchemaName IS NULL OR SCHEMA_NAME(p.schema_id) = @SchemaName)
    ORDER BY
        CASE @OrderBy
            WHEN 'TotalTime' THEN stat.total_elapsed_time
            WHEN 'AvgTime' THEN stat.total_elapsed_time / NULLIF(stat.execution_count, 0)
            ELSE stat.execution_count
        END DESC;

    PRINT '';
    PRINT '================================================';
END;
GO

-- ============================================
-- 10. SYSTEM HEALTH CHECK
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[spSystemHealthCheck]
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '================================================';
    PRINT 'DATABASE SYSTEM HEALTH CHECK';
    PRINT 'Database: ' + DB_NAME();
    PRINT 'Timestamp: ' + CONVERT(NVARCHAR(30), GETUTCDATE(), 121);
    PRINT '================================================';
    PRINT '';

    PRINT '--- DISK SPACE USAGE ---';
    SELECT
        name AS LogicalFileName,
        file_id AS FileId,
        type_desc AS FileType,
        CAST(size * 8.0 / 1024 AS DECIMAL(10, 2)) AS FileSizeMB,
        CAST((size - FILEPROPERTY(name, 'SpaceUsed')) * 8.0 / 1024 AS DECIMAL(10, 2)) AS FreeSpaceMB,
        CAST(100.0 * FILEPROPERTY(name, 'SpaceUsed') / size AS DECIMAL(10, 2)) AS UsagePercent,
        physical_name AS PhysicalPath
    FROM sys.database_files;

    PRINT '';
    PRINT '--- INTEGRITY CHECK STATUS ---';
    SELECT
        'Last DBCC CHECKDB Run' AS CheckType,
        ISNULL(CAST(MAX(create_date) AS NVARCHAR(30)), 'Never Run') AS LastRun,
        DATEDIFF(DAY, MAX(create_date), GETDATE()) AS DaysSinceLastRun
    FROM msdb.dbo.suspect_pages
    WHERE database_id = DB_ID();

    PRINT '';
    PRINT '--- HIGH FRAGMENTATION INDEXES (>30%) ---';
    SELECT
        SCHEMA_NAME(t.schema_id) AS SchemaName,
        t.name AS TableName,
        i.name AS IndexName,
        CAST(d.avg_fragmentation_in_percent AS DECIMAL(10, 2)) AS FragmentationPercent
    FROM sys.tables t
    INNER JOIN sys.indexes i ON t.object_id = i.object_id
    INNER JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') d
        ON i.object_id = d.object_id
        AND i.index_id = d.index_id
    WHERE i.index_id > 0
    AND d.avg_fragmentation_in_percent > 30
    AND d.page_count > 1000;

    IF @@ROWCOUNT = 0 PRINT '✓ No high fragmentation indexes found';

    PRINT '';
    PRINT '--- MISSING INDEXES (Top 5) ---';
    SELECT TOP 5
        CONVERT(DECIMAL(18, 2), migs.user_seeks * migs.avg_total_user_cost * (migs.avg_user_impact * 0.01)) AS Improvement,
        mid.statement AS TableName,
        mid.equality_columns
    FROM sys.dm_db_missing_index_details mid
    INNER JOIN sys.dm_db_missing_index_groups mig ON mid.index_handle = mig.index_handle
    INNER JOIN sys.dm_db_missing_index_groups_stats migs ON mig.index_group_id = migs.index_group_id
    WHERE database_id = DB_ID()
    ORDER BY Improvement DESC;

    IF @@ROWCOUNT = 0 PRINT '✓ No significant missing indexes detected';

    PRINT '';
    PRINT '--- BLOCKED PROCESSES ---';
    IF NOT EXISTS (
        SELECT 1 FROM sys.sysprocesses WHERE blocked != 0
    )
    BEGIN
        PRINT '✓ No blocked processes detected';
    END
    ELSE
    BEGIN
        SELECT
            spid,
            blocked,
            loginame,
            hostname,
            program_name,
            last_batch
        FROM sys.sysprocesses
        WHERE blocked != 0;
    END

    PRINT '';
    PRINT '================================================';
    PRINT 'HEALTH CHECK COMPLETE';
    PRINT '================================================';
END;
GO

-- ============================================
-- SUMMARY
-- ============================================

PRINT '';
PRINT '✓✓✓ SYSTEM STORED PROCEDURES CREATED ✓✓✓';
PRINT '';
PRINT 'Available System Procedures:';
PRINT '  1. spTableExists - Check if table exists';
PRINT '  2. spViewExists - Check if view exists';
PRINT '  3. spStoredProcedureExists - Check if procedure exists';
PRINT '  4. spFindTableDependencies - Find all dependencies';
PRINT '  5. spCleanTableWithDependencies - Clean table & dependencies';
PRINT '  6. spIndexStatisticsReport - Index health & stats';
PRINT '  7. spIndexRecommendationReport - Optimization recommendations';
PRINT '  8. spDatabaseObjectReport - Complete object inventory';
PRINT '  9. spProcedureExecutionStats - Procedure performance stats';
PRINT ' 10. spSystemHealthCheck - Overall database health';
PRINT '';
PRINT 'Usage Examples:';
PRINT '  EXEC spTableExists @SchemaName=''Master'', @TableName=''Tenants'', @Exists=@Result OUTPUT;';
PRINT '  EXEC spFindTableDependencies @SchemaName=''Master'', @TableName=''Lookup'';';
PRINT '  EXEC spIndexStatisticsReport @SchemaName=''Master'', @OrderBy=''Fragmentation'';';
PRINT '  EXEC spIndexRecommendationReport @MinFragmentation=10, @MinSeeks=100;';
PRINT '  EXEC spSystemHealthCheck;';
PRINT '  EXEC spDatabaseObjectReport;';
PRINT '  EXEC spProcedureExecutionStats @OrderBy=''ExecutionCount'';';
PRINT '';
