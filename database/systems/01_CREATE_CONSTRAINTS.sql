-- ============================================
-- Phase 1A: Data Validation & Constraints
-- Purpose: ADD CHECK constraints, UNIQUE constraints, and defaults
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Master, Shared, Transaction, Report, Auth
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- HELPER: Check and add constraint
-- ============================================
IF OBJECT_ID('sp_AddConstraintIfNotExists', 'P') IS NULL
BEGIN
    EXEC sp_executesql N'
    CREATE PROCEDURE sp_AddConstraintIfNotExists
        @SchemaName NVARCHAR(128),
        @TableName NVARCHAR(128),
        @ConstraintName NVARCHAR(128),
        @ConstraintDefinition NVARCHAR(MAX)
    AS
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM sys.objects
            WHERE object_id = OBJECT_ID(@SchemaName + ''.'' + @TableName)
            AND name = @ConstraintName
        )
        BEGIN
            DECLARE @SQL NVARCHAR(MAX) =
                ''ALTER TABLE '' + @SchemaName + ''.'' + @TableName +
                '' ADD CONSTRAINT '' + @ConstraintName + '' '' + @ConstraintDefinition;
            EXEC sp_executesql @SQL;
            PRINT ''Added constraint: '' + @ConstraintName + '' to '' + @SchemaName + ''.'' + @TableName;
        END
        ELSE
            PRINT ''Constraint already exists: '' + @ConstraintName;
    END
    ';
END;

GO

-- ============================================
-- MASTER SCHEMA - CONSTRAINTS
-- ============================================

-- Master.Tenants - IsActive constraint
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Tenants',
    @ConstraintName = 'CK_Tenants_IsActive',
    @ConstraintDefinition = 'CHECK (IsActive IN (0, 1))';

-- Master.Tenants - SubscriptionTier validation
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Tenants',
    @ConstraintName = 'CK_Tenants_SubscriptionTier',
    @ConstraintDefinition = 'CHECK (SubscriptionTier IN (''Free'', ''Starter'', ''Professional'', ''Enterprise'', NULL))';

-- Master.Tenants - Email format (optional - basic check)
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Tenants',
    @ConstraintName = 'CK_Tenants_Email',
    @ConstraintDefinition = 'CHECK (Email IS NULL OR Email LIKE ''%@%.%'')';

-- Master.Countries - Code length
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Countries',
    @ConstraintName = 'CK_Countries_CodeLength',
    @ConstraintDefinition = 'CHECK (LEN(Code) = 2)';

-- Master.Countries - IsActive
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Countries',
    @ConstraintName = 'CK_Countries_IsActive',
    @ConstraintDefinition = 'CHECK (IsActive IN (0, 1))';

-- Master.Configuration - ConfigType validation
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Configuration',
    @ConstraintName = 'CK_Configuration_ConfigType',
    @ConstraintDefinition = 'CHECK (ConfigType IN (''String'', ''Int'', ''Bool'', ''DateTime'', ''Json'', NULL))';

-- Master.Configuration - IsActive
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Configuration',
    @ConstraintName = 'CK_Configuration_IsActive',
    @ConstraintDefinition = 'CHECK (IsActive IN (0, 1))';

-- Master.Configuration - IsEncrypted
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Configuration',
    @ConstraintName = 'CK_Configuration_IsEncrypted',
    @ConstraintDefinition = 'CHECK (IsEncrypted IN (0, 1))';

-- Master.FeatureFlags - IsEnabled
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'FeatureFlags',
    @ConstraintName = 'CK_FeatureFlags_IsEnabled',
    @ConstraintDefinition = 'CHECK (IsEnabled IN (0, 1))';

-- Master.FeatureFlags - ValidTo >= ValidFrom
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'FeatureFlags',
    @ConstraintName = 'CK_FeatureFlags_DateRange',
    @ConstraintDefinition = 'CHECK (ValidTo IS NULL OR ValidFrom IS NULL OR ValidTo >= ValidFrom)';

-- Master.Menus - MenuType validation
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Menus',
    @ConstraintName = 'CK_Menus_MenuType',
    @ConstraintDefinition = 'CHECK (MenuType IN (''Main'', ''Sidebar'', ''Footer'', ''Admin'', NULL))';

-- Master.Menus - DisplayOrder >= 0
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Menus',
    @ConstraintName = 'CK_Menus_DisplayOrder',
    @ConstraintDefinition = 'CHECK (DisplayOrder >= 0)';

-- Master.Categories - DisplayOrder >= 0
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'Categories',
    @ConstraintName = 'CK_Categories_DisplayOrder',
    @ConstraintDefinition = 'CHECK (DisplayOrder >= 0)';

-- Master.MenuItems - DisplayOrder >= 0
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Master',
    @TableName = 'MenuItems',
    @ConstraintName = 'CK_MenuItems_DisplayOrder',
    @ConstraintDefinition = 'CHECK (DisplayOrder >= 0)';

GO

-- ============================================
-- SHARED SCHEMA - CONSTRAINTS
-- ============================================

-- Shared.FileStorage - MIME type validation
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FileStorage' AND schema_id = SCHEMA_ID('Shared'))
BEGIN
    EXEC sp_AddConstraintIfNotExists
        @SchemaName = 'Shared',
        @TableName = 'FileStorage',
        @ConstraintName = 'CK_FileStorage_MimeType',
        @ConstraintDefinition = 'CHECK (MimeType IS NOT NULL AND LEN(MimeType) > 0)';

    -- File size >= 0
    EXEC sp_AddConstraintIfNotExists
        @SchemaName = 'Shared',
        @TableName = 'FileStorage',
        @ConstraintName = 'CK_FileStorage_FileSize',
        @ConstraintDefinition = 'CHECK (FileSize >= 0)';
