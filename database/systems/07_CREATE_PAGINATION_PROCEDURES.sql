-- ============================================
-- Phase 1B: Pagination Support Procedures
-- Purpose: Standardized pagination for list queries
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Shared
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- PROCEDURE: Get Paginated BlogPosts
-- ============================================

IF OBJECT_ID('Master.spGetBlogPostsPaginated', 'P') IS NOT NULL
    DROP PROCEDURE Master.spGetBlogPostsPaginated;

GO

CREATE PROCEDURE Master.spGetBlogPostsPaginated
    @TenantId NVARCHAR(128),
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortDirection NVARCHAR(4) = 'DESC',
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT;
    DECLARE @OrderBy NVARCHAR(MAX);

    -- Validate inputs
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;
    IF @PageSize > 100 SET @PageSize = 100;

    SET @Offset = (@PageNumber - 1) * @PageSize;

    -- Build order by clause with validation
    SET @OrderBy = CASE
        WHEN @SortBy = 'Title' THEN 'Title'
        WHEN @SortBy = 'PublishedAt' THEN 'PublishedAt'
        WHEN @SortBy = 'ViewCount' THEN 'ViewCount'
        ELSE 'CreatedAt'
    END;

    IF UPPER(@SortDirection) NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'DESC';

    -- Get total count
    SELECT @TotalRecords = COUNT(*)
    FROM Master.BlogPosts
    WHERE TenantId = @TenantId
    AND IsDeleted = 0;

    -- Return paginated results
    DECLARE @SQL NVARCHAR(MAX) =
        'SELECT
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
            UpdatedAt,
            ROW_NUMBER() OVER (ORDER BY ' + @OrderBy + ' ' + @SortDirection + ') AS RowNumber,
            @TotalRecords AS TotalRecords,
            CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS TotalPages,
            @PageNumber AS CurrentPage
        FROM Master.BlogPosts
        WHERE TenantId = @TenantId
        AND IsDeleted = 0
        ORDER BY ' + @OrderBy + ' ' + @SortDirection + '
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;';

    EXEC sp_executesql @SQL,
        N'@TenantId NVARCHAR(128), @Offset INT, @PageSize INT, @TotalRecords INT',
        @TenantId, @Offset, @PageSize, @TotalRecords;

    PRINT '✅ Retrieved page ' + CAST(@PageNumber AS NVARCHAR(5)) + ' of ' +
          CAST(CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS NVARCHAR(5));
END;

GO

-- ============================================
-- PROCEDURE: Get Paginated Users
-- ============================================

