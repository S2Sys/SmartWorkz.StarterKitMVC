-- ============================================
-- System Procedure: Database Bottleneck Analyzer
-- Purpose: Comprehensive performance diagnostics & optimization strategy
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Report, dbo
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- TABLE: Bottleneck Analysis Log
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BottleneckAnalysisLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.BottleneckAnalysisLog (
        AnalysisLogId BIGINT PRIMARY KEY IDENTITY(1,1),
        AnalysisType NVARCHAR(50) NOT NULL,
        ObjectType NVARCHAR(50) NOT NULL,
        ObjectName NVARCHAR(256) NOT NULL,
        IssueCategory NVARCHAR(100) NOT NULL,
        IssueSeverity NVARCHAR(20) NOT NULL,
        FindingDetail NVARCHAR(MAX),
        RecommendedStrategy NVARCHAR(MAX),
        AnalyzedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_BottleneckLog_Type ON dbo.BottleneckAnalysisLog(ObjectType, IssueSeverity);
    PRINT '✅ Created BottleneckAnalysisLog table';
END

GO

-- ============================================
-- PROCEDURE: Find Slow Stored Procedures
-- ============================================

IF OBJECT_ID('Report.spFindSlowStoredProcedures', 'P') IS NOT NULL
    DROP PROCEDURE Report.spFindSlowStoredProcedures;

GO

CREATE PROCEDURE Report.spFindSlowStoredProcedures
    @ThresholdMs INT = 1000,
    @TopCount INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT '🐌 SLOW STORED PROCEDURES ANALYSIS';
    PRINT 'Threshold: ' + CAST(@ThresholdMs AS NVARCHAR(10)) + 'ms';
    PRINT '═══════════════════════════════════════════';
    PRINT '';

    SELECT TOP @TopCount
        OBJECT_SCHEMA_NAME(ps.object_id) AS SchemaName,
        OBJECT_NAME(ps.object_id) AS ProcedureName,
        ps.execution_count AS ExecutionCount,
        CAST(ps.total_elapsed_time / 1000.0 / ps.execution_count AS DECIMAL(10, 2)) AS AvgElapsedMs,
        CAST(ps.total_elapsed_time / 1000.0 AS DECIMAL(10, 2)) AS TotalElapsedSeconds,
        ps.min_elapsed_time / 1000.0 AS MinElapsedMs,
        ps.max_elapsed_time / 1000.0 AS MaxElapsedMs,
        ps.last_execution_time AS LastExecuted,
        CASE
            WHEN ps.total_elapsed_time / 1000.0 / ps.execution_count > @ThresholdMs THEN '🔴 CRITICAL'
            WHEN ps.total_elapsed_time / 1000.0 / ps.execution_count > @ThresholdMs / 2 THEN '🟠 HIGH'
            ELSE '🟡 MEDIUM'
        END AS Severity
    FROM sys.dm_exec_procedure_stats ps
    WHERE database_id = DB_ID()
    AND ps.total_elapsed_time / 1000.0 / ps.execution_count > @ThresholdMs / 2
    ORDER BY ps.total_elapsed_time DESC;

    PRINT '';
    PRINT '💡 OPTIMIZATION STRATEGIES:';
    PRINT '   • Add missing indexes (check spIdentifyMissingIndexes)';
    PRINT '   • Analyze execution plan for table scans';
    PRINT '   • Consider query rewrite/splitting';
    PRINT '   • Add INCLUDE columns to indexes';
    PRINT '   • Use columnstore for analytical queries';
    PRINT '';
END;

GO

-- ============================================
-- PROCEDURE: Find Slow Views
-- ============================================

IF OBJECT_ID('Report.spFindSlowViews', 'P') IS NOT NULL
    DROP PROCEDURE Report.spFindSlowViews;

GO

CREATE PROCEDURE Report.spFindSlowViews
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbId INT = DB_ID();

    PRINT '═══════════════════════════════════════════';
    PRINT '🐢 VIEWS WITH MISSING INDEXES';
    PRINT '═══════════════════════════════════════════';
    PRINT '';

    -- Views that reference tables with missing indexes
    SELECT DISTINCT
        SCHEMA_NAME(v.schema_id) AS ViewSchema,
        v.name AS ViewName,
        COUNT(DISTINCT c.object_id) AS TablesReferenced,
        COUNT(DISTINCT mid.index_handle) AS MissingIndexes
    FROM sys.views v
    INNER JOIN sys.sql_expression_dependencies sed ON v.object_id = sed.referencing_id
    INNER JOIN sys.columns c ON sed.referenced_id = c.object_id
    LEFT JOIN sys.dm_db_missing_index_details mid ON c.object_id = mid.object_id
    WHERE v.schema_id = SCHEMA_ID('Master') OR v.schema_id = SCHEMA_ID('Shared')
    GROUP BY v.schema_id, v.name
    HAVING COUNT(DISTINCT mid.index_handle) > 0
    ORDER BY MissingIndexes DESC;

    PRINT '';
    PRINT '💡 VIEW OPTIMIZATION STRATEGIES:';
    PRINT '   • Materialize high-traffic views with indexed view';
    PRINT '   • Add indexes to underlying tables';
    PRINT '   • Cache view results if data is static';
    PRINT '   • Consider columnstore index for aggregations';
    PRINT '   • Simplify view logic (reduce joins)';
    PRINT '';
END;

GO

-- ============================================
-- PROCEDURE: Find Unused Tables
-- ============================================

IF OBJECT_ID('Report.spFindUnusedTables', 'P') IS NOT NULL
    DROP PROCEDURE Report.spFindUnusedTables;

GO

CREATE PROCEDURE Report.spFindUnusedTables
    @MinDaysOld INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbId INT = DB_ID();
    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@MinDaysOld, GETUTCDATE());

    PRINT '═══════════════════════════════════════════';
    PRINT '📭 UNUSED TABLES ANALYSIS';
    PRINT 'Unused for: ' + CAST(@MinDaysOld AS NVARCHAR(3)) + ' days';
    PRINT '═══════════════════════════════════════════';
    PRINT '';

    SELECT
        SCHEMA_NAME(t.schema_id) AS SchemaName,
        t.name AS TableName,
        ps.row_count AS RowCount,
        CAST(ps.reserved_page_count * 8.0 / 1024 AS DECIMAL(10, 2)) AS SizeMB,
        ISNULL(s.user_seeks, 0) AS UserSeeks,
        ISNULL(s.user_scans, 0) AS UserScans,
        ISNULL(s.user_lookups, 0) AS UserLookups,
        ISNULL(s.user_updates, 0) AS UserUpdates,
        s.last_user_seek AS LastSeek,
        s.last_user_scan AS LastScan,
        DATEDIFF(DAY, ISNULL(s.last_user_seek, '1900-01-01'), GETUTCDATE()) AS DaysSinceLastRead,
        'DROP TABLE ' + SCHEMA_NAME(t.schema_id) + '.[' + t.name + '];' AS DropStatement
    FROM sys.tables t
    INNER JOIN sys.dm_db_partition_stats ps ON t.object_id = ps.object_id
    LEFT JOIN sys.dm_db_index_usage_stats s ON t.object_id = s.object_id
        AND s.database_id = @DbId
    WHERE SCHEMA_NAME(t.schema_id) IN ('Master', 'Shared', 'Auth', 'Report', 'Transaction')
    AND ps.row_count > 0
    AND (s.last_user_seek < @CutoffDate OR s.last_user_seek IS NULL)
    AND (s.last_user_scan < @CutoffDate OR s.last_user_scan IS NULL)
    AND (s.last_user_lookup < @CutoffDate OR s.last_user_lookup IS NULL)
    ORDER BY ps.reserved_page_count DESC;

    PRINT '';
    PRINT '⚠️ CAUTION: Verify before dropping!';
    PRINT '💡 UNUSED TABLE STRATEGY:';
    PRINT '   • Archive old data (if needed for compliance)';
    PRINT '   • Export to separate archive database';
    PRINT '   • Drop after 90+ days of no access';
    PRINT '   • Reclaim storage space';
    PRINT '';
