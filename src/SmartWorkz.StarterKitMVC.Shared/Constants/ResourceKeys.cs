namespace SmartWorkz.StarterKitMVC.Shared.Constants;

/// <summary>
/// Resource keys for localization
/// </summary>
public static class ResourceKeys
{
    /// <summary>
    /// Common labels used throughout the application
    /// </summary>
    public static class Labels
    {
        public const string Name = "common.labels.name";
        public const string Description = "common.labels.description";
        public const string Email = "common.labels.email";
        public const string Phone = "common.labels.phone";
        public const string Address = "common.labels.address";
        public const string Status = "common.labels.status";
        public const string Type = "common.labels.type";
        public const string Category = "common.labels.category";
        public const string Date = "common.labels.date";
        public const string Time = "common.labels.time";
        public const string DateTime = "common.labels.datetime";
        public const string CreatedAt = "common.labels.created_at";
        public const string UpdatedAt = "common.labels.updated_at";
        public const string CreatedBy = "common.labels.created_by";
        public const string UpdatedBy = "common.labels.updated_by";
        public const string Active = "common.labels.active";
        public const string Inactive = "common.labels.inactive";
        public const string Yes = "common.labels.yes";
        public const string No = "common.labels.no";
        public const string All = "common.labels.all";
        public const string None = "common.labels.none";
        public const string Select = "common.labels.select";
        public const string Search = "common.labels.search";
        public const string Filter = "common.labels.filter";
        public const string Sort = "common.labels.sort";
        public const string Actions = "common.labels.actions";
        public const string Details = "common.labels.details";
        public const string Settings = "common.labels.settings";
        public const string Options = "common.labels.options";
        public const string Total = "common.labels.total";
        public const string Count = "common.labels.count";
        public const string Size = "common.labels.size";
        public const string Version = "common.labels.version";
        public const string Language = "common.labels.language";
        public const string Key = "common.labels.key";
        public const string Value = "common.labels.value";
        public const string Icon = "common.labels.icon";
        public const string SortOrder = "common.labels.sort_order";
        public const string Required = "common.labels.required";
        public const string Optional = "common.labels.optional";
    }
    
    /// <summary>
    /// Button labels
    /// </summary>
    public static class Buttons
    {
        public const string Save = "common.buttons.save";
        public const string Cancel = "common.buttons.cancel";
        public const string Delete = "common.buttons.delete";
        public const string Edit = "common.buttons.edit";
        public const string Create = "common.buttons.create";
        public const string Add = "common.buttons.add";
        public const string Remove = "common.buttons.remove";
        public const string Update = "common.buttons.update";
        public const string Submit = "common.buttons.submit";
        public const string Reset = "common.buttons.reset";
        public const string Clear = "common.buttons.clear";
        public const string Close = "common.buttons.close";
        public const string Back = "common.buttons.back";
        public const string Next = "common.buttons.next";
        public const string Previous = "common.buttons.previous";
        public const string Finish = "common.buttons.finish";
        public const string Confirm = "common.buttons.confirm";
        public const string Apply = "common.buttons.apply";
        public const string Refresh = "common.buttons.refresh";
        public const string Export = "common.buttons.export";
        public const string Import = "common.buttons.import";
        public const string Download = "common.buttons.download";
        public const string Upload = "common.buttons.upload";
        public const string View = "common.buttons.view";
        public const string ViewAll = "common.buttons.view_all";
        public const string ViewDetails = "common.buttons.view_details";
        public const string Manage = "common.buttons.manage";
        public const string Configure = "common.buttons.configure";
        public const string Enable = "common.buttons.enable";
        public const string Disable = "common.buttons.disable";
        public const string Activate = "common.buttons.activate";
        public const string Deactivate = "common.buttons.deactivate";
        public const string Login = "common.buttons.login";
        public const string Logout = "common.buttons.logout";
        public const string Register = "common.buttons.register";
        public const string ForgotPassword = "common.buttons.forgot_password";
        public const string ChangePassword = "common.buttons.change_password";
        public const string SaveChanges = "common.buttons.save_changes";
        public const string ToggleAll = "common.buttons.toggle_all";
        public const string SelectAll = "common.buttons.select_all";
        public const string DeselectAll = "common.buttons.deselect_all";
    }
    
