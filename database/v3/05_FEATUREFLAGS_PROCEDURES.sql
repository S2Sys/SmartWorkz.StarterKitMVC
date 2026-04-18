-- ============================================
-- FeatureFlags Stored Procedures
-- Purpose: CRUD operations for Master.FeatureFlags
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;
GO

-- ============================================
-- spUpsertFeatureFlag - Create or update feature flag
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spUpsertFeatureFlag]
    @FeatureFlagId INT = NULL OUTPUT,
    @Name NVARCHAR(255),
    @Description NVARCHAR(1000) = NULL,
    @IsEnabled BIT = 0,
    @TenantId NVARCHAR(128) = NULL,
    @ValidFrom DATETIME2 = NULL,
    @ValidTo DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @FeatureFlagId IS NULL OR @FeatureFlagId = 0
    BEGIN
        -- INSERT new feature flag
        INSERT INTO [Master].[FeatureFlags]
        (Name, Description, IsEnabled, TenantId, ValidFrom, ValidTo, CreatedAt, UpdatedAt)
        VALUES
        (@Name, @Description, @IsEnabled, @TenantId, @ValidFrom, @ValidTo, GETUTCDATE(), GETUTCDATE());

        SET @FeatureFlagId = CAST(SCOPE_IDENTITY() AS INT);
    END
    ELSE
    BEGIN
        -- UPDATE existing feature flag
        UPDATE [Master].[FeatureFlags]
        SET
            Name = @Name,
            Description = @Description,
            IsEnabled = @IsEnabled,
            TenantId = @TenantId,
            ValidFrom = @ValidFrom,
            ValidTo = @ValidTo,
            UpdatedAt = GETUTCDATE()
        WHERE FeatureFlagId = @FeatureFlagId;
    END

    SELECT @FeatureFlagId AS FeatureFlagId;
END;
GO

-- ============================================
-- spGetFeatureFlag - Get single feature flag
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetFeatureFlag]
    @FeatureFlagId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FeatureFlagId,
        Name,
        Description,
        IsEnabled,
        TenantId,
        ValidFrom,
        ValidTo,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[FeatureFlags]
    WHERE FeatureFlagId = @FeatureFlagId
        AND IsDeleted = 0;
END;
GO

-- ============================================
-- spGetFeatureFlagByName - Get flag by name
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetFeatureFlagByName]
    @Name NVARCHAR(255),
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FeatureFlagId,
        Name,
        Description,
        IsEnabled,
        TenantId,
        ValidFrom,
        ValidTo,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[FeatureFlags]
    WHERE Name = @Name
        AND (TenantId = @TenantId OR @TenantId IS NULL)
        AND IsDeleted = 0;
END;
GO

-- ============================================
-- spGetActiveFeatureFlags - Get all active flags for tenant
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetActiveFeatureFlags]
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FeatureFlagId,
        Name,
        Description,
        IsEnabled,
        TenantId,
        ValidFrom,
        ValidTo,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[FeatureFlags]
    WHERE IsEnabled = 1
        AND (TenantId = @TenantId OR TenantId IS NULL)
        AND (ValidFrom IS NULL OR ValidFrom <= GETUTCDATE())
        AND (ValidTo IS NULL OR ValidTo >= GETUTCDATE())
        AND IsDeleted = 0
    ORDER BY TenantId DESC, Name;
END;
GO

-- ============================================
-- spGetAllFeatureFlags - Get all flags for tenant
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetAllFeatureFlags]
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FeatureFlagId,
        Name,
        Description,
        IsEnabled,
        TenantId,
        ValidFrom,
        ValidTo,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[FeatureFlags]
    WHERE (TenantId = @TenantId OR @TenantId IS NULL)
        AND IsDeleted = 0
    ORDER BY Name;
END;
GO

-- ============================================
-- spDeleteFeatureFlag - Soft delete feature flag
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spDeleteFeatureFlag]
    @FeatureFlagId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Master].[FeatureFlags]
    SET
        IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE FeatureFlagId = @FeatureFlagId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- ============================================
-- spIsFeatureEnabled - Check if feature is enabled
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spIsFeatureEnabled]
    @FlagName NVARCHAR(255),
    @TenantId NVARCHAR(128) = NULL,
    @IsEnabled BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @IsEnabled = 0;

    SELECT @IsEnabled = IsEnabled
    FROM [Master].[FeatureFlags]
    WHERE Name = @FlagName
        AND (TenantId = @TenantId OR TenantId IS NULL)
        AND IsEnabled = 1
        AND (ValidFrom IS NULL OR ValidFrom <= GETUTCDATE())
        AND (ValidTo IS NULL OR ValidTo >= GETUTCDATE())
        AND IsDeleted = 0;
END;
GO

PRINT '✓ FeatureFlags stored procedures created successfully';
