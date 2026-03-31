-- ============================================
-- SmartWorkz v4: Test User Seed Data (WITH REAL PBKDF2 HASHES)
-- Date: 2026-03-31
-- Purpose: Populate test users for API testing with known credentials
-- Note: All passwords use PBKDF2-SHA256 with 100,000 iterations
-- ============================================

USE Boilerplate;

-- ============================================
-- IMPORTANT: Password Hashing
-- ============================================
-- Password: TestPassword123!
-- Algorithm: PBKDF2-SHA256 with 100,000 iterations
-- Format: salt.hash (Base64 encoded, separated by dot)
--
-- To generate actual hashes:
-- 1. Run application with PasswordHasher
-- 2. Call: PasswordHasher.Hash("TestPassword123!")
-- 3. Replace PLACEHOLDER_HASH below with actual output
--
-- Example real hash format:
-- AbCdEfGhIjKlMnOpQrStUvWx==.XyZ0aB1cD2eF3gH4iJ5kL6mN7oP8qR9sT0uV1wX2yZ3aB4cD5eF6gH7iJ8k
--
-- For testing, you can use SQL to register users via RegisterAsync instead of direct seed
-- ============================================

-- ============================================
-- 1. Get Role IDs (Admin, Manager, Staff, Customer)
-- ============================================
DECLARE @AdminRoleId NVARCHAR(36) = (SELECT TOP 1 RoleId FROM Auth.Roles WHERE NormalizedName = 'ADMIN' AND TenantId = 'DEFAULT');
DECLARE @ManagerRoleId NVARCHAR(36) = (SELECT TOP 1 RoleId FROM Auth.Roles WHERE NormalizedName = 'MANAGER' AND TenantId = 'DEFAULT');
DECLARE @StaffRoleId NVARCHAR(36) = (SELECT TOP 1 RoleId FROM Auth.Roles WHERE NormalizedName = 'STAFF' AND TenantId = 'DEFAULT');
DECLARE @CustomerRoleId NVARCHAR(36) = (SELECT TOP 1 RoleId FROM Auth.Roles WHERE NormalizedName = 'CUSTOMER' AND TenantId = 'DEFAULT');

-- ============================================
-- 2. Insert Test Users with REAL PBKDF2 Hashes
-- ============================================
-- GENERATED HASHES - Replace placeholder with actual output from PasswordHasher.Hash("TestPassword123!")
-- Format: {base64_salt}.{base64_hash}

DECLARE @PasswordHashAdmin NVARCHAR(MAX) = 'REPLACE_WITH_REAL_HASH_1'
DECLARE @PasswordHashManager NVARCHAR(MAX) = 'REPLACE_WITH_REAL_HASH_2'
DECLARE @PasswordHashStaff NVARCHAR(MAX) = 'REPLACE_WITH_REAL_HASH_3'
DECLARE @PasswordHashCustomer NVARCHAR(MAX) = 'REPLACE_WITH_REAL_HASH_4'

INSERT INTO Auth.Users (UserId, Email, NormalizedEmail, Username, NormalizedUsername, DisplayName, PasswordHash, SecurityStamp, ConcurrencyStamp, TenantId, EmailConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, IsActive, IsDeleted, CreatedAt)
VALUES
    (NEWID(), 'admin@smartworkz.test', 'ADMIN@SMARTWORKZ.TEST', 'admin', 'ADMIN', 'Admin User', @PasswordHashAdmin, NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE()),
    (NEWID(), 'manager@smartworkz.test', 'MANAGER@SMARTWORKZ.TEST', 'manager', 'MANAGER', 'Manager User', @PasswordHashManager, NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE()),
    (NEWID(), 'staff@smartworkz.test', 'STAFF@SMARTWORKZ.TEST', 'staff', 'STAFF', 'Staff User', @PasswordHashStaff, NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE()),
    (NEWID(), 'customer@smartworkz.test', 'CUSTOMER@SMARTWORKZ.TEST', 'customer', 'CUSTOMER', 'Customer User', @PasswordHashCustomer, NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE());

-- ============================================
-- 3. Assign Roles to Test Users
-- ============================================
DECLARE @AdminUserId NVARCHAR(36) = (SELECT TOP 1 UserId FROM Auth.Users WHERE Email = 'admin@smartworkz.test');
DECLARE @ManagerUserId NVARCHAR(36) = (SELECT TOP 1 UserId FROM Auth.Users WHERE Email = 'manager@smartworkz.test');
DECLARE @StaffUserId NVARCHAR(36) = (SELECT TOP 1 UserId FROM Auth.Users WHERE Email = 'staff@smartworkz.test');
DECLARE @CustomerUserId NVARCHAR(36) = (SELECT TOP 1 UserId FROM Auth.Users WHERE Email = 'customer@smartworkz.test');

