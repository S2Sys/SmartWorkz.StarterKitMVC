# SmartWorkz.Core Code Examples

This directory contains 9 comprehensive code examples demonstrating SmartWorkz.Core integration patterns and best practices.

## Example Files

### 1. SpecificationChainingExample.cs
**Pattern:** Specification<T> - Composable query building
- Building complex filters with chaining
- Eager loading with Includes()
- Sorting, pagination, and criteria combination
- Query reusability across services

**Key Concepts:**
- Fluent API for composable queries
- Specification<T> with AddCriteria(), AddInclude(), ApplyPaging()
- And/Or/Not for complex filter combinations

### 2. ResultPatternErrorHandlingExample.cs
**Pattern:** Result<T> - Type-safe error handling
- Explicit error codes instead of exceptions
- Result.IsSuccess checking
- Error propagation across service boundaries
- Value object creation with validation

**Key Concepts:**
- Result<T> and Result (non-generic)
- Error struct with Code and Message
- Chain Results with error handling
- No exceptions for business errors

### 3. RepositoryQueryingExample.cs
**Pattern:** IRepository<TEntity, TId> - Data access abstraction
- GetByIdAsync, GetAllAsync operations
- FindAsync, FindAllAsync with specifications
- CountAsync, ExistsAsync for existence checks
- AddAsync, UpdateAsync, DeleteAsync for CRUD

**Key Concepts:**
- Repository pattern with Specification support
- Implicit soft-delete filtering
- Multi-tenancy support (TenantId)
- Batch operations (AddRangeAsync, DeleteRangeAsync)

### 4. ServiceCRUDOperationsExample.cs
**Pattern:** IService<TEntity, TDto> - Business logic layer
- Create (POST) with Result<DTO> response
- Read (GET) with entity-to-DTO mapping
- Update (PUT) with validation and business logic
- Delete (DELETE) with soft-delete support
- List (GET) with pagination

**Key Concepts:**
- Service layer encapsulates business logic
- Result-based error handling
- Entity-to-DTO mapping
- Dependency injection of repositories
- Validation and authorization checks

### 5. GuardClauseValidationExample.cs
**Pattern:** Guard - Precondition validation
- Guard.NotNull() for reference types and nullable value types
- Guard.NotEmpty() for strings and collections
- Guard.NotDefault() for IDs and unique values
- Guard.InRange() for comparative validation
- Guard.Requires() for custom conditions

**Key Concepts:**
- Fail-fast validation at method entry
- Clear, specific exceptions for debugging
- Parameter name included in error messages
- Used at domain boundaries (constructors, service methods)

### 6. MultiTenancyFilteringExample.cs
**Pattern:** Tenant isolation - Automatic data scoping
- Tenant-aware repository queries
- Automatic WHERE TenantId = currentTenant filtering
- Tenant resolution from request context
- Prevention of cross-tenant data access

**Key Concepts:**
- TenantId property on AuditableEntity
- Implicit tenant filtering in repositories
- ITenantContextProvider for tenant resolution
- Complete data isolation between tenants

### 7. SoftDeleteBehaviorExample.cs
**Pattern:** Soft delete - Non-destructive data removal
- Mark entities as deleted without removal
- IsDeleted, DeletedAt, DeletedBy tracking
- Automatic soft-delete filtering in queries
- Data recovery within grace period

**Key Concepts:**
- Entity.Delete(userId) method
- Entity.Restore() for recovery
- Audit trail (who deleted, when)
- Repository filters soft-deleted by default
- Hard delete only after grace period

### 8. DTOMappingExample.cs
**Pattern:** DTO mapping - API contract transformation
- Entity to DTO (Read operations)
- DTO to Entity (Create/Update operations)
- Flattening nested entities/value objects
- Selective field exposure (security)
- Batch mapping for performance

**Key Concepts:**
- Map at service boundaries
- Flatten value objects (PersonName, Address, Money)
- Transform data for presentation
- Separate internal model from API contract

### 9. AuditTrailQueriesExample.cs
**Pattern:** Audit trail - Compliance and forensics
- Query recently modified entities
- Find entities created by specific user
- Soft-deleted entities recovery audit
- Compliance reporting
- Forensics for unauthorized changes

**Key Concepts:**
- CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
- IsDeleted, DeletedAt, DeletedBy for soft deletes
- Time-based filtering (date ranges)
- User-based filtering (who did what)
- Audit specifications for reports

## Usage Patterns

### Typical Service Layer Flow

```
DTO Input
    ↓
Validate (Guard, Result)
    ↓
Map to Entity
    ↓
Apply Business Logic
    ↓
Persist (Repository.AddAsync/UpdateAsync)
    ↓
Map Entity to DTO
    ↓
Return Result<DTO>
```

### Query Flow with Specifications

```
Build Specification
    ↓
Combine Filters (And/Or)
    ↓
Repository.FindAsync(spec)
    ↓
Automatic filtering (soft-delete, tenant)
    ↓
Eager loading (Includes)
    ↓
Sorting and Pagination
    ↓
Map to DTOs
    ↓
Return Results
```

## Key SmartWorkz.Core Classes

- **AuditableEntity**: Base class with CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt, DeletedBy, TenantId
- **Specification<T>**: Composable query builder with criteria, includes, sorting, pagination
- **Result<T>**: Type-safe result with success/error states
- **Guard**: Precondition validation utility
- **EmailAddress, Money, PersonName, Address**: Value objects with validation
- **IRepository<TEntity, TId>**: Data access abstraction
- **IService<TEntity, TDto>**: Business logic abstraction

## Best Practices Demonstrated

1. **Guard clauses** at method entry points for fast failure
2. **Result<T> pattern** instead of exceptions for expected failures
3. **Specifications** for reusable, composable queries
4. **DTOs** to decouple API contracts from domain models
5. **Soft deletes** for compliance and data recovery
6. **Audit trails** for accountability and forensics
7. **Multi-tenancy** with automatic filtering
8. **Service layer** for business logic encapsulation
9. **Dependency injection** for loose coupling
10. **Value objects** for domain semantics

## Compilation Notes

These examples are conceptual demonstrations of patterns. In actual usage:
- Entities inherit from AuditableEntity which implements required interfaces
- Repositories are injected via dependency injection
- Services orchestrate repository and domain operations
- Build verification ensures pattern correctness

For working examples, see the actual domain models in SmartWorkz.StarterKitMVC sample application.
