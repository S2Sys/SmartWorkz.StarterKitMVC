-- ============================================
-- SmartWorkz StarterKitMVC Seed Data
-- Version: 1.0.0
-- ============================================

-- ============================================
-- DEFAULT TENANT
-- ============================================
INSERT INTO Tenants (Id, Name, Subdomain, IsActive, CreatedAt, CreatedBy)
VALUES ('default', 'Default Tenant', 'default', 1, GETUTCDATE(), 'System');

INSERT INTO TenantBranding (Id, TenantId, PrimaryColor, SecondaryColor, AccentColor, FooterText)
VALUES (NEWID(), 'default', '#0d6efd', '#6c757d', '#198754', '© 2024 SmartWorkz StarterKitMVC');

-- ============================================
-- DEFAULT ROLES
-- ============================================
DECLARE @AdminRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @UserRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @ManagerRoleId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Roles (Id, Name, NormalizedName, Description, IsSystemRole, CreatedAt, CreatedBy)
VALUES 
    (@AdminRoleId, 'Administrator', 'ADMINISTRATOR', 'Full system access', 1, GETUTCDATE(), 'System'),
    (@UserRoleId, 'User', 'USER', 'Standard user access', 1, GETUTCDATE(), 'System'),
    (@ManagerRoleId, 'Manager', 'MANAGER', 'Management access', 1, GETUTCDATE(), 'System');

-- ============================================
-- DEFAULT PERMISSIONS
-- ============================================
INSERT INTO Permissions (Id, Name, DisplayName, Description, Category, IsSystemPermission, CreatedAt)
VALUES
    -- User Management
    (NEWID(), 'users.view', 'View Users', 'Can view user list', 'Users', 1, GETUTCDATE()),
    (NEWID(), 'users.create', 'Create Users', 'Can create new users', 'Users', 1, GETUTCDATE()),
    (NEWID(), 'users.edit', 'Edit Users', 'Can edit existing users', 'Users', 1, GETUTCDATE()),
    (NEWID(), 'users.delete', 'Delete Users', 'Can delete users', 'Users', 1, GETUTCDATE()),
    -- Role Management
    (NEWID(), 'roles.view', 'View Roles', 'Can view role list', 'Roles', 1, GETUTCDATE()),
    (NEWID(), 'roles.create', 'Create Roles', 'Can create new roles', 'Roles', 1, GETUTCDATE()),
    (NEWID(), 'roles.edit', 'Edit Roles', 'Can edit existing roles', 'Roles', 1, GETUTCDATE()),
    (NEWID(), 'roles.delete', 'Delete Roles', 'Can delete roles', 'Roles', 1, GETUTCDATE()),
    -- Tenant Management
    (NEWID(), 'tenants.view', 'View Tenants', 'Can view tenant list', 'Tenants', 1, GETUTCDATE()),
    (NEWID(), 'tenants.create', 'Create Tenants', 'Can create new tenants', 'Tenants', 1, GETUTCDATE()),
    (NEWID(), 'tenants.edit', 'Edit Tenants', 'Can edit existing tenants', 'Tenants', 1, GETUTCDATE()),
    (NEWID(), 'tenants.delete', 'Delete Tenants', 'Can delete tenants', 'Tenants', 1, GETUTCDATE()),
    -- Settings Management
    (NEWID(), 'settings.view', 'View Settings', 'Can view settings', 'Settings', 1, GETUTCDATE()),
    (NEWID(), 'settings.edit', 'Edit Settings', 'Can edit settings', 'Settings', 1, GETUTCDATE()),
    -- LOV Management
    (NEWID(), 'lov.view', 'View LOV', 'Can view list of values', 'LOV', 1, GETUTCDATE()),
    (NEWID(), 'lov.create', 'Create LOV', 'Can create list of values', 'LOV', 1, GETUTCDATE()),
    (NEWID(), 'lov.edit', 'Edit LOV', 'Can edit list of values', 'LOV', 1, GETUTCDATE()),
    (NEWID(), 'lov.delete', 'Delete LOV', 'Can delete list of values', 'LOV', 1, GETUTCDATE()),
    -- Notifications
    (NEWID(), 'notifications.view', 'View Notifications', 'Can view notifications', 'Notifications', 1, GETUTCDATE()),
    (NEWID(), 'notifications.manage', 'Manage Notifications', 'Can manage notification templates', 'Notifications', 1, GETUTCDATE());

-- ============================================
-- DEFAULT ADMIN USER
-- ============================================
DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Users (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, DisplayName, TenantId, IsActive, CreatedAt, CreatedBy)
VALUES (@AdminUserId, 'admin', 'ADMIN', 'admin@example.com', 'ADMIN@EXAMPLE.COM', 1, 'System Administrator', 'default', 1, GETUTCDATE(), 'System');

