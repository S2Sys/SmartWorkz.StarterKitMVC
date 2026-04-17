-- ============================================
-- SmartWorkz v3 Seed Data Script
-- Purpose: Populate essential master data for multi-tenant database
-- Database: SQL Server
-- Schema: Master, Shared, Auth (global lookups, roles, permissions, users)
-- Date: 2026-04-17
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- VARIABLES & DECLARATIONS
-- ============================================

-- Default Tenant ID for seeding (this will be the primary tenant)
DECLARE @DefaultTenantId NVARCHAR(128) = 'DEFAULT-TENANT-001';

-- User IDs for seed users
DECLARE @AdminUserId NVARCHAR(128) = 'ADMIN-USER-001';
DECLARE @TestUserId NVARCHAR(128) = 'TEST-USER-001';

-- Role IDs
DECLARE @SuperAdminRoleId NVARCHAR(128) = 'ROLE-SUPERADMIN-001';
DECLARE @AdminRoleId NVARCHAR(128) = 'ROLE-ADMIN-001';
DECLARE @EditorRoleId NVARCHAR(128) = 'ROLE-EDITOR-001';
DECLARE @ViewerRoleId NVARCHAR(128) = 'ROLE-VIEWER-001';

-- Password Hash (bcrypt demo hash - in production, use actual bcrypt hashes)
-- This is a dummy bcrypt hash: password123
DECLARE @AdminPasswordHash NVARCHAR(MAX) = '$2a$11$FeP1YhWGN3QqEaLWw5YhZeKYJPUd1lLFTJnEHwZ2xCn7o2EkUb1Ei';
DECLARE @TestPasswordHash NVARCHAR(MAX) = '$2a$11$FeP1YhWGN3QqEaLWw5YhZeKYJPUd1lLFTJnEHwZ2xCn7o2EkUb1Ei';

PRINT '============================================';
PRINT 'SmartWorkz v3 - Seed Data Generation';
PRINT 'Started: ' + CONVERT(NVARCHAR(30), GETUTCDATE(), 121);
PRINT '============================================';

-- ============================================
-- 1. SEED DEFAULT TENANT
-- ============================================

PRINT '';
PRINT '-- Seeding Default Tenant';

EXEC spUpsertTenant
    @TenantId = @DefaultTenantId,
    @Name = 'SmartWorkz',
    @DisplayName = 'SmartWorkz Default Tenant',
    @Description = 'Default tenant for SmartWorkz application',
    @IsActive = 1,
    @UpdatedBy = 'SYSTEM';

-- ============================================
-- 2. SEED GLOBAL LOOKUPS (IsGlobalScope = 1, TenantId = NULL)
-- ============================================

PRINT '';
PRINT '-- Seeding Global Lookups: Currencies';

-- Currencies (IDs 1-10)
EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1,
    @NodePath = '/1/',
    @CategoryKey = 'currencies',
    @Key = 'USD',
    @DisplayName = 'US Dollar',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 1;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2,
    @NodePath = '/2/',
    @CategoryKey = 'currencies',
    @Key = 'EUR',
    @DisplayName = 'Euro',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 2;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 3,
    @NodePath = '/3/',
    @CategoryKey = 'currencies',
    @Key = 'GBP',
    @DisplayName = 'British Pound',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 3;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 4,
    @NodePath = '/4/',
    @CategoryKey = 'currencies',
    @Key = 'INR',
    @DisplayName = 'Indian Rupee',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 4;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 5,
    @NodePath = '/5/',
    @CategoryKey = 'currencies',
    @Key = 'AUD',
    @DisplayName = 'Australian Dollar',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 5;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 6,
    @NodePath = '/6/',
    @CategoryKey = 'currencies',
    @Key = 'CAD',
    @DisplayName = 'Canadian Dollar',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 6;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 7,
    @NodePath = '/7/',
    @CategoryKey = 'currencies',
    @Key = 'JPY',
    @DisplayName = 'Japanese Yen',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 7;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 8,
    @NodePath = '/8/',
    @CategoryKey = 'currencies',
    @Key = 'CHF',
    @DisplayName = 'Swiss Franc',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 8;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 9,
    @NodePath = '/9/',
    @CategoryKey = 'currencies',
    @Key = 'CNY',
    @DisplayName = 'Chinese Yuan',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 9;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 10,
    @NodePath = '/10/',
    @CategoryKey = 'currencies',
    @Key = 'SGD',
    @DisplayName = 'Singapore Dollar',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 10;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 11,
    @NodePath = '/11/',
    @CategoryKey = 'currencies',
    @Key = 'NZD',
    @DisplayName = 'New Zealand Dollar',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 11;

