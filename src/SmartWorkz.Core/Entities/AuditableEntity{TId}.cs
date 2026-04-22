namespace SmartWorkz.Core;

/// <summary>
/// Generic base class for all auditable, soft-deletable, tenant-scoped entities.
/// </summary>
/// <typeparam name="TId">The type of the primary key identifier.</typeparam>
/// <remarks>
/// This class provides a comprehensive base for domain entities with built-in support for:
/// - Audit trailing (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
/// - Soft delete (IsDeleted, DeletedAt, DeletedBy)
/// - Multi-tenancy (TenantId)
///
/// Generic Primary Key: Use this class with your preferred primary key type:
/// - AuditableEntity&lt;int&gt;: Integer primary key (most common for Master/Report/Transaction entities)
/// - AuditableEntity&lt;string&gt;: String primary key (e.g., for codes or unique identifiers)
/// - AuditableEntity&lt;Guid&gt;: GUID primary key (distributed systems, DDD)
/// - AuditableEntity&lt;long&gt;: Long primary key (large datasets)
///
/// Usage Examples:
/// <code>
/// // Integer primary key (Master/Report/Transaction entities)
/// public class Product : AuditableEntity&lt;int&gt;
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
///
/// // GUID primary key (Domain-Driven Design, distributed systems)
/// public class Order : AuditableEntity&lt;Guid&gt;
/// {
///     public DateTime OrderDate { get; set; }
///     public decimal TotalAmount { get; set; }
/// }
///
/// // String primary key (e.g., country codes, product SKUs)
/// public class Country : AuditableEntity&lt;string&gt;
/// {
///     public string Name { get; set; }
/// }
/// </code>
///
/// Primary Key Configuration: The Id property is the canonical primary key and should be configured
/// as the primary key in Entity Framework Core. If an entity has a named primary key (e.g., CountryId),
/// map it via Entity Framework configuration:
/// <code>
/// modelBuilder.Entity&lt;Country&gt;()
///     .HasKey(e => e.Id);
/// </code>
///
/// Audit Trail Behavior:
/// - CreatedAt: Set to DateTime.UtcNow when the entity is first created. Should not be modified.
/// - CreatedBy: Set to the user ID (or identifier) of the user who created the entity. Type: int? (nullable).
/// - UpdatedAt: Set whenever the entity is updated. Should be set to DateTime.UtcNow by the application layer.
/// - UpdatedBy: Set whenever the entity is updated. Should be set to the user ID by the application layer.
/// - All audit fields are of type nullable (int?, DateTime?) to allow for initial creation without user context.
///
/// Soft Delete Behavior:
/// - IsDeleted: Boolean flag indicating whether the entity is logically deleted.
/// - DeletedAt: Timestamp of when the entity was deleted. Set to DateTime.UtcNow during soft delete.
/// - DeletedBy: User ID of the user who deleted the entity.
/// - Repositories should filter out soft-deleted entities by default (WHERE IsDeleted = false).
///
/// Multi-Tenancy:
/// - TenantId: Optional identifier for the tenant that owns the entity. Used to isolate data
///   between multiple tenants in a SaaS application.
/// - Repositories should filter by TenantId in multi-tenant applications.
///
/// Implications:
/// - All entities have audit tracking and soft delete enabled by default.
/// - Queries must explicitly handle IsDeleted when retrieving entities (filter out soft-deleted records).
/// - Update operations must set UpdatedAt and UpdatedBy before saving.
/// - Hard delete (removing the record) should not be used in favor of soft delete.
///
/// Entity Framework Configuration: No special configuration is required for most scenarios.
/// EF Core will automatically detect all properties as columns. Consider configuring:
/// - Indexes on CreatedAt, CreatedBy, UpdatedAt, UpdatedBy for faster filtering/sorting.
/// - Indexes on IsDeleted for efficient soft delete queries.
/// - Indexes on TenantId for multi-tenant queries.
/// </remarks>
public abstract class AuditableEntity<TId> : IAuditable, ISoftDeletable
{
    /// <summary>
    /// The primary key identifier for this entity.
    /// </summary>
    /// <remarks>
    /// This property uniquely identifies the entity within the database table.
    /// The type TId allows flexibility in choosing the primary key type:
    /// - int: For auto-incrementing integer keys (most common)
    /// - Guid: For globally unique identifiers (distributed systems)
    /// - string: For natural keys (e.g., country codes, product SKUs)
    /// - long: For large datasets exceeding int range
    ///
    /// In Entity Framework Core, this property is automatically detected as the primary key
    /// due to the naming convention (Id suffix). No explicit HasKey() configuration is required
    /// for most scenarios.
    ///
    /// Default Value: Initialized to default(TId) (typically 0 for integers, empty string for strings,
    /// Guid.Empty for GUIDs). The database will typically override this during insertion.
    /// </remarks>
    public TId Id { get; set; } = default!;

    // --- IAuditable ---

