-- ============================================
-- SmartWorkz StarterKitMVC Database Schema
-- Version: 1.0.0
-- ============================================

-- ============================================
-- IDENTITY TABLES
-- ============================================

-- Users Table
CREATE TABLE IF NOT EXISTS Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserName NVARCHAR(256) NOT NULL UNIQUE,
    NormalizedUserName NVARCHAR(256) NOT NULL UNIQUE,
    Email NVARCHAR(256) NOT NULL,
    NormalizedEmail NVARCHAR(256) NOT NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    ConcurrencyStamp NVARCHAR(MAX),
    PhoneNumber NVARCHAR(50),
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
    UpdatedBy NVARCHAR(256)
);

CREATE INDEX IX_Users_NormalizedEmail ON Users(NormalizedEmail);
CREATE INDEX IX_Users_TenantId ON Users(TenantId);

-- Roles Table
CREATE TABLE IF NOT EXISTS Roles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(256) NOT NULL,
    NormalizedName NVARCHAR(256) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    TenantId NVARCHAR(128),
    IsSystemRole BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256)
);

CREATE INDEX IX_Roles_TenantId ON Roles(TenantId);

-- UserRoles Junction Table
CREATE TABLE IF NOT EXISTS UserRoles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);

-- Claims Table
CREATE TABLE IF NOT EXISTS Claims (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClaimType NVARCHAR(256) NOT NULL,
    ClaimValue NVARCHAR(MAX),
    Description NVARCHAR(500),
    IsSystemClaim BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- UserClaims Table
CREATE TABLE IF NOT EXISTS UserClaims (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ClaimType NVARCHAR(256) NOT NULL,
    ClaimValue NVARCHAR(MAX),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_UserClaims_UserId ON UserClaims(UserId);

-- RoleClaims Table
CREATE TABLE IF NOT EXISTS RoleClaims (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    ClaimType NVARCHAR(256) NOT NULL,
    ClaimValue NVARCHAR(MAX),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);

CREATE INDEX IX_RoleClaims_RoleId ON RoleClaims(RoleId);

-- Permissions Table
CREATE TABLE IF NOT EXISTS Permissions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(256) NOT NULL UNIQUE,
    DisplayName NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    Category NVARCHAR(128),
    IsSystemPermission BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- RolePermissions Junction Table
CREATE TABLE IF NOT EXISTS RolePermissions (
    RoleId UNIQUEIDENTIFIER NOT NULL,
    PermissionId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);

-- ============================================
-- MULTI-TENANCY TABLES
-- ============================================

-- Tenants Table
CREATE TABLE IF NOT EXISTS Tenants (
    Id NVARCHAR(128) PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    Subdomain NVARCHAR(128) UNIQUE,
    Domain NVARCHAR(256),
    ConnectionString NVARCHAR(MAX),
    DatabaseProvider NVARCHAR(50) DEFAULT 'SqlServer',
    IsActive BIT NOT NULL DEFAULT 1,
    ExpirationDate DATETIME2,
    MaxUsers INT DEFAULT 0,
    Features NVARCHAR(MAX), -- JSON array of enabled features
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256)
);

CREATE INDEX IX_Tenants_Subdomain ON Tenants(Subdomain);
CREATE INDEX IX_Tenants_IsActive ON Tenants(IsActive);

-- TenantBranding Table
CREATE TABLE IF NOT EXISTS TenantBranding (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId NVARCHAR(128) NOT NULL UNIQUE,
    LogoUrl NVARCHAR(500),
    FaviconUrl NVARCHAR(500),
    PrimaryColor NVARCHAR(20) DEFAULT '#0d6efd',
    SecondaryColor NVARCHAR(20) DEFAULT '#6c757d',
    AccentColor NVARCHAR(20) DEFAULT '#198754',
    CustomCss NVARCHAR(MAX),
    FooterText NVARCHAR(500),
    SupportEmail NVARCHAR(256),
    SupportPhone NVARCHAR(50),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE
);

-- ============================================
-- SETTINGS TABLES
-- ============================================

-- SettingCategories Table
CREATE TABLE IF NOT EXISTS SettingCategories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Key] NVARCHAR(128) NOT NULL UNIQUE,
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    Icon NVARCHAR(50),
    SortOrder INT NOT NULL DEFAULT 0,
    IsSystem BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- SettingDefinitions Table
CREATE TABLE IF NOT EXISTS SettingDefinitions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Key] NVARCHAR(256) NOT NULL UNIQUE,
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    ValueType NVARCHAR(50) NOT NULL DEFAULT 'String', -- String, Int, Bool, Double, DateTime, StringList, Json, EncryptedString
    DefaultValue NVARCHAR(MAX),
    ValidationRegex NVARCHAR(500),
    MinValue NVARCHAR(50),
    MaxValue NVARCHAR(50),
    Options NVARCHAR(MAX), -- JSON array for dropdown options
    IsRequired BIT NOT NULL DEFAULT 0,
    IsEncrypted BIT NOT NULL DEFAULT 0,
    IsSystem BIT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CategoryId) REFERENCES SettingCategories(Id)
);

CREATE INDEX IX_SettingDefinitions_CategoryId ON SettingDefinitions(CategoryId);

