# Examples API Reference

## Classes & Interfaces

### SpecificationChainingExample

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample`
- **Summary:** Demonstrates the Specification pattern for building composable, type-safe queries.
             Specifications encapsulate query logic (filtering, sorting, eager loading) into
             reusable, testable components without coupling to specific repositories.

#### Methods & Properties

- **ProductSpecification.WithCategory** - Adds filter: WHERE CategoryId = categoryId
- **ProductSpecification.WithPriceRange** - Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
- **ProductSpecification.WithAvailableOnly** - Adds filter: WHERE IsAvailable = true
- **ProductSpecification.WithInStock** - Adds filter: WHERE StockQuantity > minStock
- **ProductSpecification.OrderByPrice** - Adds ordering: ORDER BY Price ASC
- **ProductSpecification.OrderByPriceDesc** - Adds ordering: ORDER BY Price DESC
- **ProductSpecification.OrderByName** - Adds ordering: ORDER BY Name ASC
- **ProductSpecification.WithPaging** - Applies pagination: OFFSET skip LIMIT take
- **SpecificationUsagePatterns.Example_SimpleFilter** - Example 1: Simple specification - find available products in category.
- **SpecificationUsagePatterns.Example_ComplexFilter** - Example 2: Complex specification - affordable, available products with pagination.
- **SpecificationUsagePatterns.Example_PremiumProducts** - Example 3: Premium products - expensive, in-stock items.
- **SpecificationUsagePatterns.Example_SpecificationReuse** - Example 4: Specification reuse across multiple queries.
            Single specification definition, many uses.
- **SpecificationCombination.Example_And** - Example: Combine multiple specifications with AND (all conditions must match).
- **SpecificationCombination.Example_Or** - Example: Combine specifications with OR (at least one condition must match).
- **SpecificationCombination.Example_Not** - Example: Negate specification with NOT (inverse matching).

### GuardClauseValidationExample

- **Namespace:** `SmartWorkz.Core.Examples.GuardClauseValidationExample`
- **Summary:** Demonstrates Guard utility class for precondition validation.
            Guards enforce invariants at domain boundaries (constructors, service methods).

#### Methods & Properties

- **Order.#ctor** - Constructor with comprehensive guard clauses.

### MultiTenancyFilteringExample

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample`
- **Summary:** Demonstrates multi-tenancy with automatic tenant isolation.
            Repositories automatically filter queries by TenantId to ensure
            complete data separation between tenants.

#### Methods & Properties

- **MultiTenantUsage.Example_AutomaticTenantFiltering** - Repository automatically filters by current tenant.
            Example conceptual flow.
- **MultiTenantUsage.Example_QueryScoping** - All queries automatically scoped to current tenant.
- **MultiTenantUsage.Example_TenantSafety** - Cross-tenant access prevention.
            Even if caller passes different tenant ID, repository uses current.
- **TenantAwareService.GetActiveCustomers** - Service method automatically scoped to current tenant.
- **TenantAwareService.VerifyTenantOwnership** - Verify tenant ownership before accessing data.
- **CustomerController.GetCustomers** - GET /api/customers - List customers for current tenant.

### SoftDeleteBehaviorExample

- **Namespace:** `SmartWorkz.Core.Examples.SoftDeleteBehaviorExample`
- **Summary:** Demonstrates soft delete pattern for non-destructive data removal.
            Soft delete marks entities as deleted without removing them from the database,
            enabling recovery, audit trails, and data retention compliance.

#### Methods & Properties

- **CustomerSpecification.WithActive** - Include only active (non-deleted) customers.
- **CustomerSpecification.WithDeleted** - Include only soft-deleted customers.
- **CustomerSpecification.DeletedSince** - Filter deleted customers within date range.
- **SoftDeletePatterns.Example_SoftDelete** - Example 1: Soft delete workflow.
- **SoftDeletePatterns.Example_Restore** - Example 2: Restore soft-deleted entity.
- **SoftDeletePatterns.Example_AutomaticFiltering** - Example 3: Automatic soft-delete filtering.
            Repositories automatically exclude soft-deleted by default.
- **SoftDeletePatterns.Example_QueryDeleted** - Example 4: Query soft-deleted entities for audit/recovery.
- **SoftDeletePatterns.Example_RecoveryWindow** - Example 5: Deleted within grace period (30 days).
- **SoftDeletePatterns.Example_HardDelete** - Example 6: Hard delete after grace period.
- **SoftDeleteAudit.Example_AuditTrail** - Audit information captured on soft delete.
- **SoftDeleteAudit.Example_Compliance** - Compliance: Prove data is trackable.
- **SoftDeleteService.DeleteCustomer** - Service method: Delete customer (soft delete).
- **SoftDeleteService.RecoverCustomer** - Service method: Recover deleted customer.
- **SoftDeleteService.GetActiveCustomers** - Service method: List active customers (soft-deleted excluded).

### DTOMappingExample

- **Namespace:** `SmartWorkz.Core.Examples.DTOMappingExample`
- **Summary:** Demonstrates DTO mapping patterns for API contracts and data transformation.
            DTOs decouple internal domain models from external API representations,
            enabling flexible data transformation without exposing internal structures.

