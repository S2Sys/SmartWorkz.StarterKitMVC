# Tenant-Scoped Authorization with Super Admin Access

## Overview

This system implements multi-tenant authorization with three levels of access:

1. **Super Admin** — Global access across all tenants, system-wide operations
2. **Tenant Admin** — Full access to specific tenant's data and operations  
3. **Tenant User** — Limited permissions within their assigned tenant

## Architecture

### Database Schema

```
Auth.Roles
├── RoleId (primary key)
├── Name (e.g., "admin", "super_admin", "manager")
├── TenantId (nullable — NULL = system role)
└── IsSystemRole (1 = super_admin, 0 = tenant-scoped)

Auth.UserRoles
├── UserId
├── RoleId
└── TenantId (which tenant this role applies to)

Auth.Permissions
├── PermissionId
├── Name (e.g., "Manage Users", "View Reports")
├── PermissionType (Create, Read, Update, Delete)
├── ResourceType (Product, Order, User)
└── TenantId (which tenant this permission applies to)

Auth.RolePermissions
├── RoleId
├── PermissionId
└── TenantId
```

### Claim Format

During login, roles and permissions are encoded as claims:

```csharp
// Role claims
ClaimTypes.Role = "super_admin"           // Super admin (global)
ClaimTypes.Role = "admin"                 // Tenant admin
ClaimTypes.Role = "manager"               // Tenant manager
ClaimTypes.Role = "admin:tenant-a"        // Tenant-scoped role (optional format)

// Permission claims  
"permission" = "manage_users:tenant-a"    // Tenant-scoped permission
"permission" = "view_reports"             // Global permission

// Tenant context
"TenantId" = "tenant-a"                   // Current tenant
```

---

## Usage Examples

### 1. Page-Level Authorization (Razor Pages)

#### Require Tenant Admin
```csharp
[Authorize(Policy = "RequireTenantAdmin")]
public class SettingsModel : BasePage
{
    public void OnGet()
    {
        // Only users who are:
        // - Super admin, OR
        // - Admin for their current tenant
        // can access this page
    }
}
```

#### Require Super Admin (System-Wide)
```csharp
[Authorize(Policy = "RequireSuperAdmin")]
public class SystemSettingsModel : BasePage
{
    public void OnGet()
    {
        // Only super admins can access this page
    }
}
```

### 2. Code-Level Authorization (Extension Methods)

#### Check Super Admin Status
```csharp
public async Task<IActionResult> OnPostDeleteTenantAsync(string tenantId)
{
    // Only super admin can delete tenants
    if (!User.IsSuperAdmin())
        return Forbid();

    await _tenantService.DeleteAsync(tenantId);
    return RedirectToPage("/Index");
}
```

#### Check Tenant Admin for Current Tenant
```csharp
public void OnGet()
{
    var currentTenantId = User.GetTenantId();
    
    if (!User.IsTenantAdmin(currentTenantId))
        return Forbid();

    // User is admin for their tenant
}
```

#### Check Tenant-Scoped Permissions
```csharp
public async Task<IActionResult> OnPostAsync()
{
    var currentTenantId = User.GetTenantId();
    
    // Check if user has "Manage Users" permission for their tenant
    if (!User.HasTenantPermission("Manage Users", currentTenantId))
    {
        ModelState.AddModelError("", "You don't have permission to manage users");
        return Page();
    }

    // Proceed with user management
    await _userService.CreateAsync(Input.Email, currentTenantId);
    return RedirectToPage("Index");
}
```

#### Check Multiple Permissions
```csharp
public async Task<IActionResult> OnGetAsync()
{
    var currentTenantId = User.GetTenantId();
    
    // ALL of these permissions required
    if (!User.HasAllTenantPermissions(currentTenantId, 
        "Manage Users", "Manage Roles", "View Reports"))
    {
        return Forbid();
    }

    // User has all three permissions
    return Page();
}
```

#### Super Admin Bypass
```csharp
public async Task<IActionResult> OnPostAsync()
{
    var currentTenantId = User.GetTenantId();
    
    // Super admin always passes; other users need specific permission
    if (!User.IsSuperAdmin() && 
        !User.HasTenantPermission("Delete Reports", currentTenantId))
    {
        return Forbid();
    }

    await _reportService.DeleteAsync(reportId);
    return RedirectToPage("Index");
}
```

