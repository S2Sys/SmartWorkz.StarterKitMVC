-- ============================================
-- V2: Create LoV Schema
-- ============================================
-- Purpose: Create new LoV schema for v2 consolidated lookups
-- Execution: Run first, before creating tables

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'LoV')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA LoV'
    PRINT 'LoV schema created successfully.'
END
ELSE
BEGIN
    PRINT 'LoV schema already exists.'
END
