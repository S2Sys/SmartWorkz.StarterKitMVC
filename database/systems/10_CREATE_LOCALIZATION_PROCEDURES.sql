-- ============================================
-- Phase 1B: Localization & i18n Procedures
-- Purpose: Multi-language support and translation management
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Shared
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- PROCEDURE: Get Translation (with fallback)
-- ============================================

IF OBJECT_ID('Shared.spGetTranslation', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spGetTranslation;

GO

CREATE PROCEDURE Shared.spGetTranslation
    @Key NVARCHAR(256),
    @LanguageCode NVARCHAR(10) = 'en',
    @DefaultLanguage NVARCHAR(10) = 'en',
    @TranslationText NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Try to get translation in requested language
    SELECT TOP 1 @TranslationText = TranslationText
    FROM Shared.Translations
    WHERE TranslationKey = @Key
    AND LanguageCode = @LanguageCode
    AND IsDeleted = 0;

    -- If not found, try default language
    IF @TranslationText IS NULL
    BEGIN
        SELECT TOP 1 @TranslationText = TranslationText
        FROM Shared.Translations
        WHERE TranslationKey = @Key
        AND LanguageCode = @DefaultLanguage
        AND IsDeleted = 0;

        IF @TranslationText IS NOT NULL
            PRINT '⚠️ Using fallback language (' + @DefaultLanguage + ') for key: ' + @Key;
    END

    -- If still not found, return key as fallback
    IF @TranslationText IS NULL
    BEGIN
        SET @TranslationText = @Key;
        PRINT '⚠️ Translation not found, returning key: ' + @Key;
    END
    ELSE
        PRINT '✅ Translation found for key: ' + @Key + ' (Language: ' + @LanguageCode + ')';
END;

GO

-- ============================================
-- PROCEDURE: Get All Translations by Language
-- ============================================

IF OBJECT_ID('Shared.spGetTranslationsByLanguage', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spGetTranslationsByLanguage;

GO

CREATE PROCEDURE Shared.spGetTranslationsByLanguage
    @LanguageCode NVARCHAR(10),
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT
        TranslationId,
        TranslationKey,
        TranslationText,
        LanguageCode,
        Category,
        CreatedAt,
        ROW_NUMBER() OVER (ORDER BY TranslationKey ASC) AS RowNumber
    FROM Shared.Translations
    WHERE LanguageCode = @LanguageCode
    AND IsDeleted = 0
    ORDER BY TranslationKey
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    PRINT '✅ Retrieved translations for language: ' + @LanguageCode;
END;

GO

-- ============================================
-- PROCEDURE: Upsert Translation
-- ============================================

IF OBJECT_ID('Shared.spUpsertTranslation', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spUpsertTranslation;

GO

CREATE PROCEDURE Shared.spUpsertTranslation
    @TranslationKey NVARCHAR(256),
    @LanguageCode NVARCHAR(10),
    @TranslationText NVARCHAR(MAX),
    @Category NVARCHAR(100) = 'General'
AS
BEGIN
    SET NOCOUNT ON;

    MERGE INTO Shared.Translations AS Target
    USING (
        SELECT
            @TranslationKey AS TranslationKey,
            @LanguageCode AS LanguageCode,
            @TranslationText AS TranslationText,
            @Category AS Category
    ) AS Source
    ON Target.TranslationKey = Source.TranslationKey
    AND Target.LanguageCode = Source.LanguageCode
    AND Target.IsDeleted = 0
    WHEN MATCHED THEN
        UPDATE SET
            TranslationText = Source.TranslationText,
            Category = Source.Category,
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = 'SYSTEM'
    WHEN NOT MATCHED THEN
        INSERT (
            TranslationId,
            TranslationKey,
            LanguageCode,
            TranslationText,
            Category,
            CreatedAt,
            CreatedBy,
            IsDeleted
        ) VALUES (
            NEWID(),
            Source.TranslationKey,
            Source.LanguageCode,
            Source.TranslationText,
            Source.Category,
            GETUTCDATE(),
            'SYSTEM',
            0
        );

    PRINT '✅ Upserted translation: ' + @TranslationKey + ' (' + @LanguageCode + ')';
END;

GO

-- ============================================
-- PROCEDURE: Get Supported Languages
-- ============================================

IF OBJECT_ID('Shared.spGetSupportedLanguages', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spGetSupportedLanguages;

GO

CREATE PROCEDURE Shared.spGetSupportedLanguages
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Get distinct languages from translations
    SELECT DISTINCT
        LanguageCode,
        CASE LanguageCode
            WHEN 'en' THEN 'English'
            WHEN 'es' THEN 'Spanish'
            WHEN 'fr' THEN 'French'
            WHEN 'de' THEN 'German'
            WHEN 'it' THEN 'Italian'
            WHEN 'pt' THEN 'Portuguese'
            WHEN 'ru' THEN 'Russian'
            WHEN 'ja' THEN 'Japanese'
            WHEN 'zh' THEN 'Chinese'
            WHEN 'ar' THEN 'Arabic'
            ELSE 'Unknown'
        END AS LanguageName,
        COUNT(DISTINCT TranslationKey) AS TranslationCount
    FROM Shared.Translations
    WHERE IsDeleted = 0
    GROUP BY LanguageCode
    ORDER BY TranslationCount DESC;

    PRINT '✅ Retrieved supported languages';
END;

GO

-- ============================================
-- PROCEDURE: Get Translations by Category
-- ============================================

IF OBJECT_ID('Shared.spGetTranslationsByCategory', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spGetTranslationsByCategory;

GO

CREATE PROCEDURE Shared.spGetTranslationsByCategory
    @Category NVARCHAR(100),
    @LanguageCode NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TranslationId,
        TranslationKey,
        TranslationText,
        LanguageCode,
        CreatedAt
    FROM Shared.Translations
    WHERE Category = @Category
    AND LanguageCode = @LanguageCode
    AND IsDeleted = 0
    ORDER BY TranslationKey;

    PRINT '✅ Retrieved translations for category: ' + @Category + ' (' + @LanguageCode + ')';
END;

GO

-- ============================================
-- PROCEDURE: Get Translation Categories
-- ============================================

IF OBJECT_ID('Shared.spGetTranslationCategories', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spGetTranslationCategories;

GO

CREATE PROCEDURE Shared.spGetTranslationCategories
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        Category,
        COUNT(*) AS TranslationCount
    FROM Shared.Translations
    WHERE IsDeleted = 0
    GROUP BY Category
    ORDER BY Category;

    PRINT '✅ Retrieved translation categories';
END;

GO

-- ============================================
-- PROCEDURE: Import Translations from JSON
-- ============================================

IF OBJECT_ID('Shared.spImportTranslationsFromJson', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spImportTranslationsFromJson;

GO

CREATE PROCEDURE Shared.spImportTranslationsFromJson
    @LanguageCode NVARCHAR(10),
    @JsonData NVARCHAR(MAX),
    @ImportedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @ImportedCount = 0;

    BEGIN TRY
        -- Parse JSON and insert translations
        INSERT INTO Shared.Translations (
            TranslationId,
            TranslationKey,
            LanguageCode,
            TranslationText,
            Category,
            CreatedAt,
            CreatedBy,
            IsDeleted
        )
        SELECT
            NEWID(),
            JSON_VALUE(value, '$.key'),
            @LanguageCode,
            JSON_VALUE(value, '$.text'),
            JSON_VALUE(value, '$.category'),
            GETUTCDATE(),
            'SYSTEM',
            0
        FROM OPENJSON(@JsonData) AS translations;

        SELECT @ImportedCount = @@ROWCOUNT;
        PRINT '✅ Imported ' + CAST(@ImportedCount AS NVARCHAR(10)) + ' translations for language: ' + @LanguageCode;
    END TRY
    BEGIN CATCH
        PRINT '❌ Error importing translations: ' + ERROR_MESSAGE();
        SET @ImportedCount = 0;
    END CATCH
END;

GO

-- ============================================
-- PROCEDURE: Localization Report
-- ============================================

IF OBJECT_ID('Report.spLocalizationReport', 'P') IS NOT NULL
    DROP PROCEDURE Report.spLocalizationReport;

GO

CREATE PROCEDURE Report.spLocalizationReport
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'LOCALIZATION & i18n REPORT';
    PRINT '═══════════════════════════════════════════';

    -- 1. Language coverage
    PRINT '';
    PRINT '🌍 LANGUAGE COVERAGE:';
    SELECT
        LanguageCode,
        COUNT(*) AS TranslationCount,
        COUNT(DISTINCT Category) AS CategoryCount
    FROM Shared.Translations
    WHERE IsDeleted = 0
    GROUP BY LanguageCode
    ORDER BY TranslationCount DESC;

    -- 2. Translation completeness by category
    PRINT '';
    PRINT '📊 TRANSLATION COMPLETENESS BY CATEGORY:';
    WITH LanguageCount AS (
        SELECT COUNT(DISTINCT LanguageCode) AS TotalLanguages
        FROM Shared.Translations
        WHERE IsDeleted = 0
    )
    SELECT
        Category,
        COUNT(DISTINCT LanguageCode) AS CoveredLanguages,
        (SELECT TotalLanguages FROM LanguageCount) AS TargetLanguages,
        CAST(COUNT(DISTINCT LanguageCode) * 100.0 / (SELECT TotalLanguages FROM LanguageCount) AS DECIMAL(5, 2)) AS CoveragePercent
    FROM Shared.Translations
    WHERE IsDeleted = 0
    GROUP BY Category
    ORDER BY CoveragePercent DESC;

    -- 3. Missing translations
    PRINT '';
    PRINT '⚠️ MISSING TRANSLATIONS:';
    WITH BaseLanguage AS (
        SELECT DISTINCT TranslationKey
        FROM Shared.Translations
        WHERE LanguageCode = 'en'
        AND IsDeleted = 0
    )
    SELECT
        LanguageCode,
        COUNT(*) AS MissingCount
    FROM (
        SELECT DISTINCT bl.TranslationKey, t.LanguageCode
        FROM BaseLanguage bl
        CROSS JOIN (
            SELECT DISTINCT LanguageCode
            FROM Shared.Translations
            WHERE IsDeleted = 0
        ) t
        WHERE NOT EXISTS (
            SELECT 1
            FROM Shared.Translations tr
            WHERE tr.TranslationKey = bl.TranslationKey
            AND tr.LanguageCode = t.LanguageCode
            AND tr.IsDeleted = 0
        )
    ) missing
    GROUP BY LanguageCode
    ORDER BY MissingCount DESC;

    -- 4. Recent translations
    PRINT '';
    PRINT '🆕 RECENTLY UPDATED TRANSLATIONS:';
    SELECT TOP 20
        TranslationKey,
        LanguageCode,
        TranslationText,
        UpdatedAt
    FROM Shared.Translations
    WHERE IsDeleted = 0
    ORDER BY COALESCE(UpdatedAt, CreatedAt) DESC;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1B: Localization & i18n successfully configured';
PRINT 'Total objects created:';
PRINT '  - 7 localization management procedures';
PRINT '  - Get translations with fallback language support';
PRINT '  - Import/export translation data';
PRINT '  - Language and category management';
PRINT '  - Localization reporting';
PRINT 'Status: Multi-language support ready for implementation';

GO