### 3. Conditional Rendering in Razor Views

```html
@{
    var currentTenantId = User.GetTenantId();
}

<!-- Only show to tenant admins -->
@if (User.IsTenantAdmin(currentTenantId))
{
    <div class="admin-panel">
        <a href="/Settings">Tenant Settings</a>
        <a href="/Users">Manage Users</a>
    </div>
}

<!-- Only show to super admins -->
@if (User.IsSuperAdmin())
{
    <div class="system-panel">
        <a href="/Admin/SystemSettings">System Settings</a>
        <a href="/Admin/Tenants">Manage Tenants</a>
    </div>
}

<!-- Show if user has specific permission -->
@if (User.HasTenantPermission("View Reports", currentTenantId))
{
    <a href="/Reports">View Reports</a>
}
```

### 4. Dataflow Example: Managing Users by Tenant

```csharp
[Authorize(Policy = "CanManageUsers")]
public class Users.IndexModel : BaseListPage<UserDto>
{
    private readonly IUserService _userService;

    public Users.IndexModel(IUserService userService)
        => _userService = userService;

    public async Task OnGetAsync(int page = 1, string? search = null)
    {
        // TenantId from BasePage (set by TenantMiddleware)
        var currentTenantId = TenantId;

        // Super admin can view all tenants, others only see their own
        if (User.IsSuperAdmin())
        {
            // Load users from all tenants
            Data = await _userService.SearchAllTenantsAsync(search, page);
        }
        else
        {
            // Load users from current tenant only
            Data = await _userService.SearchByTenantAsync(currentTenantId, search, page);
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        var currentTenantId = TenantId;

        // Check permission
        if (!User.IsSuperAdmin() && 
            !User.HasTenantPermission("Manage Users", currentTenantId))
        {
            _logger.LogWarning("Unauthorized delete attempt by user {UserId}", 
                CurrentUserId);
            return Forbid();
        }

        await _userService.DeleteAsync(userId, currentTenantId);
        return RedirectToPage();
    }
}
```

---

## Implementation Steps

### Step 1: Database Setup

Ensure your database has the role and permission tables:

```sql
-- Roles table must have TenantId (nullable for system roles)
INSERT INTO Auth.Roles (RoleId, Name, NormalizedName, TenantId, IsSystemRole)
VALUES 
    -- System roles (TenantId = NULL)
    ('super_admin', 'Super Admin', 'SUPER_ADMIN', NULL, 1),
    
    -- Tenant-scoped roles
    ('admin_tenant_a', 'Tenant A Admin', 'ADMIN', 'tenant-a', 0),
    ('admin_tenant_b', 'Tenant B Admin', 'ADMIN', 'tenant-b', 0);

-- Assign users to roles with tenant context
INSERT INTO Auth.UserRoles (UserId, RoleId, TenantId)
VALUES 
    ('user-1', 'super_admin', NULL),           -- User 1 is super admin
    ('user-2', 'admin_tenant_a', 'tenant-a'),  -- User 2 is admin for tenant-a
    ('user-3', 'admin_tenant_b', 'tenant-b');  -- User 3 is admin for tenant-b
```

### Step 2: Update Login Flow

The login handler (Login.cshtml.cs) must include tenant information in claims:

```csharp
// In Login.cshtml.cs - OnPostAsync()

// Add tenant claim
if (!string.IsNullOrEmpty(user.TenantId))
    claims.Add(new("TenantId", user.TenantId));

// Add roles with tenant context
foreach (var role in user.Roles)
{
    var normalizedRole = role.ToLowerInvariant();
    claims.Add(new(ClaimTypes.Role, normalizedRole));
    
    // Optional: Add tenant-scoped format "role:tenant"
    if (!string.IsNullOrEmpty(user.TenantId))
        claims.Add(new(ClaimTypes.Role, $"{normalizedRole}:{user.TenantId}"));
}

// Add permissions with tenant context
foreach (var perm in user.Permissions)
{
    var normalizedPerm = perm.ToLowerInvariant();
    claims.Add(new("permission", normalizedPerm));
    
    // Optional: Add tenant-scoped format "permission:tenant"
    if (!string.IsNullOrEmpty(user.TenantId))
        claims.Add(new("permission", $"{normalizedPerm}:{user.TenantId}"));
}
```

