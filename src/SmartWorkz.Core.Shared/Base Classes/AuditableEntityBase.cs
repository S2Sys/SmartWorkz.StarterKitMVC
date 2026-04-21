namespace SmartWorkz.Shared;

using SmartWorkz.Core.Shared.Primitives;

public abstract class AuditableEntityBase<TId> : IEntity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public string? TenantId { get; set; }
}
