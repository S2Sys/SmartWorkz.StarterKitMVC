namespace SmartWorkz.Core.Examples;

using SmartWorkz.Shared;

/// <summary>
/// Demonstrates multi-tenancy with automatic tenant isolation.
/// Repositories automatically filter queries by TenantId to ensure
/// complete data separation between tenants.
/// </summary>
public class MultiTenancyFilteringExample
{
    /// <summary>
    /// Tenant context provider (resolves current tenant from request).
    /// </summary>
    public interface ITenantContextProvider
    {
        int GetCurrentTenantId();
    }

    /// <summary>
    /// Tenant-scoped entity (inherits TenantId from AuditableEntity).
    /// </summary>
    public class Customer : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // TenantId is inherited from AuditableEntity
    }

    public class Order : AuditableEntity
    {
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
        // TenantId is inherited from AuditableEntity
    }

    /// <summary>
    /// Demonstrates multi-tenant repository usage.
    /// </summary>
    public class MultiTenantUsage
    {
        /// <summary>
        /// Repository automatically filters by current tenant.
        /// Example conceptual flow.
        /// </summary>
        public void Example_AutomaticTenantFiltering()
        {
            // Real usage:
            // var tenantId = _tenantProvider.GetCurrentTenantId();
            // var customerRepository = new Repository<Customer>(dbContext, tenantId);
            // var customers = await customerRepository.GetAllAsync();
            // SQL: SELECT * FROM Customer WHERE TenantId = @tenantId

            System.Console.WriteLine("Repository automatically filters by tenant");
        }

        /// <summary>
        /// All queries automatically scoped to current tenant.
        /// </summary>
        public void Example_QueryScoping()
        {
            // Real usage patterns:
            // GetAllAsync() - WHERE TenantId = current
            // FindAsync(spec) - WHERE TenantId = current AND spec criteria
            // CountAsync(spec) - WHERE TenantId = current AND spec criteria
            // ExistsAsync(spec) - WHERE TenantId = current AND spec criteria

            System.Console.WriteLine("All queries automatically scoped to current tenant");
        }

        /// <summary>
        /// Cross-tenant access prevention.
        /// Even if caller passes different tenant ID, repository uses current.
        /// </summary>
        public void Example_TenantSafety()
        {
            // Real usage:
            // var currentTenantId = _tenantProvider.GetCurrentTenantId(); // 1
            // var customer = await repository.GetByIdAsync(customerId);  // customerId exists in tenant 2
            // Result: null (customer not found in current tenant 1)
            // SQL: SELECT * FROM Customer WHERE Id = @id AND TenantId = 1

            System.Console.WriteLine("Repository prevents accidental cross-tenant access");
        }
    }

    /// <summary>
    /// Customer specification for tenant-aware queries.
    /// </summary>
    public class CustomerTenantSpecification : Specification<Customer>
    {
        public CustomerTenantSpecification WithActive()
        {
            AddCriteria(c => !c.IsDeleted);
            return this;
        }

        public CustomerTenantSpecification WithName(string name)
        {
            AddCriteria(c => c.Name.Contains(name));
            return this;
        }

        public CustomerTenantSpecification OrderByName()
        {
            ApplyOrderBy(c => c.Name);
            return this;
        }
    }

    /// <summary>
    /// Tenant-aware service using repositories.
    /// </summary>
    public class TenantAwareService
    {
        private readonly ITenantContextProvider _tenantProvider;

        public TenantAwareService(ITenantContextProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        /// <summary>
        /// Service method automatically scoped to current tenant.
        /// </summary>
        public void GetActiveCustomers()
        {
            // Real usage:
            // var tenantId = _tenantProvider.GetCurrentTenantId();
            // var spec = new CustomerTenantSpecification().WithActive().OrderByName();
            // var customers = await repository.FindAllAsync(spec);
            // SQL: SELECT * FROM Customer WHERE TenantId = @tenantId AND IsDeleted = false

            System.Console.WriteLine("Service retrieves customers for current tenant only");
        }

        /// <summary>
        /// Verify tenant ownership before accessing data.
        /// </summary>
        public void VerifyTenantOwnership(int customerId)
        {
            // Real usage:
            // var tenantId = _tenantProvider.GetCurrentTenantId();
            // var customer = await repository.GetByIdAsync(customerId);
            // if (customer == null) return null; // Not found in current tenant
            // Safe to access customer.TenantId (guaranteed to be current tenant)

            System.Console.WriteLine("Verify customer belongs to current tenant");
        }
    }

    /// <summary>
    /// ASP.NET controller using tenant-aware services.
    /// </summary>
    public class CustomerController
    {
        private readonly TenantAwareService _service;

        public CustomerController(TenantAwareService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET /api/customers - List customers for current tenant.
        /// </summary>
        public void GetCustomers()
        {
            // Service handles tenant scoping automatically
            _service.GetActiveCustomers();
        }
    }
}
