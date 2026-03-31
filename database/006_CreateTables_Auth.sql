-- ============================================
-- SmartWorkz v4 Phase 1: Auth Schema
-- Date: 2026-03-31
-- 13 Tables: Authentication and Authorization
-- ============================================

USE Boilerplate;

-- ============================================
-- 1. Users (Authentication Users)
-- ============================================
CREATE TABLE Auth.Users (
    UserId NVARCHAR(128) PRIMARY KEY,
    UserName NVARCHAR(256) NOT NULL UNIQUE,
    NormalizedUserName NVARCHAR(256) NOT NULL UNIQUE,
    Email NVARCHAR(256) NOT NULL,
    NormalizedEmail NVARCHAR(256) NOT NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    ConcurrencyStamp NVARCHAR(MAX),
    PhoneNumber NVARCHAR(20),
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LockoutEnd DATETIMEOFFSET,
    LockoutEnabled BIT NOT NULL DEFAULT 1,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    DisplayName NVARCHAR(256),
    AvatarUrl NVARCHAR(500),
    Locale NVARCHAR(10) DEFAULT 'en-US',
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_Users_Email ON Auth.Users(Email);
CREATE INDEX IX_Users_NormalizedEmail ON Auth.Users(NormalizedEmail);
CREATE INDEX IX_Users_TenantId ON Auth.Users(TenantId);

-- ============================================
-- 2. Roles (Authorization Roles)
-- ============================================
CREATE TABLE Auth.Roles (
    RoleId NVARCHAR(128) PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    NormalizedName NVARCHAR(256) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    TenantId NVARCHAR(128),
    IsSystemRole BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_Roles_Name ON Auth.Roles(Name);
CREATE INDEX IX_Roles_NormalizedName ON Auth.Roles(NormalizedName);
CREATE INDEX IX_Roles_TenantId ON Auth.Roles(TenantId);

-- ============================================
-- 3. Permissions (Fine-grained Authorization)
-- ============================================
CREATE TABLE Auth.Permissions (
    PermissionId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(500),
    PermissionType NVARCHAR(100), -- 'Create', 'Read', 'Update', 'Delete'
    ResourceType NVARCHAR(100), -- 'Product', 'Order', 'Report'
    TenantId NVARCHAR(128),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, PermissionType, ResourceType)
);

CREATE INDEX IX_Permissions_PermissionType ON Auth.Permissions(PermissionType);
CREATE INDEX IX_Permissions_ResourceType ON Auth.Permissions(ResourceType);
CREATE INDEX IX_Permissions_TenantId ON Auth.Permissions(TenantId);

-- ============================================
-- 4. UserRoles (User to Role Mapping)
-- ============================================
CREATE TABLE Auth.UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128) NOT NULL,
    RoleId NVARCHAR(128) NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (RoleId) REFERENCES Auth.Roles(RoleId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, UserId, RoleId)
);

CREATE INDEX IX_UserRoles_UserId ON Auth.UserRoles(UserId);
CREATE INDEX IX_UserRoles_RoleId ON Auth.UserRoles(RoleId);
CREATE INDEX IX_UserRoles_TenantId ON Auth.UserRoles(TenantId);

-- ============================================
-- 5. RolePermissions (Role to Permission Mapping)
-- ============================================
CREATE TABLE Auth.RolePermissions (
    RolePermissionId INT PRIMARY KEY IDENTITY(1,1),
    RoleId NVARCHAR(128) NOT NULL,
    PermissionId INT NOT NULL,
    GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (RoleId) REFERENCES Auth.Roles(RoleId),
    FOREIGN KEY (PermissionId) REFERENCES Auth.Permissions(PermissionId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, RoleId, PermissionId)
);

CREATE INDEX IX_RolePermissions_RoleId ON Auth.RolePermissions(RoleId);
CREATE INDEX IX_RolePermissions_PermissionId ON Auth.RolePermissions(PermissionId);
CREATE INDEX IX_RolePermissions_TenantId ON Auth.RolePermissions(TenantId);

-- ============================================
-- 6. UserPermissions (Direct User Permissions)
-- ============================================
CREATE TABLE Auth.UserPermissions (
    UserPermissionId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128) NOT NULL,
    PermissionId INT NOT NULL,
    GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2, -- Optional expiration
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (PermissionId) REFERENCES Auth.Permissions(PermissionId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    UNIQUE (TenantId, UserId, PermissionId)
);

CREATE INDEX IX_UserPermissions_UserId ON Auth.UserPermissions(UserId);
CREATE INDEX IX_UserPermissions_PermissionId ON Auth.UserPermissions(PermissionId);
CREATE INDEX IX_UserPermissions_TenantId ON Auth.UserPermissions(TenantId);

