namespace SmartWorkz.Core.Entities;

/// <summary>
/// Convenience base class for entities with an integer primary key.
/// Equivalent to AuditableEntity&lt;int&gt;.
/// Use this for Master/Report/Shared/Transaction entities (CountryId, ProductId, etc.)
/// </summary>
public abstract class AuditableEntity : AuditableEntity<int>
{
}
