namespace SmartWorkz.Core.Shared.Security;

using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Input sanitization to prevent XSS, SQL injection, and path traversal attacks.
/// </summary>
public static class InputSanitizer
{
    /// <summary>Sanitize HTML by removing dangerous tags and attributes.</summary>
    public static string SanitizeHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove script tags and content
        var sanitized = Regex.Replace(input, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Remove event handlers
        sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*[""']?[^""'>\s]*[""']?", string.Empty, RegexOptions.IgnoreCase);

        // Remove iframe, object, embed tags
        sanitized = Regex.Replace(sanitized, @"<(iframe|object|embed)[^>]*>.*?</\1>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Remove style tags
        sanitized = Regex.Replace(sanitized, @"<style[^>]*>.*?</style>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        return sanitized;
    }

    /// <summary>Escape HTML special characters to prevent XSS.</summary>
    public static string EscapeHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }

    /// <summary>Sanitize string to prevent SQL injection (basic, not a replacement for parameterized queries).</summary>
    public static string SanitizeSql(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input
            .Replace("'", "''")
            .Replace(";", "")
            .Replace("--", "")
            .Replace("/*", "")
            .Replace("*/", "")
            .Replace("xp_", "")
            .Replace("sp_", "");
    }

    /// <summary>Sanitize file path to prevent directory traversal attacks.</summary>
    public static string SanitizeFilePath(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove directory traversal patterns
        var sanitized = Regex.Replace(input, @"\.\.[\\/]", string.Empty);
        sanitized = Regex.Replace(sanitized, @"[\\/]\.\.", string.Empty);

        // Remove absolute path references
        if (System.IO.Path.IsPathRooted(sanitized))
            sanitized = System.IO.Path.GetFileName(sanitized);

        return sanitized;
    }

    /// <summary>Sanitize and validate URL.</summary>
    public static string SanitizeUrl(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        try
        {
            var uri = new Uri(input, UriKind.RelativeOrAbsolute);

            // Only allow http and https schemes for absolute URIs
            if (uri.IsAbsoluteUri && uri.Scheme != "http" && uri.Scheme != "https")
                return string.Empty;

            return input;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>Escape string for safe JSON inclusion.</summary>
    public static string EscapeJson(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var c in input)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (char.IsControl(c))
                        sb.Append($"\\u{(int)c:x4}");
                    else
                        sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    /// <summary>Validate email format (basic check, server-side SMTP validation recommended).</summary>
    public static bool IsValidEmail(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(input);
            return addr.Address == input;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Remove null bytes and control characters.</summary>
    public static string RemoveControlCharacters(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return new string(input.Where(c => !char.IsControl(c) && c != '\0').ToArray());
    }
}
