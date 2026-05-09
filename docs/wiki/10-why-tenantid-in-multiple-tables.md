# Why TenantId Appears in Multiple Tables

Understanding why the same TenantId appears in Users table AND TenantUsers table.

## Quick Answer

**TenantId is in both tables because they serve different purposes:**

| Table | TenantId Purpose | Why Needed |
|-------|------------------|-----------|
| **Users** | Direct ownership | Fast user authentication per tenant |
| **TenantUsers** | Mapping/Membership | Track which users belong to which tenants |

Think of it like this:
- **Users.TenantId** = "This user belongs to Tenant A"
- **TenantUsers** = "This is the proof/record that user belongs to Tenant A"

---

## The Three Possible Designs

### ❌ Design 1: TenantId ONLY in Users (Redundant)

```
Users Table:
┌────────┬──────────────┬──────────┐
│ UserId │ Email        │ TenantId │
├────────┼──────────────┼──────────┤
│ USR-1  │ john@ex.com  │ ACME     │
└────────┴──────────────┴──────────┘

TenantUsers Table (pointless):
┌────────────────┬──────────┬────────┐
│ TenantUserId   │ TenantId │ UserId │
├────────────────┼──────────┼────────┤
│ 1              │ ACME     │ USR-1  │
└────────────────┴──────────┴────────┘

Problem: Data redundancy
- TenantId stored in two places
- If user moves to different tenant, must update both tables
- Harder to track membership history
```

### ❌ Design 2: TenantId ONLY in TenantUsers (Wrong)

```
Users Table (global):
┌────────┬──────────────┐
│ UserId │ Email        │
├────────┼──────────────┤
│ USR-1  │ john@ex.com  │
└────────┴──────────────┘

TenantUsers Table:
┌────────────────┬──────────┬────────┐
│ TenantUserId   │ TenantId │ UserId │
├────────────────┼──────────┼────────┤
│ 1              │ ACME     │ USR-1  │
└────────────────┴──────────┴────────┘

Problem: Performance & security
- Every login query requires JOIN to TenantUsers
- Can't authenticate user in isolation
- Doesn't answer: "Does this user belong to Tenant A?"
```

### ✅ Design 3: TenantId in BOTH (Current - Optimal)

```
Users Table:
┌────────┬──────────────┬──────────┐
│ UserId │ Email        │ TenantId │
├────────┼──────────────┼──────────┤
│ USR-1  │ john@ex.com  │ ACME     │
└────────┴──────────────┴──────────┘

TenantUsers Table:
┌────────────────┬──────────┬────────┬─────────────┐
│ TenantUserId   │ TenantId │ UserId │ Status      │
├────────────────┼──────────┼────────┼─────────────┤
│ 1              │ ACME     │ USR-1  │ Active      │
│ 2              │ ACME     │ USR-1  │ Suspended   │
└────────────────┴──────────┴────────┴─────────────┘

Benefits:
✅ Fast authentication (no join needed)
✅ Track membership history (multiple TenantUser rows)
✅ Membership status separately managed
✅ Flexible relationships
```

---

## Real-World Analogy

### Employee in Multiple Companies

```
Imagine John works for two companies: ACME and GlobalTech

EMPLOYEE TABLE (like Users):
┌──────────────┬──────────┬─────────────┐
│ EmployeeId   │ Name     │ CompanyId   │
├──────────────┼──────────┼─────────────┤
│ E-123        │ John     │ ACME        │
└──────────────┴──────────┴─────────────┘

Why CompanyId in Employee table?
- Fast lookup: "What company does John work for?" (no join)
- Authentication: "Is this John from ACME?" (quick check)
- Primary role definition

EMPLOYMENT TABLE (like TenantUsers):
┌──────────────────┬──────────┬──────────────┬──────────┐
│ EmploymentId     │ CompanyId│ EmployeeId   │ Status   │
├──────────────────┼──────────┼──────────────┼──────────┤
│ EMP-1            │ ACME     │ E-123        │ Active   │
│ EMP-2            │ GlobalTech│ E-123       │ Pending  │
│ EMP-3            │ ACME     │ E-123        │ Suspended│
└──────────────────┴──────────┴──────────────┴──────────┘

Why CompanyId also in Employment table?
- Track membership per company
- Manage status per company (active, suspended, pending)
- Historical record of employment changes
- John can work for multiple companies
```

---

## The Pattern: Denormalization for Performance

This is called **Denormalization** - intentionally storing redundant data for performance.

