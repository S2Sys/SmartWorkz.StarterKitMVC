namespace SmartWorkz.Core;

/// <summary>
/// Marks an entity that tracks who created and last modified it.
/// Implemented automatically by AuditableEntity base classes.
/// </summary>
public interface IAuditable
{
    //T Id { get; set; }
    DateTime CreatedAt { get; set; }
    int? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    int? UpdatedBy { get; set; }
}
