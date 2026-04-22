namespace SmartWorkz.Core;

/// <summary>
/// Generic base class for auditable, soft-deletable domain entities.
/// </summary>
/// <typeparam name="TId">The type of the primary key identifier. Must be a non-nullable, equatable type.</typeparam>
/// <remarks>
/// This abstract class is a standalone, self-contained entity that combines Entity&lt;TId&gt; functionality
/// with both audit trail tracking (IAuditable) and soft delete capability (ISoftDeletable) via direct
/// implementation without a hierarchical inheritance chain. It provides:
/// - Identity-based equality semantics from Entity&lt;TId&gt;
/// - Immutable creation timestamp and user tracking
/// - Mutable last-modified timestamp and user tracking
/// - Soft delete support with deletion timestamp and user tracking
///
/// Design Philosophy:
/// This standalone design makes AuditDeletableEntity self-contained and independent, avoiding forced
/// inheritance chains. The entity explicitly implements both IAuditable and ISoftDeletable, making
/// its responsibilities clear and easy to understand. This approach:
/// - Combines two critical concerns (audit + soft delete) directly
/// - Eliminates the need for abstract base class hierarchies
/// - Makes entity responsibilities explicit: "this is an audited, soft-deletable entity"
/// - Simplifies refactoring and maintenance
/// - Reduces cognitive load when understanding entity design
///
/// Audit Trail Behavior:
/// - CreatedAt: Set to the creation timestamp (typically DateTime.UtcNow). Should not be modified after creation.
/// - CreatedBy: Set to the user identifier who created the entity. Defaults to empty string.
/// - UpdatedAt: Set to the timestamp of the last modification. Null until first update occurs.
/// - UpdatedBy: Set to the user identifier who last modified the entity. Null until first update occurs.
///
/// Soft Delete Behavior:
/// - IsDeleted: Boolean flag indicating whether the entity is logically deleted. Default: false.
/// - DeletedAt: Timestamp of when the entity was deleted. Null when entity is active.
/// - DeletedBy: User ID of the user who deleted the entity. Null when entity is active.
/// - Repositories should filter out soft-deleted entities by default (WHERE IsDeleted = false).
/// - Soft-deleted entities can be recovered by setting IsDeleted = false and clearing DeletedAt/DeletedBy.
///
/// The audit and delete properties are settable at the class level to support persistence layer
/// scenarios where the database sets these values during insert/update/delete operations. However,
/// application logic should follow domain semantics:
/// - Treat CreatedAt/CreatedBy as immutable after creation
/// - Only set UpdatedAt/UpdatedBy when modifications occur
/// - Set all delete fields together to maintain consistency
///
/// Generic Primary Key Support:
/// This class can work with any primary key type:
/// - AuditDeletableEntity&lt;int&gt;: Integer primary key (most common)
/// - AuditDeletableEntity&lt;Guid&gt;: Globally unique identifier
/// - AuditDeletableEntity&lt;string&gt;: Natural key or string-based identifier
/// - AuditDeletableEntity&lt;long&gt;: Large integer identifiers
/// - Custom value objects: Any equatable, non-nullable type
///
/// Domain-Driven Design:
/// In DDD, entities maintain their identity-based equality semantics from Entity&lt;TId&gt;.
/// Two audit-deletable entities are considered equal if they have the same Id, regardless of their
/// audit or soft delete properties. This allows such entities to be used reliably in collections
/// and comparisons.
///
/// Example Usage:
/// <code>
/// // Define a domain entity with audit tracking and soft delete
/// public class Product : AuditDeletableEntity&lt;int&gt;
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
///
/// // Soft delete the entity
/// product.IsDeleted = true;
/// product.DeletedAt = DateTime.UtcNow.AddDays(1);
/// product.DeletedBy = 999;
/// </code>
///
/// Persistence Layer Integration:
/// - Set CreatedAt/CreatedBy when creating new entities
/// - Update UpdatedAt/UpdatedBy on every modification
/// - Set IsDeleted, DeletedAt, DeletedBy together when soft-deleting
/// - Use repository interceptors or value object setters to manage fields automatically
/// - Filter queries to exclude soft-deleted entities (WHERE IsDeleted = false)
/// - Ensure UpdatedAt/UpdatedBy remain null until the first update (following domain semantics)
/// - Ensure delete fields remain null until actual deletion occurs
///
/// Entity Framework Configuration: No special configuration is required for most scenarios.
/// EF Core will automatically detect all properties as columns. Consider configuring:
/// - Indexes on CreatedAt, UpdatedAt for efficient date range filtering
/// - Indexes on IsDeleted for soft delete queries
/// - Indexes on CreatedBy, UpdatedBy for user-based queries
/// - Use global query filters to automatically exclude soft-deleted entities in queries
/// </remarks>
public abstract class AuditDeletableEntity<TId> : Entity<TId>, IAuditable, ISoftDeletable
    where TId : notnull, IEquatable<TId>
{
    // --- IAuditable ---

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

    // --- ISoftDeletable ---

    /// <summary>
    /// Gets or sets a value indicating whether the entity is logically deleted (soft delete).
    /// </summary>
    /// <remarks>
    /// Boolean flag used to implement soft delete, where entities are marked as deleted
    /// instead of being permanently removed from the database.
    ///
    /// Behavior:
    /// - false (default): Entity is active and should be included in queries
    /// - true: Entity is logically deleted and should be excluded from queries
    ///
    /// Repository Filtering: All repository queries should automatically filter out
    /// soft-deleted entities (WHERE IsDeleted = false). Implement in the repository:
    /// <code>
    /// public async Task&lt;IEnumerable&lt;TEntity&gt;&gt; GetAllAsync(CancellationToken cancellationToken)
    /// {
    ///     return await context.Set&lt;TEntity&gt;()
    ///         .Where(e => !e.IsDeleted)  // Soft delete filter
    ///         .ToListAsync(cancellationToken);
    /// }
    /// </code>
    ///
    /// Hard Delete Prevention: Hard deletes (permanent removal) should not be used.
    /// Always use soft delete to preserve audit trails and enable accidental deletion recovery.
    ///
    /// Data Recovery: Soft-deleted entities can be easily recovered by setting IsDeleted = false
    /// and clearing DeletedAt/DeletedBy. Audit trails are preserved.
    ///
    /// Example Usage:
    /// <code>
    /// // Soft delete a product
    /// product.IsDeleted = true;
    /// product.DeletedAt = DateTime.UtcNow;
    /// product.DeletedBy = currentUserId;
    /// await repository.UpdateAsync(product);
    ///
    /// // Query for active products (soft-deleted excluded)
    /// var activeProducts = await repository.GetAllAsync();
    ///
    /// // Query for deleted products (for admin dashboard)
    /// var deletedProducts = await context.Products
    ///     .Where(p => p.IsDeleted)
    ///     .ToListAsync();
    /// </code>
    ///
    /// Performance: Add a database index on IsDeleted for efficient filtering of active entities
    /// in large tables.
    ///
    /// Database Schema: This property maps to a boolean/bit column in the database.
    /// Default: false (entity is active by default).
    /// </remarks>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the date and time (UTC) when the entity was deleted (soft delete).
    /// </summary>
    /// <remarks>
    /// Set to DateTime.UtcNow when the entity is soft-deleted (IsDeleted = true).
    /// Null indicates the entity is active (not deleted).
    ///
    /// Nullable: DateTime? to allow null when entity is not deleted.
    ///
    /// Usage: Use DeletedAt for audit trails, compliance reporting, and understanding
    /// when data was removed from active use.
    ///
    /// Time Zone: UTC for consistency across distributed systems.
    ///
    /// Example Queries:
    /// <code>
    /// // Find products deleted in the last 30 days
    /// var recentlyDeleted = await context.Products
    ///     .Where(p => p.IsDeleted && p.DeletedAt >= DateTime.UtcNow.AddDays(-30))
    ///     .ToListAsync();
    ///
    /// // Recover a deleted product
    /// var deletedProduct = await context.Products
    ///     .FirstOrDefaultAsync(p => p.Id == productId && p.IsDeleted);
    /// if (deletedProduct != null)
    /// {
    ///     deletedProduct.IsDeleted = false;
    ///     deletedProduct.DeletedAt = null;
    ///     deletedProduct.DeletedBy = null;
    ///     await repository.UpdateAsync(deletedProduct);
    /// }
    /// </code>
    ///
    /// Retention: Consider implementing a data retention policy that permanently deletes
    /// entities after a certain period (e.g., 90 days).
    ///
    /// Application Layer Setting: The service layer must set this field to DateTime.UtcNow
    /// when soft-deleting an entity. Must be set together with DeletedBy to maintain consistency.
    /// </remarks>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the user identifier (ID) of the user who deleted the entity (soft delete).
    /// </summary>
    /// <remarks>
    /// Set to the current user's ID when the entity is soft-deleted.
    /// Null indicates the entity is active (not deleted).
    ///
    /// Nullable: int? to allow null when entity is not deleted.
    ///
    /// Type: int by convention, matching typical user ID types.
    ///
    /// Audit Trail: Allows tracking who performed the deletion, useful for:
    /// - Compliance and regulatory reporting
    /// - Understanding data removals in audits
    /// - Accountability and access control
    ///
    /// Example Queries:
    /// <code>
    /// // Find products deleted by user 42
    /// var deletedByUser = await context.Products
    ///     .Where(p => p.IsDeleted && p.DeletedBy == 42)
    ///     .ToListAsync();
    ///
    /// // Audit: Who deleted what when
    /// var auditTrail = await context.Products
    ///     .Where(p => p.IsDeleted)
    ///     .OrderByDescending(p => p.DeletedAt)
    ///     .Select(p => new { p.Id, p.DeletedAt, p.DeletedBy })
    ///     .ToListAsync();
    /// </code>
    ///
    /// Application Layer Setting: The service layer must set this field before soft-deleting
    /// an entity, typically from the current user ID. Must be set together with DeletedAt
    /// to maintain consistency.
    /// </remarks>
    public int? DeletedBy { get; set; }
}

/// <summary>
/// Convenience base class for auditable, soft-deletable entities with an integer primary key.
/// </summary>
/// <remarks>
/// This is a non-generic convenience class equivalent to AuditDeletableEntity&lt;int&gt;.
/// Use this class when your auditable, soft-deletable entities use integer primary keys, which is
/// the most common scenario in traditional relational databases.
///
/// Example:
/// <code>
/// public class Product : AuditDeletableEntity
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
///
/// public class Category : AuditDeletableEntity
/// {
///     public string Name { get; set; }
/// }
/// </code>
///
/// The Id property is an integer that maps to the database primary key column.
/// Entity Framework Core automatically detects this as the primary key.
///
/// Inheritance:
/// AuditDeletableEntity → AuditDeletableEntity&lt;int&gt; → Entity&lt;int&gt; → Entity
///
/// Features:
/// - Identity-based equality from Entity&lt;int&gt;
/// - Audit trail: CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
/// - Soft delete: IsDeleted, DeletedAt, DeletedBy
/// - Integer primary key (int)
/// </remarks>
public abstract class AuditDeletableEntity : AuditDeletableEntity<int>
{
}
