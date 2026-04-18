-- ============================================
-- Phase 1B: Full-Text Search Implementation
-- Purpose: Full-text search capability for content discovery
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Master, Shared
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- FULL-TEXT CATALOG SETUP
-- ============================================

-- Create full-text catalog if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'SmartWorkzCatalog')
BEGIN
    CREATE FULLTEXT CATALOG SmartWorkzCatalog;
    PRINT '✅ Created fulltext catalog: SmartWorkzCatalog';
END
ELSE
    PRINT '⚠️ Fulltext catalog already exists: SmartWorkzCatalog';

GO

-- ============================================
-- FULLTEXT INDEX: BlogPosts
-- ============================================

IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Master.BlogPosts'))
BEGIN
    DROP FULLTEXT INDEX ON Master.BlogPosts;
    PRINT '⚠️ Dropped existing fulltext index on Master.BlogPosts';
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BlogPosts' AND schema_id = SCHEMA_ID('Master'))
BEGIN
    CREATE FULLTEXT INDEX ON Master.BlogPosts (
        Title LANGUAGE 'English',
        Description LANGUAGE 'English',
        Content LANGUAGE 'English'
    )
    KEY INDEX PK_BlogPosts
    ON SmartWorkzCatalog;
    PRINT '✅ Created fulltext index on Master.BlogPosts';
END

GO

-- ============================================
-- FULLTEXT INDEX: CustomPages
-- ============================================

IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Master.CustomPages'))
BEGIN
    DROP FULLTEXT INDEX ON Master.CustomPages;
    PRINT '⚠️ Dropped existing fulltext index on Master.CustomPages';
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CustomPages' AND schema_id = SCHEMA_ID('Master'))
BEGIN
    CREATE FULLTEXT INDEX ON Master.CustomPages (
        Title LANGUAGE 'English',
        Content LANGUAGE 'English'
    )
    KEY INDEX PK_CustomPages
    ON SmartWorkzCatalog;
    PRINT '✅ Created fulltext index on Master.CustomPages';
END

GO

-- ============================================
-- FULLTEXT INDEX: Categories
-- ============================================

IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Master.Categories'))
BEGIN
    DROP FULLTEXT INDEX ON Master.Categories;
    PRINT '⚠️ Dropped existing fulltext index on Master.Categories';
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Categories' AND schema_id = SCHEMA_ID('Master'))
BEGIN
    CREATE FULLTEXT INDEX ON Master.Categories (
        Name LANGUAGE 'English',
        Description LANGUAGE 'English'
    )
    KEY INDEX PK_Categories
    ON SmartWorkzCatalog;
    PRINT '✅ Created fulltext index on Master.Categories';
END

GO

-- ============================================
-- PROCEDURE: Search BlogPosts
-- ============================================

IF OBJECT_ID('Master.spSearchBlogPosts', 'P') IS NOT NULL
    DROP PROCEDURE Master.spSearchBlogPosts;

GO

CREATE PROCEDURE Master.spSearchBlogPosts
    @SearchTerm NVARCHAR(256),
    @TenantId NVARCHAR(128),
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    IF @SearchTerm IS NULL OR LEN(@SearchTerm) = 0
    BEGIN
        PRINT '❌ Search term cannot be empty';
        RETURN;
    END

    SELECT
        BlogPostId,
        TenantId,
        Title,
        Description,
        Content,
        Author,
        PublishedAt,
        ViewCount,
        IsPublished,
        CreatedAt,
        RANK() OVER (ORDER BY (SELECT 1)) AS SearchRank
    FROM Master.BlogPosts
    WHERE TenantId = @TenantId
    AND IsDeleted = 0
    AND (
        CONTAINS((Title, Description, Content), @SearchTerm) OR
        Title LIKE '%' + @SearchTerm + '%' OR
        Description LIKE '%' + @SearchTerm + '%'
    )
    ORDER BY
        CASE WHEN Title LIKE @SearchTerm THEN 0 ELSE 1 END,
        CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    PRINT '✅ BlogPost search completed for: ' + @SearchTerm;
END;

GO

-- ============================================
-- PROCEDURE: Search CustomPages
-- ============================================

IF OBJECT_ID('Master.spSearchCustomPages', 'P') IS NOT NULL
    DROP PROCEDURE Master.spSearchCustomPages;

GO

