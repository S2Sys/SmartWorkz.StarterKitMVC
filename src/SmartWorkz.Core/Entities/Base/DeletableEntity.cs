namespace SmartWorkz.Core;

/// <summary>
/// Generic base class for entities with soft delete support (logical delete, not physical delete).
/// </summary>
/// <typeparam name="TId">The type of the primary key identifier. Must be a non-nullable, equatable type.</typeparam>
/// <remarks>
/// This abstract class extends AuditEntity&lt;TId&gt; with soft delete functionality. Instead of physically
/// removing records from the database, entities are marked as deleted using the IsDeleted flag along with
/// timestamp and user tracking.
///
/// Soft Delete Behavior:
/// - IsDeleted: Boolean flag indicating deletion state (true = deleted, false = active)
/// - DeletedAt: UTC timestamp of when the entity was deleted, null if not deleted
/// - DeletedBy: User identifier who performed the deletion, null if not deleted
///
/// Key Features:
/// 1. Data Preservation: Deleted records remain in the database for audit and recovery
/// 2. Audit Trail: Complete history of who deleted what and when
/// 3. Referential Integrity: Foreign key relationships are maintained
/// 4. Recovery: Accidentally deleted records can be restored
/// 5. Compliance: Satisfies data retention and regulatory requirements
///
/// Inheritance Chain:
/// Entity&lt;TId&gt; → AuditEntity&lt;TId&gt; → DeletableEntity&lt;TId&gt;
///
/// This means DeletableEntity&lt;TId&gt; inherits:
/// - From Entity&lt;TId&gt;: Identity-based equality and the Id property
/// - From AuditEntity&lt;TId&gt;: Audit trail properties (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
/// - Native soft delete properties: IsDeleted, DeletedAt, DeletedBy
///
/// Generic Primary Key Support:
/// Like Entity and AuditEntity, this class supports any non-nullable, equatable type:
/// - DeletableEntity&lt;int&gt;: Integer primary key (most common)
/// - DeletableEntity&lt;Guid&gt;: Globally unique identifier
/// - DeletableEntity&lt;string&gt;: Natural key or string-based identifier
/// - DeletableEntity&lt;long&gt;: Large integer identifiers
/// - Custom value objects: Any equatable, non-nullable type
///
/// Default Values:
/// When a DeletableEntity is created:
/// - IsDeleted: false (entity is active)
/// - DeletedAt: null (not deleted yet)
/// - DeletedBy: null (no deletion user)
///
/// Soft Delete Pattern:
/// <code>
/// // Create an active entity
/// var product = new Product
/// {
///     Id = 1,
///     Name = "Widget",
///     CreatedAt = DateTime.UtcNow,
///     CreatedBy = "alice",
///     IsDeleted = false  // Initially active
/// };
///
/// // Later, soft delete the entity
/// product.IsDeleted = true;
/// product.DeletedAt = DateTime.UtcNow;
/// product.DeletedBy = "admin";
/// </code>
///
/// Querying Active Records:
/// Always filter by IsDeleted = false when querying for active records:
/// <code>
/// var activeProducts = await context.Products
///     .Where(p => !p.IsDeleted)
///     .ToListAsync();
/// </code>
///
/// Querying Deleted Records:
/// To view deleted records (for audit or recovery purposes):
/// <code>
/// var deletedProducts = await context.Products
///     .Where(p => p.IsDeleted)
///     .ToListAsync();
/// </code>
///
/// Database Schema:
/// For a Product entity inheriting from DeletableEntity, the database schema includes:
/// - Id: Primary key
/// - CreatedAt: Non-nullable DateTime, default GETUTCDATE()
/// - CreatedBy: Non-nullable string (default empty)
/// - UpdatedAt: Nullable DateTime
/// - UpdatedBy: Nullable string
/// - IsDeleted: Non-nullable bit/boolean, default 0/false
/// - DeletedAt: Nullable DateTime
/// - DeletedBy: Nullable string
///
/// Recommended Indexes:
/// - Clustered: (Id)
/// - Non-clustered: (IsDeleted, CreatedAt) for efficient active record queries
/// - Non-clustered: (DeletedAt) for auditing deleted records by date
/// - Non-clustered: (DeletedBy, DeletedAt) for accountability tracking
///
/// Soft Delete vs. Hard Delete:
/// Soft Delete Advantages:
/// - Preserves historical data and audit trails
/// - Allows recovery of accidentally deleted records
/// - Maintains referential integrity (soft-deleted records can still be referenced)
/// - Meets compliance and regulatory requirements
///
/// Hard Delete Trade-offs:
/// - Permanently removes data (cannot be recovered)
/// - Frees up database space
/// - Simpler query logic (no need to filter IsDeleted)
/// - May violate audit and compliance requirements
///
/// DDD Perspective:
/// In Domain-Driven Design, soft delete is a domain concept, not just a technical detail.
/// The "deleted" state is part of the entity's lifecycle and should be modeled as such.
/// Queries that exclude deleted records should be encapsulated in repository methods or
/// specifications to ensure consistent behavior across the application.
///
/// Persistence Layer Integration:
/// Entity Framework Core configuration example:
/// <code>
/// modelBuilder.Entity&lt;Product&gt;()
///     .HasQueryFilter(p => !p.IsDeleted)  // Auto-filter deleted records
///     .HasKey(p => p.Id)
///     .Property(p => p.IsDeleted)
///     .HasDefaultValue(false)
///     .Property(p => p.CreatedAt)
///     .HasDefaultValue(DateTime.UtcNow);
/// </code>
///
/// With query filters, all queries automatically exclude deleted records unless explicitly
/// using IgnoreQueryFilters(). This ensures consistent behavior and prevents accidental
/// inclusion of deleted records in normal queries.
/// </remarks>
public abstract class DeletableEntity<TId> : AuditEntity<TId>, ISoftDeletable where TId : notnull, IEquatable<TId>
{
    /// <summary>
    /// Gets or sets a value indicating whether this entity is deleted.
    /// </summary>
    /// <remarks>
    /// This boolean flag represents the soft delete state:
    /// - false: Entity is active and should be included in normal queries (default)
    /// - true: Entity is soft-deleted and should be excluded from normal queries
    ///
    /// Default Behavior:
    /// This property defaults to false when an entity is created. It should only be set to true
    /// when the entity is being soft-deleted.
    ///
    /// Update Semantics:
    /// When setting IsDeleted to true, also set DeletedAt and DeletedBy to maintain a complete
    /// soft delete record:
    /// <code>
    /// entity.IsDeleted = true;
    /// entity.DeletedAt = DateTime.UtcNow;
    /// entity.DeletedBy = currentUser.Id;
    /// </code>
    ///
    /// Query Impact:
    /// Entity Framework Core query filters can automatically exclude soft-deleted records:
    /// <code>
    /// modelBuilder.Entity&lt;Product&gt;()
    ///     .HasQueryFilter(p => !p.IsDeleted);
    /// </code>
    ///
    /// Performance:
    /// - Ensure IsDeleted is indexed for efficient filtering
    /// - Consider composite indexes like (IsDeleted, CreatedAt) for common queries
    /// </remarks>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time (UTC) when the entity was deleted, or null if not deleted.
    /// </summary>
    /// <remarks>
    /// This property tracks the deletion timestamp:
    /// - Null: Entity has not been deleted
    /// - DateTime value: Entity was soft-deleted at this UTC timestamp
    ///
    /// Default: Null (entity has not been deleted)
    ///
    /// Update Semantics:
    /// This property should be set together with IsDeleted and DeletedBy:
    /// <code>
    /// entity.IsDeleted = true;
    /// entity.DeletedAt = DateTime.UtcNow;
    /// entity.DeletedBy = currentUser.Id;
    /// </code>
    ///
    /// Consistency:
    /// For data consistency:
    /// - If IsDeleted = true, DeletedAt should not be null
    /// - If IsDeleted = false, DeletedAt should be null
    ///
    /// Querying by Deletion Date:
    /// <code>
    /// // Find products deleted in the last 7 days
    /// var recentlyDeleted = await context.Products
    ///     .Where(p => p.IsDeleted && p.DeletedAt >= DateTime.UtcNow.AddDays(-7))
    ///     .IgnoreQueryFilters()  // Required to include soft-deleted records
    ///     .ToListAsync();
    /// </code>
    ///
    /// Time Zone: Always uses UTC (Coordinated Universal Time). Convert to local time for display.
    ///
    /// Database:
    /// Map to a nullable DateTime column. Add an index for efficient querying of deletion dates.
    /// </remarks>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the user identifier of the user who deleted the entity, or null if not deleted.
    /// </summary>
    /// <remarks>
    /// This property identifies who performed the soft delete operation. It stores the user ID
    /// (typically an integer) of the user who deleted the record.
    ///
    /// Default: Null (entity has not been deleted)
    ///
    /// Update Semantics:
    /// This property should be set together with IsDeleted and DeletedAt:
    /// <code>
    /// entity.IsDeleted = true;
    /// entity.DeletedAt = DateTime.UtcNow;
    /// entity.DeletedBy = currentUser.Id;
    /// </code>
    ///
    /// Consistency:
    /// For data consistency:
    /// - If IsDeleted = true, DeletedBy should not be null
    /// - If IsDeleted = false, DeletedBy should be null
    ///
    /// Accountability and Auditing:
    /// Use DeletedBy to track who deleted records, enabling accountability and audit trails:
    /// <code>
    /// // Find all products deleted by a specific user (user ID 123)
    /// var userDeletions = await context.Products
    ///     .Where(p => p.IsDeleted && p.DeletedBy == 123)
    ///     .IgnoreQueryFilters()
    ///     .ToListAsync();
    ///
    /// // Find deletion statistics by user
    /// var deletionStats = await context.Products
    ///     .Where(p => p.IsDeleted)
    ///     .IgnoreQueryFilters()
    ///     .GroupBy(p => p.DeletedBy)
    ///     .Select(g => new { UserId = g.Key, Count = g.Count() })
    ///     .ToListAsync();
    /// </code>
    ///
    /// User Details:
    /// To retrieve full user details during queries, join with the User table:
    /// <code>
    /// var deletionsWithUserDetails = await context.Products
    ///     .Where(p => p.IsDeleted)
    ///     .IgnoreQueryFilters()
    ///     .Join(context.Users,
    ///         product => product.DeletedBy,
    ///         user => user.Id,
    ///         (product, user) => new { Product = product, DeletedByUser = user })
    ///     .ToListAsync();
    /// </code>
    /// </remarks>
    public int? DeletedBy { get; set; }
}

