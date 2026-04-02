# Multi-Tenant Login Flow

This guide explains how the starter kit organizes tenants and tenant users, and how the login system works when the same email/password exist in multiple tenants.

## Architecture Overview

### Three Key Tables

```
┌─────────────────────────────────────────────────────────────┐
│ Tenants (Master Schema)                                      │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ TenantId (PK)  │ Name            │ DisplayName │ Active  │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │ GUID-1         │ ACME Corp       │ ACME        │ True    │ │
│ │ GUID-2         │ Global Tech Ltd │ GTech       │ True    │ │
│ │ GUID-3         │ Local Services  │ LocalServ   │ True    │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ Users (Auth Schema) — GLOBAL & TENANT-SCOPED                │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ UserId │ Email        │ TenantId │ PasswordHash │ Active │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │ USR-1  │ john@ex.com  │ GUID-1   │ hash123      │ True   │ │
│ │ USR-2  │ john@ex.com  │ GUID-2   │ hash456      │ True   │ │
│ │ USR-3  │ john@ex.com  │ GUID-3   │ hash789      │ True   │ │
│ │ USR-4  │ jane@ex.com  │ GUID-1   │ hashABC      │ True   │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ TenantUsers (Pivot Table) — Maps Users to Tenants          │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ TenantUserId │ TenantId │ UserId │ Status   │ AcceptedAt│ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │ 1            │ GUID-1   │ USR-1  │ Active   │ [date]    │ │
│ │ 2            │ GUID-2   │ USR-2  │ Active   │ [date]    │ │
│ │ 3            │ GUID-3   │ USR-3  │ Pending  │ NULL      │ │
│ │ 4            │ GUID-1   │ USR-4  │ Active   │ [date]    │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Key Concept: TenantId in Login

The critical difference: **Login requires BOTH email AND TenantId.**

### User Entity (Auth Schema)

```csharp
public class User
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string TenantId { get; set; }    // ← TENANT SCOPED
    public bool IsActive { get; set; }
    // ... other properties
}
```

**Each user is scoped to a specific tenant.** The same email can exist in multiple tenants as separate User records with different `TenantId` values.

### TenantUser Entity (Pivot Table)

```csharp
public class TenantUser
{
    public int TenantUserId { get; set; }
    public string TenantId { get; set; }    // ← References Tenant
    public string UserId { get; set; }      // ← References User
    public string Status { get; set; }      // Active, Pending, Suspended
    public DateTime? AcceptedAt { get; set; }
}
```

**Links a user to the tenant they belong to.** A user can theoretically belong to multiple tenants via multiple TenantUser rows.

## Login Flow

### Step 1: User Submits Login Form

**Form Data:**
```
Email:    john@example.com
Password: MyPassword123
TenantId: GUID-1 (ACME Corp)
```

**Why TenantId is required:**
- Different tenants need different login screens or context
- User `john@example.com` may exist in 3 different tenants
- We need to know WHICH tenant is logging in

### Step 2: AuthService.LoginAsync()

```csharp
public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
{
    // request.Email = "john@example.com"
    // request.TenantId = "GUID-1"
    
    // Query: Users where Email = "john@example.com" AND TenantId = "GUID-1"
    var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);
    
    if (user == null)  // User doesn't exist in THIS tenant
        return Result.Fail(MessageKeys.Auth.InvalidCredentials);
    
    // Verify password
    if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        return Result.Fail(MessageKeys.Auth.InvalidCredentials);
    
    // Check if user is active
    if (!user.IsActive)
        return Result.Fail(MessageKeys.Auth.AccountInactive);
    
    // Get roles & permissions for THIS tenant
    var roles = await _userRepository.GetUserRolesAsync(user.UserId, request.TenantId);
    var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId, request.TenantId);
    
    // Generate JWT with tenant context
    var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
    
    return Result.Ok(new LoginResponse(accessToken, ...));
}
```

**Key Points:**
- Line 36: Query filters by BOTH `Email` AND `TenantId`
- Line 47-48: Roles and permissions are tenant-specific
- The JWT token includes `TenantId` claim for all future requests

### Step 3: JWT Token with Tenant Context

**Generated JWT includes:**
```json
{
  "sub": "USR-1",                    // User ID
  "email": "john@example.com",
  "tenant_id": "GUID-1",             // ← TENANT in the token
  "roles": ["Admin", "User"],
  "permissions": ["read:products"],
  "locale": "en",
  "iat": 1234567890,
  "exp": 1234571490
}
```

**Every request includes the token, so the server knows:**
- Who is logged in (sub claim)
- Which tenant they belong to (tenant_id claim)
- What roles they have (in that tenant)

## Real-World Scenario

### Scenario: Same User in 3 Tenants

**Database State:**

```
USERS TABLE:
┌────────┬──────────────────┬──────────┐
│ UserId │ Email            │ TenantId │
├────────┼──────────────────┼──────────┤
│ USR-1  │ john@example.com │ ACME     │
│ USR-2  │ john@example.com │ GlobalTech
│ USR-3  │ john@example.com │ LocalSvc │
└────────┴──────────────────┴──────────┘

