# SmartWorkz.Core — Domain Foundation

## Assembly Reference

**NuGet:** Not published as NuGet; consumed as ProjectReference from StarterKit projects.  
**Namespace:** `SmartWorkz.Core`  
**Target Framework:** .NET 9  
**Dependencies:** `SmartWorkz.Shared` (NuGet package for global Result pattern)  

### Add to Your Project
```xml
<ProjectReference Include="..\..\SmartWorkz.Core\SmartWorkz.Core.csproj" />
```

---

## Entity Hierarchy

SmartWorkz defines a four-tier inheritance chain for domain entities. Each tier adds properties and behavior for real-world requirements:

### Tier 1: Entity<TId> — Identity and Equality

```csharp
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; }
    // Equals/GetHashCode based on Id
    // Operators: ==, !=
}
```

**XML Summary:** Generic base class for all domain entities. Two entities are equal if their Ids are equal, regardless of other properties. Use for any entity that needs a stable unique identity.

**When to use:** All entities, even those that don't need audit or tenant fields.

**Example:**
```csharp
public class Product : Entity<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

---

### Tier 2: AuditEntity<TId> — Who Changed What and When

```csharp
public abstract class AuditEntity<TId> : Entity<TId> where TId : notnull
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }  // User ID or email
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
```

**XML Summary:** Adds timestamps and user tracking for create/update operations. Supports compliance auditing without soft deletion.

**When to use:** Most business entities (User, Order, Invoice) where you need to know who created and last modified.

**Auto-populated by:** EF Core value generators in `Infrastructure/Data/Configurations/` or repository layer.

**Example:**
```csharp
public class Order : AuditEntity<int>
{
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
}
// Tracked: CreatedAt (when?), CreatedBy (who?), UpdatedAt, UpdatedBy
```

---

### Tier 3: DeletableEntity<TId> — Soft Delete

```csharp
public abstract class DeletableEntity<TId> : AuditEntity<TId> where TId : notnull
{
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
```

**XML Summary:** Soft delete support. Records who deleted and when, without losing data. All queries should automatically filter `IsDeleted = false` at the repository layer.

**When to use:** Entities with compliance requirements (cannot permanently delete without audit trail), or where "delete" means "archive".

**Infrastructure responsibility:** DbContext should intercept queries and add `.Where(x => !x.IsDeleted)` automatically.

**Example:**
```csharp
public class User : DeletableEntity<int>
{
    public string Email { get; set; }
    public string HashedPassword { get; set; }
}
// User deleted? IsDeleted=true, DeletedAt=now, DeletedBy="admin@company.com"
```

---

### Tier 4: TenantEntity<TId> — Multi-Tenancy

```csharp
public abstract class TenantEntity<TId> : DeletableEntity<TId> where TId : notnull
{
    public string TenantId { get; set; }  // Which tenant owns this record?
}
```

**XML Summary:** Adds `TenantId` to enforce data isolation in multi-tenant systems. All queries **must** filter by tenant to prevent cross-tenant data leaks.

**When to use:** Any entity that can differ per tenant (Customers, Orders, Products, Users). Exceptions: system entities (Roles, Permissions) might be shared or tenant-specific depending on your SLA.

**Example:**
```csharp
public class Customer : TenantEntity<int>
{
    public string CompanyName { get; set; }
}
// Two tenants each have a "Acme Corp" customer, but they're separate records with different TenantIds
```

---

## The Convenience Non-Generic Aliases

For common scenarios, single-type aliases are provided:

```csharp
// Instead of Entity<int>, use:
public class Product : Entity
{
    public string Name { get; set; }
}

// The four convenience classes:
// Entity                — Entity<int>
// AuditEntity          — AuditEntity<int>
// DeletableEntity      — DeletableEntity<int>
// TenantEntity         — TenantEntity<int>
```

---

## Core Interfaces

### IRepository<TEntity, TId> — CRUD + Specification

```csharp
public interface IRepository<TEntity, TId> where TEntity : Entity<TId>
{
    // Single
    Task<TEntity?> GetByIdAsync(TId id);
    Task<TEntity?> FindAsync(Specification<TEntity> spec);
    
    // Multiple
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> FindAllAsync(Specification<TEntity> spec);
    
    // Count & exists
    Task<int> CountAsync();
    Task<bool> ExistsAsync(TId id);
    