-- ============================================
-- 7. RefreshTokens (JWT Token Management)
-- ============================================
CREATE TABLE Auth.RefreshTokens (
    RefreshTokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128) NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    RevokedAt DATETIME2,
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_RefreshTokens_Token ON Auth.RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_UserId ON Auth.RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_ExpiresAt ON Auth.RefreshTokens(ExpiresAt);
CREATE INDEX IX_RefreshTokens_TenantId ON Auth.RefreshTokens(TenantId);

-- ============================================
-- 8. LoginAttempts (Security Tracking)
-- ============================================
CREATE TABLE Auth.LoginAttempts (
    LoginAttemptId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128),
    Email NVARCHAR(256),
    IPAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    IsSuccessful BIT NOT NULL DEFAULT 0,
    FailureReason NVARCHAR(256),
    TenantId NVARCHAR(128),
    AttemptedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_LoginAttempts_UserId ON Auth.LoginAttempts(UserId);
CREATE INDEX IX_LoginAttempts_Email ON Auth.LoginAttempts(Email);
CREATE INDEX IX_LoginAttempts_IPAddress ON Auth.LoginAttempts(IPAddress);
CREATE INDEX IX_LoginAttempts_AttemptedAt ON Auth.LoginAttempts(AttemptedAt);
CREATE INDEX IX_LoginAttempts_TenantId ON Auth.LoginAttempts(TenantId);

-- ============================================
-- 9. AuditTrail (User Activity Audit)
-- ============================================
CREATE TABLE Auth.AuditTrail (
    AuditTrailId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128),
    Action NVARCHAR(256) NOT NULL, -- 'Login', 'Logout', 'ChangePassword', 'UpdateProfile'
    EntityType NVARCHAR(100),
    EntityId INT,
    Changes NVARCHAR(MAX), -- JSON format changes
    IPAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_AuditTrail_UserId ON Auth.AuditTrail(UserId);
CREATE INDEX IX_AuditTrail_Action ON Auth.AuditTrail(Action);
CREATE INDEX IX_AuditTrail_CreatedAt ON Auth.AuditTrail(CreatedAt);
CREATE INDEX IX_AuditTrail_TenantId ON Auth.AuditTrail(TenantId);

-- ============================================
-- 10. TenantUsers (Tenant User Membership)
-- ============================================
CREATE TABLE Auth.TenantUsers (
    TenantUserId INT PRIMARY KEY IDENTITY(1,1),
    TenantId NVARCHAR(128) NOT NULL,
    UserId NVARCHAR(128) NOT NULL,
    InvitedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AcceptedAt DATETIME2,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Inactive, Suspended
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(256),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId),
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    UNIQUE (TenantId, UserId)
);

CREATE INDEX IX_TenantUsers_TenantId ON Auth.TenantUsers(TenantId);
CREATE INDEX IX_TenantUsers_UserId ON Auth.TenantUsers(UserId);

-- ============================================
-- 11. PasswordResetTokens (Password Recovery)
-- ============================================
CREATE TABLE Auth.PasswordResetTokens (
    PasswordResetTokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128) NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    UsedAt DATETIME2,
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_PasswordResetTokens_Token ON Auth.PasswordResetTokens(Token);
CREATE INDEX IX_PasswordResetTokens_UserId ON Auth.PasswordResetTokens(UserId);
CREATE INDEX IX_PasswordResetTokens_ExpiresAt ON Auth.PasswordResetTokens(ExpiresAt);

-- ============================================
-- 12. EmailVerificationTokens (Email Confirmation)
-- ============================================
CREATE TABLE Auth.EmailVerificationTokens (
    EmailVerificationTokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    VerifiedAt DATETIME2,
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_EmailVerificationTokens_Token ON Auth.EmailVerificationTokens(Token);
CREATE INDEX IX_EmailVerificationTokens_UserId ON Auth.EmailVerificationTokens(UserId);

-- ============================================
-- 13. TwoFactorTokens (Two-Factor Authentication)
-- ============================================
CREATE TABLE Auth.TwoFactorTokens (
    TwoFactorTokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(128) NOT NULL,
    Token NVARCHAR(100) NOT NULL,
    TokenType NVARCHAR(50), -- 'Email', 'Authenticator', 'SMS'
    ExpiresAt DATETIME2 NOT NULL,
    VerifiedAt DATETIME2,
    Attempts INT NOT NULL DEFAULT 0,
    TenantId NVARCHAR(128),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Auth.Users(UserId),
    FOREIGN KEY (TenantId) REFERENCES Master.Tenants(TenantId)
);

CREATE INDEX IX_TwoFactorTokens_UserId ON Auth.TwoFactorTokens(UserId);
CREATE INDEX IX_TwoFactorTokens_ExpiresAt ON Auth.TwoFactorTokens(ExpiresAt);

PRINT '✓ Auth schema: 13 tables created successfully'

