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

-- Disable foreign key constraints
EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'

-- Drop all foreign keys
DECLARE @sql NVARCHAR(MAX) = N''
SELECT @sql = @sql + 'ALTER TABLE [' + CONSTRAINT_SCHEMA + '].[' + TABLE_NAME + '] DROP CONSTRAINT [' + CONSTRAINT_NAME + '];'
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE CONSTRAINT_TYPE = 'FOREIGN KEY'
  AND TABLE_SCHEMA IN ('Master', 'Shared', 'Transaction', 'Report', 'Auth')

EXEC sp_executesql @sql

-- Drop all tables
SET @sql = N''
SELECT @sql = @sql + 'DROP TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '];'
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA IN ('Master', 'Shared', 'Transaction', 'Report', 'Auth')
  AND TABLE_TYPE = 'BASE TABLE'

EXEC sp_executesql @sql

-- Drop existing schemas
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Master')
    DROP SCHEMA Master
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Shared')
    DROP SCHEMA Shared
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Transaction')
    DROP SCHEMA [Transaction]
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Report')
    DROP SCHEMA Report
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Auth')
    DROP SCHEMA Auth

-- ============================================
-- Create Schemas
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Master')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Master'
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Shared')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Shared'
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Transaction')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA [Transaction]'
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Report')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Report'
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Auth')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Auth'
END

-- ============================================
-- Enable Features
-- ============================================

-- Enable HIERARCHYID for SQL Server (required for Menu and Category trees)
-- No explicit enable needed - HIERARCHYID is available by default in SQL Server 2008+

PRINT '✓ Database initialization complete'
PRINT '✓ Schemas created: Master, Shared, Transaction, Report, Auth'

