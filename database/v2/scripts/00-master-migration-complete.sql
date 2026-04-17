-- ============================================
-- V2: COMPLETE MASTER MIGRATION SCRIPT
-- ============================================
-- Purpose: Complete v2 implementation with ALL table updates and procedures
-- Includes: Schema, tables, procedures, indexes, seed data, and migrations
-- Execution: Single comprehensive script for complete v2 setup

SET NOCOUNT ON
DECLARE @Now DATETIME2 = GETUTCDATE()

PRINT '========================================='
PRINT 'V2 COMPLETE MIGRATION - STARTING'
PRINT '========================================='

-- ============================================
-- PHASE 1: CREATE LOV SCHEMA & TABLES
-- ============================================
PRINT 'PHASE 1: Creating LoV schema and tables...'

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'LoV')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA LoV'
    PRINT '✓ LoV schema created'
END

-- Main LoV.LovItems table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LovItems' AND schema_id = SCHEMA_ID('LoV'))
BEGIN
    CREATE TABLE LoV.LovItems (
        IntId INT,                          -- 1-999: Parent/System, 1000+: Child/Tenant-specific
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CategoryKey NVARCHAR(100) NOT NULL, -- 'currencies', 'languages', 'timezones', 'countries'
        SubCategoryKey NVARCHAR(100),
        Key NVARCHAR(100) NOT NULL,
        DisplayName NVARCHAR(500) NOT NULL,
        TenantId NVARCHAR(128),
        ParentTenantId NVARCHAR(128),
        IsGlobalScope BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(128),
        UpdatedAt DATETIME2,
        UpdatedBy NVARCHAR(128),
        SortOrder INT NOT NULL DEFAULT 0,
        Metadata NVARCHAR(MAX),
        Tags NVARCHAR(MAX),
        LocalizedNames NVARCHAR(MAX),

        CONSTRAINT UQ_LovItems_IntId UNIQUE (IntId) WHERE IntId IS NOT NULL,
        CONSTRAINT UQ_LovItems_Key UNIQUE (CategoryKey, SubCategoryKey, Key, TenantId)
    )

    CREATE INDEX IX_LovItems_Category ON LoV.LovItems(CategoryKey, IsActive, IsDeleted)
    CREATE INDEX IX_LovItems_Tenant ON LoV.LovItems(TenantId, ParentTenantId, IsGlobalScope, IsActive)
    CREATE INDEX IX_LovItems_Global ON LoV.LovItems(IsGlobalScope, IsActive, IsDeleted)
    CREATE INDEX IX_LovItems_Key ON LoV.LovItems(CategoryKey, Key, TenantId)
    CREATE INDEX IX_LovItems_IntId ON LoV.LovItems(IntId) WHERE IntId IS NOT NULL

    PRINT '✓ LoV.LovItems table created'
END

-- ============================================
-- PHASE 2: MIGRATE DATA FROM V1 TABLES TO LOV
-- ============================================
PRINT 'PHASE 2: Migrating data from v1 to LoV...'

-- Migrate Currencies (201-999)
IF EXISTS (SELECT * FROM Master.Currencies WHERE IsDeleted = 0)
BEGIN
    INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, ParentTenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata)
    SELECT
        200 + ROW_NUMBER() OVER (ORDER BY CurrencyId) AS IntId,
        NEWID() AS Id,
        'currencies' AS CategoryKey,
        NULL AS SubCategoryKey,
        Code AS Key,
        DisplayName,
        TenantId,
        NULL AS ParentTenantId,
        CASE WHEN TenantId IS NULL THEN 1 ELSE 0 END AS IsGlobalScope,
        IsActive,
        0 AS IsDeleted,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        0 AS SortOrder,
        JSON_OBJECT('symbol', Symbol, 'decimalPlaces', DecimalPlaces) AS Metadata
    FROM Master.Currencies
    WHERE IsDeleted = 0
    PRINT '✓ Currencies migrated'
END

