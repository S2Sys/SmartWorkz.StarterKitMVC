-- ============================================
-- SmartWorkz v4: Cache, Template, and EmailQueue Tables
-- Date: 2026-04-02
-- Adds: Master.CacheEntries, Master.ContentTemplates, Master.ContentTemplateSections,
--       Master.TemplatePlaceholders, and stored procedures for template + emailqueue operations
-- Run AFTER: 009_CreateStoredProcedures.sql
-- ============================================

USE Boilerplate;

-- ============================================
-- Master.CacheEntries — SQL Server Distributed Cache fallback
-- Fixed schema required by Microsoft.Extensions.Caching.SqlServer
-- Column names are dictated by the library and must not be changed
-- ============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'Master' AND TABLE_NAME = 'CacheEntries')
BEGIN
    CREATE TABLE [Master].[CacheEntries] (
        [Id]                         NVARCHAR(449)  NOT NULL,
        [Value]                      VARBINARY(MAX) NOT NULL,
        [ExpiresAtTime]              DATETIMEOFFSET NOT NULL,
        [SlidingExpirationInSeconds] BIGINT         NULL,
        [AbsoluteExpiration]         DATETIMEOFFSET NULL,
        CONSTRAINT [PK_CacheEntries] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_CacheEntries_ExpiresAtTime]
        ON [Master].[CacheEntries] ([ExpiresAtTime]);

    PRINT N'[010.1] Master.CacheEntries table created';
END
ELSE
    PRINT N'[010.1] Master.CacheEntries table already exists';
GO

-- ============================================
-- Master.ContentTemplates — Unified template storage (Email, SMS, Push, Notification, Report)
-- ============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'Master' AND TABLE_NAME = 'ContentTemplates')
BEGIN
    CREATE TABLE [Master].[ContentTemplates] (
        [Id]               NVARCHAR(256)  NOT NULL,
        [Name]             NVARCHAR(256)  NOT NULL,
        [Description]      NVARCHAR(500)  NULL,
        [TemplateType]     NVARCHAR(50)   NOT NULL DEFAULT 'Email',
        [Subject]          NVARCHAR(500)  NOT NULL DEFAULT '',
        [HeaderId]         NVARCHAR(256)  NULL,
        [FooterId]         NVARCHAR(256)  NULL,
        [BodyContent]      NVARCHAR(MAX)  NOT NULL DEFAULT '',
        [PlainTextContent] NVARCHAR(MAX)  NULL,
        [Tags]             NVARCHAR(MAX)  NULL,
        [Category]         NVARCHAR(100)  NULL,
        [IsActive]         BIT            NOT NULL DEFAULT 1,
        [IsSystem]         BIT            NOT NULL DEFAULT 0,
        [TenantId]         NVARCHAR(128)  NULL,
        [Version]          INT            NOT NULL DEFAULT 1,
        [CreatedAt]        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy]        NVARCHAR(256)  NULL,
        [UpdatedAt]        DATETIME2      NULL,
        [UpdatedBy]        NVARCHAR(256)  NULL,
        [IsDeleted]        BIT            NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ContentTemplates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ContentTemplates_Tenants]
            FOREIGN KEY ([TenantId]) REFERENCES [Master].[Tenants]([TenantId])
    );

    CREATE INDEX [IX_ContentTemplates_TenantId]
        ON [Master].[ContentTemplates]([TenantId]);
    CREATE INDEX [IX_ContentTemplates_TemplateType]
        ON [Master].[ContentTemplates]([TemplateType]);
    CREATE INDEX [IX_ContentTemplates_Category]
        ON [Master].[ContentTemplates]([Category]);
    CREATE INDEX [IX_ContentTemplates_IsActive_IsDeleted]
        ON [Master].[ContentTemplates]([IsActive], [IsDeleted]);

    PRINT N'[010.2] Master.ContentTemplates table created';
END
ELSE
    PRINT N'[010.2] Master.ContentTemplates table already exists';
GO

-- ============================================
-- Master.ContentTemplateSections — Reusable sections (Header, Footer, Body)
-- ============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'Master' AND TABLE_NAME = 'ContentTemplateSections')
BEGIN
    CREATE TABLE [Master].[ContentTemplateSections] (
        [Id]          NVARCHAR(256)  NOT NULL,
        [Name]        NVARCHAR(256)  NOT NULL,
        [SectionType] NVARCHAR(50)   NOT NULL DEFAULT 'Header',
        [HtmlContent] NVARCHAR(MAX)  NOT NULL DEFAULT '',
        [IsDefault]   BIT            NOT NULL DEFAULT 0,
        [IsActive]    BIT            NOT NULL DEFAULT 1,
        [TenantId]    NVARCHAR(128)  NULL,
        [CreatedAt]   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy]   NVARCHAR(256)  NULL,
        [UpdatedAt]   DATETIME2      NULL,
        [UpdatedBy]   NVARCHAR(256)  NULL,
        [IsDeleted]   BIT            NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ContentTemplateSections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ContentTemplateSections_Tenants]
            FOREIGN KEY ([TenantId]) REFERENCES [Master].[Tenants]([TenantId])
    );

    CREATE INDEX [IX_ContentTemplateSections_TenantId]
        ON [Master].[ContentTemplateSections]([TenantId]);
    CREATE INDEX [IX_ContentTemplateSections_SectionType]
        ON [Master].[ContentTemplateSections]([SectionType]);

    PRINT N'[010.3] Master.ContentTemplateSections table created';