END;

GO

-- ============================================
-- PROCEDURE: Analyze Column Usage & Storage Strategy
-- ============================================

IF OBJECT_ID('Report.spAnalyzeColumnUsageStrategy', 'P') IS NOT NULL
    DROP PROCEDURE Report.spAnalyzeColumnUsageStrategy;

GO

CREATE PROCEDURE Report.spAnalyzeColumnUsageStrategy
    @TableName NVARCHAR(128),
    @SchemaName NVARCHAR(128) = 'Master'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ObjectId INT = OBJECT_ID(@SchemaName + '.' + @TableName);
    DECLARE @TotalRows INT;
    DECLARE @TableSizeMB DECIMAL(10, 2);

    IF @ObjectId IS NULL
    BEGIN
        PRINT '❌ Table not found: ' + @SchemaName + '.' + @TableName;
        RETURN;
    END

    SELECT @TotalRows = SUM(row_count)
    FROM sys.dm_db_partition_stats
    WHERE object_id = @ObjectId;

    SELECT @TableSizeMB = CAST(SUM(reserved_page_count) * 8.0 / 1024 AS DECIMAL(10, 2))
    FROM sys.dm_db_partition_stats
    WHERE object_id = @ObjectId;

    PRINT '═══════════════════════════════════════════';
    PRINT '📊 COLUMN USAGE & STORAGE STRATEGY';
    PRINT 'Table: ' + @SchemaName + '.' + @TableName;
    PRINT 'Rows: ' + CAST(@TotalRows AS NVARCHAR(15));
    PRINT 'Size: ' + CAST(@TableSizeMB AS NVARCHAR(10)) + 'MB';
    PRINT '═══════════════════════════════════════════';
    PRINT '';

    -- Column analysis
    PRINT '📋 COLUMN DETAILS:';
    SELECT
        COLUMN_ID,
        name AS ColumnName,
        TYPE_NAME(user_type_id) AS DataType,
        max_length AS MaxLength,
        precision,
        scale,
        is_nullable,
        CASE
            WHEN TYPE_NAME(user_type_id) IN ('int', 'bigint', 'float') THEN '✅ Good for columnstore'
            WHEN TYPE_NAME(user_type_id) IN ('nvarchar', 'varchar') AND max_length > 100 THEN '⚠️ Large text'
            WHEN TYPE_NAME(user_type_id) = 'text' THEN '🔴 Deprecated text type'
            ELSE '✅ Standard type'
        END AS DataTypeAssessment
    FROM sys.columns
    WHERE object_id = @ObjectId
    ORDER BY column_id;

    PRINT '';
    PRINT '💡 STORAGE STRATEGY RECOMMENDATIONS:';

    IF @TotalRows > 1000000
    BEGIN
        PRINT '';
        PRINT '🔸 ROWSTORE (Current):';
        PRINT '   • Good for OLTP (frequent inserts/updates)';
        PRINT '   • Supports all data types';
        PRINT '   • Row-level locking';
        PRINT '';
        PRINT '🔷 COLUMNSTORE RECOMMENDATION:';
        PRINT '   • Consider if: Analytical queries, large scans, compression needed';
        PRINT '   • Type: Nonclustered columnstore on rowstore';
        PRINT '   • Better for: Aggregations, GROUP BY, SUM/AVG queries';
        PRINT '   • Example: CREATE NONCLUSTERED COLUMNSTORE INDEX IX_' + @TableName + '_CS';
        PRINT '              ON ' + @SchemaName + '.[' + @TableName + '] (column list);';
    END
    ELSE
    BEGIN
        PRINT '';
        PRINT '✅ Table size acceptable for rowstore';
        PRINT '📌 Consider columnstore only for:';
        PRINT '   • Analytics/reporting queries';
        PRINT '   • Queries with aggregations (SUM, AVG, COUNT)';
        PRINT '   • Large table scans (>100M rows)';
    END

    PRINT '';
