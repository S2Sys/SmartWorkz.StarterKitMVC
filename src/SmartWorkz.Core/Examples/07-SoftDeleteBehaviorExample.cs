namespace SmartWorkz.Core.Examples;

/// <summary>
/// Demonstrates soft delete pattern for non-destructive data removal.
/// Soft delete marks entities as deleted without removing them from the database,
/// enabling recovery, audit trails, and data retention compliance.
/// </summary>
public class SoftDeleteBehaviorExample
{
    public class Customer : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Inherited soft-delete properties:
        // IsDeleted (bool), DeletedAt (DateTime?), DeletedBy (string?)
    }

    /// <summary>
    /// Customer specification with soft delete filters.
    /// </summary>
    public class CustomerSpecification : Specification<Customer>
    {
        /// <summary>
        /// Include only active (non-deleted) customers.
        /// </summary>
        public CustomerSpecification WithActive()
        {
            AddCriteria(c => !c.IsDeleted);
            return this;
        }

        /// <summary>
        /// Include only soft-deleted customers.
        /// </summary>
        public CustomerSpecification WithDeleted()
        {
            AddCriteria(c => c.IsDeleted);
            return this;
        }

        /// <summary>
        /// Filter deleted customers within date range.
        /// </summary>
        public CustomerSpecification DeletedSince(DateTime date)
        {
            AddCriteria(c => c.DeletedAt >= date);
            return this;
        }

        public CustomerSpecification OrderByName()
        {
            ApplyOrderBy(c => c.Name);
            return this;
        }
    }

    /// <summary>
    /// Soft delete workflow patterns.
    /// </summary>
    public class SoftDeletePatterns
    {
        /// <summary>
        /// Example 1: Soft delete workflow.
        /// </summary>
        public void Example_SoftDelete()
        {
            // Real usage:
            // 1. Fetch customer
            // var customer = await repository.GetByIdAsync(customerId);
            // 2. Mark as deleted
            // customer.Delete(currentUserId);
            // 3. Persist
            // await repository.UpdateAsync(customer);
            // After: IsDeleted = true, DeletedAt = DateTime.UtcNow, DeletedBy = currentUserId

            System.Console.WriteLine("Soft delete: Mark entity without removal");
        }

        /// <summary>
        /// Example 2: Restore soft-deleted entity.
        /// </summary>
        public void Example_Restore()
        {
            // Real usage:
            // 1. Query including soft-deleted
            // var spec = new CustomerSpecification().WithDeleted();
            // var deletedCustomer = await repository.FindAsync(spec);
            // 2. Restore
            // deletedCustomer.Restore();
            // 3. Persist
            // await repository.UpdateAsync(deletedCustomer);
            // After: IsDeleted = false, DeletedAt = null, DeletedBy = null

            System.Console.WriteLine("Restore: Unmark deleted entity");
        }

        /// <summary>
        /// Example 3: Automatic soft-delete filtering.
        /// Repositories automatically exclude soft-deleted by default.
        /// </summary>
        public void Example_AutomaticFiltering()
        {
            // Real usage:
            // var customers = await repository.GetAllAsync();
            // SQL: SELECT * FROM Customer WHERE IsDeleted = false (implicit)

            System.Console.WriteLine("Automatic: Repositories exclude soft-deleted");
        }

        /// <summary>
        /// Example 4: Query soft-deleted entities for audit/recovery.
        /// </summary>
        public void Example_QueryDeleted()
        {
            // Real usage:
            // var spec = new CustomerSpecification().WithDeleted().OrderByName();
            // var deletedCustomers = await repository.FindAllAsync(spec);

            System.Console.WriteLine("Query: Find soft-deleted for audit");
        }

        /// <summary>
        /// Example 5: Deleted within grace period (30 days).
        /// </summary>
        public void Example_RecoveryWindow()
        {
            // Real usage:
            // var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            // var spec = new CustomerSpecification()
            //     .WithDeleted()
            //     .DeletedSince(thirtyDaysAgo);
            // var recoverable = await repository.FindAllAsync(spec);

            System.Console.WriteLine("Recovery: Find entities within grace period");
        }

        /// <summary>
        /// Example 6: Hard delete after grace period.
        /// </summary>
        public void Example_HardDelete()
        {
            // Real usage:
            // var gracePeriodDays = 30;
            // var deletedDate = customer.DeletedAt!.Value;
            // var elapsedDays = (DateTime.UtcNow - deletedDate).TotalDays;
            // if (elapsedDays >= gracePeriodDays)
            // {
            //     await repository.DeleteAsync(customer);  // Permanent removal
            // }

            System.Console.WriteLine("Hard delete: Permanent removal after grace period");
        }
    }

    /// <summary>
    /// Audit trail for soft deletes.
    /// </summary>
    public class SoftDeleteAudit
    {
        /// <summary>
        /// Audit information captured on soft delete.
        /// </summary>
        public void Example_AuditTrail()
        {
            // Properties recorded:
            // DeletedAt: DateTime - When deleted
            // DeletedBy: string - Who deleted (user ID)
            // IsDeleted: bool - Flag
            //
            // Enables:
            // - Find who deleted
            // - Find when deleted
            // - Find by deletion date
            // - Compliance reporting

            System.Console.WriteLine("Audit: DeletedAt, DeletedBy tracked");
        }

        /// <summary>
        /// Compliance: Prove data is trackable.
        /// </summary>
        public void Example_Compliance()
        {
            // Real usage: Query deleted entities with metadata
            // var spec = new CustomerSpecification()
            //     .WithDeleted()
            //     .DeletedSince(auditStartDate);
            // var deletedCustomers = await repository.FindAllAsync(spec);
            // foreach (var c in deletedCustomers)
            // {
            //     logger.LogInformation("Customer deleted: {Id} by {User} at {Time}",
            //         c.Id, c.DeletedBy, c.DeletedAt);
            // }

            System.Console.WriteLine("Compliance: Audit trail for deleted entities");
        }
    }

    /// <summary>
    /// Service with soft delete support.
    /// </summary>
    public class SoftDeleteService
    {
        /// <summary>
        /// Service method: Delete customer (soft delete).
        /// </summary>
        public Result<bool> DeleteCustomer(int customerId, string currentUserId)
        {
            // Real usage:
            // var customer = await repository.GetByIdAsync(customerId);
            // if (customer == null) return Result.Fail(...);
            // customer.Delete(currentUserId);
            // await repository.UpdateAsync(customer);
            // await unitOfWork.SaveAsync();

            return Result.Ok(true);
        }

        /// <summary>
        /// Service method: Recover deleted customer.
        /// </summary>
        public Result<bool> RecoverCustomer(int customerId)
        {
            // Real usage:
            // var spec = new CustomerSpecification().WithDeleted();
            // var customer = await repository.FindAsync(spec);
            // if (customer == null) return Result.Fail(...);
            // customer.Restore();
            // await repository.UpdateAsync(customer);
            // await unitOfWork.SaveAsync();

            return Result.Ok(true);
        }

        /// <summary>
        /// Service method: List active customers (soft-deleted excluded).
        /// </summary>
        public Result<IReadOnlyCollection<Customer>> GetActiveCustomers()
        {
            // Real usage:
            // var spec = new CustomerSpecification().WithActive();
            // var customers = await repository.FindAllAsync(spec);
            // Repository automatically applies: WHERE IsDeleted = false

            return Result.Ok<IReadOnlyCollection<Customer>>(new List<Customer>());
        }
    }
}
