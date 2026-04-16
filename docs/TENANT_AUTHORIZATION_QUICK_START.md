# Tenant Authorization - Quick Start Guide

## Three-Level Access System

```
┌─────────────────────────────────────────────────────────────────┐
│                     SUPER ADMIN (Global)                        │
│  • Access: ALL tenants, ALL data, system-wide operations        │
│  • Roles: super_admin                                           │
│  • Example: Delete tenant, System settings, View all users      │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────────────┐
│              TENANT ADMIN (Tenant-Scoped)                       │
│  • Access: Own tenant only, user/role management                │
│  • Roles: admin (within tenant context)                         │
│  • Example: Manage users in tenant-a, Tenant settings           │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────────────┐
│              TENANT USER (Limited)                              │
│  • Access: Own tenant, limited to assigned permissions          │
│  • Roles: manager, viewer, etc.                                 │
│  • Permissions: View Reports, Create Order, etc.                │
└─────────────────────────────────────────────────────────────────┘
```

---

## Authorization Flow

### Request Processing

```
HTTP Request
    ↓
UseAuthentication()
    │ Load cookie → claims with role "admin" or "super_admin"
    ↓
UseTenantResolution()
    │ Extract TenantId from claims or header
    │ Store in HttpContext.Items["TenantId"]
    ↓
UseAuthorizationValidation()
    │ Enrich claims with DB permissions
    ↓
UseAuthorization()
    │ Evaluate [Authorize] policies
    │ Check: Is super_admin? YES → Allow
    │ Check: Is admin for current tenant? → Allow
    │ Check: Has specific permission? → Allow/Deny
    ↓
Page/Controller Executes
```

---

## Code Patterns

### Check Super Admin (Global)
```csharp
if (!User.IsSuperAdmin())
    return Forbid();
```
- No tenant scope
- Access all data globally

### Check Tenant Admin
```csharp
var tenantId = User.GetTenantId();
if (!User.IsTenantAdmin(tenantId))
    return Forbid();
```
- Super admin passes automatically
- Tenant admin passes for their own tenant only

### Check Tenant Permission
```csharp
var tenantId = User.GetTenantId();
if (!User.HasTenantPermission("Manage Users", tenantId))
    return Forbid();
```
- Super admin passes automatically
- Regular users need explicit permission

### Conditional Access (No Error)
```csharp
var tenantId = User.GetTenantId();

// Show data only if user has permission
if (User.IsSuperAdmin() || User.HasTenantPermission("View Reports", tenantId))
{
    var reports = await _reportService.GetAsync();
}
else
{
    reports = new();
}
```

---

## Page-Level Policies

### Require Super Admin
```csharp
[Authorize(Policy = "RequireSuperAdmin")]
public class SystemSettingsModel : BasePage { }
```
- Only `super_admin` role
- No tenant context needed

### Require Tenant Admin
```csharp
[Authorize(Policy = "RequireTenantAdmin")]
public class TenantSettingsModel : BasePage { }
```
- Super admin OR admin for current tenant
- Automatically checks current tenant

### Require Permission
```csharp
[Authorize(Policy = "CanManageUsers")]
public class UsersModel : BasePage { }
```
- Super admin OR "Manage Users" permission for tenant
- Automatically checks current tenant

---

## Setting Up Roles & Permissions

### Database Inserts

**System Roles (No Tenant):**
```sql
INSERT INTO Auth.Roles (RoleId, Name, NormalizedName, IsSystemRole)
VALUES ('super_admin', 'Super Admin', 'SUPER_ADMIN', 1)
```

**Tenant-Scoped Roles:**
```sql
INSERT INTO Auth.Roles (RoleId, Name, NormalizedName, TenantId, IsSystemRole)
VALUES 
    ('admin_ta', 'Admin', 'ADMIN', 'tenant-a', 0),
    ('manager_ta', 'Manager', 'MANAGER', 'tenant-a', 0)
```

**Permissions:**
```sql
INSERT INTO Auth.Permissions (Name, PermissionType, ResourceType, TenantId)
VALUES 
    ('Manage Users', 'Create', 'User', 'tenant-a'),
    ('View Reports', 'Read', 'Report', 'tenant-a')
```

**Assign to Users:**
```sql
-- Super admin
INSERT INTO Auth.UserRoles (UserId, RoleId)
VALUES ('user-1', 'super_admin')

-- Tenant admin
INSERT INTO Auth.UserRoles (UserId, RoleId, TenantId)
VALUES ('user-2', 'admin_ta', 'tenant-a')
```

---

## Login Example

```csharp
// In Login.cshtml.cs OnPostAsync()

var user = await _authService.LoginAsync(email, password);

var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, user.UserId),
    new(ClaimTypes.Email, user.Email),
    new("TenantId", user.TenantId ?? "DEFAULT"),
};

// Add roles (normalized to lowercase)
foreach (var role in user.Roles ?? [])
    claims.Add(new(ClaimTypes.Role, role.ToLowerInvariant()));

// Add permissions
foreach (var perm in user.Permissions ?? [])
    claims.Add(new("permission", perm.ToLowerInvariant()));

var principal = new ClaimsPrincipal(
    new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme, 
    principal);
```

---

## Access Control Examples