    /// <summary>
    /// Navigation menu items
    /// </summary>
    public static class Navigation
    {
        // Main sections
        public const string Main = "nav.sections.main";
        public const string Management = "nav.sections.management";
        public const string System = "nav.sections.system";
        public const string Help = "nav.sections.help";
        
        // Menu items
        public const string Dashboard = "nav.menu.dashboard";
        public const string Calendar = "nav.menu.calendar";
        public const string Identity = "nav.menu.identity";
        public const string Users = "nav.menu.users";
        public const string Roles = "nav.menu.roles";
        public const string Claims = "nav.menu.claims";
        public const string Permissions = "nav.menu.permissions";
        public const string Features = "nav.menu.features";
        public const string RolePermissions = "nav.menu.role_permissions";
        public const string Tenants = "nav.menu.tenants";
        public const string ListOfValues = "nav.menu.list_of_values";
        public const string Settings = "nav.menu.settings";
        public const string Notifications = "nav.menu.notifications";
        public const string Theme = "nav.menu.theme";
        public const string Localization = "nav.menu.localization";
        public const string Languages = "nav.menu.languages";
        public const string Resources = "nav.menu.resources";
        public const string Translations = "nav.menu.translations";
        public const string EmailTemplates = "nav.menu.email_templates";
        public const string Documentation = "nav.menu.documentation";
        public const string Overview = "nav.menu.overview";
        public const string GettingStarted = "nav.menu.getting_started";
        public const string Configuration = "nav.menu.configuration";
        public const string Architecture = "nav.menu.architecture";
        public const string Database = "nav.menu.database";
        public const string Security = "nav.menu.security";
        public const string ApiReference = "nav.menu.api_reference";
        public const string BackToSite = "nav.menu.back_to_site";
    }
    
    /// <summary>
    /// Page titles
    /// </summary>
    public static class Titles
    {
        public const string Dashboard = "titles.dashboard";
        public const string Users = "titles.users";
        public const string UserDetails = "titles.user_details";
        public const string CreateUser = "titles.create_user";
        public const string EditUser = "titles.edit_user";
        public const string Roles = "titles.roles";
        public const string RoleDetails = "titles.role_details";
        public const string CreateRole = "titles.create_role";
        public const string EditRole = "titles.edit_role";
        public const string Claims = "titles.claims";
        public const string ClaimTypes = "titles.claim_types";
        public const string CreateClaimType = "titles.create_claim_type";
        public const string EditClaimType = "titles.edit_claim_type";
        public const string RoleClaims = "titles.role_claims";
        public const string Permissions = "titles.permissions";
        public const string Features = "titles.features";
        public const string CreateFeature = "titles.create_feature";
        public const string EditFeature = "titles.edit_feature";
        public const string RolePermissions = "titles.role_permissions";
        public const string Settings = "titles.settings";
        public const string GeneralSettings = "titles.general_settings";
        public const string SecuritySettings = "titles.security_settings";
        public const string EmailSettings = "titles.email_settings";
        public const string Tenants = "titles.tenants";
        public const string TenantDetails = "titles.tenant_details";
        public const string CreateTenant = "titles.create_tenant";
        public const string EditTenant = "titles.edit_tenant";
        public const string Notifications = "titles.notifications";
        public const string Localization = "titles.localization";
        public const string Languages = "titles.languages";
        public const string Resources = "titles.resources";
        public const string Translations = "titles.translations";
        public const string EmailTemplates = "titles.email_templates";
        public const string ListOfValues = "titles.list_of_values";
        public const string Profile = "titles.profile";
        public const string ChangePassword = "titles.change_password";
    }
    
    /// <summary>
    /// Success messages
    /// </summary>
    public static class Messages
    {
        public const string SaveSuccess = "messages.save_success";
        public const string CreateSuccess = "messages.create_success";
        public const string UpdateSuccess = "messages.update_success";
        public const string DeleteSuccess = "messages.delete_success";
        public const string ImportSuccess = "messages.import_success";
        public const string ExportSuccess = "messages.export_success";
        public const string OperationSuccess = "messages.operation_success";
        public const string ConfirmDelete = "messages.confirm_delete";
        public const string ConfirmAction = "messages.confirm_action";
        public const string NoRecordsFound = "messages.no_records_found";
        public const string Loading = "messages.loading";
        public const string Processing = "messages.processing";
        public const string PleaseWait = "messages.please_wait";
        public const string Welcome = "messages.welcome";
        public const string Goodbye = "messages.goodbye";
    }
    
