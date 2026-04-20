# Domain Abstractions & Constants Guide

Complete reference for SmartWorkz.Core domain interfaces, base classes, and system-wide constants, covering entity abstractions, auditing patterns, soft deletion, tenancy, service implementations, aggregate roots, domain events, and configuration values.

---

## Overview

SmartWorkz follows **Domain-Driven Design (DDD)** principles with a layered architecture:

1. **Domain Interfaces** — Contract-based entity behavior (IEntity, IAuditable, ISoftDeletable, ITenantScoped)
2. **Base Entities** — AuditableEntity<TId>, AuditableEntity (int convenience alias)
3. **Aggregate Root** — AggregateRoot<TId> with domain event support
4. **Service Layer** — ServiceBase<TEntity, TDto> for CRUD with mapping
5. **Constants** — AppConstants and SharedConstants for system-wide configuration

**Key Design Patterns:**
- Generic base classes support int, string, Guid, long primary keys
- Automatic audit tracking (CreatedAt/By, UpdatedAt/By)
- Soft deletion with infrastructure-level filtering
- Multi-tenant scoping (TenantId)
- Domain events for event-driven architecture
- Specification pattern for complex queries
- Unit of Work pattern for transaction management

---

## Entity Interfaces

All domain entities in SmartWorkz implement one or more of these interfaces to define their behavior and capabilities.

### IEntity<TId> Interface

The fundamental entity contract. All entities must implement this interface and be registered in the database context.

```csharp
namespace SmartWorkz.Core.Abstractions;

public interface IEntity<TId> : SmartWorkz.Core.Shared.Primitives.IEntity<TId>
{
}
```

**Purpose:**
- Marks a class as a domain entity with a typed primary key
- Enables generic repository and service patterns
- TId can be int, string, Guid, or long

**Usage in Domain Models:**

```csharp
// Product entity with int primary key
public class Product : AuditableEntity<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Sku { get; set; }
}

// User entity with string primary key
public class User : AuditableEntity<string>
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}

// Order entity with Guid primary key
public class Order : AuditableEntity<Guid>
{
    public DateTime OrderDate { get; set; }
    public List<OrderLine> Items { get; set; }
}
```

**Generic Constraint Benefits:**
- Compile-time type safety
- No casting required
- Framework can validate TId consistency

---

### IAuditable Interface

Tracks who created and last modified an entity. Required for audit trails and compliance.

```csharp
namespace SmartWorkz.Core.Abstractions;

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}
```

**Properties:**
| Property | Type | Description |
|----------|------|-------------|
| CreatedAt | DateTime | UTC timestamp when entity was created |
| CreatedBy | string? | User ID or username who created the entity |
| UpdatedAt | DateTime? | UTC timestamp of last modification |
| UpdatedBy | string? | User ID or username who last modified the entity |

**Set Automatically By:**
- EF Core interceptors in Infrastructure layer
- Identity/authentication context for CreatedBy/UpdatedBy

**Usage Pattern:**

```csharp
// Infrastructure automatically populates on SaveChanges
public class Category : AuditableEntity<int>
{
    public string Name { get; set; }
    // IAuditable properties populated by interceptor:
    // CreatedAt = DateTime.UtcNow
    // CreatedBy = currentUserId
    // UpdatedAt = null until first update
    // UpdatedBy = null until first update
}

// Query audit history
var recentlyModified = await _repository.FindAllAsync(
    new Specification<Category>(c => c.UpdatedAt >= DateTime.UtcNow.AddDays(-7))
);

// Auditing in business logic
public async Task LogAuditTrailAsync(int categoryId)
{
    var category = await _repository.GetByIdAsync(categoryId);
    Console.WriteLine($"Created by {category.CreatedBy} on {category.CreatedAt}");
    if (category.UpdatedBy != null)
        Console.WriteLine($"Last modified by {category.UpdatedBy} on {category.UpdatedAt}");
}
```

