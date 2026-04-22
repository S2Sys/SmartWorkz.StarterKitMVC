# SmartWorkz.Core Architecture Guide

## 1. Introduction

SmartWorkz.Core is a **Domain-Driven Design (DDD)** first framework that provides a foundational architecture for building robust, scalable applications. The framework emphasizes clean separation of concerns, rich domain models, and functional error handling through the Result pattern.

**Target Audience:** Developers integrating SmartWorkz.Core into new projects who need to understand design patterns, data flow, and best practices.

**Key Principles:**
- Entity-centric business logic
- Value objects for domain concepts
- Repository pattern for data access
- Service layer for orchestration
- Async-first design
- Multi-tenancy support
- Soft delete and audit trails

---

## 2. Domain-Driven Design Principles

### Entity vs Value Object Distinction

**Entities** have identity. They are uniquely identified by their Id property and compared by identity, not value.

```csharp
// Entity: identified by its Id
public class Customer : AuditableEntity<int>
{
    public string Name { get; set; }
    public EmailAddress Email { get; set; }
    public Address BillingAddress { get; set; }
}

// Two customers with same data but different Ids are different entities
var customer1 = new Customer { Id = 1, Name = "John" };
var customer2 = new Customer { Id = 2, Name = "John" };
bool same = customer1 == customer2; // false (different Ids)
```

**Value Objects** have no identity. They are compared by their values and are immutable.

```csharp
// Value Object: identified by its values
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
}

// Two money instances with same values are equal
var price1 = Money.Create(99.99m, "USD").Value;
var price2 = Money.Create(99.99m, "USD").Value;
bool same = price1 == price2; // true (same values)
```

### Aggregates and Aggregate Roots

An **Aggregate** is a cluster of entities and value objects treated as a single unit. The **Aggregate Root** is the entry point to the aggregate.

```csharp
// Order is the aggregate root
public class Order : AuditableEntity<int>
{
    public List<OrderLine> Lines { get; } = new();
    public Money Total { get; set; }
    
    public void AddLine(Product product, int quantity)
    {
        // Aggregate enforces invariants
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive");
        
        Lines.Add(new OrderLine 
        { 
            Product = product, 
            Quantity = quantity 
        });
    }
}

// OrderLine is a child entity, accessed through Order
public class OrderLine : Entity<int>
{
    public int OrderId { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
}
```

### Domain Events

Domain events represent significant business occurrences. While SmartWorkz.Core provides the entity and value object foundation, events can be published when domain state changes:

```csharp
// Example: Customer registration event (publish after entity creation)
public class CustomerRegisteredEvent
{
    public int CustomerId { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }
}

// Service publishes the event after creating customer
var customer = await customerService.CreateAsync(dto);
if (customer.IsSuccess)
{
    // Publish event to trigger notifications, welcome email, etc.
    await eventPublisher.PublishAsync(new CustomerRegisteredEvent
    {
        CustomerId = customer.Value.Id,
        Email = customer.Value.Email,
        RegisteredAt = DateTime.UtcNow
    });
}
```

---

## 3. Repository & Specification Pattern

### Repository Pattern Purpose and Benefits

The Repository pattern abstracts data access logic and provides a collection-like interface to entities. Benefits include:

- **Testability:** Easy to mock repositories in unit tests
- **Decoupling:** Application logic decoupled from data source
- **Consistency:** All queries go through repository (ensures soft-delete filtering, tenant isolation)
- **Flexibility:** Switch storage mechanisms without changing business logic

### IRepository<TEntity, TId> Interface Overview

```csharp
public interface IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
{
    // Read operations
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> FindAllAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
    
    // Write operations
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
```

### Specification Pattern for Query Composition

Specifications encapsulate query logic for reusability and testability. A Specification defines which entities to select, how to filter, sort, and include related data.

```csharp
// Reusable specification for "active products in a category"
public class ActiveProductsInCategorySpecification : Specification<Product>
{
    public ActiveProductsInCategorySpecification(int categoryId, decimal minPrice, decimal maxPrice)
    {
        // Define the base query
        Query
            .Where(p => p.CategoryId == categoryId)
            .Where(p => !p.IsDeleted)  // Soft delete filter
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Name);
        
        // Include related entities
        Query.Include(p => p.Category);
    }
}

// Usage: Specifications compose naturally
var spec = new ActiveProductsInCategorySpecification(
    categoryId: 5,
    minPrice: 10.0m,
    maxPrice: 100.0m
);

var products = await repository.FindAllAsync(spec);
```

### Specification Benefits

- **Readability:** Query intent is explicit in specification class name
- **Reusability:** Same specification used across multiple services
- **Testability:** Specification logic can be unit tested
- **Consistency:** All queries for "active products in category" use same specification
- **Composability:** Specifications can be extended or combined

---

## 4. Unit of Work Pattern

### IUnitOfWork Purpose

The Unit of Work pattern manages a collection of repositories and coordinates database transactions. It ensures atomic operations: either all changes are persisted or none are.

```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<TEntity, TId> Repository<TEntity, TId>() 
        where TEntity : class, IEntity<TId>;
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
```

### SaveAsync() Behavior and Transaction Guarantees

