# Login Redirect Troubleshooting Guide

## Symptom: After Login, Not Redirecting to Dashboard

### ✅ What Should Happen
1. User enters email + password on `/Account/Login`
2. Form submits to `OnPostAsync()`
3. Service authenticates user
4. Admin role is verified
5. Claims are created + added to cookie identity
6. `HttpContext.SignInAsync()` is called
7. **Browser receives 302 Redirect to `/Dashboard`**
8. Browser follows redirect
9. Dashboard page loads with authenticated user context

### ❌ What's Probably Happening
If redirect isn't happening, one of these is occurring:

---

## Diagnosis Steps

### Step 1: Check Browser Network Tab
1. Open **DevTools** (F12 → Network tab)
2. Login with credentials
3. Look for the POST request to `/Account/Login`
4. Check the **Response Status Code**:
   - **200** = Page returned (login failed, no redirect)
   - **302** = Redirect response (good, check Location header)
   - **500** = Server error in login

### Step 2: Check Response Headers
If Status Code is 302:
- Look for **Location** header
- Value should be `/Dashboard`
- If Location is something else, that's the issue

### Step 3: Check Error Message on Page
If Status Code is 200 and you're back on login page:
1. Look for red error message box
2. Common errors:
   - "Invalid credentials" → Password or email wrong
   - "Access Denied" → User doesn't have admin role
   - "An error occurred during login" → Server error in SignInAsync

### Step 4: Check Application Logs
If running locally, check Visual Studio output:
```
Admin admin@smartworkz.test logged in successfully. UserId=..., TenantId=DEFAULT, Roles=admin
Redirecting to /Dashboard. Authentication complete
```

If these logs appear, sign-in succeeded, and redirect should happen.

---

## Common Issues & Fixes

### Issue 1: User Has "Manager" Role Instead of "Admin"
**Symptom**: Login succeeds but gets "Access Denied" error

**Check**:
```sql
SELECT u.Email, r.Name 
FROM Auth.Users u
JOIN Auth.UserRoles ur ON u.UserId = ur.UserId
JOIN Auth.Roles r ON ur.RoleId = r.RoleId
WHERE u.Email = 'admin@smartworkz.test'
```

**Expected**: Row with Role = "Admin"

**Fix**: Update test user role assignment in database
```sql
-- Get the admin role ID
DECLARE @AdminRoleId NVARCHAR(36) = (
    SELECT RoleId FROM Auth.Roles 
    WHERE NormalizedName = 'ADMIN' AND TenantId = 'DEFAULT'
);

-- Get the user ID
DECLARE @UserId NVARCHAR(36) = (
    SELECT UserId FROM Auth.Users 
    WHERE Email = 'admin@smartworkz.test'
);

-- Update the role assignment
DELETE FROM Auth.UserRoles 
WHERE UserId = @UserId AND TenantId = 'DEFAULT';

INSERT INTO Auth.UserRoles (UserId, RoleId, TenantId, CreatedAt)
VALUES (@UserId, @AdminRoleId, 'DEFAULT', GETUTCDATE());
```

---

### Issue 2: Cookie Not Being Set
**Symptom**: Redirect to `/Dashboard` happens but shows login page again

**Check**:
1. Open DevTools → Application → Cookies
2. Look for `.Admin.Auth` cookie
3. If missing, cookie wasn't created

**Possible Causes**:
- **HTTPS redirect issue**: Cookie created for HTTPS but request was HTTP
- **SameSite blocking**: Cookies blocked due to cross-site navigation
- **SignInAsync failed silently**: Error was caught but not logged

**Fix**: Check logs for error message:
```
Error during sign-in for admin@smartworkz.test
```

If you see this, the exception details should be in logs.

---

### Issue 3: Redirect Happens but Dashboard Shows 403/401
**Symptom**: Network shows 302 to `/Dashboard`, but page shows "Access Denied"

**Cause**: Authorization policy check failed

**Check**:
1. Dashboard has `[Authorize(Policy = "RequireAdmin")]`
2. Policy requires role = "admin"
3. Claims might not have the role

