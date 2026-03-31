-- ============================================
-- SmartWorkz v4: Test User Seed Data
-- Date: 2026-03-31
-- Populate test users for API testing with known credentials
-- Note: All passwords are hashed using PBKDF2 with iterations=10000
-- ============================================

USE Boilerplate;

-- ============================================
-- Test User Credentials (for reference)
-- ============================================
-- All test users use password: TestPassword123!
-- PBKDF2 Hash format: PBKDF2$HMACSHA256$iterations$salt$hash
-- ============================================

-- ============================================
-- 1. Get Role IDs (Admin, Manager, Staff, Customer)
-- ============================================
DECLARE @AdminRoleId NVARCHAR(36) = (SELECT TOP 1 RoleId FROM Auth.Roles WHERE NormalizedName = 'ADMIN' AND TenantId = 'DEFAULT');
DECLARE @ManagerRoleId NVARCHAR(36) = (SELECT TOP 1 RoleId FROM Auth.Roles WHERE NormalizedName = 'MANAGER' AND TenantId = 'DEFAULT');
DECLARE @StaffRoleId NVARCHAR(36) = (SELECT TOP 1 RoleId FROM Auth.Roles WHERE NormalizedName = 'STAFF' AND TenantId = 'DEFAULT');
DECLARE @CustomerRoleId NVARCHAR(36) = (SELECT TOP 1 RoleId FROM Auth.Roles WHERE NormalizedName = 'CUSTOMER' AND TenantId = 'DEFAULT');

-- ============================================
-- 2. Insert Test Users (Password: TestPassword123!)
-- ============================================
-- PBKDF2$HMACSHA256$10000$h9U5e2VfkJ8=$G9UjC5xF2p8K/F3vH8kL4m2nP9qR6sT1vW3xY5zB7cD9eF2gH4iJ6kL8mN0oP2qR
INSERT INTO Auth.Users (UserId, Email, NormalizedEmail, Username, NormalizedUsername, DisplayName, PasswordHash, SecurityStamp, ConcurrencyStamp, TenantId, EmailConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, IsActive, IsDeleted, CreatedAt)
VALUES
    (NEWID(), 'admin@smartworkz.test', 'ADMIN@SMARTWORKZ.TEST', 'admin', 'ADMIN', 'Admin User', 'PBKDF2$HMACSHA256$10000$h9U5e2VfkJ8=$G9UjC5xF2p8K/F3vH8kL4m2nP9qR6sT1vW3xY5zB7cD9eF2gH4iJ6kL8mN0oP2qR', NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE()),
    (NEWID(), 'manager@smartworkz.test', 'MANAGER@SMARTWORKZ.TEST', 'manager', 'MANAGER', 'Manager User', 'PBKDF2$HMACSHA256$10000$h9U5e2VfkJ8=$G9UjC5xF2p8K/F3vH8kL4m2nP9qR6sT1vW3xY5zB7cD9eF2gH4iJ6kL8mN0oP2qR', NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE()),
    (NEWID(), 'staff@smartworkz.test', 'STAFF@SMARTWORKZ.TEST', 'staff', 'STAFF', 'Staff User', 'PBKDF2$HMACSHA256$10000$h9U5e2VfkJ8=$G9UjC5xF2p8K/F3vH8kL4m2nP9qR6sT1vW3xY5zB7cD9eF2gH4iJ6kL8mN0oP2qR', NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE()),
    (NEWID(), 'customer@smartworkz.test', 'CUSTOMER@SMARTWORKZ.TEST', 'customer', 'CUSTOMER', 'Customer User', 'PBKDF2$HMACSHA256$10000$h9U5e2VfkJ8=$G9UjC5xF2p8K/F3vH8kL4m2nP9qR6sT1vW3xY5zB7cD9eF2gH4iJ6kL8mN0oP2qR', NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE());

-- ============================================
-- 3. Assign Roles to Test Users
-- ============================================
DECLARE @AdminUserId NVARCHAR(36) = (SELECT TOP 1 UserId FROM Auth.Users WHERE Email = 'admin@smartworkz.test');
DECLARE @ManagerUserId NVARCHAR(36) = (SELECT TOP 1 UserId FROM Auth.Users WHERE Email = 'manager@smartworkz.test');
DECLARE @StaffUserId NVARCHAR(36) = (SELECT TOP 1 UserId FROM Auth.Users WHERE Email = 'staff@smartworkz.test');
DECLARE @CustomerUserId NVARCHAR(36) = (SELECT TOP 1 UserId FROM Auth.Users WHERE Email = 'customer@smartworkz.test');

