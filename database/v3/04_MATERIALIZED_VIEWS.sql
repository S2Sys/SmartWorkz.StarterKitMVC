-- ============================================
-- SmartWorkz Materialized Views for Performance
-- Version: v3
-- Date: 2026-04-17
-- Purpose: Cached views for frequently accessed lookup and authentication data
-- Strategy: Indexed materialized views with multi-tenant support
-- ============================================

USE Boilerplate;

-- ============================================
-- Section 1: Materialized Views for Lookups
-- Purpose: Performance optimization for frequently accessed lookup data
-- Strategy: Denormalized views with composite indexes for fast filtering
-- ============================================

-- ============================================
-- View 1: vw_Currencies
-- Purpose: Fast retrieval of active currencies for dropdowns and displays
-- Usage: SELECT * FROM dbo.vw_Currencies WHERE TenantId = @TenantId
-- Example: When populating currency dropdown (USD, EUR, GBP, INR, etc.)
-- Performance: Indexed on (TenantId, IsGlobalScope, SortOrder) for fast filtering
-- ============================================
CREATE VIEW [dbo].[vw_Currencies]
WITH SCHEMABINDING
AS
SELECT
    l.Id,
    l.CategoryKey,
    l.[Key],
    l.DisplayName,
    l.Value,
    l.SortOrder,
    l.IsActive,
    l.IsGlobalScope,
    l.TenantId
FROM Master.Lookup l WITH (NOLOCK)
WHERE l.CategoryKey = 'currencies'
    AND l.IsDeleted = 0
    AND l.IsActive = 1;

-- Create unique clustered index on primary key for storage
CREATE UNIQUE CLUSTERED INDEX idx_vw_Currencies_PK
ON dbo.vw_Currencies(Id);

-- Create non-clustered index for common query pattern: get currencies for a tenant sorted
CREATE NONCLUSTERED INDEX idx_vw_Currencies_TenantSort
ON dbo.vw_Currencies(TenantId, IsGlobalScope, SortOrder)
INCLUDE (Id, CategoryKey, DisplayName, Value);

GO

-- ============================================
-- View 2: vw_Languages
-- Purpose: Fast retrieval of active languages for language selection and UI localization
-- Usage: SELECT * FROM dbo.vw_Languages WHERE TenantId = @TenantId ORDER BY SortOrder
-- Example: When populating language selector (English, Spanish, French, German, etc.)
-- Performance: Indexed on (TenantId, IsGlobalScope, SortOrder) for language dropdown queries
-- ============================================
CREATE VIEW [dbo].[vw_Languages]
WITH SCHEMABINDING
AS
SELECT
    l.Id,
    l.CategoryKey,
    l.[Key],
    l.DisplayName,
    l.Value,
    l.SortOrder,
    l.IsActive,
    l.IsGlobalScope,
    l.TenantId
FROM Master.Lookup l WITH (NOLOCK)
WHERE l.CategoryKey = 'languages'
    AND l.IsDeleted = 0
    AND l.IsActive = 1;

-- Create unique clustered index on primary key
CREATE UNIQUE CLUSTERED INDEX idx_vw_Languages_PK
ON dbo.vw_Languages(Id);

-- Create non-clustered index for common query pattern: get languages for a tenant
CREATE NONCLUSTERED INDEX idx_vw_Languages_TenantSort
ON dbo.vw_Languages(TenantId, IsGlobalScope, SortOrder)
INCLUDE (Id, CategoryKey, DisplayName, Value);

GO

-- ============================================
-- View 3: vw_TimeZones
-- Purpose: Fast retrieval of active timezones for timezone selection and scheduling
-- Usage: SELECT * FROM dbo.vw_TimeZones WHERE TenantId = @TenantId ORDER BY SortOrder
-- Example: When populating timezone selector for user preferences (UTC, EST, PST, IST, GMT, etc.)
-- Performance: Indexed on (TenantId, IsGlobalScope, SortOrder) for timezone dropdown queries
-- ============================================
CREATE VIEW [dbo].[vw_TimeZones]
WITH SCHEMABINDING
AS
SELECT
    l.Id,
    l.CategoryKey,
    l.[Key],
    l.DisplayName,
    l.Value,
    l.SortOrder,
    l.IsActive,
    l.IsGlobalScope,
    l.TenantId