#### Methods & Properties

- **DTOMappingPatterns.MapToDto** - Example 1: Entity to DTO mapping (for Read operations).
- **DTOMappingPatterns.MapToEntity** - Example 2: DTO to Entity mapping (for Create operations).
- **DTOMappingPatterns.MapToDto** - Example 3: Batch mapping (collection).
- **DTOMappingPatterns.CustomerPublicDto.#ctor** - Example 4: Selective field exposure (security).
- **DTOMappingPatterns.Example_FlatteningValueObjects** - Example 5: Flattening nested value objects.
- **CustomerService.CreateCustomer** - CREATE: DTO -> Entity -> Persist -> DTO
- **CustomerService.GetCustomer** - READ: Entity -> DTO
- **CustomerService.UpdateCustomer** - UPDATE: DTO -> Update Entity -> Persist -> DTO
- **CustomerService.GetAllCustomers** - LIST: Entities -> DTOs
- **CustomerController.CreateCustomer** - POST /customers - Create from DTO
- **CustomerController.GetCustomer** - GET /customers/{id} - Return as DTO
- **CustomerController.UpdateCustomer** - PUT /customers/{id} - Update from DTO
- **CustomerController.GetAllCustomers** - GET /customers - Return as DTOs

### AuditTrailQueriesExample

- **Namespace:** `SmartWorkz.Core.Examples.AuditTrailQueriesExample`
- **Summary:** Demonstrates querying audit trails for compliance, accountability, and forensics.
            Audit trails track entity lifecycle (creation, modification, deletion) with
            user information and timestamps, enabling compliance audits and data recovery.

#### Methods & Properties

- **AuditTrailPatterns.Example_RecentlyCreated** - Example 1: Find recently created entities.
- **AuditTrailPatterns.Example_RecentlyModified** - Example 2: Find recently modified entities.
- **AuditTrailPatterns.Example_CreatedByUser** - Example 3: Find entities created by specific user.
- **AuditTrailPatterns.Example_Deleted** - Example 4: Find soft-deleted entities (recovery).
- **AuditTrailPatterns.Example_RecentlyDeleted** - Example 5: Find recently deleted (recovery window).
- **AuditTrailPatterns.Example_CountModified** - Example 6: Count modified in date range.
- **AuditReporting.GenerateUserActivityReport** - Generate report: Changes by user (who changed what).
- **AuditReporting.ComplianceReport.#ctor** - Generate compliance report: All changes in period.
- **AuditReporting.RecoveryCandidate.#ctor** - Generate data recovery report: Recently deleted available for restore.
- **AuditTrailService.GetActivityLog** - Get activity log for audit purposes.
- **AuditTrailService.GetComplianceReport** - Get compliance report for period.
- **AuditTrailService.GetRecoveryCandidates** - Get recovery candidates for restore.
- **AuditController.GetActivityLog** - GET /audit/activity - User activity log
- **AuditController.GetComplianceReport** - GET /audit/compliance - Compliance report
- **AuditController.GetRecoveryCandidates** - GET /audit/recovery - Recovery candidates

### SpecificationChainingExample

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample`
- **Summary:** Demonstrates the Specification pattern for building composable, type-safe queries.
             Specifications encapsulate query logic (filtering, sorting, eager loading) into
             reusable, testable components without coupling to specific repositories.

#### Methods & Properties

- **ProductSpecification.WithCategory** - Adds filter: WHERE CategoryId = categoryId
- **ProductSpecification.WithPriceRange** - Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
- **ProductSpecification.WithAvailableOnly** - Adds filter: WHERE IsAvailable = true
- **ProductSpecification.WithInStock** - Adds filter: WHERE StockQuantity > minStock
- **ProductSpecification.OrderByPrice** - Adds ordering: ORDER BY Price ASC
- **ProductSpecification.OrderByPriceDesc** - Adds ordering: ORDER BY Price DESC
- **ProductSpecification.OrderByName** - Adds ordering: ORDER BY Name ASC
- **ProductSpecification.WithPaging** - Applies pagination: OFFSET skip LIMIT take
- **SpecificationUsagePatterns.Example_SimpleFilter** - Example 1: Simple specification - find available products in category.
- **SpecificationUsagePatterns.Example_ComplexFilter** - Example 2: Complex specification - affordable, available products with pagination.
- **SpecificationUsagePatterns.Example_PremiumProducts** - Example 3: Premium products - expensive, in-stock items.
- **SpecificationUsagePatterns.Example_SpecificationReuse** - Example 4: Specification reuse across multiple queries.
            Single specification definition, many uses.
- **SpecificationCombination.Example_And** - Example: Combine multiple specifications with AND (all conditions must match).
- **SpecificationCombination.Example_Or** - Example: Combine specifications with OR (at least one condition must match).
- **SpecificationCombination.Example_Not** - Example: Negate specification with NOT (inverse matching).

### GuardClauseValidationExample

- **Namespace:** `SmartWorkz.Core.Examples.GuardClauseValidationExample`
- **Summary:** Demonstrates Guard utility class for precondition validation.
            Guards enforce invariants at domain boundaries (constructors, service methods).

#### Methods & Properties

- **Order.#ctor** - Constructor with comprehensive guard clauses.

### MultiTenancyFilteringExample

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample`
- **Summary:** Demonstrates multi-tenancy with automatic tenant isolation.
            Repositories automatically filter queries by TenantId to ensure
            complete data separation between tenants.

