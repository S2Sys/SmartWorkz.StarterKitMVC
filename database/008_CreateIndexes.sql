-- ============================================
-- SmartWorkz v4 Phase 1: Performance Indexes
-- Date: 2026-03-31
-- Additional indexes for optimal query performance
-- ============================================

USE Boilerplate;

-- ============================================
-- Master Schema - Additional Indexes
-- ============================================

-- Product Queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_Products_IsActive_TenantId ON Master.Products(IsActive, TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_CategoryId_IsActive')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_Products_CategoryId_IsActive ON Master.Products(CategoryId, IsActive);

-- Category Hierarchy Queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Categories_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Categories' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_Categories_IsActive_TenantId ON Master.Categories(IsActive, TenantId);

-- Menu Performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MenuItems_MenuId_IsActive')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MenuItems' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_MenuItems_MenuId_IsActive ON Master.MenuItems(MenuId, IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MenuItems_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MenuItems' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_MenuItems_IsActive_TenantId ON Master.MenuItems(IsActive, TenantId);

-- GeoHierarchy Queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GeoHierarchy_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GeoHierarchy' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_GeoHierarchy_IsActive_TenantId ON Master.GeoHierarchy(IsActive, TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GeoHierarchy_GeoType')
    CREATE INDEX IX_GeoHierarchy_GeoType ON Master.GeoHierarchy(GeoType);

-- ============================================
-- Shared Schema - Additional Indexes
-- ============================================

-- SeoMeta Performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SeoMeta_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SeoMeta' AND TABLE_SCHEMA = 'Shared' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_SeoMeta_IsActive_TenantId ON Shared.SeoMeta(IsActive, TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SeoMeta_EntityType_TenantId')
    CREATE INDEX IX_SeoMeta_EntityType_TenantId ON Shared.SeoMeta(EntityType, TenantId);

-- Tags Performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tags_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tags' AND TABLE_SCHEMA = 'Shared' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_Tags_IsActive_TenantId ON Shared.Tags(IsActive, TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tags_EntityType_TenantId')
    CREATE INDEX IX_Tags_EntityType_TenantId ON Shared.Tags(EntityType, TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tags_TagName_TenantId')
    CREATE INDEX IX_Tags_TagName_TenantId ON Shared.Tags(TagName, TenantId);

-- Translations Queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Translations_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Translations' AND TABLE_SCHEMA = 'Shared' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_Translations_IsActive_TenantId ON Shared.Translations(IsActive, TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Translations_FieldName')
    CREATE INDEX IX_Translations_FieldName ON Shared.Translations(FieldName);

-- Notifications
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Notifications_RecipientType_IsRead')
    CREATE INDEX IX_Notifications_RecipientType_IsRead ON Shared.Notifications(RecipientType, IsRead);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Notifications_CreatedAt_TenantId')
    CREATE INDEX IX_Notifications_CreatedAt_TenantId ON Shared.Notifications(CreatedAt, TenantId);

-- FileStorage
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileStorage_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FileStorage' AND TABLE_SCHEMA = 'Shared' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_FileStorage_IsActive_TenantId ON Shared.FileStorage(IsActive, TenantId);

-- EmailQueue
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmailQueue_Status_CreatedAt')
    CREATE INDEX IX_EmailQueue_Status_CreatedAt ON Shared.EmailQueue(Status, CreatedAt);

-- ============================================
-- Auth Schema - Additional Indexes
-- ============================================

