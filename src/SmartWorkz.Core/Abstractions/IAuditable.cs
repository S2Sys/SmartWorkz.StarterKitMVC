namespace SmartWorkz.Core;

/// <summary>
/// Marks an entity that tracks who created and last modified it.
/// Implemented by auditable entity classes to provide creation and modification metadata.
/// </summary>
/// <remarks>
/// This interface defines the audit trail contract for entities that track:
/// - Creation timestamp and user
/// - Last modification timestamp and user
///
/// All properties are read-only in the interface contract, but implementing classes
/// may provide settable properties for the persistence layer.
///
/// Usage: Implement this interface on entities that require audit tracking to maintain
/// a complete history of who created and modified records.
///
/// Example:
/// <code>
/// public class Product : AuditEntity&lt;int&gt;
/// {
///     public string Name { get; set; }
/// }
///
/// // Usage with interface
/// IAuditable auditable = product;
/// var createdAt = auditable.CreatedAt;
/// var createdBy = auditable.CreatedBy;
/// </code>
/// </remarks>
public interface IAuditable
{
    /// <summary>
    /// Gets the date and time (UTC) when the entity was created.
    /// </summary>
    /// <remarks>
    /// This timestamp is immutable once set and represents when the entity
    /// first entered the system. It should never be modified after creation.
    /// </remarks>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the user identifier of the user who created the entity.
    /// </summary>
    /// <remarks>
    /// Typically a user ID or username that identifies who created this entity.
    /// Used for audit trails and accountability tracking.
    /// Empty string by default for entities created without explicit user context.
    /// </remarks>
    string CreatedBy { get; }

    /// <summary>
    /// Gets the date and time (UTC) when the entity was last updated.
    /// </summary>
    /// <remarks>
    /// This property is null until the entity is modified. After the first update,
    /// it should be set to DateTime.UtcNow by the application layer.
    /// </remarks>
    DateTime? UpdatedAt { get; }

    /// <summary>
    /// Gets the user identifier of the user who last updated the entity.
    /// </summary>
    /// <remarks>
    /// This property is null until the entity is modified. After the first update,
    /// it should be set to the current user's identifier.
    /// Only the last modifier is tracked; for complete history, use audit logs.
    /// </remarks>
    string? UpdatedBy { get; }
}
