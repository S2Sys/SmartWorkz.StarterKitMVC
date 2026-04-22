namespace SmartWorkz.Core;

/// <summary>
/// Generic base class for all domain entities.
/// </summary>
/// <typeparam name="TId">The type of the primary key identifier. Must be a non-nullable, equatable type.</typeparam>
/// <remarks>
/// This abstract class serves as the foundation for all domain entities in a Domain-Driven Design (DDD)
/// architecture. It implements value-based equality semantics where two entities are considered equal if
/// and only if they have the same identity (Id).
///
/// Key Features:
/// - Identity-based equality: Two entities are equal if their Ids are equal, regardless of other properties.
/// - Generic primary key: The TId type parameter allows flexibility in choosing key types:
///   - int: For auto-incrementing integer keys (most common in traditional databases)
///   - Guid: For globally unique identifiers (distributed systems, microservices)
///   - string: For natural keys (e.g., country codes, product SKUs, user emails)
///   - long: For large datasets exceeding int range
///   - custom value objects: Any equatable, non-nullable type
///
/// - Value equality implementation: Overrides Equals(object), GetHashCode(), and equality operators
///   (== and !=) to ensure consistent equality semantics across collections and comparisons.
///
/// Domain-Driven Design (DDD) Perspective:
/// In DDD, an entity is defined by its identity rather than its attributes. This class enforces that
/// principle by implementing equality based solely on the Id property. For example:
///
/// <code>
/// var customer1 = new Customer { Id = 1, Name = "Alice" };
/// var customer2 = new Customer { Id = 1, Name = "Alice" };
/// var customer3 = new Customer { Id = 1, Name = "Alicia" };
///
/// Assert.Equal(customer1, customer2); // True: same Id
/// Assert.Equal(customer1, customer3); // True: same Id (Name difference ignored)
/// </code>
///
/// This design allows for optimistic concurrency control and proper handling of entity state
/// changes throughout the application lifecycle.
///
/// Primary Key Constraint:
/// The TId type parameter has two constraints:
/// - notnull: Ensures the key cannot be null, preventing undefined identity.
/// - IEquatable&lt;TId&gt;: Ensures the key type can be compared for equality efficiently.
///
/// These constraints are necessary for:
/// - Reliable equality comparisons between entities
/// - Using entities as keys in dictionaries and hash sets
/// - Ensuring consistent behavior across Entity Framework Core and other ORM scenarios
///
/// Usage:
/// <code>
/// // Custom entity with Guid identifier
/// public class Order : Entity&lt;Guid&gt;
/// {
///     public DateTime OrderDate { get; set; }
///     public decimal TotalAmount { get; set; }
///     public List&lt;OrderItem&gt; Items { get; set; } = new();
/// }
///
/// // Custom entity with string identifier (natural key)
/// public class Country : Entity&lt;string&gt;
/// {
///     public string Name { get; set; }
///     public string Region { get; set; }
/// }
///
/// // Using the convenience Entity class (int primary key)
/// public class Product : Entity
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
/// </code>
///
/// Initialization:
/// When an Entity is instantiated, the Id is initialized to default(TId) for value types
/// (e.g., 0 for int, Guid.Empty for Guid) and default! (null-forgiving) for reference types.
/// In most scenarios, the Id is assigned by the persistence layer (e.g., Entity Framework Core,
/// custom ORM, or database triggers) rather than manually.
/// </remarks>
public abstract class Entity<TId> where TId : notnull, IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the primary key identifier for this entity.
    /// </summary>
    /// <remarks>
    /// This property uniquely identifies the entity within its aggregate and across the application.
    /// The value is typically assigned by the persistence layer during entity creation or loading.
    ///
    /// For value type Ids (int, Guid, long), the default value is the zero/empty value.
    /// For reference type Ids (string, custom classes), the default value requires explicit
    /// initialization or assignment.
    ///
    /// Entity Framework Core automatically detects this as the primary key due to the
    /// "Id" naming convention. No additional HasKey() configuration is required in most cases.
    /// </remarks>
    public TId Id { get; set; } = default!;

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>
    /// true if the specified object is an Entity&lt;TId&gt; with the same Id as the current entity;
    /// otherwise, false.
    /// </returns>
    /// <remarks>
    /// Equality is based solely on the Id property. Two entities are considered equal if and only if
    /// they are both instances of Entity&lt;TId&gt; (the same generic type) and have identical Id values.
    /// All other properties are ignored in the comparison.
    ///
    /// This implements the principle of value-based equality for entities in Domain-Driven Design.
    /// </remarks>
    public override bool Equals(object? obj) =>
        obj is Entity<TId> entity && Id.Equals(entity.Id);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current entity based on its Id.</returns>
    /// <remarks>
    /// The hash code is computed solely from the Id property. This ensures:
    /// - Two entities with the same Id have the same hash code (required for collections)
    /// - Entities can be used reliably in HashSet&lt;T&gt;, Dictionary&lt;TKey, TValue&gt;, and similar collections
    /// - Hash code consistency across the entity's lifetime (as long as Id doesn't change)
    ///
    /// Important: Changing an entity's Id after it has been added to a hash-based collection may
    /// result in undefined behavior. Avoid modifying Ids of entities in collections.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    /// <param name="left">The left entity to compare.</param>
    /// <param name="right">The right entity to compare.</param>
    /// <returns>
    /// true if both entities are equal (have the same Id); otherwise, false.
    /// </returns>
    /// <remarks>
    /// This operator provides a convenient way to compare entities using the == syntax.
    /// It delegates to the Equals(object) method for consistency.
    /// </remarks>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    /// <param name="left">The left entity to compare.</param>
    /// <param name="right">The right entity to compare.</param>
    /// <returns>
    /// true if the entities are not equal (have different Ids); otherwise, false.
    /// </returns>
    /// <remarks>
    /// This operator provides a convenient way to compare entities using the != syntax.
    /// It is the logical negation of the == operator.
    /// </remarks>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !(left == right);
}

/// <summary>
/// Convenience base class for entities with an integer primary key.
/// </summary>
/// <remarks>
/// This is a non-generic convenience class equivalent to Entity&lt;int&gt;.
/// Use this class when your entities use integer primary keys, which is the most common scenario
/// in traditional relational databases.
///
/// Example:
/// <code>
/// public class Product : Entity
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
///
/// public class Category : Entity
/// {
///     public string Name { get; set; }
/// }
/// </code>
///
/// The Id property is an integer that maps to the database primary key column.
/// Entity Framework Core automatically detects this as the primary key.
/// </remarks>
public abstract class Entity : Entity<int>
{
}
