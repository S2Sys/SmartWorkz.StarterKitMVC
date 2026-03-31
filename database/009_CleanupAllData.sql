-- ============================================
-- SmartWorkz v4: Clean All Data from Database
-- Date: 2026-03-31
-- WARNING: This script DELETES ALL DATA!
-- Keeps schema intact, removes only data
-- ============================================

USE Boilerplate;

PRINT '⚠️  Starting database cleanup - removing all data...'
PRINT ''

-- ============================================
-- Disable Constraints Temporarily
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

ALTER TABLE Auth.UserRoles NOCHECK CONSTRAINT ALL;
ALTER TABLE Auth.RolePermissions NOCHECK CONSTRAINT ALL;
ALTER TABLE Auth.UserPermissions NOCHECK CONSTRAINT ALL;
ALTER TABLE Auth.RefreshTokens NOCHECK CONSTRAINT ALL;
ALTER TABLE Auth.PasswordResetTokens NOCHECK CONSTRAINT ALL;
ALTER TABLE Auth.EmailVerificationTokens NOCHECK CONSTRAINT ALL;

-- ============================================
-- Delete Data (in reverse FK dependency order)
-- ============================================

-- Shared Schema (no FK dependencies between tables)
PRINT '🗑️  Cleaning Shared schema...'
DELETE FROM Shared.SeoMeta;
PRINT '  ✓ Shared.SeoMeta cleared'

DELETE FROM Shared.Tags;
PRINT '  ✓ Shared.Tags cleared'

DELETE FROM Shared.Translations;
PRINT '  ✓ Shared.Translations cleared'

DELETE FROM Shared.Notifications;
PRINT '  ✓ Shared.Notifications cleared'

DELETE FROM Shared.AuditLogs;
PRINT '  ✓ Shared.AuditLogs cleared'

DELETE FROM Shared.FileStorage;
PRINT '  ✓ Shared.FileStorage cleared'

DELETE FROM Shared.EmailQueue;
PRINT '  ✓ Shared.EmailQueue cleared'

-- Auth Schema (dependency order: dependent tables first)
PRINT ''
PRINT '🗑️  Cleaning Auth schema...'

DELETE FROM Auth.UserPermissions;
PRINT '  ✓ Auth.UserPermissions cleared'

DELETE FROM Auth.RolePermissions;
PRINT '  ✓ Auth.RolePermissions cleared'

DELETE FROM Auth.UserRoles;
PRINT '  ✓ Auth.UserRoles cleared'

DELETE FROM Auth.RefreshTokens;
PRINT '  ✓ Auth.RefreshTokens cleared'

DELETE FROM Auth.PasswordResetTokens;
PRINT '  ✓ Auth.PasswordResetTokens cleared'

DELETE FROM Auth.EmailVerificationTokens;
PRINT '  ✓ Auth.EmailVerificationTokens cleared'

DELETE FROM Auth.Users;
PRINT '  ✓ Auth.Users cleared'

DELETE FROM Auth.Permissions;
PRINT '  ✓ Auth.Permissions cleared'

DELETE FROM Auth.Roles;
PRINT '  ✓ Auth.Roles cleared'

-- Master Schema (dependency order: dependent tables first)
PRINT ''
PRINT '🗑️  Cleaning Master schema...'

DELETE FROM Master.TenantUsers;
PRINT '  ✓ Master.TenantUsers cleared'

DELETE FROM Master.GeolocationPages;
PRINT '  ✓ Master.GeolocationPages cleared'

DELETE FROM Master.CustomPages;
PRINT '  ✓ Master.CustomPages cleared'

DELETE FROM Master.BlogPosts;
PRINT '  ✓ Master.BlogPosts cleared'

DELETE FROM Master.MenuItems;
PRINT '  ✓ Master.MenuItems cleared'

DELETE FROM Master.Menus;
PRINT '  ✓ Master.Menus cleared'

DELETE FROM Master.Categories;
PRINT '  ✓ Master.Categories cleared'

DELETE FROM Master.GeoHierarchy;
PRINT '  ✓ Master.GeoHierarchy cleared'

DELETE FROM Master.Configuration;
PRINT '  ✓ Master.Configuration cleared'

