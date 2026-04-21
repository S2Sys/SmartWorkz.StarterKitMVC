namespace SmartWorkz.Core;

/// <summary>
/// Generic base class for all auditable, soft-deletable, tenant-scoped entities.
///
/// Usage:
///   public class Product : AuditableEntity&lt;int&gt; { ... }   // int PK
///   public class User    : AuditableEntity&lt;string&gt; { ... } // string PK
///   public class Order   : AuditableEntity&lt;Guid&gt; { ... }   // Guid PK
///
/// The Id property is the canonical primary key. Existing entities that use
/// a named PK (e.g. CountryId) should map it via EF: HasKey(e => e.Id)
/// and configure the column name separately.
/// </summary>
public abstract class AuditableEntity<TId> : IAuditable, ISoftDeletable, ITenantScoped
{
    /// <summary>Primary key. Maps to the entity's PK column via EF configuration.</summary>
    public TId Id { get; set; } = default!;

    // --- IAuditable ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    // --- ISoftDeletable ---
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    // --- ITenantScoped ---
    public int? TenantId { get; set; }
}
