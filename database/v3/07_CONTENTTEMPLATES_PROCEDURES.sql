-- ============================================
-- ContentTemplates Stored Procedures
-- Purpose: CRUD operations for Master.ContentTemplates, ContentTemplateSections, TemplatePlaceholders
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;
GO

-- ==========================================
-- ContentTemplates Main Table Procedures
-- ==========================================

-- ============================================
-- spUpsertContentTemplate - Create or update template
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spUpsertContentTemplate]
    @Id NVARCHAR(128) = NULL OUTPUT,
    @Name NVARCHAR(255),
    @Description NVARCHAR(MAX) = NULL,
    @TemplateType NVARCHAR(50),
    @Subject NVARCHAR(500) = NULL,
    @HeaderId NVARCHAR(128) = NULL,
    @FooterId NVARCHAR(128) = NULL,
    @BodyContent NVARCHAR(MAX),
    @PlainTextContent NVARCHAR(MAX) = NULL,
    @Tags NVARCHAR(500) = NULL,
    @Category NVARCHAR(100) = NULL,
    @TenantId NVARCHAR(128),
    @IsActive BIT = 1,
    @IsSystem BIT = 0,
    @Version INT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id IS NULL OR @Id = ''
    BEGIN
        -- Generate new ID
        SET @Id = NEWID();

        -- INSERT new template
        INSERT INTO [Master].[ContentTemplates]
        (Id, Name, Description, TemplateType, Subject, HeaderId, FooterId, BodyContent, PlainTextContent,
         Tags, Category, TenantId, IsActive, IsSystem, Version, CreatedAt, UpdatedAt)
        VALUES
        (@Id, @Name, @Description, @TemplateType, @Subject, @HeaderId, @FooterId, @BodyContent, @PlainTextContent,
         @Tags, @Category, @TenantId, @IsActive, @IsSystem, @Version, GETUTCDATE(), GETUTCDATE());
    END
    ELSE
    BEGIN
        -- UPDATE existing template
        UPDATE [Master].[ContentTemplates]
        SET
            Name = @Name,
            Description = @Description,
            TemplateType = @TemplateType,
            Subject = @Subject,
            HeaderId = @HeaderId,
            FooterId = @FooterId,
            BodyContent = @BodyContent,
            PlainTextContent = @PlainTextContent,
            Tags = @Tags,
            Category = @Category,
            IsActive = @IsActive,
            IsSystem = @IsSystem,
            Version = @Version,
            UpdatedAt = GETUTCDATE()
        WHERE Id = @Id AND TenantId = @TenantId;
    END

    SELECT @Id AS TemplateId;
END;
GO

-- ============================================
-- spGetContentTemplate - Get single template with sections
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetContentTemplate]
    @TemplateId NVARCHAR(128),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Name,
        Description,
        TemplateType,
        Subject,
        HeaderId,
        FooterId,
        BodyContent,
        PlainTextContent,
        Tags,
        Category,
        TenantId,
        IsActive,
        IsSystem,
        Version,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[ContentTemplates]
    WHERE Id = @TemplateId
        AND TenantId = @TenantId
        AND IsDeleted = 0;
END;
GO

-- ============================================
-- spGetContentTemplatesByType - Get templates by type
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetContentTemplatesByType]
    @TemplateType NVARCHAR(50),
    @TenantId NVARCHAR(128),
    @OnlyActive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Name,
        Description,
        TemplateType,
        Subject,
        HeaderId,
        FooterId,
        BodyContent,
        PlainTextContent,
        Tags,
        Category,
        TenantId,
        IsActive,
        IsSystem,
        Version,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[ContentTemplates]
    WHERE TemplateType = @TemplateType
        AND TenantId = @TenantId
        AND (@OnlyActive = 0 OR IsActive = 1)
        AND IsDeleted = 0
    ORDER BY Name;
END;
GO

-- ============================================
-- spGetContentTemplatesByCategory - Get templates by category
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetContentTemplatesByCategory]
    @Category NVARCHAR(100),
    @TenantId NVARCHAR(128),
    @OnlyActive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Name,
        Description,
        TemplateType,
        Subject,
        Category,
        TenantId,
        IsActive,
        IsSystem,
        Version,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[ContentTemplates]
    WHERE Category = @Category
        AND TenantId = @TenantId
        AND (@OnlyActive = 0 OR IsActive = 1)
        AND IsDeleted = 0
    ORDER BY Name;
END;
GO

-- ============================================
-- spDeleteContentTemplate - Soft delete template
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spDeleteContentTemplate]
    @TemplateId NVARCHAR(128),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Delete template
        UPDATE [Master].[ContentTemplates]
        SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
        WHERE Id = @TemplateId AND TenantId = @TenantId;

        -- Delete sections
        UPDATE [Master].[ContentTemplateSections]
        SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
        WHERE TemplateId = @TemplateId AND TenantId = @TenantId;

        -- Delete placeholders
        UPDATE [Master].[TemplatePlaceholders]
        SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
        WHERE TemplateId = @TemplateId AND TenantId = @TenantId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ==========================================
-- ContentTemplateSections Procedures
-- ==========================================

