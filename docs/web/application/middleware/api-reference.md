# Middleware API Reference

## Classes & Interfaces

### PermissionMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.PermissionMiddleware`
- **Summary:** Middleware that validates permissions based on claims.
            Adds permission claims to the user's identity based on their roles.
            Caches permissions per user to avoid redundant DB calls.

### PermissionMiddlewareExtensions

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.PermissionMiddlewareExtensions`
- **Summary:** Extension methods for permission middleware

### TenantMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.TenantMiddleware`
- **Summary:** Resolves TenantId once per request and stores it in HttpContext.Items["TenantId"].
            Resolution order:
              1. Authenticated user's "tenant" claim
              2. X-Tenant-ID request header
              3. Subdomain
              4. Falls back to "DEFAULT"

### PermissionMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Middleware.PermissionMiddleware`
- **Summary:** Middleware that validates permissions based on claims.
            Adds permission claims to the user's identity based on their roles.

### PermissionMiddlewareExtensions

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Middleware.PermissionMiddlewareExtensions`
- **Summary:** Extension methods for permission middleware

### TenantMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Middleware.TenantMiddleware`
- **Summary:** Resolves TenantId once per request and stores it in HttpContext.Items["TenantId"].
             Resolution order:
               1. Authenticated user's "tenant" claim
               2. X-Tenant-ID request header (API / integration scenarios)
               3. Subdomain (e.g. acme.smartworkz.com → "acme")
               4. Falls back to "DEFAULT"
            
             All page models read TenantId via BasePage.TenantId — never parse claims directly.

### PermissionMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Middleware.PermissionMiddleware`
- **Summary:** Middleware that validates permissions based on claims.
            Adds permission claims to the user's identity based on their roles.

### PermissionMiddlewareExtensions

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Middleware.PermissionMiddlewareExtensions`
- **Summary:** Extension methods for permission middleware

### TenantMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Middleware.TenantMiddleware`
- **Summary:** Resolves TenantId once per request and stores it in HttpContext.Items["TenantId"].
             Resolution order:
               1. Authenticated user's "tenant" claim
               2. X-Tenant-ID request header (API / integration scenarios)
               3. Subdomain (e.g. acme.smartworkz.com → "acme")
               4. Falls back to "DEFAULT"
            
             All page models read TenantId via BasePage.TenantId — never parse claims directly.

### PermissionMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.PermissionMiddleware`
- **Summary:** Middleware that validates permissions based on claims.
            Adds permission claims to the user's identity based on their roles.
            Caches permissions per user to avoid redundant DB calls.

### PermissionMiddlewareExtensions

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.PermissionMiddlewareExtensions`
- **Summary:** Extension methods for permission middleware

### TenantMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.TenantMiddleware`
- **Summary:** Resolves TenantId once per request and stores it in HttpContext.Items["TenantId"].
            Resolution order:
              1. Authenticated user's "tenant" claim
              2. X-Tenant-ID request header
              3. Subdomain
              4. Falls back to "DEFAULT"

### PermissionMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.PermissionMiddleware`
- **Summary:** Middleware that validates permissions based on claims.
            Adds permission claims to the user's identity based on their roles.
            Caches permissions per user to avoid redundant DB calls.

### PermissionMiddlewareExtensions

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.PermissionMiddlewareExtensions`
- **Summary:** Extension methods for permission middleware

### TenantMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Middleware.TenantMiddleware`
- **Summary:** Resolves TenantId once per request and stores it in HttpContext.Items["TenantId"].
            Resolution order:
              1. Authenticated user's "tenant" claim
              2. X-Tenant-ID request header
              3. Subdomain
              4. Falls back to "DEFAULT"

### PermissionMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Middleware.PermissionMiddleware`
- **Summary:** Middleware that validates permissions based on claims.
            Adds permission claims to the user's identity based on their roles.

### PermissionMiddlewareExtensions

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Middleware.PermissionMiddlewareExtensions`
- **Summary:** Extension methods for permission middleware

### TenantMiddleware

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Middleware.TenantMiddleware`
- **Summary:** Resolves TenantId once per request and stores it in HttpContext.Items["TenantId"].
             Resolution order:
               1. Authenticated user's "tenant" claim
               2. X-Tenant-ID request header (API / integration scenarios)
               3. Subdomain (e.g. acme.smartworkz.com → "acme")
               4. Falls back to "DEFAULT"
            
             All page models read TenantId via BasePage.TenantId — never parse claims directly.