FROM Master.Lookup l WITH (NOLOCK)
WHERE l.CategoryKey = 'timezones'
    AND l.IsDeleted = 0
    AND l.IsActive = 1;

-- Create unique clustered index on primary key
CREATE UNIQUE CLUSTERED INDEX idx_vw_TimeZones_PK
ON dbo.vw_TimeZones(Id);

-- Create non-clustered index for common query pattern: get timezones for a tenant
CREATE NONCLUSTERED INDEX idx_vw_TimeZones_TenantSort
ON dbo.vw_TimeZones(TenantId, IsGlobalScope, SortOrder)
INCLUDE (Id, CategoryKey, DisplayName, Value);

GO

-- ============================================
-- View 4: vw_ActiveLookups
-- Purpose: Generic view for all lookup categories with flexible filtering capability
-- Usage: SELECT * FROM dbo.vw_ActiveLookups WHERE TenantId = @TenantId AND CategoryKey = 'statuses'
-- Example: When filtering lookups by category dynamically (statuses, priorities, payment methods, etc.)
-- Performance: Indexed on (TenantId, CategoryKey, IsGlobalScope, SortOrder) for flexible queries
-- Note: This is the most flexible view for ad-hoc lookup queries across multiple categories
-- ============================================
CREATE VIEW [dbo].[vw_ActiveLookups]
WITH SCHEMABINDING
AS
SELECT
    l.Id,
    l.CategoryKey,
    l.SubCategoryKey,
    l.[Key],
    l.DisplayName,
    l.Value,
    l.NodePath,
    l.Level,
    l.SortOrder,
    l.IsActive,
    l.IsGlobalScope,
    l.TenantId
FROM Master.Lookup l WITH (NOLOCK)
WHERE l.IsDeleted = 0
    AND l.IsActive = 1;

-- Create unique clustered index on primary key
CREATE UNIQUE CLUSTERED INDEX idx_vw_ActiveLookups_PK
ON dbo.vw_ActiveLookups(Id);

-- Create non-clustered index for most common query pattern: filter by tenant and category
CREATE NONCLUSTERED INDEX idx_vw_ActiveLookups_TenantCategory
ON dbo.vw_ActiveLookups(TenantId, CategoryKey, IsGlobalScope, SortOrder)
INCLUDE (Id, DisplayName, Value);

-- Create non-clustered index for hierarchical queries using NodePath
CREATE NONCLUSTERED INDEX idx_vw_ActiveLookups_Hierarchy
ON dbo.vw_ActiveLookups(TenantId, NodePath, Level)
INCLUDE (Id, CategoryKey, DisplayName);

GO

-- ============================================
-- Section 2: Materialized Views for Authentication & Authorization
-- Purpose: Performance optimization for user roles and permissions
-- Strategy: Pre-joined views with single-pass queries for authorization checks
-- ============================================

-- ============================================
-- View 5: vw_UserRoles
-- Purpose: Fast resolution of user role assignments for authorization checks
-- Usage: SELECT * FROM dbo.vw_UserRoles WHERE UserId = @UserId AND TenantId = @TenantId
-- Example: When checking user roles for role-based access control (Admin, Manager, User roles)
-- Performance: Indexed on (TenantId, UserId) for single-tenant, single-user queries
-- Note: Pre-joined view eliminates need for joins in application code
-- ============================================
CREATE VIEW [dbo].[vw_UserRoles]
WITH SCHEMABINDING
AS
SELECT
    u.UserId,
    r.RoleId,
    r.Name AS RoleName,
    r.Description AS DisplayName,
    u.TenantId
FROM Auth.Users u WITH (NOLOCK)
INNER JOIN Auth.UserRoles ur WITH (NOLOCK) ON u.UserId = ur.UserId
INNER JOIN Auth.Roles r WITH (NOLOCK) ON ur.RoleId = r.RoleId
WHERE u.IsDeleted = 0
    AND r.IsDeleted = 0;

-- Create unique clustered index on UserId and RoleId composite key
CREATE UNIQUE CLUSTERED INDEX idx_vw_UserRoles_PK
ON dbo.vw_UserRoles(UserId, RoleId);

