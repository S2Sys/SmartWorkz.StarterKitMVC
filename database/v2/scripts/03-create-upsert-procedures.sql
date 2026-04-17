-- ============================================
-- V2: Create UPSERT Stored Procedures
-- ============================================

-- Main UPSERT procedure
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
    USING (
        SELECT
            @IntId AS IntId,
            @Id AS Id,
            @CategoryKey AS CategoryKey,
            @SubCategoryKey AS SubCategoryKey,
            @Key AS Key,
            @DisplayName AS DisplayName,
            @TenantId AS TenantId,
            @IsGlobalScope AS IsGlobalScope,
            @IsActive AS IsActive,
            @IsDeleted AS IsDeleted,
            @CreatedAt AS CreatedAt,
            @CreatedBy AS CreatedBy,
            @UpdatedAt AS UpdatedAt,
            @UpdatedBy AS UpdatedBy,
            @SortOrder AS SortOrder,
            @Metadata AS Metadata,
            @Tags AS Tags,
            @LocalizedNames AS LocalizedNames
    ) AS source
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
        INSERT (IntId, Id, CategoryKey, SubCategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata, Tags, LocalizedNames)
        VALUES (source.IntId, source.Id, source.CategoryKey, source.SubCategoryKey, source.Key, source.DisplayName, source.TenantId, source.IsGlobalScope, source.IsActive, source.IsDeleted, source.CreatedAt, source.CreatedBy, source.UpdatedAt, source.UpdatedBy, source.SortOrder, source.Metadata, source.Tags, source.LocalizedNames);
END
GO

-- Get by tenant hierarchy (respects inheritance)
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

    -- Extract parent tenant ID (ABC-US → ABC)
    IF @TenantId LIKE '%-'
        SET @ParentTenantId = LEFT(@TenantId, CHARINDEX('-', @TenantId) - 1)

    SELECT * FROM LoV.LovItems
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

PRINT 'Stored procedures created successfully.'
