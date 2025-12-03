namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="object"/> and generic types.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Checks if the value equals the default value for its type.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is the default; otherwise false.</returns>
    /// <example>
    /// <code>
    /// int number = 0;
    /// bool isDefault = number.IsDefault(); // true
    /// 
    /// string? text = null;
    /// bool isNull = text.IsDefault(); // true
    /// </code>
    /// </example>
    public static bool IsDefault<T>(this T value) => EqualityComparer<T>.Default.Equals(value, default!);
}
