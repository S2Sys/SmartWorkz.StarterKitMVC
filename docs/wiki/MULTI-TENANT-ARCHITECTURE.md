# Multi-Tenant Architecture — Visual Guide

Quick reference diagrams for understanding tenant isolation and login flow.

## Entity Relationships

```
┌──────────────────────────────────────────────────────────────┐
│                      MASTER SCHEMA                           │
├──────────────────────────────────────────────────────────────┤
│ Tenants                                                        │
│ ├─ TenantId (PK)                                             │
│ ├─ Name, DisplayName, IsActive                              │
│ └─ Has many: Products, Categories, Translations, etc.       │
└──────────────────────────────────────────────────────────────┘
                            ↓
                   (N-to-N via TenantUser)
                            ↓
┌──────────────────────────────────────────────────────────────┐
│                       AUTH SCHEMA                            │
├──────────────────────────────────────────────────────────────┤
│ Users                                                         │
│ ├─ UserId (PK)                                              │
│ ├─ Email, PasswordHash, TenantId (CRUCIAL)                 │
│ ├─ IsActive, DisplayName, Locale                           │
│ └─ Has many: UserRoles, UserPermissions, RefreshTokens    │
│                                                              │
│ TenantUsers (Pivot)                                         │
│ ├─ TenantUserId (PK)                                        │
│ ├─ TenantId (FK → Tenant)                                  │
│ ├─ UserId (FK → User)                                      │
│ └─ Status, InvitedAt, AcceptedAt                          │
└──────────────────────────────────────────────────────────────┘
```

## Login Sequence Diagram

```
┌─────────────┐                              ┌──────────────┐
│   Browser   │                              │   Database   │
└──────┬──────┘                              └──────┬───────┘
       │                                            │
       │ 1. Submit Form                             │
       │  (email, password, tenantId)               │
       ├───────────────────────────────────────────>│
       │                                            │
       │                                   Query:   │
       │                              SELECT * FROM Users
       │                              WHERE Email = ?
       │                              AND TenantId = ?
       │                                            │
       │                      2. User Record Found  │
       │<───────────────────────────────────────────┤
       │                                            │
       │ 3. Verify Password Hash                    │
       │    (in memory)                             │
       │                                            │
       │    ✅ Match? Yes!                          │
       │                                            │
       │    4. Generate JWT                        │
       │       (includes tenantId claim)            │
       │<───────────────────────────────────────────┤
       │                                            │
       │ 5. Store JWT in Cookie/LocalStorage        │
       │                                            │
       │ 6. Redirect to /Dashboard                  │
       │                                            │
       ├───────────────────────────────────────────>│
       │  GET /Dashboard                            │
       │  Authorization: Bearer JWT                 │
       │                                            │
       │                    7. Validate JWT         │
       │                       Extract TenantId     │
       │                                            │
       │                Query Dashboard Data:       │
       │                SELECT * FROM ...           │
       │                WHERE TenantId = JWT.tenant│
       │<───────────────────────────────────────────┤
       │                                            │
       │ 8. Dashboard Rendered                      │
       │    (only user's tenant data)               │
       │                                            │
```

## Login Failure Scenarios

### Scenario A: User Not Found in Tenant

```
Input:  Email = "john@example.com"
        TenantId = "ACME"
        
Query:  SELECT * FROM Users 
        WHERE Email = "john@example.com" 
        AND TenantId = "ACME"
        
Result: ❌ NULL (user doesn't exist in ACME)

Output: "Invalid credentials"
        (don't reveal if user exists in other tenants!)
```

### Scenario B: User Exists in Tenant But Wrong Password

```
Input:  Email = "john@example.com"
        Password = "wrong_password"
        TenantId = "ACME"
        
Query:  SELECT * FROM Users 
        WHERE Email = "john@example.com" 
        AND TenantId = "ACME"
        
Result: ✅ Found User (USR-1)
        
Password Check: Hash("wrong_password") ≠ user.PasswordHash

Output: "Invalid credentials"
```

