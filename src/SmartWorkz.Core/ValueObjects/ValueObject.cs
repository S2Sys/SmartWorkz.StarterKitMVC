namespace SmartWorkz.Core;

/// <summary>
/// Base class for all immutable value objects in the domain.
/// Provides equality semantics and hashing for value objects based on their atomic components.
/// </summary>
/// <remarks>
/// Domain-driven design value object pattern:
/// - Immutable: All properties are read-only and set during construction only
/// - Equality by Value: Two value objects are equal if all their atomic components are equal (not reference equality)
/// - Hash Code: Automatically computed from all atomic values, enabling use in collections
/// - Type-Safe: Each derived class enforces its own validation rules and invariants
///
/// Implementing classes must:
/// 1. Be sealed (to ensure equality semantics don't break in inheritance)
/// 2. Have a private constructor (to enforce factory method pattern via Create())
/// 3. Override GetAtomicValues() to yield all equality-determining components
/// 4. Implement a static Create() factory method that returns Result&lt;T&gt; for validation
///
/// This design ensures value objects cannot be instantiated in invalid states and properly
/// participate in domain models through value-based equality rather than reference identity.
/// </remarks>
/// <example>
/// Derived classes follow this pattern:
/// <code>
/// public sealed class EmailAddress : ValueObject
/// {
///     private EmailAddress(string value) => Value = value;
///
///     public string Value { get; }
///
///     public static Result&lt;EmailAddress&gt; Create(string? email)
///     {
///         if (string.IsNullOrWhiteSpace(email))
///             return Result.Fail&lt;EmailAddress&gt;(new Error("EMAIL_EMPTY", "Email cannot be empty"));
///
///         var trimmed = email.Trim().ToLowerInvariant();
///
///         if (!IsValidFormat(trimmed))
///             return Result.Fail&lt;EmailAddress&gt;(new Error("EMAIL_INVALID", "Email format is invalid"));
///
///         return Result.Ok&lt;EmailAddress&gt;(new EmailAddress(trimmed));
///     }
///
///     protected override IEnumerable&lt;object?&gt; GetAtomicValues()
///     {
///         yield return Value;
///     }
/// }
/// </code>
/// </example>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the atomic values that define this value object's identity.
    /// All properties that contribute to equality must be yielded here.
    /// </summary>
    /// <returns>An enumeration of atomic values in consistent order</returns>
    /// <remarks>
    /// Implementation should yield all properties that determine equality.
    /// Order matters: components are compared sequentially, so yield in the same
    /// order across instances to ensure consistent equality checks.
    /// Use null-coalescing in GetEqualityComponents to handle nullable values safely.
    /// </remarks>
    protected abstract IEnumerable<object?> GetAtomicValues();

    /// <summary>
    /// Converts atomic values to equality components, handling nulls safely.
    /// </summary>
    /// <returns>Atomic values with nulls replaced by empty string for consistent hashing</returns>
    /// <remarks>
    /// This method normalizes null values to empty strings to ensure consistent hash codes.
    /// It is called by Equals() and GetHashCode() to determine value object equality.
    /// </remarks>
    protected virtual IEnumerable<object> GetEqualityComponents()
    {
        foreach (var item in GetAtomicValues())
            yield return item ?? string.Empty;
    }

    /// <summary>
    /// Determines whether this value object equals another, based on atomic values.
    /// </summary>
    /// <param name="other">Another value object to compare</param>
    /// <returns>True if both objects are the same type and have identical atomic values</returns>
    /// <remarks>
    /// Two value objects are equal if:
    /// 1. They are the same type (type check prevents false positives in inheritance)
    /// 2. All their equality components match in sequence
    /// This is value-based equality, not reference equality.
    /// </remarks>
    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType()) return false;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Determines whether this value object equals another object.
    /// </summary>
    /// <param name="obj">Any object to compare</param>
    /// <returns>True if obj is a value object with identical atomic values</returns>
    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    /// <summary>
    /// Computes a hash code based on all atomic values.
    /// </summary>
    /// <returns>A hash code combining all equality components</returns>
    /// <remarks>
    /// Hash codes are computed from all atomic values to ensure that equal value objects
    /// have the same hash code. This enables value objects to be used in hash-based
    /// collections (HashSet, Dictionary) and LINQ operations correctly.
    /// </remarks>
    public override int GetHashCode()
        => GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component?.GetHashCode() ?? 0));

    /// <summary>
    /// Determines if two value objects are equal by value.
    /// </summary>
    /// <param name="left">The first value object (may be null)</param>
    /// <param name="right">The second value object (may be null)</param>
    /// <returns>True if both are null or have identical atomic values</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>
    /// Determines if two value objects are not equal by value.
    /// </summary>
    /// <param name="left">The first value object (may be null)</param>
    /// <param name="right">The second value object (may be null)</param>
    /// <returns>True if one is null and the other is not, or they have different atomic values</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);
}
