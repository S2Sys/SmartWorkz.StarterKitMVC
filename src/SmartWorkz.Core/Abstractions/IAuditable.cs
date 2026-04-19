namespace SmartWorkz.Core.Abstractions;

/// <summary>
/// Marks an entity that tracks who created and last modified it.
/// Implemented automatically by AuditableEntity base classes.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}