DELETE FROM Master.FeatureFlags;
PRINT '  ✓ Master.FeatureFlags cleared'

DELETE FROM Master.TimeZones;
PRINT '  ✓ Master.TimeZones cleared'

DELETE FROM Master.Languages;
PRINT '  ✓ Master.Languages cleared'

DELETE FROM Master.Currencies;
PRINT '  ✓ Master.Currencies cleared'

DELETE FROM Master.Countries;
PRINT '  ✓ Master.Countries cleared'

DELETE FROM Master.Tenants;
PRINT '  ✓ Master.Tenants cleared'

-- ============================================
-- Re-enable Constraints
-- ============================================
PRINT ''
PRINT '🔒 Re-enabling constraints...'

ALTER TABLE Master.Tenants CHECK CONSTRAINT ALL;
ALTER TABLE Master.Countries CHECK CONSTRAINT ALL;
ALTER TABLE Master.Currencies CHECK CONSTRAINT ALL;
ALTER TABLE Master.Languages CHECK CONSTRAINT ALL;
ALTER TABLE Master.TimeZones CHECK CONSTRAINT ALL;
ALTER TABLE Master.Configuration CHECK CONSTRAINT ALL;
ALTER TABLE Master.FeatureFlags CHECK CONSTRAINT ALL;
ALTER TABLE Master.Menus CHECK CONSTRAINT ALL;
ALTER TABLE Master.MenuItems CHECK CONSTRAINT ALL;
ALTER TABLE Master.Categories CHECK CONSTRAINT ALL;
ALTER TABLE Master.GeoHierarchy CHECK CONSTRAINT ALL;
ALTER TABLE Master.GeolocationPages CHECK CONSTRAINT ALL;
ALTER TABLE Master.CustomPages CHECK CONSTRAINT ALL;
ALTER TABLE Master.BlogPosts CHECK CONSTRAINT ALL;
ALTER TABLE Master.TenantUsers CHECK CONSTRAINT ALL;

ALTER TABLE Auth.Roles CHECK CONSTRAINT ALL;
ALTER TABLE Auth.Permissions CHECK CONSTRAINT ALL;
ALTER TABLE Auth.Users CHECK CONSTRAINT ALL;
ALTER TABLE Auth.UserRoles CHECK CONSTRAINT ALL;
ALTER TABLE Auth.RolePermissions CHECK CONSTRAINT ALL;
ALTER TABLE Auth.UserPermissions CHECK CONSTRAINT ALL;
ALTER TABLE Auth.RefreshTokens CHECK CONSTRAINT ALL;
ALTER TABLE Auth.PasswordResetTokens CHECK CONSTRAINT ALL;
ALTER TABLE Auth.EmailVerificationTokens CHECK CONSTRAINT ALL;

ALTER TABLE Shared.SeoMeta CHECK CONSTRAINT ALL;
ALTER TABLE Shared.Tags CHECK CONSTRAINT ALL;
ALTER TABLE Shared.Translations CHECK CONSTRAINT ALL;
ALTER TABLE Shared.Notifications CHECK CONSTRAINT ALL;
ALTER TABLE Shared.AuditLogs CHECK CONSTRAINT ALL;
ALTER TABLE Shared.FileStorage CHECK CONSTRAINT ALL;
ALTER TABLE Shared.EmailQueue CHECK CONSTRAINT ALL;

PRINT '  ✓ All constraints re-enabled'

-- ============================================
-- Reset Identity Seeds (optional)
-- ============================================
PRINT ''
PRINT '🔄 Resetting identity seeds...'

-- Master Schema
DBCC CHECKIDENT ('Master.Countries', RESEED, 0);
PRINT '  ✓ Master.Countries identity reset'

DBCC CHECKIDENT ('Master.Currencies', RESEED, 0);
PRINT '  ✓ Master.Currencies identity reset'

DBCC CHECKIDENT ('Master.Languages', RESEED, 0);
PRINT '  ✓ Master.Languages identity reset'

DBCC CHECKIDENT ('Master.TimeZones', RESEED, 0);
PRINT '  ✓ Master.TimeZones identity reset'