```
Pure Normalization (3NF):
┌─────────┐     ┌──────────────┐     ┌─────────────┐
│ Tenants │────▶│ TenantUsers  │◀────│ Users       │
│         │ 1:N │              │ N:1 │             │
└─────────┘     └──────────────┘     └─────────────┘

Problem: Slow authentication
SELECT u.* FROM Users u
JOIN TenantUsers tu ON u.UserId = tu.UserId
WHERE u.Email = ? AND tu.TenantId = ?

But we do Denormalization:
Users.TenantId = Direct pointer to tenant

Benefit: Fast authentication
SELECT u.* FROM Users u
WHERE u.Email = ? AND u.TenantId = ?
(No join needed!)
```

---

## Use Cases for Each Column

### Use Case 1: Fast Login (Uses Users.TenantId)

```csharp
// FAST: No join needed
public async Task<User> GetByEmailAsync(string email, Guid tenantId)
{
    return await _context.Users
        .FirstOrDefaultAsync(u => u.Email == email 
                              && u.TenantId == tenantId);
}

Query execution:
SELECT * FROM Users 
WHERE Email = ? AND TenantId = ?
│
└─ Indexed lookup (fast!)
```

### Use Case 2: Track Membership Status (Uses TenantUsers)

```csharp
// Check if user has accepted invitation to tenant
public async Task<bool> HasAcceptedAsync(Guid userId, Guid tenantId)
{
    var tenantUser = await _context.TenantUsers
        .FirstOrDefaultAsync(tu => tu.UserId == userId 
                              && tu.TenantId == tenantId
                              && tu.Status == "Active"
                              && tu.AcceptedAt != null);
    
    return tenantUser != null;
}

Query execution:
SELECT * FROM TenantUsers 
WHERE UserId = ? AND TenantId = ? AND Status = 'Active'
│
└─ Check invitation acceptance separately
```

### Use Case 3: List All Users in a Tenant (Could use either)

```csharp
// Option A: Using Users.TenantId (simpler)
public async Task<List<User>> GetTenantUsersAsync(Guid tenantId)
{
    return await _context.Users
        .Where(u => u.TenantId == tenantId)
        .ToListAsync();
}

// Option B: Using TenantUsers (tracks status)
public async Task<List<User>> GetActiveTenantUsersAsync(Guid tenantId)
{
    return await _context.TenantUsers
        .Where(tu => tu.TenantId == tenantId 
               && tu.Status == "Active"
               && tu.AcceptedAt != null)
        .Include(tu => tu.User)
        .Select(tu => tu.User)
        .ToListAsync();
}
```

---

## When to Query Which Table

### Query Users Table When:
- ✅ Authenticating (login)
- ✅ Finding user by email/ID
- ✅ Checking if user exists in tenant
- ✅ Getting user profile data
- ✅ Need fast access (indexed)

```csharp
var user = await db.Users
    .Where(u => u.Email == email && u.TenantId == tenantId)
    .FirstOrDefaultAsync();
```

### Query TenantUsers Table When:
- ✅ Tracking membership status
- ✅ Checking if user accepted invitation
- ✅ Viewing membership history
- ✅ Checking suspension/pending status
- ✅ Managing team membership

```csharp
var membership = await db.TenantUsers
    .Where(tu => tu.UserId == userId 
           && tu.TenantId == tenantId)
    .FirstOrDefaultAsync();

if (membership?.Status == "Suspended")
    return Unauthorized();
```

---

## Data Consistency: How Do We Keep Them in Sync?

### Rule 1: Users.TenantId is Primary

**When user is created:**
1. Insert into Users with TenantId
2. Insert into TenantUsers with same TenantId and Status = "Pending"

```csharp
var user = new User 
{ 
    UserId = Guid.NewGuid().ToString(),
    Email = "john@example.com",
    TenantId = "ACME",  // ← Primary ownership
    // ...
};
await db.Users.AddAsync(user);

var tenantUser = new TenantUser
{
    UserId = user.UserId,
    TenantId = "ACME",  // ← Must match Users.TenantId
    Status = "Pending",
    InvitedAt = DateTime.UtcNow
};
await db.TenantUsers.AddAsync(tenantUser);

await db.SaveChangesAsync();
```

### Rule 2: Constraint Ensures Consistency

**Database constraint:**
```sql
ALTER TABLE TenantUsers
ADD CONSTRAINT FK_TenantUser_User_Tenant
FOREIGN KEY (UserId, TenantId) REFERENCES Users(UserId, TenantId)
```

This ensures: If a TenantUser row exists, the corresponding User row with same TenantId MUST exist.

### Rule 3: Users.TenantId is Immutable

**Don't allow changing Users.TenantId after creation**

