-- ============================================
-- V2: Create LoV.LovItems Table
-- ============================================
-- Purpose: Unified lookup table consolidating Currencies, Languages, TimeZones, Countries
-- Fields: IntId (1-999 for system), Id (GUID), CategoryKey, Key, DisplayName, Metadata (JSON)

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LovItems' AND schema_id = SCHEMA_ID('LoV'))
BEGIN
    CREATE TABLE LoV.LovItems (
        IntId INT,
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CategoryKey NVARCHAR(100) NOT NULL,
        SubCategoryKey NVARCHAR(100),
        Key NVARCHAR(100) NOT NULL,
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
        Metadata NVARCHAR(MAX),        -- JSON: { "symbol": "$", "decimalPlaces": 2 }
        Tags NVARCHAR(MAX),            -- JSON: ["tag1", "tag2"]
        LocalizedNames NVARCHAR(MAX),  -- JSON: { "en-US": "English", "es-ES": "Español" }

        CONSTRAINT UQ_LovItems_IntId UNIQUE (IntId) WHERE IntId IS NOT NULL,
        CONSTRAINT UQ_LovItems_Key UNIQUE (CategoryKey, SubCategoryKey, Key, TenantId)
    )

    -- Create indexes for performance
    CREATE INDEX IX_LovItems_Category ON LoV.LovItems(CategoryKey, IsActive, IsDeleted)
    CREATE INDEX IX_LovItems_Tenant ON LoV.LovItems(TenantId, IsGlobalScope, IsActive)
    CREATE INDEX IX_LovItems_Global ON LoV.LovItems(IsGlobalScope, IsActive, IsDeleted)
    CREATE INDEX IX_LovItems_Key ON LoV.LovItems(CategoryKey, Key, TenantId)

    PRINT 'LoV.LovItems table created successfully.'
END
ELSE
BEGIN
    PRINT 'LoV.LovItems table already exists.'
END
