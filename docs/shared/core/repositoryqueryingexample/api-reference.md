# RepositoryQueryingExample API Reference

## Classes & Interfaces

### CustomerSpecification

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.CustomerSpecification`
- **Summary:** Customer Specification for queries.

### RepositoryPatterns

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.RepositoryPatterns`
- **Summary:** Demonstrates repository query patterns.
            Note: In real usage, repositories are injected via DI.

#### Methods & Properties

- **Example_GetById** - Example 1: Get customer by ID.
            Returns null if not found (no exception).
- **Example_GetAll** - Example 2: Get all customers.
            Use with caution on large tables; consider pagination.
- **Example_FindAllWithSpec** - Example 3: Query with Specification.
            Returns all entities matching specification criteria.
- **Example_FindAsync** - Example 4: Find single entity matching specification.
            Returns first match or null.
- **Example_Count** - Example 5: Count entities matching specification.
- **Example_Exists** - Example 6: Check if entities exist.
- **Example_Add** - Example 7: Add new customer.
            Changes persist when UnitOfWork.SaveAsync() is called.
- **Example_Update** - Example 8: Update customer.
- **Example_Delete** - Example 9: Delete customer (hard delete).
            For soft delete, use customer.Delete(userId) and UpdateAsync.
- **Example_BulkOperations** - Example 10: Bulk operations.

### SoftDeleteFiltering

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.SoftDeleteFiltering`
- **Summary:** Demonstrates soft-delete filtering (automatic by repositories).

#### Methods & Properties

- **Example_AutomaticFiltering** - Repositories automatically exclude soft-deleted entities.
            WHERE IsDeleted = false is applied implicitly.

### MultiTenancySupport

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.MultiTenancySupport`
- **Summary:** Demonstrates multi-tenancy support (automatic filtering).

#### Methods & Properties

- **Example_TenantFiltering** - Repository automatically filters by current tenant.
            WHERE TenantId = currentTenantId is applied implicitly.

### CustomerSpecification

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.CustomerSpecification`
- **Summary:** Customer Specification for queries.

### RepositoryPatterns

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.RepositoryPatterns`
- **Summary:** Demonstrates repository query patterns.
            Note: In real usage, repositories are injected via DI.

#### Methods & Properties

- **Example_GetById** - Example 1: Get customer by ID.
            Returns null if not found (no exception).
- **Example_GetAll** - Example 2: Get all customers.
            Use with caution on large tables; consider pagination.
- **Example_FindAllWithSpec** - Example 3: Query with Specification.
            Returns all entities matching specification criteria.
- **Example_FindAsync** - Example 4: Find single entity matching specification.
            Returns first match or null.
- **Example_Count** - Example 5: Count entities matching specification.
- **Example_Exists** - Example 6: Check if entities exist.
- **Example_Add** - Example 7: Add new customer.
            Changes persist when UnitOfWork.SaveAsync() is called.
- **Example_Update** - Example 8: Update customer.
- **Example_Delete** - Example 9: Delete customer (hard delete).
            For soft delete, use customer.Delete(userId) and UpdateAsync.
- **Example_BulkOperations** - Example 10: Bulk operations.

### SoftDeleteFiltering

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.SoftDeleteFiltering`
- **Summary:** Demonstrates soft-delete filtering (automatic by repositories).

#### Methods & Properties

- **Example_AutomaticFiltering** - Repositories automatically exclude soft-deleted entities.
            WHERE IsDeleted = false is applied implicitly.

### MultiTenancySupport

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.MultiTenancySupport`
- **Summary:** Demonstrates multi-tenancy support (automatic filtering).

#### Methods & Properties

- **Example_TenantFiltering** - Repository automatically filters by current tenant.
            WHERE TenantId = currentTenantId is applied implicitly.

### CustomerSpecification

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.CustomerSpecification`
- **Summary:** Customer Specification for queries.

### RepositoryPatterns

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.RepositoryPatterns`
- **Summary:** Demonstrates repository query patterns.
            Note: In real usage, repositories are injected via DI.

#### Methods & Properties

- **Example_GetById** - Example 1: Get customer by ID.
            Returns null if not found (no exception).
- **Example_GetAll** - Example 2: Get all customers.
            Use with caution on large tables; consider pagination.
- **Example_FindAllWithSpec** - Example 3: Query with Specification.
            Returns all entities matching specification criteria.
- **Example_FindAsync** - Example 4: Find single entity matching specification.
            Returns first match or null.
- **Example_Count** - Example 5: Count entities matching specification.
- **Example_Exists** - Example 6: Check if entities exist.
- **Example_Add** - Example 7: Add new customer.
            Changes persist when UnitOfWork.SaveAsync() is called.