    /// <summary>
    /// The date and time (UTC) when the entity was created.
    /// </summary>
    /// <remarks>
    /// Auto-populated on entity creation to DateTime.UtcNow. This field should not be modified
    /// after creation, representing an immutable record of when the entity first entered the system.
    ///
    /// Usage: Use CreatedAt to find recently created entities, audit trails, or historical analysis.
    ///
    /// Example Query:
    /// <code>
    /// // Find products created in the last 7 days
    /// var recentProducts = await repository.GetAllAsync(p =>
    ///     p.CreatedAt >= DateTime.UtcNow.AddDays(-7));
    /// </code>
    ///
    /// Type: DateTime (not nullable). If entity creation timestamp is not available,
    /// consider using DateTime.UtcNow as a fallback value.
    ///
    /// Time Zone: All timestamps use UTC (Coordinated Universal Time) for consistency
    /// across distributed systems. Convert to local time zone for display.
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The user identifier of the user who created the entity.
    /// </summary>
    /// <remarks>
    /// Typically set to the current user's ID (as string) when the entity is created.
    /// This field allows tracking which user created the entity, useful for audit trails
    /// and accountability. Can be a numeric ID, username, email, or other user identifier
    /// depending on the authentication scheme.
    ///
    /// Default: Empty string to support scenarios where entity creation occurs without
    /// explicit user context (e.g., system-generated entities, imports).
    ///
    /// Usage: Join with a User/Account table to retrieve the creator's name, email, etc.
    ///
    /// Example Query:
    /// <code>
    /// // Find products created by user "user123"
    /// var productsByUser = await repository.GetAllAsync(p => p.CreatedBy == "user123");
    ///
    /// // Find products and include creator details
    /// var productsWithCreators = await context.Products
    ///     .Include(p => p.Creator)  // Assuming navigation property
    ///     .Where(p => p.CreatedBy != string.Empty)
    ///     .ToListAsync();
    /// </code>
    ///
    /// Application Layer Setting: The application layer (service or controller) must set
    /// this field before persisting a new entity, typically from HttpContext.User or
    /// a current user service.
    /// </remarks>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// The date and time (UTC) when the entity was last updated.
    /// </summary>
    /// <remarks>
    /// Initially null at creation and populated each time the entity is modified.
    /// This field allows tracking recent changes, ordering entities by modification date,
    /// and identifying stale entities.
    ///
    /// Nullable: This field is nullable (DateTime?) because the entity may never be updated
    /// after creation.
    ///
    /// Usage: Use UpdatedAt to find recently modified entities or to sort entities by
    /// modification date for display (e.g., "recently updated" lists).
    ///
    /// Example Query:
    /// <code>
    /// // Find products modified in the last 24 hours
    /// var recentlyModified = await repository.GetAllAsync(p =>
    ///     p.UpdatedAt != null && p.UpdatedAt >= DateTime.UtcNow.AddHours(-24));
    ///
    /// // Find products that have never been updated
    /// var neverUpdated = await repository.GetAllAsync(p => p.UpdatedAt == null);
    ///
    /// // Order by most recently updated
    /// var latest = await context.Products
    ///     .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
    ///     .Take(10)
    ///     .ToListAsync();
    /// </code>
    ///
    /// Application Layer Setting: The service layer must set this field to DateTime.UtcNow
    /// before persisting an update, typically in the Update or ApplyUpdates methods.
    ///
    /// Time Zone: UTC for consistency. Display times should be converted to the user's
    /// local time zone.
    /// </remarks>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The user identifier of the user who last updated the entity.
    /// </summary>
    /// <remarks>
    /// Set each time the entity is modified to the current user's ID (as string).
    /// This field allows tracking which user made the most recent changes, useful for
    /// collaboration tracking, accountability, and edit history.
    ///
    /// Nullable: This field is nullable (string?) because:
    /// - The entity may never be updated after creation (UpdatedAt is null, UpdatedBy is null)
    /// - System-generated updates without a user context (e.g., automated imports, cleanup jobs)
    ///
    /// Type: string to support various user identifier formats (numeric ID, username, email, etc.).
    ///
    /// Usage: Join with a User/Account table to retrieve the modifier's name, email, etc.
    ///
    /// Example Query:
    /// <code>
    /// // Find products modified by user "user456"
    /// var productsByUser = await repository.GetAllAsync(p => p.UpdatedBy == "user456");
    ///
    /// // Find products modified by any user (UpdatedBy is not null)
    /// var modified = await repository.GetAllAsync(p => p.UpdatedBy != null);
    ///
    /// // Order by most recently modified, including creator if never updated
    /// var latest = await context.Products
    ///     .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
    ///     .ToListAsync();
    /// </code>
    ///
    /// Application Layer Setting: The service layer must set this field before persisting
    /// an update, typically in the Update or ApplyUpdates methods, from the current user ID.
    ///
    /// Audit Trail: For a complete audit trail, consider storing all changes (not just the
    /// last modifier). This single field only tracks the most recent modifier.
    /// </remarks>
    public string? UpdatedBy { get; set; }

    // --- ISoftDeletable ---

    /// <summary>
    /// Indicates whether the entity is logically deleted (soft delete).
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
    /// </remarks>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The date and time (UTC) when the entity was deleted (soft delete).
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
    /// when soft-deleting an entity.
    /// </remarks>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// The user identifier (ID) of the user who deleted the entity (soft delete).
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
    /// an entity, typically from the current user ID.
    /// </remarks>
    public int? DeletedBy { get; set; }
}

