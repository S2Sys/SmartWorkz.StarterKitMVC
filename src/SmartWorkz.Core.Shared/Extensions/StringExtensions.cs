namespace SmartWorkz.Shared;

public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);
    public static string SafeTrim(this string? value) => value?.Trim() ?? string.Empty;

    /// <summary>
    /// Converts a string to a URL-safe slug.
    /// "Hello World!" => "hello-world"
    /// </summary>
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return value.Trim().ToLowerInvariant()
            .Replace(' ', '-')
            .Replace("--", "-");
    }
}
