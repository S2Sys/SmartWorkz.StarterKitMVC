namespace SmartWorkz.Core;

/// <summary>
/// Generic base class for auditable domain entities that track creation and modification metadata.
/// </summary>
/// <typeparam name="TId">The type of the primary key identifier. Must be a non-nullable, equatable type.</typeparam>
/// <remarks>
/// This abstract class extends Entity&lt;TId&gt; with built-in audit trail functionality. It provides:
/// - Immutable creation timestamp and user tracking
/// - Mutable last-modified timestamp and user tracking
/// - Identity-based equality semantics inherited from Entity&lt;TId&gt;
///
/// Audit Trail Behavior:
/// - CreatedAt: Set to the creation timestamp (typically DateTime.UtcNow). Should not be modified after creation.
/// - CreatedBy: Set to the user identifier who created the entity. Defaults to empty string.
/// - UpdatedAt: Set to the timestamp of the last modification. Null until first update occurs.
/// - UpdatedBy: Set to the user identifier who last modified the entity. Null until first update occurs.
///
/// The audit properties are settable at the class level to support persistence layer scenarios where
/// the database sets these values during insert/update operations. However, application logic should
/// treat CreatedAt/CreatedBy as immutable and only set UpdatedAt/UpdatedBy when modifications occur.
///
/// Generic Primary Key Support:
/// This class can work with any primary key type:
/// - AuditEntity&lt;int&gt;: Integer primary key (most common)
/// - AuditEntity&lt;Guid&gt;: Globally unique identifier
/// - AuditEntity&lt;string&gt;: Natural key or string-based identifier
/// - AuditEntity&lt;long&gt;: Large integer identifiers
/// - Custom value objects: Any equatable, non-nullable type
///
/// Domain-Driven Design:
/// In DDD, entities maintain their identity-based equality semantics from Entity&lt;TId&gt;.
/// Two audit entities are considered equal if they have the same Id, regardless of their audit properties.
/// This allows audit entities to be used reliably in collections and comparisons.
///
/// Example Usage:
/// <code>
/// // Define a domain entity with audit tracking
/// public class Product : AuditEntity&lt;int&gt;
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
///
/// // Create and use the entity
/// var product = new Product
/// {
///     Id = 1,
///     Name = "Widget",
///     CreatedAt = DateTime.UtcNow,
///     CreatedBy = "user123"
/// };
///
/// // Track modifications
/// product.UpdatedAt = DateTime.UtcNow.AddHours(1);
/// product.UpdatedBy = "user456";
/// </code>
///
/// Persistence Layer Integration:
/// - Set CreatedAt/CreatedBy when creating new entities
/// - Update UpdatedAt/UpdatedBy on every modification
/// - Use repository interceptors or value object setters to manage audit fields automatically
/// - Ensure UpdatedAt/UpdatedBy remain null until the first update (following domain semantics)
/// </remarks>
public abstract class AuditEntity<TId> : Entity<TId>, IAuditable where TId : notnull, IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the date and time (UTC) when the entity was created.
    /// </summary>
    /// <remarks>
    /// This property is set when the entity is first created and should ideally remain immutable
    /// throughout the entity's lifetime. The persistence layer typically sets this value using
    /// DateTime.UtcNow when inserting a new record.
    ///
    /// Application Layer:
    /// - Do not modify this value after entity creation
    /// - Set during entity construction or by the repository/service layer
    /// - Use for audit trails, historical analysis, and sorting by creation date
    ///
    /// Database:
    /// - Map to a non-nullable DateTime column with a default value of GETUTCDATE() or equivalent
    /// - Consider adding an index for efficient date range queries
    ///
    /// Example Query:
    /// <code>
    /// // Find recently created products
    /// var recentProducts = await context.Products
    ///     .Where(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    ///     .ToListAsync();
    /// </code>
    ///
    /// Time Zone: Always uses UTC (Coordinated Universal Time) for consistency across
    /// distributed systems. Convert to local time for display.
    /// </remarks>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user identifier of the user who created the entity.
    /// </summary>
    /// <remarks>
    /// This property identifies who created the entity. It can be a user ID, username, email,
    /// or any other user identifier depending on the application's authentication scheme.
    /// Defaults to empty string (not null) when an entity is created without explicit user context.
    ///
    /// Application Layer:
    /// - Set when creating new entities, typically from the current user context
    /// - Should not be modified after creation (treat as immutable)
    /// - Use for accountability, audit trails, and permission checks
    /// - Join with User table to retrieve creator details (name, email, etc.)
    ///
    /// Example Usage:
    /// <code>
    /// var product = new Product
    /// {
    ///     Name = "Widget",
    ///     CreatedAt = DateTime.UtcNow,
    ///     CreatedBy = currentUser.Id.ToString()
    /// };
    ///
    /// // Find products created by a specific user
    /// var productsByUser = await context.Products
    ///     .Where(p => p.CreatedBy == "user123")
    ///     .ToListAsync();
    /// </code>
    ///
    /// Nullable Behavior:
    /// While the property has a default of empty string, it can be set to null-like values
    /// for system-generated entities or imports without user context.
    /// </remarks>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time (UTC) when the entity was last updated.
    /// </summary>
    /// <remarks>
    /// This property is initially null (before any update) and is set to DateTime.UtcNow
    /// each time the entity is modified. Following domain semantics, an entity that has never
    /// been updated will have UpdatedAt = null.
    ///
    /// Application Layer:
    /// - Null on entity creation (no updates yet)
    /// - Set to DateTime.UtcNow whenever the entity is updated
    /// - Use for tracking recently modified entities and sorting by modification date
    /// - Compare with CreatedAt to determine if entity has been modified
    ///
    /// Example Queries:
    /// <code>
    /// // Find products modified in the last 24 hours
    /// var recentlyModified = await context.Products
    ///     .Where(p => p.UpdatedAt != null && p.UpdatedAt >= DateTime.UtcNow.AddHours(-24))
    ///     .ToListAsync();
    ///
    /// // Find products that have never been updated
    /// var neverUpdated = await context.Products
    ///     .Where(p => p.UpdatedAt == null)
    ///     .ToListAsync();
    ///
    /// // Sort by modification date, or creation date if never modified
    /// var latest = await context.Products
    ///     .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
    ///     .Take(10)
    ///     .ToListAsync();
    /// </code>
    ///
    /// Database:
    /// - Map to a nullable DateTime column
    /// - Do not set a default value; leave null for new records
    /// - Consider adding an index for efficient filtering of recently modified records
    ///
    /// Time Zone: Always uses UTC. Convert to local time for display purposes.
    /// </remarks>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user identifier of the user who last updated the entity.
    /// </summary>
    /// <remarks>
    /// This property identifies who made the most recent modification to the entity.
    /// It is initially null and is set each time the entity is updated.
    ///
    /// Application Layer:
    /// - Null on entity creation (no updates yet)
    /// - Set to the current user identifier whenever the entity is updated
    /// - Use for accountability tracking and understanding who made changes
    /// - Join with User table to retrieve modifier details
    ///
    /// Example Usage:
    /// <code>
    /// // When updating an entity
    /// product.UpdatedAt = DateTime.UtcNow;
    /// product.UpdatedBy = currentUser.Id.ToString();
    ///
    /// // Find products modified by a specific user
    /// var productsByUser = await context.Products
    ///     .Where(p => p.UpdatedBy == "user456")
    ///     .ToListAsync();
    ///
    /// // Find products that have never been updated
    /// var neverUpdated = await context.Products
    ///     .Where(p => p.UpdatedBy == null)
    ///     .ToListAsync();
    /// </code>
    ///
    /// Audit Trail Limitations:
    /// This property only tracks the most recent modifier. For a complete audit trail showing
    /// all changes and who made them, implement a separate audit log table or event sourcing mechanism.
    ///
    /// Null Semantics:
    /// Null indicates the entity has never been updated. Both UpdatedAt and UpdatedBy should be
    /// set together or remain null together to maintain consistency.
    /// </remarks>
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Convenience base class for auditable entities with an integer primary key.
/// </summary>
/// <remarks>
/// This is a non-generic convenience class equivalent to AuditEntity&lt;int&gt;.
/// Use this class when your auditable entities use integer primary keys, which is the most
/// common scenario in traditional relational databases.
///
/// Example:
/// <code>
/// public class Product : AuditEntity
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
///
/// public class Category : AuditEntity
/// {
///     public string Name { get; set; }
/// }
/// </code>
///
/// The Id property is an integer that maps to the database primary key column.
/// Entity Framework Core automatically detects this as the primary key.
///
/// Inheritance Chain:
/// AuditEntity → AuditEntity&lt;int&gt; → Entity&lt;int&gt; → Entity&lt;int&gt;
/// </remarks>
public abstract class AuditEntity : AuditEntity<int>
{
}