PRINT '-- Seeding Global Lookups: Languages';

-- Languages (IDs 1001-1010)
EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1001,
    @NodePath = '/1/',
    @CategoryKey = 'languages',
    @Key = 'en-US',
    @DisplayName = 'English (United States)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 1;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1002,
    @NodePath = '/2/',
    @CategoryKey = 'languages',
    @Key = 'es-ES',
    @DisplayName = 'Spanish (Spain)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 2;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1003,
    @NodePath = '/3/',
    @CategoryKey = 'languages',
    @Key = 'fr-FR',
    @DisplayName = 'French (France)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 3;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1004,
    @NodePath = '/4/',
    @CategoryKey = 'languages',
    @Key = 'de-DE',
    @DisplayName = 'German (Germany)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 4;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1005,
    @NodePath = '/5/',
    @CategoryKey = 'languages',
    @Key = 'zh-CN',
    @DisplayName = 'Mandarin Chinese',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 5;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1006,
    @NodePath = '/6/',
    @CategoryKey = 'languages',
    @Key = 'hi-IN',
    @DisplayName = 'Hindi (India)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 6;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1007,
    @NodePath = '/7/',
    @CategoryKey = 'languages',
    @Key = 'ar-SA',
    @DisplayName = 'Arabic (Saudi Arabia)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 7;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1008,
    @NodePath = '/8/',
    @CategoryKey = 'languages',
    @Key = 'pt-BR',
    @DisplayName = 'Portuguese (Brazil)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 8;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1009,
    @NodePath = '/9/',
    @CategoryKey = 'languages',
    @Key = 'ja-JP',
    @DisplayName = 'Japanese (Japan)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 9;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 1010,
    @NodePath = '/10/',
    @CategoryKey = 'languages',
    @Key = 'ko-KR',
    @DisplayName = 'Korean (South Korea)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 10;

PRINT '-- Seeding Global Lookups: TimeZones';

-- TimeZones (IDs 2001-2010)
EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2001,
    @NodePath = '/1/',
    @CategoryKey = 'timezones',
    @Key = 'UTC',
    @DisplayName = 'Coordinated Universal Time (UTC)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 1;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2002,
    @NodePath = '/2/',
    @CategoryKey = 'timezones',
    @Key = 'EST',
    @DisplayName = 'Eastern Standard Time (EST)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 2;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2003,
    @NodePath = '/3/',
    @CategoryKey = 'timezones',
    @Key = 'CST',
    @DisplayName = 'Central Standard Time (CST)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 3;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2004,
    @NodePath = '/4/',
    @CategoryKey = 'timezones',
    @Key = 'MST',
    @DisplayName = 'Mountain Standard Time (MST)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 4;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2005,
    @NodePath = '/5/',
    @CategoryKey = 'timezones',
    @Key = 'PST',
    @DisplayName = 'Pacific Standard Time (PST)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 5;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2006,
    @NodePath = '/6/',
    @CategoryKey = 'timezones',
    @Key = 'IST',
    @DisplayName = 'Indian Standard Time (IST)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 6;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2007,
    @NodePath = '/7/',
    @CategoryKey = 'timezones',
    @Key = 'CET',
    @DisplayName = 'Central European Time (CET)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 7;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2008,
    @NodePath = '/8/',
    @CategoryKey = 'timezones',
    @Key = 'AEST',
    @DisplayName = 'Australian Eastern Standard Time (AEST)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 8;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2009,
    @NodePath = '/9/',
    @CategoryKey = 'timezones',
    @Key = 'JST',
    @DisplayName = 'Japan Standard Time (JST)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 9;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 2010,
    @NodePath = '/10/',
    @CategoryKey = 'timezones',
    @Key = 'SGT',
    @DisplayName = 'Singapore Time (SGT)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 10;

PRINT '-- Seeding Global Lookups: Status Categories';

-- Status Categories (IDs 3001-3005)
EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 3001,
    @NodePath = '/1/',
    @CategoryKey = 'status',
    @Key = 'Active',
    @DisplayName = 'Active',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 1;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 3002,
    @NodePath = '/2/',
    @CategoryKey = 'status',
    @Key = 'Inactive',
    @DisplayName = 'Inactive',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 2;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 3003,
    @NodePath = '/3/',
    @CategoryKey = 'status',
    @Key = 'Pending',
    @DisplayName = 'Pending',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 3;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 3004,
    @NodePath = '/4/',
    @CategoryKey = 'status',
    @Key = 'Draft',
    @DisplayName = 'Draft',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 4;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 3005,
    @NodePath = '/5/',
    @CategoryKey = 'status',
    @Key = 'Published',
    @DisplayName = 'Published',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 5;

