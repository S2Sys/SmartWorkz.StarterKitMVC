-- ============================================
-- SmartWorkz v4 Phase 1: Seed Data
-- Date: 2026-03-31
-- Populate initial: Countries, Languages, Tenants, Roles, Menus
-- ============================================

USE Boilerplate;

-- ============================================
-- 1. Seed Tenants
-- ============================================
INSERT INTO Master.Tenants (TenantId, Name, DisplayName, Description, IsActive, CreatedBy)
VALUES
    ('DEFAULT', 'Default Tenant', 'Default Tenant', 'Default multi-tenant system', 1, 'System'),
    ('DEMO', 'Demo Tenant', 'Demo Tenant', 'Demo environment for testing', 1, 'System');

-- ============================================
-- 2. Seed Languages
-- ============================================
INSERT INTO Master.Languages (Code, Name, DisplayName, NativeName, IsDefault, TenantId, IsActive, CreatedBy)
VALUES
    ('en-US', 'English', 'English', 'English', 1, 'DEFAULT', 1, 'System'),
    ('es-ES', 'Spanish', 'Spanish', 'Español', 0, 'DEFAULT', 1, 'System'),
    ('fr-FR', 'French', 'French', 'Français', 0, 'DEFAULT', 1, 'System'),
    ('de-DE', 'German', 'German', 'Deutsch', 0, 'DEFAULT', 1, 'System'),
    ('it-IT', 'Italian', 'Italian', 'Italiano', 0, 'DEFAULT', 1, 'System'),
    ('pt-BR', 'Portuguese', 'Portuguese (Brazil)', 'Português (Brasil)', 0, 'DEFAULT', 1, 'System'),
    ('ja-JP', 'Japanese', 'Japanese', '日本語', 0, 'DEFAULT', 1, 'System'),
    ('zh-CN', 'Chinese', 'Chinese (Simplified)', '中文 (简体)', 0, 'DEFAULT', 1, 'System');

-- ============================================
-- 3. Seed Countries
-- ============================================
INSERT INTO Master.Countries (Code, Name, DisplayName, FlagEmoji, TenantId, IsActive, CreatedBy)
VALUES
    ('US', 'United States', 'United States', '🇺🇸', 'DEFAULT', 1, 'System'),
    ('GB', 'United Kingdom', 'United Kingdom', '🇬🇧', 'DEFAULT', 1, 'System'),
    ('CA', 'Canada', 'Canada', '🇨🇦', 'DEFAULT', 1, 'System'),
    ('AU', 'Australia', 'Australia', '🇦🇺', 'DEFAULT', 1, 'System'),
    ('ES', 'Spain', 'Spain', '🇪🇸', 'DEFAULT', 1, 'System'),
    ('FR', 'France', 'France', '🇫🇷', 'DEFAULT', 1, 'System'),
    ('DE', 'Germany', 'Germany', '🇩🇪', 'DEFAULT', 1, 'System'),
    ('IT', 'Italy', 'Italy', '🇮🇹', 'DEFAULT', 1, 'System'),
    ('JP', 'Japan', 'Japan', '🇯🇵', 'DEFAULT', 1, 'System'),
    ('CN', 'China', 'China', '🇨🇳', 'DEFAULT', 1, 'System'),
    ('IN', 'India', 'India', '🇮🇳', 'DEFAULT', 1, 'System'),
    ('BR', 'Brazil', 'Brazil', '🇧🇷', 'DEFAULT', 1, 'System');

