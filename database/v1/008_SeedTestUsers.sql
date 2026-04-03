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
    -- Hash format: salt.hash (Base64 encoded with PBKDF2-SHA256, 100,000 iterations)
    -- Use PasswordHasher to generate actual hash if needed
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
    -- PBKDF2-SHA256 Hash (100,000 iterations)
    -- Password: TestPassword123!
    -- Note: These are placeholder values - UPDATE WITH ACTUAL HASHES
    INSERT INTO Auth.Users (UserId, Email, NormalizedEmail, Username, NormalizedUsername, DisplayName, PasswordHash, SecurityStamp, ConcurrencyStamp, TenantId, EmailConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, IsActive, IsDeleted, CreatedAt)
    VALUES
        (NEWID(), 'admin@smartworkz.test', 'ADMIN@SMARTWORKZ.TEST', 'admin', 'ADMIN', 'Admin User', 'k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=', NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE()),
        (NEWID(), 'manager@smartworkz.test', 'MANAGER@SMARTWORKZ.TEST', 'manager', 'MANAGER', 'Manager User', 'k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=', NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE()),
        (NEWID(), 'staff@smartworkz.test', 'STAFF@SMARTWORKZ.TEST', 'staff', 'STAFF', 'Staff User', 'k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=', NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE()),
        (NEWID(), 'customer@smartworkz.test', 'CUSTOMER@SMARTWORKZ.TEST', 'customer', 'CUSTOMER', 'Customer User', 'k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=', NEWID(), NEWID(), 'DEFAULT', 1, 0, 1, 0, 1, 0, GETUTCDATE());

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
