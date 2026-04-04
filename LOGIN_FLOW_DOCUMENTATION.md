# Login Flow Documentation

## Architecture Overview

The application has **TWO separate authentication flows**:

### 1. **Admin Portal (Razor Pages)** - Cookie-based
- **Path**: `/src/SmartWorkz.StarterKitMVC.Admin`
- **Entry Point**: `/Account/Login`
- **Authentication**: Cookie-based (ASP.NET Core Identity-style)
- **Target**: `/Dashboard` (Razor Pages)
- **Roles**: Requires "admin" role to access dashboard

### 2. **Web API (ASP.NET Core)** - JWT Bearer Token
- **Path**: `/src/SmartWorkz.StarterKitMVC.Web/Controllers/Api`
- **Entry Point**: `POST /api/auth/login`
- **Authentication**: JWT Bearer tokens
- **Target**: Returns `accessToken` + `refreshToken` + user profile

---

## Admin Portal Login Flow

### Step 1: User Navigates to Login Page
```
GET /Account/Login
→ Shows login form (Email + Password)
```

### Step 2: Submit Login Credentials
```
POST /Account/Login
Form Data:
  - Input.Email: "admin@smartworkz.test"
  - Input.Password: "TestPassword123!"
```

### Step 3: Login Page Model (`Login.cshtml.cs`)
The `LoginModel.OnPostAsync()` method executes:

1. **Validate** input model is valid
2. **Authenticate** via `IAuthService.LoginAsync()`
   - Fetches user from DB via Dapper
   - Verifies password using PBKDF2-SHA256
   - Gets user roles and permissions
   - Returns `LoginResponse` with JWT tokens
3. **Check Admin Role**
   - User must have "admin" role in `result.Data.User.Roles`
   - Returns error page if non-admin
4. **Create Claims Principal** with claims:
   - `ClaimTypes.NameIdentifier` → UserId
   - `ClaimTypes.Email` → Email
   - `ClaimTypes.Name` → DisplayName
   - `ClaimTypes.Role` → Each role (e.g., "admin")
   - `"permission"` → Each permission name
   - `"TenantId"` → User's tenant (for multi-tenant resolution)
5. **SignInAsync** with Cookie
   - Creates authentication cookie: `.Admin.Auth`
   - Cookie is HttpOnly, Strict SameSite
   - Expires in 4 hours
   - Sets authentication claims
6. **Redirect** to Dashboard
   - Default: `LocalRedirect("/Dashboard")`
   - Or: `LocalRedirect(returnUrl)` if specified

### Step 4: Dashboard Access
```
GET /Dashboard
→ Middleware resolves TenantId from claims
→ Authorization checks:
   - [Authorize(Policy = "RequireAdmin")] 
   - Requires "admin" role
→ Returns dashboard view with user context
```

---

## Key Configuration

### Authentication Setup (`Program.cs`)
```csharp
// Default scheme is Cookie (not JWT)
options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

// Cookie configuration
options.LoginPath = "/Account/Login";
options.AccessDeniedPath = "/Account/AccessDenied";
options.ExpireTimeSpan = TimeSpan.FromHours(4);
options.Cookie.Name = ".Admin.Auth";
options.Cookie.HttpOnly = true;
options.Cookie.SameSite = SameSiteMode.Strict;
```

### Authorization Policies
```csharp
// RequireAdmin policy
options.AddPolicy("RequireAdmin", policy =>
    policy.RequireAuthenticatedUser()
          .RequireRole("admin"));
```

### TenantId Resolution (`TenantMiddleware`)
Priority order:
1. Authenticated user's `"TenantId"` claim
2. `X-Tenant-ID` request header
3. Subdomain (e.g., `admin.localhost`)
4. Falls back to `"DEFAULT"`

---

## Test Users

All test users have password: `TestPassword123!`