**Fix**: Ensure role claim is added:
```csharp
// In Login.cshtml.cs, verify this code runs:
foreach (var role in user.Roles)
{
    claims.Add(new(ClaimTypes.Role, role));
    claims.Add(new("role", role));
}
```

Both `ClaimTypes.Role` and lowercase `"role"` should be added.

---

### Issue 4: TenantId Not Resolving
**Symptom**: Dashboard loads but user context is wrong or session expires immediately

**Check**: Look for TenantId in logs:
```
UserId=..., TenantId=DEFAULT, Roles=...
```

Should show `TenantId=DEFAULT`

**Fix**: Ensure TenantId claim is added:
```csharp
new("TenantId", user.TenantId),
```

---

## Testing Checklist

### Before Testing Login
- [ ] Database has test users (check `Auth.Users`)
- [ ] Users have correct password hash: `k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=`
- [ ] Users have Admin role assigned (check `Auth.UserRoles`)
- [ ] Dashboard page exists at `/src/SmartWorkz.StarterKitMVC.Admin/Pages/Dashboard/Index.cshtml`
- [ ] Dashboard.Index.cshtml.cs has `[Authorize(Policy = "RequireAdmin")]`

### Login Test
1. Start Admin app: `dotnet run --project src/SmartWorkz.StarterKitMVC.Admin`
2. Open DevTools (F12)
3. Navigate to `https://localhost:5001/Account/Login`
4. Enter:
   - Email: `admin@smartworkz.test`
   - Password: `TestPassword123!`
5. Click Sign In
6. **Check Network Tab**:
   - POST to `/Account/Login` should return 302
   - Location header should be `/Dashboard`
7. **Check Application Tab → Cookies**:
   - `.Admin.Auth` cookie should exist
   - Should have `HttpOnly` flag
   - SameSite should be `Strict`
8. **Verify Redirect**:
   - After redirect, URL should be `/Dashboard`
   - Page should load successfully
   - User info should be visible

---

## Detailed Debug: Viewing Actual Claims

Add this temporary code to Dashboard to see what claims are present:

```csharp
public void OnGet()
{
    var claims = User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
    System.Diagnostics.Debug.WriteLine("=== User Claims ===");
    foreach (var claim in claims)
        System.Diagnostics.Debug.WriteLine(claim);
    System.Diagnostics.Debug.WriteLine("==================");
    
    // Rest of your code...
}
```

Then check Visual Studio Output window for the claims dump. You should see:
```
=== User Claims ===
http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier=<UserId>
http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress=admin@smartworkz.test
http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name=Admin User
TenantId=DEFAULT
http://schemas.microsoft.com/ws/2008/06/identity/claims/role=admin
role=admin
==================
```

If `role=admin` is missing, that's why authorization fails!

---

## Enable Debug Logging

To see detailed login flow logging, update `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "SmartWorkz.StarterKitMVC.Admin.Pages.Account.LoginModel": "Debug",
      "SmartWorkz.StarterKitMVC.Admin.Middleware": "Debug",
      "Microsoft.AspNetCore.Authentication": "Debug",
      "Microsoft.AspNetCore.Authorization": "Debug"
    }
  }
}
```

Then check Visual Studio Output window for detailed logs during login.

---

## Still Not Working?

1. **Clear browser cache**: Ctrl+Shift+Delete
2. **Clear cookies**: Delete `.Admin.Auth` cookie manually
3. **Restart app**: Stop and restart with `dotnet run`
4. **Check database**:
   ```sql
   SELECT * FROM Auth.Users WHERE Email = 'admin@smartworkz.test';
   SELECT * FROM Auth.UserRoles WHERE UserId IN (
       SELECT UserId FROM Auth.Users WHERE Email = 'admin@smartworkz.test'
   );
   ```
5. **Check logs**: Full Visual Studio Output (not just Immediate Window)
6. **Trace through code**: Put breakpoint in `LoginModel.OnPostAsync()` and step through

