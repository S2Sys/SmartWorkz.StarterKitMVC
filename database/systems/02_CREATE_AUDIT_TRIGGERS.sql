-- ============================================
-- Phase 1A: Audit & Compliance Triggers
-- Purpose: Auto-audit triggers for change tracking and compliance
-- Database: SQL Server (Boilerplate v3)
-- Schemas: Master, Shared, Auth
-- Reusable: YES - Shared across multiple applications
-- Date: 2026-04-18
-- ============================================

USE Boilerplate;

SET NOCOUNT ON;

-- ============================================
-- AUDIT TRIGGER FOR MASTER.USERS (Auth Schema)
-- ============================================

-- Create trigger for Users table
IF OBJECT_ID('Auth.tr_Users_Audit', 'TR') IS NOT NULL
    DROP TRIGGER Auth.tr_Users_Audit;

GO

CREATE TRIGGER Auth.tr_Users_Audit
ON Auth.Users
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId NVARCHAR(128);
    DECLARE @Action NVARCHAR(10);
    DECLARE @Changes NVARCHAR(MAX);
    DECLARE @OldValues NVARCHAR(MAX) = '';
    DECLARE @NewValues NVARCHAR(MAX) = '';

    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        IF EXISTS (SELECT 1 FROM deleted)
        BEGIN
            -- UPDATE operation
            SET @Action = 'UPDATE';

            SELECT TOP 1 @UserId = UserId FROM inserted;

            -- Build old and new values as JSON
            SELECT @OldValues = (
                SELECT TOP 1
                    UserId, Email, FirstName, LastName, IsActive, IsLocked, FailedLoginAttempts,
                    LastLoginAt, CreatedAt, UpdatedAt
                FROM deleted
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            );

            SELECT @NewValues = (
                SELECT TOP 1
                    UserId, Email, FirstName, LastName, IsActive, IsLocked, FailedLoginAttempts,
                    LastLoginAt, CreatedAt, UpdatedAt
                FROM inserted
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            );

            INSERT INTO Auth.AuditTrail (UserId, Action, TableName, RecordId, OldValues, NewValues, ChangedAt, ChangedBy)
            SELECT TOP 1
                i.UserId,
                @Action,
                'Auth.Users',
                i.UserId,
                @OldValues,
                @NewValues,
                GETUTCDATE(),
                i.UpdatedBy
            FROM inserted i;
        END
        ELSE
        BEGIN
            -- INSERT operation
            SET @Action = 'INSERT';

            SELECT TOP 1 @UserId = UserId FROM inserted;

            SELECT @NewValues = (
                SELECT TOP 1
                    UserId, Email, FirstName, LastName, IsActive, IsLocked, FailedLoginAttempts,
                    LastLoginAt, CreatedAt
                FROM inserted
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            );

            INSERT INTO Auth.AuditTrail (UserId, Action, TableName, RecordId, NewValues, ChangedAt, ChangedBy)
            SELECT TOP 1
                i.UserId,
                @Action,
                'Auth.Users',
                i.UserId,
                @NewValues,
                GETUTCDATE(),
                i.CreatedBy
            FROM inserted i;
        END
    END
END;

GO

-- ============================================
-- AUDIT TRIGGER FOR MASTER.ROLES
-- ============================================

IF OBJECT_ID('Auth.tr_Roles_Audit', 'TR') IS NOT NULL
    DROP TRIGGER Auth.tr_Roles_Audit;

GO