#### Methods & Properties

- **MultiTenantUsage.Example_AutomaticTenantFiltering** - Repository automatically filters by current tenant.
            Example conceptual flow.
- **MultiTenantUsage.Example_QueryScoping** - All queries automatically scoped to current tenant.
- **MultiTenantUsage.Example_TenantSafety** - Cross-tenant access prevention.
            Even if caller passes different tenant ID, repository uses current.
- **TenantAwareService.GetActiveCustomers** - Service method automatically scoped to current tenant.
- **TenantAwareService.VerifyTenantOwnership** - Verify tenant ownership before accessing data.
- **CustomerController.GetCustomers** - GET /api/customers - List customers for current tenant.

### SoftDeleteBehaviorExample

- **Namespace:** `SmartWorkz.Core.Examples.SoftDeleteBehaviorExample`
- **Summary:** Demonstrates soft delete pattern for non-destructive data removal.
            Soft delete marks entities as deleted without removing them from the database,
            enabling recovery, audit trails, and data retention compliance.

#### Methods & Properties

- **CustomerSpecification.WithActive** - Include only active (non-deleted) customers.
- **CustomerSpecification.WithDeleted** - Include only soft-deleted customers.
- **CustomerSpecification.DeletedSince** - Filter deleted customers within date range.
- **SoftDeletePatterns.Example_SoftDelete** - Example 1: Soft delete workflow.
- **SoftDeletePatterns.Example_Restore** - Example 2: Restore soft-deleted entity.
- **SoftDeletePatterns.Example_AutomaticFiltering** - Example 3: Automatic soft-delete filtering.
            Repositories automatically exclude soft-deleted by default.
- **SoftDeletePatterns.Example_QueryDeleted** - Example 4: Query soft-deleted entities for audit/recovery.
- **SoftDeletePatterns.Example_RecoveryWindow** - Example 5: Deleted within grace period (30 days).
- **SoftDeletePatterns.Example_HardDelete** - Example 6: Hard delete after grace period.
- **SoftDeleteAudit.Example_AuditTrail** - Audit information captured on soft delete.
- **SoftDeleteAudit.Example_Compliance** - Compliance: Prove data is trackable.
- **SoftDeleteService.DeleteCustomer** - Service method: Delete customer (soft delete).
- **SoftDeleteService.RecoverCustomer** - Service method: Recover deleted customer.
- **SoftDeleteService.GetActiveCustomers** - Service method: List active customers (soft-deleted excluded).

### DTOMappingExample

- **Namespace:** `SmartWorkz.Core.Examples.DTOMappingExample`
- **Summary:** Demonstrates DTO mapping patterns for API contracts and data transformation.
            DTOs decouple internal domain models from external API representations,
            enabling flexible data transformation without exposing internal structures.

#### Methods & Properties

- **DTOMappingPatterns.MapToDto** - Example 1: Entity to DTO mapping (for Read operations).
- **DTOMappingPatterns.MapToEntity** - Example 2: DTO to Entity mapping (for Create operations).
- **DTOMappingPatterns.MapToDto** - Example 3: Batch mapping (collection).
- **DTOMappingPatterns.CustomerPublicDto.#ctor** - Example 4: Selective field exposure (security).
- **DTOMappingPatterns.Example_FlatteningValueObjects** - Example 5: Flattening nested value objects.
- **CustomerService.CreateCustomer** - CREATE: DTO -> Entity -> Persist -> DTO
- **CustomerService.GetCustomer** - READ: Entity -> DTO
- **CustomerService.UpdateCustomer** - UPDATE: DTO -> Update Entity -> Persist -> DTO
- **CustomerService.GetAllCustomers** - LIST: Entities -> DTOs
- **CustomerController.CreateCustomer** - POST /customers - Create from DTO
- **CustomerController.GetCustomer** - GET /customers/{id} - Return as DTO
- **CustomerController.UpdateCustomer** - PUT /customers/{id} - Update from DTO
- **CustomerController.GetAllCustomers** - GET /customers - Return as DTOs

### AuditTrailQueriesExample

- **Namespace:** `SmartWorkz.Core.Examples.AuditTrailQueriesExample`
- **Summary:** Demonstrates querying audit trails for compliance, accountability, and forensics.
            Audit trails track entity lifecycle (creation, modification, deletion) with
            user information and timestamps, enabling compliance audits and data recovery.

#### Methods & Properties