INSERT INTO UserRoles (UserId, RoleId)
VALUES (@AdminUserId, @AdminRoleId);

-- ============================================
-- SETTING CATEGORIES
-- ============================================
DECLARE @GeneralCatId UNIQUEIDENTIFIER = NEWID();
DECLARE @SecurityCatId UNIQUEIDENTIFIER = NEWID();
DECLARE @EmailCatId UNIQUEIDENTIFIER = NEWID();
DECLARE @AppearanceCatId UNIQUEIDENTIFIER = NEWID();

INSERT INTO SettingCategories (Id, [Key], Name, Description, Icon, SortOrder, IsSystem, CreatedAt)
VALUES
    (@GeneralCatId, 'general', 'General', 'General application settings', 'bi-gear', 1, 1, GETUTCDATE()),
    (@SecurityCatId, 'security', 'Security', 'Security and authentication settings', 'bi-shield-lock', 2, 1, GETUTCDATE()),
    (@EmailCatId, 'email', 'Email', 'Email and SMTP settings', 'bi-envelope', 3, 1, GETUTCDATE()),
    (@AppearanceCatId, 'appearance', 'Appearance', 'UI and theme settings', 'bi-palette', 4, 1, GETUTCDATE());

-- ============================================
-- SETTING DEFINITIONS
-- ============================================
INSERT INTO SettingDefinitions (Id, [Key], CategoryId, Name, Description, ValueType, DefaultValue, IsRequired, IsSystem, SortOrder, CreatedAt)
VALUES
    -- General Settings
    (NEWID(), 'app.name', @GeneralCatId, 'Application Name', 'The name of the application', 'String', 'SmartWorkz StarterKitMVC', 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'app.description', @GeneralCatId, 'Application Description', 'Brief description of the application', 'String', 'Enterprise ASP.NET Core MVC Boilerplate', 0, 1, 2, GETUTCDATE()),
    (NEWID(), 'app.timezone', @GeneralCatId, 'Default Timezone', 'Default timezone for the application', 'String', 'UTC', 1, 1, 3, GETUTCDATE()),
    (NEWID(), 'app.dateformat', @GeneralCatId, 'Date Format', 'Default date format', 'String', 'yyyy-MM-dd', 1, 1, 4, GETUTCDATE()),
    (NEWID(), 'app.maintenance', @GeneralCatId, 'Maintenance Mode', 'Enable maintenance mode', 'Bool', 'false', 0, 1, 5, GETUTCDATE()),
    -- Security Settings
    (NEWID(), 'security.passwordminlength', @SecurityCatId, 'Minimum Password Length', 'Minimum required password length', 'Int', '8', 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'security.requiredigit', @SecurityCatId, 'Require Digit', 'Password must contain a digit', 'Bool', 'true', 0, 1, 2, GETUTCDATE()),
    (NEWID(), 'security.requireuppercase', @SecurityCatId, 'Require Uppercase', 'Password must contain uppercase letter', 'Bool', 'true', 0, 1, 3, GETUTCDATE()),
    (NEWID(), 'security.lockoutmaxattempts', @SecurityCatId, 'Max Login Attempts', 'Maximum failed login attempts before lockout', 'Int', '5', 1, 1, 4, GETUTCDATE()),
    (NEWID(), 'security.lockoutminutes', @SecurityCatId, 'Lockout Duration', 'Account lockout duration in minutes', 'Int', '15', 1, 1, 5, GETUTCDATE()),
    (NEWID(), 'security.sessiontimeout', @SecurityCatId, 'Session Timeout', 'Session timeout in minutes', 'Int', '30', 1, 1, 6, GETUTCDATE()),
    -- Email Settings
    (NEWID(), 'email.smtphost', @EmailCatId, 'SMTP Host', 'SMTP server hostname', 'String', 'smtp.example.com', 0, 1, 1, GETUTCDATE()),
    (NEWID(), 'email.smtpport', @EmailCatId, 'SMTP Port', 'SMTP server port', 'Int', '587', 0, 1, 2, GETUTCDATE()),
    (NEWID(), 'email.smtpuser', @EmailCatId, 'SMTP Username', 'SMTP authentication username', 'String', '', 0, 1, 3, GETUTCDATE()),
    (NEWID(), 'email.smtppassword', @EmailCatId, 'SMTP Password', 'SMTP authentication password', 'EncryptedString', '', 0, 1, 4, GETUTCDATE()),
    (NEWID(), 'email.enablessl', @EmailCatId, 'Enable SSL', 'Use SSL for SMTP connection', 'Bool', 'true', 0, 1, 5, GETUTCDATE()),
    (NEWID(), 'email.fromaddress', @EmailCatId, 'From Address', 'Default sender email address', 'String', 'noreply@example.com', 0, 1, 6, GETUTCDATE()),
    (NEWID(), 'email.fromname', @EmailCatId, 'From Name', 'Default sender name', 'String', 'SmartWorkz StarterKitMVC', 0, 1, 7, GETUTCDATE()),
    -- Appearance Settings
    (NEWID(), 'appearance.theme', @AppearanceCatId, 'Default Theme', 'Default color theme', 'String', 'light', 0, 1, 1, GETUTCDATE()),
    (NEWID(), 'appearance.primarycolor', @AppearanceCatId, 'Primary Color', 'Primary brand color', 'String', '#0d6efd', 0, 1, 2, GETUTCDATE()),
    (NEWID(), 'appearance.logo', @AppearanceCatId, 'Logo URL', 'Application logo URL', 'String', '', 0, 1, 3, GETUTCDATE()),
    (NEWID(), 'appearance.favicon', @AppearanceCatId, 'Favicon URL', 'Browser favicon URL', 'String', '', 0, 1, 4, GETUTCDATE());