---

### ISoftDeletable Interface

Marks entities as soft-deletable. Records are marked deleted rather than physically removed, enabling data recovery and historical analysis.

```csharp
namespace SmartWorkz.Core.Abstractions;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
```

**Properties:**
| Property | Type | Description |
|----------|------|-------------|
| IsDeleted | bool | Flag indicating deletion status |
| DeletedAt | DateTime? | UTC timestamp when entity was deleted |
| DeletedBy | string? | User ID/username who deleted the entity |

**How It Works:**
- Infrastructure layer (DbContext, Dapper interceptors) applies global filter: `IsDeleted == false`
- All queries automatically exclude deleted records
- Manual delete queries must check `IsDeleted` property
- Data is never physically removed (reversible)

**Usage Pattern:**

```csharp
public class Product : AuditableEntity<int>, ISoftDeletable
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Deleting an entity (soft delete)
public async Task DeleteProductAsync(int productId)
{
    var product = await _repository.GetByIdAsync(productId);
    if (product == null)
        return Result.Fail("Product not found");
    
    // Service/Infrastructure handles:
    // product.IsDeleted = true
    // product.DeletedAt = DateTime.UtcNow
    // product.DeletedBy = currentUserId
    await _repository.DeleteAsync(product);
}

// Query: automatically excludes soft-deleted records
var activeProducts = await _repository.GetAllAsync(); // IsDeleted = false only

// Query deleted records (if needed)
var allIncludingDeleted = await _context.Products
    .IgnoreQueryFilters()  // Bypass global filter
    .Where(p => p.IsDeleted)
    .ToListAsync();

// Restore deleted entity (if needed)
public async Task RestoreProductAsync(int productId)
{
    var product = await _context.Products
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(p => p.Id == productId);
    
    if (product?.IsDeleted == true)
    {
        product.IsDeleted = false;
        product.DeletedAt = null;
        product.DeletedBy = null;
        await _context.SaveChangesAsync();
    }
}
```

---

### ITenantScoped Interface

Marks entities that belong to a tenant in a multi-tenant application. Infrastructure layer automatically scopes queries to the current tenant.

```csharp
namespace SmartWorkz.Core.Abstractions;

public interface ITenantScoped
{
    string? TenantId { get; set; }
}
```

**Properties:**
| Property | Type | Description |
|----------|------|-------------|
| TenantId | string? | Unique identifier for the tenant that owns this entity |

**Multi-Tenant Architecture:**
- Each tenant has isolated data
- Infrastructure applies automatic filter: `TenantId == currentTenantId`
- Users can only see/modify their tenant's data
- Set at entity creation time, typically immutable after

**Usage Pattern:**

```csharp
public class CustomerAccount : AuditableEntity<int>
{
    public string TenantId { get; set; }      // Multi-tenant isolation
    public string AccountNumber { get; set; }
    public decimal Balance { get; set; }
}

// Infrastructure interceptor automatically sets TenantId
public async Task<Result> CreateAccountAsync(CreateAccountDto dto, string currentTenantId)
{
    var account = new CustomerAccount
    {
        TenantId = currentTenantId,  // Set before save
        AccountNumber = dto.AccountNumber,
        Balance = dto.InitialBalance
    };
    
    await _repository.AddAsync(account);
    return Result.Ok();
}

// Queries automatically filtered by TenantId
public async Task<IReadOnlyCollection<CustomerAccount>> GetTenantAccountsAsync(string tenantId)
{
    // Infrastructure globally filters: account.TenantId == tenantId
    return await _repository.GetAllAsync();
}

// Data isolation is enforced
var tenantAAccounts = await _accountRepo.GetAllAsync(); // Only Tenant A's accounts
// Tenant B cannot access Tenant A's accounts even with direct DB access
```

---

## Base Entity Classes

### AuditableEntity<TId>: Generic Foundation