- **AuditTrailPatterns.Example_RecentlyCreated** - Example 1: Find recently created entities.
- **AuditTrailPatterns.Example_RecentlyModified** - Example 2: Find recently modified entities.
- **AuditTrailPatterns.Example_CreatedByUser** - Example 3: Find entities created by specific user.
- **AuditTrailPatterns.Example_Deleted** - Example 4: Find soft-deleted entities (recovery).
- **AuditTrailPatterns.Example_RecentlyDeleted** - Example 5: Find recently deleted (recovery window).
- **AuditTrailPatterns.Example_CountModified** - Example 6: Count modified in date range.
- **AuditReporting.GenerateUserActivityReport** - Generate report: Changes by user (who changed what).
- **AuditReporting.ComplianceReport.#ctor** - Generate compliance report: All changes in period.
- **AuditReporting.RecoveryCandidate.#ctor** - Generate data recovery report: Recently deleted available for restore.
- **AuditTrailService.GetActivityLog** - Get activity log for audit purposes.
- **AuditTrailService.GetComplianceReport** - Get compliance report for period.
- **AuditTrailService.GetRecoveryCandidates** - Get recovery candidates for restore.
- **AuditController.GetActivityLog** - GET /audit/activity - User activity log
- **AuditController.GetComplianceReport** - GET /audit/compliance - Compliance report
- **AuditController.GetRecoveryCandidates** - GET /audit/recovery - Recovery candidates

### SpecificationChainingExample

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample`
- **Summary:** Demonstrates the Specification pattern for building composable, type-safe queries.
             Specifications encapsulate query logic (filtering, sorting, eager loading) into
             reusable, testable components without coupling to specific repositories.

#### Methods & Properties

- **ProductSpecification.WithCategory** - Adds filter: WHERE CategoryId = categoryId
- **ProductSpecification.WithPriceRange** - Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
- **ProductSpecification.WithAvailableOnly** - Adds filter: WHERE IsAvailable = true
- **ProductSpecification.WithInStock** - Adds filter: WHERE StockQuantity > minStock
- **ProductSpecification.OrderByPrice** - Adds ordering: ORDER BY Price ASC
- **ProductSpecification.OrderByPriceDesc** - Adds ordering: ORDER BY Price DESC
- **ProductSpecification.OrderByName** - Adds ordering: ORDER BY Name ASC
- **ProductSpecification.WithPaging** - Applies pagination: OFFSET skip LIMIT take
- **SpecificationUsagePatterns.Example_SimpleFilter** - Example 1: Simple specification - find available products in category.
- **SpecificationUsagePatterns.Example_ComplexFilter** - Example 2: Complex specification - affordable, available products with pagination.
- **SpecificationUsagePatterns.Example_PremiumProducts** - Example 3: Premium products - expensive, in-stock items.
- **SpecificationUsagePatterns.Example_SpecificationReuse** - Example 4: Specification reuse across multiple queries.
            Single specification definition, many uses.
- **SpecificationCombination.Example_And** - Example: Combine multiple specifications with AND (all conditions must match).
- **SpecificationCombination.Example_Or** - Example: Combine specifications with OR (at least one condition must match).
- **SpecificationCombination.Example_Not** - Example: Negate specification with NOT (inverse matching).

### GuardClauseValidationExample

- **Namespace:** `SmartWorkz.Core.Examples.GuardClauseValidationExample`
- **Summary:** Demonstrates Guard utility class for precondition validation.
            Guards enforce invariants at domain boundaries (constructors, service methods).

#### Methods & Properties

- **Order.#ctor** - Constructor with comprehensive guard clauses.

### MultiTenancyFilteringExample

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample`
- **Summary:** Demonstrates multi-tenancy with automatic tenant isolation.
            Repositories automatically filter queries by TenantId to ensure
            complete data separation between tenants.

#### Methods & Properties

- **MultiTenantUsage.Example_AutomaticTenantFiltering** - Repository automatically filters by current tenant.
            Example conceptual flow.
- **MultiTenantUsage.Example_QueryScoping** - All queries automatically scoped to current tenant.
- **MultiTenantUsage.Example_TenantSafety** - Cross-tenant access prevention.
            Even if caller passes different tenant ID, repository uses current.
- **TenantAwareService.GetActiveCustomers** - Service method automatically scoped to current tenant.
- **TenantAwareService.VerifyTenantOwnership** - Verify tenant ownership before accessing data.
- **CustomerController.GetCustomers** - GET /api/customers - List customers for current tenant.

### SoftDeleteBehaviorExample

- **Namespace:** `SmartWorkz.Core.Examples.SoftDeleteBehaviorExample`
- **Summary:** Demonstrates soft delete pattern for non-destructive data removal.
            Soft delete marks entities as deleted without removing them from the database,
            enabling recovery, audit trails, and data retention compliance.

#### Methods & Properties

- **CustomerSpecification.WithActive** - Include only active (non-deleted) customers.
- **CustomerSpecification.WithDeleted** - Include only soft-deleted customers.
- **CustomerSpecification.DeletedSince** - Filter deleted customers within date range.
- **SoftDeletePatterns.Example_SoftDelete** - Example 1: Soft delete workflow.
- **SoftDeletePatterns.Example_Restore** - Example 2: Restore soft-deleted entity.
- **SoftDeletePatterns.Example_AutomaticFiltering** - Example 3: Automatic soft-delete filtering.
            Repositories automatically exclude soft-deleted by default.