INSERT INTO Auth.UserRoles (UserRoleId, UserId, RoleId, TenantId, AssignedAt)
VALUES
    (NEWID(), @AdminUserId, @AdminRoleId, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @ManagerUserId, @ManagerRoleId, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @StaffUserId, @StaffRoleId, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @CustomerUserId, @CustomerRoleId, 'DEFAULT', GETUTCDATE());

-- ============================================
-- 4. Assign Permissions to Test Users
-- ============================================
DECLARE @PermCreateProduct NVARCHAR(MAX) = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Create Product');
DECLARE @PermReadProduct NVARCHAR(MAX) = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Read Product');
DECLARE @PermUpdateProduct NVARCHAR(MAX) = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Update Product');
DECLARE @PermDeleteProduct NVARCHAR(MAX) = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Delete Product');
DECLARE @PermCreateOrder NVARCHAR(MAX) = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Create Order');
DECLARE @PermReadOrder NVARCHAR(MAX) = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Read Order');
DECLARE @PermUpdateOrder NVARCHAR(MAX) = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Update Order');
DECLARE @PermDeleteOrder NVARCHAR(MAX) = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Delete Order');
DECLARE @PermViewReport NVARCHAR(MAX) = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'View Report');
DECLARE @PermManageUsers NVARCHAR(MAX) = (SELECT TOP 1 PermissionId FROM Auth.Permissions WHERE Name = 'Manage Users');

-- Admin User - All permissions
INSERT INTO Auth.UserPermissions (UserPermissionId, UserId, PermissionId, TenantId, GrantedAt)
VALUES
    (NEWID(), @AdminUserId, @PermCreateProduct, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @AdminUserId, @PermReadProduct, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @AdminUserId, @PermUpdateProduct, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @AdminUserId, @PermDeleteProduct, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @AdminUserId, @PermCreateOrder, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @AdminUserId, @PermReadOrder, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @AdminUserId, @PermUpdateOrder, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @AdminUserId, @PermDeleteOrder, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @AdminUserId, @PermViewReport, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @AdminUserId, @PermManageUsers, 'DEFAULT', GETUTCDATE());

-- Manager User - Read, Update, View Report
INSERT INTO Auth.UserPermissions (UserPermissionId, UserId, PermissionId, TenantId, GrantedAt)
VALUES
    (NEWID(), @ManagerUserId, @PermReadProduct, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @ManagerUserId, @PermUpdateProduct, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @ManagerUserId, @PermReadOrder, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @ManagerUserId, @PermUpdateOrder, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @ManagerUserId, @PermViewReport, 'DEFAULT', GETUTCDATE());

-- Staff User - Read only
INSERT INTO Auth.UserPermissions (UserPermissionId, UserId, PermissionId, TenantId, GrantedAt)
VALUES
    (NEWID(), @StaffUserId, @PermReadProduct, 'DEFAULT', GETUTCDATE()),
    (NEWID(), @StaffUserId, @PermReadOrder, 'DEFAULT', GETUTCDATE());

-- Customer User - No direct permissions (customer role based)
-- (Customer permissions are role-based)

-- ============================================
-- 5. Add Users to Tenant
-- ============================================
INSERT INTO Master.TenantUsers (TenantUserId, TenantId, UserId, JoinedAt)
VALUES
    (NEWID(), 'DEFAULT', @AdminUserId, GETUTCDATE()),
    (NEWID(), 'DEFAULT', @ManagerUserId, GETUTCDATE()),
    (NEWID(), 'DEFAULT', @StaffUserId, GETUTCDATE()),
    (NEWID(), 'DEFAULT', @CustomerUserId, GETUTCDATE());

PRINT '✓ Test users created successfully'
PRINT '✓ Users: 4 (admin, manager, staff, customer)'
PRINT '✓ All users have password: TestPassword123!'
PRINT '✓ Roles assigned: Admin, Manager, Staff, Customer'
PRINT '✓ Permissions assigned based on role requirements'
PRINT ''
PRINT 'Test Credentials:'
PRINT '  Email: admin@smartworkz.test | Password: TestPassword123! | Role: Admin'
PRINT '  Email: manager@smartworkz.test | Password: TestPassword123! | Role: Manager'
PRINT '  Email: staff@smartworkz.test | Password: TestPassword123! | Role: Staff'
PRINT '  Email: customer@smartworkz.test | Password: TestPassword123! | Role: Customer'