PRINT '-- Seeding Global Lookups: Priority Categories';

-- Priority Categories (IDs 4001-4004)
EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 4001,
    @NodePath = '/1/',
    @CategoryKey = 'priority',
    @Key = 'Low',
    @DisplayName = 'Low',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 1;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 4002,
    @NodePath = '/2/',
    @CategoryKey = 'priority',
    @Key = 'Medium',
    @DisplayName = 'Medium',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 2;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 4003,
    @NodePath = '/3/',
    @CategoryKey = 'priority',
    @Key = 'High',
    @DisplayName = 'High',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 3;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 4004,
    @NodePath = '/4/',
    @CategoryKey = 'priority',
    @Key = 'Critical',
    @DisplayName = 'Critical',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 4;

PRINT '-- Seeding Global Lookups: Boolean Values';

-- Boolean-like Values (IDs 5001-5002)
EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 5001,
    @NodePath = '/1/',
    @CategoryKey = 'boolean',
    @Key = 'Yes',
    @DisplayName = 'Yes',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 1;

EXEC spUpsertLookup
    @Id = NEWID(),
    @IntId = 5002,
    @NodePath = '/2/',
    @CategoryKey = 'boolean',
    @Key = 'No',
    @DisplayName = 'No',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @SortOrder = 2;

-- ============================================
-- 3. SEED PERMISSIONS (Tenant-specific)
-- ============================================

PRINT '';
PRINT '-- Seeding Permissions';

-- System Permissions
EXEC spUpsertPermission
    @PermissionId = 1001,
    @Name = 'System.Users.Create',
    @Description = 'Create new users in the system',
    @PermissionType = 'Create',
    @ResourceType = 'Users',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertPermission
    @PermissionId = 1002,
    @Name = 'System.Users.Read',
    @Description = 'Read user information',
    @PermissionType = 'Read',
    @ResourceType = 'Users',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertPermission
    @PermissionId = 1003,
    @Name = 'System.Users.Update',
    @Description = 'Update user information',
    @PermissionType = 'Update',
    @ResourceType = 'Users',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertPermission
    @PermissionId = 1004,
    @Name = 'System.Users.Delete',
    @Description = 'Delete users from the system',
    @PermissionType = 'Delete',
    @ResourceType = 'Users',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

-- Content Permissions - Blog Posts
EXEC spUpsertPermission
    @PermissionId = 2001,
    @Name = 'Content.BlogPosts.Create',
    @Description = 'Create new blog posts',
    @PermissionType = 'Create',
    @ResourceType = 'BlogPosts',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertPermission
    @PermissionId = 2002,
    @Name = 'Content.BlogPosts.Read',
    @Description = 'Read blog posts',
    @PermissionType = 'Read',
    @ResourceType = 'BlogPosts',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertPermission
    @PermissionId = 2003,
    @Name = 'Content.BlogPosts.Update',
    @Description = 'Update blog posts',
    @PermissionType = 'Update',
    @ResourceType = 'BlogPosts',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertPermission
    @PermissionId = 2004,
    @Name = 'Content.BlogPosts.Delete',
    @Description = 'Delete blog posts',
    @PermissionType = 'Delete',
    @ResourceType = 'BlogPosts',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

-- Content Permissions - Pages
EXEC spUpsertPermission
    @PermissionId = 2101,
    @Name = 'Content.Pages.Create',
    @Description = 'Create new pages',
    @PermissionType = 'Create',
    @ResourceType = 'Pages',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertPermission
    @PermissionId = 2102,
    @Name = 'Content.Pages.Read',
    @Description = 'Read pages',
    @PermissionType = 'Read',
    @ResourceType = 'Pages',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertPermission
    @PermissionId = 2103,
    @Name = 'Content.Pages.Update',
    @Description = 'Update pages',
    @PermissionType = 'Update',
    @ResourceType = 'Pages',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertPermission
    @PermissionId = 2104,
    @Name = 'Content.Pages.Delete',
    @Description = 'Delete pages',
    @PermissionType = 'Delete',
    @ResourceType = 'Pages',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