`SaveChangesAsync()` persists all pending changes in a single database transaction. If any operation fails, all changes are rolled back.

```csharp
// Example: Transfer funds between accounts (atomic operation)
using var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

var accountRepo = unitOfWork.Repository<Account, int>();

var sourceAccount = await accountRepo.GetByIdAsync(sourceId);
var destAccount = await accountRepo.GetByIdAsync(destId);

// Apply business logic
var transferResult = sourceAccount.Withdraw(amount);
if (!transferResult.IsSuccess)
    return Result.Fail<bool>(transferResult.Error);

var depositResult = destAccount.Deposit(amount);
if (!depositResult.IsSuccess)
    return Result.Fail<bool>(depositResult.Error);

// Persist both updates atomically
await accountRepo.UpdateAsync(sourceAccount);
await accountRepo.UpdateAsync(destAccount);

// If SaveChangesAsync fails, both updates are rolled back
try
{
    await unitOfWork.SaveChangesAsync();
    return Result.Ok(true);
}
catch (Exception ex)
{
    // Both updates are rolled back automatically
    logger.LogError(ex, "Transfer failed and rolled back");
    return Result.Fail<bool>(new Error("TRANSFER_FAILED", ex.Message));
}
```

### Rollback on Exception

Database transaction is automatically rolled back if `SaveChangesAsync()` throws an exception or if an unhandled exception occurs within the using block.

---

## 5. Service Layer Architecture

### IService<TEntity, TDto> Pattern

The generic service layer orchestrates CRUD operations on entities with automatic mapping between domain entities and DTOs.

```csharp
public interface IService<TEntity, TDto> where TEntity : class, IEntity<int>
{
    Task<Result<TDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<TDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TDto>> CreateAsync(TDto dto, CancellationToken cancellationToken = default);
    Task<Result<TDto>> UpdateAsync(int id, TDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

### CRUD Operation Lifecycle

| Operation | Workflow | Notes |
|-----------|----------|-------|
| **Create** | Validate DTO → MapToEntity → Repository.AddAsync → Map to DTO → Return Result<TDto> | Database-generated values (Id, timestamps) included in response |
| **Read** | Repository.GetByIdAsync → Map to DTO → Return Result<TDto> | Returns ENTITY_NOT_FOUND error if entity doesn't exist |
| **Update** | Validate id → Repository.GetByIdAsync → ApplyUpdates → Repository.UpdateAsync → Map to DTO → Return Result<TDto> | Concurrent updates use "Last-Write-Wins"; consider optimistic concurrency |
| **Delete** | Validate id → Repository.GetByIdAsync → Repository.DeleteAsync → Return Result<bool> | Soft delete if entity implements ISoftDeletable |

### Example Service Implementation

```csharp
public class ProductService : ServiceBase<Product, ProductDto>
{
    public ProductService(IRepository<Product, int> repository) : base(repository) { }

    protected override ProductDto Map(Product entity) =>
        new ProductDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price.Amount,
            Currency = entity.Price.Currency,
            CategoryId = entity.CategoryId
        };

    protected override Product MapToEntity(ProductDto dto) =>
        new Product
        {
            Name = dto.Name,
            Price = Money.Create(dto.Price, dto.Currency).Value,
            CategoryId = dto.CategoryId
        };

    protected override void ApplyUpdates(Product entity, ProductDto dto)
    {
        entity.Name = dto.Name;
        entity.Price = Money.Create(dto.Price, dto.Currency).Value;
        entity.CategoryId = dto.CategoryId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = GetCurrentUserId(); // Implemented by derived service
    }
}
```

### Dependency Injection Structure

Services are injected via constructor with the repository as a dependency:

```csharp
// DI Registration (in Startup.cs or Program.cs)
services
    .AddScoped<IRepository<Product, int>, ProductRepository>()
    .AddScoped<IService<Product, ProductDto>, ProductService>();

// Usage in controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IService<Product, ProductDto> _productService;

    public ProductsController(IService<Product, ProductDto> productService)
    {
        _productService = productService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        if (result.IsSuccess)
            return Ok(result.Value);
        
        return NotFound(new { error = result.Error.Message });
    }
}
```

---

## 6. Result Pattern (Error Handling)

### Result<T> Instead of Exceptions for Business Logic

The Result pattern provides functional error handling for expected business failures. Return `Result.Fail<T>()` instead of throwing exceptions for validation errors, not-found, conflicts, etc.

```csharp
// Traditional exception-based approach (anti-pattern)
public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
{
    if (string.IsNullOrEmpty(dto.Email))
        throw new ArgumentException("Email is required");
    
    var exists = await repository.ExistsAsync(u => u.Email == dto.Email);
    if (exists)
        throw new InvalidOperationException("Email already registered");
    
    // Create user...
    return userDto;
}