The core base class for all auditable, soft-deletable, tenant-scoped entities. Implements all three interfaces with automatic audit tracking.

```csharp
namespace SmartWorkz.Core.Entities;

public abstract class AuditableEntity<TId> : IAuditable, ISoftDeletable, ITenantScoped
{
    /// <summary>Primary key. Maps to the entity's PK column via EF configuration.</summary>
    public TId Id { get; set; } = default!;

    // --- IAuditable ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? UpdatedBy { get; set; }

    // --- ISoftDeletable ---
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // --- ITenantScoped ---
    public string? TenantId { get; set; }
}
```

**Design Notes:**
- Generic TId supports any key type (int, string, Guid, long)
- CreatedAt defaults to UTC.Now at instantiation
- Other audit fields populated by infrastructure layer on SaveChanges
- Soft delete fields allow data recovery
- TenantId enables multi-tenant isolation

**Type Parameter Examples:**

```csharp
// Using AuditableEntity<TId>
public class Product : AuditableEntity<int> { }
public class UserProfile : AuditableEntity<string> { }
public class Order : AuditableEntity<Guid> { }

// All inherit:
// - Id: TId
// - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
// - IsDeleted, DeletedAt, DeletedBy
// - TenantId
```

---

### AuditableEntity: Integer Key Convenience

Short alias for AuditableEntity<int>. Use for Master/Report/Shared/Transaction entities with integer primary keys (common case).

```csharp
namespace SmartWorkz.Core.Entities;

public abstract class AuditableEntity : AuditableEntity<int>
{
}
```

**When to Use:**
- Master data (Country, Region, Product)
- Lookup tables (EntityStatus, Priority, Category)
- Report entities
- Transactional entities with auto-increment IDs

**Example:**

```csharp
// Equivalent: public class Category : AuditableEntity<int>
public class Category : AuditableEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
}

// Id is automatically int
var category = new Category { Name = "Electronics" };
category.Id = 1;  // int Id property inherited
```

---

## Aggregate Root: Domain Events

### AggregateRoot<TId> Class

Implements Domain-Driven Design aggregate root pattern with domain event support. Use for entities that are aggregate roots (boundary entities managing consistency).

```csharp
namespace SmartWorkz.Core.Shared.Base_Classes;

public abstract class AggregateRoot<TId> : IEntity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public TId Id { get; protected set; } = default!;
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Key Features:**
- Inherits IEntity<TId> for repository support
- Maintains internal list of domain events
- RaiseDomainEvent() protected method for subclasses
- ClearDomainEvents() removes all raised events
- Events published after SaveChanges() succeeds

**Domain Events Pattern:**

```csharp
// Define domain event
public class OrderCreatedEvent : DomainEvent
{
    public int OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
}

// Aggregate root using events
public class Order : AggregateRoot<Guid>
{
    private List<OrderLine> _items = new();
    
    public string OrderNumber { get; private set; }
    public DateTime OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public IReadOnlyCollection<OrderLine> Items => _items.AsReadOnly();

    // Factory method
    public static Order Create(string orderNumber, List<OrderLine> items, string createdBy)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            OrderDate = DateTime.UtcNow,
            _items = items,
            TotalAmount = items.Sum(i => i.SubTotal)
        };

        // Raise domain event for side effects (email, inventory, etc.)
        order.RaiseDomainEvent(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CreatedAt = order.OrderDate,
            CreatedBy = createdBy
        });

        return order;
    }

    public void AddItem(OrderLine item)
    {
        _items.Add(item);
        TotalAmount += item.SubTotal;
    }

    public void Complete()
    {
        RaiseDomainEvent(new OrderCompletedEvent { OrderId = Id });
    }
}

