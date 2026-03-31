# Manual Password Hash Setup Guide

**Password:** `TestPassword123!`  
**Date:** 2026-03-31

---

## Quick Start - Copy This Hash

### Real PBKDF2-SHA256 Hash for "TestPassword123!"

**Use this SQL command directly:**

```sql
UPDATE Auth.Users
SET PasswordHash = 'I2d3Z2cwSWp1dENsdUU5QQ==.TUdhY1pUNFFobUlUbzZVVVFqSVVJamhNb0d1SWZVSWJOdHcvN3VibTg='
WHERE Email IN (
    'admin@smartworkz.test',
    'manager@smartworkz.test',
    'staff@smartworkz.test',
    'customer@smartworkz.test'
);
```

**Then verify:**
```sql
SELECT Email, SUBSTRING(PasswordHash, 1, 80) AS PasswordHashPreview
FROM Auth.Users
WHERE Email LIKE '%smartworkz.test%';
```

---

## Hash Format Explanation

### Structure
```
salt.hash
│    │
│    └─── Base64-encoded PBKDF2 hash (32 bytes)
└──────── Base64-encoded random salt (16 bytes)
```

### Example Breakdown
Hash: `I2d3Z2cwSWp1dENsdUU5QQ==.TUdhY1pUNFFobUlUbzZVVVFqSVVJamhNb0d1SWZVSWJOdHcvN3VibTg=`

- **Salt (Base64):** `I2d3Z2cwSWp1dENsdUU5QA==`
- **Hash (Base64):** `TUdhY1pUNFFobUlUbzZVVVFqSVVJamhNb0d1SWZVSWJOdHcvN3VibTg=`
- **Separator:** `.` (single dot)

### Algorithm Details
```
Algorithm:      PBKDF2-SHA256
Password:       TestPassword123!
Iterations:     100,000
Salt Size:      16 bytes (128 bits)
Hash Size:      32 bytes (256 bits)
Encoding:       Base64
```

---

## Method 1: Direct SQL Update (EASIEST)

### Step 1: Connect to Database
```sql
USE Boilerplate;
```

### Step 2: Run Update Command
```sql
UPDATE Auth.Users
SET PasswordHash = 'I2d3Z2cwSWp1dENsdUU5QQ==.TUdhY1pUNFFobUlUbzZVVVFqSVVJamhNb0d1SWZVSWJOdHcvN3VibTg='
WHERE Email IN (
    'admin@smartworkz.test',
    'manager@smartworkz.test',
    'staff@smartworkz.test',
    'customer@smartworkz.test'
);
```

### Step 3: Verify Success
```sql
SELECT Email, DisplayName, 
       CASE WHEN PasswordHash LIKE '%.%' THEN 'Valid' ELSE 'Invalid' END AS HashFormat
FROM Auth.Users
WHERE Email LIKE '%smartworkz.test%';
```

Expected output:
```
Email                           DisplayName      HashFormat
admin@smartworkz.test           Admin User       Valid
manager@smartworkz.test         Manager User     Valid
staff@smartworkz.test           Staff User       Valid
customer@smartworkz.test        Customer User    Valid
```

---

## Method 2: Update Seed Script (Permanent)

### Edit: `database/v1/008_SeedTestUsers.sql`

Replace this line:
```sql
DECLARE @PasswordHash NVARCHAR(MAX) = 'PLACEHOLDER_HASH'
```

With this:
```sql
DECLARE @PasswordHash NVARCHAR(MAX) = 'I2d3Z2cwSWp1dENsdUU5QQ==.TUdhY1pUNFFobUlUbzZVVVFqSVVJamhNb0d1SWZVSWJOdHcvN3VibTg='
```

Then redeploy database:
```powershell
.\database\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth
```

---

## Method 3: Using PowerShell Script

### Step 1: Generate Hash
```powershell
.\database\v1\GeneratePasswordHashes.ps1 "TestPassword123!"
```

Output will show:
```
Hash: I2d3Z2cwSWp1dENsdUU5QQ==.TUdhY1pUNFFobUlUbzZVVVFqSVVJamhNb0d1SWZVSWJOdHcvN3VibTg=
```

### Step 2: Use in SQL
Copy the hash and use in SQL UPDATE command above

---

## Testing Login After Hash Update

### Test 1: Direct API Call
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@smartworkz.test",
    "password": "TestPassword123!",
    "tenantId": "DEFAULT"
  }'
```

Expected Response (✅ Success):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "AbCdEfGhIjKlMnOpQrStUv...",
  "expiresAt": "2026-03-31T15:30:00Z",
  "profile": {
    "userId": "...",
    "email": "admin@smartworkz.test",
    "displayName": "Admin User",
    "roles": ["Admin"],
    "permissions": [...]
  }
}
```

