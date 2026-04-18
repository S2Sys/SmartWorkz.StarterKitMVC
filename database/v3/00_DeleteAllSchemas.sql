-- ============================================
-- SmartWorkz v4: Delete All Tables, SPs, and Indexes
-- Date: 2026-03-31
-- WARNING: This script DELETES all tables, stored procedures, and indexes
-- Database itself is preserved
-- ============================================

USE Boilerplate;

PRINT '⚠️  WARNING: Deleting all tables, stored procedures, and indexes...'
PRINT ''

-- ============================================
-- Disable All Foreign Key Constraints
-- ============================================
PRINT '🔒 Disabling all foreign key constraints...'

-- Dynamically disable all FK constraints
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' NOCHECK CONSTRAINT ALL;' + CHAR(10)
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name NOT IN ('sys', 'information_schema')
ORDER BY t.name;

EXEC sp_executesql @sql;
PRINT '  ✓ All constraints disabled'

-- ============================================
-- Drop All Stored Procedures
-- ============================================
PRINT ''
PRINT '🗑️  Dropping all stored procedures...'

DECLARE @procName NVARCHAR(MAX);
DECLARE procCursor CURSOR FOR
    SELECT QUOTENAME(s.name) + '.' + QUOTENAME(p.name)
    FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name NOT IN ('sys', 'information_schema')
    ORDER BY p.name;

OPEN procCursor;
FETCH NEXT FROM procCursor INTO @procName;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('DROP PROCEDURE ' + @procName);
    PRINT '  ✓ Dropped ' + @procName;
    FETCH NEXT FROM procCursor INTO @procName;
END

CLOSE procCursor;
DEALLOCATE procCursor;

-- ============================================
-- Drop All Indexes (except primary keys)
-- ============================================
PRINT ''
PRINT '🗑️  Dropping all indexes (except primary keys)...'

DECLARE @indexName NVARCHAR(MAX);
DECLARE @tableName NVARCHAR(MAX);
DECLARE @schemaName NVARCHAR(MAX);

DECLARE indexCursor CURSOR FOR
    SELECT QUOTENAME(s.name) AS schemaName, QUOTENAME(t.name) AS tableName, QUOTENAME(i.name) AS indexName
    FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name NOT IN ('sys', 'information_schema')
    AND i.name IS NOT NULL
    AND i.type != 1  -- Exclude clustered indexes (primary keys)
    AND i.is_primary_key = 0
    ORDER BY t.name, i.name;

OPEN indexCursor;
FETCH NEXT FROM indexCursor INTO @schemaName, @tableName, @indexName;

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        EXEC('DROP INDEX ' + @indexName + ' ON ' + @schemaName + '.' + @tableName);
        PRINT '  ✓ Dropped index ' + @indexName + ' on ' + @tableName;
    END TRY
    BEGIN CATCH
        PRINT '  ⚠️  Could not drop index ' + @indexName + ' (may already be dropped)';
    END CATCH
    FETCH NEXT FROM indexCursor INTO @schemaName, @tableName, @indexName;
END

CLOSE indexCursor;
DEALLOCATE indexCursor;

-- ============================================
-- Drop All Tables (in reverse dependency order)
-- ============================================
PRINT ''
PRINT '🗑️  Dropping all tables...'

DECLARE @tableName2 NVARCHAR(MAX);
DECLARE @schemaName2 NVARCHAR(MAX);
DECLARE tableDropCursor CURSOR FOR
    SELECT QUOTENAME(s.name) AS schemaName, QUOTENAME(t.name) AS tableName, s.name AS origSchema, t.name AS origTable
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name NOT IN ('sys', 'information_schema')
    ORDER BY t.create_date DESC;  -- Drop in reverse creation order

OPEN tableDropCursor;
FETCH NEXT FROM tableDropCursor INTO @schemaName2, @tableName2, @schemaName, @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        EXEC('DROP TABLE ' + @schemaName2 + '.' + @tableName2);
        PRINT '  ✓ Dropped ' + @schemaName + '.' + @tableName;
    END TRY
    BEGIN CATCH
        PRINT '  ⚠️  Could not drop table ' + @tableName + ' (may have dependencies)';
    END CATCH
    FETCH NEXT FROM tableDropCursor INTO @schemaName2, @tableName2, @schemaName, @tableName;
END

CLOSE tableDropCursor;
DEALLOCATE tableDropCursor;

-- ============================================
-- Re-enable Constraints
-- ============================================
PRINT ''
PRINT '🔓 Re-enabling all foreign key constraints...'

DECLARE @sql2 NVARCHAR(MAX) = '';
SELECT @sql2 += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' CHECK CONSTRAINT ALL;' + CHAR(10)
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name NOT IN ('sys', 'information_schema')
ORDER BY t.name;

EXEC sp_executesql @sql2;
PRINT '  ✓ All constraints re-enabled'

PRINT ''
PRINT '✅ All tables, stored procedures, and indexes removed successfully!'
PRINT ''
PRINT 'Summary:'
PRINT '  • All tables dropped'
PRINT '  • All stored procedures dropped'
PRINT '  • All non-clustered indexes dropped'
PRINT '  • Database preserved and ready for new schema'
PRINT ''
