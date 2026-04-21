namespace SmartWorkz.Core;

/// <summary>
/// Base class for immutable value objects.
/// Two value objects are equal if all their components are equal.
///
/// Usage:
///   public sealed class EmailAddress : ValueObject
///   {
///       public string Value { get; }
///       public EmailAddress(string value)
///       {
///           if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Email required.");
///           Value = value.Trim().ToLowerInvariant();
///       }
///       protected override IEnumerable&lt;object&gt; GetEqualityComponents()
///       {
///           yield return Value;
///       }
///   }
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetAtomicValues();

    protected virtual IEnumerable<object> GetEqualityComponents()
    {
        foreach (var item in GetAtomicValues())
            yield return item ?? string.Empty;
    }

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType()) return false;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public override int GetHashCode()
        => GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);
}