- **SoftDeletePatterns.Example_QueryDeleted** - Example 4: Query soft-deleted entities for audit/recovery.
- **SoftDeletePatterns.Example_RecoveryWindow** - Example 5: Deleted within grace period (30 days).
- **SoftDeletePatterns.Example_HardDelete** - Example 6: Hard delete after grace period.
- **SoftDeleteAudit.Example_AuditTrail** - Audit information captured on soft delete.
- **SoftDeleteAudit.Example_Compliance** - Compliance: Prove data is trackable.
- **SoftDeleteService.DeleteCustomer** - Service method: Delete customer (soft delete).
- **SoftDeleteService.RecoverCustomer** - Service method: Recover deleted customer.
- **SoftDeleteService.GetActiveCustomers** - Service method: List active customers (soft-deleted excluded).

### DTOMappingExample

- **Namespace:** `SmartWorkz.Core.Examples.DTOMappingExample`
- **Summary:** Demonstrates DTO mapping patterns for API contracts and data transformation.
            DTOs decouple internal domain models from external API representations,
            enabling flexible data transformation without exposing internal structures.

#### Methods & Properties

- **DTOMappingPatterns.MapToDto** - Example 1: Entity to DTO mapping (for Read operations).
- **DTOMappingPatterns.MapToEntity** - Example 2: DTO to Entity mapping (for Create operations).
- **DTOMappingPatterns.MapToDto** - Example 3: Batch mapping (collection).
- **DTOMappingPatterns.CustomerPublicDto.#ctor** - Example 4: Selective field exposure (security).
- **DTOMappingPatterns.Example_FlatteningValueObjects** - Example 5: Flattening nested value objects.
- **CustomerService.CreateCustomer** - CREATE: DTO -> Entity -> Persist -> DTO
- **CustomerService.GetCustomer** - READ: Entity -> DTO
- **CustomerService.UpdateCustomer** - UPDATE: DTO -> Update Entity -> Persist -> DTO
- **CustomerService.GetAllCustomers** - LIST: Entities -> DTOs
- **CustomerController.CreateCustomer** - POST /customers - Create from DTO
- **CustomerController.GetCustomer** - GET /customers/{id} - Return as DTO
- **CustomerController.UpdateCustomer** - PUT /customers/{id} - Update from DTO
- **CustomerController.GetAllCustomers** - GET /customers - Return as DTOs

### AuditTrailQueriesExample

- **Namespace:** `SmartWorkz.Core.Examples.AuditTrailQueriesExample`
- **Summary:** Demonstrates querying audit trails for compliance, accountability, and forensics.
            Audit trails track entity lifecycle (creation, modification, deletion) with
            user information and timestamps, enabling compliance audits and data recovery.

#### Methods & Properties

- **AuditTrailPatterns.Example_RecentlyCreated** - Example 1: Find recently created entities.
- **AuditTrailPatterns.Example_RecentlyModified** - Example 2: Find recently modified entities.
- **AuditTrailPatterns.Example_CreatedByUser** - Example 3: Find entities created by specific user.
- **AuditTrailPatterns.Example_Deleted** - Example 4: Find soft-deleted entities (recovery).
- **AuditTrailPatterns.Example_RecentlyDeleted** - Example 5: Find recently deleted (recovery window).
- **AuditTrailPatterns.Example_CountModified** - Example 6: Count modified in date range.
- **AuditReporting.GenerateUserActivityReport** - Generate report: Changes by user (who changed what).
- **AuditReporting.ComplianceReport.#ctor** - Generate compliance report: All changes in period.
- **AuditReporting.RecoveryCandidate.#ctor** - Generate data recovery report: Recently deleted available for restore.
- **AuditTrailService.GetActivityLog** - Get activity log for audit purposes.
- **AuditTrailService.GetComplianceReport** - Get compliance report for period.
- **AuditTrailService.GetRecoveryCandidates** - Get recovery candidates for restore.
- **AuditController.GetActivityLog** - GET /audit/activity - User activity log
- **AuditController.GetComplianceReport** - GET /audit/compliance - Compliance report
- **AuditController.GetRecoveryCandidates** - GET /audit/recovery - Recovery candidates

