-- ============================================
-- SmartWorkz v4: Delete All Schemas and Objects
-- Date: 2026-03-31
-- WARNING: This script DELETES ALL TABLES AND SCHEMAS!
-- This script runs FIRST to clean up before deployment
-- ============================================

USE master;

-- ============================================
-- Drop Database if Exists (Clean slate)
-- ============================================
IF DB_ID('Boilerplate') IS NOT NULL
BEGIN
    ALTER DATABASE [Boilerplate] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [Boilerplate];
    PRINT '✓ Dropped existing Boilerplate database'
END
ELSE
BEGIN
    PRINT 'ℹ️  Boilerplate database does not exist (clean start)'
END

PRINT ''
PRINT '✅ All schemas and objects removed successfully!'
PRINT ''