CREATE PROCEDURE Master.spSearchCustomPages
    @SearchTerm NVARCHAR(256),
    @TenantId NVARCHAR(128),
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    IF @SearchTerm IS NULL OR LEN(@SearchTerm) = 0
    BEGIN
        PRINT '❌ Search term cannot be empty';
        RETURN;
    END

    SELECT
        CustomPageId,
        TenantId,
        Title,
        Content,
        Slug,
        IsPublished,
        ViewCount,
        CreatedAt,
        RANK() OVER (ORDER BY (SELECT 1)) AS SearchRank
    FROM Master.CustomPages
    WHERE TenantId = @TenantId
    AND IsDeleted = 0
    AND (
        CONTAINS((Title, Content), @SearchTerm) OR
        Title LIKE '%' + @SearchTerm + '%'
    )
    ORDER BY
        CASE WHEN Title LIKE @SearchTerm THEN 0 ELSE 1 END,
        CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    PRINT '✅ CustomPage search completed for: ' + @SearchTerm;
END;

GO

-- ============================================
-- PROCEDURE: Search Categories
-- ============================================

IF OBJECT_ID('Master.spSearchCategories', 'P') IS NOT NULL
    DROP PROCEDURE Master.spSearchCategories;

GO

CREATE PROCEDURE Master.spSearchCategories
    @SearchTerm NVARCHAR(256),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    IF @SearchTerm IS NULL OR LEN(@SearchTerm) = 0
    BEGIN
        PRINT '❌ Search term cannot be empty';
        RETURN;
    END

    SELECT
        CategoryId,
        TenantId,
        Name,
        Slug,
        Description,
        DisplayOrder,
        IsActive,
        CreatedAt
    FROM Master.Categories
    WHERE TenantId = @TenantId
    AND IsDeleted = 0
    AND (
        CONTAINS((Name, Description), @SearchTerm) OR
        Name LIKE '%' + @SearchTerm + '%'
    )
    ORDER BY
        CASE WHEN Name LIKE @SearchTerm THEN 0 ELSE 1 END,
        DisplayOrder ASC;

    PRINT '✅ Category search completed for: ' + @SearchTerm;
END;

GO

-- ============================================
-- PROCEDURE: Full-Text Search Maintenance
-- ============================================

IF OBJECT_ID('dbo.spMaintainFullTextIndexes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.spMaintainFullTextIndexes;

GO

CREATE PROCEDURE dbo.spMaintainFullTextIndexes
    @RebuildAll BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'FULLTEXT INDEX MAINTENANCE';
    PRINT '═══════════════════════════════════════════';

    IF @RebuildAll = 1
    BEGIN
        PRINT '';
        PRINT 'Rebuilding all fulltext indexes (this may take a moment)...';

        IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Master.BlogPosts'))
        BEGIN
            ALTER FULLTEXT INDEX ON Master.BlogPosts REBUILD;
            PRINT '✅ Rebuilt fulltext index: Master.BlogPosts';
        END

        IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Master.CustomPages'))
        BEGIN
            ALTER FULLTEXT INDEX ON Master.CustomPages REBUILD;
            PRINT '✅ Rebuilt fulltext index: Master.CustomPages';
        END

        IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Master.Categories'))
        BEGIN
            ALTER FULLTEXT INDEX ON Master.Categories REBUILD;
            PRINT '✅ Rebuilt fulltext index: Master.Categories';
        END
    END
    ELSE
    BEGIN
        PRINT '';
        PRINT 'Performing incremental fulltext index population...';

        IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Master.BlogPosts'))
        BEGIN
            ALTER FULLTEXT INDEX ON Master.BlogPosts START INCREMENTAL POPULATION;
            PRINT '✅ Updated fulltext index: Master.BlogPosts';
        END

        IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Master.CustomPages'))
        BEGIN
            ALTER FULLTEXT INDEX ON Master.CustomPages START INCREMENTAL POPULATION;
            PRINT '✅ Updated fulltext index: Master.CustomPages';
        END

        IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Master.Categories'))
        BEGIN
            ALTER FULLTEXT INDEX ON Master.Categories START INCREMENTAL POPULATION;
            PRINT '✅ Updated fulltext index: Master.Categories';
        END
    END

    PRINT '';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1B: Full-Text Search successfully configured';
PRINT 'Total objects created:';
PRINT '  - 1 fulltext catalog (SmartWorkzCatalog)';
PRINT '  - 3 fulltext indexes (BlogPosts, CustomPages, Categories)';
PRINT '  - 3 search procedures (SearchBlogPosts, SearchCustomPages, SearchCategories)';
PRINT '  - 1 maintenance procedure (MaintainFullTextIndexes)';
PRINT 'Status: Full-text search ready for queries';

GO
