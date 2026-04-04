# Admin Login Redirect Troubleshooting Guide

## Symptoms
Login in admin portal is not redirecting to Dashboard after successful authentication.

## Diagnostic Steps

### Step 1: Check Log Files
The application logs detailed debugging information to disk.

1. **Console Logs** (if running from command line)
   - Look for lines starting with:
     - `Admin {email} logged in successfully`
     - `Claims added to principal`
     - `Redirecting to /Dashboard`
     - `Dashboard.OnGet called`

2. **File Logs** (recommended method)
   - Location: `bin/Debug/net9.0/Logs/` directory under admin project
   - Files:
     - `admin-YYYY-MM-DD.log` - All HTTP requests/responses
     - `login-redirect.log` - Specific login redirect info

### Step 2: Browser DevTools Analysis

**Chrome/Edge:**
1. Open DevTools (F12)
2. Go to Network tab
3. Clear all requests
4. Perform login
5. Look for the POST /Account/Login request

**Check:**
- ✅ POST /Account/Login returns **302 Found** (not 200)
- ✅ Response headers include `Location: /Dashboard`
- ✅ Response headers include `Set-Cookie: .Admin.Auth=...`
- ✅ The next GET /Dashboard request includes the cookie
- ✅ GET /Dashboard returns 200 (not 302 or 403)

### Step 3: Identify the Problem

#### Problem: Login returns 200 instead of 302

**Symptom:** Page stays on login form, no error message

**Cause:** Login succeeded but redirect code wasn't executed

**Check logs for:** "Admin {email} logged in successfully"

**Fix:** Review Login.cshtml.cs - make sure `return LocalRedirect(redirectUrl)` is reached

---

#### Problem: Login returns 302 but browser shows login page again

**Symptom:** POST returns 302 with Location header, but you end up back on login

**Cause:** Cookie not being sent on redirect, or cookie is invalid

**Check logs for:**
- "Claims added to principal: ..." - verify admin role is included
- Check if claims include `http://schemas.microsoft.com/ws/2008/06/identity/claims/role=admin`

**Fix:** 
- Verify `SameSite=Lax` in [Program.cs:54](src/SmartWorkz.StarterKitMVC.Admin/Program.cs#L54)
- Verify cookie is being set in SignInAsync call

---

#### Problem: Redirect to /Dashboard returns 403 Forbidden

**Symptom:** Login redirects successfully (302), but Dashboard shows access denied

**Cause:** User doesn't have required role or authorization check failed

**Check logs for:**
- "Dashboard.OnGet called. User authenticated: False" - User not authenticated after redirect
- "User claims: ..." - Verify admin role is present

**Fix:**
- Verify admin role is added: `claims.Add(new(ClaimTypes.Role, role));`
- Check that login service returned user with "admin" role
- Verify database has user with admin role

---

#### Problem: Redirect to /Dashboard returns 404

**Symptom:** Browser shows "Page not found"

**Cause:** Dashboard URL is incorrect or page doesn't exist

**Fix:**
- Dashboard page should be at: `/Dashboard/Index`
- LocalRedirect to `/Dashboard` should work automatically due to route conventions
- Verify page exists: `src/SmartWorkz.StarterKitMVC.Admin/Pages/Dashboard/Index.cshtml`

---

### Step 4: Connection String Verification

If login fails completely, ensure database connection is working:

```bash
# From Admin project directory
dotnet run
# Should connect to: Server=115.124.106.158;Initial Catalog=Boilerplate;...
```

Check appsettings.json for correct connection string.

---

### Step 5: Enable Additional Logging

To see more detail, enable Microsoft.AspNetCore logging:

1. Open `appsettings.Development.json` in Admin project
2. Find or add:
```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.AspNetCore": "Debug",
    "Microsoft.AspNetCore.Authentication": "Debug"
  }
}
```

3. Rebuild and run

This will show authentication/authorization decisions in logs.

---

## Key Code Locations

- **Login page:** [src/SmartWorkz.StarterKitMVC.Admin/Pages/Account/Login.cshtml.cs](src/SmartWorkz.StarterKitMVC.Admin/Pages/Account/Login.cshtml.cs)
  - Line 120: `return LocalRedirect(redirectUrl);`

- **Cookie configuration:** [src/SmartWorkz.StarterKitMVC.Admin/Program.cs](src/SmartWorkz.StarterKitMVC.Admin/Program.cs)
  - Line 54: `options.Cookie.SameSite = SameSiteMode.Lax;`

- **Dashboard page:** [src/SmartWorkz.StarterKitMVC.Admin/Pages/Dashboard/Index.cshtml.cs](src/SmartWorkz.StarterKitMVC.Admin/Pages/Dashboard/Index.cshtml.cs)
  - Line 7: `[Authorize(Policy = "RequireAdmin")]`

- **Authorization policy:** [src/SmartWorkz.StarterKitMVC.Admin/Program.cs](src/SmartWorkz.StarterKitMVC.Admin/Program.cs)
  - Line 62-64: Admin policy definition

---

## Quick Test: Manual Database Check

If logs show login failing, check if user exists and has admin role:

```sql
USE Boilerplate;

-- Check if admin user exists
SELECT UserId, Email, PasswordHash 
FROM Auth.Users 
WHERE Email = 'admin@smartworkz.test';

-- Check if user has admin role
SELECT u.Email, r.Name 
FROM Auth.Users u
JOIN Auth.UserRoles ur ON u.UserId = ur.UserId
JOIN Auth.Roles r ON ur.RoleId = r.RoleId
WHERE u.Email = 'admin@smartworkz.test';
```

---

## Common Solutions

| Symptom | Solution |
|---------|----------|
| Login page shows again after submit | Update `SameSite=Lax` in Program.cs, rebuild |
| 403 Access Denied on Dashboard | Verify user has "admin" role in database |
| 404 Dashboard not found | Check page path: `/Pages/Dashboard/Index.cshtml` exists |
| Login fails immediately | Check test user password hash matches database |

---

## Next Steps

1. **Check the log files** (Logs/ directory) for detailed messages
2. **Use Browser DevTools** to see actual HTTP responses
3. **Compare with Public site** - it uses similar cookie auth but redirects to home page
4. **Report findings** with:
   - Exact HTTP status code returned
   - Login form response (302 or 200?)
   - Any error messages shown
   - Log file excerpts
