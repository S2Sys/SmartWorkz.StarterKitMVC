-- ============================================
-- SmartWorkz v4 Phase 1: Master Schema Tables
-- Date: 2026-03-31
-- 14 Tables: Core boilerplate - Configuration, Navigation, Localization, CMS, Multi-tenancy
-- ============================================

USE Boilerplate;

-- ============================================
-- 1. Tenants (Multi-tenancy support)
-- ============================================
CREATE TABLE Master.Tenants (
    TenantId NVARCHAR(128) PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    DisplayName NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_Tenants_IsActive ON Master.Tenants(IsActive);

-- ============================================
-- 2. Countries (Lookup)
-- ============================================
CREATE TABLE Master.Countries (
    CountryId INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(2) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    FlagEmoji NVARCHAR(10),
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_Countries_Code ON Master.Countries(Code);
CREATE INDEX IX_Countries_TenantId ON Master.Countries(TenantId);

-- ============================================
-- 3. Currencies (Lookup)
-- ============================================
CREATE TABLE Master.Currencies (
    CurrencyId INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(3) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    Symbol NVARCHAR(10),
    DecimalPlaces INT NOT NULL DEFAULT 2,
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_Currencies_Code ON Master.Currencies(Code);
CREATE INDEX IX_Currencies_TenantId ON Master.Currencies(TenantId);

-- ============================================
-- 4. Languages (Lookup)
-- ============================================
CREATE TABLE Master.Languages (
    LanguageId INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(5) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    NativeName NVARCHAR(100),
    IsDefault BIT NOT NULL DEFAULT 0,
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_Languages_Code ON Master.Languages(Code);
CREATE INDEX IX_Languages_TenantId ON Master.Languages(TenantId);

-- ============================================
-- 5. TimeZones (Lookup)
-- ============================================
CREATE TABLE Master.TimeZones (
    TimeZoneId INT PRIMARY KEY IDENTITY(1,1),
    Identifier NVARCHAR(100) NOT NULL UNIQUE,
    DisplayName NVARCHAR(200) NOT NULL,
    StandardName NVARCHAR(200),
    DaylightName NVARCHAR(200),
    OffsetHours DECIMAL(4,2),
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

-- ============================================
-- 6. Configuration (App Settings)
-- ============================================
CREATE TABLE Master.Configuration (
    ConfigId INT PRIMARY KEY IDENTITY(1,1),
    [Key] NVARCHAR(256) NOT NULL,
    Value NVARCHAR(MAX),
    ConfigType NVARCHAR(50), -- String, Int, Bool, Decimal, Json
    Description NVARCHAR(500),
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, [Key])
);

CREATE INDEX IX_Configuration_Key ON Master.Configuration([Key]);
CREATE INDEX IX_Configuration_TenantId ON Master.Configuration(TenantId);

-- ============================================
-- 7. FeatureFlags (Feature Management)
-- ============================================
CREATE TABLE Master.FeatureFlags (
    FeatureFlagId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    IsEnabled BIT NOT NULL DEFAULT 0,
    TenantId NVARCHAR(128),
    ValidFrom DATETIME2,
    ValidTo DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_FeatureFlags_Name ON Master.FeatureFlags(Name);
CREATE INDEX IX_FeatureFlags_TenantId ON Master.FeatureFlags(TenantId);

-- ============================================
-- 8. Menus (Navigation Hierarchy)
-- ============================================
CREATE TABLE Master.Menus (
    MenuId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    MenuType NVARCHAR(50), -- Main, Footer, Sidebar, Admin
    DisplayOrder INT NOT NULL DEFAULT 0,
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, Name)
);

CREATE INDEX IX_Menus_MenuType ON Master.Menus(MenuType);
CREATE INDEX IX_Menus_TenantId ON Master.Menus(TenantId);

-- ============================================
-- 9. MenuItems (Hierarchical Menu Items)
-- ============================================
CREATE TABLE Master.MenuItems (
    MenuItemId INT PRIMARY KEY IDENTITY(1,1),
    MenuId INT NOT NULL,
    ParentMenuItemId INT,
    Title NVARCHAR(256) NOT NULL,
    URL NVARCHAR(500),
    Icon NVARCHAR(100),
    DisplayOrder INT NOT NULL DEFAULT 0,
    NodePath HIERARCHYID, -- For unlimited nesting: /1/, /1/1/, /1/1/1/
    Level AS (NodePath.GetLevel()) PERSISTED,
    RequiredRole NVARCHAR(256), -- Role-based visibility
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (MenuId) REFERENCES Master.Menus(MenuId),
    FOREIGN KEY (ParentMenuItemId) REFERENCES Master.MenuItems(MenuItemId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_MenuItems_MenuId ON Master.MenuItems(MenuId);
CREATE INDEX IX_MenuItems_ParentMenuItemId ON Master.MenuItems(ParentMenuItemId);
CREATE INDEX IX_MenuItems_NodePath ON Master.MenuItems(NodePath);
CREATE INDEX IX_MenuItems_TenantId ON Master.MenuItems(TenantId);
CREATE INDEX IX_MenuItems_Level ON Master.MenuItems(Level);

-- ============================================
-- 10. GeoHierarchy (Geographic Hierarchy)
-- ============================================
CREATE TABLE Master.GeoHierarchy (
    GeoId INT PRIMARY KEY IDENTITY(1,1),
    ParentGeoId INT,
    Name NVARCHAR(256) NOT NULL,
    GeoType NVARCHAR(50), -- Continent, Country, Region, City
    NodePath HIERARCHYID,
    Level AS (NodePath.GetLevel()) PERSISTED,
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ParentGeoId) REFERENCES Master.GeoHierarchy(GeoId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_GeoHierarchy_ParentGeoId ON Master.GeoHierarchy(ParentGeoId);
CREATE INDEX IX_GeoHierarchy_NodePath ON Master.GeoHierarchy(NodePath);
CREATE INDEX IX_GeoHierarchy_TenantId ON Master.GeoHierarchy(TenantId);

-- ============================================
-- 11. GeolocationPages (Regional Content)
-- ============================================
CREATE TABLE Master.GeolocationPages (
    GeoPageId INT PRIMARY KEY IDENTITY(1,1),
    GeoId INT NOT NULL,
    Title NVARCHAR(256) NOT NULL,
    Slug NVARCHAR(256) NOT NULL,
    Content NVARCHAR(MAX),
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (GeoId) REFERENCES Master.GeoHierarchy(GeoId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, Slug)
);

CREATE INDEX IX_GeolocationPages_GeoId ON Master.GeolocationPages(GeoId);
CREATE INDEX IX_GeolocationPages_Slug ON Master.GeolocationPages(Slug);
CREATE INDEX IX_GeolocationPages_TenantId ON Master.GeolocationPages(TenantId);

-- ============================================
-- 12. CustomPages (CMS Pages)
-- ============================================
CREATE TABLE Master.CustomPages (
    PageId INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(256) NOT NULL,
    Slug NVARCHAR(256) NOT NULL,
    Content NVARCHAR(MAX),
    MetaDescription NVARCHAR(500),
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, Slug)
);

CREATE INDEX IX_CustomPages_Slug ON Master.CustomPages(Slug);
CREATE INDEX IX_CustomPages_TenantId ON Master.CustomPages(TenantId);

-- ============================================
-- 13. BlogPosts (Blog Content)
-- ============================================
CREATE TABLE Master.BlogPosts (
    PostId INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(256) NOT NULL,
    Slug NVARCHAR(256) NOT NULL,
    Content NVARCHAR(MAX),
    Author NVARCHAR(256),
    PublishedAt DATETIME2,
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, Slug)
);

CREATE INDEX IX_BlogPosts_Slug ON Master.BlogPosts(Slug);
CREATE INDEX IX_BlogPosts_PublishedAt ON Master.BlogPosts(PublishedAt);
CREATE INDEX IX_BlogPosts_TenantId ON Master.BlogPosts(TenantId);

-- ============================================
-- 14. TenantUsers (User to Tenant Mapping)
-- ============================================
CREATE TABLE Master.TenantUsers (
    TenantUserId INT PRIMARY KEY IDENTITY(1,1),
    TenantId NVARCHAR(128) NOT NULL,
    UserId NVARCHAR(128) NOT NULL,
    JoinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LeftAt DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, UserId)
);

CREATE INDEX IX_TenantUsers_TenantId ON Master.TenantUsers(TenantId);
CREATE INDEX IX_TenantUsers_UserId ON Master.TenantUsers(UserId);

PRINT '✓ Master schema: 14 tables created successfully'