// Infrastructure publishes events after SaveChanges
public async Task<Result> CreateOrderAsync(CreateOrderDto dto)
{
    var order = Order.Create(dto.OrderNumber, dto.Items, currentUserId);
    
    await _repository.AddAsync(order);
    await _unitOfWork.SaveChangesAsync();
    
    // Infrastructure publishes all order.DomainEvents
    // Event handlers execute: send email, update inventory, etc.
    
    order.ClearDomainEvents();  // Prevent duplicate publication
    return Result.Ok();
}
```

---

### IDomainEvent Interface

Contract for all domain events. Events represent significant business occurrences within aggregates.

```csharp
namespace SmartWorkz.Core.Shared.Base_Classes;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
```

---

### DomainEvent: Base Implementation

Abstract base class for all domain events. Provides default EventId (new Guid) and OccurredAt (current UTC time).

```csharp
namespace SmartWorkz.Core.Shared.Base_Classes;

public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

**Usage:**

```csharp
public class UserRegisteredEvent : DomainEvent
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }
}

// Auto-populated on instantiation:
var evt = new UserRegisteredEvent
{
    UserId = "usr-123",
    Email = "user@example.com",
    RegisteredAt = DateTime.UtcNow
};
// EventId is auto-generated Guid
// OccurredAt is set to DateTime.UtcNow
```

---

## Service Base Classes

### ServiceBase<TEntity, TDto>: CRUD Implementation

Base class for all domain services. Provides generic CRUD operations with automatic mapping and result handling. Services are the application entry point for business operations.

```csharp
namespace SmartWorkz.Core.Services;

public abstract class ServiceBase<TEntity, TDto> : IService<TEntity, TDto>
    where TEntity : class, IEntity<int>
{
    protected readonly IRepository<TEntity, int> Repository;

    protected ServiceBase(IRepository<TEntity, int> repository)
    {
        Repository = Guard.NotNull(repository, nameof(repository));
    }

    public virtual async Task<Result<TDto>> GetByIdAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        Guard.NotDefault(id, nameof(id));

        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Fail<TDto>(new Error("ENTITY_NOT_FOUND", 
                $"{typeof(TEntity).Name} not found"));

        return Result.Ok(Map(entity));
    }

    public virtual async Task<Result<IReadOnlyCollection<TDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetAllAsync(cancellationToken);
        var dtos = entities.Select(Map).ToList().AsReadOnly();
        return Result.Ok<IReadOnlyCollection<TDto>>(dtos);
    }

    public virtual async Task<Result<TDto>> CreateAsync(
        TDto dto, 
        CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var entity = MapToEntity(dto);
        await Repository.AddAsync(entity, cancellationToken);
        return Result.Ok(Map(entity));
    }

    public virtual async Task<Result<TDto>> UpdateAsync(
        int id, 
        TDto dto, 
        CancellationToken cancellationToken = default)
    {
        Guard.NotDefault(id, nameof(id));
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Fail<TDto>(new Error("ENTITY_NOT_FOUND", 
                $"{typeof(TEntity).Name} not found"));

        ApplyUpdates(entity, dto);
        await Repository.UpdateAsync(entity, cancellationToken);
        return Result.Ok(Map(entity));
    }

    public virtual async Task<Result> DeleteAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        Guard.NotDefault(id, nameof(id));

        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Fail(new Error("ENTITY_NOT_FOUND", 
                $"{typeof(TEntity).Name} not found"));

        await Repository.DeleteAsync(id, cancellationToken);
        return Result.Ok();
    }

    protected abstract TDto Map(TEntity entity);
    protected abstract TEntity MapToEntity(TDto dto);
    protected virtual void ApplyUpdates(TEntity entity, TDto dto) { }
}
```

**CRUD Methods:**

| Method | Signature | Purpose |
|--------|-----------|---------|
| GetByIdAsync | (int id) → Result<TDto> | Fetch single entity by primary key |
| GetAllAsync | () → Result<IReadOnlyCollection<TDto>> | Fetch all entities |
| CreateAsync | (TDto dto) → Result<TDto> | Create new entity from DTO |
| UpdateAsync | (int id, TDto dto) → Result<TDto> | Update existing entity |
| DeleteAsync | (int id) → Result | Delete entity by ID |