END
ELSE
    PRINT N'[010.3] Master.ContentTemplateSections table already exists';
GO

-- ============================================
-- Master.TemplatePlaceholders — Placeholder definitions per template
-- ============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'Master' AND TABLE_NAME = 'TemplatePlaceholders')
BEGIN
    CREATE TABLE [Master].[TemplatePlaceholders] (
        [PlaceholderId]  INT           NOT NULL IDENTITY(1,1),
        [TemplateId]     NVARCHAR(256) NOT NULL,
        [PlaceholderKey] NVARCHAR(256) NOT NULL,
        [DisplayName]    NVARCHAR(256) NOT NULL,
        [Description]    NVARCHAR(500) NULL,
        [DefaultValue]   NVARCHAR(500) NULL,
        [SampleValue]    NVARCHAR(500) NULL,
        [PlaceholderType] NVARCHAR(50) NOT NULL DEFAULT 'Text',
        [IsRequired]     BIT           NOT NULL DEFAULT 0,
        [DisplayOrder]   INT           NOT NULL DEFAULT 0,
        [TenantId]       NVARCHAR(128) NULL,
        CONSTRAINT [PK_TemplatePlaceholders] PRIMARY KEY ([PlaceholderId]),
        CONSTRAINT [UQ_TemplatePlaceholders_TemplateId_Key]
            UNIQUE ([TemplateId], [PlaceholderKey]),
        CONSTRAINT [FK_TemplatePlaceholders_Templates]
            FOREIGN KEY ([TemplateId]) REFERENCES [Master].[ContentTemplates]([Id])
                ON DELETE CASCADE,
        CONSTRAINT [FK_TemplatePlaceholders_Tenants]
            FOREIGN KEY ([TenantId]) REFERENCES [Master].[Tenants]([TenantId])
    );

    CREATE INDEX [IX_TemplatePlaceholders_TemplateId]
        ON [Master].[TemplatePlaceholders]([TemplateId]);

    PRINT N'[010.4] Master.TemplatePlaceholders table created';
END
ELSE
    PRINT N'[010.4] Master.TemplatePlaceholders table already exists';
GO

-- ============================================
-- STORED PROCEDURES — Master.sp_* for Templates
-- ============================================

-- Master.sp_UpsertContentTemplate
IF OBJECT_ID('Master.sp_UpsertContentTemplate', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_UpsertContentTemplate;
GO
CREATE PROCEDURE Master.sp_UpsertContentTemplate
    @Id               NVARCHAR(256),
    @Name             NVARCHAR(256),
    @Description      NVARCHAR(500)  = NULL,
    @TemplateType     NVARCHAR(50)   = 'Email',
    @Subject          NVARCHAR(500)  = '',
    @HeaderId         NVARCHAR(256)  = NULL,
    @FooterId         NVARCHAR(256)  = NULL,
    @BodyContent      NVARCHAR(MAX)  = '',
    @PlainTextContent NVARCHAR(MAX)  = NULL,
    @Tags             NVARCHAR(MAX)  = NULL,
    @Category         NVARCHAR(100)  = NULL,
    @IsActive         BIT            = 1,
    @IsSystem         BIT            = 0,
    @TenantId         NVARCHAR(128)  = NULL,
    @Version          INT            = 1,
    @CreatedBy        NVARCHAR(256)  = NULL,
    @UpdatedBy        NVARCHAR(256)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Master.ContentTemplates WHERE Id = @Id AND IsDeleted = 0)
    BEGIN
        UPDATE Master.ContentTemplates
        SET    Name = @Name, Description = @Description, TemplateType = @TemplateType,
               Subject = @Subject, HeaderId = @HeaderId, FooterId = @FooterId,
               BodyContent = @BodyContent, PlainTextContent = @PlainTextContent,
               Tags = @Tags, Category = @Category, IsActive = @IsActive,
               Version = @Version, UpdatedAt = GETUTCDATE(), UpdatedBy = @UpdatedBy
        WHERE  Id = @Id AND IsDeleted = 0;
    END
    ELSE
    BEGIN
        INSERT INTO Master.ContentTemplates
            (Id, Name, Description, TemplateType, Subject, HeaderId, FooterId,
             BodyContent, PlainTextContent, Tags, Category, IsActive, IsSystem,
             TenantId, Version, CreatedAt, CreatedBy, IsDeleted)
        VALUES
            (@Id, @Name, @Description, @TemplateType, @Subject, @HeaderId, @FooterId,
             @BodyContent, @PlainTextContent, @Tags, @Category, @IsActive, @IsSystem,
             @TenantId, @Version, GETUTCDATE(), @CreatedBy, 0);
    END