TENANT_USERS TABLE:
┌────────────────┬──────┬────────┬─────────┐
│ TenantUserId   │ Tenant│ UserId │ Status  │
├────────────────┼──────┼────────┼─────────┤
│ 1              │ ACME │ USR-1  │ Active  │
│ 2              │ GlobalTech │ USR-2  │ Active  │
│ 3              │ LocalSvc   │ USR-3  │ Pending │
└────────────────┴──────┴────────┴─────────┘
```

### Login to ACME (Successful)

```
Input:
  Email: john@example.com
  Password: correct123
  TenantId: ACME
  
Query: SELECT * FROM Users 
       WHERE Email = 'john@example.com' 
       AND TenantId = 'ACME'
       
Result: Returns USR-1
        Password match: ✅
        IsActive: ✅
        
Output: JWT with tenant_id = "ACME"
```

### Login to GlobalTech (Successful)

```
Input:
  Email: john@example.com
  Password: correct123
  TenantId: GlobalTech
  
Query: SELECT * FROM Users 
       WHERE Email = 'john@example.com' 
       AND TenantId = 'GlobalTech'
       
Result: Returns USR-2
        Password match: ✅
        IsActive: ✅
        
Output: JWT with tenant_id = "GlobalTech"
```

### Login to LocalSvc (Fails)

```
Input:
  Email: john@example.com
  Password: correct123
  TenantId: LocalSvc
  
Query: SELECT * FROM Users 
       WHERE Email = 'john@example.com' 
       AND TenantId = 'LocalSvc'
       
Result: Returns USR-3
        Password match: ✅
        IsActive: ❌ (TenantUser status is "Pending", or user not yet confirmed)
        
Output: Account Inactive error
```

## How to Avoid "First Set of User" Issue

### Problem Description

You mentioned: *"its not works on first set of user if i create same user and password on multiple tenent"*

This likely means:
- User created in Tenant A works fine
- User created in Tenant B with same email fails to login
- Or users are sharing logins across tenants

### Root Cause

If not working, likely issues:

1. **Missing TenantId in Login Request**
   ```csharp
   // ❌ Bad - no tenant specified
   var result = await _authService.LoginAsync(
       new LoginRequest("john@example.com", "password123", null));
   
   // ✅ Good - tenant specified
   var result = await _authService.LoginAsync(
       new LoginRequest("john@example.com", "password123", "ACME"));
   ```

2. **Login Page Not Collecting TenantId**
   ```razor
   <!-- ❌ Missing tenant context -->
   <form method="post">
       <input name="Email" />
       <input name="Password" />
       <button>Login</button>
   </form>
   
   <!-- ✅ Include tenant selection -->
   <form method="post">
       <select name="TenantId">
           <option value="ACME">ACME Corp</option>
           <option value="GlobalTech">Global Tech</option>
       </select>
       <input name="Email" />
       <input name="Password" />
       <button>Login</button>
   </form>
   ```

3. **UserRepository.GetByEmailAsync() Not Filtering by TenantId**
   ```csharp
   // ❌ Wrong - no tenant filter
   var user = await _context.Users
       .FirstOrDefaultAsync(u => u.Email == email);
   
   // ✅ Correct - filters by tenant
   var user = await _context.Users
       .FirstOrDefaultAsync(u => u.Email == email 
                              && u.TenantId == tenantId);
   ```

4. **Password Hash Different Between Tenants**
   ```
   If the same password is hashed differently in different tenants,
   login will fail. Passwords must be hashed consistently:
   
   ✅ Same plaintext password → Same hash in all tenants
   (assuming same hashing algorithm)
   ```

## Typical Multi-Tenant Login UI

### Option 1: Tenant Selector on Login Page

```razor
<div class="login-form">
    <form method="post">
        <div class="form-group">
            <label>Select Your Organization</label>
            <select name="TenantId" required>
                <option value="">-- Choose Organization --</option>
                <option value="ACME">ACME Corp</option>
                <option value="GlobalTech">Global Tech</option>
                <option value="LocalSvc">Local Services</option>
            </select>
        </div>
        
        <div class="form-group">
            <label>Email</label>
            <input type="email" name="Email" required />
        </div>
        
        <div class="form-group">
            <label>Password</label>
            <input type="password" name="Password" required />
        </div>
        
        <button type="submit">Login</button>
    </form>