    // Write
    Task<TEntity> AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    Task UpdateAsync(TEntity entity);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities);
    Task DeleteAsync(TEntity entity);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities);
}
```

**Usage:**
```csharp
var product = await _repository.GetByIdAsync(123);
var activeProducts = await _repository.FindAllAsync(
    new Specification<Product>(p => p.IsActive)
);
```

---

### IUnitOfWork — Transaction Scope

```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<TEntity, TId> Repository<TEntity, TId>()
        where TEntity : Entity<TId>;
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
```

**Usage:**
```csharp
using var uow = _unitOfWork;
await uow.BeginTransactionAsync();
try
{
    var order = new Order { /* ... */ };
    await uow.Repository<Order, int>().AddAsync(order);
    await uow.SaveChangesAsync();
    await uow.CommitAsync();
}
catch
{
    await uow.RollbackAsync();
    throw;
}
```

---

## Value Objects

Value Objects are immutable, equality-by-value types. Each has a factory method that returns `Result<T>` for validation.

### Money

```csharp
public class Money
{
    public decimal Amount { get; }
    public string Currency { get; }  // ISO 4217: USD, EUR, GBP, JPY, CAD, AUD, INR, CNY
    
    // Factory (validates)
    public static Result<Money> Create(decimal amount, string currency)
    // Returns error if: Amount < 0, Currency empty, Currency not in ISO 4217 whitelist
    
    // Operations (return Result<Money>)
    public Result<Money> Add(Money other)       // Checks currency match
    public Result<Money> Subtract(Money other)  // Checks currency match
}
```

**Error Codes:**
- `MONEY_NEGATIVE` — Amount < 0
- `CURRENCY_EMPTY` — Currency string empty
- `CURRENCY_INVALID` — Not in ISO 4217 list
- `CURRENCY_MISMATCH` — Operations across different currencies
- `INSUFFICIENT_FUNDS` — Subtraction result would be negative

**Usage:**
```csharp
var result = Money.Create(99.99m, "USD");
if (result.IsSuccess)
{
    var price = result.Data;
    var withTax = price.Add(Money.Create(10m, "USD").Data);
}
```

### EmailAddress

```csharp
public class EmailAddress : ValueObject
{
    public string Value { get; }
    
    public static Result<EmailAddress> Create(string email)
    // Validates RFC 5322 basic pattern
}
```

### Other Built-In Value Objects
- `Address` — Street, City, State, ZipCode, Country
- `PersonName` — FirstName, LastName
- `PhoneNumber` — International E.164 format validation

---

## Guard Class — Validation at Entry Points

The `Guard` static class provides fail-fast validation for common scenarios. All methods throw `ArgumentException` on failure.

```csharp
public static class Guard
{
    // String validation
    public static string NotEmpty(string value, string paramName)
    public static string LengthBetween(string value, int min, int max, string paramName)
    public static string NoExtraWhitespace(string value, string paramName)
    
    // Email/URL validation
    public static string ValidEmail(string email, string paramName)
    public static string ValidUrl(string url, string paramName)
    public static string ValidHttpsUrl(string url, string paramName)
    
    // Phone validation
    public static string ValidPhone(string phone, string paramName)
    public static string ValidPhoneE164(string phone, string paramName)  // Strict international
    public static string PhoneInList(string phone, IEnumerable<string> whitelist, string paramName)
    
    // Money & Currency
    public static void ValidCurrency(string currency, string paramName)
    public static void ValidMoney(decimal amount, string currency, decimal? min, decimal? max, string paramName)
    
    // Enum validation
    public static void ValidEnum<TEnum>(TEnum value, string paramName) where TEnum : Enum
    
    // State machine (workflow) validation
    public static void ValidStateForEntity(EntityState current, string paramName, IReadOnlySet<EntityState> allowed)
    
    // Pagination
    public static int ValidPageSize(int size, string paramName, int min = 1, int max = 100)
    public static int ValidPageNumber(int number, string paramName)
    
    // Regex matching
    public static string MatchesRegex(string value, string pattern, string paramName)
    public static string MatchesAlphanumeric(string value, string paramName)
    public static string MatchesAlphanumericWithHyphens(string value, string paramName)
    
    // Text sanitization
    public static string SanitizeText(string value)  // Removes control chars, trims
}
```

**Usage in domain logic:**
```csharp
public class User : Entity
{
    public string Email { get; private set; }
    