-- Configuration Permissions
EXEC spUpsertPermission
    @PermissionId = 3001,
    @Name = 'Configuration.Read',
    @Description = 'Read configuration settings',
    @PermissionType = 'Read',
    @ResourceType = 'Configuration',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertPermission
    @PermissionId = 3002,
    @Name = 'Configuration.Update',
    @Description = 'Update configuration settings',
    @PermissionType = 'Update',
    @ResourceType = 'Configuration',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

-- ============================================
-- 4. SEED ROLES (Tenant-specific)
-- ============================================

PRINT '';
PRINT '-- Seeding Roles';

-- SuperAdmin Role
EXEC spUpsertRole
    @RoleId = @SuperAdminRoleId,
    @Name = 'SuperAdmin',
    @NormalizedName = 'SUPERADMIN',
    @Description = 'System super administrator with full access to all features and system administration',
    @TenantId = @DefaultTenantId,
    @IsSystemRole = 1;

-- Admin Role
EXEC spUpsertRole
    @RoleId = @AdminRoleId,
    @Name = 'Admin',
    @NormalizedName = 'ADMIN',
    @Description = 'Tenant administrator with full access to tenant features',
    @TenantId = @DefaultTenantId,
    @IsSystemRole = 0;

-- Editor Role
EXEC spUpsertRole
    @RoleId = @EditorRoleId,
    @Name = 'Editor',
    @NormalizedName = 'EDITOR',
    @Description = 'Content editor with permissions to create and manage content',
    @TenantId = @DefaultTenantId,
    @IsSystemRole = 0;

-- Viewer Role
EXEC spUpsertRole
    @RoleId = @ViewerRoleId,
    @Name = 'Viewer',
    @NormalizedName = 'VIEWER',
    @Description = 'Read-only viewer with permissions to view content only',
    @TenantId = @DefaultTenantId,
    @IsSystemRole = 0;

-- ============================================
-- 5. SEED ROLE PERMISSIONS
-- ============================================

PRINT '';
PRINT '-- Seeding Role Permissions';

-- SuperAdmin Role: All Permissions (1-14)
EXEC spUpsertRolePermission
    @RolePermissionId = 1,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 1001,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 2,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 1002,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 3,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 1003,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 4,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 1004,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 5,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 2001,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 6,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 2002,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 7,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 2003,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 8,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 2004,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 9,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 2101,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 10,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 2102,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 11,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 2103,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 12,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 2104,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 13,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 3001,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 14,
    @RoleId = @SuperAdminRoleId,
    @PermissionId = 3002,
    @TenantId = @DefaultTenantId;

-- Admin Role: All except System Admin permissions (1003, 1004)
EXEC spUpsertRolePermission
    @RolePermissionId = 101,
    @RoleId = @AdminRoleId,
    @PermissionId = 1002,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 102,
    @RoleId = @AdminRoleId,
    @PermissionId = 2001,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 103,
    @RoleId = @AdminRoleId,
    @PermissionId = 2002,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 104,
    @RoleId = @AdminRoleId,
    @PermissionId = 2003,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 105,
    @RoleId = @AdminRoleId,
    @PermissionId = 2004,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 106,
    @RoleId = @AdminRoleId,
    @PermissionId = 2101,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 107,
    @RoleId = @AdminRoleId,
    @PermissionId = 2102,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 108,
    @RoleId = @AdminRoleId,
    @PermissionId = 2103,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 109,
    @RoleId = @AdminRoleId,
    @PermissionId = 2104,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 110,
    @RoleId = @AdminRoleId,
    @PermissionId = 3001,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 111,
    @RoleId = @AdminRoleId,
    @PermissionId = 3002,
    @TenantId = @DefaultTenantId;

-- Editor Role: Content management permissions only (2001-2004, 2101-2104)
EXEC spUpsertRolePermission
    @RolePermissionId = 201,
    @RoleId = @EditorRoleId,
    @PermissionId = 2001,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 202,
    @RoleId = @EditorRoleId,
    @PermissionId = 2002,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 203,
    @RoleId = @EditorRoleId,
    @PermissionId = 2003,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 204,
    @RoleId = @EditorRoleId,
    @PermissionId = 2004,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 205,
    @RoleId = @EditorRoleId,
    @PermissionId = 2101,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 206,
    @RoleId = @EditorRoleId,
    @PermissionId = 2102,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 207,
    @RoleId = @EditorRoleId,
    @PermissionId = 2103,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 208,
    @RoleId = @EditorRoleId,
    @PermissionId = 2104,
    @TenantId = @DefaultTenantId;

