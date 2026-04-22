namespace SmartWorkz.Core;

/// <summary>
/// Generic base class for multi-tenant domain entities with full inheritance stack.
/// </summary>
/// <typeparam name="TId">The type of the primary key identifier. Must be a non-nullable, equatable type.</typeparam>
/// <remarks>
/// This abstract class extends DeletableEntity&lt;TId&gt; with multi-tenant support. It provides
/// complete inheritance chain for domain entities in a multi-tenant Domain-Driven Design (DDD) architecture.
///
/// Full Inheritance Chain:
/// Entity&lt;TId&gt; → AuditEntity&lt;TId&gt; → DeletableEntity&lt;TId&gt; → TenantEntity&lt;TId&gt;
///
/// This means TenantEntity&lt;TId&gt; inherits:
/// - From Entity&lt;TId&gt;: Identity-based equality and the Id property
/// - From AuditEntity&lt;TId&gt;: Audit trail properties (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
/// - From DeletableEntity&lt;TId&gt;: Soft delete properties (IsDeleted, DeletedAt, DeletedBy)
/// - Native multi-tenant property: TenantId
///
/// Multi-Tenancy Architecture:
/// In a multi-tenant system, each entity belongs to a specific tenant. The TenantId property
/// uniquely identifies the tenant that owns the entity. This ensures data isolation and prevents
/// cross-tenant data access.
///
/// Key Features:
/// 1. Tenant Isolation: TenantId segregates data by tenant
/// 2. Audit Trail: Tracks creation, modification, and deletion with user context
/// 3. Soft Delete: Preserves deleted records for compliance and recovery
/// 4. Identity-based Equality: Two entities are equal if they have the same Id
/// 5. Generic Primary Key: Supports any non-nullable, equatable type
///
/// Generic Primary Key Support:
/// Like Entity, AuditEntity, and DeletableEntity, this class supports any non-nullable, equatable type:
/// - TenantEntity&lt;int&gt;: Integer primary key (most common)
/// - TenantEntity&lt;Guid&gt;: Globally unique identifier
/// - TenantEntity&lt;string&gt;: Natural key or string-based identifier
/// - TenantEntity&lt;long&gt;: Large integer identifiers
/// - Custom value objects: Any equatable, non-nullable type
///
/// Default Values:
/// When a TenantEntity is created:
/// - TenantId: empty string (must be set by application/persistence layer)
/// - IsDeleted: false (entity is active)
/// - DeletedAt: null (not deleted yet)
/// - DeletedBy: null (no deletion user)
/// - CreatedAt: DateTime.MinValue (must be set by persistence layer)
/// - CreatedBy: empty string (must be set by application layer)
/// - UpdatedAt: null (no updates yet)
/// - UpdatedBy: null (no updates yet)
///
/// Multi-Tenant Usage Pattern:
/// <code>
/// // Define a multi-tenant domain entity
/// public class Order : TenantEntity
/// {
///     public string OrderNumber { get; set; }
///     public decimal TotalAmount { get; set; }
///     public List&lt;OrderItem&gt; Items { get; set; } = new();
/// }
///
/// // When creating a new order
/// var order = new Order
/// {
///     OrderNumber = "ORD-12345",
///     TotalAmount = 99.99m,
///     TenantId = currentUser.TenantId,  // Set from current tenant context
///     CreatedAt = DateTime.UtcNow,
///     CreatedBy = currentUser.Id
/// };
///
/// // When querying, always filter by tenant
/// var tenantOrders = await context.Orders
///     .Where(o => o.TenantId == currentUserTenantId && !o.IsDeleted)
///     .ToListAsync();
/// </code>
///
/// Persistence Layer Integration:
/// - Set TenantId from the current user's tenant context when creating entities
/// - Apply automatic tenant filtering in repository methods
/// - Consider using Entity Framework Core's query filters for automatic tenant filtering
/// - Implement row-level security (RLS) at the database level for additional protection
///
/// Example EF Core Configuration:
/// <code>
/// modelBuilder.Entity&lt;Order&gt;()
///     .HasQueryFilter(o => o.TenantId == EF.Property&lt;string&gt;(o, "TenantId"))
///     .HasKey(o => o.Id);
/// </code>
///
/// Security Considerations:
/// 1. Always verify the current user's tenant matches the entity's TenantId
/// 2. Never allow cross-tenant data access, even for administrators
/// 3. Log all access attempts for audit trails
/// 4. Use database-level constraints (RLS) to enforce tenant isolation
/// 5. Encrypt sensitive data per tenant for additional security
/// 6. Consider using cryptographic techniques to ensure TenantId immutability
///
/// DDD Perspective:
/// In Domain-Driven Design, multi-tenancy is a domain concept, not just a technical detail.
/// The tenant context is part of the bounded context and should be modeled explicitly.
/// Aggregates and entities should enforce tenant boundaries through their business logic.
///
/// Inheritance Chain Validation:
/// This entity validates the full inheritance chain by ensuring all base class properties
/// are accessible and functional:
/// - Entity properties: Id, Equals(), GetHashCode()
/// - AuditEntity properties: CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
/// - DeletableEntity properties: IsDeleted, DeletedAt, DeletedBy
/// - TenantEntity properties: TenantId, ITenantScoped implementation
/// </remarks>
public abstract class TenantEntity<TId> : DeletableEntity<TId>, ITenantScoped where TId : notnull, IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the unique identifier of the tenant that owns this entity.
    /// </summary>
    /// <remarks>
    /// This property identifies which tenant owns the entity in a multi-tenant system.
    /// It defaults to an empty string and should be set by the application layer when
    /// creating new entities, typically from the current user's tenant context.
    ///
    /// Tenant ID Format:
    /// The format of TenantId depends on the organization's tenant identification scheme:
    /// - Tenant Name: "acme-corp", "customer-123"
    /// - Numeric ID: "12345", "98765"
    /// - GUID: "550e8400-e29b-41d4-a716-446655440000"
    /// - Custom Format: Whatever makes sense for your multi-tenant model
    ///
    /// Default: Empty string (must be set before persistence)
    ///
    /// Immutability Consideration:
    /// In some multi-tenant models, TenantId is considered immutable after creation.
    /// Consider making this property read-only in your domain entity or implementing
    /// domain events to prevent unauthorized tenant reassignment.
    ///
    /// Querying by TenantId:
    /// <code>
    /// // Retrieve all active entities for a specific tenant
    /// var tenantEntities = await context.Orders
    ///     .Where(o => o.TenantId == tenantId && !o.IsDeleted)
    ///     .ToListAsync();
    ///
    /// // Include deleted records with IgnoreQueryFilters()
    /// var allTenantEntities = await context.Orders
    ///     .Where(o => o.TenantId == tenantId)
    ///     .IgnoreQueryFilters()
    ///     .ToListAsync();
    /// </code>
    ///
    /// Data Isolation:
    /// - Always include TenantId in WHERE clauses
    /// - Use repository methods that automatically apply tenant filtering
    /// - Implement query filters at the Entity Framework Core level
    /// - Enforce tenant isolation at the database level with RLS
    ///
    /// Null Safety:
    /// While this property defaults to empty string (not null), it should never be null
    /// in a valid, persistent entity. The empty string default serves as a sentinel value
    /// indicating that the TenantId has not been set.
    ///
    /// Validation:
    /// Consider implementing domain validation to ensure TenantId is not empty before
    /// persisting new entities, or rely on the database schema to enforce NOT NULL constraints.
    /// </remarks>
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// Convenience base class for multi-tenant entities with an integer primary key.
/// </summary>
/// <remarks>
/// This is a non-generic convenience class equivalent to TenantEntity&lt;int&gt;.
/// Use this class when your multi-tenant entities use integer primary keys, which is the most
/// common scenario in traditional relational databases.
///
/// Example:
/// <code>
/// public class Order : TenantEntity
/// {
///     public string OrderNumber { get; set; }
///     public decimal TotalAmount { get; set; }
/// }
///
/// public class Invoice : TenantEntity
/// {
///     public string InvoiceNumber { get; set; }
///     public decimal Amount { get; set; }
/// }
///
/// // Usage
/// var order = new Order
/// {
///     Id = 1,
///     OrderNumber = "ORD-001",
///     TenantId = "customer-123",
///     CreatedAt = DateTime.UtcNow,
///     CreatedBy = "alice",
///     IsDeleted = false
/// };
/// </code>
///
/// The Id property is an integer that maps to the database primary key column.
/// Entity Framework Core automatically detects this as the primary key.
///
/// Inheritance Chain:
/// TenantEntity → TenantEntity&lt;int&gt; → DeletableEntity&lt;int&gt; → AuditEntity&lt;int&gt; → Entity&lt;int&gt;
/// </remarks>
public abstract class TenantEntity : TenantEntity<int>
{
}
