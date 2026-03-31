-- ============================================
-- SmartWorkz v4: Drop All Tables
-- Date: 2026-03-31
-- WARNING: This script DELETES ALL TABLES!
-- ============================================

USE Boilerplate;

PRINT '⚠️  WARNING: Dropping all tables from database...'
PRINT ''

-- ============================================
-- Disable All Foreign Key Constraints
-- ============================================
ALTER TABLE Shared.SeoMeta NOCHECK CONSTRAINT ALL;
ALTER TABLE Shared.Tags NOCHECK CONSTRAINT ALL;
ALTER TABLE Shared.Translations NOCHECK CONSTRAINT ALL;
ALTER TABLE Shared.Notifications NOCHECK CONSTRAINT ALL;
ALTER TABLE Shared.AuditLogs NOCHECK CONSTRAINT ALL;
ALTER TABLE Shared.FileStorage NOCHECK CONSTRAINT ALL;
ALTER TABLE Shared.EmailQueue NOCHECK CONSTRAINT ALL;

ALTER TABLE Master.MenuItems NOCHECK CONSTRAINT ALL;
ALTER TABLE Master.Categories NOCHECK CONSTRAINT ALL;
ALTER TABLE Master.GeolocationPages NOCHECK CONSTRAINT ALL;
ALTER TABLE Master.CustomPages NOCHECK CONSTRAINT ALL;
ALTER TABLE Master.BlogPosts NOCHECK CONSTRAINT ALL;
ALTER TABLE Master.TenantUsers NOCHECK CONSTRAINT ALL;
ALTER TABLE Master.GeoHierarchy NOCHECK CONSTRAINT ALL;

ALTER TABLE Auth.UserRoles NOCHECK CONSTRAINT ALL;
ALTER TABLE Auth.RolePermissions NOCHECK CONSTRAINT ALL;
ALTER TABLE Auth.UserPermissions NOCHECK CONSTRAINT ALL;
ALTER TABLE Auth.RefreshTokens NOCHECK CONSTRAINT ALL;
ALTER TABLE Auth.PasswordResetTokens NOCHECK CONSTRAINT ALL;
ALTER TABLE Auth.EmailVerificationTokens NOCHECK CONSTRAINT ALL;

-- ============================================
-- Drop All Tables (in reverse dependency order)
-- ============================================

-- Shared Schema Tables
PRINT '🗑️  Dropping Shared schema tables...'

IF OBJECT_ID('Shared.SeoMeta', 'U') IS NOT NULL
BEGIN
    DROP TABLE Shared.SeoMeta;
    PRINT '  ✓ Dropped Shared.SeoMeta'
END

IF OBJECT_ID('Shared.Tags', 'U') IS NOT NULL
BEGIN
    DROP TABLE Shared.Tags;
    PRINT '  ✓ Dropped Shared.Tags'
END

IF OBJECT_ID('Shared.Translations', 'U') IS NOT NULL
BEGIN
    DROP TABLE Shared.Translations;
    PRINT '  ✓ Dropped Shared.Translations'
END

IF OBJECT_ID('Shared.Notifications', 'U') IS NOT NULL
BEGIN
    DROP TABLE Shared.Notifications;
    PRINT '  ✓ Dropped Shared.Notifications'
END

IF OBJECT_ID('Shared.AuditLogs', 'U') IS NOT NULL
BEGIN
    DROP TABLE Shared.AuditLogs;
    PRINT '  ✓ Dropped Shared.AuditLogs'
END

IF OBJECT_ID('Shared.FileStorage', 'U') IS NOT NULL
BEGIN
    DROP TABLE Shared.FileStorage;
    PRINT '  ✓ Dropped Shared.FileStorage'
END

IF OBJECT_ID('Shared.EmailQueue', 'U') IS NOT NULL
BEGIN
    DROP TABLE Shared.EmailQueue;
    PRINT '  ✓ Dropped Shared.EmailQueue'
END

-- Auth Schema Tables
PRINT ''
PRINT '🗑️  Dropping Auth schema tables...'

