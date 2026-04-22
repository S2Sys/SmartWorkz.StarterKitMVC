namespace SmartWorkz.Core;

/// <summary>
/// Service for retrieving localized (translated) strings and resource text.
/// </summary>
/// <remarks>
/// Purpose: Provides access to application strings in the current culture/language,
/// enabling multi-language support throughout the application.
///
/// Error Handling: Returns default values (empty string or provided defaultValue) for
/// missing keys rather than throwing exceptions. This allows graceful degradation.
/// Unexpected failures (e.g., resource file I/O) may throw exceptions.
///
/// Async Behavior: Provides both synchronous (GetString) and asynchronous (GetStringAsync)
/// methods. Synchronous methods are preferred for UI rendering to avoid unnecessary async
/// overhead. Asynchronous methods are useful when loading resources on-demand or from
/// remote sources.
///
/// Resource Format: Typically uses JSON, XML, or database-backed resources organized
/// by culture (e.g., en-US, de-DE, fr-FR).
///
/// String Formatting: Supports string.Format-style placeholders {0}, {1}, etc.
/// for dynamic content injection. Example: "Hello {0}, welcome to {1}".
///
/// Culture Integration: Works with ICultureService.CurrentUICulture to determine
/// which language's resources to load.
///
/// Typical Usage: Injected into ViewModels, Controllers, and UI components to fetch
/// translated strings for display. Common keys: errors.validation.required,
/// messages.success.created, labels.firstName, etc.
/// </remarks>
public interface ILocalizationService : IService
{
    /// <summary>
    /// Retrieves a localized string by key in the current UI culture.
    /// </summary>
    /// <param name="key">The resource key (e.g., "errors.validation.required"). Must not be null or empty.</param>
    /// <param name="defaultValue">Optional default value to return if the key is not found.
    /// If null, an empty string is returned on key miss.</param>
    /// <returns>
    /// The localized string for the current UI culture.
    /// If the key is not found, returns defaultValue (or empty string if defaultValue is null).
    /// Never null.
    /// </returns>
    /// <remarks>
    /// Key Format: Use hierarchical, dot-separated keys for organization:
    /// - errors.validation.required = "This field is required"
    /// - messages.success.created = "Record created successfully"
    /// - labels.firstName = "First Name"
    /// - buttons.save = "Save"
    /// - buttons.cancel = "Cancel"
    ///
    /// Culture: Uses the current UI culture (from ICultureService.CurrentUICulture).
    /// To retrieve strings in a specific culture, change the UI culture first or use
    /// a culture-specific method if available.
    ///
    /// Missing Keys: Returns defaultValue if provided, otherwise empty string.
    /// This prevents null-reference exceptions and supports graceful degradation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var requiredFieldError = localizationService.GetString("errors.validation.required");
    /// // Returns "This field is required" in English, "Dieses Feld ist erforderlich" in German
    ///
    /// var customError = localizationService.GetString(
    ///     "errors.custom.unknownError",
    ///     "An unknown error occurred"
    /// );
    /// // If key exists, returns localized string. Otherwise, returns the default.
    /// </code>
    /// </example>
    string GetString(string key, string? defaultValue = null);