END;

GO

-- ============================================
-- PROCEDURE: Analyze Index Strategy by Type
-- ============================================

IF OBJECT_ID('Report.spAnalyzeIndexStrategy', 'P') IS NOT NULL
    DROP PROCEDURE Report.spAnalyzeIndexStrategy;

GO

CREATE PROCEDURE Report.spAnalyzeIndexStrategy
    @TableName NVARCHAR(128),
    @SchemaName NVARCHAR(128) = 'Master'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ObjectId INT = OBJECT_ID(@SchemaName + '.' + @TableName);
    DECLARE @DbId INT = DB_ID();

    IF @ObjectId IS NULL
    BEGIN
        PRINT '❌ Table not found: ' + @SchemaName + '.' + @TableName;
        RETURN;
    END

    PRINT '═══════════════════════════════════════════';
    PRINT '🔑 INDEX STRATEGY ANALYSIS';
    PRINT 'Table: ' + @SchemaName + '.' + @TableName;
    PRINT '═══════════════════════════════════════════';
    PRINT '';

    -- Current indexes
    PRINT '📊 CURRENT INDEXES:';
    SELECT
        i.name AS IndexName,
        i.type_desc AS IndexType,
        STRING_AGG(c.name, ', ') AS Columns,
        ps.avg_fragmentation_in_percent AS Fragmentation,
        s.user_seeks + s.user_scans + s.user_lookups AS TotalReads,
        s.user_updates AS Updates,
        CASE
            WHEN i.type = 1 THEN 'Clustered'
            WHEN i.type = 2 THEN 'Nonclustered (B-tree)'
            WHEN i.type = 5 THEN 'Clustered Columnstore'
            WHEN i.type = 6 THEN 'Nonclustered Columnstore'
            ELSE 'Other'
        END AS IndexTypeDetail
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    LEFT JOIN sys.dm_db_index_physical_stats(@DbId, @ObjectId, NULL, NULL, 'LIMITED') ps
        ON i.object_id = ps.object_id AND i.index_id = ps.index_id
    LEFT JOIN sys.dm_db_index_usage_stats s ON i.object_id = s.object_id
        AND i.index_id = s.index_id AND s.database_id = @DbId
    WHERE i.object_id = @ObjectId
    AND i.type > 0
    GROUP BY i.name, i.type_desc, i.type, ps.avg_fragmentation_in_percent,
             s.user_seeks, s.user_scans, s.user_lookups, s.user_updates
    ORDER BY i.type, i.name;

    PRINT '';
    PRINT '💡 INDEX TYPE RECOMMENDATIONS:';
    PRINT '';
    PRINT 'CLUSTERED INDEX (B-tree):';
    PRINT '  ✅ Use for: Primary key, frequently queried columns, range queries';
    PRINT '  ✅ Typical: ORDER BY column, WHERE filters, JOIN conditions';
    PRINT '';
    PRINT 'NONCLUSTERED INDEX (B-tree):';
    PRINT '  ✅ Use for: Supporting additional search paths, covering queries';
    PRINT '  ✅ Typical: Covering index with INCLUDE, filtered indexes';
    PRINT '';
    PRINT 'CLUSTERED COLUMNSTORE:';
    PRINT '  ✅ Use for: Read-only analytics, batch inserts, compression';
    PRINT '  ✅ Typical: Data warehouse, historical data, aggregation tables';
    PRINT '';
    PRINT 'NONCLUSTERED COLUMNSTORE:';
    PRINT '  ✅ Use for: Hybrid (OLTP + analytics), selective columns';
    PRINT '  ✅ Typical: Large fact tables with analytical queries';
    PRINT '';
    PRINT 'HASH INDEX (Memory-optimized):';
    PRINT '  ✅ Use for: High-frequency exact match lookups, In-Memory OLTP';
    PRINT '  ✅ Typical: Session state, real-time counters';
    PRINT '';

    -- Missing indexes for this table
    PRINT '⚠️ MISSING INDEXES FOR THIS TABLE:';
    SELECT TOP 10
        mid.equality_columns,
        mid.included_columns,
        migs.user_seeks,
        migs.avg_user_impact,
        'CREATE NONCLUSTERED INDEX IX_' + REPLACE(mid.equality_columns, ', ', '_') +
        ' ON ' + @SchemaName + '.[' + @TableName + '] (' + mid.equality_columns +
        CASE WHEN mid.included_columns IS NOT NULL THEN ') INCLUDE (' + mid.included_columns ELSE '' END +
        ');' AS CreateStatement
    FROM sys.dm_db_missing_index_details mid
    INNER JOIN sys.dm_db_missing_index_groups mig ON mid.index_handle = mig.index_handle
    INNER JOIN sys.dm_db_missing_index_groups_stats migs ON mig.index_group_id = migs.index_group_id
    WHERE mid.object_id = @ObjectId
    ORDER BY migs.user_seeks * migs.avg_total_user_cost * (migs.avg_user_impact * 0.01) DESC;

    PRINT '';
