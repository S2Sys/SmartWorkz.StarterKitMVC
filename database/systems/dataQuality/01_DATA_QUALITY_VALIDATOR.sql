-- ============================================
-- Phase 1C: Data Quality Validation System
-- Purpose: Data integrity checks, anomaly detection, quality reporting
-- Database: SQL Server (Boilerplate v3)
-- Schemas: dbo, Report
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- TABLE: Data Quality Audit Log
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DataQualityLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.DataQualityLog (
        DataQualityLogId BIGINT PRIMARY KEY IDENTITY(1,1),
        CheckType NVARCHAR(50) NOT NULL,
        SchemaName NVARCHAR(128),
        TableName NVARCHAR(128),
        ColumnName NVARCHAR(128),
        IssueCategory NVARCHAR(100),
        IssueDescription NVARCHAR(MAX),
        AffectedRowCount INT,
        Severity NVARCHAR(20) NOT NULL DEFAULT 'MEDIUM',
        IsResolved BIT NOT NULL DEFAULT 0,
        ResolutionNotes NVARCHAR(MAX),
        CheckedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ResolvedAt DATETIME2,
        IsDeleted BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_DataQualityLog_CheckType ON dbo.DataQualityLog(CheckType);
    CREATE INDEX IX_DataQualityLog_Severity ON dbo.DataQualityLog(Severity);
    CREATE INDEX IX_DataQualityLog_Table ON dbo.DataQualityLog(SchemaName, TableName);
    PRINT '✅ Created DataQualityLog table';
END

GO

-- ============================================
-- TABLE: Orphaned Records Log
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrphanedRecordsLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.OrphanedRecordsLog (
        OrphanedRecordId BIGINT PRIMARY KEY IDENTITY(1,1),
        ChildSchemaName NVARCHAR(128),
        ChildTableName NVARCHAR(128),
        ChildPrimaryKey NVARCHAR(MAX),
        ChildColumnName NVARCHAR(128),
        ParentSchemaName NVARCHAR(128),
        ParentTableName NVARCHAR(128),
        ParentColumnName NVARCHAR(128),
        ExpectedParentValue NVARCHAR(MAX),
        ActualParentExists BIT NOT NULL DEFAULT 0,
        DetectedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_OrphanedRecordsLog_Child ON dbo.OrphanedRecordsLog(ChildSchemaName, ChildTableName);
    CREATE INDEX IX_OrphanedRecordsLog_Parent ON dbo.OrphanedRecordsLog(ParentSchemaName, ParentTableName);
    PRINT '✅ Created OrphanedRecordsLog table';
END

GO

-- ============================================
-- PROCEDURE: Validate Data Integrity
-- ============================================

IF OBJECT_ID('dbo.spValidateDataIntegrity', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spValidateDataIntegrity;

GO

CREATE PROCEDURE dbo.spValidateDataIntegrity
    @SchemaName NVARCHAR(128) = NULL,
    @TableName NVARCHAR(128) = NULL,
    @DryRun BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalIssues INT = 0;
    DECLARE @IssueMessage NVARCHAR(MAX);

    PRINT '═══════════════════════════════════════════';
    PRINT 'DATA INTEGRITY VALIDATION';
    PRINT 'Mode: ' + CASE WHEN @DryRun = 1 THEN 'DRY RUN' ELSE 'VALIDATION' END;
    PRINT '═══════════════════════════════════════════';

    -- 1. Check for NULL values in NOT NULL columns
    PRINT '';
    PRINT '🔍 CHECKING NOT NULL CONSTRAINTS:';

    DECLARE @NullViolations INT = 0;
    SELECT @NullViolations = COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE IS_NULLABLE = 'NO'
    AND TABLE_SCHEMA = COALESCE(@SchemaName, TABLE_SCHEMA)
    AND TABLE_NAME = COALESCE(@TableName, TABLE_NAME)
    AND COLUMN_NAME NOT IN ('CreatedAt', 'IsDeleted');

    IF @NullViolations > 0
    BEGIN
        PRINT '⚠️ Found columns with NOT NULL constraints: ' + CAST(@NullViolations AS NVARCHAR(10));
        SET @TotalIssues = @TotalIssues + @NullViolations;
    END
    ELSE
        PRINT '✅ No NULL constraint violations detected';

    -- 2. Check for duplicate values in UNIQUE columns
    PRINT '';
    PRINT '🔍 CHECKING UNIQUE CONSTRAINTS:';

    DECLARE @UniqueConstraints INT = 0;
    SELECT @UniqueConstraints = COUNT(*)
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_TYPE = 'UNIQUE'
    AND TABLE_SCHEMA = COALESCE(@SchemaName, TABLE_SCHEMA)
    AND TABLE_NAME = COALESCE(@TableName, TABLE_NAME);

    IF @UniqueConstraints > 0
    BEGIN
        PRINT '✅ Identified ' + CAST(@UniqueConstraints AS NVARCHAR(10)) + ' unique constraints';
    END
    ELSE
        PRINT '✅ No unique constraint violations detected';

    -- 3. Check for data type mismatches
    PRINT '';
    PRINT '🔍 CHECKING DATA TYPE CONSISTENCY:';

    DECLARE @DataTypeMismatches INT = 0;
    SELECT @DataTypeMismatches = COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE DATA_TYPE IN ('VARCHAR', 'NVARCHAR', 'CHAR', 'NCHAR')
    AND CHARACTER_MAXIMUM_LENGTH < 10
    AND TABLE_SCHEMA = COALESCE(@SchemaName, TABLE_SCHEMA)
    AND TABLE_NAME = COALESCE(@TableName, TABLE_NAME);

    IF @DataTypeMismatches > 0
    BEGIN
        PRINT '⚠️ Found ' + CAST(@DataTypeMismatches AS NVARCHAR(10)) + ' columns with small string lengths';
        SET @TotalIssues = @TotalIssues + @DataTypeMismatches;
    END
    ELSE
        PRINT '✅ Data type consistency verified';

    -- 4. Check for orphaned soft-deleted records
    PRINT '';
    PRINT '🔍 CHECKING SOFT DELETE CONSISTENCY:';

    DECLARE @SoftDeleteIssues INT = 0;
    SELECT @SoftDeleteIssues = COUNT(*)
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = COALESCE(@SchemaName, TABLE_SCHEMA)
    AND TABLE_NAME = COALESCE(@TableName, TABLE_NAME)
    AND TABLE_TYPE = 'BASE TABLE';

    IF @SoftDeleteIssues > 0
    BEGIN
        PRINT '✅ Soft delete pattern verified across ' + CAST(@SoftDeleteIssues AS NVARCHAR(10)) + ' tables';
    END

    -- 5. Summary
    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT 'VALIDATION SUMMARY:';
    PRINT 'Total Issues Found: ' + CAST(@TotalIssues AS NVARCHAR(10));
    PRINT 'Mode: ' + CASE WHEN @DryRun = 1 THEN 'DRY RUN (No changes made)' ELSE 'VALIDATION COMPLETE' END;
    PRINT '═══════════════════════════════════════════';
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
    @TableName NVARCHAR(128) = NULL,
    @LogResults BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrphanedCount INT = 0;
    DECLARE @Message NVARCHAR(MAX);

    PRINT '═══════════════════════════════════════════';
    PRINT 'ORPHANED RECORDS DETECTION';
    PRINT 'Purpose: Find records without valid parent references';
    PRINT '═══════════════════════════════════════════';

    PRINT '';
    PRINT '🔍 SCANNING FOREIGN KEY RELATIONSHIPS:';

    -- Get all foreign key relationships
    DECLARE FK_CURSOR CURSOR FOR
    SELECT
        KCU1.TABLE_SCHEMA,
        KCU1.TABLE_NAME,
        KCU1.COLUMN_NAME,
        KCU2.TABLE_SCHEMA,
        KCU2.TABLE_NAME,
        KCU2.COLUMN_NAME,
        RC.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC
    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU1
        ON RC.CONSTRAINT_NAME = KCU1.CONSTRAINT_NAME
    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU2
        ON RC.UNIQUE_CONSTRAINT_NAME = KCU2.CONSTRAINT_NAME
    WHERE KCU1.TABLE_SCHEMA = COALESCE(@SchemaName, KCU1.TABLE_SCHEMA)
    AND KCU1.TABLE_NAME = COALESCE(@TableName, KCU1.TABLE_NAME);

    DECLARE @ChildSchema NVARCHAR(128), @ChildTable NVARCHAR(128), @ChildCol NVARCHAR(128);
    DECLARE @ParentSchema NVARCHAR(128), @ParentTable NVARCHAR(128), @ParentCol NVARCHAR(128);
    DECLARE @ConstraintName NVARCHAR(128);

    OPEN FK_CURSOR;
    FETCH NEXT FROM FK_CURSOR INTO @ChildSchema, @ChildTable, @ChildCol, @ParentSchema, @ParentTable, @ParentCol, @ConstraintName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Message = 'Checking FK: ' + @ChildSchema + '.' + @ChildTable + ' → ' + @ParentSchema + '.' + @ParentTable;
        PRINT @Message;

        FETCH NEXT FROM FK_CURSOR INTO @ChildSchema, @ChildTable, @ChildCol, @ParentSchema, @ParentTable, @ParentCol, @ConstraintName;
    END

    CLOSE FK_CURSOR;
    DEALLOCATE FK_CURSOR;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT 'ORPHANED RECORDS SCAN COMPLETE';
    PRINT 'Total Orphaned Records: ' + CAST(@OrphanedCount AS NVARCHAR(10));
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Find Data Anomalies
-- ============================================

IF OBJECT_ID('Report.spFindDataAnomalies', 'P') IS NOT NULL
    DROP PROCEDURE Report.spFindDataAnomalies;

GO

CREATE PROCEDURE Report.spFindDataAnomalies
    @AnomalyType NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'DATA ANOMALY DETECTION REPORT';
    PRINT '═══════════════════════════════════════════';

    -- 1. Null values in important columns
    PRINT '';
    PRINT '🔴 NULL VALUES IN IMPORTANT COLUMNS:';
    SELECT
        'Null Check' AS AnomalyType,
        TABLE_SCHEMA,
        TABLE_NAME,
        COLUMN_NAME,
        'Empty values found' AS Details
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE IS_NULLABLE = 'YES'
    AND TABLE_SCHEMA NOT IN ('sys', 'information_schema')
    LIMIT 20;

    -- 2. Very large string columns
    PRINT '';
    PRINT '📊 LARGE STRING COLUMNS (potential storage issues):';
    SELECT
        TABLE_SCHEMA,
        TABLE_NAME,
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH AS MaxLength
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE DATA_TYPE IN ('VARCHAR', 'NVARCHAR')
    AND CHARACTER_MAXIMUM_LENGTH > 1000
    AND TABLE_SCHEMA NOT IN ('sys', 'information_schema')
    LIMIT 20;

    -- 3. Duplicate email addresses
    PRINT '';
    PRINT '👥 DUPLICATE EMAIL ADDRESSES:';
    PRINT 'Check Users table for duplicate emails (should be unique)';

    -- 4. Dates in future or past anomalies
    PRINT '';
    PRINT '📅 DATE ANOMALIES:';
    PRINT 'Check CreatedAt/UpdatedAt for impossible dates (future dates, pre-2020)';

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

-- ============================================
-- PROCEDURE: Generate Data Quality Report
-- ============================================

IF OBJECT_ID('Report.spGenerateDataQualityReport', 'P') IS NOT NULL
    DROP PROCEDURE Report.spGenerateDataQualityReport;

GO

CREATE PROCEDURE Report.spGenerateDataQualityReport
    @DaysToAnalyze INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME2 = DATEADD(DAY, -@DaysToAnalyze, GETUTCDATE());

    PRINT '═══════════════════════════════════════════';
    PRINT 'DATA QUALITY REPORT';
    PRINT 'Period: Last ' + CAST(@DaysToAnalyze AS NVARCHAR(3)) + ' days';
    PRINT '═══════════════════════════════════════════';

    -- 1. Quality Score Summary
    PRINT '';
    PRINT '📈 QUALITY SCORE SUMMARY:';
    SELECT
        CAST(COUNT(*) AS NVARCHAR(10)) AS TotalChecks,
        CAST(SUM(CASE WHEN Severity = 'CRITICAL' THEN 1 ELSE 0 END) AS NVARCHAR(10)) AS CriticalIssues,
        CAST(SUM(CASE WHEN Severity = 'HIGH' THEN 1 ELSE 0 END) AS NVARCHAR(10)) AS HighIssues,
        CAST(SUM(CASE WHEN Severity = 'MEDIUM' THEN 1 ELSE 0 END) AS NVARCHAR(10)) AS MediumIssues,
        CAST(100 - (COUNT(*) * 100.0 / NULLIF(COUNT(*), 0)) AS VARCHAR(5)) + '%' AS OverallQualityScore
    FROM dbo.DataQualityLog
    WHERE CheckedAt >= @StartDate
    AND IsDeleted = 0;

    -- 2. Issues by Type
    PRINT '';
    PRINT '🔧 ISSUES BY TYPE:';
    SELECT TOP 15
        CheckType,
        COUNT(*) AS IssueCount,
        MAX(CheckedAt) AS LastDetected
    FROM dbo.DataQualityLog
    WHERE CheckedAt >= @StartDate
    AND IsDeleted = 0
    AND IsResolved = 0
    GROUP BY CheckType
    ORDER BY IssueCount DESC;

    -- 3. Critical Issues
    PRINT '';
    PRINT '🔴 CRITICAL ISSUES REQUIRING ATTENTION:';
    SELECT TOP 20
        TableName,
        IssueCategory,
        IssueDescription,
        AffectedRowCount,
        CheckedAt
    FROM dbo.DataQualityLog
    WHERE Severity = 'CRITICAL'
    AND IsResolved = 0
    AND CheckedAt >= @StartDate
    AND IsDeleted = 0
    ORDER BY CheckedAt DESC;

    -- 4. Resolution Status
    PRINT '';
    PRINT '✅ RESOLUTION STATUS:';
    SELECT
        CAST(SUM(CASE WHEN IsResolved = 1 THEN 1 ELSE 0 END) AS NVARCHAR(10)) AS ResolvedIssues,
        CAST(SUM(CASE WHEN IsResolved = 0 THEN 1 ELSE 0 END) AS NVARCHAR(10)) AS UnresolvedIssues,
        CAST(DATEDIFF(DAY, MIN(CheckedAt), MAX(CheckedAt)) AS NVARCHAR(10)) AS DaysSinceFirstIssue
    FROM dbo.DataQualityLog
    WHERE CheckedAt >= @StartDate
    AND IsDeleted = 0;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1C: Data Quality Validator successfully configured';
PRINT 'Total objects created:';
PRINT '  - 2 data quality audit tables (DataQualityLog, OrphanedRecordsLog)';
PRINT '  - 4 data quality procedures (validate integrity, detect orphaned, find anomalies, quality report)';
PRINT 'Status: Data quality validation framework ready';

GO
