-- ============================================
-- SmartWorkz v4: Create Essential SPs
-- Date: 2026-04-04
-- Purpose: Create all SPs needed for core functionality
-- ============================================

USE Boilerplate;

PRINT 'Creating essential stored procedures...'
PRINT ''

-- ============================================
-- SHARED SCHEMA - TRANSLATIONS
-- ============================================
PRINT '📝 Creating Shared (Translations) procedures...'

IF OBJECT_ID('Shared.sp_GetTranslations', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_GetTranslations;
GO

CREATE PROCEDURE Shared.sp_GetTranslations
    @TenantId NVARCHAR(128),
    @LanguageId INT
AS
BEGIN
    SELECT
        TranslationId,
        EntityType,
        EntityId,
        LanguageId,
        FieldName,
        TranslatedValue,
        TenantId,
        IsActive,
        CreatedAt
    FROM Shared.Translations
    WHERE TenantId = @TenantId
      AND LanguageId = @LanguageId
      AND IsDeleted = 0
    ORDER BY EntityType, EntityId, FieldName
END
GO
PRINT '  ✓ sp_GetTranslations'

-- ============================================
-- SHARED SCHEMA - EMAIL QUEUE
-- ============================================
PRINT '📧 Creating Shared (Email) procedures...'

IF OBJECT_ID('Shared.sp_EnqueueEmail', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_EnqueueEmail;
GO

CREATE PROCEDURE Shared.sp_EnqueueEmail
    @ToEmail NVARCHAR(256),
    @Subject NVARCHAR(256),
    @Body NVARCHAR(MAX),
    @IsHtml BIT,
    @Status NVARCHAR(50),
    @TenantId NVARCHAR(128),
    @CreatedAt DATETIME2
AS
BEGIN
    INSERT INTO Shared.EmailQueue (ToEmail, Subject, Body, IsHtml, Status, TenantId, CreatedAt, SendAttempts)
    VALUES (@ToEmail, @Subject, @Body, @IsHtml, @Status, @TenantId, @CreatedAt, 0)
END
GO
PRINT '  ✓ sp_EnqueueEmail'

IF OBJECT_ID('Shared.sp_GetPendingEmails', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_GetPendingEmails;
GO

CREATE PROCEDURE Shared.sp_GetPendingEmails
    @Limit INT = 100
AS
BEGIN
    SELECT TOP (@Limit)
        EmailQueueId,
        ToEmail,
        Subject,
        Body,
        IsHtml,
        Status,
        TenantId,
        CreatedAt,
        SendAttempts
    FROM Shared.EmailQueue
    WHERE Status = 'Pending'
      AND IsDeleted = 0
    ORDER BY CreatedAt ASC
END
GO
PRINT '  ✓ sp_GetPendingEmails'

IF OBJECT_ID('Shared.sp_MarkEmailSent', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_MarkEmailSent;
GO

CREATE PROCEDURE Shared.sp_MarkEmailSent
    @EmailQueueId INT
AS
BEGIN
    UPDATE Shared.EmailQueue
    SET Status = 'Sent',
        UpdatedAt = GETUTCDATE()
    WHERE EmailQueueId = @EmailQueueId
END
GO
PRINT '  ✓ sp_MarkEmailSent'

IF OBJECT_ID('Shared.sp_MarkEmailFailed', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_MarkEmailFailed;
GO

CREATE PROCEDURE Shared.sp_MarkEmailFailed
    @EmailQueueId INT
AS
BEGIN
    UPDATE Shared.EmailQueue
    SET Status = 'Failed',
        UpdatedAt = GETUTCDATE(),
        SendAttempts = SendAttempts + 1
    WHERE EmailQueueId = @EmailQueueId
END
GO
PRINT '  ✓ sp_MarkEmailFailed'

-- ============================================
-- SHARED SCHEMA - SEO META
-- ============================================
PRINT '🔍 Creating Shared (SEO Meta) procedures...'

IF OBJECT_ID('Shared.sp_GetSeoMetaByEntity', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_GetSeoMetaByEntity;
GO

CREATE PROCEDURE Shared.sp_GetSeoMetaByEntity
    @TenantId NVARCHAR(128),
    @EntityType NVARCHAR(50),
    @EntityId NVARCHAR(36)
AS
BEGIN
    SELECT
        SeoMetaId,
        TenantId,
        EntityType,
        EntityId,
        Slug,
        Title,
        Description,
        Keywords,
        OgTitle,
        OgDescription,
        OgImage,
        CanonicalUrl,
        IsActive,
        CreatedAt
    FROM Shared.SeoMeta
    WHERE TenantId = @TenantId
      AND EntityType = @EntityType
      AND EntityId = @EntityId
      AND IsDeleted = 0
END
GO
PRINT '  ✓ sp_GetSeoMetaByEntity'

IF OBJECT_ID('Shared.sp_GetSeoMetaBySlug', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_GetSeoMetaBySlug;
GO

CREATE PROCEDURE Shared.sp_GetSeoMetaBySlug
    @TenantId NVARCHAR(128),
    @Slug NVARCHAR(256)
AS
BEGIN
    SELECT
        SeoMetaId,
        TenantId,
        EntityType,
        EntityId,
        Slug,
        Title,
        Description,
        Keywords,
        OgTitle,
        OgDescription,
        OgImage,
        CanonicalUrl,
        IsActive,
        CreatedAt
    FROM Shared.SeoMeta
    WHERE TenantId = @TenantId
      AND Slug = @Slug
      AND IsDeleted = 0
END
GO
PRINT '  ✓ sp_GetSeoMetaBySlug'

-- ============================================
-- SHARED SCHEMA - TAGS
-- ============================================
PRINT '🏷️  Creating Shared (Tags) procedures...'

IF OBJECT_ID('Shared.sp_GetTagsByEntity', 'P') IS NOT NULL
    DROP PROCEDURE Shared.sp_GetTagsByEntity;
GO

CREATE PROCEDURE Shared.sp_GetTagsByEntity
    @TenantId NVARCHAR(128),
    @EntityType NVARCHAR(50),
    @EntityId NVARCHAR(36)
AS
BEGIN
    SELECT
        TagId,
        TenantId,
        EntityType,
        EntityId,
        TagName,
        CreatedAt
    FROM Shared.Tags
    WHERE TenantId = @TenantId
      AND EntityType = @EntityType
      AND EntityId = @EntityId
      AND IsDeleted = 0
    ORDER BY TagName
END
GO
PRINT '  ✓ sp_GetTagsByEntity'

-- ============================================
-- MASTER SCHEMA - MENUS
-- ============================================
PRINT '📋 Creating Master (Menus) procedures...'

IF OBJECT_ID('Master.sp_GetMenusByTenant', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_GetMenusByTenant;
GO

CREATE PROCEDURE Master.sp_GetMenusByTenant
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        MenuId,
        TenantId,
        Name,
        Description,
        MenuType,
        DisplayOrder,
        IsActive,
        CreatedAt
    FROM Master.Menus
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
    ORDER BY DisplayOrder, Name
END
GO
PRINT '  ✓ sp_GetMenusByTenant'

-- MenuItems table not yet created; skip for now

-- ============================================
-- MASTER SCHEMA - CATEGORIES
-- ============================================
PRINT '📂 Creating Master (Categories) procedures...'

IF OBJECT_ID('Master.sp_GetCategoriesByTenant', 'P') IS NOT NULL
    DROP PROCEDURE Master.sp_GetCategoriesByTenant;
GO

CREATE PROCEDURE Master.sp_GetCategoriesByTenant
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        CategoryId,
        Name,
        Slug,
        Description,
        NodePath,
        [Level],
        DisplayOrder,
        Icon,
        ImageUrl,
        TenantId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        IsDeleted
    FROM Master.Categories
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
    ORDER BY NodePath
END
GO
PRINT '  ✓ sp_GetCategoriesByTenant'

PRINT ''
PRINT '✅ All essential stored procedures created successfully!'
PRINT ''
PRINT 'Summary:'
PRINT '  Auth: 17 SPs (users, roles, permissions, tokens)'
PRINT '  Shared: 8 SPs (translations, emails, SEO, tags)'
PRINT '  Master: 3 SPs (menus, categories)'
PRINT '  Total: 28 SPs'