-- ============================================
-- spUpsertContentTemplateSection - Create or update section
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spUpsertContentTemplateSection]
    @SectionId NVARCHAR(128) = NULL OUTPUT,
    @TemplateId NVARCHAR(128),
    @TenantId NVARCHAR(128),
    @Name NVARCHAR(255),
    @SectionType NVARCHAR(50),
    @HtmlContent NVARCHAR(MAX),
    @IsDefault BIT = 0,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @SectionId IS NULL OR @SectionId = ''
    BEGIN
        SET @SectionId = NEWID();

        INSERT INTO [Master].[ContentTemplateSections]
        (Id, TemplateId, TenantId, Name, SectionType, HtmlContent, IsDefault, IsActive, CreatedAt, UpdatedAt)
        VALUES
        (@SectionId, @TemplateId, @TenantId, @Name, @SectionType, @HtmlContent, @IsDefault, @IsActive, GETUTCDATE(), GETUTCDATE());
    END
    ELSE
    BEGIN
        UPDATE [Master].[ContentTemplateSections]
        SET
            Name = @Name,
            SectionType = @SectionType,
            HtmlContent = @HtmlContent,
            IsDefault = @IsDefault,
            IsActive = @IsActive,
            UpdatedAt = GETUTCDATE()
        WHERE Id = @SectionId AND TenantId = @TenantId;
    END

    SELECT @SectionId AS SectionId;
END;
GO

-- ============================================
-- spGetContentTemplateSections - Get sections for template
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetContentTemplateSections]
    @TemplateId NVARCHAR(128),
    @TenantId NVARCHAR(128),
    @OnlyActive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        TemplateId,
        TenantId,
        Name,
        SectionType,
        HtmlContent,
        IsDefault,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[ContentTemplateSections]
    WHERE TemplateId = @TemplateId
        AND TenantId = @TenantId
        AND (@OnlyActive = 0 OR IsActive = 1)
        AND IsDeleted = 0
    ORDER BY CreatedAt;
END;
GO

-- ============================================
-- spDeleteContentTemplateSection - Soft delete section
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spDeleteContentTemplateSection]
    @SectionId NVARCHAR(128),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Master].[ContentTemplateSections]
    SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
    WHERE Id = @SectionId AND TenantId = @TenantId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- ==========================================
-- TemplatePlaceholders Procedures
-- ==========================================

-- ============================================
-- spUpsertTemplatePlaceholder - Create or update placeholder
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spUpsertTemplatePlaceholder]
    @PlaceholderId INT = NULL OUTPUT,
    @TemplateId NVARCHAR(128),
    @TenantId NVARCHAR(128),
    @PlaceholderKey NVARCHAR(128),
    @DisplayName NVARCHAR(255),
    @Description NVARCHAR(500) = NULL,
    @DefaultValue NVARCHAR(MAX) = NULL,
    @SampleValue NVARCHAR(MAX) = NULL,
    @PlaceholderType NVARCHAR(50) = 'Text',
    @IsRequired BIT = 0,
    @DisplayOrder INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @PlaceholderId IS NULL OR @PlaceholderId = 0
    BEGIN
        INSERT INTO [Master].[TemplatePlaceholders]
        (TemplateId, TenantId, PlaceholderKey, DisplayName, Description, DefaultValue, SampleValue,
         PlaceholderType, IsRequired, DisplayOrder, CreatedAt, UpdatedAt)
        VALUES
        (@TemplateId, @TenantId, @PlaceholderKey, @DisplayName, @Description, @DefaultValue, @SampleValue,
         @PlaceholderType, @IsRequired, @DisplayOrder, GETUTCDATE(), GETUTCDATE());

        SET @PlaceholderId = CAST(SCOPE_IDENTITY() AS INT);
    END
    ELSE
    BEGIN
        UPDATE [Master].[TemplatePlaceholders]
        SET
            PlaceholderKey = @PlaceholderKey,
            DisplayName = @DisplayName,
            Description = @Description,
            DefaultValue = @DefaultValue,
            SampleValue = @SampleValue,
            PlaceholderType = @PlaceholderType,
            IsRequired = @IsRequired,
            DisplayOrder = @DisplayOrder,
            UpdatedAt = GETUTCDATE()
        WHERE PlaceholderId = @PlaceholderId AND TenantId = @TenantId;
    END

    SELECT @PlaceholderId AS PlaceholderId;
END;
GO

-- ============================================
-- spGetTemplatePlaceholders - Get placeholders for template
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spGetTemplatePlaceholders]
    @TemplateId NVARCHAR(128),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PlaceholderId,
        TemplateId,
        TenantId,
        PlaceholderKey,
        DisplayName,
        Description,
        DefaultValue,
        SampleValue,
        PlaceholderType,
        IsRequired,
        DisplayOrder,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Master].[TemplatePlaceholders]
    WHERE TemplateId = @TemplateId
        AND TenantId = @TenantId
        AND IsDeleted = 0
    ORDER BY DisplayOrder, DisplayName;
END;
GO

-- ============================================
-- spDeleteTemplatePlaceholder - Soft delete placeholder
-- ============================================
CREATE OR ALTER PROCEDURE [Master].[spDeleteTemplatePlaceholder]
    @PlaceholderId INT,
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Master].[TemplatePlaceholders]
    SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
    WHERE PlaceholderId = @PlaceholderId AND TenantId = @TenantId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

PRINT '✓ ContentTemplates stored procedures created successfully';