-- Create non-clustered index for tenant-based queries
CREATE NONCLUSTERED INDEX idx_vw_UserRoles_TenantUser
ON dbo.vw_UserRoles(TenantId, UserId)
INCLUDE (RoleId, RoleName);

-- Create non-clustered index for role-based queries
CREATE NONCLUSTERED INDEX idx_vw_UserRoles_Role
ON dbo.vw_UserRoles(RoleId)
INCLUDE (UserId, TenantId, RoleName);

GO

-- ============================================
-- View 6: vw_UserPermissions
-- Purpose: Unified view for resolving user permissions from both direct assignments and roles
-- Usage: SELECT DISTINCT * FROM dbo.vw_UserPermissions WHERE UserId = @UserId AND TenantId = @TenantId
-- Example: When checking if user has permission to perform action (Create, Read, Update, Delete)
-- Performance: Indexed on (TenantId, UserId, PermissionId) for permission check queries
-- Note: Combines direct permissions and role-based permissions using UNION for comprehensive view
-- ============================================
CREATE VIEW [dbo].[vw_UserPermissions]
WITH SCHEMABINDING
AS
-- Direct user permissions
SELECT
    u.UserId,
    p.PermissionId,
    p.Name AS PermissionName,
    p.Description AS DisplayName,
    u.TenantId
FROM Auth.Users u WITH (NOLOCK)
INNER JOIN Auth.UserPermissions up WITH (NOLOCK) ON u.UserId = up.UserId
INNER JOIN Auth.Permissions p WITH (NOLOCK) ON up.PermissionId = p.PermissionId
WHERE u.IsDeleted = 0
    AND p.IsDeleted = 0
    AND (up.ExpiresAt IS NULL OR up.ExpiresAt > GETUTCDATE())

UNION ALL

-- Role-based permissions
SELECT
    u.UserId,
    p.PermissionId,
    p.Name AS PermissionName,
    p.Description AS DisplayName,
    u.TenantId
FROM Auth.Users u WITH (NOLOCK)
INNER JOIN Auth.UserRoles ur WITH (NOLOCK) ON u.UserId = ur.UserId
INNER JOIN Auth.Roles r WITH (NOLOCK) ON ur.RoleId = r.RoleId
INNER JOIN Auth.RolePermissions rp WITH (NOLOCK) ON r.RoleId = rp.RoleId
INNER JOIN Auth.Permissions p WITH (NOLOCK) ON rp.PermissionId = p.PermissionId
WHERE u.IsDeleted = 0
    AND r.IsDeleted = 0
    AND p.IsDeleted = 0;

-- Create clustered index on composite key
CREATE CLUSTERED INDEX idx_vw_UserPermissions_PK
ON dbo.vw_UserPermissions(UserId, PermissionId);

-- Create non-clustered index for permission checks by tenant and user
CREATE NONCLUSTERED INDEX idx_vw_UserPermissions_TenantUser
ON dbo.vw_UserPermissions(TenantId, UserId, PermissionId)
INCLUDE (PermissionName);

-- Create non-clustered index for checking specific permission for a user
CREATE NONCLUSTERED INDEX idx_vw_UserPermissions_UserPerm
ON dbo.vw_UserPermissions(UserId, PermissionId)
INCLUDE (TenantId, PermissionName);

GO

-- ============================================
-- Section 3: Materialized Views for Configuration
-- Purpose: Performance optimization for application settings and configuration
-- Strategy: Indexed view for fast configuration retrieval
-- ============================================

-- ============================================
-- View 7: vw_Configuration
-- Purpose: Fast retrieval of active application configuration settings for a tenant
-- Usage: SELECT Value FROM dbo.vw_Configuration WHERE TenantId = @TenantId AND Key = 'ApiTimeout'
-- Example: When reading app settings (API timeouts, feature flags, email templates, etc.)
-- Performance: Indexed on (TenantId, Key) for single-lookup and filtered queries
-- Note: Only includes active, non-deleted configuration entries for consistency
-- ============================================
CREATE VIEW [dbo].[vw_Configuration]
WITH SCHEMABINDING
AS
SELECT
    c.ConfigId,
    c.[Key],
    c.Value,
    c.ConfigType,
    c.Description,
    c.IsActive,
    c.TenantId