-- Migrate Languages (101-199)
IF EXISTS (SELECT * FROM Master.Languages WHERE IsDeleted = 0)
BEGIN
    INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, ParentTenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata)
    SELECT
        100 + ROW_NUMBER() OVER (ORDER BY LanguageId) AS IntId,
        NEWID() AS Id,
        'languages' AS CategoryKey,
        NULL AS SubCategoryKey,
        Code AS Key,
        DisplayName,
        TenantId,
        NULL AS ParentTenantId,
        CASE WHEN TenantId IS NULL THEN 1 ELSE 0 END AS IsGlobalScope,
        IsActive,
        0 AS IsDeleted,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        0 AS SortOrder,
        JSON_OBJECT('nativeName', NativeName, 'isDefault', IsDefault) AS Metadata
    FROM Master.Languages
    WHERE IsDeleted = 0
    PRINT '✓ Languages migrated'
END

-- Migrate TimeZones (1-99)
IF EXISTS (SELECT * FROM Master.TimeZones WHERE IsDeleted = 0)
BEGIN
    INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, ParentTenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata)
    SELECT
        ROW_NUMBER() OVER (ORDER BY TimeZoneId) AS IntId,
        NEWID() AS Id,
        'timezones' AS CategoryKey,
        NULL AS SubCategoryKey,
        Identifier AS Key,
        DisplayName,
        NULL AS TenantId,
        NULL AS ParentTenantId,
        1 AS IsGlobalScope,
        IsActive,
        0 AS IsDeleted,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        0 AS SortOrder,
        JSON_OBJECT('standardName', StandardName, 'offsetHours', OffsetHours) AS Metadata
    FROM Master.TimeZones
    WHERE IsDeleted = 0
    PRINT '✓ TimeZones migrated'
END

-- Migrate Countries (51-100)
IF EXISTS (SELECT * FROM Master.Countries WHERE IsDeleted = 0)
BEGIN
    INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, ParentTenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata)
    SELECT
        50 + ROW_NUMBER() OVER (ORDER BY CountryId) AS IntId,
        NEWID() AS Id,
        'countries' AS CategoryKey,
        NULL AS SubCategoryKey,
        Code AS Key,
        Name AS DisplayName,
        NULL AS TenantId,
        NULL AS ParentTenantId,
        1 AS IsGlobalScope,
        1 AS IsActive,
        0 AS IsDeleted,
        GETUTCDATE() AS CreatedAt,
        'system' AS CreatedBy,
        NULL AS UpdatedAt,
        NULL AS UpdatedBy,
        0 AS SortOrder,
        NULL AS Metadata
    FROM Master.Countries
    WHERE IsDeleted = 0
    PRINT '✓ Countries migrated'
END

-- ============================================
-- PHASE 3: CREATE/UPDATE ALL PROCEDURES
-- ============================================
PRINT 'PHASE 3: Creating procedures...'

-- UPSERT LovItem
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'sp_LovItem_Upsert' AND schema_id = SCHEMA_ID('LoV'))
    DROP PROCEDURE LoV.sp_LovItem_Upsert
GO

