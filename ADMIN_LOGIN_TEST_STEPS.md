# Admin Login Test - Step by Step

## Prerequisites

### Database Setup
Verify test user exists with correct data:

```sql
-- Check user exists
SELECT UserId, Email, PasswordHash, IsActive, IsDeleted, TenantId
FROM Auth.Users
WHERE Email = 'admin@smartworkz.test';

-- Should return 1 row with:
-- - PasswordHash: k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=
-- - IsActive: 1
-- - IsDeleted: 0
-- - TenantId: DEFAULT
```

### Verify User Has Admin Role

```sql
-- Check user has admin role
SELECT u.Email, r.Name, ur.TenantId
FROM Auth.Users u
JOIN Auth.UserRoles ur ON u.UserId = ur.UserId
JOIN Auth.Roles r ON ur.RoleId = r.RoleId
WHERE u.Email = 'admin@smartworkz.test' AND r.Name = 'Admin';

-- Should return 1 row with Role = 'Admin'
```

---

## Step 1: Build the Application

```bash
cd "s:/02_Projects/Starter/03_Development/MVC/SmartWorkz.StarterKitMVC"
dotnet build
```

**Expected**: Build succeeds with 0 errors

---

## Step 2: Start the Admin Application

```bash
cd "src/SmartWorkz.StarterKitMVC.Admin"
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to quit.
```

---

## Step 3: Open Browser and Navigate to Login

1. Open **Chrome/Edge/Firefox**
2. Navigate to: `https://localhost:5001/Account/Login`
3. You should see the login page with:
   - "SmartWorkz" header
   - "Admin Portal" subtitle
   - Email input field
   - Password input field
   - Sign In button

---

## Step 4: Open Developer Tools

Press **F12** to open DevTools:
1. Click **Network** tab
2. Leave it open during login
3. This lets us see the HTTP requests/responses

---

## Step 5: Enter Credentials

Fill in the login form:
- **Email**: `admin@smartworkz.test`
- **Password**: `TestPassword123!`

Do NOT click Sign In yet - prepare DevTools first.

---

## Step 6: Monitor Network During Login

1. In DevTools **Network** tab, make sure recording is on (red dot)
2. Click **Sign In** button
3. Watch the network requests in real-time

**You should see**:
1. POST request to `/Account/Login`
2. In **Response** tab, check the Status Code (should be **302**)
3. In **Headers** tab, find **Location**: `/Dashboard`

---

## Step 7: Verify Redirect Happens

After submitting login:

### ✅ SUCCESS PATH
1. Network shows **302 Redirect** to `/Dashboard`
2. Page automatically redirects
3. URL changes to `https://localhost:5001/Dashboard`
4. Dashboard page loads with user info visible

### ❌ FAILURE PATH #1: Login Error
1. Network shows **200 OK** (not 302)
2. You're back on login page
3. Red error box shows message:
   - "Invalid credentials" → Wrong password/email
   - "Access Denied" → User doesn't have admin role
   - "An error occurred during login" → Server error

**Fix**: Review troubleshooting guide

### ❌ FAILURE PATH #2: Redirect but No Page
1. Network shows **302** with Location `/Dashboard`
2. Browser follows redirect
3. But then shows **403 Forbidden** or **401 Unauthorized**
4. Or shows blank page / "Not Authorized"

**Cause**: Authorization policy failed after redirect  
**Fix**: Check if role claim is present (see step 8)

---

## Step 8: Check Cookie Was Created

In DevTools:
1. Click **Application** tab
2. Expand **Cookies** on left
3. Click `https://localhost:5001`
4. Look for `.Admin.Auth` cookie

**Should see**:
- Name: `.Admin.Auth`
- Value: Long encrypted string
- HttpOnly: ✓ (checked)
- Secure: ✓ (checked) if HTTPS
- SameSite: `Strict`

If `.Admin.Auth` cookie is missing → SignInAsync didn't succeed

---

## Step 9: Verify User Is Authenticated

At the Dashboard page (`https://localhost:5001/Dashboard`):

### If You See Dashboard Content
- Title: "Dashboard" or similar
- Statistics or welcome message
- User profile info somewhere on page

**✅ SUCCESS** - Login and redirect worked!

### If You See Access Denied
- Message: "Access Denied" or "401 Unauthorized" or "403 Forbidden"
- User is authenticated but authorization failed

**Cause**: Missing admin role claim  
**Fix**: Role wasn't added to claims during SignInAsync

---

## Step 10: Check Application Logs

Look at the console where you ran `dotnet run`:

### Expected Log Messages
```
info: SmartWorkz.StarterKitMVC.Admin.Pages.Account.LoginModel[0]
      Admin admin@smartworkz.test logged in successfully. UserId=..., TenantId=DEFAULT, Roles=admin

info: SmartWorkz.StarterKitMVC.Admin.Pages.Account.LoginModel[0]
      Redirecting to /Dashboard. Authentication complete
```

### Error Messages to Watch For
```
warn: SmartWorkz.StarterKitMVC.Admin.Pages.Account.LoginModel[0]
      Failed admin login for admin@smartworkz.test

warn: SmartWorkz.StarterKitMVC.Admin.Pages.Account.LoginModel[0]
      Non-admin login attempt for admin@smartworkz.test

fail: SmartWorkz.StarterKitMVC.Admin.Pages.Account.LoginModel[0]
      Error during sign-in for admin@smartworkz.test
      System.Exception: ...
```

If you see error messages, that's the issue to debug.

---

## Complete Test Scenario

### Scenario 1: Successful Admin Login
**Setup**: User `admin@smartworkz.test` with Admin role  
**Steps**: Follow Step 5-7  
**Expected**: 302 redirect to `/Dashboard`, page loads

### Scenario 2: Wrong Password
**Setup**: Same user  
**Enter**: Correct email, **wrong password**  
**Expected**: Login page with "Invalid credentials" error

### Scenario 3: Wrong Email
**Setup**: Same user  
**Enter**: **Wrong email**, correct password  
**Expected**: Login page with "Invalid credentials" error

### Scenario 4: Non-Admin User
**Setup**: User `manager@smartworkz.test` with Manager role  
**Enter**: Correct credentials  
**Expected**: Login page with "Access Denied" error

---

## Quick Debug: Print Claims

If dashboard loads but seems "wrong", add this to `Dashboard/Index.cshtml.cs`:

```csharp
public void OnGet()
{
    _logger.LogInformation("User claims: {Claims}", 
        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
    
    // Rest of method...
}
```

Then check logs for claim list. You should see:
- `nameidentifier=<UserId>`
- `emailaddress=admin@smartworkz.test`
- `role=admin`
- `TenantId=DEFAULT`

---

## Success Indicators

You'll know login is working when:

1. ✅ Login page loads at `https://localhost:5001/Account/Login`
2. ✅ Form accepts email + password without errors
3. ✅ Clicking Sign In shows 302 redirect in DevTools Network tab
4. ✅ `.Admin.Auth` cookie appears in DevTools Cookies
5. ✅ Browser auto-redirects to `/Dashboard`
6. ✅ Dashboard page loads with user content visible
7. ✅ Application logs show "logged in successfully" message
8. ✅ No 403/401/Access Denied errors

---

## Still Stuck?

Follow **LOGIN_REDIRECT_TROUBLESHOOTING.md** for detailed diagnosis.

Common issues in order of likelihood:
1. User doesn't have Admin role
2. Cookie not set (SignInAsync error)
3. Role claim not added to claims
4. Authorization policy config issue
5. TenantId not in claims