CREATE TRIGGER Auth.tr_Roles_Audit
ON Auth.Roles
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RoleId INT;

    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        SELECT TOP 1 @RoleId = RoleId FROM inserted;

        IF EXISTS (SELECT 1 FROM deleted)
        BEGIN
            -- UPDATE
            INSERT INTO Auth.AuditTrail (Action, TableName, RecordId, OldValues, NewValues, ChangedAt)
            SELECT TOP 1
                'UPDATE',
                'Auth.Roles',
                i.RoleId,
                (SELECT * FROM deleted d WHERE d.RoleId = i.RoleId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                (SELECT * FROM inserted i2 WHERE i2.RoleId = i.RoleId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                GETUTCDATE()
            FROM inserted i;
        END
        ELSE
        BEGIN
            -- INSERT
            INSERT INTO Auth.AuditTrail (Action, TableName, RecordId, NewValues, ChangedAt)
            SELECT TOP 1
                'INSERT',
                'Auth.Roles',
                i.RoleId,
                (SELECT * FROM inserted i2 WHERE i2.RoleId = i.RoleId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                GETUTCDATE()
            FROM inserted i;
        END
    END
END;

GO

-- ============================================
-- AUDIT TRIGGER FOR MASTER.PERMISSIONS
-- ============================================

IF OBJECT_ID('Auth.tr_Permissions_Audit', 'TR') IS NOT NULL
    DROP TRIGGER Auth.tr_Permissions_Audit;

GO

CREATE TRIGGER Auth.tr_Permissions_Audit
ON Auth.Permissions
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PermissionId INT;

    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        SELECT TOP 1 @PermissionId = PermissionId FROM inserted;

        IF EXISTS (SELECT 1 FROM deleted)
        BEGIN
            -- UPDATE
            INSERT INTO Auth.AuditTrail (Action, TableName, RecordId, OldValues, NewValues, ChangedAt)
            SELECT TOP 1
                'UPDATE',
                'Auth.Permissions',
                i.PermissionId,
                (SELECT * FROM deleted d WHERE d.PermissionId = i.PermissionId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                (SELECT * FROM inserted i2 WHERE i2.PermissionId = i.PermissionId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                GETUTCDATE()
            FROM inserted i;
        END
        ELSE
        BEGIN
            -- INSERT
            INSERT INTO Auth.AuditTrail (Action, TableName, RecordId, NewValues, ChangedAt)
            SELECT TOP 1
                'INSERT',
                'Auth.Permissions',
                i.PermissionId,
                (SELECT * FROM inserted i2 WHERE i2.PermissionId = i.PermissionId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                GETUTCDATE()
            FROM inserted i;
        END
    END
END;

GO

-- ============================================
-- AUDIT TRIGGER FOR MASTER.CONFIGURATION
-- ============================================

IF OBJECT_ID('Master.tr_Configuration_Audit', 'TR') IS NOT NULL
    DROP TRIGGER Master.tr_Configuration_Audit;

GO

CREATE TRIGGER Master.tr_Configuration_Audit
ON Master.Configuration
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ConfigId INT;

    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        SELECT TOP 1 @ConfigId = ConfigId FROM inserted;

        IF EXISTS (SELECT 1 FROM deleted)
        BEGIN
            -- UPDATE
            INSERT INTO Auth.AuditTrail (Action, TableName, RecordId, OldValues, NewValues, ChangedAt)
            SELECT TOP 1
                'UPDATE',
                'Master.Configuration',
                i.ConfigId,
                (SELECT [Key], Value, ConfigType FROM deleted d WHERE d.ConfigId = i.ConfigId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                (SELECT [Key], Value, ConfigType FROM inserted i2 WHERE i2.ConfigId = i.ConfigId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                GETUTCDATE()
            FROM inserted i;
        END
        ELSE
        BEGIN
            -- INSERT
            INSERT INTO Auth.AuditTrail (Action, TableName, RecordId, NewValues, ChangedAt)
            SELECT TOP 1
                'INSERT',
                'Master.Configuration',
                i.ConfigId,
                (SELECT [Key], Value, ConfigType FROM inserted i2 WHERE i2.ConfigId = i.ConfigId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                GETUTCDATE()
            FROM inserted i;
        END
    END
END;

GO

-- ============================================
-- AUDIT TRIGGER FOR MASTER.FEATUREFLAGS
-- ============================================

IF OBJECT_ID('Master.tr_FeatureFlags_Audit', 'TR') IS NOT NULL
    DROP TRIGGER Master.tr_FeatureFlags_Audit;

GO

CREATE TRIGGER Master.tr_FeatureFlags_Audit
ON Master.FeatureFlags
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FlagId INT;

    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        SELECT TOP 1 @FlagId = FeatureFlagId FROM inserted;

        IF EXISTS (SELECT 1 FROM deleted)
        BEGIN
            -- UPDATE
            INSERT INTO Auth.AuditTrail (Action, TableName, RecordId, OldValues, NewValues, ChangedAt)
            SELECT TOP 1
                'UPDATE',
                'Master.FeatureFlags',
                i.FeatureFlagId,
                (SELECT Name, IsEnabled, ValidFrom, ValidTo FROM deleted d WHERE d.FeatureFlagId = i.FeatureFlagId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                (SELECT Name, IsEnabled, ValidFrom, ValidTo FROM inserted i2 WHERE i2.FeatureFlagId = i.FeatureFlagId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                GETUTCDATE()
            FROM inserted i;
        END
        ELSE
        BEGIN
            -- INSERT
            INSERT INTO Auth.AuditTrail (Action, TableName, RecordId, NewValues, ChangedAt)
            SELECT TOP 1
                'INSERT',
                'Master.FeatureFlags',
                i.FeatureFlagId,
                (SELECT Name, IsEnabled, ValidFrom, ValidTo FROM inserted i2 WHERE i2.FeatureFlagId = i.FeatureFlagId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
                GETUTCDATE()
            FROM inserted i;
        END
    END
END;

GO

-- ============================================
-- AUTO-UPDATE TIMESTAMPS
-- ============================================

-- Trigger to auto-update UpdatedAt for Users
IF OBJECT_ID('Auth.tr_Users_UpdateTimestamp', 'TR') IS NOT NULL
    DROP TRIGGER Auth.tr_Users_UpdateTimestamp;

GO

CREATE TRIGGER Auth.tr_Users_UpdateTimestamp
ON Auth.Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Auth.Users
    SET UpdatedAt = GETUTCDATE()
    WHERE UserId IN (SELECT UserId FROM inserted);
END;

GO

-- Trigger to auto-update UpdatedAt for Roles
IF OBJECT_ID('Auth.tr_Roles_UpdateTimestamp', 'TR') IS NOT NULL
    DROP TRIGGER Auth.tr_Roles_UpdateTimestamp;

GO

CREATE TRIGGER Auth.tr_Roles_UpdateTimestamp
ON Auth.Roles
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Auth.Roles
    SET UpdatedAt = GETUTCDATE()
    WHERE RoleId IN (SELECT RoleId FROM inserted);
END;

GO

-- Trigger to auto-update UpdatedAt for Configuration
IF OBJECT_ID('Master.tr_Configuration_UpdateTimestamp', 'TR') IS NOT NULL
    DROP TRIGGER Master.tr_Configuration_UpdateTimestamp;

GO

CREATE TRIGGER Master.tr_Configuration_UpdateTimestamp
ON Master.Configuration
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Master.Configuration
    SET UpdatedAt = GETUTCDATE()
    WHERE ConfigurationId IN (SELECT ConfigurationId FROM inserted);
END;

GO

PRINT '✅ Phase 1A: Audit Triggers successfully created';
PRINT 'Total triggers created:';
PRINT '  - 5 Audit triggers (Users, Roles, Permissions, Configuration, FeatureFlags)';
PRINT '  - 3 Timestamp auto-update triggers';
PRINT '  - Change tracking enabled (JSON before/after values)';
PRINT 'Status: Audit trail now tracking all critical changes';

GO
