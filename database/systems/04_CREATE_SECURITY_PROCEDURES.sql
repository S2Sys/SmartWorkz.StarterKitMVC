-- ============================================
-- Phase 1A: Security Procedures
-- Purpose: Password policy, account lockout, session management
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Auth
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- PROCEDURE: Validate Password Policy
-- ============================================

IF OBJECT_ID('Auth.spValidatePasswordPolicy', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spValidatePasswordPolicy;

GO

CREATE PROCEDURE Auth.spValidatePasswordPolicy
    @Password NVARCHAR(256),
    @IsValid BIT OUTPUT,
    @ErrorMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @IsValid = 1;
    SET @ErrorMessage = '';

    -- Password requirements:
    -- - Minimum 8 characters
    -- - At least one uppercase letter
    -- - At least one lowercase letter
    -- - At least one digit
    -- - At least one special character

    IF LEN(@Password) < 8
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = 'Password must be at least 8 characters long. ';
    END

    IF @IsValid = 1 AND NOT (@Password LIKE '%[A-Z]%')
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = CONCAT(@ErrorMessage, 'Password must contain at least one uppercase letter. ');
    END

    IF @IsValid = 1 AND NOT (@Password LIKE '%[a-z]%')
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = CONCAT(@ErrorMessage, 'Password must contain at least one lowercase letter. ');
    END

    IF @IsValid = 1 AND NOT (@Password LIKE '%[0-9]%')
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = CONCAT(@ErrorMessage, 'Password must contain at least one digit. ');
    END

    IF @IsValid = 1 AND NOT (@Password LIKE '%[!@#$%^&*()_+-=\[\]{};:'\'",.<>?/\\|`~]%')
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = CONCAT(@ErrorMessage, 'Password must contain at least one special character. ');
    END

    PRINT CASE WHEN @IsValid = 1 THEN '✅ Password policy validation: PASSED' ELSE '❌ Password policy validation: FAILED' END;
    IF @ErrorMessage <> ''
        PRINT 'Details: ' + @ErrorMessage;
END;

GO

-- ============================================
-- PROCEDURE: Record Failed Login Attempt
-- ============================================

IF OBJECT_ID('Auth.spRecordLoginAttempt', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spRecordLoginAttempt;

GO

CREATE PROCEDURE Auth.spRecordLoginAttempt
    @UserId NVARCHAR(128),
    @Email NVARCHAR(256),
    @IsSuccessful BIT,
    @IpAddress NVARCHAR(45) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @LockAccountOnFailure BIT = 1,
    @MaxFailedAttempts INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FailedCount INT = 0;
    DECLARE @IsLocked BIT = 0;

    -- Record the login attempt
    INSERT INTO Auth.LoginAttempts (UserId, Email, AttemptedAt, IsSuccessful, IpAddress, UserAgent, TenantId, CreatedAt)
    SELECT
        @UserId,
        @Email,
        GETUTCDATE(),
        @IsSuccessful,
        @IpAddress,
        @UserAgent,
        TenantId,
        GETUTCDATE()
    FROM Auth.Users
    WHERE UserId = @UserId;

    -- If successful, reset failed attempts and update last login
    IF @IsSuccessful = 1
    BEGIN
        UPDATE Auth.Users
        SET FailedLoginAttempts = 0,
            LastLoginAt = GETUTCDATE(),
            IsLocked = 0,
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = 'SYSTEM'
        WHERE UserId = @UserId;

        PRINT '✅ Successful login recorded for: ' + @Email;
    END
    ELSE
    BEGIN
        -- Increment failed attempts
        UPDATE Auth.Users
        SET FailedLoginAttempts = FailedLoginAttempts + 1,
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = 'SYSTEM'
        WHERE UserId = @UserId;

        -- Check if account should be locked
        SELECT @FailedCount = FailedLoginAttempts
        FROM Auth.Users
        WHERE UserId = @UserId;

        IF @FailedCount >= @MaxFailedAttempts AND @LockAccountOnFailure = 1
        BEGIN
            UPDATE Auth.Users
            SET IsLocked = 1,
                UpdatedAt = GETUTCDATE(),
                UpdatedBy = 'SYSTEM'
            WHERE UserId = @UserId;

            PRINT '🔒 Account locked after ' + CAST(@FailedCount AS NVARCHAR(2)) + ' failed attempts: ' + @Email;
        END
        ELSE
        BEGIN
            PRINT '⚠️ Failed login attempt ' + CAST(@FailedCount AS NVARCHAR(2)) + '/' + CAST(@MaxFailedAttempts AS NVARCHAR(2)) + ' for: ' + @Email;
        END
    END
END;

GO

-- ============================================
-- PROCEDURE: Unlock User Account
-- ============================================

IF OBJECT_ID('Auth.spUnlockUserAccount', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spUnlockUserAccount;

GO

CREATE PROCEDURE Auth.spUnlockUserAccount
    @UserId NVARCHAR(128),
    @ResetFailedAttempts BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Auth.Users
    SET IsLocked = 0,
        FailedLoginAttempts = CASE WHEN @ResetFailedAttempts = 1 THEN 0 ELSE FailedLoginAttempts END,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = 'SYSTEM'
    WHERE UserId = @UserId;

    PRINT '✅ Account unlocked: ' + @UserId;
END;

GO

-- ============================================
-- PROCEDURE: Revoke User Sessions (All Tokens)
-- ============================================

IF OBJECT_ID('Auth.spRevokeUserSessions', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spRevokeUserSessions;

GO

CREATE PROCEDURE Auth.spRevokeUserSessions
    @UserId NVARCHAR(128),
    @ExceptTokenId NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Count INT = 0;

    UPDATE Auth.AuthTokens
    SET IsRevoked = 1,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = 'SYSTEM'
    WHERE UserId = @UserId
    AND IsRevoked = 0
    AND (@ExceptTokenId IS NULL OR TokenId <> @ExceptTokenId);

    SELECT @Count = COUNT(*)
    FROM Auth.AuthTokens
    WHERE UserId = @UserId
    AND IsRevoked = 1;

    PRINT '✅ Revoked all sessions for user: ' + @UserId + ' (Total sessions: ' + CAST(@Count AS NVARCHAR(10)) + ')';
END;

GO

-- ============================================
-- PROCEDURE: Create Secure AuthToken
-- ============================================

IF OBJECT_ID('Auth.spCreateAuthToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spCreateAuthToken;

GO

CREATE PROCEDURE Auth.spCreateAuthToken
    @UserId NVARCHAR(128),
    @TokenType NVARCHAR(50),
    @Token NVARCHAR(MAX),
    @ExpiresInHours INT = 24,
    @TenantId NVARCHAR(128) = NULL,
    @TokenId NVARCHAR(128) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ExpiresAt DATETIME2 = DATEADD(HOUR, @ExpiresInHours, GETUTCDATE());

    SET @TokenId = NEWID();

    INSERT INTO Auth.AuthTokens (
        TokenId,
        UserId,
        Token,
        TokenType,
        ExpiresAt,
        IsRevoked,
        TenantId,
        CreatedAt,
        CreatedBy
    ) VALUES (
        @TokenId,
        @UserId,
        @Token,
        @TokenType,
        @ExpiresAt,
        0,
        @TenantId,
        GETUTCDATE(),
        'SYSTEM'
    );

    PRINT '✅ Auth token created: ' + @TokenType + ' (Expires: ' + CONVERT(NVARCHAR(19), @ExpiresAt, 121) + ')';
END;

GO

-- ============================================
-- PROCEDURE: Validate Auth Token
-- ============================================

IF OBJECT_ID('Auth.spValidateAuthToken', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spValidateAuthToken;

GO

CREATE PROCEDURE Auth.spValidateAuthToken
    @Token NVARCHAR(MAX),
    @IsValid BIT OUTPUT,
    @UserId NVARCHAR(128) OUTPUT,
    @ErrorMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @IsValid = 0;
    SET @UserId = NULL;
    SET @ErrorMessage = '';

    -- Check if token exists and is valid
    SELECT TOP 1
        @UserId = UserId,
        @IsValid = CASE
            WHEN IsRevoked = 1 THEN 0
            WHEN ExpiresAt < GETUTCDATE() THEN 0
            ELSE 1
        END
    FROM Auth.AuthTokens
    WHERE Token = @Token
    ORDER BY CreatedAt DESC;

    IF @UserId IS NULL
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = 'Token not found';
    END
    ELSE IF @IsValid = 0
    BEGIN
        SELECT TOP 1
            @ErrorMessage = CASE
                WHEN IsRevoked = 1 THEN 'Token has been revoked'
                WHEN ExpiresAt < GETUTCDATE() THEN 'Token has expired'
                ELSE 'Token is invalid'
            END
        FROM Auth.AuthTokens
        WHERE Token = @Token
        ORDER BY CreatedAt DESC;
    END

    PRINT CASE WHEN @IsValid = 1 THEN '✅ Token validation: VALID' ELSE '❌ Token validation: INVALID (' + @ErrorMessage + ')' END;
END;

GO

-- ============================================
-- PROCEDURE: Security Audit Report
-- ============================================

IF OBJECT_ID('Auth.spSecurityAuditReport', 'P') IS NOT NULL
    DROP PROCEDURE Auth.spSecurityAuditReport;

GO

CREATE PROCEDURE Auth.spSecurityAuditReport
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '═══════════════════════════════════════════';
    PRINT 'SECURITY AUDIT REPORT';
    PRINT '═══════════════════════════════════════════';

    -- 1. Locked accounts
    PRINT '';
    PRINT '🔒 LOCKED ACCOUNTS:';
    SELECT
        UserId,
        Email,
        FirstName + ' ' + LastName AS FullName,
        FailedLoginAttempts,
        UpdatedAt AS LockedAt
    FROM Auth.Users
    WHERE IsLocked = 1
    AND IsDeleted = 0;

    -- 2. Users with many failed attempts (but not locked)
    PRINT '';
    PRINT '⚠️ SUSPICIOUS ACTIVITY (3+ failed attempts):';
    SELECT
        UserId,
        Email,
        FailedLoginAttempts,
        LastLoginAt,
        UpdatedAt
    FROM Auth.Users
    WHERE FailedLoginAttempts >= 3
    AND IsLocked = 0
    AND IsDeleted = 0;

    -- 3. Expired tokens still in system
    PRINT '';
    PRINT '⏰ EXPIRED TOKENS (not revoked):';
    SELECT
        COUNT(*) AS ExpiredTokenCount
    FROM Auth.AuthTokens
    WHERE ExpiresAt < GETUTCDATE()
    AND IsRevoked = 0;

    -- 4. Recently revoked tokens
    PRINT '';
    PRINT '🚫 RECENTLY REVOKED TOKENS (last 7 days):';
    SELECT
        TokenId,
        UserId,
        TokenType,
        UpdatedAt AS RevokedAt
    FROM Auth.AuthTokens
    WHERE IsRevoked = 1
    AND UpdatedAt >= DATEADD(DAY, -7, GETUTCDATE());

    -- 5. Recent login attempts
    PRINT '';
    PRINT '📊 RECENT FAILED LOGINS (last 24 hours):';
    SELECT
        COUNT(*) AS FailedLoginCount
    FROM Auth.LoginAttempts
    WHERE IsSuccessful = 0
    AND AttemptedAt >= DATEADD(DAY, -1, GETUTCDATE());

    -- 6. User permission summary
    PRINT '';
    PRINT '👥 ACTIVE USERS BY ROLE:';
    SELECT
        r.Name AS RoleName,
        COUNT(DISTINCT ur.UserId) AS UserCount
    FROM Auth.Roles r
    LEFT JOIN Auth.UserRoles ur ON r.RoleId = ur.RoleId
    LEFT JOIN Auth.Users u ON ur.UserId = u.UserId AND u.IsDeleted = 0 AND u.IsActive = 1
    GROUP BY r.RoleId, r.Name;

    PRINT '';
    PRINT '═══════════════════════════════════════════';
    PRINT 'AUDIT REPORT COMPLETE';
    PRINT '═══════════════════════════════════════════';
END;

GO

PRINT '✅ Phase 1A: Security Procedures successfully created';
PRINT 'Total procedures created:';
PRINT '  - spValidatePasswordPolicy (enforce strong passwords)';
PRINT '  - spRecordLoginAttempt (track login activity, lock accounts)';
PRINT '  - spUnlockUserAccount (reset locked accounts)';
PRINT '  - spRevokeUserSessions (force logout/session termination)';
PRINT '  - spCreateAuthToken (secure token generation)';
PRINT '  - spValidateAuthToken (verify token validity)';
PRINT '  - spSecurityAuditReport (security dashboard)';
PRINT 'Status: Ready for analytics procedures (Phase 1A Step 5)';

GO