END;

GO

-- ============================================
-- PROCEDURE: Comprehensive Bottleneck Report
-- ============================================

IF OBJECT_ID('Report.spComprehensiveBottleneckAnalysis', 'P') IS NOT NULL
    DROP PROCEDURE Report.spComprehensiveBottleneckAnalysis;

GO

CREATE PROCEDURE Report.spComprehensiveBottleneckAnalysis
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '';
    PRINT '╔════════════════════════════════════════════════════════════════╗';
    PRINT '║        COMPREHENSIVE DATABASE BOTTLENECK ANALYSIS              ║';
    PRINT '║                   SmartWorkz v3 Database                       ║';
    PRINT '║                     ' + CONVERT(NVARCHAR(19), GETUTCDATE(), 121) + '                         ║';
    PRINT '╚════════════════════════════════════════════════════════════════╝';
    PRINT '';

    -- 1. Slow Procedures
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    EXEC Report.spFindSlowStoredProcedures @ThresholdMs = 500, @TopCount = 10;

    -- 2. Slow Views
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    EXEC Report.spFindSlowViews;

    -- 3. Unused Tables
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    EXEC Report.spFindUnusedTables @MinDaysOld = 30;

    -- 4. Index Fragmentation
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT '🔧 INDEX FRAGMENTATION SUMMARY:';
    PRINT '';
    EXEC Report.spIndexHealthReport;

    -- 5. Wait Statistics
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT '⏱️ TOP WAIT TYPES:';
    PRINT '';
    SELECT TOP 5
        wait_type,
        waiting_tasks_count,
        wait_time_ms,
        CAST(wait_time_ms * 100.0 / SUM(wait_time_ms) OVER() AS DECIMAL(5, 2)) AS WaitTimePercent
    FROM sys.dm_os_wait_stats
    WHERE wait_type NOT LIKE '%SLEEP%'
    AND wait_type NOT LIKE '%QUEUE%'
    ORDER BY wait_time_ms DESC;

    -- 6. Memory Usage
    PRINT '';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT '💾 MEMORY & RESOURCE USAGE:';
    PRINT '';
    SELECT
        (SELECT SUM(pages_kb) FROM sys.dm_os_memory_clerks) / 1024.0 AS TotalMemoryMB,
        (SELECT SUM(pages_kb) FROM sys.dm_os_memory_clerks WHERE type LIKE '%BUFFER%') / 1024.0 AS BufferPoolMB,
        (SELECT SUM(granted_memory_kb) FROM sys.dm_exec_query_memory_grants) / 1024.0 AS QueryMemoryMB;

    PRINT '';
    PRINT '═══════════════════════════════════════════════════════════════';
    PRINT '✅ ANALYSIS COMPLETE';
    PRINT '═══════════════════════════════════════════════════════════════';
    PRINT '';
    PRINT 'NEXT STEPS:';
    PRINT '1. Review slow stored procedures - check execution plans';
    PRINT '2. Run: EXEC Report.spAnalyzeColumnUsageStrategy @TableName=''<table>''';
    PRINT '3. Run: EXEC Report.spAnalyzeIndexStrategy @TableName=''<table>''';
    PRINT '4. Implement missing indexes (review impact scores)';
    PRINT '5. Archive unused tables (after verification)';
    PRINT '';

