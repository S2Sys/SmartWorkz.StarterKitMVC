# SmartWorkz.Core — Domain Foundation

## Assembly Reference

**Path:** `src/SmartWorkz.Core/`  
**Namespace:** `SmartWorkz.Core`  
**Target Framework:** .NET 9  
**Dependencies:** SmartWorkz.Shared (Result pattern)  

### Add to Your Project

```xml
<ProjectReference Include="..\..\SmartWorkz.Core\SmartWorkz.Core.csproj" />
```

---

## Entity Hierarchy (XML Documented)

Four-tier inheritance chain. Each tier adds properties for real-world needs.

### Tier 1: Entity<TId> — Identity

Generic base class for all domain entities. Equality based on Id.

```csharp
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; }
}
```

Provides: Equals/GetHashCode (Id-based), operators (==, !=)

### Tier 2: AuditEntity<TId> — Audit Trail

Adds timestamps and user tracking for create/update operations.

```csharp
public abstract class AuditEntity<TId> : Entity<TId>
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }      // User ID or email
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
```

Auto-populated by EF Core value generators or repository layer.

### Tier 3: DeletableEntity<TId> — Soft Delete

Supports soft deletion. Records who deleted and when.

```csharp
public abstract class DeletableEntity<TId> : AuditEntity<TId>
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
```

All queries must filter `IsDeleted = false` at repository layer.

### Tier 4: TenantEntity<TId> — Multi-Tenancy

Adds TenantId for data isolation in multi-tenant systems.

```csharp
public abstract class TenantEntity<TId> : DeletableEntity<TId>
{
    public string TenantId { get; set; }
}
```

All queries must filter by tenant to prevent cross-tenant leaks.

### Convenience Aliases

```csharp
// Use these instead of Entity<int>
public class Product : Entity { ... }              // Entity<int>
public class Order : AuditEntity { ... }          // AuditEntity<int>
public class User : DeletableEntity { ... }       // DeletableEntity<int>
public class Customer : TenantEntity { ... }      // TenantEntity<int>
```

---

## Core Interfaces (XML Documented)

**IRepository<TEntity, TId>** — CRUD + Specification pattern

Single entity: GetByIdAsync, FindAsync  
Multiple: GetAllAsync, FindAllAsync  
Aggregates: CountAsync, ExistsAsync  
Mutations: AddAsync, UpdateAsync, DeleteAsync, (Range variants)

**IUnitOfWork** — Transaction scope

GetRepository<>(), SaveChangesAsync(), BeginTransactionAsync(), CommitAsync(), RollbackAsync()

---

## Value Objects (XML Documented)

Immutable, equality-by-value types with factory methods returning Result<T>.

**Money** — ISO 4217 currency with validation

Create(decimal amount, string currency) → Result<Money>  
Add/Subtract (checks currency match)  
Error codes: NEGATIVE, EMPTY, INVALID, MISMATCH, INSUFFICIENT

**EmailAddress** — RFC 5322 email validation

Create(string email) → Result<EmailAddress>

**Address, PersonName, PhoneNumber** — Standard value objects

---

## Guard Class (XML Documented)

Static fail-fast validation. Throws ArgumentException on failure.

**String validation:** NotEmpty, LengthBetween, NoExtraWhitespace  
**Email/URL:** ValidEmail, ValidUrl, ValidHttpsUrl  
**Phone:** ValidPhone, ValidPhoneE164, PhoneInList  
**Money:** ValidCurrency, ValidMoney  
**Enum:** ValidEnum  
**State machine:** ValidStateForEntity  
**Pagination:** ValidPageSize, ValidPageNumber  
**Regex:** MatchesRegex, MatchesAlphanumeric, MatchesAlphanumericWithHyphens  
**Text:** SanitizeText

---

## EntityState Enum (XML Documented)

64 values in 15 categories representing all entity lifecycle states.

**Lifecycle:** Created, Active, Inactive, Archived, Suspended, Terminated  
**Verification:** PendingVerification, VerificationInProgress, Verified, VerificationFailed, VerificationExpired  
**Approval, Document, Security, Payment, Order, Inventory, Communication, Subscription, UserAccount, ReturnRefund, TaskJob, ReviewRating, Shipping**

**StateGroups helper** — O(1) lookup by state type

---

## ServiceBase<TEntity, TDto> (XML Documented)

CRUD service template. Override Map/MapToEntity/ApplyUpdates for custom logic.

GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync — all sealed

---

## Usage Examples

**Create domain entity:**

```csharp
public class Product : Entity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

**Use Guard clauses:**

```csharp
Guard.ValidEmail(email, nameof(email));
Guard.ValidMoney(amount, "USD", min: 0, max: 10000, nameof(amount));
```

**Check workflow states:**

```csharp
if (order.Status == EntityState.Completed)
{
    // Safe to archive
}
```

**Use CRUD service:**

```csharp
var service = new ProductService(_repository);
var result = await service.GetByIdAsync(1);
if (result.IsSuccess) { /* use result.Data */ }
```

---

**Next:** [04-smartworkz-core-web.md](./04-smartworkz-core-web.md)
