namespace SmartWorkz.StarterKitMVC.Shared.Constants;

/// <summary>
/// All user-facing message keys. Never use literal strings in code.
/// Keys are resolved at runtime via ITranslationService (DB-backed, per-tenant).
/// Format: {domain}.{action_or_state}
/// </summary>
public static class MessageKeys
{
    public static class Auth
    {
        public const string InvalidCredentials      = "auth.invalid_credentials";
        public const string AccountLocked           = "auth.account_locked";
        public const string AccountInactive         = "auth.account_inactive";
        public const string EmailNotConfirmed       = "auth.email_not_confirmed";
        public const string EmailAlreadyRegistered  = "auth.email_already_registered";
        public const string LoginSuccess            = "auth.login_success";
        public const string LogoutSuccess           = "auth.logout_success";
        public const string RegisterSuccess         = "auth.register_success";
        public const string PasswordResetSent       = "auth.password_reset_sent";
        public const string PasswordResetSuccess    = "auth.password_reset_success";
        public const string PasswordResetInvalid    = "auth.password_reset_invalid";
        public const string PasswordChanged         = "auth.password_changed";
        public const string PasswordMismatch        = "auth.password_mismatch";
        public const string EmailVerified           = "auth.email_verified";
        public const string EmailVerifyInvalid      = "auth.email_verify_invalid";
        public const string Unauthorized            = "auth.unauthorized";
        public const string AccessDenied            = "auth.access_denied";
        public const string LoginError              = "auth.login_error";
    }

    public static class Validation
    {
        public const string Required                = "validation.required";
        public const string EmailInvalid            = "validation.email_invalid";
        public const string MinLength               = "validation.min_length";
        public const string MaxLength               = "validation.max_length";
        public const string PasswordTooWeak         = "validation.password_too_weak";
        public const string InvalidFormat           = "validation.invalid_format";
        public const string MustBeUnique            = "validation.must_be_unique";
        public const string NotFound                = "validation.not_found";
    }

    public static class Crud
    {
        public const string SaveSuccess             = "crud.save_success";
        public const string SaveError               = "crud.save_error";
        public const string DeleteSuccess           = "crud.delete_success";
        public const string DeleteError             = "crud.delete_error";
        public const string DeleteConfirm           = "crud.delete_confirm";
        public const string NotFound                = "crud.not_found";
        public const string NoChanges               = "crud.no_changes";
        public const string ConcurrencyConflict     = "crud.concurrency_conflict";
    }

    public static class User
    {
        public const string ProfileUpdated          = "user.profile_updated";
        public const string ProfileUpdateError      = "user.profile_update_error";
        public const string UserNotFound            = "user.not_found";
        public const string RoleAssigned            = "user.role_assigned";
        public const string RoleRemoved             = "user.role_removed";
        public const string PermissionGranted       = "user.permission_granted";
        public const string PermissionRevoked       = "user.permission_revoked";
    }

    public static class Tenant
    {
        public const string NotFound                = "tenant.not_found";
        public const string CreateSuccess           = "tenant.create_success";
        public const string UpdateSuccess           = "tenant.update_success";
        public const string DeleteSuccess           = "tenant.delete_success";
    }

    public static class General
    {
        public const string UnexpectedError         = "general.unexpected_error";
        public const string ServiceUnavailable      = "general.service_unavailable";
        public const string Loading                 = "general.loading";
        public const string NoResults               = "general.no_results";
        public const string Confirm                 = "general.confirm";
        public const string Cancel                  = "general.cancel";
        public const string Save                    = "general.save";
        public const string Delete                  = "general.delete";
        public const string Edit                    = "general.edit";
        public const string Create                  = "general.create";
        public const string Search                  = "general.search";
        public const string Back                    = "general.back";
    }

    public static class Template
    {
        public const string NotFound                = "template.not_found";
        public const string Inactive                = "template.inactive";
        public const string RenderFailed            = "template.render_failed";
        public const string PlaceholderMissing      = "template.placeholder_missing";
        public const string SaveSuccess             = "template.save_success";
        public const string SaveError               = "template.save_error";
        public const string DeleteSuccess           = "template.delete_success";
        public const string SystemCannotDelete      = "template.system_cannot_delete";
    }

    public static class EmailQueue
    {
        public const string EnqueueSuccess          = "emailqueue.enqueue_success";
        public const string EnqueueError            = "emailqueue.enqueue_error";
        public const string SendSuccess             = "emailqueue.send_success";
        public const string SendError               = "emailqueue.send_error";
    }

    public static class Cache
    {
        public const string ServiceUnavailable      = "cache.service_unavailable";
    }
}
