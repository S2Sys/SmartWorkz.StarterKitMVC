namespace SmartWorkz.Core;

/// <summary>
/// Marker interface for entities that are scoped to a tenant in a multi-tenant application.
/// </summary>
/// <remarks>
/// This interface defines the contract for multi-tenant entities. In a multi-tenant system,
/// each entity belongs to a specific tenant and should not be accessible to other tenants.
///
/// Multi-Tenancy Concept:
/// Multi-tenancy is an architecture where a single instance of an application serves multiple
/// organizations (tenants), with each tenant's data logically isolated from others. The TenantId
/// property enables this isolation at the entity level.
///
/// Implementation Details:
/// - TenantId uniquely identifies the tenant that owns this entity
/// - All queries should filter by TenantId to ensure data isolation
/// - Repository and service layers should automatically include tenant filtering
/// - The TenantId is typically set from the current user's tenant context
///
/// Example Usage:
/// <code>
/// // Define a multi-tenant entity
/// public class Order : TenantEntity
/// {
///     public string OrderNumber { get; set; }
///     public decimal TotalAmount { get; set; }
/// }
///
/// // When querying, always filter by tenant
/// var orders = await context.Orders
///     .Where(o => o.TenantId == currentUserTenantId)
///     .ToListAsync();
/// </code>
///
/// Data Isolation Best Practices:
/// 1. Always include TenantId in WHERE clauses for queries
/// 2. Use repository methods that automatically apply tenant filtering
/// 3. Never bypass tenant filtering, even for admin users
/// 4. Log and audit cross-tenant access attempts
/// 5. Use database-level constraints to enforce tenant isolation
/// 6. Consider encrypting sensitive data per tenant
///
/// Row-Level Security (RLS):
/// For database-level enforcement, implement RLS policies that automatically restrict
/// records based on the current session's tenant context. This provides a second layer
/// of protection against application-level filtering bypasses.
/// </remarks>
public interface ITenantScoped
{
    /// <summary>
    /// Gets the unique identifier of the tenant that owns this entity.
    /// </summary>
    /// <remarks>
    /// This property identifies which tenant owns the entity in a multi-tenant system.
    /// It should never be null or empty in a valid, persistent entity.
    ///
    /// Tenant ID Format:
    /// The format of TenantId depends on the organization's tenant identification scheme:
    /// - Tenant Name: "acme-corp", "customer-123"
    /// - Numeric ID: "12345", "98765"
    /// - GUID: "550e8400-e29b-41d4-a716-446655440000"
    /// - Custom Format: Whatever makes sense for your multi-tenant model
    ///
    /// Default Behavior:
    /// When a new entity is created, TenantId should be set from the current user's
    /// tenant context. The persistence layer or service layer typically handles this
    /// automatically.
    ///
    /// Querying:
    /// <code>
    /// // Retrieve all entities for a specific tenant
    /// var tenantEntities = await context.Set&lt;Order&gt;()
    ///     .Where(e => e.TenantId == tenantId)
    ///     .ToListAsync();
    /// </code>
    ///
    /// Security:
    /// - Always validate that the current user belongs to the tenant being queried
    /// - Never allow users from Tenant A to access data from Tenant B
    /// - Log and monitor attempts to access mismatched tenant data
    /// </remarks>
    string TenantId { get; }
}