IF OBJECT_ID('Auth.UserPermissions', 'U') IS NOT NULL
BEGIN
    DROP TABLE Auth.UserPermissions;
    PRINT '  ✓ Dropped Auth.UserPermissions'
END

IF OBJECT_ID('Auth.RolePermissions', 'U') IS NOT NULL
BEGIN
    DROP TABLE Auth.RolePermissions;
    PRINT '  ✓ Dropped Auth.RolePermissions'
END

IF OBJECT_ID('Auth.UserRoles', 'U') IS NOT NULL
BEGIN
    DROP TABLE Auth.UserRoles;
    PRINT '  ✓ Dropped Auth.UserRoles'
END

IF OBJECT_ID('Auth.RefreshTokens', 'U') IS NOT NULL
BEGIN
    DROP TABLE Auth.RefreshTokens;
    PRINT '  ✓ Dropped Auth.RefreshTokens'
END

IF OBJECT_ID('Auth.PasswordResetTokens', 'U') IS NOT NULL
BEGIN
    DROP TABLE Auth.PasswordResetTokens;
    PRINT '  ✓ Dropped Auth.PasswordResetTokens'
END

IF OBJECT_ID('Auth.EmailVerificationTokens', 'U') IS NOT NULL
BEGIN
    DROP TABLE Auth.EmailVerificationTokens;
    PRINT '  ✓ Dropped Auth.EmailVerificationTokens'
END

IF OBJECT_ID('Auth.Permissions', 'U') IS NOT NULL
BEGIN
    DROP TABLE Auth.Permissions;
    PRINT '  ✓ Dropped Auth.Permissions'
END

IF OBJECT_ID('Auth.Users', 'U') IS NOT NULL
BEGIN
    DROP TABLE Auth.Users;
    PRINT '  ✓ Dropped Auth.Users'
END

IF OBJECT_ID('Auth.UserRoles', 'U') IS NOT NULL
BEGIN
    DROP TABLE Auth.UserRoles;
    PRINT '  ✓ Dropped Auth.UserRoles (second check)'
END

IF OBJECT_ID('Auth.Roles', 'U') IS NOT NULL
BEGIN
    DROP TABLE Auth.Roles;
    PRINT '  ✓ Dropped Auth.Roles'
END

-- Master Schema Tables
PRINT ''
PRINT '🗑️  Dropping Master schema tables...'