**Abstract Methods (Implement in Subclass):**

| Method | Signature | Purpose |
|--------|-----------|---------|
| Map | (TEntity entity) → TDto | Entity to DTO mapping |
| MapToEntity | (TDto dto) → TEntity | DTO to Entity mapping |
| ApplyUpdates | (TEntity entity, TDto dto) → void | Optional: apply DTO changes to entity |

**Service Implementation Example:**

```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class ProductService : ServiceBase<Product, ProductDto>
{
    public ProductService(IRepository<Product, int> repository) 
        : base(repository)
    {
    }

    protected override ProductDto Map(Product entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price
        };

    protected override Product MapToEntity(ProductDto dto) =>
        new()
        {
            Name = dto.Name,
            Price = dto.Price
        };

    protected override void ApplyUpdates(Product entity, ProductDto dto)
    {
        entity.Name = dto.Name;
        entity.Price = dto.Price;
    }
}

// Usage in Controller
[HttpGet("{id}")]
public async Task<IActionResult> GetProduct(int id)
{
    var result = await _productService.GetByIdAsync(id);
    return result.IsSuccess 
        ? Ok(result.Value) 
        : NotFound(result.Error);
}

[HttpPost]
public async Task<IActionResult> CreateProduct(ProductDto dto)
{
    var result = await _productService.CreateAsync(dto);
    return result.IsSuccess 
        ? CreatedAtAction("GetProduct", new { id = result.Value.Id }, result.Value)
        : BadRequest(result.Error);
}
```

---

## Entity Status Enumeration

### EntityStatus Enum

Standard enumeration for entity lifecycle states. Use to control soft deletion and visibility logic at the domain level.

```csharp
namespace SmartWorkz.Core.Enums;

public enum EntityStatus
{
    [Display(Name = "Active")]
    Active = 0,
    
    [Display(Name = "Inactive")]
    Inactive = 1,
    
    [Display(Name = "Archived")]
    Archived = 2,
    
    [Display(Name = "Deleted")]
    Deleted = 3
}
```

**Usage Pattern:**

```csharp
public class Product : AuditableEntity
{
    public string Name { get; set; }
    public EntityStatus Status { get; set; }
}

// Setting status
var product = new Product
{
    Name = "Widget",
    Status = EntityStatus.Active
};

// Filtering by status
var activeProducts = await _repository.FindAllAsync(
    new Specification<Product>(p => p.Status == EntityStatus.Active && !p.IsDeleted)
);

// Deactivating (alternative to soft delete)
public async Task DeactivateProductAsync(int productId)
{
    var product = await _repository.GetByIdAsync(productId);
    product.Status = EntityStatus.Inactive;
    await _repository.UpdateAsync(product);
}
```

---

## System Constants

### AppConstants: Core Configuration

Application-wide constants for caching, validation, messaging, and pagination defaults.

```csharp
namespace SmartWorkz.Core.Constants;

public static class AppConstants
{
    public const string DefaultCulture = "en-US";
    public const string DefaultTimeZone = "UTC";
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;
    public const int MinPageSize = 1;

    public static class Cache
    {
        public const string KeyPrefix = "smartworkz:";
        public const int DefaultDurationMinutes = 30;
        public const int LongDurationHours = 24;
        public const int ShortDurationMinutes = 5;
    }

    public static class Validation
    {
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int MinNameLength = 1;
        public const int MaxNameLength = 256;
        public const int MaxEmailLength = 256;
        public const int MaxPhoneLength = 20;
    }

    public static class Messages
    {
        public const string Required = "This field is required";
        public const string InvalidFormat = "Invalid format";
        public const string NotFound = "Resource not found";
        public const string Unauthorized = "Unauthorized access";
    }
}
```

**Usage:**