END
GO

-- Master.sp_GetContentTemplateById
IF OBJECT_ID('Master.sp_GetContentTemplateById', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_GetContentTemplateById;
GO
CREATE PROCEDURE Master.sp_GetContentTemplateById
    @Id NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Master.ContentTemplates
    WHERE Id = @Id AND IsDeleted = 0;
END
GO

-- Master.sp_GetContentTemplatesByTenant
IF OBJECT_ID('Master.sp_GetContentTemplatesByTenant', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_GetContentTemplatesByTenant;
GO
CREATE PROCEDURE Master.sp_GetContentTemplatesByTenant
    @TenantId     NVARCHAR(128),
    @TemplateType NVARCHAR(50) = NULL,
    @Category     NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Master.ContentTemplates
    WHERE (TenantId = @TenantId OR TenantId IS NULL)
      AND IsDeleted = 0
      AND (@TemplateType IS NULL OR TemplateType = @TemplateType)
      AND (@Category     IS NULL OR Category     = @Category)
    ORDER BY Name;
END
GO

-- Master.sp_DeleteContentTemplate
IF OBJECT_ID('Master.sp_DeleteContentTemplate', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_DeleteContentTemplate;
GO
CREATE PROCEDURE Master.sp_DeleteContentTemplate
    @Id NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Master.ContentTemplates
    SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

-- Master.sp_UpsertContentTemplateSection
IF OBJECT_ID('Master.sp_UpsertContentTemplateSection', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_UpsertContentTemplateSection;
GO
CREATE PROCEDURE Master.sp_UpsertContentTemplateSection
    @Id          NVARCHAR(256),
    @Name        NVARCHAR(256),
    @SectionType NVARCHAR(50)  = 'Header',
    @HtmlContent NVARCHAR(MAX) = '',
    @IsDefault   BIT           = 0,
    @IsActive    BIT           = 1,
    @TenantId    NVARCHAR(128) = NULL,
    @CreatedBy   NVARCHAR(256) = NULL,
    @UpdatedBy   NVARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @IsDefault = 1
        UPDATE Master.ContentTemplateSections
        SET IsDefault = 0
        WHERE SectionType = @SectionType
          AND (TenantId = @TenantId OR (TenantId IS NULL AND @TenantId IS NULL))
          AND Id <> @Id
          AND IsDeleted = 0;

    IF EXISTS (SELECT 1 FROM Master.ContentTemplateSections WHERE Id = @Id AND IsDeleted = 0)
    BEGIN
        UPDATE Master.ContentTemplateSections
        SET Name = @Name, SectionType = @SectionType, HtmlContent = @HtmlContent,
            IsDefault = @IsDefault, IsActive = @IsActive,
            UpdatedAt = GETUTCDATE(), UpdatedBy = @UpdatedBy
        WHERE Id = @Id AND IsDeleted = 0;
    END
    ELSE
    BEGIN
        INSERT INTO Master.ContentTemplateSections
            (Id, Name, SectionType, HtmlContent, IsDefault, IsActive,
             TenantId, CreatedAt, CreatedBy, IsDeleted)
        VALUES
            (@Id, @Name, @SectionType, @HtmlContent, @IsDefault, @IsActive,
             @TenantId, GETUTCDATE(), @CreatedBy, 0);
    END
END
GO

-- Master.sp_GetContentTemplateSectionsByTenant
IF OBJECT_ID('Master.sp_GetContentTemplateSectionsByTenant', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_GetContentTemplateSectionsByTenant;
GO
CREATE PROCEDURE Master.sp_GetContentTemplateSectionsByTenant
    @TenantId    NVARCHAR(128),
    @SectionType NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Master.ContentTemplateSections
    WHERE (TenantId = @TenantId OR TenantId IS NULL)
      AND IsDeleted = 0
      AND (@SectionType IS NULL OR SectionType = @SectionType)
    ORDER BY SectionType, Name;
END
GO

-- Master.sp_GetContentTemplatePlaceholders
IF OBJECT_ID('Master.sp_GetContentTemplatePlaceholders', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_GetContentTemplatePlaceholders;
GO
CREATE PROCEDURE Master.sp_GetContentTemplatePlaceholders
    @TemplateId NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Master.TemplatePlaceholders
    WHERE TemplateId = @TemplateId
    ORDER BY DisplayOrder;
END
GO

-- Master.sp_ReplaceContentTemplatePlaceholders
IF OBJECT_ID('Master.sp_ReplaceContentTemplatePlaceholders', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_ReplaceContentTemplatePlaceholders;
GO
CREATE PROCEDURE Master.sp_ReplaceContentTemplatePlaceholders
    @TemplateId  NVARCHAR(256),
    @Placeholders NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Master.TemplatePlaceholders WHERE TemplateId = @TemplateId;

    IF @Placeholders IS NOT NULL AND LEN(@Placeholders) > 0
    BEGIN
        INSERT INTO Master.TemplatePlaceholders
            (TemplateId, PlaceholderKey, DisplayName, Description,
             DefaultValue, SampleValue, PlaceholderType, IsRequired, DisplayOrder)
        SELECT
            @TemplateId,
            j.[Key],
            j.DisplayName,
            j.Description,
            j.DefaultValue,
            j.SampleValue,
            j.PlaceholderType,
            j.IsRequired,
            j.[Order]
        FROM OPENJSON(@Placeholders)
        WITH (
            [Key]            NVARCHAR(256) '$.Key',
            DisplayName      NVARCHAR(256) '$.DisplayName',
            Description      NVARCHAR(500) '$.Description',
            DefaultValue     NVARCHAR(500) '$.DefaultValue',
            SampleValue      NVARCHAR(500) '$.SampleValue',
            PlaceholderType  NVARCHAR(50)  '$.Type',
            IsRequired       BIT           '$.IsRequired',
            [Order]          INT           '$.Order'
        ) AS j;
    END
END
GO

-- ============================================
-- STORED PROCEDURES — Shared.sp_* for EmailQueue
-- ============================================

-- Shared.sp_EnqueueEmail
IF OBJECT_ID('Shared.sp_EnqueueEmail', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_EnqueueEmail;
GO
CREATE PROCEDURE Shared.sp_EnqueueEmail
    @ToEmail   NVARCHAR(256),
    @CcEmail   NVARCHAR(500) = NULL,
    @BccEmail  NVARCHAR(500) = NULL,
    @Subject   NVARCHAR(256),
    @Body      NVARCHAR(MAX),
    @IsHtml    BIT           = 1,
    @TenantId  NVARCHAR(128) = NULL,
    @CreatedBy NVARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Shared.EmailQueue
        (ToEmail, CcEmail, BccEmail, Subject, Body, IsHtml,
         Status, SendAttempts, TenantId, CreatedAt, CreatedBy, IsDeleted)
    VALUES
        (@ToEmail, @CcEmail, @BccEmail, @Subject, @Body, @IsHtml,
         'Pending', 0, @TenantId, GETUTCDATE(), @CreatedBy, 0);

    SELECT SCOPE_IDENTITY() AS EmailQueueId;
END
GO

-- Shared.sp_GetPendingEmails
IF OBJECT_ID('Shared.sp_GetPendingEmails', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_GetPendingEmails;
GO
CREATE PROCEDURE Shared.sp_GetPendingEmails
    @BatchSize    INT          = 50,
    @MaxAttempts  INT          = 3
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@BatchSize) *
    FROM Shared.EmailQueue
    WHERE Status = 'Pending'
      AND SendAttempts < @MaxAttempts
      AND IsDeleted = 0
    ORDER BY CreatedAt ASC;
END
GO

-- Shared.sp_MarkEmailSent
IF OBJECT_ID('Shared.sp_MarkEmailSent', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_MarkEmailSent;
GO
CREATE PROCEDURE Shared.sp_MarkEmailSent
    @EmailQueueId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Shared.EmailQueue
    SET Status = 'Sent', SentAt = GETUTCDATE(),
        SendAttempts = SendAttempts + 1,
        UpdatedAt = GETUTCDATE()
    WHERE EmailQueueId = @EmailQueueId;
END
GO

-- Shared.sp_MarkEmailFailed
IF OBJECT_ID('Shared.sp_MarkEmailFailed', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_MarkEmailFailed;
GO
CREATE PROCEDURE Shared.sp_MarkEmailFailed
    @EmailQueueId   INT,
    @FailureReason  NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Shared.EmailQueue
    SET Status = CASE WHEN SendAttempts + 1 >= 3 THEN 'Failed' ELSE 'Pending' END,
        SendAttempts  = SendAttempts + 1,
        LastAttemptAt = GETUTCDATE(),
        FailureReason = @FailureReason,
        UpdatedAt     = GETUTCDATE()
    WHERE EmailQueueId = @EmailQueueId;
END
GO

PRINT N'[010] Cache, Template, and EmailQueue tables + SPs successfully created';