- **Example_Update** - Example 8: Update customer.
- **Example_Delete** - Example 9: Delete customer (hard delete).
            For soft delete, use customer.Delete(userId) and UpdateAsync.
- **Example_BulkOperations** - Example 10: Bulk operations.

### SoftDeleteFiltering

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.SoftDeleteFiltering`
- **Summary:** Demonstrates soft-delete filtering (automatic by repositories).

#### Methods & Properties

- **Example_AutomaticFiltering** - Repositories automatically exclude soft-deleted entities.
            WHERE IsDeleted = false is applied implicitly.

### MultiTenancySupport

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.MultiTenancySupport`
- **Summary:** Demonstrates multi-tenancy support (automatic filtering).

#### Methods & Properties

- **Example_TenantFiltering** - Repository automatically filters by current tenant.
            WHERE TenantId = currentTenantId is applied implicitly.

### CustomerSpecification

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.CustomerSpecification`
- **Summary:** Customer Specification for queries.

### RepositoryPatterns

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.RepositoryPatterns`
- **Summary:** Demonstrates repository query patterns.
            Note: In real usage, repositories are injected via DI.

#### Methods & Properties

- **Example_GetById** - Example 1: Get customer by ID.
            Returns null if not found (no exception).
- **Example_GetAll** - Example 2: Get all customers.
            Use with caution on large tables; consider pagination.
- **Example_FindAllWithSpec** - Example 3: Query with Specification.
            Returns all entities matching specification criteria.
- **Example_FindAsync** - Example 4: Find single entity matching specification.
            Returns first match or null.
- **Example_Count** - Example 5: Count entities matching specification.
- **Example_Exists** - Example 6: Check if entities exist.
- **Example_Add** - Example 7: Add new customer.
            Changes persist when UnitOfWork.SaveAsync() is called.
- **Example_Update** - Example 8: Update customer.
- **Example_Delete** - Example 9: Delete customer (hard delete).
            For soft delete, use customer.Delete(userId) and UpdateAsync.
- **Example_BulkOperations** - Example 10: Bulk operations.

### SoftDeleteFiltering

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.SoftDeleteFiltering`
- **Summary:** Demonstrates soft-delete filtering (automatic by repositories).

#### Methods & Properties

- **Example_AutomaticFiltering** - Repositories automatically exclude soft-deleted entities.
            WHERE IsDeleted = false is applied implicitly.

### MultiTenancySupport

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.MultiTenancySupport`
- **Summary:** Demonstrates multi-tenancy support (automatic filtering).

#### Methods & Properties

- **Example_TenantFiltering** - Repository automatically filters by current tenant.
            WHERE TenantId = currentTenantId is applied implicitly.

### CustomerSpecification

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.CustomerSpecification`
- **Summary:** Customer Specification for queries.

### RepositoryPatterns

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.RepositoryPatterns`
- **Summary:** Demonstrates repository query patterns.
            Note: In real usage, repositories are injected via DI.

#### Methods & Properties

- **Example_GetById** - Example 1: Get customer by ID.
            Returns null if not found (no exception).
- **Example_GetAll** - Example 2: Get all customers.
            Use with caution on large tables; consider pagination.
- **Example_FindAllWithSpec** - Example 3: Query with Specification.
            Returns all entities matching specification criteria.
- **Example_FindAsync** - Example 4: Find single entity matching specification.
            Returns first match or null.
- **Example_Count** - Example 5: Count entities matching specification.
- **Example_Exists** - Example 6: Check if entities exist.
- **Example_Add** - Example 7: Add new customer.
            Changes persist when UnitOfWork.SaveAsync() is called.
- **Example_Update** - Example 8: Update customer.
- **Example_Delete** - Example 9: Delete customer (hard delete).
            For soft delete, use customer.Delete(userId) and UpdateAsync.
- **Example_BulkOperations** - Example 10: Bulk operations.

### SoftDeleteFiltering

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.SoftDeleteFiltering`
- **Summary:** Demonstrates soft-delete filtering (automatic by repositories).

#### Methods & Properties

- **Example_AutomaticFiltering** - Repositories automatically exclude soft-deleted entities.
            WHERE IsDeleted = false is applied implicitly.

### MultiTenancySupport

- **Namespace:** `SmartWorkz.Core.Examples.RepositoryQueryingExample.MultiTenancySupport`
- **Summary:** Demonstrates multi-tenancy support (automatic filtering).

#### Methods & Properties

- **Example_TenantFiltering** - Repository automatically filters by current tenant.
            WHERE TenantId = currentTenantId is applied implicitly.

