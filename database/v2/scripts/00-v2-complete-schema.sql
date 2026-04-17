-- ============================================
-- SmartWorkz v2 Complete Schema
-- Purpose: Add LovItems (consolidated lookups) to Master schema
-- Strategy: Keep existing Master tables, add Master.LovItems, use UPSERT procedures
-- Date: 2026-04-17
-- ============================================

USE Boilerplate;

-- ============================================
-- STEP 1: Create/Verify Master Schema
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Master')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Master'
END
GO

-- ============================================
-- STEP 2: Create All Master Tables (from v1, unchanged)
-- ============================================

-- 1. Tenants (Multi-tenancy support)
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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tenants_IsActive')
CREATE INDEX IX_Tenants_IsActive ON Master.Tenants(IsActive);

-- 2. Countries (Lookup - keep for backward compatibility)
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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Countries_Code')
CREATE INDEX IX_Countries_Code ON Master.Countries(Code);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Countries_TenantId')
CREATE INDEX IX_Countries_TenantId ON Master.Countries(TenantId);

-- 3. Currencies (Lookup - keep for backward compatibility)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Currencies' AND schema_id = SCHEMA_ID('Master'))
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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Currencies_Code')
CREATE INDEX IX_Currencies_Code ON Master.Currencies(Code);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Currencies_TenantId')
CREATE INDEX IX_Currencies_TenantId ON Master.Currencies(TenantId);

-- 4. Languages (Lookup - keep for backward compatibility)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Languages' AND schema_id = SCHEMA_ID('Master'))
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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Languages_Code')
CREATE INDEX IX_Languages_Code ON Master.Languages(Code);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Languages_TenantId')
CREATE INDEX IX_Languages_TenantId ON Master.Languages(TenantId);

-- 5. TimeZones (Lookup - keep for backward compatibility)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TimeZones' AND schema_id = SCHEMA_ID('Master'))
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

-- 6. Configuration (App Settings)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Configuration' AND schema_id = SCHEMA_ID('Master'))
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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Configuration_Key')
CREATE INDEX IX_Configuration_Key ON Master.Configuration([Key]);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Configuration_TenantId')
CREATE INDEX IX_Configuration_TenantId ON Master.Configuration(TenantId);

-- 7. FeatureFlags (Feature Management)
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

