namespace SmartWorkz.Core.Web.Services.Components;

/// <summary>
/// Provides localized validation error messages with support for custom registration.
/// Includes 14 built-in validation messages for common validation scenarios.
/// </summary>
public class ValidationMessageProvider : IValidationMessageProvider
{
    private readonly Dictionary<string, string> _messages = new(StringComparer.OrdinalIgnoreCase)
    {
        { "required", "This field is required" },
        { "email", "Please enter a valid email address" },
        { "minlength", "This field must be at least {0} characters" },
        { "maxlength", "This field cannot exceed {0} characters" },
        { "pattern", "This field format is invalid" },
        { "min", "This field must be at least {0}" },
        { "max", "This field cannot exceed {0}" },
        { "unique", "This value is already in use" },
        { "invalid", "This field contains an invalid value" },
        { "match", "This field does not match {0}" },
        { "regex", "This field format is invalid" },
        { "url", "Please enter a valid URL" },
        { "number", "Please enter a valid number" },
        { "date", "Please enter a valid date" },
    };

    /// <summary>
    /// Get validation message for given error type.
    /// </summary>
    /// <param name="errorType">The type of validation error.</param>
    /// <returns>The validation message for the error type.</returns>
    public string GetMessage(string errorType)
    {
        return GetMessage(errorType, null);
    }

    /// <summary>
    /// Get validation message with property name included.
    /// </summary>
    /// <param name="errorType">The type of validation error.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <returns>The validation message with property name included.</returns>
    public string GetMessage(string errorType, string? propertyName)
    {
        if (!_messages.TryGetValue(errorType, out var message))
            return $"Validation error: {errorType}";

        if (string.IsNullOrEmpty(propertyName))
            return message;

        return $"{propertyName}: {message}";
    }

    /// <summary>
    /// Register custom validation message.
    /// </summary>
    /// <param name="errorType">The type of validation error to register.</param>
    /// <param name="message">The custom message for the error type.</param>
    public void RegisterMessage(string errorType, string message)
    {
        _messages[errorType] = message;
    }
}
