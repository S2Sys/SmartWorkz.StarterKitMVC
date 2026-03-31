-- ============================================
-- SmartWorkz v4 Phase 1: Shared Schema Tables
-- Date: 2026-03-31
-- 7 Tables: SEO, Tags, Translations, Audit
-- ============================================

USE Boilerplate;

-- ============================================
-- 1. SeoMeta (Polymorphic SEO Metadata)
-- ============================================
CREATE TABLE Shared.SeoMeta (
    SeoMetaId INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL, -- 'Product', 'Category', 'BlogPost', 'CustomPage', 'MenuItem', 'GeolocationPage'
    EntityId INT NOT NULL, -- FK to the entity
    Slug NVARCHAR(256) NOT NULL,
    Title NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    Keywords NVARCHAR(500),
    StructuredData NVARCHAR(MAX), -- JSON-LD schema
    MetaRobots NVARCHAR(100), -- index,follow or noindex,nofollow
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

-- Indexes for polymorphic queries
CREATE INDEX IX_SeoMeta_EntityType_EntityId ON Shared.SeoMeta(EntityType, EntityId);
CREATE INDEX IX_SeoMeta_Slug ON Shared.SeoMeta(Slug);
CREATE INDEX IX_SeoMeta_TenantId ON Shared.SeoMeta(TenantId);

-- ============================================
-- 2. Tags (Polymorphic Tagging System)
-- ============================================
CREATE TABLE Shared.Tags (
    TagId INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL, -- 'Product', 'Order', 'Customer', 'BlogPost'
    EntityId INT NOT NULL,
    TagName NVARCHAR(256) NOT NULL,
    TagCategory NVARCHAR(100), -- Optional categorization
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

-- Indexes for polymorphic queries
CREATE INDEX IX_Tags_EntityType_EntityId ON Shared.Tags(EntityType, EntityId);
CREATE INDEX IX_Tags_TagName ON Shared.Tags(TagName);
CREATE INDEX IX_Tags_TenantId ON Shared.Tags(TenantId);

-- ============================================
-- 3. Translations (Multi-language content)
-- ============================================
CREATE TABLE Shared.Translations (
    TranslationId INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL, -- Entity being translated
    EntityId INT NOT NULL,
    LanguageId INT NOT NULL,
    FieldName NVARCHAR(256) NOT NULL, -- 'Title', 'Description', 'Content'
    TranslatedValue NVARCHAR(MAX) NOT NULL,
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (LanguageId) REFERENCES Master.Languages(LanguageId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, EntityType, EntityId, LanguageId, FieldName)
);

CREATE INDEX IX_Translations_EntityType_EntityId ON Shared.Translations(EntityType, EntityId);
CREATE INDEX IX_Translations_LanguageId ON Shared.Translations(LanguageId);
CREATE INDEX IX_Translations_TenantId ON Shared.Translations(TenantId);

-- ============================================
-- 4. Notifications (System Notifications)
-- ============================================
CREATE TABLE Shared.Notifications (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    NotificationType NVARCHAR(100) NOT NULL, -- 'OrderConfirmed', 'ShipmentUpdate', 'Promotion'
    RecipientType NVARCHAR(50), -- 'User', 'Customer', 'Admin'
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

-- ============================================
-- 5. AuditLogs (Audit Trail)
-- ============================================
CREATE TABLE Shared.AuditLogs (
    AuditLogId INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL,
    EntityId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- 'Create', 'Update', 'Delete'
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

-- ============================================
-- 6. FileStorage (Document Management)
-- ============================================
CREATE TABLE Shared.FileStorage (
    FileId INT PRIMARY KEY IDENTITY(1,1),
    FileName NVARCHAR(256) NOT NULL,
    FileSize BIGINT NOT NULL,
    MimeType NVARCHAR(100),
    FilePath NVARCHAR(500) NOT NULL,
    EntityType NVARCHAR(100), -- 'Product', 'BlogPost', 'UserAvatar'
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

-- ============================================
-- 7. EmailQueue (Email Delivery Queue)
-- ============================================
CREATE TABLE Shared.EmailQueue (
    EmailQueueId INT PRIMARY KEY IDENTITY(1,1),
    ToEmail NVARCHAR(256) NOT NULL,
    CcEmail NVARCHAR(500),
    BccEmail NVARCHAR(500),
    Subject NVARCHAR(256) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    IsHtml BIT NOT NULL DEFAULT 1,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Sent, Failed, Cancelled
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

PRINT '✓ Shared schema: 7 tables created successfully'

