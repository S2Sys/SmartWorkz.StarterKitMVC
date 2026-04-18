-- ============================================
-- FileStorage Stored Procedures
-- Purpose: CRUD operations for Shared.FileStorage
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;
GO

-- ============================================
-- spUpsertFileRecord - Create or update file record
-- ============================================
CREATE OR ALTER PROCEDURE [Shared].[spUpsertFileRecord]
    @FileId UNIQUEIDENTIFIER = NULL OUTPUT,
    @TenantId NVARCHAR(128),
    @FileName NVARCHAR(255),
    @FileSize BIGINT,
    @MimeType NVARCHAR(100),
    @FilePath NVARCHAR(1000),
    @EntityType NVARCHAR(100) = NULL,
    @EntityId NVARCHAR(255) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @FileId IS NULL OR @FileId = '00000000-0000-0000-0000-000000000000'
    BEGIN
        -- INSERT new file record
        SET @FileId = NEWID();

        INSERT INTO [Shared].[FileStorage]
        (FileId, TenantId, FileName, FileSize, MimeType, FilePath, EntityType, EntityId, IsActive, CreatedAt, UpdatedAt)
        VALUES
        (@FileId, @TenantId, @FileName, @FileSize, @MimeType, @FilePath, @EntityType, @EntityId, @IsActive, GETUTCDATE(), GETUTCDATE());
    END
    ELSE
    BEGIN
        -- UPDATE existing file record
        UPDATE [Shared].[FileStorage]
        SET
            FileName = @FileName,
            FileSize = @FileSize,
            MimeType = @MimeType,
            FilePath = @FilePath,
            EntityType = @EntityType,
            EntityId = @EntityId,
            IsActive = @IsActive,
            UpdatedAt = GETUTCDATE()
        WHERE FileId = @FileId AND TenantId = @TenantId;
    END

    SELECT @FileId AS FileId;
END;
GO

-- ============================================
-- spGetFileRecord - Get single file record
-- ============================================
CREATE OR ALTER PROCEDURE [Shared].[spGetFileRecord]
    @FileId UNIQUEIDENTIFIER,
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FileId,
        TenantId,
        FileName,
        FileSize,
        MimeType,
        FilePath,
        EntityType,
        EntityId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Shared].[FileStorage]
    WHERE FileId = @FileId
        AND TenantId = @TenantId
        AND IsDeleted = 0;
END;
GO

-- ============================================
-- spGetFilesByEntity - Get all files for entity
-- ============================================
CREATE OR ALTER PROCEDURE [Shared].[spGetFilesByEntity]
    @TenantId NVARCHAR(128),
    @EntityType NVARCHAR(100),
    @EntityId NVARCHAR(255),
    @OnlyActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FileId,
        TenantId,
        FileName,
        FileSize,
        MimeType,
        FilePath,
        EntityType,
        EntityId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Shared].[FileStorage]
    WHERE TenantId = @TenantId
        AND EntityType = @EntityType
        AND EntityId = @EntityId
        AND (@OnlyActive = 0 OR IsActive = 1)
        AND IsDeleted = 0
    ORDER BY CreatedAt DESC;
END;
GO

-- ============================================
-- spGetFilesByTenant - Get all files for tenant
-- ============================================
CREATE OR ALTER PROCEDURE [Shared].[spGetFilesByTenant]
    @TenantId NVARCHAR(128),
    @OnlyActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FileId,
        TenantId,
        FileName,
        FileSize,
        MimeType,
        FilePath,
        EntityType,
        EntityId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Shared].[FileStorage]
    WHERE TenantId = @TenantId
        AND (@OnlyActive = 0 OR IsActive = 1)
        AND IsDeleted = 0
    ORDER BY CreatedAt DESC;
END;
GO

-- ============================================
-- spDeleteFileRecord - Soft delete file record
-- ============================================
CREATE OR ALTER PROCEDURE [Shared].[spDeleteFileRecord]
    @FileId UNIQUEIDENTIFIER,
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Shared].[FileStorage]
    SET
        IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE FileId = @FileId
        AND TenantId = @TenantId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- ============================================
-- spGetFileSizeStats - Get file size statistics
-- ============================================
CREATE OR ALTER PROCEDURE [Shared].[spGetFileSizeStats]
    @TenantId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TenantId,
        COUNT(*) AS TotalFiles,
        SUM(FileSize) AS TotalSize,
        AVG(FileSize) AS AverageFileSize,
        MAX(FileSize) AS LargestFileSize,
        MIN(FileSize) AS SmallestFileSize
    FROM [Shared].[FileStorage]
    WHERE TenantId = @TenantId
        AND IsDeleted = 0
    GROUP BY TenantId;
END;
GO

-- ============================================
-- spGetFilesByMimeType - Get files filtered by MIME type
-- ============================================
CREATE OR ALTER PROCEDURE [Shared].[spGetFilesByMimeType]
    @TenantId NVARCHAR(128),
    @MimeTypePattern NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FileId,
        TenantId,
        FileName,
        FileSize,
        MimeType,
        FilePath,
        EntityType,
        EntityId,
        IsActive,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    FROM [Shared].[FileStorage]
    WHERE TenantId = @TenantId
        AND MimeType LIKE @MimeTypePattern + '%'
        AND IsDeleted = 0
    ORDER BY CreatedAt DESC;
END;
GO

PRINT '✓ FileStorage stored procedures created successfully';