### Step 3: Update Authorization Middleware

The AuthorizationMiddleware should enrich claims per tenant:

```csharp
// In AuthorizationMiddleware.cs - ValidateAndEnrichAuthorizationAsync()

var userTenantId = context.User.FindFirst("TenantId")?.Value;

// Validate and enrich claims per tenant
foreach (var claimType in claimTypes)
{
    var claimValuesForTenant = await claimService
        .GetClaimValuesForRoleAndTenantAsync(roles, claimType.Key, userTenantId);

    foreach (var value in claimValuesForTenant)
    {
        var tenantScopedValue = $"{value}:{userTenantId}";
        identity.AddClaim(new Claim(claimType.Key, tenantScopedValue));
    }
}
```

### Step 4: Use the Extensions

In your pages and services:

```csharp
// In any Razor Page
public void OnGet()
{
    var tenantId = User.GetTenantId();
    
    // Is super admin?
    if (User.IsSuperAdmin()) { /* ... */ }
    
    // Is tenant admin?
    if (User.IsTenantAdmin(tenantId)) { /* ... */ }
    
    // Has permission?
    if (User.HasTenantPermission("Manage Users", tenantId)) { /* ... */ }
}
```

---

## Policy Reference

### [Authorize(Policy = "RequireAdmin")]
- Requires: `admin` OR `super_admin` role
- Scope: Single tenant (checks user's TenantId)
- Use: Basic admin pages

### [Authorize(Policy = "RequireSuperAdmin")]
- Requires: `super_admin` role only
- Scope: Global (all tenants)
- Use: System-wide operations (delete tenant, system settings)

### [Authorize(Policy = "RequireTenantAdmin")]
- Requires: User is super admin OR admin for current tenant
- Scope: Current tenant with super admin bypass
- Use: Tenant-level admin pages (user management, settings)

### [Authorize(Policy = "CanManageUsers")]
- Requires: "Manage Users" permission for current tenant
- Scope: Tenant-scoped with super admin bypass
- Use: User management pages

---

## Access Control Matrix

| User Type | Super Admin | Tenant Admin | Can View Own Tenant | Can View Other Tenants | Can Delete Tenant |
|-----------|:-----------:|:------------:|:-------------------:|:----------------------:|:-----------------:|
| Super Admin | ✅ | ✅ (all) | ✅ | ✅ | ✅ |
| Tenant Admin | ❌ | ✅ (own) | ✅ | ❌ | ❌ |
| Tenant User | ❌ | ❌ | ✅ | ❌ | ❌ |

---

## Key Points

1. **Super Admin Bypass** — Super admins automatically pass all tenant-scoped checks
2. **Tenant Isolation** — Regular admins are isolated to their assigned tenant
3. **Current Tenant** — `User.GetTenantId()` returns user's current tenant from claims
4. **Case Insensitive** — All role and permission checks are case-insensitive
5. **Claim-Based** — Everything relies on claims in the cookie; no DB lookups in authorization
6. **Cached** — Permissions cached for 5-10 minutes to minimize DB load

---

## Testing Super Admin vs Tenant Admin

```csharp
[Fact]
public async Task SuperAdminCanAccessAllTenants()
{
    var superAdminUser = CreateUserWithRole("super_admin");
    
    // Should pass for ANY tenant
    Assert.True(superAdminUser.IsSuperAdmin());
    Assert.True(superAdminUser.IsTenantAdmin("tenant-a"));
    Assert.True(superAdminUser.IsTenantAdmin("tenant-b"));
}

[Fact]
public async Task TenantAdminCanOnlyAccessOwnTenant()
{
    var adminUser = CreateUserWithRole("admin", tenantId: "tenant-a");
    
    Assert.False(adminUser.IsSuperAdmin());
    Assert.True(adminUser.IsTenantAdmin("tenant-a"));
    Assert.False(adminUser.IsTenantAdmin("tenant-b"));
}
```