// Result-based approach (recommended)
public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto dto)
{
    if (string.IsNullOrEmpty(dto.Email))
        return Result.Fail<UserDto>(
            new Error("EMAIL_REQUIRED", "Email address is required")
        );
    
    var exists = await repository.ExistsAsync(u => u.Email == dto.Email);
    if (exists)
        return Result.Fail<UserDto>(
            new Error("EMAIL_ALREADY_REGISTERED", "Email is already registered")
        );
    
    // Create user...
    return Result.Ok(userDto);
}
```

### When to Use Result vs Throw

| Scenario | Use Result | Use Exception |
|----------|-----------|---------------|
| Validation error (invalid email, empty name) | Yes | No |
| Entity not found | Yes | No |
| Email already registered (business conflict) | Yes | No |
| Database connection error | No | Yes (infrastructure) |
| Null reference in code | No | Yes (programming error) |
| File I/O failure | No | Yes (infrastructure) |

**Rule of thumb:** Use Result for expected, recoverable business failures. Throw exceptions for unexpected, non-recoverable infrastructure failures.

### Error Codes and Error Messages

Error codes are machine-readable identifiers for error types. Messages are human-readable descriptions.

```csharp
public record Error(string Code, string Message);

// Examples
Result.Fail<UserDto>(new Error("EMAIL_REQUIRED", "Email address is required"));
Result.Fail<UserDto>(new Error("EMAIL_INVALID", "Email format is invalid"));
Result.Fail<UserDto>(new Error("EMAIL_ALREADY_REGISTERED", "Email is already registered"));
Result.Fail<UserDto>(new Error("USER_NOT_FOUND", "User with ID 42 not found"));
Result.Fail<UserDto>(new Error("INSUFFICIENT_PERMISSIONS", "User does not have permission to delete this resource"));
```

### Service Call with Result Handling

```csharp
public async Task<IActionResult> Register(RegisterUserDto dto)
{
    var result = await _userService.CreateAsync(
        new CreateUserDto { Email = dto.Email, Password = dto.Password }
    );

    if (result.IsSuccess)
    {
        // Success: User created
        return CreatedAtAction("GetUser", new { id = result.Value.Id }, result.Value);
    }

    // Failure: Log error and return appropriate HTTP status
    logger.LogWarning($"User registration failed: {result.Error.Code} - {result.Error.Message}");

    return result.Error.Code switch
    {
        "EMAIL_INVALID" => BadRequest(new { error = result.Error.Message }),
        "EMAIL_ALREADY_REGISTERED" => Conflict(new { error = result.Error.Message }),
        _ => StatusCode(500, new { error = "An unexpected error occurred" })
    };
}
```

---

## 7. Multi-Tenancy

### TenantId Filtering Architecture

Multi-tenancy allows a single application instance to serve multiple organizations (tenants) with isolated data. Every entity that is tenant-specific has a `TenantId` property.

```csharp
public abstract class AuditableEntity<TId> : ITenantScoped
{
    public int? TenantId { get; set; }  // Tenant identifier
}
```

### Shared vs Tenant-Specific Entities

| Entity Type | TenantId | Example |
|-------------|----------|---------|
| **Tenant-specific** | Set to tenant ID | Customer, Order, Product (for that tenant) |
| **Shared/System** | NULL | Country, Currency, GlobalSettings |

```csharp
// Tenant-specific entity
var customer = new Customer { Id = 1, TenantId = 42, Name = "Acme Corp" };

// Shared entity (accessible by all tenants)
var country = new Country { Id = "US", TenantId = null, Name = "United States" };
```

### Repository Implementation for Tenant Isolation

Repositories must filter by TenantId in all queries to prevent cross-tenant data leaks. **This is critical for security.**

```csharp
public class TenantScopedRepository<TEntity> : IRepository<TEntity, int>
    where TEntity : AuditableEntity<int>
{
    private readonly DbContext _context;
    private readonly int _tenantId;  // Current tenant

    public TenantScopedRepository(DbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantId = tenantProvider.GetCurrentTenantId();
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>()
            .Where(e => e.TenantId == _tenantId && !e.IsDeleted)  // Tenant + soft delete filter
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>()
            .Where(e => e.TenantId == _tenantId && !e.IsDeleted)  // Tenant + soft delete filter
            .ToListAsync(cancellationToken);
    }
    
    // All other methods must apply the same tenant filter
}
```

### Example: Repository Filtering by Tenant

```csharp
// Controller: Get all products for the current tenant
[HttpGet]
public async Task<IActionResult> GetProducts()
{
    var products = await _productService.GetAllAsync();
    return Ok(products.Value);
}

// Service layer: Repository automatically filters by TenantId
// Output: Only products where TenantId == currentTenant (e.g., 42)
// Products for other tenants are never returned
```

---

## 8. Entity Lifecycle & Soft Delete

### AuditableEntity<TId> Base Class Overview

All entities inherit from `AuditableEntity<TId>`, which provides audit tracking and soft delete capabilities.

```csharp
public abstract class AuditableEntity<TId> : IAuditable, ISoftDeletable, ITenantScoped
{
    // Primary Key
    public TId Id { get; set; }
    