-- ============================================
-- 4. Seed Currencies
-- ============================================
INSERT INTO Master.Currencies (Code, Name, Symbol, DecimalPlaces, TenantId, IsActive, CreatedBy)
VALUES
    ('USD', 'US Dollar', '$', 2, 'DEFAULT', 1, 'System'),
    ('EUR', 'Euro', '€', 2, 'DEFAULT', 1, 'System'),
    ('GBP', 'British Pound', '£', 2, 'DEFAULT', 1, 'System'),
    ('JPY', 'Japanese Yen', '¥', 0, 'DEFAULT', 1, 'System'),
    ('CNY', 'Chinese Yuan', '¥', 2, 'DEFAULT', 1, 'System'),
    ('INR', 'Indian Rupee', '₹', 2, 'DEFAULT', 1, 'System'),
    ('BRL', 'Brazilian Real', 'R$', 2, 'DEFAULT', 1, 'System'),
    ('CAD', 'Canadian Dollar', 'C$', 2, 'DEFAULT', 1, 'System'),
    ('AUD', 'Australian Dollar', 'A$', 2, 'DEFAULT', 1, 'System');

-- ============================================
-- 5. Seed Roles
-- ============================================
INSERT INTO Auth.Roles (RoleId, Name, NormalizedName, Description, TenantId, IsSystemRole, CreatedBy)
VALUES
    (NEWID(), 'Super Admin', 'SUPER_ADMIN', 'System Administrator with full access', 'DEFAULT', 1, 'System'),
    (NEWID(), 'Admin', 'ADMIN', 'Tenant Administrator', 'DEFAULT', 0, 'System'),
    (NEWID(), 'Manager', 'MANAGER', 'Manager role for business operations', 'DEFAULT', 0, 'System'),
    (NEWID(), 'Staff', 'STAFF', 'Staff member role', 'DEFAULT', 0, 'System'),
    (NEWID(), 'Customer', 'CUSTOMER', 'Customer role for public portal', 'DEFAULT', 0, 'System'),
    (NEWID(), 'Guest', 'GUEST', 'Guest role with minimal access', 'DEFAULT', 0, 'System');

-- ============================================
-- 6. Seed Permissions
-- ============================================
INSERT INTO Auth.Permissions (Code, Name, Description, PermissionType, ResourceType, TenantId, IsActive, CreatedBy)
VALUES
    ('PRODUCT_CREATE', 'Create Product', 'Permission to create products', 'Create', 'Product', 'DEFAULT', 1, 'System'),
    ('PRODUCT_READ', 'Read Product', 'Permission to read products', 'Read', 'Product', 'DEFAULT', 1, 'System'),
    ('PRODUCT_UPDATE', 'Update Product', 'Permission to update products', 'Update', 'Product', 'DEFAULT', 1, 'System'),
    ('PRODUCT_DELETE', 'Delete Product', 'Permission to delete products', 'Delete', 'Product', 'DEFAULT', 1, 'System'),
    ('ORDER_CREATE', 'Create Order', 'Permission to create orders', 'Create', 'Order', 'DEFAULT', 1, 'System'),
    ('ORDER_READ', 'Read Order', 'Permission to read orders', 'Read', 'Order', 'DEFAULT', 1, 'System'),
    ('ORDER_UPDATE', 'Update Order', 'Permission to update orders', 'Update', 'Order', 'DEFAULT', 1, 'System'),
    ('ORDER_DELETE', 'Delete Order', 'Permission to delete orders', 'Delete', 'Order', 'DEFAULT', 1, 'System'),
    ('REPORT_READ', 'View Report', 'Permission to view reports', 'Read', 'Report', 'DEFAULT', 1, 'System'),
    ('USER_UPDATE', 'Manage Users', 'Permission to manage users', 'Update', 'User', 'DEFAULT', 1, 'System'),
    ('MENU_READ', 'View Menu', 'Permission to view menus', 'Read', 'Menu', 'DEFAULT', 1, 'System'),
    ('MENU_UPDATE', 'Manage Menu', 'Permission to manage menus', 'Update', 'Menu', 'DEFAULT', 1, 'System');

-- ============================================
-- 7. Seed Menus (Navigation Structure)
-- ============================================
INSERT INTO Master.Menus (Name, Description, MenuType, DisplayOrder, TenantId, IsActive, CreatedBy)
VALUES
    ('Main Menu', 'Main navigation menu for public portal', 'Main', 1, 'DEFAULT', 1, 'System'),
    ('Footer Menu', 'Footer navigation for public portal', 'Footer', 2, 'DEFAULT', 1, 'System'),
    ('Admin Menu', 'Admin dashboard navigation', 'Admin', 3, 'DEFAULT', 1, 'System');