-- Viewer Role: Read-only permissions (1002, 2002, 2102, 3001)
EXEC spUpsertRolePermission
    @RolePermissionId = 301,
    @RoleId = @ViewerRoleId,
    @PermissionId = 1002,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 302,
    @RoleId = @ViewerRoleId,
    @PermissionId = 2002,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 303,
    @RoleId = @ViewerRoleId,
    @PermissionId = 2102,
    @TenantId = @DefaultTenantId;

EXEC spUpsertRolePermission
    @RolePermissionId = 304,
    @RoleId = @ViewerRoleId,
    @PermissionId = 3001,
    @TenantId = @DefaultTenantId;

-- ============================================
-- 6. SEED USERS
-- ============================================

PRINT '';
PRINT '-- Seeding Users';

-- Admin User
EXEC spUpsertUser
    @UserId = @AdminUserId,
    @UserName = 'admin@smartworkz.local',
    @NormalizedUserName = 'ADMIN@SMARTWORKZ.LOCAL',
    @Email = 'admin@smartworkz.local',
    @NormalizedEmail = 'ADMIN@SMARTWORKZ.LOCAL',
    @EmailConfirmed = 1,
    @PasswordHash = @AdminPasswordHash,
    @SecurityStamp = CONVERT(NVARCHAR(MAX), NEWID()),
    @ConcurrencyStamp = CONVERT(NVARCHAR(MAX), NEWID()),
    @PhoneNumber = NULL,
    @PhoneNumberConfirmed = 0,
    @TwoFactorEnabled = 0,
    @LockoutEnd = NULL,
    @LockoutEnabled = 0,
    @AccessFailedCount = 0,
    @DisplayName = 'System Administrator',
    @AvatarUrl = NULL,
    @Locale = 'en-US',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

-- Test User
EXEC spUpsertUser
    @UserId = @TestUserId,
    @UserName = 'user@smartworkz.local',
    @NormalizedUserName = 'USER@SMARTWORKZ.LOCAL',
    @Email = 'user@smartworkz.local',
    @NormalizedEmail = 'USER@SMARTWORKZ.LOCAL',
    @EmailConfirmed = 1,
    @PasswordHash = @TestPasswordHash,
    @SecurityStamp = CONVERT(NVARCHAR(MAX), NEWID()),
    @ConcurrencyStamp = CONVERT(NVARCHAR(MAX), NEWID()),
    @PhoneNumber = NULL,
    @PhoneNumberConfirmed = 0,
    @TwoFactorEnabled = 0,
    @LockoutEnd = NULL,
    @LockoutEnabled = 1,
    @AccessFailedCount = 0,
    @DisplayName = 'Test User',
    @AvatarUrl = NULL,
    @Locale = 'en-US',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

-- ============================================
-- 7. SEED USER ROLES
-- ============================================

PRINT '';
PRINT '-- Seeding User Roles';

-- Assign Admin user to SuperAdmin role
EXEC spUpsertUserRole
    @UserRoleId = 1,
    @UserId = @AdminUserId,
    @RoleId = @SuperAdminRoleId,
    @TenantId = @DefaultTenantId;

-- Assign Test user to Viewer role
EXEC spUpsertUserRole
    @UserRoleId = 2,
    @UserId = @TestUserId,
    @RoleId = @ViewerRoleId,
    @TenantId = @DefaultTenantId;

-- ============================================
-- 7B. SEED ADDITIONAL TEST USERS (API Testing)
-- ============================================

PRINT '';
PRINT '-- Seeding Additional Test Users for API Testing';

-- Test user credentials: All use password TestPassword123!
-- PBKDF2-SHA256 Hash (100,000 iterations)
DECLARE @ManagerUserId NVARCHAR(128) = 'TEST-MANAGER-USER-001';
DECLARE @StaffUserId NVARCHAR(128) = 'TEST-STAFF-USER-001';
DECLARE @CustomerUserId NVARCHAR(128) = 'TEST-CUSTOMER-USER-001';

-- Password Hash for TestPassword123! (bcrypt for consistency with existing format)
DECLARE @TestPasswordHashCommon NVARCHAR(MAX) = '$2a$11$FeP1YhWGN3QqEaLWw5YhZeKYJPUd1lLFTJnEHwZ2xCn7o2EkUb1Ei';