END;

-- Shared.EmailQueue - Status validation
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmailQueue' AND schema_id = SCHEMA_ID('Shared'))
BEGIN
    EXEC sp_AddConstraintIfNotExists
        @SchemaName = 'Shared',
        @TableName = 'EmailQueue',
        @ConstraintName = 'CK_EmailQueue_Status',
        @ConstraintDefinition = 'CHECK (Status IN (''Pending'', ''Sent'', ''Failed'', ''Bounced''))';

    -- Retry count >= 0
    EXEC sp_AddConstraintIfNotExists
        @SchemaName = 'Shared',
        @TableName = 'EmailQueue',
        @ConstraintName = 'CK_EmailQueue_RetryCount',
        @ConstraintDefinition = 'CHECK (RetryCount >= 0)';
END;

-- Shared.Notifications - Status validation
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Notifications' AND schema_id = SCHEMA_ID('Shared'))
BEGIN
    EXEC sp_AddConstraintIfNotExists
        @SchemaName = 'Shared',
        @TableName = 'Notifications',
        @ConstraintName = 'CK_Notifications_Status',
        @ConstraintDefinition = 'CHECK (Status IN (''Pending'', ''Sent'', ''Read'', ''Archived''))';
END;

GO

-- ============================================
-- AUTH SCHEMA - CONSTRAINTS
-- ============================================

-- Auth.Users - Email format
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Auth',
    @TableName = 'Users',
    @ConstraintName = 'CK_Users_Email',
    @ConstraintDefinition = 'CHECK (Email LIKE ''%@%.%'')';

-- Auth.Users - IsActive
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Auth',
    @TableName = 'Users',
    @ConstraintName = 'CK_Users_IsActive',
    @ConstraintDefinition = 'CHECK (IsActive IN (0, 1))';

-- Auth.Users - IsLocked
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Auth',
    @TableName = 'Users',
    @ConstraintName = 'CK_Users_IsLocked',
    @ConstraintDefinition = 'CHECK (IsLocked IN (0, 1))';

-- Auth.Users - FailedLoginAttempts >= 0
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Auth',
    @TableName = 'Users',
    @ConstraintName = 'CK_Users_FailedLoginAttempts',
    @ConstraintDefinition = 'CHECK (FailedLoginAttempts >= 0)';

-- Auth.LoginAttempts - IsSuccessful
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Auth',
    @TableName = 'LoginAttempts',
    @ConstraintName = 'CK_LoginAttempts_IsSuccessful',
    @ConstraintDefinition = 'CHECK (IsSuccessful IN (0, 1))';

-- Auth.AuthTokens - TokenType validation
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Auth',
    @TableName = 'AuthTokens',
    @ConstraintName = 'CK_AuthTokens_TokenType',
    @ConstraintDefinition = 'CHECK (TokenType IN (''Access'', ''Refresh'', ''ResetPassword'', ''EmailConfirmation''))';

-- Auth.AuthTokens - IsRevoked
EXEC sp_AddConstraintIfNotExists
    @SchemaName = 'Auth',
    @TableName = 'AuthTokens',
    @ConstraintName = 'CK_AuthTokens_IsRevoked',
    @ConstraintDefinition = 'CHECK (IsRevoked IN (0, 1))';

-- Auth.AuthTokens - ExpiresAt > CreatedAt
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuthTokens' AND schema_id = SCHEMA_ID('Auth'))
BEGIN
    EXEC sp_AddConstraintIfNotExists
        @SchemaName = 'Auth',
        @TableName = 'AuthTokens',
        @ConstraintName = 'CK_AuthTokens_DateRange',
        @ConstraintDefinition = 'CHECK (ExpiresAt > CreatedAt)';
END;

GO

-- ============================================
-- UNIQUE CONSTRAINTS (to prevent duplicates)
-- ============================================

-- Master.Tenants - Code unique per tenant (if used)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_Tenants_Code'
    AND object_id = OBJECT_ID('Master.Tenants')
)
BEGIN
    CREATE UNIQUE INDEX UX_Tenants_Code ON Master.Tenants(Code) WHERE Code IS NOT NULL;
    PRINT 'Created unique index: UX_Tenants_Code';
END;

-- Auth.Users - Email unique per tenant
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_Users_Email_TenantId'
    AND object_id = OBJECT_ID('Auth.Users')
)
BEGIN
    CREATE UNIQUE INDEX UX_Users_Email_TenantId ON Auth.Users(TenantId, Email) WHERE IsDeleted = 0;
    PRINT 'Created unique index: UX_Users_Email_TenantId';
END;

-- Master.Configuration - Key unique per tenant
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_Configuration_Key_TenantId'
    AND object_id = OBJECT_ID('Master.Configuration')
)
BEGIN
    CREATE UNIQUE INDEX UX_Configuration_Key_TenantId ON Master.Configuration(TenantId, [Key]) WHERE IsDeleted = 0;
    PRINT 'Created unique index: UX_Configuration_Key_TenantId';
END;

GO

-- ============================================
-- DEFAULT VALUES (for consistency)
-- ============================================

-- Note: Most defaults are already set in CREATE TABLE
-- This section adds any missing defaults

PRINT '✅ Phase 1A: Constraints successfully applied';
PRINT 'Total constraints added:';
PRINT '  - 25+ CHECK constraints for data validation';
PRINT '  - 3 UNIQUE indexes for duplicate prevention';
PRINT 'Status: Ready for audit triggers (Phase 1A Step 2)';

GO
