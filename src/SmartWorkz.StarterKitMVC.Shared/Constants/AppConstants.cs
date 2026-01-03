namespace SmartWorkz.StarterKitMVC.Shared.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    public const string AppName = "StarterKitMVC";
    public const string AppTitle = "SmartWorkz StarterKitMVC";
    public const string Version = "1.0.0";
    
    /// <summary>
    /// Default roles in the system
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";
        public const string Guest = "Guest";
        
        public static readonly string[] All = [Admin, Manager, User, Guest];
    }
    
    /// <summary>
    /// Claim types used in the system
    /// </summary>
    public static class ClaimTypes
    {
        public const string Permission = "permission";
        public const string TenantId = "tenant_id";
        public const string Department = "department";
        public const string EmployeeId = "employee_id";
    }
    
    /// <summary>
    /// Entity names for permissions and claims
    /// </summary>
    public static class Entities
    {
        public const string Users = "users";
        public const string Roles = "roles";
        public const string Claims = "claims";
        public const string Permissions = "permissions";
        public const string Settings = "settings";
        public const string Tenants = "tenants";
        public const string Notifications = "notifications";
        public const string Resources = "resources";
        public const string EmailTemplates = "emailtemplates";
        public const string Lov = "lov";
        public const string Categories = "categories";
        public const string Audit = "audit";
        public const string Dashboard = "dashboard";
        public const string Reports = "reports";
        
        public static readonly string[] All = 
        [
            Users, Roles, Claims, Permissions, Settings, Tenants,
            Notifications, Resources, EmailTemplates, Lov, 
            Categories, Audit, Dashboard, Reports
        ];
    }
    
    /// <summary>
    /// Standard CRUD actions
    /// </summary>
    public static class Actions
    {
        public const string View = "view";
        public const string Create = "create";
        public const string Edit = "edit";
        public const string Delete = "delete";
        public const string Export = "export";
        public const string Import = "import";
        public const string Manage = "manage";
        
        public static readonly string[] Crud = [View, Create, Edit, Delete];
        public static readonly string[] All = [View, Create, Edit, Delete, Export, Import, Manage];
    }
    
    /// <summary>
    /// Claim categories
    /// </summary>
    public static class ClaimCategories
    {
        public const string Identity = "Identity";
        public const string System = "System";
        public const string Content = "Content";
        public const string Custom = "Custom";
    }
    
    /// <summary>
    /// Default language codes
    /// </summary>
    public static class Languages
    {
        public const string English = "en";
        public const string Spanish = "es";
        public const string French = "fr";
        public const string German = "de";
        public const string Arabic = "ar";
        public const string Chinese = "zh";
        public const string Japanese = "ja";
        public const string Hindi = "hi";
    }
    
    /// <summary>
    /// Resource categories
    /// </summary>
    public static class ResourceCategories
    {
        public const string Labels = "Labels";
        public const string Buttons = "Buttons";
        public const string Messages = "Messages";
        public const string Errors = "Errors";
        public const string Validation = "Validation";
        public const string Navigation = "Navigation";
        public const string Titles = "Titles";
        public const string Placeholders = "Placeholders";
    }
    
    /// <summary>
    /// Resource modules
    /// </summary>
    public static class ResourceModules
    {
        public const string Common = "Common";
        public const string Admin = "Admin";
        public const string Identity = "Identity";
        public const string Settings = "Settings";
        public const string Notifications = "Notifications";
        public const string Reports = "Reports";
    }
}