/// <summary>
/// Convenience base class for soft-deletable entities with an integer primary key.
/// </summary>
/// <remarks>
/// This is a non-generic convenience class equivalent to DeletableEntity&lt;int&gt;.
/// Use this class when your soft-deletable entities use integer primary keys, which is the most
/// common scenario in traditional relational databases.
///
/// Example:
/// <code>
/// public class Product : DeletableEntity
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
///
/// public class Category : DeletableEntity
/// {
///     public string Name { get; set; }
/// }
///
/// // Usage
/// var product = new Product
/// {
///     Id = 1,
///     Name = "Widget",
///     CreatedAt = DateTime.UtcNow,
///     CreatedBy = "alice",
///     IsDeleted = false
/// };
///
/// // Soft delete
/// product.IsDeleted = true;
/// product.DeletedAt = DateTime.UtcNow;
/// product.DeletedBy = "admin";
/// </code>
///
/// The Id property is an integer that maps to the database primary key column.
/// Entity Framework Core automatically detects this as the primary key.
///
/// Inheritance Chain:
/// DeletableEntity → DeletableEntity&lt;int&gt; → AuditEntity&lt;int&gt; → Entity&lt;int&gt;
/// </remarks>
public abstract class DeletableEntity : DeletableEntity<int>
{
}
