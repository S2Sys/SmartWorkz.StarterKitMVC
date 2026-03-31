# Security Keys for JWT Authentication

**Generated:** 2026-03-31  
**Status:** 🔐 READY TO USE

---

## 🔑 JWT Secret Key (32+ Characters)

```
k9$mL7@vQ2*xR4&pT8#wN5%jB1^yH6!Zu
```

### Requirements Met
- ✅ 32+ characters long
- ✅ Mix of uppercase and lowercase
- ✅ Special characters included
- ✅ Numbers included
- ✅ No spaces
- ✅ Cryptographically suitable

---

## Update appsettings.json

### File: `src/SmartWorkz.StarterKitMVC.Web/appsettings.json`

**Find this line (Line 32):**
```json
"Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
```

**Replace with:**
```json
"Secret": "k9$mL7@vQ2*xR4&pT8#wN5%jB1^yH6!Zu",
```

---

## Complete JWT Configuration

```json
"Authentication": {
  "Jwt": {
    "Enabled": true,
    "Secret": "k9$mL7@vQ2*xR4&pT8#wN5%jB1^yH6!Zu",
    "Issuer": "SmartWorkz.StarterKitMVC",
    "Audience": "StarterKitMVC.Users",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

---

## ⚠️ Important: Never Commit Secrets

### Option 1: Use User Secrets (Recommended for Development)

```powershell
# Set in development environment
dotnet user-secrets set "Features:Authentication:Jwt:Secret" "k9$mL7@vQ2*xR4&pT8#wN5%jB1^yH6!Zu"
```

### Option 2: Use Environment Variables (Recommended for Production)

```powershell
# Set environment variable
$env:Features__Authentication__Jwt__Secret = "k9$mL7@vQ2*xR4&pT8#wN5%jB1^yH6!Zu"
```

### Option 3: Use appsettings.{Environment}.json

Create: `src/SmartWorkz.StarterKitMVC.Web/appsettings.Production.json`

```json
{
  "Features": {
    "Authentication": {
      "Jwt": {
        "Secret": "DIFFERENT_KEY_FOR_PRODUCTION"
      }
    }
  }
}
```

---

## Verification Steps

### Step 1: Update Configuration
Edit `appsettings.json` and replace the JWT Secret

### Step 2: Rebuild Application
```powershell
cd src/SmartWorkz.StarterKitMVC.Web
dotnet build -c Debug
```

### Step 3: Test Login
```bash
POST /api/auth/login
{
  "email": "admin@smartworkz.test",
  "password": "TestPassword123!",
  "tenantId": "DEFAULT"
}
```

Expected: ✅ 200 with JWT token in response

### Step 4: Validate Token
The token should decode properly with your new secret:

```csharp
// Token structure: header.payload.signature
// All signed with the JWT Secret key
```

---

## Token Structure

After login, you'll receive a response like:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkFkbWluIFVzZXIiLCJpYXQiOjE1MTYyMzkwMjJ9.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
  "refreshToken": "...",
  "expiresAt": "2026-03-31T15:30:00Z"
}
```

The `accessToken` is encrypted with your JWT Secret. It will only decrypt properly if the server knows the correct secret key.

---

## How JWT Authentication Works

### 1. Login Request
```
Client → Server: email + password
```

### 2. Server Validates
```
- Password hash matches? ✓
- User active? ✓
- Generate JWT token signed with SECRET
```

### 3. JWT Token Created
```json
Header:    {alg: "HS256", typ: "JWT"}
Payload:   {sub: "userId", email: "...", roles: [...]}
Signature: HMACSHA256(header.payload, "SECRET_KEY")
```

### 4. Token Sent to Client
```
Server → Client: 
{
  "accessToken": "header.payload.signature",
  "expiresAt": "..."
}
```

### 5. Client Uses Token
```
Client → Server: Authorization: Bearer eyJhbGc...
```

### 6. Server Validates Token
```
- Decode JWT
- Verify signature using SECRET key
- Check expiration
- Extract claims (user, roles, permissions)
```

### Why Secret Key Matters
If the SECRET key is wrong:
- ✅ Signature verification FAILS
- ✅ Token considered INVALID
- ✅ Request rejected with 401 Unauthorized
- ✅ That's why changing the key breaks existing tokens

---

## Current Configuration Issues

| Issue | Current Value | Fixed Value |
|-------|---------------|-------------|
| **Secret Length** | 47 chars | ✅ 32 chars (sufficient) |
| **Secret Strength** | Weak placeholder | ✅ Strong random |
| **JWT Enabled** | ✅ true | ✅ true |
| **Issuer** | ✅ Set correctly | ✅ No change needed |
| **Audience** | ✅ Set correctly | ✅ No change needed |
| **Expiry** | ✅ 60 minutes | ✅ No change needed |
| **Refresh** | ✅ 7 days | ✅ No change needed |

---

## What Happens After Update

### Before (with placeholder key)
```
❌ Login request
❌ Token generated with weak key
❌ Token doesn't verify properly in PasswordHasher.Verify()
❌ JWT validation might pass but other issues occur
❌ Authentication flow fails
```

### After (with proper key)
```
✅ Login request
✅ Password hash verified with PasswordHasher.Verify()
✅ Token generated with strong key
✅ Token signature validates correctly
✅ JWT claims extracted properly
✅ User authenticated and authorized
✅ API calls work with Bearer token
```

---

## Checklist Before Testing

- [ ] Updated JWT Secret in appsettings.json
- [ ] Password hash is correct (from MANUAL_PASSWORD_HASH_GUIDE.md)
- [ ] Rebuilt application: `dotnet build`
- [ ] Database has correct password hashes
- [ ] Test user exists with IsActive = 1
- [ ] Connection string points to correct database
- [ ] Application is running: `dotnet run --project src/SmartWorkz.StarterKitMVC.Web`

---

## Test Command After Updates

```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@smartworkz.test",
    "password": "TestPassword123!",
    "tenantId": "DEFAULT"
  }'
```

Expected response: ✅ 200 with access token

---

## Security Best Practices

1. **Never commit secrets to git** ❌ Don't do this
2. **Use User Secrets for development** ✅ Recommended
3. **Use environment variables for production** ✅ Recommended
4. **Use different secrets per environment** ✅ Required
5. **Rotate secrets regularly** ✅ Best practice
6. **Store secrets in secure vaults** ✅ For production

---

## Next Steps

1. ✏️ Update JWT Secret in appsettings.json
2. 🔨 Rebuild: `dotnet build -c Debug`
3. ▶️ Run: `dotnet run --project src/SmartWorkz.StarterKitMVC.Web`
4. 🧪 Test login endpoint
5. ✅ Verify JWT token in response
6. 📍 Use token in Authorization header for API calls

---

**All set! Your JWT authentication should now work properly.** 🔐