```csharp
// Caching
var cacheKey = AppConstants.Cache.KeyPrefix + "products:all";
var ttl = TimeSpan.FromMinutes(AppConstants.Cache.DefaultDurationMinutes);

// Validation
if (password.Length < AppConstants.Validation.MinPasswordLength)
    throw new ValidationException(AppConstants.Messages.Required);

// Pagination
var pageSize = Math.Min(request.PageSize, AppConstants.MaxPageSize);

// Globalization
var culture = new CultureInfo(AppConstants.DefaultCulture);
```

---

### SharedConstants: Shared Defaults

Comprehensive configuration shared across all layers: pagination, validation, security, formatting, caching, and standard messages.

```csharp
namespace SmartWorkz.Core.Shared.Constants;

public static class SharedConstants
{
    public static class Pagination
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int MinPageSize = 1;
        public const int DefaultPage = 1;
    }

    public static class Validation
    {
        public const int MinNameLength = 1;
        public const int MaxNameLength = 255;
        public const int MinEmailLength = 5;
        public const int MaxEmailLength = 254;
        public const int MinPhoneLength = 10;
        public const int MaxPhoneLength = 20;
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int MaxCommentLength = 5000;
        public const int MaxDescriptionLength = 2000;
        public const int MaxAddressLength = 500;
    }

    public static class Security
    {
        public const int PasswordHashIterations = 10000;
        public const int EncryptionKeyLength = 32;     // 256 bits
        public const int SaltLength = 16;              // 128 bits
    }

    public static class Formatting
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string TimeFormat = "HH:mm:ss";
        public const string CurrencyFormat = "C2";
        public const string PercentFormat = "P2";
    }

    public static class Caching
    {
        public const string KeyPrefix = "sw:";
        public const int DefaultDurationMinutes = 30;
        public const int ShortDurationMinutes = 5;
        public const int LongDurationMinutes = 120;
    }

    public static class Messages
    {
        public const string Success = "Operation completed successfully.";
        public const string Error = "An error occurred while processing your request.";
        public const string NotFound = "The requested resource was not found.";
        public const string Unauthorized = "You are not authorized to perform this action.";
        public const string ValidationFailed = "One or more validation errors occurred.";
        public const string OperationCancelled = "The operation was cancelled.";
        public const string DuplicateFound = "A record with this value already exists.";
    }
}
```

**Usage Examples:**

```csharp
// Pagination with safe boundaries
public PaginatedList<T> Paginate<T>(IEnumerable<T> items, int pageNumber, int pageSize)
{
    var validPageSize = Math.Min(pageSize, SharedConstants.Pagination.MaxPageSize);
    var validPageNumber = Math.Max(pageNumber, SharedConstants.Pagination.DefaultPage);
    
    return new PaginatedList<T>(items, validPageNumber, validPageSize);
}

// Validation with SharedConstants
[StringLength(
    SharedConstants.Validation.MaxNameLength, 
    MinimumLength = SharedConstants.Validation.MinNameLength)]
public string Name { get; set; }

// Security: password hashing
using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 
    SharedConstants.Security.PasswordHashIterations, HashAlgorithmName.SHA256);

// Formatting: date/time display
var formattedDate = dateTime.ToString(SharedConstants.Formatting.DateTimeFormat);
var currencyValue = amount.ToString(SharedConstants.Formatting.CurrencyFormat);

// Caching with shared constants
var cacheKey = SharedConstants.Caching.KeyPrefix + entityId;
var cacheDuration = TimeSpan.FromMinutes(SharedConstants.Caching.DefaultDurationMinutes);

// Standard error messages
if (result.IsFailure)
    _logger.LogError(SharedConstants.Messages.Error);
```

---

## Complete Domain Example: Order Aggregate

Full working example combining all domain abstractions: aggregate root, entities, domain events, service, and constants.

