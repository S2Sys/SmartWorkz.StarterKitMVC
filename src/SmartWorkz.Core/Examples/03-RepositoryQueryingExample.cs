namespace SmartWorkz.Core.Examples;

using SmartWorkz.Shared;

/// <summary>
/// Demonstrates IRepository<TEntity, TId> data access patterns.
/// Repositories provide abstraction over database operations with
/// built-in support for Specifications, soft-delete filtering, and multi-tenancy.
/// </summary>
public class RepositoryQueryingExample
{
    public class Customer : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Customer Specification for queries.
    /// </summary>
    public class CustomerSpecification : Specification<Customer>
    {
        public CustomerSpecification WithActive()
        {
            AddCriteria(c => c.IsActive);
            return this;
        }

        public CustomerSpecification WithNameContaining(string name)
        {
            AddCriteria(c => c.Name.Contains(name));
            return this;
        }

        public CustomerSpecification OrderByName()
        {
            ApplyOrderBy(c => c.Name);
            return this;
        }
    }

    /// <summary>
    /// Demonstrates repository query patterns.
    /// Note: In real usage, repositories are injected via DI.
    /// </summary>
    public class RepositoryPatterns
    {
        /// <summary>
        /// Example 1: Get customer by ID.
        /// Returns null if not found (no exception).
        /// </summary>
        public void Example_GetById()
        {
            // Real usage: var customer = await _repository.GetByIdAsync(customerId);
            System.Console.WriteLine("Get by ID: repository.GetByIdAsync(id)");
        }

        /// <summary>
        /// Example 2: Get all customers.
        /// Use with caution on large tables; consider pagination.
        /// </summary>
        public void Example_GetAll()
        {
            // Real usage: var customers = await _repository.GetAllAsync();
            System.Console.WriteLine("Get all: repository.GetAllAsync()");
        }

        /// <summary>
        /// Example 3: Query with Specification.
        /// Returns all entities matching specification criteria.
        /// </summary>
        public void Example_FindAllWithSpec()
        {
            // Real usage:
            // var spec = new CustomerSpecification().WithActive().OrderByName();
            // var customers = await _repository.FindAllAsync(spec);
            System.Console.WriteLine("Find all: repository.FindAllAsync(spec)");
        }

        /// <summary>
        /// Example 4: Find single entity matching specification.
        /// Returns first match or null.
        /// </summary>
        public void Example_FindAsync()
        {
            // Real usage:
            // var spec = new CustomerSpecification().WithNameContaining("John");
            // var customer = await _repository.FindAsync(spec);
            System.Console.WriteLine("Find one: repository.FindAsync(spec)");
        }

        /// <summary>
        /// Example 5: Count entities matching specification.
        /// </summary>
        public void Example_Count()
        {
            // Real usage:
            // var spec = new CustomerSpecification().WithActive();
            // var count = await _repository.CountAsync(spec);
            System.Console.WriteLine("Count: repository.CountAsync(spec)");
        }

        /// <summary>
        /// Example 6: Check if entities exist.
        /// </summary>
        public void Example_Exists()
        {
            // Real usage:
            // var spec = new CustomerSpecification().WithActive();
            // bool exists = await _repository.ExistsAsync(spec);
            System.Console.WriteLine("Exists: repository.ExistsAsync(spec)");
        }

        /// <summary>
        /// Example 7: Add new customer.
        /// Changes persist when UnitOfWork.SaveAsync() is called.
        /// </summary>
        public void Example_Add()
        {
            // Real usage:
            // var customer = new Customer { Name = "Alice", Email = "alice@example.com" };
            // await _repository.AddAsync(customer);
            // await _unitOfWork.SaveAsync();
            System.Console.WriteLine("Add: repository.AddAsync(entity)");
        }

        /// <summary>
        /// Example 8: Update customer.
        /// </summary>
        public void Example_Update()
        {
            // Real usage:
            // customer.Name = "Updated Name";
            // await _repository.UpdateAsync(customer);
            // await _unitOfWork.SaveAsync();
            System.Console.WriteLine("Update: repository.UpdateAsync(entity)");
        }

        /// <summary>
        /// Example 9: Delete customer (hard delete).
        /// For soft delete, use customer.Delete(userId) and UpdateAsync.
        /// </summary>
        public void Example_Delete()
        {
            // Real usage:
            // await _repository.DeleteAsync(customerId);
            // await _unitOfWork.SaveAsync();
            System.Console.WriteLine("Delete: repository.DeleteAsync(id)");
        }

        /// <summary>
        /// Example 10: Bulk operations.
        /// </summary>
        public void Example_BulkOperations()
        {
            // Real usage:
            // var customers = new[] { ... };
            // await _repository.AddRangeAsync(customers);
            // await _unitOfWork.SaveAsync();
            System.Console.WriteLine("Bulk add: repository.AddRangeAsync(entities)");
            System.Console.WriteLine("Bulk delete: repository.DeleteRangeAsync(entities)");
        }
    }

    /// <summary>
    /// Demonstrates soft-delete filtering (automatic by repositories).
    /// </summary>
    public class SoftDeleteFiltering
    {
        /// <summary>
        /// Repositories automatically exclude soft-deleted entities.
        /// WHERE IsDeleted = false is applied implicitly.
        /// </summary>
        public void Example_AutomaticFiltering()
        {
            // Real usage:
            // var customers = await _repository.GetAllAsync();
            // SQL: SELECT * FROM Customer WHERE IsDeleted = false (applied by repository)
            System.Console.WriteLine("Automatic soft-delete filtering");
        }

        /// <summary>
        /// To include soft-deleted, modify specification.
        /// </summary>
        public class DeletedCustomerSpec : Specification<Customer>
        {
            public DeletedCustomerSpec()
            {
                // Include soft-deleted (remove auto-filter)
                AddCriteria(c => c.IsDeleted);
            }
        }
    }

    /// <summary>
    /// Demonstrates multi-tenancy support (automatic filtering).
    /// </summary>
    public class MultiTenancySupport
    {
        /// <summary>
        /// Repository automatically filters by current tenant.
        /// WHERE TenantId = currentTenantId is applied implicitly.
        /// </summary>
        public void Example_TenantFiltering()
        {
            // Real usage:
            // Repository<Customer> repository = new Repository<Customer>(dbContext, currentTenantId);
            // var customers = await repository.GetAllAsync();
            // SQL: SELECT * FROM Customer WHERE TenantId = @tenantId (applied by repository)
            System.Console.WriteLine("Automatic tenant filtering");
        }
    }
}
