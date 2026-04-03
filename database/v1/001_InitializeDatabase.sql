-- ============================================
-- SmartWorkz v4 Phase 1: Database Initialization
-- Date: 2026-03-31
-- Version: 4.0.0
-- ============================================
-- Step 1: Initialize Database and Create Schemas
-- ============================================

-- Create database (if not exists - executed by PowerShell)
-- USE Boilerplate;

-- ============================================
-- Cleanup: Drop Existing Tables and Schemas
-- ============================================

-- Drop all stored procedures first (before dropping schemas)
DECLARE @sql NVARCHAR(MAX) = N''
SELECT @sql = @sql + 'DROP PROCEDURE [' + ROUTINE_SCHEMA + '].[' + ROUTINE_NAME + '];' + CHAR(10)
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
  AND ROUTINE_SCHEMA IN ('Master', 'Shared', 'Transaction', 'Report', 'Auth')

IF LEN(@sql) > 0
BEGIN
    BEGIN TRY
        EXEC sp_executesql @sql
        PRINT '✓ Dropped all stored procedures'
    END TRY
    BEGIN CATCH
        PRINT '⚠️ Warning: Could not drop some stored procedures'
    END CATCH
END

-- Disable foreign key constraints
BEGIN TRY
    EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
END TRY
BEGIN CATCH
    -- Ignore if no tables exist
END CATCH

-- Drop all foreign keys
SET @sql = N''
SELECT @sql = @sql + 'ALTER TABLE [' + CONSTRAINT_SCHEMA + '].[' + TABLE_NAME + '] DROP CONSTRAINT [' + CONSTRAINT_NAME + '];'
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE CONSTRAINT_TYPE = 'FOREIGN KEY'
  AND TABLE_SCHEMA IN ('Master', 'Shared', 'Transaction', 'Report', 'Auth')

IF LEN(@sql) > 0
BEGIN
    BEGIN TRY
        EXEC sp_executesql @sql
        PRINT '✓ Dropped all foreign keys'
    END TRY
    BEGIN CATCH
        PRINT '⚠️ Warning: Could not drop some foreign keys'
    END CATCH
END

-- Drop all tables
SET @sql = N''
SELECT @sql = @sql + 'DROP TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '];'
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA IN ('Master', 'Shared', 'Transaction', 'Report', 'Auth')
  AND TABLE_TYPE = 'BASE TABLE'

IF LEN(@sql) > 0
BEGIN
    BEGIN TRY
        EXEC sp_executesql @sql
        PRINT '✓ Dropped all tables'
    END TRY
    BEGIN CATCH
        PRINT '⚠️ Warning: Could not drop some tables'
    END CATCH
END

-- Drop existing schemas
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Master')
BEGIN
    DROP SCHEMA Master
    PRINT '✓ Dropped schema: Master'
END
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Shared')
BEGIN
    DROP SCHEMA Shared
    PRINT '✓ Dropped schema: Shared'
END
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Transaction')
BEGIN
    DROP SCHEMA [Transaction]
    PRINT '✓ Dropped schema: Transaction'
END
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Report')
BEGIN
    DROP SCHEMA Report
    PRINT '✓ Dropped schema: Report'
END
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Auth')
BEGIN
    DROP SCHEMA Auth
    PRINT '✓ Dropped schema: Auth'
END

-- ============================================
-- Create Schemas
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Master')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Master'
    PRINT '✓ Created schema: Master'
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Shared')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Shared'
    PRINT '✓ Created schema: Shared'
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Transaction')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA [Transaction]'
    PRINT '✓ Created schema: Transaction'
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Report')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Report'
    PRINT '✓ Created schema: Report'
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Auth')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Auth'
    PRINT '✓ Created schema: Auth'
END

-- ============================================
-- Enable Features
-- ============================================

-- Enable HIERARCHYID for SQL Server (required for Menu and Category trees)
-- No explicit enable needed - HIERARCHYID is available by default in SQL Server 2008+

PRINT ''
PRINT '✅ Database initialization complete'
PRINT '✓ Schemas created: Master, Shared, Transaction, Report, Auth'