-- ============================================
-- 8. Seed Menu Items (with HierarchyID)
-- ============================================
-- Main Menu Items
DECLARE @MenuId INT = (SELECT MenuId FROM Master.Menus WHERE Name = 'Main Menu' AND TenantId = 'DEFAULT');

INSERT INTO Master.MenuItems (MenuId, Title, URL, Icon, DisplayOrder, NodePath, TenantId, IsActive, CreatedBy)
VALUES
    (@MenuId, 'Home', '/', 'fas fa-home', 1, '/1/', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Products', '/products', 'fas fa-box', 2, '/2/', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Categories', '/categories', 'fas fa-th', 3, '/3/', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Blog', '/blog', 'fas fa-blog', 4, '/4/', 'DEFAULT', 1, 'System'),
    (@MenuId, 'About Us', '/about', 'fas fa-info-circle', 5, '/5/', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Contact', '/contact', 'fas fa-envelope', 6, '/6/', 'DEFAULT', 1, 'System');

-- Footer Menu Items
SET @MenuId = (SELECT MenuId FROM Master.Menus WHERE Name = 'Footer Menu' AND TenantId = 'DEFAULT');

INSERT INTO Master.MenuItems (MenuId, Title, URL, Icon, DisplayOrder, NodePath, TenantId, IsActive, CreatedBy)
VALUES
    (@MenuId, 'Terms of Service', '/terms', NULL, 1, '/1/', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Privacy Policy', '/privacy', NULL, 2, '/2/', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Cookie Policy', '/cookies', NULL, 3, '/3/', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Refund Policy', '/refund', NULL, 4, '/4/', 'DEFAULT', 1, 'System');

-- Admin Menu Items
SET @MenuId = (SELECT MenuId FROM Master.Menus WHERE Name = 'Admin Menu' AND TenantId = 'DEFAULT');

INSERT INTO Master.MenuItems (MenuId, Title, URL, Icon, DisplayOrder, NodePath, RequiredRole, TenantId, IsActive, CreatedBy)
VALUES
    (@MenuId, 'Dashboard', '/admin/dashboard', 'fas fa-tachometer-alt', 1, '/1/', 'Admin', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Products', '/admin/products', 'fas fa-box', 2, '/2/', 'Admin', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Orders', '/admin/orders', 'fas fa-shopping-cart', 3, '/3/', 'Admin', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Customers', '/admin/customers', 'fas fa-users', 4, '/4/', 'Admin', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Reports', '/admin/reports', 'fas fa-chart-bar', 5, '/5/', 'Manager', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Settings', '/admin/settings', 'fas fa-cog', 6, '/6/', 'Admin', 'DEFAULT', 1, 'System'),
    (@MenuId, 'Users', '/admin/users', 'fas fa-user-tie', 7, '/7/', 'Super Admin', 'DEFAULT', 1, 'System');

-- ============================================
-- 9. Seed Categories
-- ============================================
INSERT INTO Master.Categories (Name, Slug, Description, NodePath, TenantId, IsActive, CreatedBy)
VALUES
    ('Electronics', 'electronics', 'Electronic products', '/1/', 'DEFAULT', 1, 'System'),
    ('Clothing', 'clothing', 'Clothing and apparel', '/2/', 'DEFAULT', 1, 'System'),
    ('Books', 'books', 'Books and publications', '/3/', 'DEFAULT', 1, 'System'),
    ('Home & Garden', 'home-garden', 'Home and garden products', '/4/', 'DEFAULT', 1, 'System'),
    ('Sports', 'sports', 'Sports and outdoor equipment', '/5/', 'DEFAULT', 1, 'System');

-- ============================================
-- 10. Seed TimeZones
-- ============================================
INSERT INTO Master.TimeZones (Identifier, DisplayName, StandardName, OffsetHours, TenantId, IsActive, CreatedBy)
VALUES
    ('UTC', 'Coordinated Universal Time', 'UTC', 0, 'DEFAULT', 1, 'System'),
    ('America/New_York', 'Eastern Time (US & Canada)', 'EST', -5, 'DEFAULT', 1, 'System'),
    ('America/Chicago', 'Central Time (US & Canada)', 'CST', -6, 'DEFAULT', 1, 'System'),
    ('America/Denver', 'Mountain Time (US & Canada)', 'MST', -7, 'DEFAULT', 1, 'System'),
    ('America/Los_Angeles', 'Pacific Time (US & Canada)', 'PST', -8, 'DEFAULT', 1, 'System'),
    ('Europe/London', 'London', 'GMT', 0, 'DEFAULT', 1, 'System'),
    ('Europe/Paris', 'Central European Time', 'CET', 1, 'DEFAULT', 1, 'System'),
    ('Asia/Tokyo', 'Japan Standard Time', 'JST', 9, 'DEFAULT', 1, 'System'),
    ('Australia/Sydney', 'Australian Eastern Time', 'AEST', 10, 'DEFAULT', 1, 'System');

-- ============================================
-- 11. Seed Configuration
-- ============================================
INSERT INTO Master.Configuration ([Key], Value, ConfigType, Description, TenantId, IsActive, CreatedBy)
VALUES
    ('AppName', 'SmartWorkz', 'String', 'Application Name', 'DEFAULT', 1, 'System'),
    ('AppVersion', '4.0.0', 'String', 'Application Version', 'DEFAULT', 1, 'System'),
    ('MaintenanceMode', 'false', 'Bool', 'Enable maintenance mode', 'DEFAULT', 1, 'System'),
    ('MaxLoginAttempts', '5', 'Int', 'Maximum failed login attempts', 'DEFAULT', 1, 'System'),
    ('SessionTimeoutMinutes', '30', 'Int', 'User session timeout in minutes', 'DEFAULT', 1, 'System'),
    ('ItemsPerPage', '20', 'Int', 'Default items per page in pagination', 'DEFAULT', 1, 'System'),
    ('EmailSmtpHost', 'smtp.gmail.com', 'String', 'SMTP server host', 'DEFAULT', 1, 'System'),
    ('EmailSmtpPort', '587', 'Int', 'SMTP server port', 'DEFAULT', 1, 'System');

-- ============================================
-- 12. Seed Feature Flags
-- ============================================
INSERT INTO Master.FeatureFlags (Name, Description, IsEnabled, TenantId, CreatedBy)
VALUES
    ('EnableTwoFactorAuth', 'Enable two-factor authentication', 1, 'DEFAULT', 'System'),
    ('EnableUserRegistration', 'Enable public user registration', 1, 'DEFAULT', 'System'),
    ('EnableProductReviews', 'Enable product reviews feature', 1, 'DEFAULT', 'System'),
    ('EnableWishlist', 'Enable wishlist feature', 1, 'DEFAULT', 'System'),
    ('EnableGuestCheckout', 'Enable guest checkout', 0, 'DEFAULT', 'System'),
    ('EnableBlogSection', 'Enable blog section', 1, 'DEFAULT', 'System');

PRINT '✓ Seed data inserted successfully'
PRINT '✓ Tenants: 2 (DEFAULT, DEMO)'
PRINT '✓ Languages: 8'
PRINT '✓ Countries: 12'
PRINT '✓ Currencies: 9'
PRINT '✓ Roles: 6'
PRINT '✓ Permissions: 12'
PRINT '✓ Menus: 3'
PRINT '✓ Menu Items: 13'
PRINT '✓ Categories: 5'
PRINT '✓ TimeZones: 9'
PRINT '✓ Configuration: 8'
PRINT '✓ Feature Flags: 6'