### SpecificationChainingExample

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample`
- **Summary:** Demonstrates the Specification pattern for building composable, type-safe queries.
             Specifications encapsulate query logic (filtering, sorting, eager loading) into
             reusable, testable components without coupling to specific repositories.

#### Methods & Properties

- **ProductSpecification.WithCategory** - Adds filter: WHERE CategoryId = categoryId
- **ProductSpecification.WithPriceRange** - Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
- **ProductSpecification.WithAvailableOnly** - Adds filter: WHERE IsAvailable = true
- **ProductSpecification.WithInStock** - Adds filter: WHERE StockQuantity > minStock
- **ProductSpecification.OrderByPrice** - Adds ordering: ORDER BY Price ASC
- **ProductSpecification.OrderByPriceDesc** - Adds ordering: ORDER BY Price DESC
- **ProductSpecification.OrderByName** - Adds ordering: ORDER BY Name ASC
- **ProductSpecification.WithPaging** - Applies pagination: OFFSET skip LIMIT take
- **SpecificationUsagePatterns.Example_SimpleFilter** - Example 1: Simple specification - find available products in category.
- **SpecificationUsagePatterns.Example_ComplexFilter** - Example 2: Complex specification - affordable, available products with pagination.
- **SpecificationUsagePatterns.Example_PremiumProducts** - Example 3: Premium products - expensive, in-stock items.
- **SpecificationUsagePatterns.Example_SpecificationReuse** - Example 4: Specification reuse across multiple queries.
            Single specification definition, many uses.
- **SpecificationCombination.Example_And** - Example: Combine multiple specifications with AND (all conditions must match).
- **SpecificationCombination.Example_Or** - Example: Combine specifications with OR (at least one condition must match).
- **SpecificationCombination.Example_Not** - Example: Negate specification with NOT (inverse matching).

### GuardClauseValidationExample

- **Namespace:** `SmartWorkz.Core.Examples.GuardClauseValidationExample`
- **Summary:** Demonstrates Guard utility class for precondition validation.
            Guards enforce invariants at domain boundaries (constructors, service methods).

#### Methods & Properties

- **Order.#ctor** - Constructor with comprehensive guard clauses.

### MultiTenancyFilteringExample

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample`
- **Summary:** Demonstrates multi-tenancy with automatic tenant isolation.
            Repositories automatically filter queries by TenantId to ensure
            complete data separation between tenants.

#### Methods & Properties

- **MultiTenantUsage.Example_AutomaticTenantFiltering** - Repository automatically filters by current tenant.
            Example conceptual flow.
- **MultiTenantUsage.Example_QueryScoping** - All queries automatically scoped to current tenant.
- **MultiTenantUsage.Example_TenantSafety** - Cross-tenant access prevention.
            Even if caller passes different tenant ID, repository uses current.
- **TenantAwareService.GetActiveCustomers** - Service method automatically scoped to current tenant.
- **TenantAwareService.VerifyTenantOwnership** - Verify tenant ownership before accessing data.
- **CustomerController.GetCustomers** - GET /api/customers - List customers for current tenant.

### SoftDeleteBehaviorExample

- **Namespace:** `SmartWorkz.Core.Examples.SoftDeleteBehaviorExample`
- **Summary:** Demonstrates soft delete pattern for non-destructive data removal.
            Soft delete marks entities as deleted without removing them from the database,
            enabling recovery, audit trails, and data retention compliance.

#### Methods & Properties

- **CustomerSpecification.WithActive** - Include only active (non-deleted) customers.
- **CustomerSpecification.WithDeleted** - Include only soft-deleted customers.
- **CustomerSpecification.DeletedSince** - Filter deleted customers within date range.
- **SoftDeletePatterns.Example_SoftDelete** - Example 1: Soft delete workflow.
- **SoftDeletePatterns.Example_Restore** - Example 2: Restore soft-deleted entity.
- **SoftDeletePatterns.Example_AutomaticFiltering** - Example 3: Automatic soft-delete filtering.
            Repositories automatically exclude soft-deleted by default.
- **SoftDeletePatterns.Example_QueryDeleted** - Example 4: Query soft-deleted entities for audit/recovery.
- **SoftDeletePatterns.Example_RecoveryWindow** - Example 5: Deleted within grace period (30 days).
- **SoftDeletePatterns.Example_HardDelete** - Example 6: Hard delete after grace period.
- **SoftDeleteAudit.Example_AuditTrail** - Audit information captured on soft delete.
- **SoftDeleteAudit.Example_Compliance** - Compliance: Prove data is trackable.
- **SoftDeleteService.DeleteCustomer** - Service method: Delete customer (soft delete).
- **SoftDeleteService.RecoverCustomer** - Service method: Recover deleted customer.
- **SoftDeleteService.GetActiveCustomers** - Service method: List active customers (soft-deleted excluded).

### DTOMappingExample

- **Namespace:** `SmartWorkz.Core.Examples.DTOMappingExample`
- **Summary:** Demonstrates DTO mapping patterns for API contracts and data transformation.
            DTOs decouple internal domain models from external API representations,
            enabling flexible data transformation without exposing internal structures.

#### Methods & Properties

- **DTOMappingPatterns.MapToDto** - Example 1: Entity to DTO mapping (for Read operations).
- **DTOMappingPatterns.MapToEntity** - Example 2: DTO to Entity mapping (for Create operations).
- **DTOMappingPatterns.MapToDto** - Example 3: Batch mapping (collection).
- **DTOMappingPatterns.CustomerPublicDto.#ctor** - Example 4: Selective field exposure (security).
- **DTOMappingPatterns.Example_FlatteningValueObjects** - Example 5: Flattening nested value objects.
- **CustomerService.CreateCustomer** - CREATE: DTO -> Entity -> Persist -> DTO
- **CustomerService.GetCustomer** - READ: Entity -> DTO
- **CustomerService.UpdateCustomer** - UPDATE: DTO -> Update Entity -> Persist -> DTO
- **CustomerService.GetAllCustomers** - LIST: Entities -> DTOs
- **CustomerController.CreateCustomer** - POST /customers - Create from DTO
- **CustomerController.GetCustomer** - GET /customers/{id} - Return as DTO
- **CustomerController.UpdateCustomer** - PUT /customers/{id} - Update from DTO
- **CustomerController.GetAllCustomers** - GET /customers - Return as DTOs