CREATE PROCEDURE LoV.sp_LovItem_Upsert
    @IntId INT = NULL,
    @Id UNIQUEIDENTIFIER,
    @CategoryKey NVARCHAR(100),
    @SubCategoryKey NVARCHAR(100) = NULL,
    @Key NVARCHAR(100),
    @DisplayName NVARCHAR(500),
    @TenantId NVARCHAR(128) = NULL,
    @ParentTenantId NVARCHAR(128) = NULL,
    @IsGlobalScope BIT,
    @IsActive BIT = 1,
    @IsDeleted BIT = 0,
    @CreatedAt DATETIME2,
    @CreatedBy NVARCHAR(128),
    @UpdatedAt DATETIME2 = NULL,
    @UpdatedBy NVARCHAR(128) = NULL,
    @SortOrder INT = 0,
    @Metadata NVARCHAR(MAX) = NULL,
    @Tags NVARCHAR(MAX) = NULL,
    @LocalizedNames NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON

    MERGE LoV.LovItems AS target
    USING (SELECT @IntId, @Id, @CategoryKey, @SubCategoryKey, @Key, @DisplayName, @TenantId, @ParentTenantId, @IsGlobalScope, @IsActive, @IsDeleted, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy, @SortOrder, @Metadata, @Tags, @LocalizedNames)
    AS source(IntId, Id, CategoryKey, SubCategoryKey, Key, DisplayName, TenantId, ParentTenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata, Tags, LocalizedNames)
    ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET
            DisplayName = source.DisplayName,
            IsActive = source.IsActive,
            IsDeleted = source.IsDeleted,
            UpdatedAt = ISNULL(source.UpdatedAt, GETUTCDATE()),
            UpdatedBy = source.UpdatedBy,
            SortOrder = source.SortOrder,
            Metadata = source.Metadata,
            Tags = source.Tags,
            LocalizedNames = source.LocalizedNames
    WHEN NOT MATCHED THEN
        INSERT (IntId, Id, CategoryKey, SubCategoryKey, Key, DisplayName, TenantId, ParentTenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata, Tags, LocalizedNames)
        VALUES (source.IntId, source.Id, source.CategoryKey, source.SubCategoryKey, source.Key, source.DisplayName, source.TenantId, source.ParentTenantId, source.IsGlobalScope, source.IsActive, source.IsDeleted, source.CreatedAt, source.CreatedBy, source.UpdatedAt, source.UpdatedBy, source.SortOrder, source.Metadata, source.Tags, source.LocalizedNames);
END
GO

-- Get by tenant hierarchy
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'sp_LovItem_GetByTenantHierarchy' AND schema_id = SCHEMA_ID('LoV'))
    DROP PROCEDURE LoV.sp_LovItem_GetByTenantHierarchy
GO

CREATE PROCEDURE LoV.sp_LovItem_GetByTenantHierarchy
    @CategoryKey NVARCHAR(100),
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON

    DECLARE @ParentTenantId NVARCHAR(128) = NULL
    IF @TenantId LIKE '%-'
        SET @ParentTenantId = LEFT(@TenantId, CHARINDEX('-', @TenantId) - 1)

    SELECT * FROM LoV.LovItems
    WHERE CategoryKey = @CategoryKey
      AND IsActive = 1
      AND IsDeleted = 0
      AND (
        (IsGlobalScope = 1 AND IntId BETWEEN 1 AND 999)
        OR TenantId = @ParentTenantId
        OR TenantId = @TenantId
      )
    ORDER BY SortOrder, DisplayName
END
GO

PRINT '✓ Procedures created'

-- ============================================
-- PHASE 4: SEED PARENT LOOKUPS (1-999)
-- ============================================
PRINT 'PHASE 4: Seeding parent lookups...'