### Scenario C: User Account Inactive

```
Input:  Email = "john@example.com"
        Password = "correct_password"
        TenantId = "ACME"
        
Query:  SELECT * FROM Users 
        WHERE Email = "john@example.com" 
        AND TenantId = "ACME"
        
Result: ✅ Found User (USR-1)

Password Check: ✅ Verified

IsActive Check: ❌ user.IsActive = false

Output: "Account is inactive"
```

## Data Isolation Pattern

### Example: Getting Products

```
┌────────────────────────────────────────────────┐
│ Current User's JWT Token                       │
│ {                                              │
│   "sub": "USR-1",                              │
│   "tenant_id": "ACME",  ← CRITICAL             │
│   "email": "john@example.com",                 │
│   "roles": ["Admin"]                           │
│ }                                              │
└────────────────────────────────────────────────┘
                    ↓
        Extract TenantId from JWT
        tenantId = "ACME"
                    ↓
    Query Products for THIS tenant:
    
    SELECT * FROM Products
    WHERE TenantId = "ACME"
                    ↓
    ┌──────────────────────────────────────────┐
    │ PRODUCTS DATABASE STATE                  │
    ├──────────────────────────────────────────┤
    │ TenantId │ ProductId │ Name               │
    ├──────────┼───────────┼────────────────────┤
    │ ACME     │ PROD-1    │ Widget A     ✅    │
    │ ACME     │ PROD-2    │ Widget B     ✅    │
    │ GlobalTech│ PROD-3   │ Gadget X     ❌   │
    │ LocalSvc │ PROD-4    │ Tool Z       ❌   │
    │ ACME     │ PROD-5    │ Widget C     ✅    │
    └──────────────────────────────────────────┘
                    ↓
    Result: Only ACME products returned
    (Products from other tenants filtered out)
```

## Wrong vs Right Implementation

### ❌ WRONG: Single Users Table, Global Login

```
Users Table (Wrong)
┌────────┬──────────────────┬──────────────────┐
│ UserId │ Email            │ PasswordHash     │
├────────┼──────────────────┼──────────────────┤
│ 1      │ john@example.com │ hash_ABC_123     │
│ 2      │ jane@example.com │ hash_DEF_456     │
│ 3      │ john@example.com │ hash_XYZ_789     │ ← Conflict!
└────────┴──────────────────┴──────────────────┘

Problems:
❌ Same email in multiple rows (confusing)
❌ Which hash to verify on login?
❌ No tenant isolation
❌ User 1 & 3 could see each other's data
```

**Login Query:**
```sql
-- ❌ This returns BOTH john records, ambiguous!
SELECT * FROM Users WHERE Email = 'john@example.com'
```

### ✅ RIGHT: Users Scoped to Tenants

```
Users Table (Right)
┌────────┬──────────────────┬──────────┬────────────────┐
│ UserId │ Email            │ TenantId │ PasswordHash   │
├────────┼──────────────────┼──────────┼────────────────┤
│ 1      │ john@example.com │ ACME     │ hash_ABC_123   │
│ 2      │ jane@example.com │ ACME     │ hash_DEF_456   │
│ 3      │ john@example.com │ GlobalTech│ hash_XYZ_789  │
└────────┴──────────────────┴──────────┴────────────────┘

Unique Constraint: (Email, TenantId)
Benefits:
✅ Same email allowed in different tenants
✅ Unambiguous password verification per tenant
✅ Clear data isolation
✅ Multiple login scenarios supported
```

**Login Query:**
```sql
-- ✅ This returns exactly ONE record
SELECT * FROM Users 
WHERE Email = 'john@example.com' 
AND TenantId = 'ACME'
```

## Multi-Tenant Login Decision Tree

