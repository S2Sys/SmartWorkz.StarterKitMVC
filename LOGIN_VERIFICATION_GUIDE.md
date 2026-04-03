# Login Verification Guide

## Password Hash Details

**Algorithm:** PBKDF2-SHA256  
**Iterations:** 100,000  
**Salt Size:** 16 bytes  
**Hash Size:** 32 bytes  
**Format:** `{Base64Salt}.{Base64Hash}`

## Test User Hash

**Password:** `TestPassword123!`  
**Hash:** `k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=`

Breakdown:
- Salt (Base64): `k23Gu+N1T4pqRO1hJHpuzw==` (decodes to 16 bytes)
- Hash (Base64): `iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=` (decodes to 32 bytes)

## Test Users (Created by migrations)

All test users are created in the `Auth.Users` table by migration `008_SeedTestUsers.sql` with the above hash.

| Email | Password | TenantId | Role |
|-------|----------|----------|------|
| admin@smartworkz.test | TestPassword123! | DEFAULT | Admin |
| manager@smartworkz.test | TestPassword123! | DEFAULT | Manager |
| staff@smartworkz.test | TestPassword123! | DEFAULT | Staff |
| customer@smartworkz.test | TestPassword123! | DEFAULT | Customer |

## Login Flow

1. **User enters email and password**
   - Example: `admin@smartworkz.test` / `TestPassword123!`

2. **AuthService.LoginAsync** calls:
   ```csharp
   var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);
   
   if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
       return Result.Fail<LoginResponse>(MessageKeys.Auth.InvalidCredentials);
   ```

3. **PasswordHasher.Verify** does:
   ```csharp
   var parts = passwordHash.Split('.');  // Split salt.hash
   var salt = Convert.FromBase64String(parts[0]);  // Decode salt
   var storedHash = Convert.FromBase64String(parts[1]);  // Decode hash
   
   // Re-derive hash using entered password + stored salt
   var inputHash = Rfc2898DeriveBytes.Pbkdf2(
       Encoding.UTF8.GetBytes(request.Password),  // "TestPassword123!"
       salt,
       100_000,  // iterations
       HashAlgorithmName.SHA256,
       32  // hash size
   );
   
   // Compare using constant-time comparison
   return CryptographicOperations.FixedTimeEquals(storedHash, inputHash);
   ```

4. **If verification passes:**
   - Check `IsActive` flag
   - Check lockout status
   - Generate JWT access token
   - Create refresh token
   - Return login response

## How to Test Password Verification

### Using C# REPL or Unit Test

```csharp
var hasher = new PasswordHasher();
var password = "TestPassword123!";
var hash = "k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=";

bool isValid = hasher.Verify(password, hash);
Console.WriteLine($"Password valid: {isValid}");  // Should print: true
```

### Using SQL Query

```sql
USE Boilerplate;

SELECT 
    Email,
    PasswordHash,
    IsActive,
    TenantId,
    CreatedAt
FROM Auth.Users
WHERE Email = 'admin@smartworkz.test'
  AND IsDeleted = 0;
```

Expected result:
- Email: `admin@smartworkz.test`
- PasswordHash: `k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=`
- IsActive: 1
- TenantId: `DEFAULT`
- CreatedAt: (timestamp from migration run)

## Troubleshooting Login Failures

### Error: "Could not find user"
- ✓ User doesn't exist in `Auth.Users` table
- ✓ User is deleted (`IsDeleted = 1`)
- **Fix:** Run `QUICK-DEPLOY.ps1` to seed test users

### Error: "Invalid credentials"
- ✓ Password hash doesn't match
- ✓ User password was never hashed correctly
- **Verify:** Run the C# verification test above

### Error: "Account inactive"
- ✓ `IsActive = 0` in database
- **Fix:** Check user record has `IsActive = 1`

### Error: "Account locked"
- ✓ `LockoutEnabled = 1` AND `LockoutEnd > NOW`
- **Fix:** Clear lockout: `UPDATE Auth.Users SET LockoutEnd = NULL WHERE Email = '...'`

## Hash Format Validation

The seed data uses this exact format:
```
Salt.Hash
├─ Salt: Base64 encoded (16 bytes = 24 chars + padding)
└─ Hash: Base64 encoded (32 bytes = 44 chars)
```

**Valid example:**
```
k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=
```

**Invalid examples:**
```
plain_password                          ❌ Not hashed
ABC123==.DEF456==                       ❌ Hash too short
k23Gu+N1T4pqRO1hJHpuzw                  ❌ Missing hash portion
k23Gu+N1T4pqRO1hJHpuzw==|HASH           ❌ Wrong delimiter (| instead of .)
```

## Migration Status

**Required migrations:** 000-010  
**Test user creation:** Migration 008 (`008_SeedTestUsers.sql`)  
**Hash format:** PBKDF2-SHA256, 100,000 iterations, salt.hash in Base64

Before any login attempt, run:
```powershell
cd database
.\QUICK-DEPLOY.ps1 -ServerName "115.124.106.158" -DatabaseName "Boilerplate" -Username "zenthil" -Password "PinkPanther#1"
```

This ensures all tables, stored procedures, and test users are created with the correct password hash.