IF OBJECT_ID('Auth.spGetUsersPaginated', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spGetUsersPaginated;

GO

CREATE PROCEDURE Auth.spGetUsersPaginated
    @TenantId NVARCHAR(128),
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortDirection NVARCHAR(4) = 'DESC',
    @IsActiveFilter BIT = NULL,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT;
    DECLARE @OrderBy NVARCHAR(MAX);

    -- Validate inputs
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;
    IF @PageSize > 100 SET @PageSize = 100;

    SET @Offset = (@PageNumber - 1) * @PageSize;

    -- Build order by clause
    SET @OrderBy = CASE
        WHEN @SortBy = 'Email' THEN 'Email'
        WHEN @SortBy = 'LastLoginAt' THEN 'LastLoginAt'
        WHEN @SortBy = 'CreatedAt' THEN 'CreatedAt'
        ELSE 'CreatedAt'
    END;

    IF UPPER(@SortDirection) NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'DESC';

    -- Get total count with filter
    SELECT @TotalRecords = COUNT(*)
    FROM Auth.Users
    WHERE TenantId = @TenantId
    AND IsDeleted = 0
    AND (@IsActiveFilter IS NULL OR IsActive = @IsActiveFilter);

    -- Return paginated results
    SELECT
        UserId,
        TenantId,
        Email,
        FirstName,
        LastName,
        IsActive,
        IsLocked,
        FailedLoginAttempts,
        LastLoginAt,
        CreatedAt,
        ROW_NUMBER() OVER (ORDER BY
            CASE WHEN @OrderBy = 'Email' THEN Email
                 WHEN @OrderBy = 'LastLoginAt' THEN CAST(LastLoginAt AS NVARCHAR(19))
                 ELSE CAST(CreatedAt AS NVARCHAR(19))
            END) AS RowNumber,
        @TotalRecords AS TotalRecords,
        CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS TotalPages,
        @PageNumber AS CurrentPage
    FROM Auth.Users
    WHERE TenantId = @TenantId
    AND IsDeleted = 0
    AND (@IsActiveFilter IS NULL OR IsActive = @IsActiveFilter)
    ORDER BY
        CASE WHEN @OrderBy = 'Email' THEN Email
             WHEN @OrderBy = 'LastLoginAt' THEN CAST(LastLoginAt AS NVARCHAR(19))
             ELSE CAST(CreatedAt AS NVARCHAR(19))
        END COLLATE SQL_Latin1_General_CP1_CI_AS
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    PRINT '✅ Retrieved page ' + CAST(@PageNumber AS NVARCHAR(5)) + ' of ' +
          CAST(CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS NVARCHAR(5));
END;

GO

-- ============================================
-- PROCEDURE: Get Paginated Categories
-- ============================================

IF OBJECT_ID('Master.spGetCategoriesPaginated', 'P') IS NOT NULL
    DROP PROCEDURE Master.spGetCategoriesPaginated;

GO

CREATE PROCEDURE Master.spGetCategoriesPaginated
    @TenantId NVARCHAR(128),
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SortBy NVARCHAR(50) = 'DisplayOrder',
    @SortDirection NVARCHAR(4) = 'ASC',
    @ParentCategoryId INT = NULL,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT;

    -- Validate inputs
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;
    IF @PageSize > 100 SET @PageSize = 100;

    SET @Offset = (@PageNumber - 1) * @PageSize;

    IF UPPER(@SortDirection) NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'ASC';

    -- Get total count
    SELECT @TotalRecords = COUNT(*)
    FROM Master.Categories
    WHERE TenantId = @TenantId
    AND IsDeleted = 0
    AND (@ParentCategoryId IS NULL OR ParentCategoryId = @ParentCategoryId);

    -- Return paginated results
    SELECT
        CategoryId,
        TenantId,
        Name,
        Slug,
        Description,
        DisplayOrder,
        IsActive,
        ParentCategoryId,
        CreatedAt,
        ROW_NUMBER() OVER (ORDER BY
            CASE WHEN @SortBy = 'Name' THEN Name
                 WHEN @SortBy = 'CreatedAt' THEN CAST(CreatedAt AS NVARCHAR(19))
                 ELSE CAST(DisplayOrder AS NVARCHAR(10))
            END) AS RowNumber,
        @TotalRecords AS TotalRecords,
        CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS TotalPages,
        @PageNumber AS CurrentPage
    FROM Master.Categories
    WHERE TenantId = @TenantId
    AND IsDeleted = 0
    AND (@ParentCategoryId IS NULL OR ParentCategoryId = @ParentCategoryId)
    ORDER BY
        CASE WHEN @SortBy = 'Name' THEN Name
             WHEN @SortBy = 'CreatedAt' THEN CAST(CreatedAt AS NVARCHAR(19))
             ELSE CAST(DisplayOrder AS NVARCHAR(10))
        END
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    PRINT '✅ Retrieved page ' + CAST(@PageNumber AS NVARCHAR(5)) + ' of ' +
          CAST(CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS NVARCHAR(5));
END;

GO

-- ============================================
-- PROCEDURE: Get Paginated AuditLogs
-- ============================================

IF OBJECT_ID('Shared.spGetAuditLogsPaginated', 'P') IS NOT NULL
    DROP PROCEDURE Shared.spGetAuditLogsPaginated;

GO

CREATE PROCEDURE Shared.spGetAuditLogsPaginated
    @TenantId NVARCHAR(128),
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @FilterByEntity NVARCHAR(128) = NULL,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT;

    -- Validate inputs
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 50;
    IF @PageSize > 500 SET @PageSize = 500;

    SET @Offset = (@PageNumber - 1) * @PageSize;

    -- Get total count
    SELECT @TotalRecords = COUNT(*)
    FROM Shared.AuditLogs
    WHERE TenantId = @TenantId
    AND IsDeleted = 0
    AND (@FilterByEntity IS NULL OR EntityName = @FilterByEntity);

    -- Return paginated results (most recent first)
    SELECT
        AuditLogId,
        TenantId,
        UserId,
        EntityName,
        EntityId,
        Action,
        OldValues,
        NewValues,
        IpAddress,
        CreatedAt,
        ROW_NUMBER() OVER (ORDER BY CreatedAt DESC) AS RowNumber,
        @TotalRecords AS TotalRecords,
        CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS TotalPages,
        @PageNumber AS CurrentPage
    FROM Shared.AuditLogs
    WHERE TenantId = @TenantId
    AND IsDeleted = 0
    AND (@FilterByEntity IS NULL OR EntityName = @FilterByEntity)
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    PRINT '✅ Retrieved page ' + CAST(@PageNumber AS NVARCHAR(5)) + ' of ' +
          CAST(CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS NVARCHAR(5));
END;

GO

PRINT '✅ Phase 1B: Pagination Procedures successfully created';
PRINT 'Total objects created:';
PRINT '  - 4 paginated list procedures';
PRINT '  - Standardized OFFSET/FETCH pagination';
PRINT '  - Automatic row counting and page calculation';
PRINT '  - Configurable sorting and filtering';
PRINT 'Status: Pagination support ready for list views';

GO
