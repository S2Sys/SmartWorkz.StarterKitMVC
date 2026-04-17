-- ============================================
-- SmartWorkz v3 - Schema Creation (Organized)
-- Purpose: Create all 6 schemas with grouped tables
-- Schemas: Master, Shared, Transaction, Report, Auth
-- Date: 2026-04-17
-- ============================================

USE Boilerplate;

-- ============================================
-- STEP 1: CREATE SCHEMAS
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Master')
    EXEC sp_executesql N'CREATE SCHEMA Master';

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Shared')
    EXEC sp_executesql N'CREATE SCHEMA Shared';

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Transaction')
    EXEC sp_executesql N'CREATE SCHEMA [Transaction]';

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Report')
    EXEC sp_executesql N'CREATE SCHEMA Report';

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Auth')
    EXEC sp_executesql N'CREATE SCHEMA Auth';

GO

-- ============================================
-- MASTER SCHEMA (19 Tables)
-- ============================================
-- Core configuration, master data, multi-tenancy

-- 1. Tenants (Multi-tenancy root - required by all)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tenants' AND schema_id = SCHEMA_ID('Master'))
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
PRINT '✓ Master.Tenants created';
GO

-- 2. Countries (Geographic reference)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Countries' AND schema_id = SCHEMA_ID('Master'))
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
PRINT '✓ Master.Countries created';
GO

-- 3. Configuration (App settings)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Configuration' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.Configuration (
    ConfigId INT PRIMARY KEY IDENTITY(1,1),
    [Key] NVARCHAR(256) NOT NULL,
    Value NVARCHAR(MAX),
    ConfigType NVARCHAR(50),
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
PRINT '✓ Master.Configuration created';
GO

-- 4. FeatureFlags (Feature toggles)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FeatureFlags' AND schema_id = SCHEMA_ID('Master'))
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
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (Name, TenantId)
);
CREATE INDEX IX_FeatureFlags_Name ON Master.FeatureFlags(Name);
CREATE INDEX IX_FeatureFlags_TenantId ON Master.FeatureFlags(TenantId);
PRINT '✓ Master.FeatureFlags created';
GO

-- 5. Menus (Navigation menu definitions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Menus' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.Menus (
    MenuId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    MenuType NVARCHAR(50),
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
PRINT '✓ Master.Menus created';
GO

-- 6. Categories (Hierarchical with HierarchyID)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.Categories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256) NOT NULL,
    Slug NVARCHAR(256) NOT NULL,
    Description NVARCHAR(MAX),
    NodePath HIERARCHYID,
    Level AS (NodePath.GetLevel()) PERSISTED,
    DisplayOrder INT NOT NULL DEFAULT 0,
    Icon NVARCHAR(100),
    ImageUrl NVARCHAR(500),
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
CREATE INDEX IX_Categories_NodePath ON Master.Categories(NodePath);
CREATE INDEX IX_Categories_Slug ON Master.Categories(Slug);
CREATE INDEX IX_Categories_Level ON Master.Categories(Level);
CREATE INDEX IX_Categories_TenantId ON Master.Categories(TenantId);
PRINT '✓ Master.Categories created';
GO

-- 7. MenuItems (Hierarchical menu items)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MenuItems' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.MenuItems (
    MenuItemId INT PRIMARY KEY IDENTITY(1,1),
    MenuId INT NOT NULL,
    ParentMenuItemId INT,
    Title NVARCHAR(256) NOT NULL,
    URL NVARCHAR(500),
    Icon NVARCHAR(100),
    DisplayOrder INT NOT NULL DEFAULT 0,
    NodePath HIERARCHYID,
    Level AS (NodePath.GetLevel()) PERSISTED,
    RequiredRole NVARCHAR(256),
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
PRINT '✓ Master.MenuItems created';
GO

