-- ============================================
-- SmartWorkz v4: Create Stored Procedures
-- Date: 2026-03-31
-- Purpose: Create all SPs for Dapper data access
-- ============================================

USE Boilerplate;

PRINT 'Creating stored procedures for Dapper data access...'
PRINT ''

-- ============================================
-- AUTH PROCEDURES
-- ============================================
PRINT '🔐 Creating Auth procedures...'

-- 1. GetUserByEmail (for login)
IF OBJECT_ID('Auth.sp_GetUserByEmail', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_GetUserByEmail;
GO

CREATE PROCEDURE Auth.sp_GetUserByEmail
    @Email NVARCHAR(256),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        u.UserId,
        u.Email,
        u.NormalizedEmail,
        u.Username,
        u.NormalizedUsername,
        u.DisplayName,
        u.PasswordHash,
        u.SecurityStamp,
        u.ConcurrencyStamp,
        u.TenantId,
        u.EmailConfirmed,
        u.TwoFactorEnabled,
        u.LockoutEnabled,
        u.LockoutEnd,
        u.AccessFailedCount,
        u.IsActive,
        u.IsDeleted,
        u.CreatedAt,
        u.CreatedBy,
        u.UpdatedAt,
        u.UpdatedBy
    FROM Auth.Users u
    WHERE u.Email = @Email
      AND u.TenantId = @TenantId
      AND u.IsDeleted = 0
END
GO

PRINT '  ✓ sp_GetUserByEmail'

-- 2. GetUserRoles (for user roles)
IF OBJECT_ID('Auth.sp_GetUserRoles', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_GetUserRoles;
GO

CREATE PROCEDURE Auth.sp_GetUserRoles
    @UserId NVARCHAR(36),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        r.Name
    FROM Auth.Roles r
    INNER JOIN Auth.UserRoles ur ON r.RoleId = ur.RoleId
    WHERE ur.UserId = @UserId
      AND r.TenantId = @TenantId
      AND r.IsDeleted = 0
END
GO

PRINT '  ✓ sp_GetUserRoles'

-- 3. GetUserPermissions (for user permissions)
IF OBJECT_ID('Auth.sp_GetUserPermissions', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_GetUserPermissions;
GO

CREATE PROCEDURE Auth.sp_GetUserPermissions
    @UserId NVARCHAR(36),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT DISTINCT
        p.Name
    FROM Auth.Permissions p
    LEFT JOIN Auth.UserPermissions up ON p.PermissionId = up.PermissionId
    LEFT JOIN Auth.RolePermissions rp ON p.PermissionId = rp.PermissionId
    LEFT JOIN Auth.UserRoles ur ON rp.RoleId = ur.RoleId
    WHERE p.TenantId = @TenantId
      AND p.IsDeleted = 0
      AND (
        (up.UserId = @UserId AND up.TenantId = @TenantId)
        OR (ur.UserId = @UserId AND ur.TenantId = @TenantId)
      )
END
GO

PRINT '  ✓ sp_GetUserPermissions'

-- 4. GetRoleWithPermissions
IF OBJECT_ID('Auth.sp_GetRoleWithPermissions', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_GetRoleWithPermissions;
GO

CREATE PROCEDURE Auth.sp_GetRoleWithPermissions
    @RoleId NVARCHAR(36),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        p.PermissionId,
        p.Name,
        p.Description,
        p.PermissionType,
        p.ResourceType,
        p.TenantId,
        p.IsActive,
        p.CreatedAt,
        p.CreatedBy,
        p.UpdatedAt,
        p.UpdatedBy,
        p.IsDeleted
    FROM Auth.Permissions p
    INNER JOIN Auth.RolePermissions rp ON p.PermissionId = rp.PermissionId
    WHERE rp.RoleId = @RoleId
      AND p.TenantId = @TenantId
      AND p.IsDeleted = 0
END
GO

PRINT '  ✓ sp_GetRoleWithPermissions'

-- RefreshToken SPs moved to 011_CreateMissingAuthStoredProcedures.sql

-- ============================================
-- MASTER PROCEDURES
-- ============================================
PRINT ''
PRINT '📚 Creating Master procedures...'

-- 7. GetCategoriesByTenant
IF OBJECT_ID('Master.sp_GetCategoriesByTenant', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_GetCategoriesByTenant;
GO

CREATE PROCEDURE Master.sp_GetCategoriesByTenant
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        CategoryId,
        Name,
        Slug,
        Description,
        NodePath,
        [Level],
        DisplayOrder,
        Icon,
        ImageUrl,
        TenantId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        IsDeleted
    FROM Master.Categories
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
    ORDER BY NodePath
END
GO

PRINT '  ✓ sp_GetCategoriesByTenant'

-- 8. GetMenusByTenant
IF OBJECT_ID('Master.sp_GetMenusByTenant', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_GetMenusByTenant;
GO

CREATE PROCEDURE Master.sp_GetMenusByTenant
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        MenuId,
        Name,
        Description,
        MenuType,
        DisplayOrder,
        TenantId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        IsDeleted
    FROM Master.Menus
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
    ORDER BY DisplayOrder
END
GO

PRINT '  ✓ sp_GetMenusByTenant'

-- 9. GetMenuItemsByMenu
IF OBJECT_ID('Master.sp_GetMenuItemsByMenu', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_GetMenuItemsByMenu;
GO

CREATE PROCEDURE Master.sp_GetMenuItemsByMenu
    @MenuId INT,
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        MenuItemId,
        MenuId,
        Title,
        URL,
        Icon,
        [Level],
        DisplayOrder,
        NodePath,
        RequiredRole,
        TenantId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        IsDeleted
    FROM Master.MenuItems
    WHERE MenuId = @MenuId
      AND TenantId = @TenantId
      AND IsDeleted = 0
    ORDER BY NodePath
END
GO

PRINT '  ✓ sp_GetMenuItemsByMenu'

-- ============================================
-- SHARED PROCEDURES
-- ============================================
PRINT ''
PRINT '🔗 Creating Shared procedures...'

-- 10. GetSeoMetaByEntity
IF OBJECT_ID('Shared.sp_GetSeoMetaByEntity', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_GetSeoMetaByEntity;
GO

CREATE PROCEDURE Shared.sp_GetSeoMetaByEntity
    @EntityType NVARCHAR(50),
    @EntityId INT,
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        SeoMetaId,
        EntityType,
        EntityId,
        Slug,
        Title,
        Description,
        Keywords,
        StructuredData,
        MetaRobots,
        CanonicalUrl,
        OgTitle,
        OgDescription,
        OgImage,
        TwitterCard,
        TwitterImage,
        TenantId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        IsDeleted
    FROM Shared.SeoMeta
    WHERE EntityType = @EntityType
      AND EntityId = @EntityId
      AND TenantId = @TenantId
      AND IsDeleted = 0
END
GO

PRINT '  ✓ sp_GetSeoMetaByEntity'

-- 11. GetSeoMetaBySlug
IF OBJECT_ID('Shared.sp_GetSeoMetaBySlug', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_GetSeoMetaBySlug;
GO

CREATE PROCEDURE Shared.sp_GetSeoMetaBySlug
    @Slug NVARCHAR(256),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        SeoMetaId,
        EntityType,
        EntityId,
        Slug,
        Title,
        Description,
        Keywords,
        StructuredData,
        MetaRobots,
        CanonicalUrl,
        OgTitle,
        OgDescription,
        OgImage,
        TwitterCard,
        TwitterImage,
        TenantId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        IsDeleted
    FROM Shared.SeoMeta
    WHERE Slug = @Slug
      AND TenantId = @TenantId
      AND IsDeleted = 0
END
GO

PRINT '  ✓ sp_GetSeoMetaBySlug'

-- 12. GetTagsByEntity
IF OBJECT_ID('Shared.sp_GetTagsByEntity', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_GetTagsByEntity;
GO

CREATE PROCEDURE Shared.sp_GetTagsByEntity
    @EntityType NVARCHAR(50),
    @EntityId INT,
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        TagId,
        EntityType,
        EntityId,
        TagName,
        TagCategory,
        TenantId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        IsDeleted
    FROM Shared.Tags
    WHERE EntityType = @EntityType
      AND EntityId = @EntityId
      AND TenantId = @TenantId
      AND IsDeleted = 0
    ORDER BY TagName
END
GO

PRINT '  ✓ sp_GetTagsByEntity'

-- ============================================
-- Summary
-- ============================================
PRINT ''
PRINT '✅ All stored procedures created successfully!'
PRINT ''
PRINT 'Summary:'
PRINT '  Auth procedures: 6'
PRINT '  Master procedures: 3'
PRINT '  Shared procedures: 3'
PRINT '  Total: 12 stored procedures'
PRINT ''
PRINT 'All procedures are now ready for Dapper data access'
PRINT ''