-- User Queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND TABLE_SCHEMA = 'Auth' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_Users_IsActive_TenantId ON Auth.Users(IsActive, TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_UserName_TenantId')
    CREATE INDEX IX_Users_UserName_TenantId ON Auth.Users(UserName, TenantId);

-- Role Queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Roles_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Roles' AND TABLE_SCHEMA = 'Auth' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_Roles_IsActive_TenantId ON Auth.Roles(IsActive, TenantId);

-- LoginAttempts
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoginAttempts_IsSuccessful_AttemptedAt')
    CREATE INDEX IX_LoginAttempts_IsSuccessful_AttemptedAt ON Auth.LoginAttempts(IsSuccessful, AttemptedAt);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoginAttempts_Email_AttemptedAt')
    CREATE INDEX IX_LoginAttempts_Email_AttemptedAt ON Auth.LoginAttempts(Email, AttemptedAt);

-- RefreshTokens
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RefreshTokens_RevokedAt')
    CREATE INDEX IX_RefreshTokens_RevokedAt ON Auth.RefreshTokens(RevokedAt);

-- TenantUsers
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TenantUsers_Status')
    CREATE INDEX IX_TenantUsers_Status ON Auth.TenantUsers(Status);

-- ============================================
-- Transaction Schema - Additional Indexes
-- ============================================

-- TransactionLog Performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TransactionLog_EntityType_TenantId')
    CREATE INDEX IX_TransactionLog_EntityType_TenantId ON [Transaction].TransactionLog(EntityType, TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TransactionLog_Status_CreatedAt')
    CREATE INDEX IX_TransactionLog_Status_CreatedAt ON [Transaction].TransactionLog(Status, CreatedAt);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TransactionLog_PaymentMethod')
    CREATE INDEX IX_TransactionLog_PaymentMethod ON [Transaction].TransactionLog(PaymentMethod);

-- ============================================
-- Report Schema - Additional Indexes
-- ============================================

-- Reports
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reports_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reports' AND TABLE_SCHEMA = 'Report' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_Reports_IsActive_TenantId ON Report.Reports(IsActive, TenantId);

-- ReportSchedules
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReportSchedules_IsActive_TenantId')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ReportSchedules' AND TABLE_SCHEMA = 'Report' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_ReportSchedules_IsActive_TenantId ON Report.ReportSchedules(IsActive, TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReportSchedules_Frequency')
    CREATE INDEX IX_ReportSchedules_Frequency ON Report.ReportSchedules(Frequency);

-- ReportData
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReportData_TenantId_GeneratedAt')
    CREATE INDEX IX_ReportData_TenantId_GeneratedAt ON Report.ReportData(TenantId, GeneratedAt DESC);

-- Analytics
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_TenantId_EventDate')
    CREATE INDEX IX_Analytics_TenantId_EventDate ON Report.Analytics(TenantId, EventDate DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_EventName_EventDate')
    CREATE INDEX IX_Analytics_EventName_EventDate ON Report.Analytics(EventName, EventDate);

-- ============================================
-- Composite Indexes for Common Queries
-- ============================================

-- Product Catalog Queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_TenantId_IsActive_Slug')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'Slug')
    CREATE INDEX IX_Products_TenantId_IsActive_Slug ON Master.Products(TenantId, IsActive, Slug);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Categories_TenantId_IsActive_Slug')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Categories' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Categories' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'Slug')
    CREATE INDEX IX_Categories_TenantId_IsActive_Slug ON Master.Categories(TenantId, IsActive, Slug);

-- Menu Navigation
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MenuItems_MenuId_IsActive_DisplayOrder')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MenuItems' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MenuItems' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'DisplayOrder')
    CREATE INDEX IX_MenuItems_MenuId_IsActive_DisplayOrder ON Master.MenuItems(MenuId, IsActive, DisplayOrder);

-- Blog/Content Queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BlogPosts_TenantId_IsActive_PublishedAt')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BlogPosts' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BlogPosts' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'PublishedAt')
    CREATE INDEX IX_BlogPosts_TenantId_IsActive_PublishedAt ON Master.BlogPosts(TenantId, IsActive, PublishedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CustomPages_TenantId_IsActive')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomPages' AND TABLE_SCHEMA = 'Master' AND COLUMN_NAME = 'IsActive')
    CREATE INDEX IX_CustomPages_TenantId_IsActive ON Master.CustomPages(TenantId, IsActive);

-- SEO Lookups
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SeoMeta_TenantId_IsActive_Slug')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SeoMeta' AND TABLE_SCHEMA = 'Shared' AND COLUMN_NAME = 'IsActive')
  AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SeoMeta' AND TABLE_SCHEMA = 'Shared' AND COLUMN_NAME = 'Slug')
    CREATE INDEX IX_SeoMeta_TenantId_IsActive_Slug ON Shared.SeoMeta(TenantId, IsActive, Slug);

-- ============================================
-- Statistics for Query Optimizer
-- ============================================

DBCC DBREINDEX ('Master.Tenants', '', 80);
DBCC DBREINDEX ('Master.Products', '', 80);
DBCC DBREINDEX ('Master.Categories', '', 80);
DBCC DBREINDEX ('Master.MenuItems', '', 80);
DBCC DBREINDEX ('Shared.SeoMeta', '', 80);
DBCC DBREINDEX ('Shared.Tags', '', 80);
DBCC DBREINDEX ('Auth.Users', '', 80);
DBCC DBREINDEX ('Auth.Roles', '', 80);

PRINT 'All performance indexes created successfully'
PRINT 'Statistics updated'
PRINT 'Database ready for production'

