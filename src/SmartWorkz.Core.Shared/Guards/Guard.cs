namespace SmartWorkz.Shared;

/// <summary>
/// Static guard clauses for argument validation at method entry points.
/// Throw immediately on invalid input — fail fast, fail loudly.
///
/// Usage:
///   Guard.NotNull(userId, nameof(userId));
///   Guard.NotEmpty(name, nameof(name));
///   Guard.InRange(pageSize, 1, 100, nameof(pageSize));
///
/// These replace the ValidationExtensions.EnsureNotNull() extension method
/// and the scattered ArgumentNullException throws throughout the codebase.
/// </summary>
public static class Guard
{
    /// <summary>Throws ArgumentNullException if value is null.</summary>
    public static T NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    /// <summary>Throws ArgumentNullException if value is null (struct/nullable).</summary>
    public static T NotNull<T>(T? value, string paramName) where T : struct
    {
        if (!value.HasValue)
            throw new ArgumentNullException(paramName);
        return value.Value;
    }

    /// <summary>Throws ArgumentException if string is null, empty, or whitespace.</summary>
    public static string NotEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        return value;
    }

    /// <summary>Throws ArgumentException if collection is null or has no elements.</summary>
    public static IEnumerable<T> NotEmpty<T>(IEnumerable<T>? value, string paramName)
    {
        if (value is null || !value.Any())
            throw new ArgumentException("Collection cannot be null or empty.", paramName);
        return value;
    }

    /// <summary>Throws ArgumentException if value equals the default for its type (0, null, Guid.Empty).</summary>
    public static T NotDefault<T>(T value, string paramName)
    {
        if (EqualityComparer<T>.Default.Equals(value, default!))
            throw new ArgumentException($"Value cannot be the default value for {typeof(T).Name}.", paramName);
        return value;
    }

    /// <summary>Throws ArgumentOutOfRangeException if value is outside [min, max].</summary>
    public static T InRange<T>(T value, T min, T max, string paramName) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}.");
        return value;
    }

    /// <summary>Throws ArgumentException if condition is false.</summary>
    public static void Requires(bool condition, string paramName, string message)
    {
        if (!condition)
            throw new ArgumentException(message, paramName);
    }
}
