namespace SmartWorkz.Core.Shared.Helpers;

public static class StringHelper
{
    public static string ToCamelCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        return char.ToLowerInvariant(input[0]) + input.Substring(1);
    }

    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        return char.ToUpperInvariant(input[0]) + input.Substring(1);
    }

    public static string ToKebabCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        var pattern = new System.Text.RegularExpressions.Regex(@"[A-Z]");
        return pattern.Replace(input, m => "-" + m.Value.ToLowerInvariant()).Trim('-');
    }

    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        var pattern = new System.Text.RegularExpressions.Regex(@"[A-Z]");
        return pattern.Replace(input, m => "_" + m.Value.ToLowerInvariant()).Trim('_');
    }

    public static string Truncate(string input, int length, string suffix = "...")
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length <= length) return input;
        return input.Substring(0, length - suffix.Length) + suffix;
    }

    public static bool ContainsAll(string input, params string[] values)
    {
        return values.All(v => input.Contains(v, StringComparison.OrdinalIgnoreCase));
    }

    public static bool ContainsAny(string input, params string[] values)
    {
        return values.Any(v => input.Contains(v, StringComparison.OrdinalIgnoreCase));
    }

    public static string RemoveDiacritics(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        var normalized = input.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }
}