    public static Result<User> Create(string email)
    {
        try
        {
            Guard.ValidEmail(email, nameof(email));
            return Result<User>.Success(new User { Email = email });
        }
        catch (ArgumentException ex)
        {
            return Result<User>.Failure($"INVALID_EMAIL", ex.Message);
        }
    }
}
```

---

## EntityState Enum — Workflow States

A 64-value enum representing all possible states for domain entities, grouped by 15 domain areas:

```csharp
public enum EntityState
{
    // Lifecycle (6)
    Created = 0,
    Active = 1,
    Inactive = 2,
    Archived = 3,
    Suspended = 4,
    Terminated = 5,
    
    // Verification (5)
    PendingVerification = 10,
    VerificationInProgress = 11,
    Verified = 12,
    VerificationFailed = 13,
    VerificationExpired = 14,
    
    // ... (9 more categories covering Approval, Document, Security, Payment, Order, Inventory, Communication, Subscription, UserAccount, ReturnRefund, TaskJob, ReviewRating, Shipping)
}
```

**StateGroups Helper — Fast state lookups:**

```csharp
public static class StateGroups
{
    public static IReadOnlySet<EntityState> LifecycleStates { get; }         // { Created, Active, ... }
    public static IReadOnlySet<EntityState> OrderStates { get; }             // Order workflow states
    public static IReadOnlySet<EntityState> PaymentStates { get; }           // Payment workflow states
    public static IReadOnlySet<EntityState> UserAccountStates { get; }       // User lifecycle states
    // ... (11 more predefined sets)
    
    // Lookup by type
    public static IReadOnlySet<EntityState> GetValidStatesFor(Type entityType)  // O(1) lookup
}
```

**Usage in validation:**
```csharp
Guard.ValidStateForEntity(order.Status, nameof(order.Status), StateGroups.OrderStates);
// Throws if order.Status is not a valid OrderState
```

---

## ServiceBase<TEntity, TDto> — CRUD Service Template

Base class for application services that handle Create, Read, Update, Delete:

```csharp
public abstract class ServiceBase<TEntity, TDto> : IService<TEntity, TDto>
    where TEntity : Entity
    where TDto : class
{
    // Constructor: takes repository
    protected ServiceBase(IRepository<TEntity, int> repository)
    
    // Virtual methods to override for custom logic
    protected abstract TDto Map(TEntity entity);                      // Entity → DTO
    protected abstract TEntity MapToEntity(TDto dto);                 // DTO → Entity
    protected virtual TEntity ApplyUpdates(TEntity entity, TDto dto)  // Partial updates
    
    // Concrete CRUD (sealed)
    public async Task<Result<TDto>> GetByIdAsync(int id)
    public async Task<Result<IEnumerable<TDto>>> GetAllAsync()
    public async Task<Result<TDto>> CreateAsync(TDto dto)
    public async Task<Result<TDto>> UpdateAsync(int id, TDto dto)
    public async Task<Result<bool>> DeleteAsync(int id)
}
```

**Example:**
```csharp
public class ProductService : ServiceBase<Product, ProductDto>
{
    public ProductService(IRepository<Product, int> repository) : base(repository) { }
    
    protected override ProductDto Map(Product entity)
        => new ProductDto { Id = entity.Id, Name = entity.Name, Price = entity.Price };
    
    protected override Product MapToEntity(ProductDto dto)
        => new Product { Name = dto.Name, Price = dto.Price };
}
```

---

## Usage Patterns

### 1. Create a Domain Entity
```csharp
public class Invoice : TenantEntity<int>
{
    public string InvoiceNumber { get; set; }
    public Money TotalAmount { get; set; }
    public DateTime DueDate { get; set; }
}
```

### 2. Use Guard Clauses in Methods
```csharp
public void SetAmount(decimal amount, string currency)
{
    var moneyResult = Money.Create(amount, currency);
    if (!moneyResult.IsSuccess)
        throw new DomainException(moneyResult.ErrorCode);
    
    TotalAmount = moneyResult.Data;
}
```

### 3. Check Workflow States
```csharp
if (order.Status == EntityState.Completed)
{
    // Safe to archive
}
```

### 4. Use ServiceBase for CRUD
```csharp
var service = new ProductService(_productRepository);
var result = await service.GetByIdAsync(1);
if (result.IsSuccess)
{
    Console.WriteLine($"Product: {result.Data.Name}");
}
```

---

**Next:** [SmartWorkz.Core.Web — Tag Helpers, Blazor Components, GraphQL](04-smartworkz-core-web.md)