### Example 1: View User List (Multi-Tenant)
```csharp
public async Task OnGetAsync()
{
    // Get tenant ID from claims
    var tenantId = User.GetTenantId();

    // Super admin sees ALL tenants; others see only their own
    if (User.IsSuperAdmin())
        Users = await _userService.GetAllAsync();
    else
        Users = await _userService.GetByTenantAsync(tenantId);
}
```

### Example 2: Delete User (Permission-Based)
```csharp
public async Task<IActionResult> OnPostDeleteAsync(string userId)
{
    var tenantId = User.GetTenantId();

    // Requires "Manage Users" permission for tenant
    // Super admin passes automatically
    if (!User.IsSuperAdmin() && 
        !User.HasTenantPermission("Manage Users", tenantId))
    {
        return Forbid();
    }

    await _userService.DeleteAsync(userId, tenantId);
    return RedirectToPage();
}
```

### Example 3: System Operation (Super Admin Only)
```csharp
public async Task<IActionResult> OnPostDeleteTenantAsync(string tenantId)
{
    // Only super admin can delete tenants
    if (!User.IsSuperAdmin())
        return Forbid();

    await _tenantService.DeleteAsync(tenantId);
    return RedirectToPage("/Tenants");
}
```

---

## Extension Methods Reference

| Method | Returns | Usage |
|--------|---------|-------|
| `User.IsSuperAdmin()` | bool | Is user a super admin? |
| `User.IsTenantAdmin(tenantId)` | bool | Is user admin for tenant? |
| `User.HasTenantRole(role, tenantId)` | bool | Does user have role in tenant? |
| `User.HasTenantPermission(perm, tenantId)` | bool | Does user have permission in tenant? |
| `User.HasAnyTenantPermission(tenantId, perms)` | bool | Does user have ANY permission? |
| `User.HasAllTenantPermissions(tenantId, perms)` | bool | Does user have ALL permissions? |
| `User.GetTenantId()` | string? | Get user's current tenant ID |
| `User.GetTenantIds()` | List<string> | Get all tenant IDs for user |
| `User.HasTenantAccess(role, perm, tenantId)` | bool | Comprehensive check |

---

## Key Differences: Super Admin vs Tenant Admin

| Operation | Super Admin | Tenant Admin | Regular User |
|-----------|:-----------:|:------------:|:------------:|
| View own tenant | ✅ | ✅ | ✅ |
| View other tenants | ✅ | ❌ | ❌ |
| Manage users in own tenant | ✅ | ✅ (if permission) | ❌ |
| Manage users in other tenants | ✅ | ❌ | ❌ |
| Delete tenant | ✅ | ❌ | ❌ |
| System settings | ✅ | ❌ | ❌ |
| Manage roles | ✅ | ✅ (own tenant) | ❌ |

---

## Testing

```csharp
[Fact]
public void SuperAdminPassesAllChecks()
{
    var user = CreateUserWithClaim(ClaimTypes.Role, "super_admin");
    
    Assert.True(user.IsSuperAdmin());
    Assert.True(user.IsTenantAdmin("any-tenant"));
    Assert.True(user.HasTenantPermission("any-permission", "any-tenant"));
}

[Fact]
public void TenantAdminLimitedToOwnTenant()
{
    var user = CreateUserWithClaims(
        new(ClaimTypes.Role, "admin"),
        new("TenantId", "tenant-a"));
    
    Assert.False(user.IsSuperAdmin());
    Assert.True(user.IsTenantAdmin("tenant-a"));
    Assert.False(user.IsTenantAdmin("tenant-b"));
}

[Fact]
public void PermissionChecksHonorTenant()
{
    var user = CreateUserWithClaims(
        new("permission", "manage_users:tenant-a"),
        new("TenantId", "tenant-a"));
    
    Assert.True(user.HasTenantPermission("manage_users", "tenant-a"));
    Assert.False(user.HasTenantPermission("manage_users", "tenant-b"));
}
```

---

## Common Pitfalls

❌ **Wrong:** Checking role without tenant context
```csharp
if (User.HasRole("admin")) // admin might be for different tenant!
    return Ok();
```

✅ **Right:** Use tenant-aware method
```csharp
if (User.IsTenantAdmin(User.GetTenantId()))
    return Ok();
```

❌ **Wrong:** Forgetting super admin bypass
```csharp
if (!User.HasTenantPermission("view_reports", tenantId))
    return Forbid(); // Super admin can't see reports!
```

✅ **Right:** Add super admin bypass
```csharp
if (!User.IsSuperAdmin() && 
    !User.HasTenantPermission("view_reports", tenantId))
    return Forbid();
```

❌ **Wrong:** Hardcoding tenant ID
```csharp
var data = await _service.GetAsync("tenant-a"); // Only for one tenant
```

✅ **Right:** Use dynamic tenant from claims
```csharp
var tenantId = User.GetTenantId();
var data = await _service.GetAsync(tenantId);
```

---

## See Also

- [TENANT_AUTHORIZATION.md](TENANT_AUTHORIZATION.md) — Full documentation
- [Program.cs](../src/SmartWorkz.StarterKitMVC.Admin/Program.cs) — Policy configuration
- [TenantAuthorizationExtensions.cs](../src/SmartWorkz.StarterKitMVC.Shared/Extensions/TenantAuthorizationExtensions.cs) — Extension methods
- [TenantAdminRequirement.cs](../src/SmartWorkz.StarterKitMVC.Infrastructure/Authorization/TenantAdminRequirement.cs) — Policy handler