-- ============================================
-- STEP 3: Create Master.LovItems (NEW - Consolidated Lookups)
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LovItems' AND schema_id = SCHEMA_ID('Master'))
CREATE TABLE Master.LovItems (
    IntId INT,
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CategoryKey NVARCHAR(100) NOT NULL,           -- 'currencies', 'languages', 'timezones', 'countries'
    Key NVARCHAR(100) NOT NULL,                   -- 'USD', 'en-US', 'America/New_York', 'US'
    DisplayName NVARCHAR(500) NOT NULL,           -- 'US Dollar', 'English', etc.
    TenantId NVARCHAR(128),                       -- NULL for global, 'ABC' for parent, 'ABC-US' for child
    IsGlobalScope BIT NOT NULL DEFAULT 0,         -- 1 = global (IntId 1-999), 0 = tenant-specific (IntId 1000+)
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(128),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(128),
    SortOrder INT NOT NULL DEFAULT 0,
    Metadata NVARCHAR(MAX),                       -- JSON: {"symbol":"$","decimalPlaces":2}
    LocalizedNames NVARCHAR(MAX),                 -- JSON: {"en-US":"English","es-ES":"Español"}

    CONSTRAINT UQ_LovItems_IntId UNIQUE (IntId) WHERE IntId IS NOT NULL,
    CONSTRAINT UQ_LovItems_Key UNIQUE (CategoryKey, Key, TenantId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

-- Indexes for performance
CREATE INDEX IX_LovItems_Category ON Master.LovItems(CategoryKey, IsActive, IsDeleted);
CREATE INDEX IX_LovItems_Tenant ON Master.LovItems(TenantId, IsGlobalScope, IsActive);
CREATE INDEX IX_LovItems_Global ON Master.LovItems(IsGlobalScope, IsActive, IsDeleted);
CREATE INDEX IX_LovItems_Key ON Master.LovItems(CategoryKey, Key, TenantId);
CREATE INDEX IX_LovItems_IntId ON Master.LovItems(IntId) WHERE IntId IS NOT NULL;

-- ============================================
-- STEP 4: Create UPSERT Procedures
-- ============================================

-- Drop existing procedures if they exist
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_LovItem_Upsert' AND schema_id = SCHEMA_ID('Master'))
    DROP PROCEDURE Master.sp_LovItem_Upsert;

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_LovItem_GetByTenantHierarchy' AND schema_id = SCHEMA_ID('Master'))
    DROP PROCEDURE Master.sp_LovItem_GetByTenantHierarchy;

GO

-- UPSERT Procedure (MERGE-based)
CREATE PROCEDURE Master.sp_LovItem_Upsert
    @IntId INT = NULL,
    @Id UNIQUEIDENTIFIER,
    @CategoryKey NVARCHAR(100),
    @Key NVARCHAR(100),
    @DisplayName NVARCHAR(500),
    @TenantId NVARCHAR(128) = NULL,
    @IsGlobalScope BIT,
    @IsActive BIT = 1,
    @IsDeleted BIT = 0,
    @CreatedAt DATETIME2,
    @CreatedBy NVARCHAR(128),
    @UpdatedAt DATETIME2 = NULL,
    @UpdatedBy NVARCHAR(128) = NULL,
    @SortOrder INT = 0,
    @Metadata NVARCHAR(MAX) = NULL,
    @LocalizedNames NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    MERGE Master.LovItems AS target
    USING (SELECT @IntId, @Id, @CategoryKey, @Key, @DisplayName, @TenantId, @IsGlobalScope, @IsActive, @IsDeleted, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy, @SortOrder, @Metadata, @LocalizedNames)
    AS source(IntId, Id, CategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata, LocalizedNames)
    ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET
            DisplayName = source.DisplayName,
            IsActive = source.IsActive,
            IsDeleted = source.IsDeleted,
            UpdatedAt = COALESCE(source.UpdatedAt, GETUTCDATE()),
            UpdatedBy = source.UpdatedBy,
            SortOrder = source.SortOrder,
            Metadata = source.Metadata,
            LocalizedNames = source.LocalizedNames
    WHEN NOT MATCHED THEN
        INSERT (IntId, Id, CategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata, LocalizedNames)
        VALUES (source.IntId, source.Id, source.CategoryKey, source.Key, source.DisplayName, source.TenantId, source.IsGlobalScope, source.IsActive, source.IsDeleted, source.CreatedAt, source.CreatedBy, source.UpdatedAt, source.UpdatedBy, source.SortOrder, source.Metadata, source.LocalizedNames);
END
GO

-- Tenant Hierarchy Query Procedure
CREATE PROCEDURE Master.sp_LovItem_GetByTenantHierarchy
    @CategoryKey NVARCHAR(100),
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ParentTenantId NVARCHAR(128) = NULL

    -- Extract parent tenant ID (ABC-US → ABC)
    IF @TenantId LIKE '%-'
        SET @ParentTenantId = LEFT(@TenantId, CHARINDEX('-', @TenantId) - 1)

    SELECT * FROM Master.LovItems
    WHERE CategoryKey = @CategoryKey
      AND IsActive = 1
      AND IsDeleted = 0
      AND (
        IsGlobalScope = 1
        OR TenantId IS NULL
        OR TenantId = @ParentTenantId
        OR TenantId = @TenantId
      )
    ORDER BY SortOrder, DisplayName
END
GO

-- ============================================
-- STEP 5: Seed Global Lookups using UPSERT (IDs 1-999)
-- ============================================

DECLARE @Now DATETIME2 = GETUTCDATE();

-- TimeZones (IDs 1-10)
EXEC Master.sp_LovItem_Upsert
    @IntId = 1,
    @Id = NEWID(),
    @CategoryKey = 'timezones',
    @Key = 'America/New_York',
    @DisplayName = 'Eastern Time (US & Canada)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 1,
    @Metadata = '{"offsetHours":-5,"isDaylightSavings":true}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 2,
    @Id = NEWID(),
    @CategoryKey = 'timezones',
    @Key = 'America/Chicago',
    @DisplayName = 'Central Time (US & Canada)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 2,
    @Metadata = '{"offsetHours":-6,"isDaylightSavings":true}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 3,
    @Id = NEWID(),
    @CategoryKey = 'timezones',
    @Key = 'America/Denver',
    @DisplayName = 'Mountain Time (US & Canada)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 3,
    @Metadata = '{"offsetHours":-7,"isDaylightSavings":true}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 4,
    @Id = NEWID(),
    @CategoryKey = 'timezones',
    @Key = 'America/Los_Angeles',
    @DisplayName = 'Pacific Time (US & Canada)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 4,
    @Metadata = '{"offsetHours":-8,"isDaylightSavings":true}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 5,
    @Id = NEWID(),
    @CategoryKey = 'timezones',
    @Key = 'Europe/London',
    @DisplayName = 'GMT/BST (UK)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 5,
    @Metadata = '{"offsetHours":0,"isDaylightSavings":true}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 6,
    @Id = NEWID(),
    @CategoryKey = 'timezones',
    @Key = 'Europe/Paris',
    @DisplayName = 'CET/CEST (Europe)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 6,
    @Metadata = '{"offsetHours":1,"isDaylightSavings":true}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 7,
    @Id = NEWID(),
    @CategoryKey = 'timezones',
    @Key = 'Asia/Tokyo',
    @DisplayName = 'Japan Standard Time',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 7,
    @Metadata = '{"offsetHours":9,"isDaylightSavings":false}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 8,
    @Id = NEWID(),
    @CategoryKey = 'timezones',
    @Key = 'Asia/Shanghai',
    @DisplayName = 'China Standard Time',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 8,
    @Metadata = '{"offsetHours":8,"isDaylightSavings":false}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 9,
    @Id = NEWID(),
    @CategoryKey = 'timezones',
    @Key = 'Asia/Dubai',
    @DisplayName = 'Gulf Standard Time',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 9,
    @Metadata = '{"offsetHours":4,"isDaylightSavings":false}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 10,
    @Id = NEWID(),
    @CategoryKey = 'timezones',
    @Key = 'Asia/Kolkata',
    @DisplayName = 'India Standard Time',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 10,
    @Metadata = '{"offsetHours":5.5,"isDaylightSavings":false}';

-- Countries (IDs 51-60)
EXEC Master.sp_LovItem_Upsert
    @IntId = 51,
    @Id = NEWID(),
    @CategoryKey = 'countries',
    @Key = 'US',
    @DisplayName = 'United States',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 1,
    @Metadata = '{"code":"US","flagEmoji":"🇺🇸"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 52,
    @Id = NEWID(),
    @CategoryKey = 'countries',
    @Key = 'CA',
    @DisplayName = 'Canada',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 2,
    @Metadata = '{"code":"CA","flagEmoji":"🇨🇦"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 53,
    @Id = NEWID(),
    @CategoryKey = 'countries',
    @Key = 'GB',
    @DisplayName = 'United Kingdom',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 3,
    @Metadata = '{"code":"GB","flagEmoji":"🇬🇧"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 54,
    @Id = NEWID(),
    @CategoryKey = 'countries',
    @Key = 'DE',
    @DisplayName = 'Germany',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 4,
    @Metadata = '{"code":"DE","flagEmoji":"🇩🇪"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 55,
    @Id = NEWID(),
    @CategoryKey = 'countries',
    @Key = 'FR',
    @DisplayName = 'France',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 5,
    @Metadata = '{"code":"FR","flagEmoji":"🇫🇷"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 56,
    @Id = NEWID(),
    @CategoryKey = 'countries',
    @Key = 'JP',
    @DisplayName = 'Japan',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 6,
    @Metadata = '{"code":"JP","flagEmoji":"🇯🇵"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 57,
    @Id = NEWID(),
    @CategoryKey = 'countries',
    @Key = 'CN',
    @DisplayName = 'China',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 7,
    @Metadata = '{"code":"CN","flagEmoji":"🇨🇳"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 58,
    @Id = NEWID(),
    @CategoryKey = 'countries',
    @Key = 'IN',
    @DisplayName = 'India',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 8,
    @Metadata = '{"code":"IN","flagEmoji":"🇮🇳"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 59,
    @Id = NEWID(),
    @CategoryKey = 'countries',
    @Key = 'BR',
    @DisplayName = 'Brazil',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 9,
    @Metadata = '{"code":"BR","flagEmoji":"🇧🇷"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 60,
    @Id = NEWID(),
    @CategoryKey = 'countries',
    @Key = 'AU',
    @DisplayName = 'Australia',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 10,
    @Metadata = '{"code":"AU","flagEmoji":"🇦🇺"}';

-- Languages (IDs 101-110)
EXEC Master.sp_LovItem_Upsert
    @IntId = 101,
    @Id = NEWID(),
    @CategoryKey = 'languages',
    @Key = 'en-US',
    @DisplayName = 'English (US)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 1,
    @Metadata = '{"nativeName":"English","isDefault":true}',
    @LocalizedNames = '{"en-US":"English","es-ES":"Inglés"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 102,
    @Id = NEWID(),
    @CategoryKey = 'languages',
    @Key = 'en-GB',
    @DisplayName = 'English (UK)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 2,
    @Metadata = '{"nativeName":"English","isDefault":false}',
    @LocalizedNames = '{"en-US":"English (UK)","es-ES":"Inglés (RU)"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 103,
    @Id = NEWID(),
    @CategoryKey = 'languages',
    @Key = 'fr-FR',
    @DisplayName = 'Français',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 3,
    @Metadata = '{"nativeName":"Français","isDefault":false}',
    @LocalizedNames = '{"en-US":"French","es-ES":"Francés"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 104,
    @Id = NEWID(),
    @CategoryKey = 'languages',
    @Key = 'de-DE',
    @DisplayName = 'Deutsch',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 4,
    @Metadata = '{"nativeName":"Deutsch","isDefault":false}',
    @LocalizedNames = '{"en-US":"German","es-ES":"Alemán"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 105,
    @Id = NEWID(),
    @CategoryKey = 'languages',
    @Key = 'es-ES',
    @DisplayName = 'Español',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 5,
    @Metadata = '{"nativeName":"Español","isDefault":false}',
    @LocalizedNames = '{"en-US":"Spanish","es-ES":"Español"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 106,
    @Id = NEWID(),
    @CategoryKey = 'languages',
    @Key = 'it-IT',
    @DisplayName = 'Italiano',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 6,
    @Metadata = '{"nativeName":"Italiano","isDefault":false}',
    @LocalizedNames = '{"en-US":"Italian","es-ES":"Italiano"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 107,
    @Id = NEWID(),
    @CategoryKey = 'languages',
    @Key = 'ja-JP',
    @DisplayName = '日本語',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 7,
    @Metadata = '{"nativeName":"日本語","isDefault":false}',
    @LocalizedNames = '{"en-US":"Japanese","es-ES":"Japonés"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 108,
    @Id = NEWID(),
    @CategoryKey = 'languages',
    @Key = 'zh-CN',
    @DisplayName = '中文(简体)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 8,
    @Metadata = '{"nativeName":"中文","isDefault":false}',
    @LocalizedNames = '{"en-US":"Chinese (Simplified)","es-ES":"Chino (Simplificado)"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 109,
    @Id = NEWID(),
    @CategoryKey = 'languages',
    @Key = 'pt-BR',
    @DisplayName = 'Português (Brasil)',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 9,
    @Metadata = '{"nativeName":"Português","isDefault":false}',
    @LocalizedNames = '{"en-US":"Portuguese (Brazil)","es-ES":"Portugués (Brasil)"}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 110,
    @Id = NEWID(),
    @CategoryKey = 'languages',
    @Key = 'ar-SA',
    @DisplayName = 'العربية',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 10,
    @Metadata = '{"nativeName":"العربية","isDefault":false}',
    @LocalizedNames = '{"en-US":"Arabic","es-ES":"Árabe"}';

-- Currencies (IDs 201-210)
EXEC Master.sp_LovItem_Upsert
    @IntId = 201,
    @Id = NEWID(),
    @CategoryKey = 'currencies',
    @Key = 'USD',
    @DisplayName = 'US Dollar',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 1,
    @Metadata = '{"symbol":"$","decimalPlaces":2}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 202,
    @Id = NEWID(),
    @CategoryKey = 'currencies',
    @Key = 'EUR',
    @DisplayName = 'Euro',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 2,
    @Metadata = '{"symbol":"€","decimalPlaces":2}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 203,
    @Id = NEWID(),
    @CategoryKey = 'currencies',
    @Key = 'GBP',
    @DisplayName = 'British Pound',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 3,
    @Metadata = '{"symbol":"£","decimalPlaces":2}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 204,
    @Id = NEWID(),
    @CategoryKey = 'currencies',
    @Key = 'JPY',
    @DisplayName = 'Japanese Yen',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 4,
    @Metadata = '{"symbol":"¥","decimalPlaces":0}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 205,
    @Id = NEWID(),
    @CategoryKey = 'currencies',
    @Key = 'CNY',
    @DisplayName = 'Chinese Yuan',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 5,
    @Metadata = '{"symbol":"¥","decimalPlaces":2}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 206,
    @Id = NEWID(),
    @CategoryKey = 'currencies',
    @Key = 'INR',
    @DisplayName = 'Indian Rupee',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 6,
    @Metadata = '{"symbol":"₹","decimalPlaces":2}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 207,
    @Id = NEWID(),
    @CategoryKey = 'currencies',
    @Key = 'CAD',
    @DisplayName = 'Canadian Dollar',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 7,
    @Metadata = '{"symbol":"$","decimalPlaces":2}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 208,
    @Id = NEWID(),
    @CategoryKey = 'currencies',
    @Key = 'AUD',
    @DisplayName = 'Australian Dollar',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 8,
    @Metadata = '{"symbol":"$","decimalPlaces":2}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 209,
    @Id = NEWID(),
    @CategoryKey = 'currencies',
    @Key = 'CHF',
    @DisplayName = 'Swiss Franc',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 9,
    @Metadata = '{"symbol":"Fr","decimalPlaces":2}';

EXEC Master.sp_LovItem_Upsert
    @IntId = 210,
    @Id = NEWID(),
    @CategoryKey = 'currencies',
    @Key = 'BRL',
    @DisplayName = 'Brazilian Real',
    @TenantId = NULL,
    @IsGlobalScope = 1,
    @IsActive = 1,
    @IsDeleted = 0,
    @CreatedAt = @Now,
    @CreatedBy = 'system',
    @SortOrder = 10,
    @Metadata = '{"symbol":"R$","decimalPlaces":2}';

PRINT '✓ v2 Schema created successfully';
PRINT '✓ Master.LovItems table created with UPSERT procedures';
PRINT '✓ 40 global lookups seeded (10 timezones, 10 countries, 10 languages, 10 currencies)';
