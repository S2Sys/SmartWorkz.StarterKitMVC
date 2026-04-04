-- ============================================
-- SmartWorkz v4: Create Missing Auth SPs
-- Date: 2026-04-04
-- Purpose: Create essential Auth SPs for login flow
-- ============================================

USE Boilerplate;

PRINT 'Creating missing Auth stored procedures...'
PRINT ''

-- Create RefreshToken SPs
IF OBJECT_ID('Auth.sp_CreateRefreshToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_CreateRefreshToken;
GO

CREATE PROCEDURE Auth.sp_CreateRefreshToken
    @UserId NVARCHAR(36),
    @Token NVARCHAR(MAX),
    @ExpiresAt DATETIME2,
    @TenantId NVARCHAR(128),
    @CreatedAt DATETIME2
AS
BEGIN
    INSERT INTO Auth.RefreshTokens (UserId, Token, ExpiresAt, TenantId, CreatedAt)
    VALUES (@UserId, @Token, @ExpiresAt, @TenantId, @CreatedAt)
END
GO
PRINT '  ✓ sp_CreateRefreshToken'

IF OBJECT_ID('Auth.sp_GetRefreshToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_GetRefreshToken;
GO

CREATE PROCEDURE Auth.sp_GetRefreshToken
    @Token NVARCHAR(MAX),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        RefreshTokenId,
        UserId,
        Token,
        ExpiresAt,
        RevokedAt,
        TenantId,
        CreatedAt
    FROM Auth.RefreshTokens
    WHERE Token = @Token
      AND TenantId = @TenantId
      AND ExpiresAt > GETUTCDATE()
      AND RevokedAt IS NULL
      AND IsDeleted = 0
END
GO
PRINT '  ✓ sp_GetRefreshToken'

IF OBJECT_ID('Auth.sp_RevokeRefreshToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_RevokeRefreshToken;
GO

CREATE PROCEDURE Auth.sp_RevokeRefreshToken
    @UserId NVARCHAR(36),
    @Token NVARCHAR(MAX)
AS
BEGIN
    UPDATE Auth.RefreshTokens
    SET RevokedAt = GETUTCDATE()
    WHERE UserId = @UserId
      AND Token = @Token
      AND RevokedAt IS NULL
END
GO
PRINT '  ✓ sp_RevokeRefreshToken'

-- Create User SPs needed by app
IF OBJECT_ID('Auth.sp_UpsertUser', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_UpsertUser;
GO

CREATE PROCEDURE Auth.sp_UpsertUser
    @UserId NVARCHAR(36),
    @UserName NVARCHAR(256),
    @Email NVARCHAR(256),
    @PasswordHash NVARCHAR(MAX),
    @DisplayName NVARCHAR(256),
    @PhoneNumber NVARCHAR(20),
    @AvatarUrl NVARCHAR(MAX),
    @Locale NVARCHAR(10),
    @TenantId NVARCHAR(128),
    @IsActive BIT,
    @EmailConfirmed BIT,
    @TwoFactorEnabled BIT,
    @LockoutEnabled BIT,
    @AccessFailedCount INT,
    @UpdatedAt DATETIME2,
    @UpdatedBy NVARCHAR(36),
    @Roles NVARCHAR(MAX) = NULL,
    @Permissions NVARCHAR(MAX) = NULL
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Auth.Users WHERE UserId = @UserId AND IsDeleted = 0)
    BEGIN
        UPDATE Auth.Users
        SET
            Email = @Email,
            NormalizedEmail = UPPER(@Email),
            Username = @UserName,
            NormalizedUsername = UPPER(@UserName),
            PasswordHash = @PasswordHash,
            DisplayName = @DisplayName,
            PhoneNumber = @PhoneNumber,
            AvatarUrl = @AvatarUrl,
            Locale = @Locale,
            IsActive = @IsActive,
            EmailConfirmed = @EmailConfirmed,
            TwoFactorEnabled = @TwoFactorEnabled,
            LockoutEnabled = @LockoutEnabled,
            AccessFailedCount = @AccessFailedCount,
            UpdatedAt = @UpdatedAt,
            UpdatedBy = @UpdatedBy
        WHERE UserId = @UserId
    END
    ELSE
    BEGIN
        INSERT INTO Auth.Users
        (UserId, Email, NormalizedEmail, Username, NormalizedUsername, DisplayName, PasswordHash, PhoneNumber, AvatarUrl, Locale, TenantId, IsActive, EmailConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, SecurityStamp, ConcurrencyStamp, CreatedAt)
        VALUES
        (@UserId, @Email, UPPER(@Email), @UserName, UPPER(@UserName), @DisplayName, @PasswordHash, @PhoneNumber, @AvatarUrl, @Locale, @TenantId, @IsActive, @EmailConfirmed, @TwoFactorEnabled, @LockoutEnabled, @AccessFailedCount, NEWID(), NEWID(), GETUTCDATE())
    END
END
GO
PRINT '  ✓ sp_UpsertUser'

IF OBJECT_ID('Auth.sp_UserExists', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_UserExists;
GO

CREATE PROCEDURE Auth.sp_UserExists
    @Email NVARCHAR(256),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT COUNT(*) FROM Auth.Users
    WHERE Email = @Email
      AND TenantId = @TenantId
      AND IsDeleted = 0
END
GO
PRINT '  ✓ sp_UserExists'

IF OBJECT_ID('Auth.sp_GetUserById', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_GetUserById;
GO

CREATE PROCEDURE Auth.sp_GetUserById
    @UserId NVARCHAR(36)
AS
BEGIN
    SELECT
        UserId, Email, NormalizedEmail, Username, NormalizedUsername, DisplayName,
        PasswordHash, SecurityStamp, ConcurrencyStamp, TenantId, EmailConfirmed,
        TwoFactorEnabled, LockoutEnabled, LockoutEnd, AccessFailedCount, IsActive,
        IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, PhoneNumber
    FROM Auth.Users
    WHERE UserId = @UserId
      AND IsDeleted = 0
END
GO
PRINT '  ✓ sp_GetUserById'

-- Password Reset Token SPs
IF OBJECT_ID('Auth.sp_CreatePasswordResetToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_CreatePasswordResetToken;
GO

CREATE PROCEDURE Auth.sp_CreatePasswordResetToken
    @UserId NVARCHAR(36),
    @Token NVARCHAR(MAX),
    @ExpiresAt DATETIME2,
    @TenantId NVARCHAR(128),
    @CreatedAt DATETIME2
AS
BEGIN
    INSERT INTO Auth.PasswordResetTokens (UserId, Token, ExpiresAt, TenantId, CreatedAt)
    VALUES (@UserId, @Token, @ExpiresAt, @TenantId, @CreatedAt)
END
GO
PRINT '  ✓ sp_CreatePasswordResetToken'

IF OBJECT_ID('Auth.sp_GetPasswordResetToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_GetPasswordResetToken;
GO

CREATE PROCEDURE Auth.sp_GetPasswordResetToken
    @UserId NVARCHAR(36),
    @Token NVARCHAR(MAX),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        PasswordResetTokenId, UserId, Token, ExpiresAt, UsedAt, TenantId, CreatedAt
    FROM Auth.PasswordResetTokens
    WHERE UserId = @UserId
      AND Token = @Token
      AND TenantId = @TenantId
      AND ExpiresAt > GETUTCDATE()
      AND UsedAt IS NULL
      AND IsDeleted = 0
END
GO
PRINT '  ✓ sp_GetPasswordResetToken'

IF OBJECT_ID('Auth.sp_UpdatePasswordResetToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_UpdatePasswordResetToken;
GO

CREATE PROCEDURE Auth.sp_UpdatePasswordResetToken
    @PasswordResetTokenId INT,
    @UsedAt DATETIME2
AS
BEGIN
    UPDATE Auth.PasswordResetTokens
    SET UsedAt = @UsedAt
    WHERE PasswordResetTokenId = @PasswordResetTokenId
END
GO
PRINT '  ✓ sp_UpdatePasswordResetToken'

IF OBJECT_ID('Auth.sp_InvalidatePreviousPasswordResetTokens', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_InvalidatePreviousPasswordResetTokens;
GO

CREATE PROCEDURE Auth.sp_InvalidatePreviousPasswordResetTokens
    @UserId NVARCHAR(36)
AS
BEGIN
    UPDATE Auth.PasswordResetTokens
    SET IsDeleted = 1
    WHERE UserId = @UserId
      AND UsedAt IS NULL
      AND ExpiresAt > GETUTCDATE()
      AND IsDeleted = 0
END
GO
PRINT '  ✓ sp_InvalidatePreviousPasswordResetTokens'

-- Email Verification Token SPs
IF OBJECT_ID('Auth.sp_CreateEmailVerificationToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_CreateEmailVerificationToken;
GO

CREATE PROCEDURE Auth.sp_CreateEmailVerificationToken
    @UserId NVARCHAR(36),
    @Email NVARCHAR(256),
    @Token NVARCHAR(MAX),
    @ExpiresAt DATETIME2,
    @TenantId NVARCHAR(128),
    @CreatedAt DATETIME2
AS
BEGIN
    INSERT INTO Auth.EmailVerificationTokens (UserId, Email, Token, ExpiresAt, TenantId, CreatedAt)
    VALUES (@UserId, @Email, @Token, @ExpiresAt, @TenantId, @CreatedAt)
END
GO
PRINT '  ✓ sp_CreateEmailVerificationToken'

IF OBJECT_ID('Auth.sp_GetEmailVerificationToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_GetEmailVerificationToken;
GO

CREATE PROCEDURE Auth.sp_GetEmailVerificationToken
    @UserId NVARCHAR(36),
    @Token NVARCHAR(MAX),
    @TenantId NVARCHAR(128)
AS
BEGIN
    SELECT
        EmailVerificationTokenId, UserId, Email, Token, ExpiresAt, VerifiedAt, TenantId, CreatedAt
    FROM Auth.EmailVerificationTokens
    WHERE UserId = @UserId
      AND Token = @Token
      AND TenantId = @TenantId
      AND ExpiresAt > GETUTCDATE()
      AND VerifiedAt IS NULL
      AND IsDeleted = 0
END
GO
PRINT '  ✓ sp_GetEmailVerificationToken'

IF OBJECT_ID('Auth.sp_UpdateEmailVerificationToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.sp_UpdateEmailVerificationToken;
GO

CREATE PROCEDURE Auth.sp_UpdateEmailVerificationToken
    @EmailVerificationTokenId INT,
    @VerifiedAt DATETIME2
AS
BEGIN
    UPDATE Auth.EmailVerificationTokens
    SET VerifiedAt = @VerifiedAt
    WHERE EmailVerificationTokenId = @EmailVerificationTokenId
END
GO
PRINT '  ✓ sp_UpdateEmailVerificationToken'

PRINT ''
PRINT '✅ All Auth stored procedures created successfully!'