-- ============================================
-- LOV CATEGORIES
-- ============================================
DECLARE @CountriesCatId UNIQUEIDENTIFIER = NEWID();
DECLARE @StatusesCatId UNIQUEIDENTIFIER = NEWID();
DECLARE @PrioritiesCatId UNIQUEIDENTIFIER = NEWID();
DECLARE @GendersCatId UNIQUEIDENTIFIER = NEWID();

INSERT INTO LovCategories (Id, [Key], Name, Description, Icon, IsSystem, IsActive, CreatedAt, CreatedBy)
VALUES
    (@CountriesCatId, 'countries', 'Countries', 'List of countries', 'bi-globe', 1, 1, GETUTCDATE(), 'System'),
    (@StatusesCatId, 'statuses', 'Statuses', 'Common status values', 'bi-check-circle', 1, 1, GETUTCDATE(), 'System'),
    (@PrioritiesCatId, 'priorities', 'Priorities', 'Priority levels', 'bi-flag', 1, 1, GETUTCDATE(), 'System'),
    (@GendersCatId, 'genders', 'Genders', 'Gender options', 'bi-person', 1, 1, GETUTCDATE(), 'System');

-- ============================================
-- LOV ITEMS
-- ============================================
-- Countries
INSERT INTO LovItems (Id, CategoryId, [Key], DisplayName, SortOrder, IsActive, CreatedAt, CreatedBy)
VALUES
    (NEWID(), @CountriesCatId, 'US', 'United States', 1, 1, GETUTCDATE(), 'System'),
    (NEWID(), @CountriesCatId, 'GB', 'United Kingdom', 2, 1, GETUTCDATE(), 'System'),
    (NEWID(), @CountriesCatId, 'CA', 'Canada', 3, 1, GETUTCDATE(), 'System'),
    (NEWID(), @CountriesCatId, 'AU', 'Australia', 4, 1, GETUTCDATE(), 'System'),
    (NEWID(), @CountriesCatId, 'DE', 'Germany', 5, 1, GETUTCDATE(), 'System'),
    (NEWID(), @CountriesCatId, 'FR', 'France', 6, 1, GETUTCDATE(), 'System'),
    (NEWID(), @CountriesCatId, 'IN', 'India', 7, 1, GETUTCDATE(), 'System'),
    (NEWID(), @CountriesCatId, 'JP', 'Japan', 8, 1, GETUTCDATE(), 'System');

-- Statuses
INSERT INTO LovItems (Id, CategoryId, [Key], DisplayName, Color, SortOrder, IsActive, CreatedAt, CreatedBy)
VALUES
    (NEWID(), @StatusesCatId, 'active', 'Active', '#198754', 1, 1, GETUTCDATE(), 'System'),
    (NEWID(), @StatusesCatId, 'inactive', 'Inactive', '#6c757d', 2, 1, GETUTCDATE(), 'System'),
    (NEWID(), @StatusesCatId, 'pending', 'Pending', '#ffc107', 3, 1, GETUTCDATE(), 'System'),
    (NEWID(), @StatusesCatId, 'approved', 'Approved', '#0d6efd', 4, 1, GETUTCDATE(), 'System'),
    (NEWID(), @StatusesCatId, 'rejected', 'Rejected', '#dc3545', 5, 1, GETUTCDATE(), 'System'),
    (NEWID(), @StatusesCatId, 'archived', 'Archived', '#6c757d', 6, 1, GETUTCDATE(), 'System');