```csharp
// Domain Events
public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OrderShippedEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public DateTime ShippedDate { get; set; }
    public string TrackingNumber { get; set; }
}

// Value Object
public class OrderLine : AuditableEntity<int>
{
    public Guid OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    public decimal SubTotal => Quantity * UnitPrice;
}

// Aggregate Root
public class Order : AggregateRoot<Guid>, IAuditable, ISoftDeletable, ITenantScoped
{
    private List<OrderLine> _items = new();
    
    public string OrderNumber { get; private set; }
    public DateTime OrderDate { get; private set; }
    public EntityStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? TrackingNumber { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Soft delete fields
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // Tenant field
    public string? TenantId { get; set; }
    
    public IReadOnlyCollection<OrderLine> Items => _items.AsReadOnly();
    
    // Factory method
    public static Order Create(
        string orderNumber, 
        List<OrderLine> items, 
        string tenantId,
        string createdBy)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Order must have at least one item");
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            OrderDate = DateTime.UtcNow,
            Status = EntityStatus.Active,
            _items = items,
            TotalAmount = items.Sum(i => i.SubTotal),
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
        
        order.RaiseDomainEvent(new OrderCreatedEvent
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount
        });
        
        return order;
    }
    
    public void Ship(string trackingNumber)
    {
        if (Status != EntityStatus.Active)
            throw new InvalidOperationException("Can only ship active orders");
        
        TrackingNumber = trackingNumber;
        
        RaiseDomainEvent(new OrderShippedEvent
        {
            OrderId = Id,
            ShippedDate = DateTime.UtcNow,
            TrackingNumber = trackingNumber
        });
    }
}

// DTOs
public class OrderLineDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public EntityStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderLineDto> Items { get; set; }
}

// Service
public class OrderService : ServiceBase<Order, OrderDto>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ITenantContext _tenantContext;
    
    public OrderService(
        IRepository<Order, int> repository,
        IEventPublisher eventPublisher,
        ITenantContext tenantContext) 
        : base(repository)
    {
        _eventPublisher = Guard.NotNull(eventPublisher, nameof(eventPublisher));
        _tenantContext = Guard.NotNull(tenantContext, nameof(tenantContext));
    }
    
    public async Task<Result<OrderDto>> CreateOrderAsync(
        OrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var items = dto.Items
            .Select(i => new OrderLine { ProductId = i.ProductId, Quantity = i.Quantity })
            .ToList();
        
        var order = Order.Create(
            dto.OrderNumber,
            items,
            _tenantContext.TenantId,
            _tenantContext.UserId);
        
        await Repository.AddAsync(order, cancellationToken);
        
        // Publish domain events
        foreach (var domainEvent in order.DomainEvents)
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        
        order.ClearDomainEvents();
        return Result.Ok(Map(order));
    }
    
    public async Task<Result<OrderDto>> ShipOrderAsync(Guid orderId, string trackingNumber)
    {
        // Cast id for base Repository (which uses int)
        // In real app, custom repository would handle Guid
        throw new NotImplementedException("Guid PK requires custom repository");
    }
    
    protected override OrderDto Map(Order entity) =>
        new()
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            OrderDate = entity.OrderDate,
            Status = entity.Status,
            TotalAmount = entity.TotalAmount,
            Items = entity.Items
                .Select(i => new OrderLineDto 
                { 
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList()
        };
    
    protected override Order MapToEntity(OrderDto dto) =>
        new()
        {
            OrderNumber = dto.OrderNumber,
            OrderDate = dto.OrderDate,
            Status = dto.Status,
            TotalAmount = dto.TotalAmount
        };
}
```

---

## Dependency Injection Setup

Complete example of wiring all abstractions together in DI container.