    /// <summary>
    /// Retrieves a localized string and formats it with the provided arguments.
    /// </summary>
    /// <param name="key">The resource key (e.g., "messages.greeting"). Must not be null or empty.</param>
    /// <param name="args">Format arguments to inject into the string using {0}, {1}, etc. placeholders.</param>
    /// <returns>
    /// The localized string formatted with the provided arguments.
    /// If the key is not found, returns an empty string.
    /// Never null.
    /// </returns>
    /// <remarks>
    /// Formatting: Uses standard string.Format-style placeholders: {0}, {1}, {2}, etc.
    ///
    /// String Format in Resources:
    /// - Key: "messages.greeting" = "Hello {0}, welcome to {1}"
    /// - Call: GetString("messages.greeting", "John", "MyApp")
    /// - Result: "Hello John, welcome to MyApp"
    ///
    /// Missing Keys: Returns empty string if the key is not found.
    ///
    /// Argument Validation: Ensure the number and type of arguments match the placeholders
    /// in the resource string. Mismatched arguments may result in incomplete formatting
    /// or exceptions (depending on implementation).
    /// </remarks>
    /// <example>
    /// <code>
    /// // In resource: "emails.welcome" = "Welcome {0}, your account was created on {1}"
    /// var welcomeMessage = localizationService.GetString(
    ///     "emails.welcome",
    ///     userDto.Name,
    ///     DateTime.Now.ToString("yyyy-MM-dd")
    /// );
    /// // Returns: "Welcome John Doe, your account was created on 2025-02-20"
    ///
    /// // Error message with format
    /// var errorMsg = localizationService.GetString(
    ///     "errors.itemNotFound",
    ///     "Product",
    ///     productId
    /// );
    /// // In resource: "errors.itemNotFound" = "{0} with ID {1} not found"
    /// // Returns: "Product with ID 123 not found"
    /// </code>
    /// </example>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Retrieves a collection of localized strings matching a key prefix.
    /// </summary>
    /// <param name="prefix">The key prefix to match (e.g., "buttons", "errors.validation").
    /// Must not be null or empty.</param>
    /// <returns>
    /// A dictionary of all keys and values where the key starts with the prefix.
    /// If no keys match, returns an empty dictionary. Never null.
    /// </returns>
    /// <remarks>
    /// Use Case: Useful for bulk loading related strings (e.g., all button labels,
    /// all validation error messages) to avoid multiple individual GetString calls.
    ///
    /// Prefix Format: Use hierarchical keys to organize resources:
    /// - "buttons" matches buttons.save, buttons.cancel, buttons.delete, etc.
    /// - "errors.validation" matches errors.validation.required,
    ///   errors.validation.email, errors.validation.minLength, etc.
    ///
    /// Performance: May trigger batch resource loading. Cache results if called frequently.
    ///
    /// Typical Usage: UI dropdown population, error message mapping, button label assignment.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Load all button labels at once
    /// var buttons = localizationService.GetStrings("buttons");
    /// // Result: {
    /// //   "buttons.save": "Save",
    /// //   "buttons.cancel": "Cancel",
    /// //   "buttons.delete": "Delete",
    /// //   ...
    /// // }
    /// foreach (var kvp in buttons)
    /// {
    ///     Console.WriteLine($"Button: {kvp.Value}");
    /// }
    ///
    /// // Load all validation error messages
    /// var validationErrors = localizationService.GetStrings("errors.validation");
    /// // Result: {
    /// //   "errors.validation.required": "This field is required",
    /// //   "errors.validation.email": "Must be a valid email",
    /// //   "errors.validation.minLength": "Minimum length is {0}",
    /// //   ...
    /// // }
    /// </code>
    /// </example>
    Dictionary<string, string> GetStrings(string prefix);

    /// <summary>
    /// Asynchronously retrieves a localized string by key in the current UI culture.
    /// </summary>
    /// <param name="key">The resource key (e.g., "messages.success.created"). Must not be null or empty.</param>
    /// <param name="defaultValue">Optional default value to return if the key is not found.
    /// If null, an empty string is returned on key miss.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// A task that resolves to the localized string for the current UI culture.
    /// If the key is not found, returns defaultValue (or empty string if defaultValue is null).
    /// Never null.
    /// </returns>
    /// <remarks>
    /// Async Alternative: Use when resources are loaded from remote sources (database,
    /// API) or need time-consuming I/O. For in-memory resources, prefer synchronous
    /// GetString to avoid async overhead.
    ///
    /// Cancellation: Respects CancellationToken for graceful cancellation of long-running
    /// resource loads.
    ///
    /// Cache Strategy: Implementations should cache frequently accessed strings to minimize
    /// repeated I/O.
    ///
    /// Missing Keys: Returns defaultValue if provided, otherwise empty string.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Load localized email subject (from database or remote resource)
    /// var emailSubject = await localizationService.GetStringAsync(
    ///     "emails.resetPassword.subject",
    ///     "Password Reset Request"
    /// );
    ///
    /// // Load with cancellation
    /// var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    /// try
    /// {
    ///     var message = await localizationService.GetStringAsync(
    ///         "messages.notification",
    ///         cancellationToken: cts.Token
    ///     );
    /// }
    /// catch (OperationCanceledException)
    /// {
    ///     logger.LogWarning("Resource load was cancelled");
    /// }
    /// </code>
    /// </example>
    Task<string> GetStringAsync(string key, string? defaultValue = null, CancellationToken cancellationToken = default);
}