### AuditTrailQueriesExample

- **Namespace:** `SmartWorkz.Core.Examples.AuditTrailQueriesExample`
- **Summary:** Demonstrates querying audit trails for compliance, accountability, and forensics.
            Audit trails track entity lifecycle (creation, modification, deletion) with
            user information and timestamps, enabling compliance audits and data recovery.

#### Methods & Properties

- **AuditTrailPatterns.Example_RecentlyCreated** - Example 1: Find recently created entities.
- **AuditTrailPatterns.Example_RecentlyModified** - Example 2: Find recently modified entities.
- **AuditTrailPatterns.Example_CreatedByUser** - Example 3: Find entities created by specific user.
- **AuditTrailPatterns.Example_Deleted** - Example 4: Find soft-deleted entities (recovery).
- **AuditTrailPatterns.Example_RecentlyDeleted** - Example 5: Find recently deleted (recovery window).
- **AuditTrailPatterns.Example_CountModified** - Example 6: Count modified in date range.
- **AuditReporting.GenerateUserActivityReport** - Generate report: Changes by user (who changed what).
- **AuditReporting.ComplianceReport.#ctor** - Generate compliance report: All changes in period.
- **AuditReporting.RecoveryCandidate.#ctor** - Generate data recovery report: Recently deleted available for restore.
- **AuditTrailService.GetActivityLog** - Get activity log for audit purposes.
- **AuditTrailService.GetComplianceReport** - Get compliance report for period.
- **AuditTrailService.GetRecoveryCandidates** - Get recovery candidates for restore.
- **AuditController.GetActivityLog** - GET /audit/activity - User activity log
- **AuditController.GetComplianceReport** - GET /audit/compliance - Compliance report
- **AuditController.GetRecoveryCandidates** - GET /audit/recovery - Recovery candidates

