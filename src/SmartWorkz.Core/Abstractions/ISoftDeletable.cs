namespace SmartWorkz.Core;

/// <summary>
/// Marks an entity that supports soft deletion.
/// Infrastructure layer (DbContext, Dapper interceptors) should filter IsDeleted = false automatically.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    int? DeletedBy { get; set; }
}
