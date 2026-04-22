namespace SmartWorkz.Core;

/// <summary>
/// DEPRECATED: Use AuditEntity{TId} instead.
/// Generic base class for auditable, soft-deletable entities.
/// </summary>
/// <typeparam name="TId">The type of the primary key identifier.</typeparam>
/// <remarks>
/// This class is now obsolete and has been refactored to align with the new standalone entity design.
/// It now inherits from AuditEntity{TId} for backward compatibility while maintaining all existing functionality.
///
/// MIGRATION GUIDE:
/// This class previously included soft delete support (IsDeleted, DeletedAt, DeletedBy).
/// The new AuditEntity design separates concerns:
/// - AuditEntity{TId}: Audit tracking only (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
/// - For soft delete support, create a specialized entity that implements both Entity{TId} and ISoftDeletable
///
/// Why the change?
/// - Eliminates forced inheritance chains
/// - Makes entity responsibilities explicit
/// - Improves code clarity and maintainability
/// - Aligns with Domain-Driven Design principles
///
/// Old code:
/// <code>
/// public class Product : AuditableEntity{TId}
/// {
///     public string Name { get; set; }
/// }
/// </code>
///
/// New approach (recommended):
/// - For audit tracking only: public class Product : AuditEntity{TId}
/// - For audit + soft delete: Create a composite entity or use separate interceptors
///
/// BACKWARD COMPATIBILITY:
/// This class remains functional for existing code but is marked obsolete to encourage migration.
/// All existing functionality is preserved through inheritance from AuditEntity{TId}.
/// The soft delete properties (IsDeleted, DeletedAt, DeletedBy) are no longer part of the base class
/// and should be added to specific entities that require soft delete support.
/// </remarks>
[Obsolete("Use AuditEntity<TId> instead. AuditableEntity has been refactored to align with the new standalone entity design. " +
          "For soft delete support, implement ISoftDeletable separately on entities that need it. " +
          "See ARCHITECTURE.md for migration guidelines.", false)]
public abstract class AuditableEntity<TId> : AuditEntity<TId> where TId : notnull, IEquatable<TId>
{
    /// <summary>
    /// DEPRECATED: Use AuditEntity properties instead.
    /// Indicates whether the entity is logically deleted (soft delete).
    /// </summary>
    /// <remarks>
    /// This property is kept for backward compatibility with existing code that inherits from AuditableEntity.
    /// New code should implement ISoftDeletable separately on entities that require soft delete functionality.
    /// </remarks>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// DEPRECATED: Use soft delete properties from a dedicated soft-delete entity instead.
    /// The date and time (UTC) when the entity was deleted (soft delete).
    /// </summary>
    /// <remarks>
    /// This property is kept for backward compatibility with existing code.
    /// New code should implement ISoftDeletable separately.
    /// </remarks>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// DEPRECATED: Use soft delete properties from a dedicated soft-delete entity instead.
    /// The user identifier (ID) of the user who deleted the entity (soft delete).
    /// </summary>
    /// <remarks>
    /// This property is kept for backward compatibility with existing code.
    /// New code should implement ISoftDeletable separately.
    /// </remarks>
    public int? DeletedBy { get; set; }
}