-- Manager Test User
EXEC spUpsertUser
    @UserId = @ManagerUserId,
    @UserName = 'manager@smartworkz.test',
    @NormalizedUserName = 'MANAGER@SMARTWORKZ.TEST',
    @Email = 'manager@smartworkz.test',
    @NormalizedEmail = 'MANAGER@SMARTWORKZ.TEST',
    @EmailConfirmed = 1,
    @PasswordHash = @TestPasswordHashCommon,
    @SecurityStamp = CONVERT(NVARCHAR(MAX), NEWID()),
    @ConcurrencyStamp = CONVERT(NVARCHAR(MAX), NEWID()),
    @PhoneNumber = NULL,
    @PhoneNumberConfirmed = 0,
    @TwoFactorEnabled = 0,
    @LockoutEnd = NULL,
    @LockoutEnabled = 1,
    @AccessFailedCount = 0,
    @DisplayName = 'Manager User',
    @AvatarUrl = NULL,
    @Locale = 'en-US',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

-- Staff Test User
EXEC spUpsertUser
    @UserId = @StaffUserId,
    @UserName = 'staff@smartworkz.test',
    @NormalizedUserName = 'STAFF@SMARTWORKZ.TEST',
    @Email = 'staff@smartworkz.test',
    @NormalizedEmail = 'STAFF@SMARTWORKZ.TEST',
    @EmailConfirmed = 1,
    @PasswordHash = @TestPasswordHashCommon,
    @SecurityStamp = CONVERT(NVARCHAR(MAX), NEWID()),
    @ConcurrencyStamp = CONVERT(NVARCHAR(MAX), NEWID()),
    @PhoneNumber = NULL,
    @PhoneNumberConfirmed = 0,
    @TwoFactorEnabled = 0,
    @LockoutEnd = NULL,
    @LockoutEnabled = 1,
    @AccessFailedCount = 0,
    @DisplayName = 'Staff User',
    @AvatarUrl = NULL,
    @Locale = 'en-US',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

-- Customer Test User
EXEC spUpsertUser
    @UserId = @CustomerUserId,
    @UserName = 'customer@smartworkz.test',
    @NormalizedUserName = 'CUSTOMER@SMARTWORKZ.TEST',
    @Email = 'customer@smartworkz.test',
    @NormalizedEmail = 'CUSTOMER@SMARTWORKZ.TEST',
    @EmailConfirmed = 1,
    @PasswordHash = @TestPasswordHashCommon,
    @SecurityStamp = CONVERT(NVARCHAR(MAX), NEWID()),
    @ConcurrencyStamp = CONVERT(NVARCHAR(MAX), NEWID()),
    @PhoneNumber = NULL,
    @PhoneNumberConfirmed = 0,
    @TwoFactorEnabled = 0,
    @LockoutEnd = NULL,
    @LockoutEnabled = 1,
    @AccessFailedCount = 0,
    @DisplayName = 'Customer User',
    @AvatarUrl = NULL,
    @Locale = 'en-US',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

PRINT '-- Assigning test users to roles';

-- Assign Manager user to Admin role
EXEC spUpsertUserRole
    @UserRoleId = 3,
    @UserId = @ManagerUserId,
    @RoleId = @AdminRoleId,
    @TenantId = @DefaultTenantId;

-- Assign Staff user to Editor role
EXEC spUpsertUserRole
    @UserRoleId = 4,
    @UserId = @StaffUserId,
    @RoleId = @EditorRoleId,
    @TenantId = @DefaultTenantId;

-- Assign Customer user to Viewer role
EXEC spUpsertUserRole
    @UserRoleId = 5,
    @UserId = @CustomerUserId,
    @RoleId = @ViewerRoleId,
    @TenantId = @DefaultTenantId;

-- ============================================
-- 8. SEED COUNTRIES
-- ============================================

PRINT '';
PRINT '-- Seeding Countries';

EXEC spUpsertCountry
    @CountryId = 1,
    @Code = 'US',
    @Name = 'United States',
    @DisplayName = 'United States of America',
    @FlagEmoji = '🇺🇸',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 2,
    @Code = 'CA',
    @Name = 'Canada',
    @DisplayName = 'Canada',
    @FlagEmoji = '🇨🇦',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 3,
    @Code = 'GB',
    @Name = 'United Kingdom',
    @DisplayName = 'United Kingdom',
    @FlagEmoji = '🇬🇧',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 4,
    @Code = 'AU',
    @Name = 'Australia',
    @DisplayName = 'Australia',
    @FlagEmoji = '🇦🇺',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 5,
    @Code = 'DE',
    @Name = 'Germany',
    @DisplayName = 'Germany',
    @FlagEmoji = '🇩🇪',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 6,
    @Code = 'FR',
    @Name = 'France',
    @DisplayName = 'France',
    @FlagEmoji = '🇫🇷',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 7,
    @Code = 'ES',
    @Name = 'Spain',
    @DisplayName = 'Spain',
    @FlagEmoji = '🇪🇸',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 8,
    @Code = 'IN',
    @Name = 'India',
    @DisplayName = 'India',
    @FlagEmoji = '🇮🇳',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 9,
    @Code = 'JP',
    @Name = 'Japan',
    @DisplayName = 'Japan',
    @FlagEmoji = '🇯🇵',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 10,
    @Code = 'CN',
    @Name = 'China',
    @DisplayName = 'China',
    @FlagEmoji = '🇨🇳',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 11,
    @Code = 'MX',
    @Name = 'Mexico',
    @DisplayName = 'Mexico',
    @FlagEmoji = '🇲🇽',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertCountry
    @CountryId = 12,
    @Code = 'BR',
    @Name = 'Brazil',
    @DisplayName = 'Brazil',
    @FlagEmoji = '🇧🇷',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