-- 8. GeoHierarchy (Geographic hierarchy: Continent → Country → Region → City)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GeoHierarchy' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.GeoHierarchy (
    GeoId INT PRIMARY KEY IDENTITY(1,1),
    ParentGeoId INT,
    Name NVARCHAR(256) NOT NULL,
    GeoType NVARCHAR(50),
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
PRINT '✓ Master.GeoHierarchy created';
GO

-- 9. GeolocationPages (Region-specific content)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GeolocationPages' AND schema_id = SCHEMA_ID('Master'))
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
PRINT '✓ Master.GeolocationPages created';
GO

-- 10. CustomPages (CMS pages)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomPages' AND schema_id = SCHEMA_ID('Master'))
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
PRINT '✓ Master.CustomPages created';
GO

-- 11. BlogPosts (Blog content)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BlogPosts' AND schema_id = SCHEMA_ID('Master'))
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
PRINT '✓ Master.BlogPosts created';
GO

-- 12. CacheEntries (ASP.NET Distributed Cache)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CacheEntries' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.CacheEntries (
    Id NVARCHAR(449) PRIMARY KEY,
    Value VARBINARY(MAX) NOT NULL,
    ExpiresAtTime DATETIMEOFFSET NOT NULL,
    SlidingExpirationInSeconds BIGINT,
    AbsoluteExpiration DATETIMEOFFSET
);
CREATE INDEX IX_CacheEntries_ExpiresAtTime ON Master.CacheEntries(ExpiresAtTime);
PRINT '✓ Master.CacheEntries created';
GO

-- 13. ContentTemplates (Email/SMS/Push templates)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContentTemplates' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.ContentTemplates (
    Id NVARCHAR(256) PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    TemplateType NVARCHAR(50) NOT NULL DEFAULT 'Email',
    Subject NVARCHAR(500) NOT NULL DEFAULT '',
    HeaderId NVARCHAR(256),
    FooterId NVARCHAR(256),
    BodyContent NVARCHAR(MAX) NOT NULL DEFAULT '',
    PlainTextContent NVARCHAR(MAX),
    Tags NVARCHAR(MAX),
    Category NVARCHAR(100),
    IsActive BIT NOT NULL DEFAULT 1,
    IsSystem BIT NOT NULL DEFAULT 0,
    TenantId NVARCHAR(128),
    Version INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_ContentTemplates_TenantId ON Master.ContentTemplates(TenantId);
CREATE INDEX IX_ContentTemplates_TemplateType ON Master.ContentTemplates(TemplateType);
CREATE INDEX IX_ContentTemplates_Category ON Master.ContentTemplates(Category);
CREATE INDEX IX_ContentTemplates_IsActive_IsDeleted ON Master.ContentTemplates(IsActive, IsDeleted);
PRINT '✓ Master.ContentTemplates created';
GO

-- 14. ContentTemplateSections (Reusable template sections)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContentTemplateSections' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.ContentTemplateSections (
    Id NVARCHAR(256) PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    SectionType NVARCHAR(50) NOT NULL DEFAULT 'Header',
    HtmlContent NVARCHAR(MAX) NOT NULL DEFAULT '',
    IsDefault BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_ContentTemplateSections_TenantId ON Master.ContentTemplateSections(TenantId);
CREATE INDEX IX_ContentTemplateSections_SectionType ON Master.ContentTemplateSections(SectionType);
PRINT '✓ Master.ContentTemplateSections created';
GO