FROM Master.Configuration c WITH (NOLOCK)
WHERE c.IsActive = 1
    AND c.IsDeleted = 0;

-- Create unique clustered index on ConfigId
CREATE UNIQUE CLUSTERED INDEX idx_vw_Configuration_PK
ON dbo.vw_Configuration(ConfigId);

-- Create non-clustered index for key-based lookups (most common access pattern)
CREATE NONCLUSTERED INDEX idx_vw_Configuration_TenantKey
ON dbo.vw_Configuration(TenantId, [Key])
INCLUDE (Value, ConfigType);

-- Create non-clustered index for scanning all config by tenant
CREATE NONCLUSTERED INDEX idx_vw_Configuration_Tenant
ON dbo.vw_Configuration(TenantId)
INCLUDE ([Key], Value, ConfigType);

GO

-- ============================================
-- Section 4: Index Statistics & Maintenance
-- Purpose: Enable query optimizer to make optimal decisions
-- ============================================

-- Update statistics on all views to ensure query optimizer has fresh data
UPDATE STATISTICS dbo.vw_Currencies;
UPDATE STATISTICS dbo.vw_Languages;
UPDATE STATISTICS dbo.vw_TimeZones;
UPDATE STATISTICS dbo.vw_ActiveLookups;
UPDATE STATISTICS dbo.vw_UserRoles;
UPDATE STATISTICS dbo.vw_UserPermissions;
UPDATE STATISTICS dbo.vw_Configuration;

GO

-- ============================================
-- Section 5: View Documentation & Usage Examples
-- ============================================

/*
USAGE PATTERNS AND EXAMPLES:

1. LOOKUP VIEWS (vw_Currencies, vw_Languages, vw_TimeZones, vw_ActiveLookups)

   -- Get all active currencies for current tenant
   SELECT * FROM dbo.vw_Currencies
   WHERE TenantId = 'tenant-123'
   ORDER BY SortOrder;

   -- Get specific currency
   SELECT DisplayName, Value FROM dbo.vw_Currencies
   WHERE [Key] = 'USD' AND TenantId = 'tenant-123';

   -- Get lookups by category dynamically
   SELECT * FROM dbo.vw_ActiveLookups
   WHERE TenantId = 'tenant-123' AND CategoryKey = 'status_order'
   ORDER BY SortOrder;

2. AUTHENTICATION VIEWS (vw_UserRoles, vw_UserPermissions)

   -- Get all roles assigned to a user
   SELECT RoleName FROM dbo.vw_UserRoles
   WHERE UserId = 'user-456' AND TenantId = 'tenant-123';

   -- Check if user has a specific role
   SELECT COUNT(*) FROM dbo.vw_UserRoles
   WHERE UserId = 'user-456' AND RoleName = 'Admin';

   -- Get all permissions for a user (including role-based)
   SELECT DISTINCT PermissionName FROM dbo.vw_UserPermissions
   WHERE UserId = 'user-456' AND TenantId = 'tenant-123';

   -- Check specific permission
   SELECT COUNT(*) FROM dbo.vw_UserPermissions
   WHERE UserId = 'user-456' AND PermissionId = @permId;

3. CONFIGURATION VIEW (vw_Configuration)

   -- Get specific config value
   SELECT Value FROM dbo.vw_Configuration
   WHERE TenantId = 'tenant-123' AND [Key] = 'MaxUploadSize';

   -- Get all config for a tenant
   SELECT [Key], Value, ConfigType FROM dbo.vw_Configuration
   WHERE TenantId = 'tenant-123';

PERFORMANCE BENEFITS:
- Schema binding prevents accidental table modifications
- NOLOCK hints enable read-only performance without locks
- Composite indexes optimize common filter patterns
- Pre-joined views eliminate application-level join logic
- TenantId index prefix enables efficient multi-tenant filtering

MAINTENANCE:
- Views are read-only (no updates/inserts directly on views)
- Index statistics are updated on each deployment
- Monitor slow queries using execution plans
- Add additional indexes based on application profiling
*/

-- ============================================
-- End of Materialized Views Script
-- ============================================