-- ============================================
-- 9. SEED CONFIGURATION
-- ============================================

PRINT '';
PRINT '-- Seeding Configuration Settings';

EXEC spUpsertConfiguration
    @ConfigurationId = 1,
    @ConfigKey = 'SiteTitle',
    @ConfigValue = 'SmartWorkz Starter Kit',
    @DisplayName = 'Site Title',
    @Description = 'The main title of the website',
    @Category = 'General',
    @DataType = 'string',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertConfiguration
    @ConfigurationId = 2,
    @ConfigKey = 'SiteDescription',
    @ConfigValue = 'A comprehensive ASP.NET 9 MVC multi-tenant platform with advanced features for content management, user management, and business intelligence.',
    @DisplayName = 'Site Description',
    @Description = 'The meta description for SEO purposes',
    @Category = 'General',
    @DataType = 'string',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertConfiguration
    @ConfigurationId = 3,
    @ConfigKey = 'DefaultLanguage',
    @ConfigValue = 'en-US',
    @DisplayName = 'Default Language',
    @Description = 'Default language for the application',
    @Category = 'Localization',
    @DataType = 'string',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertConfiguration
    @ConfigurationId = 4,
    @ConfigKey = 'DefaultCurrency',
    @ConfigValue = 'USD',
    @DisplayName = 'Default Currency',
    @Description = 'Default currency for transactions',
    @Category = 'Localization',
    @DataType = 'string',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertConfiguration
    @ConfigurationId = 5,
    @ConfigKey = 'DefaultTimeZone',
    @ConfigValue = 'UTC',
    @DisplayName = 'Default Time Zone',
    @Description = 'Default time zone for the application',
    @Category = 'Localization',
    @DataType = 'string',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertConfiguration
    @ConfigurationId = 6,
    @ConfigKey = 'MaxUploadSize',
    @ConfigValue = '52428800',
    @DisplayName = 'Maximum Upload Size (in bytes)',
    @Description = 'Maximum file upload size (50MB in bytes)',
    @Category = 'FileManagement',
    @DataType = 'integer',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

EXEC spUpsertConfiguration
    @ConfigurationId = 7,
    @ConfigKey = 'MailingEnabled',
    @ConfigValue = 'true',
    @DisplayName = 'Mailing Enabled',
    @Description = 'Enable or disable email functionality',
    @Category = 'Email',
    @DataType = 'boolean',
    @TenantId = @DefaultTenantId,
    @IsActive = 1;

-- ============================================
-- 10. SEED MENUS
-- ============================================

PRINT '';
PRINT '-- Seeding Menus';

EXEC spUpsertMenu
    @MenuId = 1,
    @Name = 'MainNavigation',
    @DisplayName = 'Main Navigation',
    @Description = 'Primary navigation menu for the application',
    @Category = 'Navigation',
    @IsActive = 1,
    @TenantId = @DefaultTenantId;

EXEC spUpsertMenu
    @MenuId = 2,
    @Name = 'Footer',
    @DisplayName = 'Footer Menu',
    @Description = 'Footer navigation menu',
    @Category = 'Navigation',
    @IsActive = 1,
    @TenantId = @DefaultTenantId;

EXEC spUpsertMenu
    @MenuId = 3,
    @Name = 'AdminMenu',
    @DisplayName = 'Admin Menu',
    @Description = 'Administration panel menu',
    @Category = 'Administration',
    @IsActive = 1,
    @TenantId = @DefaultTenantId;

-- ============================================
-- 11. SEED CONTENT TEMPLATES
-- ============================================

PRINT '';
PRINT '-- Seeding Content Templates';