    /// <summary>
    /// Error messages
    /// </summary>
    public static class Errors
    {
        public const string General = "errors.general";
        public const string NotFound = "errors.not_found";
        public const string Unauthorized = "errors.unauthorized";
        public const string Forbidden = "errors.forbidden";
        public const string BadRequest = "errors.bad_request";
        public const string ServerError = "errors.server_error";
        public const string ValidationFailed = "errors.validation_failed";
        public const string SaveFailed = "errors.save_failed";
        public const string DeleteFailed = "errors.delete_failed";
        public const string DuplicateEntry = "errors.duplicate_entry";
        public const string InvalidInput = "errors.invalid_input";
        public const string RequiredField = "errors.required_field";
        public const string InvalidEmail = "errors.invalid_email";
        public const string InvalidPassword = "errors.invalid_password";
        public const string PasswordMismatch = "errors.password_mismatch";
        public const string SessionExpired = "errors.session_expired";
        public const string NetworkError = "errors.network_error";
    }
    
    /// <summary>
    /// Validation messages
    /// </summary>
    public static class Validation
    {
        public const string Required = "validation.required";
        public const string MinLength = "validation.min_length";
        public const string MaxLength = "validation.max_length";
        public const string Email = "validation.email";
        public const string Phone = "validation.phone";
        public const string Url = "validation.url";
        public const string Number = "validation.number";
        public const string Integer = "validation.integer";
        public const string Decimal = "validation.decimal";
        public const string Date = "validation.date";
        public const string Range = "validation.range";
        public const string Regex = "validation.regex";
        public const string Compare = "validation.compare";
        public const string Unique = "validation.unique";
    }
    
    /// <summary>
    /// Admin-specific labels
    /// </summary>
    public static class Admin
    {
        public const string AdminPanel = "admin.admin_panel";
        public const string WelcomeAdmin = "admin.welcome_admin";
        public const string TotalUsers = "admin.total_users";
        public const string TotalRoles = "admin.total_roles";
        public const string TotalTenants = "admin.total_tenants";
        public const string ActiveUsers = "admin.active_users";
        public const string PendingApprovals = "admin.pending_approvals";
        public const string SystemHealth = "admin.system_health";
        public const string RecentActivity = "admin.recent_activity";
        public const string QuickActions = "admin.quick_actions";
        public const string SystemInfo = "admin.system_info";
        public const string AllRightsReserved = "admin.all_rights_reserved";
    }
    
    /// <summary>
    /// Identity-related labels
    /// </summary>
    public static class IdentityLabels
    {
        public const string Username = "identity.username";
        public const string Password = "identity.password";
        public const string ConfirmPassword = "identity.confirm_password";
        public const string CurrentPassword = "identity.current_password";
        public const string NewPassword = "identity.new_password";
        public const string FirstName = "identity.first_name";
        public const string LastName = "identity.last_name";
        public const string FullName = "identity.full_name";
        public const string DisplayName = "identity.display_name";
        public const string EmailAddress = "identity.email_address";
        public const string PhoneNumber = "identity.phone_number";
        public const string Role = "identity.role";
        public const string Roles = "identity.roles";
        public const string Claim = "identity.claim";
        public const string Claims = "identity.claims";
        public const string Permission = "identity.permission";
        public const string Permissions = "identity.permissions";
        public const string LastLogin = "identity.last_login";
        public const string AccountLocked = "identity.account_locked";
        public const string EmailConfirmed = "identity.email_confirmed";
        public const string TwoFactorEnabled = "identity.two_factor_enabled";
    }
    
    /// <summary>
    /// Placeholders for input fields
    /// </summary>
    public static class Placeholders
    {
        public const string EnterName = "placeholders.enter_name";
        public const string EnterEmail = "placeholders.enter_email";
        public const string EnterPassword = "placeholders.enter_password";
        public const string EnterSearch = "placeholders.enter_search";
        public const string SelectOption = "placeholders.select_option";
        public const string TypeToSearch = "placeholders.type_to_search";
        public const string EnterDescription = "placeholders.enter_description";
        public const string EnterKey = "placeholders.enter_key";
        public const string EnterValue = "placeholders.enter_value";
    }
}
