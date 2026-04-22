namespace SmartWorkz.Core;

/// <summary>
/// Convenience base class for entities with an integer primary key.
/// </summary>
/// <remarks>
/// This is a non-generic convenience class equivalent to AuditableEntity&lt;int&gt;.
/// Use this class for entities that use an integer primary key, such as Master, Report, Shared,
/// or Transaction entities (e.g., CountryId, ProductId, CategoryId).
///
/// Inherits From: AuditableEntity&lt;int&gt;, providing audit tracking (CreatedAt, CreatedBy,
/// UpdatedAt, UpdatedBy), soft delete support (IsDeleted, DeletedAt, DeletedBy), and tenant
/// scoping (TenantId) out of the box.
///
/// Usage:
/// <code>
/// public class Product : AuditableEntity
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
///     public int CategoryId { get; set; }
/// }
/// </code>
///
/// Primary Key: The Id property is an integer that maps to the database primary key column.
/// Entity Framework Core will automatically detect this as the primary key during model
/// configuration.
/// </remarks>
public abstract class AuditableEntity : AuditableEntity<int>
{
}