```csharp
// ❌ BAD: User changes tenants (breaks consistency)
user.TenantId = "GlobalTech";
await db.SaveChangesAsync();

// ✅ GOOD: Create new user record for different tenant
var newUser = new User 
{ 
    UserId = Guid.NewGuid().ToString(),
    Email = "john@example.com",
    TenantId = "GlobalTech",
    // Copy other fields
};
```

---

## Entity Relationship Diagram

### The Relationships

```
┌─────────────────────────────────────────┐
│ Tenants (Master Schema)                 │
│ ├─ TenantId (PK)                        │
│ ├─ Name                                 │
│ └─ (Global reference data)              │
└──────────────────┬──────────────────────┘
                   │ 1:N
                   ▼
┌─────────────────────────────────────────┐
│ Users (Auth Schema) - DENORMALIZED      │
│ ├─ UserId (PK)                          │
│ ├─ Email                                │
│ ├─ PasswordHash                         │
│ ├─ TenantId (FK) ← Direct ownership     │
│ └─ IsActive, CreatedAt, etc.            │
└──────────────────┬──────────────────────┘
                   │ 1:N (but why?)
                   ▼
┌─────────────────────────────────────────┐
│ TenantUsers (Membership Pivot)          │
│ ├─ TenantUserId (PK)                    │
│ ├─ TenantId (FK) ← Redundant copy       │
│ ├─ UserId (FK)                          │
│ ├─ Status (Active, Pending, Suspended) │
│ ├─ AcceptedAt, InvitedAt                │
│ └─ Composite FK: (UserId, TenantId)     │
└─────────────────────────────────────────┘

WHY TenantId in BOTH Users and TenantUsers?
1. Users.TenantId = Ownership (fast access)
2. TenantUsers.TenantId = Membership proof + status
3. Denormalization for performance
4. Constraint ensures consistency
```

---

## Performance Comparison

### Login Query Performance

**Without TenantId in Users (requires join):**
```sql
SELECT u.* FROM Users u
JOIN TenantUsers tu ON u.UserId = tu.UserId
WHERE u.Email = 'john@example.com'
  AND tu.TenantId = 'ACME'
  AND tu.Status = 'Active'

Execution plan: 
  ├─ Scan TenantUsers (filter by TenantId) → small
  ├─ Lookup Users (join by UserId) → fast
  └─ Total: Two index accesses

Time: ~5-10ms (depends on data size)
```

**With TenantId in Users (direct query):**
```sql
SELECT u.* FROM Users u
WHERE u.Email = 'john@example.com'
  AND u.TenantId = 'ACME'
  AND u.IsActive = true

Execution plan:
  └─ Index lookup on (Email, TenantId) → direct hit

Time: ~1-2ms (single index access)
```

**Performance gain: 5-10x faster** for login (most frequent operation)

---

## Migration Path: Why Not Denormalize?

### Original Normalized Design (Why It's Bad)

```
Users Table (Global):
├─ No TenantId
└─ Query: SELECT u.* FROM Users WHERE Email = ?
          (Returns user from ANY tenant)

TenantUsers Table:
├─ TenantId + UserId mapping
└─ Query requires: JOIN Users ON UserId
                   JOIN TenantUsers ON UserId, TenantId

Problem: Authentication requires join every login
         This is slow and violates multi-tenancy principle
```

### Current Design (Optimized)

```
Users Table (Tenant-scoped):
├─ TenantId column (denormalized)
└─ Query: SELECT u.* FROM Users WHERE Email = ?, TenantId = ?
          (Fast index lookup!)

TenantUsers Table (Membership tracking):
├─ TenantId for FK constraint
├─ Status tracking (Active, Pending, Suspended)
└─ Historical records (multiple rows per user)

Benefit: Both fast authentication AND flexible membership
```

---

## Summary Table

| Aspect | Users.TenantId | TenantUsers.TenantId |
|--------|-----------------|----------------------|
| **Purpose** | Ownership | Membership proof |
| **Query Performance** | Fast (no join) | Medium (with joins) |
| **Cardinality** | 1:1 per user | 1:N (history) |
| **Use in** | Authentication | Status tracking |
| **Mutability** | Immutable | Mutable (status changes) |
| **Data Type** | Required FK | Required FK |
| **Constraint** | Foreign key to Tenant | Composite FK + consistency check |

---

## See Also

- [Multi-Tenant Login Flow](./06-multi-tenant-login-flow.md) — How these tables work together
- [Multi-Tenant Architecture](./MULTI-TENANT-ARCHITECTURE.md) — Entity diagrams
- User Entity — Database schema definition
- TenantUser Entity — Membership schema definition