-- Priorities
INSERT INTO LovItems (Id, CategoryId, [Key], DisplayName, Color, Icon, SortOrder, IsActive, CreatedAt, CreatedBy)
VALUES
    (NEWID(), @PrioritiesCatId, 'low', 'Low', '#198754', 'bi-arrow-down', 1, 1, GETUTCDATE(), 'System'),
    (NEWID(), @PrioritiesCatId, 'medium', 'Medium', '#ffc107', 'bi-dash', 2, 1, GETUTCDATE(), 'System'),
    (NEWID(), @PrioritiesCatId, 'high', 'High', '#fd7e14', 'bi-arrow-up', 3, 1, GETUTCDATE(), 'System'),
    (NEWID(), @PrioritiesCatId, 'critical', 'Critical', '#dc3545', 'bi-exclamation-triangle', 4, 1, GETUTCDATE(), 'System');

-- Genders
INSERT INTO LovItems (Id, CategoryId, [Key], DisplayName, SortOrder, IsActive, CreatedAt, CreatedBy)
VALUES
    (NEWID(), @GendersCatId, 'male', 'Male', 1, 1, GETUTCDATE(), 'System'),
    (NEWID(), @GendersCatId, 'female', 'Female', 2, 1, GETUTCDATE(), 'System'),
    (NEWID(), @GendersCatId, 'other', 'Other', 3, 1, GETUTCDATE(), 'System'),
    (NEWID(), @GendersCatId, 'prefer_not_to_say', 'Prefer not to say', 4, 1, GETUTCDATE(), 'System');

-- ============================================
-- NOTIFICATION TEMPLATES
-- ============================================
INSERT INTO NotificationTemplates (Id, [Key], Name, Description, Subject, Body, BodyHtml, Channel, IsActive, CreatedAt, CreatedBy)
VALUES
    (NEWID(), 'welcome', 'Welcome Email', 'Sent to new users after registration', 
     'Welcome to {{AppName}}!', 
     'Hello {{UserName}},\n\nWelcome to {{AppName}}! Your account has been created successfully.\n\nBest regards,\nThe {{AppName}} Team',
     '<h1>Welcome to {{AppName}}!</h1><p>Hello {{UserName}},</p><p>Welcome to {{AppName}}! Your account has been created successfully.</p><p>Best regards,<br>The {{AppName}} Team</p>',
     'Email', 1, GETUTCDATE(), 'System'),
    
    (NEWID(), 'password_reset', 'Password Reset', 'Sent when user requests password reset',
     'Reset Your Password - {{AppName}}',
     'Hello {{UserName}},\n\nYou requested to reset your password. Click the link below to reset it:\n\n{{ResetLink}}\n\nThis link will expire in {{ExpiryHours}} hours.\n\nIf you did not request this, please ignore this email.',
     '<h1>Reset Your Password</h1><p>Hello {{UserName}},</p><p>You requested to reset your password. Click the button below to reset it:</p><p><a href="{{ResetLink}}" style="background:#0d6efd;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px;">Reset Password</a></p><p>This link will expire in {{ExpiryHours}} hours.</p><p>If you did not request this, please ignore this email.</p>',
     'Email', 1, GETUTCDATE(), 'System'),
    
    (NEWID(), 'account_locked', 'Account Locked', 'Sent when account is locked due to failed attempts',
     'Account Locked - {{AppName}}',
     'Hello {{UserName}},\n\nYour account has been locked due to multiple failed login attempts. Please contact support or wait {{LockoutMinutes}} minutes before trying again.',
     '<h1>Account Locked</h1><p>Hello {{UserName}},</p><p>Your account has been locked due to multiple failed login attempts.</p><p>Please contact support or wait {{LockoutMinutes}} minutes before trying again.</p>',
     'Email', 1, GETUTCDATE(), 'System'),
    
    (NEWID(), 'new_login', 'New Login Alert', 'Sent when user logs in from new device/location',
     'New Login Detected - {{AppName}}',
     'Hello {{UserName}},\n\nA new login to your account was detected:\n\nDevice: {{Device}}\nLocation: {{Location}}\nTime: {{LoginTime}}\n\nIf this was not you, please secure your account immediately.',
     '<h1>New Login Detected</h1><p>Hello {{UserName}},</p><p>A new login to your account was detected:</p><ul><li><strong>Device:</strong> {{Device}}</li><li><strong>Location:</strong> {{Location}}</li><li><strong>Time:</strong> {{LoginTime}}</li></ul><p>If this was not you, please secure your account immediately.</p>',
     'Email', 1, GETUTCDATE(), 'System');

GO