```
                    User Submits Login
                   (email, password)
                           │
                           ↓
                ┌─────────────────────────┐
                │ Is TenantId provided?   │
                └────┬──────────────┬─────┘
                     │ YES           │ NO
                     ↓               ↓
              Use provided      Auto-detect from:
              TenantId          • Subdomain
                     │          • Email domain
                     │          • User selection
                     │               │
                     └───────┬───────┘
                             ↓
                  Query Users WHERE
                  Email = input.email
                  AND TenantId = detected_tenant
                             │
                ┌────────────┴────────────┐
                │ User Found?             │
                └─────┬──────────┬────────┘
                    YES           NO
                      │            │
                      ↓            ↓
            ┌──────────────────┐  │
            │ Verify Password  │  │
            └────┬──────┬──────┘  │
                YES     NO        │
                 │       │        │
                 │       └────────┼────────────┐
                 ↓                ↓            ↓
             Generate JWT   ❌ Invalid Credentials
             Set TenantId    (don't reveal which)
             in token
                 │
                 ↓
            ✅ Login Success
            Return JWT
            Redirect to /Dashboard
```

## TenantId in JWT

```
┌─────────────────────────────────────────────┐
│ Access Token (JWT) in Authorization Header  │
├─────────────────────────────────────────────┤
│ Header:                                      │
│ {                                            │
│   "alg": "HS256",                           │
│   "typ": "JWT"                              │
│ }                                            │
│                                              │
│ Payload: (Claims)                            │
│ {                                            │
│   "sub": "USR-1",       ← User ID           │
│   "email": "john@...",                      │
│   "tenant_id": "ACME",  ← ⭐ CRITICAL      │
│   "roles": ["Admin"],                       │
│   "permissions": [                          │
│     "products:read",                        │
│     "products:write"                        │
│   ],                                         │
│   "locale": "en",                           │
│   "iat": 1704067200,    ← Issued At        │
│   "exp": 1704070800     ← Expires In 60min │
│ }                                            │
│                                              │
│ Signature:                                   │
│ HMACSHA256(                                 │
│   base64UrlEncode(header) + "." +           │
│   base64UrlEncode(payload),                 │
│   secret                                    │
│ )                                            │
└─────────────────────────────────────────────┘

How Tenant Context is Preserved:
1. User logs in to Tenant A
2. JWT includes tenant_id = "A"
3. Token sent with every request
4. Server extracts tenant_id from JWT
5. All queries filtered by that tenant_id
6. User can only see Tenant A data
```

## Common Mistakes (Visual)

### Mistake 1: No TenantId in User Query

```
❌ WRONG:
SELECT * FROM Users WHERE Email = ?

Results: Returns USR-1 (ACME), USR-2 (GlobalTech)
         Which password to verify? Ambiguous!

✅ RIGHT:
SELECT * FROM Users 
WHERE Email = ? AND TenantId = ?

Results: Returns only the specific user record
```

### Mistake 2: Querying Data Without TenantId Filter

```
❌ WRONG:
SELECT * FROM Products

Results: Shows ALL products from ALL tenants
         Data leak!

✅ RIGHT:
SELECT * FROM Products 
WHERE TenantId = (from JWT)

Results: Shows only current tenant's products
```

### Mistake 3: Hardcoded TenantId

```
❌ WRONG:
SELECT * FROM Products 
WHERE TenantId = 'ACME'

Results: Always returns ACME data
         Works for ACME user, fails for GlobalTech user

✅ RIGHT:
SELECT * FROM Products 
WHERE TenantId = @tenantId

Usage:
  await GetProductsAsync(user.TenantId)  // From JWT
  await GetProductsAsync(currentTenantId) // From context
```

## See Also

- [Multi-Tenant Login Flow](./06-multi-tenant-login-flow.md) — Detailed explanation
- [Base Page Pattern](./03-base-page-pattern.md) — TenantId in page context
- [AuthService.cs](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Services/AuthService.cs) — Implementation