### SpecificationChainingExample

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample`
- **Summary:** Demonstrates the Specification pattern for building composable, type-safe queries.
             Specifications encapsulate query logic (filtering, sorting, eager loading) into
             reusable, testable components without coupling to specific repositories.

#### Methods & Properties

- **ProductSpecification.WithCategory** - Adds filter: WHERE CategoryId = categoryId
- **ProductSpecification.WithPriceRange** - Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
- **ProductSpecification.WithAvailableOnly** - Adds filter: WHERE IsAvailable = true
- **ProductSpecification.WithInStock** - Adds filter: WHERE StockQuantity > minStock
- **ProductSpecification.OrderByPrice** - Adds ordering: ORDER BY Price ASC
- **ProductSpecification.OrderByPriceDesc** - Adds ordering: ORDER BY Price DESC
- **ProductSpecification.OrderByName** - Adds ordering: ORDER BY Name ASC
- **ProductSpecification.WithPaging** - Applies pagination: OFFSET skip LIMIT take
- **SpecificationUsagePatterns.Example_SimpleFilter** - Example 1: Simple specification - find available products in category.
- **SpecificationUsagePatterns.Example_ComplexFilter** - Example 2: Complex specification - affordable, available products with pagination.
- **SpecificationUsagePatterns.Example_PremiumProducts** - Example 3: Premium products - expensive, in-stock items.
- **SpecificationUsagePatterns.Example_SpecificationReuse** - Example 4: Specification reuse across multiple queries.
            Single specification definition, many uses.
- **SpecificationCombination.Example_And** - Example: Combine multiple specifications with AND (all conditions must match).
- **SpecificationCombination.Example_Or** - Example: Combine specifications with OR (at least one condition must match).
- **SpecificationCombination.Example_Not** - Example: Negate specification with NOT (inverse matching).

### GuardClauseValidationExample

- **Namespace:** `SmartWorkz.Core.Examples.GuardClauseValidationExample`
- **Summary:** Demonstrates Guard utility class for precondition validation.
            Guards enforce invariants at domain boundaries (constructors, service methods).

#### Methods & Properties

- **Order.#ctor** - Constructor with comprehensive guard clauses.

### MultiTenancyFilteringExample

- **Namespace:** `SmartWorkz.Core.Examples.MultiTenancyFilteringExample`
- **Summary:** Demonstrates multi-tenancy with automatic tenant isolation.
            Repositories automatically filter queries by TenantId to ensure
            complete data separation between tenants.

#### Methods & Properties

- **MultiTenantUsage.Example_AutomaticTenantFiltering** - Repository automatically filters by current tenant.
            Example conceptual flow.
- **MultiTenantUsage.Example_QueryScoping** - All queries automatically scoped to current tenant.
- **MultiTenantUsage.Example_TenantSafety** - Cross-tenant access prevention.
            Even if caller passes different tenant ID, repository uses current.
- **TenantAwareService.GetActiveCustomers** - Service method automatically scoped to current tenant.
- **TenantAwareService.VerifyTenantOwnership** - Verify tenant ownership before accessing data.
- **CustomerController.GetCustomers** - GET /api/customers - List customers for current tenant.

### SoftDeleteBehaviorExample

- **Namespace:** `SmartWorkz.Core.Examples.SoftDeleteBehaviorExample`
- **Summary:** Demonstrates soft delete pattern for non-destructive data removal.
            Soft delete marks entities as deleted without removing them from the database,
            enabling recovery, audit trails, and data retention compliance.

#### Methods & Properties

- **CustomerSpecification.WithActive** - Include only active (non-deleted) customers.
- **CustomerSpecification.WithDeleted** - Include only soft-deleted customers.
- **CustomerSpecification.DeletedSince** - Filter deleted customers within date range.
- **SoftDeletePatterns.Example_SoftDelete** - Example 1: Soft delete workflow.
- **SoftDeletePatterns.Example_Restore** - Example 2: Restore soft-deleted entity.
- **SoftDeletePatterns.Example_AutomaticFiltering** - Example 3: Automatic soft-delete filtering.
            Repositories automatically exclude soft-deleted by default.
- **SoftDeletePatterns.Example_QueryDeleted** - Example 4: Query soft-deleted entities for audit/recovery.
- **SoftDeletePatterns.Example_RecoveryWindow** - Example 5: Deleted within grace period (30 days).
- **SoftDeletePatterns.Example_HardDelete** - Example 6: Hard delete after grace period.
- **SoftDeleteAudit.Example_AuditTrail** - Audit information captured on soft delete.
- **SoftDeleteAudit.Example_Compliance** - Compliance: Prove data is trackable.
- **SoftDeleteService.DeleteCustomer** - Service method: Delete customer (soft delete).
- **SoftDeleteService.RecoverCustomer** - Service method: Recover deleted customer.
- **SoftDeleteService.GetActiveCustomers** - Service method: List active customers (soft-deleted excluded).

### DTOMappingExample

- **Namespace:** `SmartWorkz.Core.Examples.DTOMappingExample`
- **Summary:** Demonstrates DTO mapping patterns for API contracts and data transformation.
            DTOs decouple internal domain models from external API representations,
            enabling flexible data transformation without exposing internal structures.

#### Methods & Properties

- **DTOMappingPatterns.MapToDto** - Example 1: Entity to DTO mapping (for Read operations).
- **DTOMappingPatterns.MapToEntity** - Example 2: DTO to Entity mapping (for Create operations).
- **DTOMappingPatterns.MapToDto** - Example 3: Batch mapping (collection).
- **DTOMappingPatterns.CustomerPublicDto.#ctor** - Example 4: Selective field exposure (security).
- **DTOMappingPatterns.Example_FlatteningValueObjects** - Example 5: Flattening nested value objects.
- **CustomerService.CreateCustomer** - CREATE: DTO -> Entity -> Persist -> DTO
- **CustomerService.GetCustomer** - READ: Entity -> DTO
- **CustomerService.UpdateCustomer** - UPDATE: DTO -> Update Entity -> Persist -> DTO
- **CustomerService.GetAllCustomers** - LIST: Entities -> DTOs
- **CustomerController.CreateCustomer** - POST /customers - Create from DTO
- **CustomerController.GetCustomer** - GET /customers/{id} - Return as DTO
- **CustomerController.UpdateCustomer** - PUT /customers/{id} - Update from DTO
- **CustomerController.GetAllCustomers** - GET /customers - Return as DTOs

### AuditTrailQueriesExample

- **Namespace:** `SmartWorkz.Core.Examples.AuditTrailQueriesExample`
- **Summary:** Demonstrates querying audit trails for compliance, accountability, and forensics.
            Audit trails track entity lifecycle (creation, modification, deletion) with
            user information and timestamps, enabling compliance audits and data recovery.

#### Methods & Properties

- **AuditTrailPatterns.Example_RecentlyCreated** - Example 1: Find recently created entities.
- **AuditTrailPatterns.Example_RecentlyModified** - Example 2: Find recently modified entities.
- **AuditTrailPatterns.Example_CreatedByUser** - Example 3: Find entities created by specific user.
- **AuditTrailPatterns.Example_Deleted** - Example 4: Find soft-deleted entities (recovery).
- **AuditTrailPatterns.Example_RecentlyDeleted** - Example 5: Find recently deleted (recovery window).
- **AuditTrailPatterns.Example_CountModified** - Example 6: Count modified in date range.
- **AuditReporting.GenerateUserActivityReport** - Generate report: Changes by user (who changed what).
- **AuditReporting.ComplianceReport.#ctor** - Generate compliance report: All changes in period.
- **AuditReporting.RecoveryCandidate.#ctor** - Generate data recovery report: Recently deleted available for restore.
- **AuditTrailService.GetActivityLog** - Get activity log for audit purposes.
- **AuditTrailService.GetComplianceReport** - Get compliance report for period.
- **AuditTrailService.GetRecoveryCandidates** - Get recovery candidates for restore.
- **AuditController.GetActivityLog** - GET /audit/activity - User activity log
- **AuditController.GetComplianceReport** - GET /audit/compliance - Compliance report
- **AuditController.GetRecoveryCandidates** - GET /audit/recovery - Recovery candidates

