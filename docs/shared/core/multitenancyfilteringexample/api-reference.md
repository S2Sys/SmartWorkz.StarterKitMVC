# MultiTenancyFilteringExample API Reference

## Classes & Interfaces

### ITenantContextProvider

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.ITenantContextProvider`
- **Summary:** Tenant context provider (resolves current tenant from request).

### Customer

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.Customer`
- **Summary:** Tenant-scoped entity (inherits TenantId from AuditableEntity).

### MultiTenantUsage

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.MultiTenantUsage`
- **Summary:** Demonstrates multi-tenant repository usage.

#### Methods & Properties

- **Example_AutomaticTenantFiltering** - Repository automatically filters by current tenant.
            Example conceptual flow.
- **Example_QueryScoping** - All queries automatically scoped to current tenant.
- **Example_TenantSafety** - Cross-tenant access prevention.
            Even if caller passes different tenant ID, repository uses current.

### CustomerTenantSpecification

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.CustomerTenantSpecification`
- **Summary:** Customer specification for tenant-aware queries.

### TenantAwareService

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.TenantAwareService`
- **Summary:** Tenant-aware service using repositories.

#### Methods & Properties

- **GetActiveCustomers** - Service method automatically scoped to current tenant.
- **VerifyTenantOwnership** - Verify tenant ownership before accessing data.

### CustomerController

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.CustomerController`
- **Summary:** ASP.NET controller using tenant-aware services.

#### Methods & Properties

- **GetCustomers** - GET /api/customers - List customers for current tenant.

### ITenantContextProvider

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.ITenantContextProvider`
- **Summary:** Tenant context provider (resolves current tenant from request).

### Customer

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.Customer`
- **Summary:** Tenant-scoped entity (inherits TenantId from AuditableEntity).

### MultiTenantUsage

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.MultiTenantUsage`
- **Summary:** Demonstrates multi-tenant repository usage.

#### Methods & Properties

- **Example_AutomaticTenantFiltering** - Repository automatically filters by current tenant.
            Example conceptual flow.
- **Example_QueryScoping** - All queries automatically scoped to current tenant.
- **Example_TenantSafety** - Cross-tenant access prevention.
            Even if caller passes different tenant ID, repository uses current.

### CustomerTenantSpecification

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.CustomerTenantSpecification`
- **Summary:** Customer specification for tenant-aware queries.

### TenantAwareService

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.TenantAwareService`
- **Summary:** Tenant-aware service using repositories.

#### Methods & Properties

- **GetActiveCustomers** - Service method automatically scoped to current tenant.
- **VerifyTenantOwnership** - Verify tenant ownership before accessing data.

### CustomerController

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.CustomerController`
- **Summary:** ASP.NET controller using tenant-aware services.

#### Methods & Properties

- **GetCustomers** - GET /api/customers - List customers for current tenant.

### ITenantContextProvider

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.ITenantContextProvider`
- **Summary:** Tenant context provider (resolves current tenant from request).

### Customer

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.Customer`
- **Summary:** Tenant-scoped entity (inherits TenantId from AuditableEntity).

### MultiTenantUsage

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.MultiTenantUsage`
- **Summary:** Demonstrates multi-tenant repository usage.

#### Methods & Properties

- **Example_AutomaticTenantFiltering** - Repository automatically filters by current tenant.
            Example conceptual flow.
- **Example_QueryScoping** - All queries automatically scoped to current tenant.
- **Example_TenantSafety** - Cross-tenant access prevention.
            Even if caller passes different tenant ID, repository uses current.

### CustomerTenantSpecification

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.CustomerTenantSpecification`
- **Summary:** Customer specification for tenant-aware queries.

### TenantAwareService

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.TenantAwareService`
- **Summary:** Tenant-aware service using repositories.

#### Methods & Properties

- **GetActiveCustomers** - Service method automatically scoped to current tenant.
- **VerifyTenantOwnership** - Verify tenant ownership before accessing data.

### CustomerController

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.CustomerController`
- **Summary:** ASP.NET controller using tenant-aware services.

#### Methods & Properties

- **GetCustomers** - GET /api/customers - List customers for current tenant.

### ITenantContextProvider

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.ITenantContextProvider`
- **Summary:** Tenant context provider (resolves current tenant from request).

### Customer

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.Customer`
- **Summary:** Tenant-scoped entity (inherits TenantId from AuditableEntity).

### MultiTenantUsage

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.MultiTenantUsage`
- **Summary:** Demonstrates multi-tenant repository usage.

#### Methods & Properties

- **Example_AutomaticTenantFiltering** - Repository automatically filters by current tenant.
            Example conceptual flow.
- **Example_QueryScoping** - All queries automatically scoped to current tenant.
- **Example_TenantSafety** - Cross-tenant access prevention.
            Even if caller passes different tenant ID, repository uses current.

### CustomerTenantSpecification

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.CustomerTenantSpecification`
- **Summary:** Customer specification for tenant-aware queries.

### TenantAwareService

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.TenantAwareService`
- **Summary:** Tenant-aware service using repositories.

#### Methods & Properties

- **GetActiveCustomers** - Service method automatically scoped to current tenant.
- **VerifyTenantOwnership** - Verify tenant ownership before accessing data.

### CustomerController

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.CustomerController`
- **Summary:** ASP.NET controller using tenant-aware services.

#### Methods & Properties

- **GetCustomers** - GET /api/customers - List customers for current tenant.

### ITenantContextProvider

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.ITenantContextProvider`
- **Summary:** Tenant context provider (resolves current tenant from request).

### Customer

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.Customer`
- **Summary:** Tenant-scoped entity (inherits TenantId from AuditableEntity).

### MultiTenantUsage

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.MultiTenantUsage`
- **Summary:** Demonstrates multi-tenant repository usage.

#### Methods & Properties

- **Example_AutomaticTenantFiltering** - Repository automatically filters by current tenant.
            Example conceptual flow.
- **Example_QueryScoping** - All queries automatically scoped to current tenant.
- **Example_TenantSafety** - Cross-tenant access prevention.
            Even if caller passes different tenant ID, repository uses current.

### CustomerTenantSpecification

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.CustomerTenantSpecification`
- **Summary:** Customer specification for tenant-aware queries.

### TenantAwareService

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.TenantAwareService`
- **Summary:** Tenant-aware service using repositories.

#### Methods & Properties

- **GetActiveCustomers** - Service method automatically scoped to current tenant.
- **VerifyTenantOwnership** - Verify tenant ownership before accessing data.

### CustomerController

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample.CustomerController`
- **Summary:** ASP.NET controller using tenant-aware services.

#### Methods & Properties

- **GetCustomers** - GET /api/customers - List customers for current tenant.

