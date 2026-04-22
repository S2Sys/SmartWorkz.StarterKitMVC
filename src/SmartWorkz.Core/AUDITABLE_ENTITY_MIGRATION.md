# AuditableEntity Refactoring and Migration Guide

## Overview

The `AuditableEntity<TId>` and `AuditableEntity` classes have been refactored to align with the new standalone entity design. This guide explains the changes, why they were made, and how to migrate your code.

## What Changed

### Before (Old Design)
`AuditableEntity<TId>` was a monolithic class that combined:
- Audit tracking (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
- Soft delete support (IsDeleted, DeletedAt, DeletedBy)
- Inheritance hierarchy complexity

```csharp
// Old hierarchical design
public abstract class AuditableEntity<TId> : IAuditable, ISoftDeletable
{
    public TId Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}
```

### After (New Design)
The responsibility has been split into:
- **AuditEntity<TId>**: Audit tracking only (inherits from Entity<TId>)
- **AuditableEntity<TId>**: Now marked as Obsolete, delegates to AuditEntity<TId> for backward compatibility
- Soft delete support is now a separate concern (implement ISoftDeletable explicitly)

```csharp
// New standalone design
public abstract class AuditEntity<TId> : Entity<TId>, IAuditable
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

// Legacy class for backward compatibility (DEPRECATED)
[Obsolete("Use AuditEntity<TId> instead...")]
public abstract class AuditableEntity<TId> : AuditEntity<TId>
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}
```

## Why This Change?

1. **Eliminates forced inheritance chains**: Entities that only need audit tracking don't need soft delete properties
2. **Makes responsibilities explicit**: Clear separation of concerns in the codebase
3. **Improves maintainability**: Easier to understand what each class provides
4. **Aligns with DDD principles**: Entities should have focused responsibilities
5. **Reduces property clutter**: Unnecessary properties are no longer inherited by default

## Backward Compatibility

Your existing code will continue to work without changes:

```csharp
// This still works (but generates an obsolete warning)
public class Product : AuditableEntity<int>
{
    public string Name { get; set; }
}
```

However, you'll see a compile-time warning encouraging migration.

## Migration Path

### Option 1: Audit Tracking Only (Recommended for most cases)

If your entity only needs audit tracking, simply replace `AuditableEntity` with `AuditEntity`:

```csharp
// Before
public class Product : AuditableEntity<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// After
public class Product : AuditEntity<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

**Benefits:**
- No compiler warnings
- Cleaner code
- Clear intent: "this entity is audited"

### Option 2: Audit + Soft Delete (When both are needed)

If you need both audit tracking AND soft delete, create a specialized entity class:

```csharp
// Define once as a base for all soft-deletable audited entities
public abstract class AuditDeletableEntity<TId> : AuditEntity<TId>, ISoftDeletable
    where TId : notnull, IEquatable<TId>
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}

// Then use it in your domain entities
public class Order : AuditDeletableEntity<int>
{
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
}

public class Invoice : AuditDeletableEntity<Guid>
{
    public string InvoiceNumber { get; set; }
    public Money Amount { get; set; }
}
```

**Benefits:**
- Explicit about soft delete support
- Reusable for multiple entities
- Clear intent in code

### Option 3: Multi-tenancy + Audit + Soft Delete

For complex scenarios, build composition-based solutions:

```csharp
public abstract class TenantAuditDeletableEntity<TId> : 
    AuditEntity<TId>, ISoftDeletable, ITenantScoped
    where TId : notnull, IEquatable<TId>
{
    public int TenantId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}
```

## Implementation Details

### Entity Inheritance Chain

**New Design:**

```
Entity<TId> (identity-based equality)
    ↓
AuditEntity<TId> (adds audit tracking)
    ↓
(Optionally implement ISoftDeletable in subclasses)
```

**Legacy Design (now deprecated):**

```
Entity<TId>
    ↓
AuditableEntity<TId> (audit + soft delete combined)
    ↓
(Used by existing entities)
```

### Interface Contracts

**IAuditable** (read-only properties):
- `DateTime CreatedAt { get; }`
- `string CreatedBy { get; }`
- `DateTime? UpdatedAt { get; }`
- `string? UpdatedBy { get; }`

**ISoftDeletable** (read-write properties):
- `bool IsDeleted { get; set; }`
- `DateTime? DeletedAt { get; set; }`
- `int? DeletedBy { get; set; }`

## Migration Checklist

- [ ] Review entities inheriting from `AuditableEntity<TId>`
- [ ] Identify which need soft delete support
- [ ] For audit-only entities: change to `AuditEntity<TId>`
- [ ] For audit + soft delete: create specialized base class
- [ ] Run full test suite to verify no breaking changes
- [ ] Update ARCHITECTURE.md examples if needed
- [ ] Remove obsolete warnings in build output

## Common Patterns

### Soft Delete Queries

Regardless of whether you use `AuditableEntity` or custom `AuditDeletableEntity`, soft delete queries remain the same:

```csharp
// Query active records (not deleted)
var activeProducts = await context.Products
    .Where(p => !p.IsDeleted)
    .ToListAsync();

// Query deleted records (for admin/recovery)
var deletedProducts = await context.Products
    .Where(p => p.IsDeleted)
    .OrderByDescending(p => p.DeletedAt)
    .ToListAsync();
```

### Repository Base Class

If you have a base repository, consider parameterizing soft delete behavior:

```csharp
public class GenericRepository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
{
    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        var query = context.Set<TEntity>();

        // Only filter soft deletes if entity implements ISoftDeletable
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(e => !((ISoftDeletable)e).IsDeleted);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
```

## See Also

- [ARCHITECTURE.md](./ARCHITECTURE.md) - Overall framework architecture
- [VALUE-OBJECTS.md](./VALUE-OBJECTS.md) - Value object patterns
- `Entity<TId>` - Base entity class with identity-based equality
- `AuditEntity<TId>` - New audit-focused entity class
- `ISoftDeletable` - Interface for soft delete support
- `IAuditable` - Interface for audit tracking

## Questions?

If you encounter issues during migration or have questions about the new design, refer to:
1. The detailed comments in the entity class files
2. The example files in the `Examples` folder
3. The test files in `SmartWorkz.Core.Tests/Entities`