```csharp
// Program.cs - Startup configuration
public static void AddDomainServices(this IServiceCollection services)
{
    // Repositories
    services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
    
    // Unit of Work
    services.AddScoped<IUnitOfWork, EfUnitOfWork>();
    
    // Services
    services.AddScoped<ProductService>();
    services.AddScoped<OrderService>();
    services.AddScoped<CategoryService>();
    
    // Event Publishing
    services.AddScoped<IEventPublisher, InMemoryEventPublisher>();
    services.AddScoped<IEventSubscriber, InMemoryEventSubscriber>();
    
    // Infrastructure
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
    );
    
    // Request context (for TenantId, UserId)
    services.AddScoped<ITenantContext>(sp =>
    {
        var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
        var tenantId = httpContext?.User?.FindFirst("tenant_id")?.Value ?? "default";
        var userId = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return new TenantContext { TenantId = tenantId, UserId = userId };
    });
}

// Controller usage
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    
    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDto dto)
    {
        var result = await _productService.CreateAsync(dto);
        return result.IsSuccess 
            ? CreatedAtAction("GetProduct", new { id = result.Value.Id }, result.Value)
            : BadRequest(result.Error);
    }
}
```

---

## Best Practices & Patterns

### 1. Always Use Base Classes for New Entities

```csharp
// Good: inherits audit, soft delete, tenancy
public class Product : AuditableEntity { }

// Avoid: missing infrastructure support
public class Product { public int Id { get; set; } }
```

### 2. Use Generic TId for Flexibility

```csharp
// Good: works with any key type
public class Entity : AuditableEntity<TId> { }

// Avoid: locked to int
public class Entity : AuditableEntity { }
```

### 3. Aggregate Roots for Complex Domains

```csharp
// Good: Order is an aggregate root managing OrderLines
public class Order : AggregateRoot<Guid> { }

// Avoid: treating every entity as an aggregate
public class OrderLine : AuditableEntity { } // OK for non-root
```

### 4. Immutable Business Logic

```csharp
// Good: private setters, factory methods
public class Order : AggregateRoot<Guid>
{
    public string OrderNumber { get; private set; }
    public static Order Create(...) { }
}

// Avoid: public setters for business-critical properties
public class Order { public string OrderNumber { get; set; } }
```

### 5. Service CRUD + Custom Methods

```csharp
public class OrderService : ServiceBase<Order, OrderDto>
{
    // Base CRUD: GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync
    
    // Custom business methods
    public async Task<Result> ShipOrderAsync(int orderId, string trackingNumber)
    {
        var order = await Repository.GetByIdAsync(orderId);
        order.Ship(trackingNumber);
        await Repository.UpdateAsync(order);
        return Result.Ok();
    }
}
```

---

## Common Troubleshooting

**Q: Why is my audit data null?**
A: Ensure EF interceptor is registered in DbContext configuration and SaveChanges is called through IUnitOfWork.

**Q: How do I query soft-deleted records?**
A: Use `.IgnoreQueryFilters()` on the DbSet to bypass global soft-delete filter.

**Q: Can I use string as TId instead of int?**
A: Yes. Use `AuditableEntity<string>` or `AggregateRoot<string>`. Ensure repository is registered with matching TId.

**Q: How are domain events published?**
A: After SaveChangesAsync succeeds, Infrastructure calls IEventPublisher.PublishAsync for each DomainEvent, then ClearDomainEvents().

**Q: What's the difference between IEntity and AuditableEntity?**
A: IEntity is the base contract; AuditableEntity adds audit tracking, soft deletion, and tenancy.

---

## Summary

SmartWorkz domain abstractions provide:
- **Type-safe entity contracts** via generic IEntity<TId>
- **Automatic audit tracking** (IAuditable)
- **Safe deletion** (ISoftDeletable)
- **Multi-tenant isolation** (ITenantScoped)
- **Domain-driven aggregates** (AggregateRoot<TId>)
- **Consistent CRUD services** (ServiceBase<TEntity, TDto>)
- **Centralized configuration** (AppConstants, SharedConstants)

Use these abstractions to build domain models that are maintainable, testable, and aligned with business requirements.