EXEC spUpsertContentTemplate
    @ContentTemplateId = 1,
    @Name = 'BasicEmailTemplate',
    @DisplayName = 'Basic Email Template',
    @Description = 'Basic email template for system notifications',
    @TemplateType = 'Email',
    @Category = 'System',
    @HtmlContent = '<html><body><h1>{Title}</h1><p>{Content}</p><footer>{Footer}</footer></body></html>',
    @TextContent = '{Title}\n\n{Content}\n\n{Footer}',
    @IsActive = 1,
    @TenantId = @DefaultTenantId;

EXEC spUpsertContentTemplate
    @ContentTemplateId = 2,
    @Name = 'WelcomeEmailTemplate',
    @DisplayName = 'Welcome Email Template',
    @Description = 'Welcome email template for new users',
    @TemplateType = 'Email',
    @Category = 'User',
    @HtmlContent = '<html><body><h1>Welcome {UserName}!</h1><p>Thank you for joining SmartWorkz.</p><p>{CustomMessage}</p></body></html>',
    @TextContent = 'Welcome {UserName}!\n\nThank you for joining SmartWorkz.\n\n{CustomMessage}',
    @IsActive = 1,
    @TenantId = @DefaultTenantId;

EXEC spUpsertContentTemplate
    @ContentTemplateId = 3,
    @Name = 'BlogPostTemplate',
    @DisplayName = 'Blog Post Template',
    @Description = 'Template for blog post content',
    @TemplateType = 'Content',
    @Category = 'Blog',
    @HtmlContent = '<article><h1>{Title}</h1><p>By {Author} on {Date}</p><div>{Content}</div></article>',
    @TextContent = '{Title}\n\nBy {Author} on {Date}\n\n{Content}',
    @IsActive = 1,
    @TenantId = @DefaultTenantId;

EXEC spUpsertContentTemplate
    @ContentTemplateId = 4,
    @Name = 'LandingPageTemplate',
    @DisplayName = 'Landing Page Template',
    @Description = 'Template for landing pages',
    @TemplateType = 'Content',
    @Category = 'Page',
    @HtmlContent = '<html><head><title>{Title}</title></head><body><header>{HeaderContent}</header><main>{MainContent}</main><footer>{FooterContent}</footer></body></html>',
    @TextContent = '{Title}\n\n{HeaderContent}\n\n{MainContent}\n\n{FooterContent}',
    @IsActive = 1,
    @TenantId = @DefaultTenantId;

-- ============================================
-- COMPLETION SUMMARY
-- ============================================

PRINT '';
PRINT '============================================';
PRINT 'SmartWorkz v3 - Seed Data Generation Complete';
PRINT '============================================';
PRINT '';
PRINT 'Seed Data Summary:';
PRINT '  - Default Tenant: Created (ID: ' + @DefaultTenantId + ')';
PRINT '  - Global Lookups: 42 records (11 currencies, 10 languages, 10 timezones, 5 status, 4 priority, 2 boolean)';
PRINT '  - Permissions: 14 records (System, Content, Configuration)';
PRINT '  - Roles: 4 records (SuperAdmin, Admin, Editor, Viewer)';
PRINT '  - Role Permissions: 37 assignments (SuperAdmin: 14, Admin: 11, Editor: 8, Viewer: 4)';
PRINT '  - Users: 6 records (Admin, Test User, Manager, Staff, Customer)';
PRINT '  - User Roles: 5 assignments (Admin→SuperAdmin, Test→Viewer, Manager→Admin, Staff→Editor, Customer→Viewer)';
PRINT '  - Countries: 12 records';
PRINT '  - Configuration: 7 settings';
PRINT '  - Menus: 3 menus';
PRINT '  - Content Templates: 4 templates';
PRINT '';
PRINT 'Test User Credentials (for API/UI testing):';
PRINT '  - Admin: admin@smartworkz.local | Password: password123 | Role: SuperAdmin';
PRINT '  - Test: user@smartworkz.local | Password: password123 | Role: Viewer';
PRINT '';
PRINT 'API Test Users (additional test users):';
PRINT '  - Manager: manager@smartworkz.test | Password: TestPassword123! | Role: Admin';
PRINT '  - Staff: staff@smartworkz.test | Password: TestPassword123! | Role: Editor';
PRINT '  - Customer: customer@smartworkz.test | Password: TestPassword123! | Role: Viewer';
PRINT '';
PRINT 'Completed: ' + CONVERT(NVARCHAR(30), GETUTCDATE(), 121);
PRINT '============================================';
PRINT '';
