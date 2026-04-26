# Authentication API Reference

## Classes & Interfaces

### AuthorizationMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.AuthorizationMiddleware`
- **Summary:** Comprehensive authorization middleware that validates:
             1. Roles - standard ASP.NET Core roles
             2. Claims - custom claim types and values
             3. Permissions - entity-level CRUD permissions
            
             Validates that users have the required authorization to access resources
             and enriches the principal with permission claims from the database.

#### Methods & Properties

- **ValidateAndEnrichAuthorizationAsync** - Validates user authorization and enriches principal with claims/permissions
- **ValidateRoles** - Validates that roles are properly formatted and recognized
- **ValidateAndEnrichClaimsAsync** - Validates and enriches claims from the claim service
- **HasAccessToResource** - Validates that a user has access to a resource
- **GetClaimValues** - Gets all claims of a specific type from the user
- **GetUserPermissions** - Gets all permissions the user has
- **GetUserRoles** - Gets all roles the user has
- **LogAuthorizationInfo** - Helper to log authorization information

### AuthorizationMiddlewareExtensions

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.AuthorizationMiddlewareExtensions`
- **Summary:** Extension methods for the authorization middleware

### AuthenticationController

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Controllers.AuthenticationController`
- **Summary:** Authentication controller for user login, registration, and password management

#### Methods & Properties

- **Login** - Login form page
- **Login** - Process login request
- **Register** - Registration form page
- **Register** - Process registration request
- **Logout** - Logout action
- **ForgotPassword** - Forgot password form page
- **ForgotPassword** - Process forgot password request
- **ResetPassword** - Reset password form page
- **ResetPassword** - Process password reset
- **AccessDenied** - Access denied page

### AuthenticationController

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Controllers.AuthenticationController`
- **Summary:** Authentication controller for user login, registration, and password management

#### Methods & Properties

- **Login** - Login form page
- **Login** - Process login request
- **Register** - Registration form page
- **Register** - Process registration request
- **Logout** - Logout action
- **ForgotPassword** - Forgot password form page
- **ForgotPassword** - Process forgot password request
- **ResetPassword** - Reset password form page
- **ResetPassword** - Process password reset
- **AccessDenied** - Access denied page

### AuthorizationMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.AuthorizationMiddleware`
- **Summary:** Comprehensive authorization middleware that validates:
             1. Roles - standard ASP.NET Core roles
             2. Claims - custom claim types and values
             3. Permissions - entity-level CRUD permissions
            
             Validates that users have the required authorization to access resources
             and enriches the principal with permission claims from the database.

#### Methods & Properties

- **ValidateAndEnrichAuthorizationAsync** - Validates user authorization and enriches principal with claims/permissions
- **ValidateRoles** - Validates that roles are properly formatted and recognized
- **ValidateAndEnrichClaimsAsync** - Validates and enriches claims from the claim service
- **HasAccessToResource** - Validates that a user has access to a resource
- **GetClaimValues** - Gets all claims of a specific type from the user
- **GetUserPermissions** - Gets all permissions the user has
- **GetUserRoles** - Gets all roles the user has
- **LogAuthorizationInfo** - Helper to log authorization information

### AuthorizationMiddlewareExtensions

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.AuthorizationMiddlewareExtensions`
- **Summary:** Extension methods for the authorization middleware

### AuthorizationMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.AuthorizationMiddleware`
- **Summary:** Comprehensive authorization middleware that validates:
             1. Roles - standard ASP.NET Core roles
             2. Claims - custom claim types and values
             3. Permissions - entity-level CRUD permissions
            
             Validates that users have the required authorization to access resources
             and enriches the principal with permission claims from the database.

#### Methods & Properties

- **ValidateAndEnrichAuthorizationAsync** - Validates user authorization and enriches principal with claims/permissions
- **ValidateRoles** - Validates that roles are properly formatted and recognized
- **ValidateAndEnrichClaimsAsync** - Validates and enriches claims from the claim service
- **HasAccessToResource** - Validates that a user has access to a resource
- **GetClaimValues** - Gets all claims of a specific type from the user
- **GetUserPermissions** - Gets all permissions the user has
- **GetUserRoles** - Gets all roles the user has
- **LogAuthorizationInfo** - Helper to log authorization information

### AuthorizationMiddlewareExtensions

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.AuthorizationMiddlewareExtensions`
- **Summary:** Extension methods for the authorization middleware

### AuthenticationController

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Controllers.AuthenticationController`
- **Summary:** Authentication controller for user login, registration, and password management

#### Methods & Properties

- **Login** - Login form page
- **Login** - Process login request
- **Register** - Registration form page
- **Register** - Process registration request
- **Logout** - Logout action
- **ForgotPassword** - Forgot password form page
- **ForgotPassword** - Process forgot password request
- **ResetPassword** - Reset password form page
- **ResetPassword** - Process password reset
- **AccessDenied** - Access denied page