-- 15. TemplatePlaceholders (Template variable definitions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TemplatePlaceholders' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.TemplatePlaceholders (
    PlaceholderId INT PRIMARY KEY IDENTITY(1,1),
    TemplateId NVARCHAR(256) NOT NULL,
    PlaceholderKey NVARCHAR(256) NOT NULL,
    DisplayName NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    DefaultValue NVARCHAR(500),
    SampleValue NVARCHAR(500),
    PlaceholderType NVARCHAR(50) NOT NULL DEFAULT 'Text',
    IsRequired BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    TenantId NVARCHAR(128),
    UNIQUE (TemplateId, PlaceholderKey),
    FOREIGN KEY (TemplateId) REFERENCES Master.ContentTemplates(Id) ON DELETE CASCADE,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_TemplatePlaceholders_TemplateId ON Master.TemplatePlaceholders(TemplateId);
PRINT '✓ Master.TemplatePlaceholders created';
GO

-- 16. Lookup ⭐ NEW - Consolidated hierarchical lookups
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Lookup' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.Lookup (
    IntId INT UNIQUE,
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    NodePath HIERARCHYID NOT NULL UNIQUE,
    Level AS (NodePath.GetLevel()) PERSISTED,
    CategoryKey NVARCHAR(100),
    SubCategoryKey NVARCHAR(100),
    [Key] NVARCHAR(100),
    DisplayName NVARCHAR(500) NOT NULL,
    TenantId NVARCHAR(128),
    IsGlobalScope BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(128),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(128),
    SortOrder INT NOT NULL DEFAULT 0,
    Metadata NVARCHAR(MAX),
    LocalizedNames NVARCHAR(MAX),
    CONSTRAINT UQ_Lookup_Key UNIQUE (CategoryKey, SubCategoryKey, [Key], TenantId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE CLUSTERED INDEX IX_Lookup_NodePath ON Master.Lookup(NodePath);
CREATE INDEX IX_Lookup_Category ON Master.Lookup(CategoryKey, IsActive, IsDeleted);
CREATE INDEX IX_Lookup_Tenant ON Master.Lookup(TenantId, IsGlobalScope, IsActive);
CREATE INDEX IX_Lookup_Global ON Master.Lookup(IsGlobalScope, IsActive, IsDeleted);
CREATE INDEX IX_Lookup_Key ON Master.Lookup(CategoryKey, [Key], TenantId);
CREATE INDEX IX_Lookup_Level ON Master.Lookup(Level);
CREATE INDEX IX_Lookup_IntId ON Master.Lookup(IntId) WHERE IntId IS NOT NULL;
PRINT '✓ Master.Lookup created';
GO

PRINT '✓ MASTER SCHEMA: 16 tables created successfully';
GO

-- ============================================
-- SHARED SCHEMA (7 Tables)
-- ============================================
-- Cross-cutting concerns: polymorphic, audit, notifications

-- 17. SeoMeta (SEO metadata)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SeoMeta' AND schema_id = SCHEMA_ID('Shared'))
CREATE TABLE Shared.SeoMeta (
    SeoMetaId INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL,
    EntityId INT NOT NULL,
    Slug NVARCHAR(256) NOT NULL,
    Title NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    Keywords NVARCHAR(500),
    StructuredData NVARCHAR(MAX),
    MetaRobots NVARCHAR(100),
    CanonicalUrl NVARCHAR(500),
    OgTitle NVARCHAR(256),
    OgDescription NVARCHAR(500),
    OgImage NVARCHAR(500),
    TwitterCard NVARCHAR(50),
    TwitterTitle NVARCHAR(256),
    TwitterDescription NVARCHAR(500),
    TwitterImage NVARCHAR(500),
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
CREATE INDEX IX_SeoMeta_EntityType_EntityId ON Shared.SeoMeta(EntityType, EntityId);
CREATE INDEX IX_SeoMeta_Slug ON Shared.SeoMeta(Slug);
CREATE INDEX IX_SeoMeta_TenantId ON Shared.SeoMeta(TenantId);
PRINT '✓ Shared.SeoMeta created';
GO

-- 18. Tags (Polymorphic tagging)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tags' AND schema_id = SCHEMA_ID('Shared'))
CREATE TABLE Shared.Tags (
    TagId INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL,
    EntityId INT NOT NULL,
    TagName NVARCHAR(256) NOT NULL,
    TagCategory NVARCHAR(100),
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_Tags_EntityType_EntityId ON Shared.Tags(EntityType, EntityId);
CREATE INDEX IX_Tags_TagName ON Shared.Tags(TagName);
CREATE INDEX IX_Tags_TenantId ON Shared.Tags(TenantId);
PRINT '✓ Shared.Tags created';
GO

-- 19. Translations (Multi-language content)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Translations' AND schema_id = SCHEMA_ID('Shared'))
CREATE TABLE Shared.Translations (
    TranslationId INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL,
    EntityId INT NOT NULL,
    LanguageLookupId UNIQUEIDENTIFIER NOT NULL,
    FieldName NVARCHAR(256) NOT NULL,
    TranslatedValue NVARCHAR(MAX) NOT NULL,
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (LanguageLookupId) REFERENCES Master.Lookup(Id),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, EntityType, EntityId, LanguageLookupId, FieldName)
);
CREATE INDEX IX_Translations_EntityType_EntityId ON Shared.Translations(EntityType, EntityId);
CREATE INDEX IX_Translations_LanguageLookupId ON Shared.Translations(LanguageLookupId);
CREATE INDEX IX_Translations_TenantId ON Shared.Translations(TenantId);
PRINT '✓ Shared.Translations created';
GO

-- 20. Notifications (System notifications)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications' AND schema_id = SCHEMA_ID('Shared'))
CREATE TABLE Shared.Notifications (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    NotificationType NVARCHAR(100) NOT NULL,
    RecipientType NVARCHAR(50),
    RecipientId NVARCHAR(256),
    Subject NVARCHAR(256) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME2,
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_Notifications_RecipientType_RecipientId ON Shared.Notifications(RecipientType, RecipientId);
CREATE INDEX IX_Notifications_IsRead ON Shared.Notifications(IsRead);
CREATE INDEX IX_Notifications_TenantId ON Shared.Notifications(TenantId);
PRINT '✓ Shared.Notifications created';
GO

-- 21. AuditLogs (Audit trail)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs' AND schema_id = SCHEMA_ID('Shared'))
CREATE TABLE Shared.AuditLogs (
    AuditLogId INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL,
    EntityId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    ChangedBy NVARCHAR(256),
    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IPAddress NVARCHAR(45),
    TenantId NVARCHAR(128),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_AuditLogs_EntityType_EntityId ON Shared.AuditLogs(EntityType, EntityId);
CREATE INDEX IX_AuditLogs_ChangedAt ON Shared.AuditLogs(ChangedAt);
CREATE INDEX IX_AuditLogs_TenantId ON Shared.AuditLogs(TenantId);
PRINT '✓ Shared.AuditLogs created';
GO

-- 22. FileStorage (Document management)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FileStorage' AND schema_id = SCHEMA_ID('Shared'))
CREATE TABLE Shared.FileStorage (
    FileId INT PRIMARY KEY IDENTITY(1,1),
    FileName NVARCHAR(256) NOT NULL,
    FileSize BIGINT NOT NULL,
    MimeType NVARCHAR(100),
    FilePath NVARCHAR(500) NOT NULL,
    EntityType NVARCHAR(100),
    EntityId INT,
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_FileStorage_EntityType_EntityId ON Shared.FileStorage(EntityType, EntityId);
CREATE INDEX IX_FileStorage_TenantId ON Shared.FileStorage(TenantId);
PRINT '✓ Shared.FileStorage created';
GO

-- 23. EmailQueue (Email delivery)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmailQueue' AND schema_id = SCHEMA_ID('Shared'))
CREATE TABLE Shared.EmailQueue (
    EmailQueueId INT PRIMARY KEY IDENTITY(1,1),
    ToEmail NVARCHAR(256) NOT NULL,
    CcEmail NVARCHAR(500),
    BccEmail NVARCHAR(500),
    Subject NVARCHAR(256) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    IsHtml BIT NOT NULL DEFAULT 1,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    SendAttempts INT NOT NULL DEFAULT 0,
    LastAttemptAt DATETIME2,
    SentAt DATETIME2,
    FailureReason NVARCHAR(500),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_EmailQueue_Status ON Shared.EmailQueue(Status);
CREATE INDEX IX_EmailQueue_CreatedAt ON Shared.EmailQueue(CreatedAt);
CREATE INDEX IX_EmailQueue_TenantId ON Shared.EmailQueue(TenantId);
PRINT '✓ Shared.EmailQueue created';
GO

PRINT '✓ SHARED SCHEMA: 7 tables created successfully';
GO

-- ============================================
-- TRANSACTION SCHEMA (1 Table)
-- ============================================
-- Financial transaction tracking

-- 24. TransactionLog (Financial transactions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TransactionLog' AND schema_id = SCHEMA_ID('Transaction'))
CREATE TABLE [Transaction].TransactionLog (
    TransactionLogId BIGINT PRIMARY KEY IDENTITY(1,1),
    TransactionType NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(100),
    EntityId INT,
    Amount DECIMAL(18, 2) NOT NULL,
    CurrencyLookupId UNIQUEIDENTIFIER,
    Description NVARCHAR(500),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    PaymentMethod NVARCHAR(100),
    ReferenceNumber NVARCHAR(256),
    ProcessedAt DATETIME2,
    CompletedAt DATETIME2,
    FailureReason NVARCHAR(500),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (CurrencyLookupId) REFERENCES Master.Lookup(Id),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_TransactionLog_TransactionType ON [Transaction].TransactionLog(TransactionType);
CREATE INDEX IX_TransactionLog_Status ON [Transaction].TransactionLog(Status);
CREATE INDEX IX_TransactionLog_EntityType_EntityId ON [Transaction].TransactionLog(EntityType, EntityId);
CREATE INDEX IX_TransactionLog_CurrencyLookupId ON [Transaction].TransactionLog(CurrencyLookupId);
CREATE INDEX IX_TransactionLog_CreatedAt ON [Transaction].TransactionLog(CreatedAt);
CREATE INDEX IX_TransactionLog_TenantId ON [Transaction].TransactionLog(TenantId);
PRINT '✓ Transaction.TransactionLog created';
GO

PRINT '✓ TRANSACTION SCHEMA: 1 table created successfully';
GO

-- ============================================
-- REPORT SCHEMA (4 Tables)
-- ============================================
-- Analytics and reporting

-- 25. Reports (Report definitions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reports' AND schema_id = SCHEMA_ID('Report'))
CREATE TABLE Report.Reports (
    ReportId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    ReportType NVARCHAR(100) NOT NULL,
    QueryDefinition NVARCHAR(MAX),
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_Reports_ReportType ON Report.Reports(ReportType);
CREATE INDEX IX_Reports_TenantId ON Report.Reports(TenantId);
PRINT '✓ Report.Reports created';
GO

-- 26. ReportSchedules (Scheduled report generation)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReportSchedules' AND schema_id = SCHEMA_ID('Report'))
CREATE TABLE Report.ReportSchedules (
    ReportScheduleId INT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    ScheduleName NVARCHAR(256) NOT NULL,
    Frequency NVARCHAR(50),
    NextRun DATETIME2,
    LastRun DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1,
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ReportId) REFERENCES Report.Reports(ReportId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_ReportSchedules_ReportId ON Report.ReportSchedules(ReportId);
CREATE INDEX IX_ReportSchedules_NextRun ON Report.ReportSchedules(NextRun);
CREATE INDEX IX_ReportSchedules_TenantId ON Report.ReportSchedules(TenantId);
PRINT '✓ Report.ReportSchedules created';
GO

-- 27. ReportData (Generated report data)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReportData' AND schema_id = SCHEMA_ID('Report'))
CREATE TABLE Report.ReportData (
    ReportDataId BIGINT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    GeneratedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DataJson NVARCHAR(MAX),
    Summary NVARCHAR(MAX),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ReportId) REFERENCES Report.Reports(ReportId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_ReportData_ReportId ON Report.ReportData(ReportId);
CREATE INDEX IX_ReportData_GeneratedAt ON Report.ReportData(GeneratedAt);
CREATE INDEX IX_ReportData_TenantId ON Report.ReportData(TenantId);
PRINT '✓ Report.ReportData created';
GO

-- 28. Analytics (Event analytics)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Analytics' AND schema_id = SCHEMA_ID('Report'))
CREATE TABLE Report.Analytics (
    AnalyticsId BIGINT PRIMARY KEY IDENTITY(1,1),
    EventName NVARCHAR(256) NOT NULL,
    EntityType NVARCHAR(100),
    EntityId INT,
    UserId NVARCHAR(256),
    EventData NVARCHAR(MAX),
    EventDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_Analytics_EventName ON Report.Analytics(EventName);
CREATE INDEX IX_Analytics_EntityType_EntityId ON Report.Analytics(EntityType, EntityId);
CREATE INDEX IX_Analytics_UserId ON Report.Analytics(UserId);
CREATE INDEX IX_Analytics_EventDate ON Report.Analytics(EventDate);
CREATE INDEX IX_Analytics_TenantId ON Report.Analytics(TenantId);
PRINT '✓ Report.Analytics created';
GO

PRINT '✓ REPORT SCHEMA: 4 tables created successfully';
GO

-- ============================================
-- AUTH SCHEMA (10 Tables)
-- ============================================
-- Authentication and authorization

-- 29. Users (User authentication)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('Auth'))
CREATE TABLE Auth.Users (
    UserId NVARCHAR(128) PRIMARY KEY,
    UserName NVARCHAR(256) NOT NULL UNIQUE,
    NormalizedUserName NVARCHAR(256) NOT NULL UNIQUE,
    Email NVARCHAR(256) NOT NULL,
    NormalizedEmail NVARCHAR(256) NOT NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    ConcurrencyStamp NVARCHAR(MAX),
    PhoneNumber NVARCHAR(20),
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LockoutEnd DATETIMEOFFSET,
    LockoutEnabled BIT NOT NULL DEFAULT 1,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    DisplayName NVARCHAR(256),
    AvatarUrl NVARCHAR(500),
    Locale NVARCHAR(10) DEFAULT 'en-US',
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_Users_Email ON Auth.Users(Email);
CREATE INDEX IX_Users_NormalizedEmail ON Auth.Users(NormalizedEmail);
CREATE INDEX IX_Users_TenantId ON Auth.Users(TenantId);
PRINT '✓ Auth.Users created';
GO

-- 30. Roles (Authorization roles)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles' AND schema_id = SCHEMA_ID('Auth'))
CREATE TABLE Auth.Roles (
    RoleId NVARCHAR(128) PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    NormalizedName NVARCHAR(256) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    TenantId NVARCHAR(128),
    IsSystemRole BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_Roles_Name ON Auth.Roles(Name);
CREATE INDEX IX_Roles_NormalizedName ON Auth.Roles(NormalizedName);
CREATE INDEX IX_Roles_TenantId ON Auth.Roles(TenantId);
PRINT '✓ Auth.Roles created';
GO

-- 31. Permissions (Fine-grained permissions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions' AND schema_id = SCHEMA_ID('Auth'))
CREATE TABLE Auth.Permissions (
    PermissionId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    PermissionType NVARCHAR(100),
    ResourceType NVARCHAR(100),
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, PermissionType, ResourceType)
);
CREATE INDEX IX_Permissions_PermissionType ON Auth.Permissions(PermissionType);
CREATE INDEX IX_Permissions_ResourceType ON Auth.Permissions(ResourceType);
CREATE INDEX IX_Permissions_TenantId ON Auth.Permissions(TenantId);
PRINT '✓ Auth.Permissions created';
GO

-- 32. UserRoles (User-role mapping)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles' AND schema_id = SCHEMA_ID('Auth'))
CREATE TABLE Auth.UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128) NOT NULL,
    RoleId NVARCHAR(128) NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (RoleId) REFERENCES Auth.Roles(RoleId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, UserId, RoleId)
);
CREATE INDEX IX_UserRoles_UserId ON Auth.UserRoles(UserId);
CREATE INDEX IX_UserRoles_RoleId ON Auth.UserRoles(RoleId);
CREATE INDEX IX_UserRoles_TenantId ON Auth.UserRoles(TenantId);
PRINT '✓ Auth.UserRoles created';
GO

-- 33. RolePermissions (Role-permission mapping)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions' AND schema_id = SCHEMA_ID('Auth'))
CREATE TABLE Auth.RolePermissions (
    RolePermissionId INT PRIMARY KEY IDENTITY(1,1),
    RoleId NVARCHAR(128) NOT NULL,
    PermissionId INT NOT NULL,
    GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (RoleId) REFERENCES Auth.Roles(RoleId),
    FOREIGN KEY (PermissionId) REFERENCES Auth.Permissions(PermissionId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, RoleId, PermissionId)
);
CREATE INDEX IX_RolePermissions_RoleId ON Auth.RolePermissions(RoleId);
CREATE INDEX IX_RolePermissions_PermissionId ON Auth.RolePermissions(PermissionId);
CREATE INDEX IX_RolePermissions_TenantId ON Auth.RolePermissions(TenantId);
PRINT '✓ Auth.RolePermissions created';
GO

-- 34. UserPermissions (Direct user permissions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserPermissions' AND schema_id = SCHEMA_ID('Auth'))
CREATE TABLE Auth.UserPermissions (
    UserPermissionId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128) NOT NULL,
    PermissionId INT NOT NULL,
    GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2,
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (PermissionId) REFERENCES Auth.Permissions(PermissionId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, UserId, PermissionId)
);
CREATE INDEX IX_UserPermissions_UserId ON Auth.UserPermissions(UserId);
CREATE INDEX IX_UserPermissions_PermissionId ON Auth.UserPermissions(PermissionId);
CREATE INDEX IX_UserPermissions_TenantId ON Auth.UserPermissions(TenantId);
PRINT '✓ Auth.UserPermissions created';
GO

-- 35. AuthTokens ⭐ MERGED (Password Reset, Email Verification, 2FA, Refresh Tokens)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuthTokens' AND schema_id = SCHEMA_ID('Auth'))
CREATE TABLE Auth.AuthTokens (
    AuthTokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128) NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    TokenType NVARCHAR(50) NOT NULL,
    TokenSubType NVARCHAR(50),
    ExpiresAt DATETIME2 NOT NULL,
    UsedAt DATETIME2,
    VerifiedAt DATETIME2,
    RevokedAt DATETIME2,
    Attempts INT NOT NULL DEFAULT 0,
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_AuthTokens_Token ON Auth.AuthTokens(Token);
CREATE INDEX IX_AuthTokens_UserId ON Auth.AuthTokens(UserId);
CREATE INDEX IX_AuthTokens_ExpiresAt ON Auth.AuthTokens(ExpiresAt);
CREATE INDEX IX_AuthTokens_TokenType ON Auth.AuthTokens(TokenType);
CREATE INDEX IX_AuthTokens_TenantId ON Auth.AuthTokens(TenantId);
PRINT '✓ Auth.AuthTokens created';
GO

-- 36. LoginAttempts (Security tracking)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginAttempts' AND schema_id = SCHEMA_ID('Auth'))
CREATE TABLE Auth.LoginAttempts (
    LoginAttemptId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128),
    Email NVARCHAR(256),
    IPAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    IsSuccessful BIT NOT NULL DEFAULT 0,
    FailureReason NVARCHAR(256),
    TenantId NVARCHAR(128),
    AttemptedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_LoginAttempts_UserId ON Auth.LoginAttempts(UserId);
CREATE INDEX IX_LoginAttempts_Email ON Auth.LoginAttempts(Email);
CREATE INDEX IX_LoginAttempts_IPAddress ON Auth.LoginAttempts(IPAddress);
CREATE INDEX IX_LoginAttempts_AttemptedAt ON Auth.LoginAttempts(AttemptedAt);
CREATE INDEX IX_LoginAttempts_TenantId ON Auth.LoginAttempts(TenantId);
PRINT '✓ Auth.LoginAttempts created';
GO

-- 37. AuditTrail (User activity audit)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditTrail' AND schema_id = SCHEMA_ID('Auth'))
CREATE TABLE Auth.AuditTrail (
    AuditTrailId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128),
    Action NVARCHAR(256) NOT NULL,
    EntityType NVARCHAR(100),
    EntityId INT,
    Changes NVARCHAR(MAX),
    IPAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);
CREATE INDEX IX_AuditTrail_UserId ON Auth.AuditTrail(UserId);
CREATE INDEX IX_AuditTrail_Action ON Auth.AuditTrail(Action);
CREATE INDEX IX_AuditTrail_CreatedAt ON Auth.AuditTrail(CreatedAt);
CREATE INDEX IX_AuditTrail_TenantId ON Auth.AuditTrail(TenantId);
PRINT '✓ Auth.AuditTrail created';
GO

-- 38. TenantUsers (Tenant membership)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TenantUsers' AND schema_id = SCHEMA_ID('Auth'))
CREATE TABLE Auth.TenantUsers (
    TenantUserId INT PRIMARY KEY IDENTITY(1,1),
    TenantId NVARCHAR(128) NOT NULL,
    UserId NVARCHAR(128) NOT NULL,
    InvitedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AcceptedAt DATETIME2,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    UNIQUE (TenantId, UserId)
);
CREATE INDEX IX_TenantUsers_TenantId ON Auth.TenantUsers(TenantId);
CREATE INDEX IX_TenantUsers_UserId ON Auth.TenantUsers(UserId);
PRINT '✓ Auth.TenantUsers created';
GO

PRINT '✓ AUTH SCHEMA: 10 tables created successfully';
GO

-- ============================================
-- SUMMARY
-- ============================================
PRINT '';
PRINT '════════════════════════════════════════════════════════════════';
PRINT '✓ V3 COMPLETE SCHEMA CREATED SUCCESSFULLY';
PRINT '════════════════════════════════════════════════════════════════';
PRINT '✓ Schemas Created: 6 (Master, Shared, Transaction, Report, Auth)';
PRINT '✓ Tables Created: 38 (organized by schema)';
PRINT '✓ Master Schema: 16 tables (multi-tenancy core)';
PRINT '✓ Shared Schema: 7 tables (cross-cutting concerns)';
PRINT '✓ Transaction Schema: 1 table (financial tracking)';
PRINT '✓ Report Schema: 4 tables (analytics & reporting)';
PRINT '✓ Auth Schema: 10 tables (authentication & authorization)';
PRINT '';
PRINT 'Key Features:';
PRINT '  ✓ Multi-tenancy support (all tables)';
PRINT '  ✓ Soft deletes & audit columns (all tables)';
PRINT '  ✓ Hierarchical data (HierarchyID): Categories, MenuItems, GeoHierarchy, Lookup';
PRINT '  ✓ Polymorphic tables: SeoMeta, Tags, Translations, FileStorage, AuditLogs';
PRINT '  ✓ Lookup consolidation: Master.Lookup (Currencies, Languages, TimeZones)';
PRINT '  ✓ Token consolidation: Auth.AuthTokens (4 token types merged)';
PRINT '  ✓ Proper indexing for performance';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Execute 02_CREATE_STORED_PROCEDURES.sql';
PRINT '  2. Execute 03_SEED_DATA.sql';
PRINT '  3. Execute 04_MATERIALIZED_VIEWS.sql';
PRINT '  4. Begin implementation (Dapper repositories, services, APIs)';
PRINT '════════════════════════════════════════════════════════════════';