-- SettingValues Table
CREATE TABLE IF NOT EXISTS SettingValues (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DefinitionId UNIQUEIDENTIFIER NOT NULL,
    TenantId NVARCHAR(128), -- NULL for global settings
    UserId UNIQUEIDENTIFIER, -- NULL for tenant/global settings
    Value NVARCHAR(MAX),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy NVARCHAR(256),
    FOREIGN KEY (DefinitionId) REFERENCES SettingDefinitions(Id) ON DELETE CASCADE,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE (DefinitionId, TenantId, UserId)
);

CREATE INDEX IX_SettingValues_DefinitionId ON SettingValues(DefinitionId);
CREATE INDEX IX_SettingValues_TenantId ON SettingValues(TenantId);

-- ============================================
-- LIST OF VALUES (LOV) TABLES
-- ============================================

-- LovCategories Table
CREATE TABLE IF NOT EXISTS LovCategories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Key] NVARCHAR(128) NOT NULL UNIQUE,
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    Icon NVARCHAR(50),
    IsSystem BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256)
);

-- LovSubCategories Table
CREATE TABLE IF NOT EXISTS LovSubCategories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    [Key] NVARCHAR(128) NOT NULL,
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CategoryId) REFERENCES LovCategories(Id) ON DELETE CASCADE,
    UNIQUE (CategoryId, [Key])
);

-- LovItems Table
CREATE TABLE IF NOT EXISTS LovItems (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    SubCategoryId UNIQUEIDENTIFIER,
    [Key] NVARCHAR(128) NOT NULL,
    DisplayName NVARCHAR(256) NOT NULL,
    Value NVARCHAR(MAX),
    Description NVARCHAR(500),
    Icon NVARCHAR(50),
    Color NVARCHAR(20),
    ParentId UNIQUEIDENTIFIER, -- For hierarchical items
    SortOrder INT NOT NULL DEFAULT 0,
    Tags NVARCHAR(500), -- Comma-separated tags
    Metadata NVARCHAR(MAX), -- JSON for additional data
    TenantId NVARCHAR(128), -- NULL for global items
    IsDefault BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    FOREIGN KEY (CategoryId) REFERENCES LovCategories(Id),
    FOREIGN KEY (SubCategoryId) REFERENCES LovSubCategories(Id),
    FOREIGN KEY (ParentId) REFERENCES LovItems(Id),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE,
    UNIQUE (CategoryId, [Key], TenantId)
);

CREATE INDEX IX_LovItems_CategoryId ON LovItems(CategoryId);
CREATE INDEX IX_LovItems_TenantId ON LovItems(TenantId);

-- LovItemLocalizations Table
CREATE TABLE IF NOT EXISTS LovItemLocalizations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ItemId UNIQUEIDENTIFIER NOT NULL,
    CultureCode NVARCHAR(10) NOT NULL,
    DisplayName NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    FOREIGN KEY (ItemId) REFERENCES LovItems(Id) ON DELETE CASCADE,
    UNIQUE (ItemId, CultureCode)
);

-- ============================================
-- NOTIFICATIONS TABLES
-- ============================================

-- NotificationTemplates Table
CREATE TABLE IF NOT EXISTS NotificationTemplates (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Key] NVARCHAR(128) NOT NULL UNIQUE,
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    Subject NVARCHAR(500),
    Body NVARCHAR(MAX) NOT NULL,
    BodyHtml NVARCHAR(MAX),
    Channel NVARCHAR(50) NOT NULL DEFAULT 'Email', -- Email, SMS, Push, InApp
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256)
);

-- Notifications Table
CREATE TABLE IF NOT EXISTS Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    TenantId NVARCHAR(128),
    TemplateId UNIQUEIDENTIFIER,
    Title NVARCHAR(256) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50) NOT NULL DEFAULT 'Info', -- Info, Success, Warning, Error
    Channel NVARCHAR(50) NOT NULL DEFAULT 'InApp', -- Email, SMS, Push, InApp
    ActionUrl NVARCHAR(500),
    ActionText NVARCHAR(100),
    Icon NVARCHAR(50),
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME2,
    IsSent BIT NOT NULL DEFAULT 0,
    SentAt DATETIME2,
    ExpiresAt DATETIME2,
    Metadata NVARCHAR(MAX), -- JSON for additional data
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    FOREIGN KEY (TemplateId) REFERENCES NotificationTemplates(Id)
);

CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_TenantId ON Notifications(TenantId);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt DESC);

-- ============================================
-- AUDIT LOG TABLE
-- ============================================

CREATE TABLE IF NOT EXISTS AuditLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId UNIQUEIDENTIFIER,
    UserName NVARCHAR(256),
    TenantId NVARCHAR(128),
    Action NVARCHAR(50) NOT NULL, -- Create, Update, Delete, Login, Logout, etc.
    EntityType NVARCHAR(256),
    EntityId NVARCHAR(128),
    OldValues NVARCHAR(MAX), -- JSON
    NewValues NVARCHAR(MAX), -- JSON
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    AdditionalInfo NVARCHAR(MAX), -- JSON
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_TenantId ON AuditLogs(TenantId);
CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType);
CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt DESC);

GO
