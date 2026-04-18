-- ============================================
-- MenuItems Stored Procedures
-- Purpose: CRUD operations for Master.MenuItems
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;
GO

-- ============================================
-- spUpsertMenuItem - Create or update menu item
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spUpsertMenuItem]
    @MenuItemId INT = NULL,
    @MenuId INT,
    @Title NVARCHAR(255),
    @Url NVARCHAR(500),
    @Icon NVARCHAR(100) = NULL,
    @DisplayOrder INT = 0,
    @RequiredRole NVARCHAR(128) = NULL,
    @TenantId NVARCHAR(128),
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @MenuItemId IS NULL OR @MenuItemId = 0
    BEGIN
        -- INSERT new menu item
        INSERT INTO [Master].[MenuItems]
        (MenuId, Title, [Url], Icon, DisplayOrder, RequiredRole, TenantId, IsActive, CreatedAt, UpdatedAt)
        VALUES
        (@MenuId, @Title, @Url, @Icon, @DisplayOrder, @RequiredRole, @TenantId, @IsActive, GETUTCDATE(), GETUTCDATE());

        SELECT CAST(SCOPE_IDENTITY() AS INT) AS MenuItemId;
    END
    ELSE
    BEGIN
        -- UPDATE existing menu item
        UPDATE [Master].[MenuItems]
        SET
            Title = @Title,
            [Url] = @Url,
            Icon = @Icon,
            DisplayOrder = @DisplayOrder,
            RequiredRole = @RequiredRole,
            IsActive = @IsActive,
            UpdatedAt = GETUTCDATE()
        WHERE MenuItemId = @MenuItemId AND TenantId = @TenantId;

        SELECT @MenuItemId AS MenuItemId;
    END
END;
GO

-- ============================================
-- spGetMenuItems - Get all items for a menu
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetMenuItems]
    @MenuId INT,
    @TenantId NVARCHAR(128),
    @OnlyActive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MenuItemId,
        MenuId,
        Title,
        [Url],
        Icon,
        DisplayOrder,
        RequiredRole,
        TenantId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[MenuItems]
    WHERE MenuId = @MenuId
        AND TenantId = @TenantId
        AND (@OnlyActive = 0 OR IsActive = 1)
        AND IsDeleted = 0
    ORDER BY DisplayOrder, Title;
END;
GO

-- ============================================
-- spGetMenuItemById - Get single menu item
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetMenuItemById]
    @MenuItemId INT,
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MenuItemId,
        MenuId,
        Title,
        [Url],
        Icon,
        DisplayOrder,
        RequiredRole,
        TenantId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[MenuItems]
    WHERE MenuItemId = @MenuItemId
        AND TenantId = @TenantId
        AND IsDeleted = 0;
END;
GO

-- ============================================
-- spDeleteMenuItem - Soft delete menu item
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spDeleteMenuItem]
    @MenuItemId INT,
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Master].[MenuItems]
    SET
        IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE MenuItemId = @MenuItemId
        AND TenantId = @TenantId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- ============================================
-- spReorderMenuItems - Update display order for multiple items
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spReorderMenuItems]
    @MenuId INT,
    @TenantId NVARCHAR(128),
    @OrderJson NVARCHAR(MAX) -- JSON array: [{"id": 1, "order": 1}, {"id": 2, "order": 2}]
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Parse JSON and update display order
        UPDATE mi
        SET mi.DisplayOrder = j.DisplayOrder,
            mi.UpdatedAt = GETUTCDATE()
        FROM [Master].[MenuItems] mi
        INNER JOIN OPENJSON(@OrderJson)
            WITH (
                id INT '$.id',
                DisplayOrder INT '$.order'
            ) j ON mi.MenuItemId = j.id
        WHERE mi.MenuId = @MenuId
            AND mi.TenantId = @TenantId
            AND mi.IsDeleted = 0;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT '✓ MenuItems stored procedures created successfully';