</div>
```

**UX:** User selects their organization first, then enters credentials.

### Option 2: Subdomain-Based Tenant Selection

```
https://acme.myapp.com/login
  → TenantId automatically set to "ACME"
  → Only login with ACME users

https://globaltech.myapp.com/login
  → TenantId automatically set to "GlobalTech"
  → Only login with GlobalTech users
```

**Extract from subdomain:**
```csharp
public class SubdomainTenantMiddleware
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var host = context.Request.Host.Host;  // e.g., "acme.myapp.com"
        var subdomain = host.Split('.')[0];     // e.g., "acme"
        
        // Map subdomain to TenantId
        var tenantId = await _tenantService.GetTenantIdBySubdomainAsync(subdomain);
        tenantContext.SetTenantId(tenantId);
        
        await _next(context);
    }
}
```

### Option 3: Email Domain-Based Tenant Selection

```
Email:    john@acme-corp.com
  → Extract domain "acme-corp.com"
  → Look up tenant by domain
  → Auto-select ACME tenant

Email:    john@globaltech.org
  → Extract domain "globaltech.org"
  → Look up tenant by domain
  → Auto-select GlobalTech tenant
```

**Implementation:**
```csharp
public async Task<string> GetTenantIdByEmailDomainAsync(string email)
{
    var domain = email.Split('@')[1];  // e.g., "acme-corp.com"
    
    var tenant = await _context.Tenants
        .FirstOrDefaultAsync(t => t.EmailDomain == domain);
    
    return tenant?.TenantId;
}
```

## Data Isolation Verification

### Row-Level Security (RLS) Pattern

After login, all queries must filter by TenantId:

```csharp
// ✅ Correct - filters by current tenant
public async Task<List<Product>> GetProductsAsync(Guid tenantId)
{
    return await _context.Products
        .Where(p => p.TenantId == tenantId)
        .ToListAsync();
}

// ❌ Wrong - returns all products from all tenants
public async Task<List<Product>> GetProductsAsync()
{
    return await _context.Products.ToListAsync();
}
```

**Every repository method must:**
1. Accept TenantId parameter
2. Filter queries by TenantId
3. Prevent cross-tenant data leakage

## Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| Same user can't exist in multiple tenants | Missing TenantId in User table unique constraint | Unique(Email, TenantId) instead of Unique(Email) |
| Login works for one tenant, fails for another | TenantId not in login request | Add TenantId to LoginRequest DTO |
| User logged into Tenant A can see Tenant B data | Missing TenantId filter in queries | Add .Where(x => x.TenantId == tenantId) to all repository methods |
| Password hash doesn't match | Different hashing between tenants | Use same hashing algorithm for all tenants |
| First user works, second user fails | Unique constraint on Email only | Change to Unique(Email, TenantId) |

## See Also

- [Base Page Pattern](./03-base-page-pattern.md) — TenantId context in pages
- [Translation System](./01-translation-system.md) — Per-tenant translation overrides
- [AuthService.cs](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Services/AuthService.cs) — Login implementation
- [User Entity](../../src/SmartWorkz.StarterKitMVC.Domain/Entities/Auth/User.cs) — User schema
- [TenantUser Entity](../../src/SmartWorkz.StarterKitMVC.Domain/Entities/Auth/TenantUser.cs) — Pivot table