END;

GO

-- ============================================
-- PROCEDURE: Strategy Recommendation Engine
-- ============================================

IF OBJECT_ID('Report.spOptimizationStrategyRecommendations', 'P') IS NOT NULL
    DROP PROCEDURE Report.spOptimizationStrategyRecommendations;

GO

CREATE PROCEDURE Report.spOptimizationStrategyRecommendations
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbId INT = DB_ID();

    PRINT '═══════════════════════════════════════════════════════════════';
    PRINT '🎯 OPTIMIZATION STRATEGY RECOMMENDATIONS';
    PRINT '═══════════════════════════════════════════════════════════════';
    PRINT '';

    PRINT '1️⃣ IMMEDIATE PRIORITIES (Day 1):';
    PRINT '   ✓ Run index maintenance: spRunIndexMaintenanceJob';
    PRINT '   ✓ Update statistics: spUpdateIndexStatistics';
    PRINT '   ✓ Analyze missing indexes: spIdentifyMissingIndexes';
    PRINT '';

    PRINT '2️⃣ QUICK WINS (This Week):';
    PRINT '   ✓ Create top 5 missing indexes (highest impact)';
    PRINT '   ✓ Archive/drop unused tables';
    PRINT '   ✓ Add INCLUDE columns to cover queries';
    PRINT '   ✓ Review slow procedure execution plans';
    PRINT '';

    PRINT '3️⃣ MEDIUM-TERM (This Month):';
    PRINT '   ✓ Implement columnstore indexes for analytics tables';
    PRINT '   ✓ Partition large tables (>100M rows)';
    PRINT '   ✓ Optimize queries with table hints';
    PRINT '   ✓ Set up query store for monitoring';
    PRINT '';

    PRINT '4️⃣ LONG-TERM (Ongoing):';
    PRINT '   ✓ Schedule weekly index maintenance';
    PRINT '   ✓ Monitor wait statistics monthly';
    PRINT '   ✓ Review query execution plans quarterly';
    PRINT '   ✓ Archive old data annually';
    PRINT '';

    PRINT '📊 INDEX TYPE DECISION MATRIX:';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT 'Workload Type     | Primary Index        | Secondary Indexes';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT 'OLTP (Frequent    | Clustered B-tree     | Nonclustered B-tree';
    PRINT 'inserts/updates)  | on PK column         | on FK/search columns';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT 'Analytics/Reporting| Clustered            | Nonclustered';
    PRINT '(Aggregations)    | Columnstore          | Columnstore';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT 'Mixed (OLTP +     | Clustered B-tree     | NC B-tree + NC';
    PRINT 'Analytics)        | (update friendly)    | Columnstore';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT '';

    PRINT '💾 STORAGE TYPE RECOMMENDATIONS:';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT '';
    PRINT 'ROWSTORE (B-tree) ✅ Best for:';
    PRINT '  • Frequent inserts/updates (OLTP)';
    PRINT '  • Point lookups';
    PRINT '  • All data types supported';
    PRINT '  • Row-level locking';
    PRINT '';
    PRINT 'COLUMNSTORE ✅ Best for:';
    PRINT '  • Batch inserts (data warehousing)';
    PRINT '  • Aggregations (SUM, AVG, COUNT)';
    PRINT '  • Table scans (large datasets)';
    PRINT '  • Compression (save 10-20x space)';
    PRINT '';

    PRINT '═══════════════════════════════════════════════════════════════';

END;

GO

PRINT '✅ Database Bottleneck Analyzer System Created';
PRINT '';
PRINT 'Total Objects Created:';
PRINT '  - 1 Analysis log table';
PRINT '  - 6 Diagnostic procedures:';
PRINT '    • spFindSlowStoredProcedures';
PRINT '    • spFindSlowViews';
PRINT '    • spFindUnusedTables';
PRINT '    • spAnalyzeColumnUsageStrategy';
PRINT '    • spAnalyzeIndexStrategy';
PRINT '    • spComprehensiveBottleneckAnalysis (master)';
PRINT '    • spOptimizationStrategyRecommendations';
PRINT '';
PRINT 'Status: Production-ready bottleneck detection system';

GO