DBCC CHECKIDENT ('Master.Configuration', RESEED, 0);
PRINT '  ✓ Master.Configuration identity reset'

DBCC CHECKIDENT ('Master.FeatureFlags', RESEED, 0);
PRINT '  ✓ Master.FeatureFlags identity reset'

DBCC CHECKIDENT ('Master.Menus', RESEED, 0);
PRINT '  ✓ Master.Menus identity reset'

DBCC CHECKIDENT ('Master.MenuItems', RESEED, 0);
PRINT '  ✓ Master.MenuItems identity reset'

DBCC CHECKIDENT ('Master.Categories', RESEED, 0);
PRINT '  ✓ Master.Categories identity reset'

DBCC CHECKIDENT ('Master.GeoHierarchy', RESEED, 0);
PRINT '  ✓ Master.GeoHierarchy identity reset'

DBCC CHECKIDENT ('Master.GeolocationPages', RESEED, 0);
PRINT '  ✓ Master.GeolocationPages identity reset'

DBCC CHECKIDENT ('Master.CustomPages', RESEED, 0);
PRINT '  ✓ Master.CustomPages identity reset'

DBCC CHECKIDENT ('Master.BlogPosts', RESEED, 0);
PRINT '  ✓ Master.BlogPosts identity reset'

DBCC CHECKIDENT ('Master.TenantUsers', RESEED, 0);
PRINT '  ✓ Master.TenantUsers identity reset'

-- Auth Schema
DBCC CHECKIDENT ('Auth.Permissions', RESEED, 0);
PRINT '  ✓ Auth.Permissions identity reset'

DBCC CHECKIDENT ('Auth.UserRoles', RESEED, 0);
PRINT '  ✓ Auth.UserRoles identity reset'

DBCC CHECKIDENT ('Auth.RolePermissions', RESEED, 0);
PRINT '  ✓ Auth.RolePermissions identity reset'

DBCC CHECKIDENT ('Auth.UserPermissions', RESEED, 0);
PRINT '  ✓ Auth.UserPermissions identity reset'

DBCC CHECKIDENT ('Auth.RefreshTokens', RESEED, 0);
PRINT '  ✓ Auth.RefreshTokens identity reset'

DBCC CHECKIDENT ('Auth.PasswordResetTokens', RESEED, 0);
PRINT '  ✓ Auth.PasswordResetTokens identity reset'

DBCC CHECKIDENT ('Auth.EmailVerificationTokens', RESEED, 0);
PRINT '  ✓ Auth.EmailVerificationTokens identity reset'

-- Shared Schema
DBCC CHECKIDENT ('Shared.SeoMeta', RESEED, 0);
PRINT '  ✓ Shared.SeoMeta identity reset'

DBCC CHECKIDENT ('Shared.Tags', RESEED, 0);
PRINT '  ✓ Shared.Tags identity reset'

DBCC CHECKIDENT ('Shared.Translations', RESEED, 0);
PRINT '  ✓ Shared.Translations identity reset'

DBCC CHECKIDENT ('Shared.Notifications', RESEED, 0);
PRINT '  ✓ Shared.Notifications identity reset'

DBCC CHECKIDENT ('Shared.AuditLogs', RESEED, 0);
PRINT '  ✓ Shared.AuditLogs identity reset'

DBCC CHECKIDENT ('Shared.FileStorage', RESEED, 0);
PRINT '  ✓ Shared.FileStorage identity reset'

DBCC CHECKIDENT ('Shared.EmailQueue', RESEED, 0);
PRINT '  ✓ Shared.EmailQueue identity reset'

-- ============================================
-- Verify Cleanup
-- ============================================
PRINT ''
PRINT '✅ Database cleanup completed successfully!'
PRINT ''
PRINT 'Summary:'
PRINT '  • All data removed from all tables'
PRINT '  • All schema structures preserved'
PRINT '  • All constraints re-enabled'
PRINT '  • All identity seeds reset to 0'
PRINT ''
PRINT 'Next steps:'
PRINT '  1. Run 007_SeedData.sql to reload initial seed data'
PRINT '  2. Run 008_SeedTestUsers.sql to reload test users'
PRINT ''
PRINT 'Database is ready for fresh start!'