    // Audit Trail
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    
    // Multi-Tenancy
    public int? TenantId { get; set; }
}
```

### Soft Delete Pattern

Soft delete marks entities as deleted without removing them from the database. This preserves audit trails and enables recovery.

```csharp
// Soft delete workflow
public async Task<Result<bool>> DeleteAsync(int id)
{
    var entity = await repository.GetByIdAsync(id);
    if (entity is null)
        return Result.Fail<bool>(new Error("NOT_FOUND", "Entity not found"));
    
    // Mark as deleted (soft delete)
    entity.IsDeleted = true;
    entity.DeletedAt = DateTime.UtcNow;
    entity.DeletedBy = currentUserId;
    
    await repository.UpdateAsync(entity);
    return Result.Ok(true);
}

// Repositories automatically filter out soft-deleted entities
var activeProducts = await repository.GetAllAsync();
// Returns only products where IsDeleted = false

// Recover a deleted entity
var deletedProduct = await context.Products
    .IgnoreQueryFilters()  // Temporarily ignore soft-delete filter
    .FirstOrDefaultAsync(p => p.Id == productId && p.IsDeleted);

if (deletedProduct != null)
{
    deletedProduct.IsDeleted = false;
    deletedProduct.DeletedAt = null;
    deletedProduct.DeletedBy = null;
    await repository.UpdateAsync(deletedProduct);
}
```

### CreatedBy/UpdatedBy Audit Trail

Every entity tracks who created and last updated it, enabling compliance reporting and accountability.

```csharp
// Service sets audit fields before persistence
var product = new Product { Name = "Laptop", Price = ... };
product.CreatedAt = DateTime.UtcNow;
product.CreatedBy = currentUserId;

await repository.AddAsync(product);

// Later, when updating
product.UpdatedAt = DateTime.UtcNow;
product.UpdatedBy = currentUserId;

await repository.UpdateAsync(product);

// Query for audit trail
var auditTrail = await context.Products
    .Where(p => p.Id == productId)
    .Select(p => new
    {
        p.CreatedAt,
        p.CreatedBy,
        p.UpdatedAt,
        p.UpdatedBy,
        p.DeletedAt,
        p.DeletedBy
    })
    .ToListAsync();
```

### Query Filtering for Soft-Deleted Entities

Repositories must automatically exclude soft-deleted entities from all queries:

```csharp
// Entity Framework Query Filters (configured in DbContext)
modelBuilder.Entity<Product>()
    .HasQueryFilter(p => !p.IsDeleted);

// Now all queries automatically exclude IsDeleted = true
var products = await context.Products.ToListAsync();  // No deleted products

// To include deleted entities (admin dashboard)
var allProducts = await context.Products
    .IgnoreQueryFilters()
    .ToListAsync();
```

---

## 9. Validation & Guard Clauses

### Guard Clauses for Contract Validation

Guard clauses validate preconditions at method entry points. The `Guard` helper class provides common validations:

```csharp
public class Guard
{
    public static T NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be empty");
        return value;
    }

    public static T NotDefault<T>(T value, string paramName) where T : struct
    {
        if (value.Equals(default(T)))
            throw new ArgumentException($"{paramName} cannot be default");
        return value;
    }
}
```

### Guard Clause Examples

```csharp
// Repository constructor: validate injected dependency
public Repository(DbContext context)
{
    DbContext = Guard.NotNull(context, nameof(context));
}

// Service constructor: validate repository
public ProductService(IRepository<Product, int> repository)
{
    Repository = Guard.NotNull(repository, nameof(repository));
}

// Service method: validate input
public async Task<Result<TDto>> GetByIdAsync(int id)
{
    Guard.NotDefault(id, nameof(id));  // Ensure id is not 0
    
    var entity = await Repository.GetByIdAsync(id);
    if (entity is null)
        return Result.Fail<TDto>(new Error("NOT_FOUND", "Entity not found"));
    
    return Result.Ok(Map(entity));
}

// Value object: validate parameters during creation
public static Result<Money> Create(decimal amount, string? currency)
{
    if (amount < 0)
        return Result.Fail<Money>(new Error("NEGATIVE", "Amount cannot be negative"));
    
    if (string.IsNullOrWhiteSpace(currency))
        return Result.Fail<Money>(new Error("EMPTY_CURRENCY", "Currency is required"));
    
    // ... further validation
}
```

### When to Validate

- **At domain boundaries:** Controllers, services receive external input
- **In value object factories:** Create() method validates all inputs
- **In entity constructors:** Enforce invariants (business rules)
- **In service methods:** Before executing business logic

### Entity Validation

Entities can implement a `Validate()` method to enforce business rules:

```csharp
public class Order : AuditableEntity<int>
{
    public decimal Total { get; set; }
    public List<OrderLine> Lines { get; set; } = new();

    public Result<bool> Validate()
    {
        if (Lines.Count == 0)
            return Result.Fail<bool>(new Error("NO_ITEMS", "Order must contain at least one item"));

        if (Total < 0)
            return Result.Fail<bool>(new Error("NEGATIVE_TOTAL", "Order total cannot be negative"));

        return Result.Ok(true);
    }
}

// Service calls validation before persistence
var order = MapToEntity(dto);
var validationResult = order.Validate();
if (!validationResult.IsSuccess)
    return Result.Fail<OrderDto>(validationResult.Error);