IF OBJECT_ID('Master.TenantUsers', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.TenantUsers;
    PRINT '  ✓ Dropped Master.TenantUsers'
END

IF OBJECT_ID('Master.GeolocationPages', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.GeolocationPages;
    PRINT '  ✓ Dropped Master.GeolocationPages'
END

IF OBJECT_ID('Master.CustomPages', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.CustomPages;
    PRINT '  ✓ Dropped Master.CustomPages'
END

IF OBJECT_ID('Master.BlogPosts', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.BlogPosts;
    PRINT '  ✓ Dropped Master.BlogPosts'
END

IF OBJECT_ID('Master.MenuItems', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.MenuItems;
    PRINT '  ✓ Dropped Master.MenuItems'
END

IF OBJECT_ID('Master.Menus', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.Menus;
    PRINT '  ✓ Dropped Master.Menus'
END

IF OBJECT_ID('Master.Categories', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.Categories;
    PRINT '  ✓ Dropped Master.Categories'
END

IF OBJECT_ID('Master.GeoHierarchy', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.GeoHierarchy;
    PRINT '  ✓ Dropped Master.GeoHierarchy'
END

IF OBJECT_ID('Master.Configuration', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.Configuration;
    PRINT '  ✓ Dropped Master.Configuration'
END

IF OBJECT_ID('Master.FeatureFlags', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.FeatureFlags;
    PRINT '  ✓ Dropped Master.FeatureFlags'
END

IF OBJECT_ID('Master.TimeZones', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.TimeZones;
    PRINT '  ✓ Dropped Master.TimeZones'
END

IF OBJECT_ID('Master.Languages', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.Languages;
    PRINT '  ✓ Dropped Master.Languages'
END

IF OBJECT_ID('Master.Currencies', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.Currencies;
    PRINT '  ✓ Dropped Master.Currencies'
END

IF OBJECT_ID('Master.Countries', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.Countries;
    PRINT '  ✓ Dropped Master.Countries'
END

IF OBJECT_ID('Master.Tenants', 'U') IS NOT NULL
BEGIN
    DROP TABLE Master.Tenants;
    PRINT '  ✓ Dropped Master.Tenants'
END

-- Transaction Schema Tables
PRINT ''
PRINT '🗑️  Dropping Transaction schema tables...'

-- Check if Transaction schema has any tables and drop them
DECLARE @TableName NVARCHAR(MAX);
DECLARE TableCursor CURSOR FOR
    SELECT TABLE_NAME
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Transaction'
    AND TABLE_TYPE = 'BASE TABLE';

OPEN TableCursor;
FETCH NEXT FROM TableCursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('DROP TABLE [Transaction].[' + @TableName + '];');
    PRINT '  ✓ Dropped Transaction.' + @TableName;
    FETCH NEXT FROM TableCursor INTO @TableName;
END

CLOSE TableCursor;
DEALLOCATE TableCursor;

-- Report Schema Tables
PRINT ''
PRINT '🗑️  Dropping Report schema tables...'

-- Check if Report schema has any tables and drop them
DECLARE @ReportTableName NVARCHAR(MAX);
DECLARE ReportTableCursor CURSOR FOR
    SELECT TABLE_NAME
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Report'
    AND TABLE_TYPE = 'BASE TABLE';

OPEN ReportTableCursor;
FETCH NEXT FROM ReportTableCursor INTO @ReportTableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('DROP TABLE [Report].[' + @ReportTableName + '];');
    PRINT '  ✓ Dropped Report.' + @ReportTableName;
    FETCH NEXT FROM ReportTableCursor INTO @ReportTableName;
END

CLOSE ReportTableCursor;
DEALLOCATE ReportTableCursor;

-- ============================================
-- Drop Schemas (if empty)
-- ============================================
PRINT ''
PRINT '🗑️  Dropping empty schemas...'

IF SCHEMA_ID('Shared') IS NOT NULL
BEGIN
    DROP SCHEMA Shared;
    PRINT '  ✓ Dropped Shared schema'
END

IF SCHEMA_ID('Master') IS NOT NULL
BEGIN
    DROP SCHEMA Master;
    PRINT '  ✓ Dropped Master schema'
END

IF SCHEMA_ID('Auth') IS NOT NULL
BEGIN
    DROP SCHEMA Auth;
    PRINT '  ✓ Dropped Auth schema'
END

IF SCHEMA_ID('Transaction') IS NOT NULL
BEGIN
    DROP SCHEMA [Transaction];
    PRINT '  ✓ Dropped Transaction schema'
END

IF SCHEMA_ID('Report') IS NOT NULL
BEGIN
    DROP SCHEMA Report;
    PRINT '  ✓ Dropped Report schema'
END

-- ============================================
-- Completion
-- ============================================
PRINT ''
PRINT '✅ All tables and schemas dropped successfully!'
PRINT ''
PRINT 'Database structure:'
PRINT '  • All 43 tables removed'
PRINT '  • All 5 schemas removed (Master, Shared, Auth, Transaction, Report)'
PRINT '  • Database ready for new schema design'
PRINT ''
PRINT 'To recreate the schema:'
PRINT '  Run migration scripts in order:'
PRINT '    001_InitializeDatabase.sql'
PRINT '    002_CreateTables_Master.sql'
PRINT '    003_CreateTables_Shared.sql'
PRINT '    004_CreateTables_Transaction.sql'
PRINT '    005_CreateTables_Report.sql'
PRINT '    006_CreateTables_Auth.sql'
PRINT ''
PRINT 'Then seed data:'
PRINT '    007_SeedData.sql'
PRINT '    008_SeedTestUsers.sql'