| Email | Password | TenantId | Role |
|-------|----------|----------|------|
| admin@smartworkz.test | TestPassword123! | DEFAULT | Admin ✅ |
| manager@smartworkz.test | TestPassword123! | DEFAULT | Manager ❌ |
| staff@smartworkz.test | TestPassword123! | DEFAULT | Staff ❌ |
| customer@smartworkz.test | TestPassword123! | DEFAULT | Customer ❌ |

✅ = Can access admin dashboard  
❌ = Gets "Access Denied" error

---

## Troubleshooting

### Login Fails with "Invalid Credentials"
- **Cause**: Password hash mismatch in database
- **Check**: User password hash in `Auth.Users` table
- **Expected Hash**: `k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=`
- **Fix**: Update database with correct hash

### After Login, Redirects but Pages Show "Not Authorized"
- **Cause**: Authorization policy not matching
- **Check**: User roles include "admin" (case-insensitive)
- **Check**: Claims contain `ClaimTypes.Role` with value "admin"
- **Fix**: Verify role is seeded correctly in `Auth.UserRoles` table

### Cookie Not Set / Session Lost
- **Cause**: SameSite=Strict blocking cross-site cookies
- **Check**: Cookie.SecurePolicy setting
- **For Development**: Use `CookieSecurePolicy.SameAsRequest`
- **For Production**: Use HTTPS + `CookieSecurePolicy.Always`

### 404 on `/Dashboard` Redirect
- **Cause**: Dashboard page doesn't exist at that route
- **Current Route**: `/Dashboard` (Razor Pages routing)
- **File Location**: `/Pages/Dashboard/Index.cshtml`
- **Note**: Razor Pages maps `Index` pages to parent folder route

### TenantId Not Resolving
- **Cause**: Middleware can't find TenantId claim
- **Check**: Login.cshtml.cs adds `"TenantId"` claim
- **Current**: Added in this version to ensure proper resolution
- **Fallback**: Defaults to "DEFAULT" if missing

---

## Cookie-Based vs JWT-Based Auth

### Use Cookie-Based (Admin Portal)
- Traditional MVC/Razor Pages app
- Server-side session management
- HTML form submissions
- Cookie automatically sent with requests
- SameSite protection against CSRF

### Use JWT-Based (Web API)
- Single Page Applications (SPA)
- Mobile apps
- Cross-origin requests
- Client manually includes `Authorization: Bearer <token>`
- Token in response body, app stores it (localStorage/sessionStorage)

---

## Database Dependencies

### Required Tables
- `Auth.Users` - User accounts with password hashes
- `Auth.Roles` - Role definitions
- `Auth.UserRoles` - User-to-role mappings
- `Auth.Permissions` - Permission definitions
- `Auth.RolePermissions` - Role-to-permission mappings
- `Auth.UserPermissions` - Direct user-to-permission mappings

### Required Stored Procedures (Dapper)
- `Auth.sp_GetUserByEmail` - Fetch user with credentials
- `Auth.sp_GetUserRoles` - Get roles for user
- `Auth.sp_GetUserPermissions` - Get permissions for user

---

## Security Considerations

1. **Password Hashing**: PBKDF2-SHA256 with 100,000 iterations
2. **Salt**: 16 bytes, randomly generated per password
3. **Cookie**: HttpOnly (prevents JS access), Strict SameSite (CSRF protection)
4. **Authorization**: Role-based + Policy-based checks
5. **Tenant Isolation**: TenantId in claims + middleware enforcement

---

## Next Steps to Test

1. **Start Admin Portal**: `dotnet run` from `SmartWorkz.StarterKitMVC.Admin` folder
2. **Navigate to**: `https://localhost:5001/Account/Login`
3. **Enter Credentials**:
   - Email: `admin@smartworkz.test`
   - Password: `TestPassword123!`
4. **Submit**: Should redirect to `/Dashboard` with logged-in user context
5. **Verify**: Cookie `.Admin.Auth` should be present in browser DevTools