await repository.AddAsync(order);
```

---

## 10. Data Transfer Objects (DTOs)

### DTO Purpose and Usage

DTOs (Data Transfer Objects) are simple objects used to transfer data between application layers. They decouple the API contract from the domain model.

| Aspect | Entity | DTO |
|--------|--------|-----|
| **Purpose** | Domain model with business logic | Data transfer over API |
| **Scope** | Private implementation details | Public API contract |
| **Validation** | Domain validation (invariants) | Input validation (DataAnnotations) |
| **Exposure** | Never exposed to clients | Exposed to API consumers |

```csharp
// Domain Entity (never exposed)
public class Customer : AuditableEntity<int>
{
    public EmailAddress Email { get; set; }  // Value object
    public PersonName Name { get; set; }
    public Address BillingAddress { get; set; }
    public bool IsBlacklisted { get; set; }  // Internal flag
}

// DTO (API contract)
public class CustomerDto
{
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    [Required]
    public string Street { get; set; }
    
    public string? MiddleName { get; set; }
    
    // IsBlacklisted is NOT exposed to clients
}
```

### Mapping from Entity to DTO

Mapping is typically done by the service layer using MapToEntity() and Map() methods:

```csharp
// Service implementation
protected override CustomerDto Map(Customer entity) =>
    new CustomerDto
    {
        Email = entity.Email.Value,
        FirstName = entity.Name.FirstName,
        LastName = entity.Name.LastName,
        MiddleName = entity.Name.MiddleName,
        Street = entity.BillingAddress.Street,
        // IsBlacklisted intentionally omitted
    };

protected override Customer MapToEntity(CustomerDto dto)
{
    var emailResult = EmailAddress.Create(dto.Email);
    if (!emailResult.IsSuccess)
        throw new ArgumentException(emailResult.Error.Message);
    
    var nameResult = PersonName.Create(dto.FirstName, dto.LastName, dto.MiddleName);
    if (!nameResult.IsSuccess)
        throw new ArgumentException(nameResult.Error.Message);
    
    var addressResult = Address.Create(dto.Street, /* other fields */);
    if (!addressResult.IsSuccess)
        throw new ArgumentException(addressResult.Error.Message);
    
    return new Customer
    {
        Email = emailResult.Value,
        Name = nameResult.Value,
        BillingAddress = addressResult.Value,
        IsBlacklisted = false  // Default to not blacklisted
    };
}
```

### Using AutoMapper for Complex Mappings

For projects with many entities, AutoMapper simplifies mapping:

```csharp
// AutoMapper profile
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Value))
            .ForMember(d => d.FirstName, opt => opt.MapFrom(s => s.Name.FirstName))
            .ForMember(d => d.LastName, opt => opt.MapFrom(s => s.Name.LastName))
            .ForMember(d => d.MiddleName, opt => opt.MapFrom(s => s.Name.MiddleName))
            .ReverseMap();
    }
}

// Service using AutoMapper
protected override CustomerDto Map(Customer entity) => _mapper.Map<CustomerDto>(entity);
protected override Customer MapToEntity(CustomerDto dto) => _mapper.Map<Customer>(dto);
```

### Security Implications

DTOs control what data is exposed to API clients:

```csharp
// Entity has sensitive fields
public class User : AuditableEntity<int>
{
    public string PasswordHash { get; set; }  // Never expose
    public bool IsAdmin { get; set; }  // Expose only to admins
    public string InternalNotes { get; set; }  // Never expose
}

// DTO omits sensitive fields
public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    // PasswordHash, IsAdmin, InternalNotes are never exposed
}

// Admin DTO exposes admin-only fields
public class AdminUserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public bool IsAdmin { get; set; }  // Only in admin DTO
}
```

---

## 11. Dependency Injection

### Container Setup

SmartWorkz services are registered in the DI container during application startup:

```csharp
// Program.cs (.NET 6+)
var builder = WebApplicationBuilder.CreateBuilder(args);

// Register SmartWorkz services
builder.Services
    .AddScoped<DbContext>(provider => new AppDbContext(connectionString))
    .AddScoped(typeof(IRepository<,>), typeof(Repository<,>))
    .AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();
```

### Service Registration

Services implementing `IService<TEntity, TDto>` are registered with appropriate lifetimes:

```csharp
// Register specific services
builder.Services
    .AddScoped<IService<Product, ProductDto>, ProductService>()
    .AddScoped<IService<Customer, CustomerDto>, CustomerService>()
    .AddScoped<IService<Order, OrderDto>, OrderService>();

