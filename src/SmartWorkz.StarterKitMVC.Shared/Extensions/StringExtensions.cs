namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/> manipulation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Checks if the string is null, empty, or consists only of whitespace.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if null, empty, or whitespace; otherwise false.</returns>
    /// <example>
    /// <code>
    /// string? name = null;
    /// if (name.IsNullOrWhiteSpace())
    ///     Console.WriteLine("Name is empty!");
    /// </code>
    /// </example>
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Trims the string safely, returning an empty string if the input is null.
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <returns>Trimmed string or empty string if null.</returns>
    /// <example>
    /// <code>
    /// string? input = "  hello  ";
    /// var trimmed = input.SafeTrim(); // Returns "hello"
    /// 
    /// string? nullInput = null;
    /// var safe = nullInput.SafeTrim(); // Returns ""
    /// </code>
    /// </example>
    public static string SafeTrim(this string? value) => value?.Trim() ?? string.Empty;
}
