# Password Hash Format Issue - Fix Guide

**Date:** 2026-03-31  
**Issue:** Test user passwords in seed data use wrong hash format

---

## Problem

❌ **Seed Data Hash Format:** `PBKDF2$HMACSHA256$10000$salt$hash` (old format)
✅ **PasswordHasher Format:** `salt.hash` (Base64 encoded, .NET format)

The `PasswordHasher` class in `src/SmartWorkz.StarterKitMVC.Infrastructure/Services/PasswordHasher.cs` uses a different format than what's in the database seed script.

**Result:** Login with test users FAILS because `PasswordHasher.Verify()` expects the new format but finds the old format in the database.

---

## Root Cause

The seed data (008_SeedTestUsers.sql) was created with a hardcoded hash in the wrong format:

```sql
-- WRONG FORMAT (hardcoded, non-working)
'PBKDF2$HMACSHA256$10000$h9U5e2VfkJ8=$G9UjC5xF2p8K/F3vH8kL4m2nP9qR6sT1vW3xY5zB7cD9eF2gH4iJ6kL8mN0oP2qR'
```

PasswordHasher expects:
```csharp
// CORRECT FORMAT (salt.hash in Base64)
"AbCdEfGhIjKlMnOpQrStUvWx==.XyZ0aB1cD2eF3gH4iJ5kL6mN7oP8qR9sT0uV1wX2yZ3aB4cD5eF6gH7iJ8k"
```

---

## How PasswordHasher Works

### Hashing (Registration)
```csharp
public string Hash(string password)
{
    // 1. Generate random 16-byte salt
    var salt = RandomNumberGenerator.GetBytes(16);
    
    // 2. Derive hash using PBKDF2-SHA256 (100,000 iterations)
    var hash = Rfc2898DeriveBytes.Pbkdf2(
        Encoding.UTF8.GetBytes(password),
        salt,
        100000,  // iterations
        HashAlgorithmName.SHA256,
        32  // hash size in bytes
    );
    
    // 3. Return: salt.hash (both Base64 encoded)
    return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
}
```

### Verification (Login)
```csharp
public bool Verify(string password, string passwordHash)
{
    // 1. Split on the dot
    var parts = passwordHash.Split('.');
    if (parts.Length != 2)
        return false;  // Wrong format = false
    
    // 2. Extract salt and stored hash
    var salt = Convert.FromBase64String(parts[0]);
    var storedHash = Convert.FromBase64String(parts[1]);
    
    // 3. Recompute hash with same salt
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

---

## Solution

### Option 1: Generate Real Hashes (RECOMMENDED)

Use the PowerShell script to generate actual PBKDF2 hashes:

```powershell
# Generate hash for "TestPassword123!"
.\database\v1\GeneratePasswordHashes.ps1 "TestPassword123!"
```

Output will show:
```
DECLARE @PasswordHash1 NVARCHAR(MAX) = 'AbCdEfGhIjKlMnOpQrStUvWx==.XyZ0aB1cD2eF3gH4iJ5kL6mN7oP8qR9sT0uV1wX2yZ3aB4cD5eF6gH7iJ8k'
```

Then replace in 008_SeedTestUsers.sql:
```sql
-- Before
PasswordHash = 'PBKDF2$HMACSHA256$10000$...'

-- After
PasswordHash = 'AbCdEfGhIjKlMnOpQrStUvWx==.XyZ0aB1cD2eF3gH4iJ5kL6mN7oP8qR9sT0uV1wX2yZ3aB4cD5eF6gH7iJ8k'
```

### Option 2: User Registers During Test

Don't seed test users with passwords. Instead:

1. Deploy database with empty/placeholder users
2. Use RegisterAsync API endpoint to create test users
3. PasswordHasher automatically generates correct hash on registration

```csharp
// API call (create proper test users)
POST /api/auth/register
{
    "email": "admin@smartworkz.test",
    "username": "admin",
    "password": "TestPassword123!",
    "displayName": "Admin User"
}
```

### Option 3: Update Database After Deployment

Run SQL update after getting the hash:

```sql
-- Get the actual hash from running the app
DECLARE @Hash NVARCHAR(MAX) = 'ActualHashFromPasswordHasher'

UPDATE Auth.Users
SET PasswordHash = @Hash
WHERE Email IN ('admin@smartworkz.test', 'manager@smartworkz.test', 'staff@smartworkz.test', 'customer@smartworkz.test')
```

---

## Recommended Fix Path

### Step 1: Generate Hashes (One-time)
```powershell
.\database\v1\GeneratePasswordHashes.ps1 "TestPassword123!" | Tee-Object -FilePath hashes.txt
```

### Step 2: Update Seed Script
Copy the generated hashes and update 008_SeedTestUsers.sql:
```sql
-- Replace PLACEHOLDER_HASH with actual output
DECLARE @PasswordHash NVARCHAR(MAX) = 'OUTPUT_FROM_STEP_1'
```

### Step 3: Redeploy Database
```powershell
.\database\QUICK-DEPLOY.ps1 -ServerName ".\SQLEXPRESS" -IntegratedAuth
```

### Step 4: Test Login
```bash
POST /api/auth/login
{
    "email": "admin@smartworkz.test",
    "password": "TestPassword123!",
    "tenantId": "DEFAULT"
}
```

Expected: ✅ Login successful with JWT token

---

## Files Affected

- `database/v1/008_SeedTestUsers.sql` - **NEEDS UPDATE** - Invalid placeholder hashes
- `src/SmartWorkz.StarterKitMVC.Infrastructure/Services/PasswordHasher.cs` - Correct (no changes needed)
- `src/SmartWorkz.StarterKitMVC.Infrastructure/Services/AuthService.cs` - Correct (uses PasswordHasher.Verify)

---

## Verification After Fix

### Check hash format in database
```sql
SELECT Email, SUBSTRING(PasswordHash, 1, 50) AS HashPrefix
FROM Auth.Users
WHERE Email LIKE '%smartworkz.test%'
```

Expected: Should see dots (.) in hash, not dollar signs ($)

### Test login
```csharp
var result = _passwordHasher.Verify("TestPassword123!", storedHash);
// Should be: true ✅
```

---

## Why This Matters

The PasswordHasher format is:
- ✅ PBKDF2-SHA256 with 100,000 iterations (secure)
- ✅ Random salt per user (prevents rainbow tables)
- ✅ Constant-time comparison (prevents timing attacks)
- ✅ Standard .NET format

The old hardcoded hash format was:
- ❌ Hardcoded (same for all users)
- ❌ Not verifiable by current code
- ❌ Incompatible with PasswordHasher

---

## Implementation Timeline

| Action | Time | Dependencies |
|--------|------|--------------|
| Generate hashes | 5 min | PowerShell + .NET runtime |
| Update SQL script | 2 min | Text editor |
| Redeploy database | 2 min | QUICK-DEPLOY.ps1 |
| Test login | 2 min | Running application |
| **Total** | **~11 min** | - |

---

## Alternative: Simple Test Without Database Users

If you want to test without seeding users:

```csharp
// In test/development code
var hasher = new PasswordHasher();
var hash = hasher.Hash("TestPassword123!");

// This hash can now be used in RegisterAsync or direct DB insert
// var testUser = new User { PasswordHash = hash, ... };
```

---

**Status:** 🔴 NEEDS FIX before login will work

Next step: Generate actual hashes using GeneratePasswordHashes.ps1 and update 008_SeedTestUsers.sql