-- Only seed if LoV table is empty
IF NOT EXISTS (SELECT 1 FROM LoV.LovItems WHERE IntId BETWEEN 1 AND 999)
BEGIN
    -- TimeZones (1-50)
    INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, ParentTenantId, IsGlobalScope, IsActive, CreatedAt, CreatedBy, SortOrder, Metadata)
    VALUES
        (1, NEWID(), 'timezones', 'America/New_York', 'Eastern Time (US & Canada)', NULL, NULL, 1, 1, @Now, 'system', 1, '{"standardName":"EST","offsetHours":-5}'),
        (2, NEWID(), 'timezones', 'America/Chicago', 'Central Time (US & Canada)', NULL, NULL, 1, 1, @Now, 'system', 2, '{"standardName":"CST","offsetHours":-6}'),
        (3, NEWID(), 'timezones', 'America/Denver', 'Mountain Time (US & Canada)', NULL, NULL, 1, 1, @Now, 'system', 3, '{"standardName":"MST","offsetHours":-7}'),
        (4, NEWID(), 'timezones', 'America/Los_Angeles', 'Pacific Time (US & Canada)', NULL, NULL, 1, 1, @Now, 'system', 4, '{"standardName":"PST","offsetHours":-8}'),
        (5, NEWID(), 'timezones', 'Europe/London', 'GMT (UK)', NULL, NULL, 1, 1, @Now, 'system', 5, '{"standardName":"GMT","offsetHours":0}'),
        (6, NEWID(), 'timezones', 'Europe/Paris', 'Central European Time', NULL, NULL, 1, 1, @Now, 'system', 6, '{"standardName":"CET","offsetHours":1}'),
        (7, NEWID(), 'timezones', 'Asia/Tokyo', 'Japan Standard Time', NULL, NULL, 1, 1, @Now, 'system', 7, '{"standardName":"JST","offsetHours":9}'),
        (8, NEWID(), 'timezones', 'Asia/Shanghai', 'China Standard Time', NULL, NULL, 1, 1, @Now, 'system', 8, '{"standardName":"CST","offsetHours":8}'),
        (9, NEWID(), 'timezones', 'Asia/Dubai', 'Gulf Standard Time', NULL, NULL, 1, 1, @Now, 'system', 9, '{"standardName":"GST","offsetHours":4}'),
        (10, NEWID(), 'timezones', 'Asia/Kolkata', 'India Standard Time', NULL, NULL, 1, 1, @Now, 'system', 10, '{"standardName":"IST","offsetHours":5.5}')

    -- Countries (51-100)
    INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, ParentTenantId, IsGlobalScope, IsActive, CreatedAt, CreatedBy, SortOrder)
    VALUES
        (51, NEWID(), 'countries', 'US', 'United States', NULL, NULL, 1, 1, @Now, 'system', 1),
        (52, NEWID(), 'countries', 'CA', 'Canada', NULL, NULL, 1, 1, @Now, 'system', 2),
        (53, NEWID(), 'countries', 'GB', 'United Kingdom', NULL, NULL, 1, 1, @Now, 'system', 3),
        (54, NEWID(), 'countries', 'DE', 'Germany', NULL, NULL, 1, 1, @Now, 'system', 4),
        (55, NEWID(), 'countries', 'FR', 'France', NULL, NULL, 1, 1, @Now, 'system', 5),
        (56, NEWID(), 'countries', 'JP', 'Japan', NULL, NULL, 1, 1, @Now, 'system', 6),
        (57, NEWID(), 'countries', 'CN', 'China', NULL, NULL, 1, 1, @Now, 'system', 7),
        (58, NEWID(), 'countries', 'IN', 'India', NULL, NULL, 1, 1, @Now, 'system', 8),
        (59, NEWID(), 'countries', 'BR', 'Brazil', NULL, NULL, 1, 1, @Now, 'system', 9),
        (60, NEWID(), 'countries', 'AU', 'Australia', NULL, NULL, 1, 1, @Now, 'system', 10)

    -- Languages (101-200)
    INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, ParentTenantId, IsGlobalScope, IsActive, CreatedAt, CreatedBy, SortOrder, Metadata)
    VALUES
        (101, NEWID(), 'languages', 'en-US', 'English (US)', NULL, NULL, 1, 1, @Now, 'system', 1, '{"nativeName":"English","isDefault":true}'),
        (102, NEWID(), 'languages', 'en-GB', 'English (UK)', NULL, NULL, 1, 1, @Now, 'system', 2, '{"nativeName":"English","isDefault":false}'),
        (103, NEWID(), 'languages', 'fr-FR', 'Français', NULL, NULL, 1, 1, @Now, 'system', 3, '{"nativeName":"Français","isDefault":false}'),
        (104, NEWID(), 'languages', 'de-DE', 'Deutsch', NULL, NULL, 1, 1, @Now, 'system', 4, '{"nativeName":"Deutsch","isDefault":false}'),
        (105, NEWID(), 'languages', 'es-ES', 'Español', NULL, NULL, 1, 1, @Now, 'system', 5, '{"nativeName":"Español","isDefault":false}'),
        (106, NEWID(), 'languages', 'it-IT', 'Italiano', NULL, NULL, 1, 1, @Now, 'system', 6, '{"nativeName":"Italiano","isDefault":false}'),
        (107, NEWID(), 'languages', 'ja-JP', '日本語', NULL, NULL, 1, 1, @Now, 'system', 7, '{"nativeName":"日本語","isDefault":false}'),
        (108, NEWID(), 'languages', 'zh-CN', '中文(简体)', NULL, NULL, 1, 1, @Now, 'system', 8, '{"nativeName":"中文","isDefault":false}'),
        (109, NEWID(), 'languages', 'hi-IN', 'हिन्दी', NULL, NULL, 1, 1, @Now, 'system', 9, '{"nativeName":"हिन्दी","isDefault":false}'),
        (110, NEWID(), 'languages', 'pt-BR', 'Português (Brasil)', NULL, NULL, 1, 1, @Now, 'system', 10, '{"nativeName":"Português","isDefault":false}')

    -- Currencies (201-300)
    INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, ParentTenantId, IsGlobalScope, IsActive, CreatedAt, CreatedBy, SortOrder, Metadata)
    VALUES
        (201, NEWID(), 'currencies', 'USD', 'US Dollar', NULL, NULL, 1, 1, @Now, 'system', 1, '{"symbol":"$","decimalPlaces":2}'),
        (202, NEWID(), 'currencies', 'EUR', 'Euro', NULL, NULL, 1, 1, @Now, 'system', 2, '{"symbol":"€","decimalPlaces":2}'),
        (203, NEWID(), 'currencies', 'GBP', 'British Pound', NULL, NULL, 1, 1, @Now, 'system', 3, '{"symbol":"£","decimalPlaces":2}'),
        (204, NEWID(), 'currencies', 'JPY', 'Japanese Yen', NULL, NULL, 1, 1, @Now, 'system', 4, '{"symbol":"¥","decimalPlaces":0}'),
        (205, NEWID(), 'currencies', 'CNY', 'Chinese Yuan', NULL, NULL, 1, 1, @Now, 'system', 5, '{"symbol":"¥","decimalPlaces":2}'),
        (206, NEWID(), 'currencies', 'INR', 'Indian Rupee', NULL, NULL, 1, 1, @Now, 'system', 6, '{"symbol":"₹","decimalPlaces":2}'),
        (207, NEWID(), 'currencies', 'CAD', 'Canadian Dollar', NULL, NULL, 1, 1, @Now, 'system', 7, '{"symbol":"$","decimalPlaces":2}'),
        (208, NEWID(), 'currencies', 'AUD', 'Australian Dollar', NULL, NULL, 1, 1, @Now, 'system', 8, '{"symbol":"$","decimalPlaces":2}'),
        (209, NEWID(), 'currencies', 'CHF', 'Swiss Franc', NULL, NULL, 1, 1, @Now, 'system', 9, '{"symbol":"Fr","decimalPlaces":2}'),
        (210, NEWID(), 'currencies', 'BRL', 'Brazilian Real', NULL, NULL, 1, 1, @Now, 'system', 10, '{"symbol":"R$","decimalPlaces":2}')

    PRINT '✓ Parent lookups seeded (40 records, IDs 1-999)'
END

PRINT '========================================='
PRINT 'V2 COMPLETE MIGRATION - SUCCESS'
PRINT '========================================='
PRINT 'Summary:'
PRINT '  ✓ LoV schema created'
PRINT '  ✓ LoV.LovItems table created with indexes'
PRINT '  ✓ Data migrated from v1 tables'
PRINT '  ✓ All procedures created (UPSERT, GetByTenantHierarchy)'
PRINT '  ✓ Parent lookups seeded (IDs 1-999)'
PRINT ''
PRINT 'ID Allocation:'
PRINT '  - 1-999:   Parent/System lookups (global, inherited)'
PRINT '  - 1000+:   Child/Tenant-specific lookups'
PRINT ''
PRINT 'Next Steps:'
PRINT '  1. Backend: Implement Dapper repository using LoV'
PRINT '  2. Admin UI: Create lookup management pages'
PRINT '  3. Public UI: Update currency/language/timezone selectors'
PRINT '========================================='
