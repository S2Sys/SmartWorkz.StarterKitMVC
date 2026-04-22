namespace SmartWorkz.Core;

/// <summary>
/// DEPRECATED: Use AuditEntity instead.
/// Convenience base class for auditable entities with an integer primary key.
/// </summary>
/// <remarks>
/// This is a non-generic convenience class equivalent to AuditableEntity{int}.
/// DEPRECATED: Use AuditEntity (without type parameter) for the same functionality with a cleaner design.
///
/// This class remains for backward compatibility but is marked as obsolete.
/// All existing functionality is preserved through inheritance from AuditableEntity{int}.
///
/// Old code:
/// <code>
/// public class Product : AuditableEntity
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
/// </code>
///
/// New code (recommended):
/// <code>
/// public class Product : AuditEntity
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
/// </code>
///
/// Primary Key: The Id property is an integer that maps to the database primary key column.
/// Entity Framework Core will automatically detect this as the primary key.
/// </remarks>
[Obsolete("Use AuditEntity instead. AuditableEntity has been refactored to align with the new standalone entity design. " +
          "See ARCHITECTURE.md for migration guidelines.", false)]
public abstract class AuditableEntity : AuditableEntity<int>
{
}
