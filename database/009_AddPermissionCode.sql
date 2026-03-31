-- ============================================
-- SmartWorkz v4: Add Permission Code Column
-- Date: 2026-03-31
-- Adds missing Code column to Auth.Permissions table
-- ============================================

USE Boilerplate;

-- ============================================
-- Add Code column if not exists
-- ============================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Auth'
    AND TABLE_NAME = 'Permissions'
    AND COLUMN_NAME = 'Code'
)
BEGIN
    ALTER TABLE Auth.Permissions
    ADD Code NVARCHAR(256) NULL;

    PRINT '✓ Code column added to Auth.Permissions';
END
ELSE
BEGIN
    PRINT '✓ Code column already exists in Auth.Permissions';
END;

-- ============================================
-- Populate Code column with values (one-time)
-- ============================================
UPDATE Auth.Permissions
SET Code = UPPER(CONCAT(PermissionType, '_', ResourceType))
WHERE Code IS NULL;

PRINT '✓ Permission codes populated';

-- ============================================
-- Add unique constraint on Code
-- ============================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Auth'
    AND TABLE_NAME = 'Permissions'
    AND CONSTRAINT_NAME = 'UQ_Permissions_Code'
)
BEGIN
    ALTER TABLE Auth.Permissions
    ADD CONSTRAINT UQ_Permissions_Code UNIQUE (Code);

    PRINT '✓ Unique constraint added on Code column';
END
ELSE
BEGIN
    PRINT '✓ Unique constraint already exists on Code column';
END;

-- ============================================
-- Make Code column NOT NULL
-- ============================================
ALTER TABLE Auth.Permissions
ALTER COLUMN Code NVARCHAR(256) NOT NULL;

PRINT '✓ Permission Code column is now NOT NULL';
PRINT '✓ Migration completed successfully';