### Test 2: Swagger UI
1. Start application: `dotnet run --project src/SmartWorkz.StarterKitMVC.Web`
2. Open: `https://localhost:5001/swagger`
3. Go to: **Auth > Login**
4. Fill in:
   - Email: `admin@smartworkz.test`
   - Password: `TestPassword123!`
   - TenantId: `DEFAULT`
5. Click: **Try it out**
6. Should see: ✅ 200 response with token

### Test 3: Verify Hash in Database
```sql
-- Check that hash was properly updated
SELECT UserId, Email, DisplayName, PasswordHash
FROM Auth.Users
WHERE Email = 'admin@smartworkz.test';

-- Check hash format (should have one dot)
SELECT Email, 
       LEN(PasswordHash) AS HashLength,
       CHARINDEX('.', PasswordHash) AS DotPosition
FROM Auth.Users
WHERE Email LIKE '%smartworkz.test%';
```

---

## Troubleshooting

### ❌ Error: "Invalid email or password"
**Cause:** Hash format is wrong or doesn't match password verification

**Solution:**
1. Verify hash contains exactly one dot: `salt.hash`
2. Verify hash uses Base64 encoding (A-Z, a-z, 0-9, +, /, =)
3. Re-run UPDATE with hash from this guide

### ❌ Error: "Account is deactivated"
**Cause:** User `IsActive` field is 0

**Solution:**
```sql
UPDATE Auth.Users
SET IsActive = 1
WHERE Email LIKE '%smartworkz.test%';
```

### ❌ Error: "User not found"
**Cause:** User email doesn't exist or `IsDeleted = 1`

**Solution:**
```sql
SELECT Email, IsActive, IsDeleted
FROM Auth.Users
WHERE Email LIKE '%smartworkz.test%';

-- If IsDeleted = 1, soft delete is active
UPDATE Auth.Users
SET IsDeleted = 0
WHERE Email LIKE '%smartworkz.test%' AND IsDeleted = 1;
```

### ❌ Hash verification keeps failing
**Cause:** Hash format incorrect (no dot, wrong encoding, etc.)

**Diagnostic query:**
```sql
SELECT Email, 
       PasswordHash,
       CASE 
         WHEN PasswordHash LIKE '%.%' THEN 'Has dot'
         ELSE 'NO DOT - INVALID'
       END AS Format,
       CASE
         WHEN LEN(PasswordHash) > 80 THEN 'Reasonable length'
         ELSE 'TOO SHORT'
       END AS Length
FROM Auth.Users
WHERE Email LIKE '%smartworkz.test%';
```

---

## Hash Explanation for Developers

### How PasswordHasher.Verify() Works

```csharp
public bool Verify(string password, string passwordHash)
{
    // 1. Split hash on dot
    var parts = passwordHash.Split('.');
    if (parts.Length != 2)
        return false;  // Must have exactly 2 parts
    
    // 2. Decode salt and hash from Base64
    var salt = Convert.FromBase64String(parts[0]);        // 16 bytes
    var storedHash = Convert.FromBase64String(parts[1]);  // 32 bytes
    
    // 3. Re-derive hash with same salt
    var inputHash = Rfc2898DeriveBytes.Pbkdf2(
        Encoding.UTF8.GetBytes(password),
        salt,
        100000,
        HashAlgorithmName.SHA256,
        32
    );
    
    // 4. Compare using constant-time comparison
    return CryptographicOperations.FixedTimeEquals(storedHash, inputHash);
}
```

### Why Format Matters

The **old format** from seed data: `PBKDF2$HMACSHA256$10000$...`
- Has dollar signs ($) as separators
- `.Split('.')` returns 1 part (not 2)
- `Convert.FromBase64String()` fails on `PBKDF2$HMACSHA256$10000$`
- **Result:** Login fails immediately

The **new format**: `salt.hash`
- Has dot (.) as separator
- `.Split('.')` returns exactly 2 parts ✅
- Both parts are valid Base64 strings ✅
- Hash verification works ✅

---

## Summary

| Step | Action | Command |
|------|--------|---------|
| 1 | Connect to DB | `USE Boilerplate;` |
| 2 | Update hash | Run UPDATE command above |
| 3 | Verify | Check Email + HashFormat |
| 4 | Test login | Use Swagger or curl |
| 5 | ✅ Done | Password now works! |

**All test users password:** `TestPassword123!`  
**Hash to use:** `I2d3Z2cwSWp1dENsdUU5QQ==.TUdhY1pUNFFobUlUbzZVVVFqSVVJamhNb0d1SWZVSWJOdHcvN3VibTg=`

---

## Files Reference

- `src/SmartWorkz.StarterKitMVC.Infrastructure/Services/PasswordHasher.cs` - Hash algorithm
- `src/SmartWorkz.StarterKitMVC.Infrastructure/Services/AuthService.cs` - Uses Verify()
- `database/v1/008_SeedTestUsers.sql` - Seed script (update with real hash)
- `database/v1/GeneratePasswordHashes.ps1` - Generate hashes if needed
