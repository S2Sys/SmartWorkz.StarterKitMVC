using System;

namespace SmartWorkz.Core.Shared.Security.Validation
{
    public interface IInputValidator
    {
        bool ContainsSqlInjectionPattern(string? input);
        bool ContainsXssPattern(string? input);
        bool ContainsCommandInjectionPattern(string? input);
        bool ContainsPathTraversalPattern(string? input);
        string SanitizeHtml(string? input);
        bool IsValidEmail(string? email);
        bool IsValidUrl(string? url);
    }
}
