namespace SmartWorkz.Core.Shared.Primitives;

/// <summary>
/// Marks a class as a domain entity with a typed primary key.
/// </summary>
/// <typeparam name="TId">The type of the primary key (int, string, Guid, long).</typeparam>
public interface IEntity<TId>
{
    TId Id { get; }
}