// Or generic factory for all services
builder.Services.Scan(scan => scan
    .FromAssemblies(typeof(ProductService).Assembly)
    .AddClasses(classes => classes.AssignableTo(typeof(IService)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
```

### How to Inject Dependencies

Dependencies are injected via constructor (constructor injection):

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IService<Product, ProductDto> _productService;
    private readonly ILogger<ProductsController> _logger;

    // Constructor injection
    public ProductsController(
        IService<Product, ProductDto> productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        if (result.IsSuccess)
            return Ok(result.Value);
        return NotFound();
    }
}
```

### Typical Dependency Graph

```
Controller (HTTP)
    └─> Service<TEntity, TDto> (Business Logic)
        └─> Repository<TEntity, TId> (Data Access)
            └─> DbContext (ORM)
                └─> Database
```

---

## 12. Async-First Design

### Task-Based Operations

All I/O operations (database, network, file) are async-based using `Task` and `Task<T>`:

```csharp
// Async repository operations
Task<TEntity?> GetByIdAsync(TId id);
Task AddAsync(TEntity entity);
Task UpdateAsync(TEntity entity);
Task DeleteAsync(TId id);

// Async service operations
Task<Result<TDto>> GetByIdAsync(int id);
Task<Result<TDto>> CreateAsync(TDto dto);
Task<Result<TDto>> UpdateAsync(int id, TDto dto);

// Always async method signatures
public async Task<Result<ProductDto>> GetAsync(int id)
{
    var entity = await repository.GetByIdAsync(id);
    return Result.Ok(Map(entity));
}
```

### CancellationToken Support

All async methods support `CancellationToken` to enable graceful cancellation:

```csharp
// Method signature includes CancellationToken
public async Task<Result<TDto>> GetByIdAsync(
    int id, 
    CancellationToken cancellationToken = default)
{
    var entity = await repository.GetByIdAsync(id, cancellationToken);
    if (entity is null)
        return Result.Fail<TDto>(new Error("NOT_FOUND", "Entity not found"));
    
    return Result.Ok(Map(entity));
}

// Controller passes cancellation token
[HttpGet("{id}")]
public async Task<ActionResult<ProductDto>> GetProduct(
    int id, 
    CancellationToken cancellationToken)
{
    var result = await _productService.GetByIdAsync(id, cancellationToken);
    return result.IsSuccess ? Ok(result.Value) : NotFound();
}
```

### Why Async-First?

- **Thread Pool Efficiency:** Async frees threads for other requests instead of blocking
- **Scalability:** Async enables handling more concurrent requests with fewer threads
- **Responsiveness:** UI threads never block on I/O operations
- **Resource Utilization:** Better CPU and memory usage under high concurrency

### Best Practices

**DO:**
```csharp
// Good: Use async all the way
var result = await repository.GetByIdAsync(id);
var updated = Map(result);
return Ok(updated);
```

**DON'T:**
```csharp
// Bad: Blocking on async (deadlock risk)
var result = repository.GetByIdAsync(id).Result;

// Bad: Calling async from sync method
var product = GetProductAsync(id).GetAwaiter().GetResult();
```

---

## 13. Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Presentation Layer                            │
│                   (Controller, ViewModel, View)                      │
│  - Handle HTTP requests/responses                                    │
│  - Parse input, validate DTOs, format responses                      │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                    Depends on Service
                             │
┌────────────────────────────▼────────────────────────────────────────┐
│                      Application Layer                               │
│          (ServiceBase<T>, IService<TEntity, TDto>)                   │
│        ┌─────────────────────────────────────────────┐               │
│        │  1. Validate DTO with DataAnnotations       │               │
│        │  2. Map DTO to Entity (MapToEntity)         │               │
│        │  3. Invoke Domain Logic via Repository      │               │
│        │  4. Map Entity to DTO (Map)                 │               │
│        │  5. Return Result<TDto> to caller           │               │
│        │  6. Handle Result or propagate Error        │               │
│        └─────────────────────────────────────────────┘               │
│  - Orchestrates CRUD operations                                      │
│  - Encapsulates business workflows                                   │
│  - Handles Result-based error propagation                            │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                    Depends on Repository
                             │
┌────────────────────────────▼────────────────────────────────────────┐
│                       Domain Layer                                   │
│   (Entity<TId>, AuditableEntity<TId>, ValueObject, Specification)   │
│        ┌─────────────────────────────────────────────┐               │
│        │  1. Encapsulate Business Logic              │               │
│        │  2. Enforce Invariants (Guard clauses)      │               │
│        │  3. Maintain Audit Trail                    │               │
│        │  4. Track Soft-Deleted Entities             │               │
│        │  5. Represent Domain Concepts (Value Objs)  │               │
│        └─────────────────────────────────────────────┘               │
│  - Rich domain models with behavior                                  │
│  - Immutable value objects                                           │
│  - Entity aggregate roots                                            │
│  - Domain-driven invariants                                          │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                    Depends on IRepository
                             │
┌────────────────────────────▼────────────────────────────────────────┐
│                  Infrastructure/Persistence Layer                    │
│      (IRepository, DbContext, IUnitOfWork, Migrations)               │
│        ┌─────────────────────────────────────────────┐               │
│        │  1. Database Queries (LINQ, Specification)  │               │
│        │  2. Database Commands (Create, Update, Del) │               │
│        │  3. Atomic Transactions (SaveAsync)         │               │
│        │  4. Entity Mapping (EF Core)                │               │
│        │  5. Multi-Tenancy Filtering                 │               │
│        │  6. Soft-Delete Filtering                   │               │
│        └─────────────────────────────────────────────┘               │
│  - Data access implementations                                       │
│  - ORM configuration (EF Core)                                       │
│  - Transaction management                                            │
│  - Query filters (soft delete, tenant)                               │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 14. Security Considerations

### Multi-Tenancy Data Isolation (Critical)

Every repository query MUST filter by TenantId to prevent accidental cross-tenant data leaks.

```csharp
// Repository must filter by tenant
public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken)
{
    return await context.Set<TEntity>()
        .Where(e => e.TenantId == _currentTenantId)  // CRITICAL
        .Where(e => !e.IsDeleted)
        .ToListAsync(cancellationToken);
}

// Test: Verify tenant isolation
[Test]
public async Task GetAll_ShouldFilterByTenant()
{
    var products1 = await new TenantScopedRepository<Product>(context, tenant1).GetAllAsync();
    var products2 = await new TenantScopedRepository<Product>(context, tenant2).GetAllAsync();
    
    // Tenant1 should never see Tenant2's products
    Assert.That(products1.All(p => p.TenantId == tenant1.Id), Is.True);
    Assert.That(products2.All(p => p.TenantId == tenant2.Id), Is.True);
}
```

### Soft-Delete Enforcement

All repository queries must include `WHERE IsDeleted = false` (via query filters) to prevent exposing deleted entities:

```csharp
// Entity Framework Query Filter (configured in DbContext OnModelCreating)
modelBuilder.Entity<Product>()
    .HasQueryFilter(p => !p.IsDeleted);

// OR explicit filtering in repository
public async Task<TEntity?> GetByIdAsync(TId id)
{
    return await context.Set<TEntity>()
        .Where(e => !e.IsDeleted)  // Soft delete filter
        .FirstOrDefaultAsync(e => e.Id == id);
}
```

### DTO Projection to Avoid Exposing Internal Fields

DTOs control what data is exposed. Never return domain entities directly:

```csharp
// Bad: Exposing entity directly (includes all fields)
[HttpGet("{id}")]
public async Task<Product> GetProduct(int id)
{
    return await repository.GetByIdAsync(id);  // SECURITY ISSUE
}

// Good: Return mapped DTO
[HttpGet("{id}")]
public async Task<ProductDto> GetProduct(int id)
{
    var result = await productService.GetByIdAsync(id);
    return result.IsSuccess ? Ok(result.Value) : NotFound();
}
```

### Guard Clause Validation at Boundaries

All external input is validated at entry points:

```csharp
[HttpPost]
public async Task<Result<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
{
    // Validation happens in controller or service
    if (dto == null)
        return BadRequest("Request body is required");
    
    if (string.IsNullOrWhiteSpace(dto.Name))
        return BadRequest("Product name is required");
    
    var result = await _productService.CreateAsync(dto);
    return result;
}
```

### Audit Trail for Compliance

Every entity tracks who created and modified it:

```csharp
// Audit trail is maintained automatically
public class Product : AuditableEntity<int>
{
    public DateTime CreatedAt { get; set; }  // When created
    public int? CreatedBy { get; set; }  // Who created it
    public DateTime? UpdatedAt { get; set; }  // When last updated
    public int? UpdatedBy { get; set; }  // Who last updated it
    public bool IsDeleted { get; set; }  // Soft delete flag
    public DateTime? DeletedAt { get; set; }  // When deleted
    public int? DeletedBy { get; set; }  // Who deleted it
}

// Query audit trail for compliance reporting
var auditReport = await context.Products
    .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
    .Select(p => new
    {
        p.Id, p.Name,
        p.CreatedAt, p.CreatedBy,
        p.UpdatedAt, p.UpdatedBy,
        p.DeletedAt, p.DeletedBy
    })
    .ToListAsync();
```

---

## 15. Testing Strategy (High-Level)

### Integration Tests with Real Database

SmartWorkz.Core recommends integration tests using a real (test) database to verify:

```csharp
[TestFixture]
public class ProductRepositoryTests
{
    private DbContext _context;
    private IRepository<Product, int> _repository;

    [SetUp]
    public async Task Setup()
    {
        _context = new TestAppDbContext();
        await _context.Database.EnsureCreatedAsync();
        _repository = new Repository<Product, int>(_context);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnProduct()
    {
        var product = new Product { Id = 1, Name = "Laptop", TenantId = 1 };
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Laptop"));
    }

    [Test]
    public async Task GetAllAsync_ShouldExcludeSoftDeleted()
    {
        var active = new Product { Id = 1, Name = "Active", TenantId = 1, IsDeleted = false };
        var deleted = new Product { Id = 2, Name = "Deleted", TenantId = 1, IsDeleted = true };
        
        await _repository.AddAsync(active);
        await _repository.AddAsync(deleted);
        await _context.SaveChangesAsync();

        var results = await _repository.GetAllAsync();

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results.First().Name, Is.EqualTo("Active"));
    }
}
```

### Specification Composition Testing

Test that specifications compose correctly:

```csharp
[TestFixture]
public class ActiveProductsInCategorySpecificationTests
{
    [Test]
    public async Task Specification_ShouldFilterByCategory()
    {
        var spec = new ActiveProductsInCategorySpecification(
            categoryId: 5,
            minPrice: 10.0m,
            maxPrice: 100.0m
        );

        var products = await _repository.FindAllAsync(spec);

        Assert.That(products.All(p => p.CategoryId == 5), Is.True);
        Assert.That(products.All(p => !p.IsDeleted), Is.True);
        Assert.That(products.All(p => p.Price >= 10.0m && p.Price <= 100.0m), Is.True);
    }
}
```

### Service CRUD Operation Testing

Test service layer operations with Result handling:

```csharp
[TestFixture]
public class ProductServiceTests
{
    private IService<Product, ProductDto> _service;
    private IRepository<Product, int> _repository;

    [Test]
    public async Task CreateAsync_ShouldReturnSuccessResult()
    {
        var dto = new ProductDto { Name = "Laptop", Price = 999.99m };

        var result = await _service.CreateAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("Laptop"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNotFoundError()
    {
        var result = await _service.GetByIdAsync(9999);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Code, Is.EqualTo("ENTITY_NOT_FOUND"));
    }
}
```

### Repository Filtering Testing (Tenant Isolation)

Verify that repositories properly isolate tenant data:

```csharp
[TestFixture]
public class TenantIsolationTests
{
    [Test]
    public async Task Repository_ShouldNotReturnOtherTenantData()
    {
        var product1 = new Product { TenantId = 1, Name = "Product A" };
        var product2 = new Product { TenantId = 2, Name = "Product B" };
        
        await _repository.AddRangeAsync(new[] { product1, product2 });

        // Tenant 1 repository should not see Tenant 2's product
        var tenant1Repo = new TenantScopedRepository<Product>(_context, tenantProvider: 1);
        var results = await tenant1Repo.GetAllAsync();

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results.First().TenantId, Is.EqualTo(1));
    }
}
```

---

## 16. Configuration & Setup

### Service Registration in DI Container

```csharp
// Program.cs (.NET 6+)
var builder = WebApplicationBuilder.CreateBuilder(args);

// Add SmartWorkz.Core infrastructure
builder.Services
    .AddScoped<DbContext>(provider => new AppDbContext(
        builder.Configuration.GetConnectionString("DefaultConnection")))
    .AddScoped(typeof(IRepository<,>), typeof(Repository<,>))
    .AddScoped<IUnitOfWork, UnitOfWork>()
    .AddLogging();

// Add application services
builder.Services
    .AddScoped<IService<Product, ProductDto>, ProductService>()
    .AddScoped<IService<Customer, CustomerDto>, CustomerService>()
    .AddScoped<IService<Order, OrderDto>, OrderService>();

// Add other infrastructure
builder.Services
    .AddControllers()
    .AddJsonOptions(options => /* JSON settings */);

var app = builder.Build();

app.MapControllers();
app.Run();
```

### AppConstants Configuration

Application-wide constants can be centralized:

```csharp
public static class AppConstants
{
    public const string DatabaseConnectionName = "DefaultConnection";
    
    public static class Validation
    {
        public const int MaxNameLength = 256;
        public const int MaxEmailLength = 256;
        public const int MaxAddressLength = 500;
    }
    
    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
    }
    
    public static class Currencies
    {
        public static readonly string[] Supported = { "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "INR", "CNY" };
    }
}
```

### Multi-Tenancy Setup

Multi-tenancy requires:

1. **Tenant Provider:** Gets the current tenant from HTTP context
2. **Tenant-Scoped Repository:** Filters by TenantId
3. **Middleware:** Sets tenant context from HTTP headers or claims

```csharp
// Tenant Provider Interface
public interface ITenantProvider
{
    int GetCurrentTenantId();
}

// HTTP-based Tenant Provider
public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetCurrentTenantId()
    {
        var tenantIdHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-ID"];
        if (int.TryParse(tenantIdHeader, out var tenantId))
            return tenantId;
        
        throw new InvalidOperationException("Tenant ID not provided");
    }
}

// Register in DI
builder.Services
    .AddScoped<ITenantProvider, HttpTenantProvider>()
    .AddHttpContextAccessor();
```

### Example IServiceCollection.AddSmartWorkz() Extension

Create a convenience extension method to register all SmartWorkz services:

```csharp
public static class SmartWorkzExtensions
{
    public static IServiceCollection AddSmartWorkz(
        this IServiceCollection services,
        string connectionString)
    {
        services
            .AddScoped<DbContext>(provider => new AppDbContext(connectionString))
            .AddScoped(typeof(IRepository<,>), typeof(Repository<,>))
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<ITenantProvider, HttpTenantProvider>()
            .AddHttpContextAccessor();

        return services;
    }
}

// Usage
builder.Services.AddSmartWorkz(
    builder.Configuration.GetConnectionString("DefaultConnection"));
```

---

## Summary

SmartWorkz.Core provides a comprehensive architecture for building Domain-Driven Design applications with:

- **Rich domain models** using entities, value objects, and aggregates
- **Repository pattern** for data access with Specification composition
- **Service layer** for business logic orchestration with Result-based error handling
- **Multi-tenancy** support with automatic tenant isolation
- **Soft delete & audit trails** for compliance and data recovery
- **Async-first design** for scalability and responsiveness
- **Type-safe DTOs** for API contracts that decouple domain from clients

Follow these patterns to build maintainable, scalable, and secure applications with SmartWorkz.Core.
