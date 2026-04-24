using System;
using System.Text.RegularExpressions;

namespace SmartWorkz.Core.Shared.Security.Validation
{
    public class InputValidator : IInputValidator
    {
        private static class Patterns
        {
            public static readonly Regex SqlInjection = new(
                @"(\b(UNION|SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|SCRIPT|JAVASCRIPT|EVAL)\b)|('|;|--|#|/\*|\*/|xp_|sp_)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            public static readonly Regex Xss = new(
                @"(<script[^>]*>.*?</script>|javascript:|on\w+\s*=|<iframe|<object|<embed|<img[^>]*onerror|<svg[^>]*onload)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            public static readonly Regex CommandInjection = new(
                @"(;|\||&|`|\$\(|&&|\|\|)",
                RegexOptions.Compiled);

            public static readonly Regex PathTraversal = new(
                @"(\.\.[\\/]|\.\.%2[fF])",
                RegexOptions.Compiled);

            public static readonly Regex Email = new(
                @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
                RegexOptions.Compiled);

            public static readonly Regex Url = new(
                @"^https?://[^\s]+$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public bool ContainsSqlInjectionPattern(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return Patterns.SqlInjection.IsMatch(input);
        }

        public bool ContainsXssPattern(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return Patterns.Xss.IsMatch(input);
        }

        public bool ContainsCommandInjectionPattern(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return Patterns.CommandInjection.IsMatch(input);
        }

        public bool ContainsPathTraversalPattern(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return Patterns.PathTraversal.IsMatch(input);
        }

        public string SanitizeHtml(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return input ?? string.Empty;

            var sanitized = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*[""']?[^""'\s>]*[""']?", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase);

            return sanitized;
        }

        public bool IsValidEmail(string? email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            return Patterns.Email.IsMatch(email) && !ContainsSqlInjectionPattern(email);
        }

        public bool IsValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            return Patterns.Url.IsMatch(url) && !ContainsSqlInjectionPattern(url);
        }
    }
}