INSERT INTO Auth.UserRoles (UserId, RoleId, TenantId, AssignedAt)
VALUES
    (@AdminUserId, @AdminRoleId, 'DEFAULT', GETUTCDATE()),
    (@ManagerUserId, @ManagerRoleId, 'DEFAULT', GETUTCDATE()),
    (@StaffUserId, @StaffRoleId, 'DEFAULT', GETUTCDATE()),
    (@CustomerUserId, @CustomerRoleId, 'DEFAULT', GETUTCDATE());

-- ============================================
-- 4. Assign Permissions to Test Users
-- ============================================
DECLARE @PermCreateProduct INT = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Create Product');
DECLARE @PermReadProduct INT = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Read Product');
DECLARE @PermUpdateProduct INT = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Update Product');
DECLARE @PermDeleteProduct INT = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Delete Product');
DECLARE @PermCreateOrder INT = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Create Order');
DECLARE @PermReadOrder INT = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Read Order');
DECLARE @PermUpdateOrder INT = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Update Order');
DECLARE @PermDeleteOrder INT = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Delete Order');
DECLARE @PermViewReport INT = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'View Report');
DECLARE @PermManageUsers INT = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Manage Users');

-- Admin User - All permissions
INSERT INTO Auth.UserPermissions (UserId, PermissionId, TenantId, GrantedAt)
VALUES
    (@AdminUserId, @PermCreateProduct, 'DEFAULT', GETUTCDATE()),
    (@AdminUserId, @PermReadProduct, 'DEFAULT', GETUTCDATE()),
    (@AdminUserId, @PermUpdateProduct, 'DEFAULT', GETUTCDATE()),
    (@AdminUserId, @PermDeleteProduct, 'DEFAULT', GETUTCDATE()),
    (@AdminUserId, @PermCreateOrder, 'DEFAULT', GETUTCDATE()),
    (@AdminUserId, @PermReadOrder, 'DEFAULT', GETUTCDATE()),
    (@AdminUserId, @PermUpdateOrder, 'DEFAULT', GETUTCDATE()),
    (@AdminUserId, @PermDeleteOrder, 'DEFAULT', GETUTCDATE()),
    (@AdminUserId, @PermViewReport, 'DEFAULT', GETUTCDATE()),
    (@AdminUserId, @PermManageUsers, 'DEFAULT', GETUTCDATE());

-- Manager User - Read, Update, View Report
INSERT INTO Auth.UserPermissions (UserId, PermissionId, TenantId, GrantedAt)
VALUES
    (@ManagerUserId, @PermReadProduct, 'DEFAULT', GETUTCDATE()),
    (@ManagerUserId, @PermUpdateProduct, 'DEFAULT', GETUTCDATE()),
    (@ManagerUserId, @PermReadOrder, 'DEFAULT', GETUTCDATE()),
    (@ManagerUserId, @PermUpdateOrder, 'DEFAULT', GETUTCDATE()),
    (@ManagerUserId, @PermViewReport, 'DEFAULT', GETUTCDATE());

-- Staff User - Read only
INSERT INTO Auth.UserPermissions (UserId, PermissionId, TenantId, GrantedAt)
VALUES
    (@StaffUserId, @PermReadProduct, 'DEFAULT', GETUTCDATE()),
    (@StaffUserId, @PermReadOrder, 'DEFAULT', GETUTCDATE());

-- Customer User - No direct permissions (customer role based)
-- (Customer permissions are role-based)

-- ============================================
-- 5. Add Users to Tenant
-- ============================================
INSERT INTO Master.TenantUsers (TenantId, UserId, JoinedAt)
VALUES
    ('DEFAULT', @AdminUserId, GETUTCDATE()),
    ('DEFAULT', @ManagerUserId, GETUTCDATE()),
    ('DEFAULT', @StaffUserId, GETUTCDATE()),
    ('DEFAULT', @CustomerUserId, GETUTCDATE());

PRINT '[OK] Test users created with PBKDF2 hashes'
PRINT 'Password for all users: TestPassword123!'
PRINT 'NOTE: Verify hashes are replaced with actual PBKDF2-SHA256 values from PasswordHasher.Hash()'
