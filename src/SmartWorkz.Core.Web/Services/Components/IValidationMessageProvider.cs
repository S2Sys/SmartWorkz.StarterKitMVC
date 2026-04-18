namespace SmartWorkz.Core.Web.Services.Components;

/// <summary>
/// Provides localized validation error messages with support for custom registration.
/// </summary>
public interface IValidationMessageProvider
{
    /// <summary>
    /// Get validation message for given error type.
    /// </summary>
    /// <param name="errorType">The type of validation error.</param>
    /// <returns>The validation message for the error type.</returns>
    string GetMessage(string errorType);

    /// <summary>
    /// Get validation message with property name included.
    /// </summary>
    /// <param name="errorType">The type of validation error.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <returns>The validation message with property name included.</returns>
    string GetMessage(string errorType, string? propertyName);

    /// <summary>
    /// Register custom validation message.
    /// </summary>
    /// <param name="errorType">The type of validation error to register.</param>
    /// <param name="message">The custom message for the error type.</param>
    void RegisterMessage(string errorType, string message);
}
