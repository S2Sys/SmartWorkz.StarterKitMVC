namespace SmartWorkz.Core;

/// <summary>
/// Service for managing application culture and locale settings.
/// </summary>
/// <remarks>
/// Purpose: Manages culture and UI culture settings for the application, enabling
/// support for multiple languages and localization of content, formatting, and display.
///
/// Culture vs UI Culture:
/// - CurrentCulture: Controls formatting of numbers, dates, currencies. Example: "en-US", "de-DE".
/// - CurrentUICulture: Controls which language resources (strings, labels) are loaded.
///
/// Error Handling: Uses synchronous methods (no exceptions for invalid cultures by design).
/// Attempting to set an unsupported culture may silently fail or fall back to default.
/// Implementations should document their behavior for invalid cultures.
///
/// Typical Usage: Injected into ViewModels, Controllers, and UI components to determine
/// which language and formatting to use. Usually set based on user preferences or
/// browser language headers.
///
/// Thread Safety: Culture is typically thread-local (via CultureInfo.CurrentCulture).
/// Each request or user session may have different culture settings.
/// For ASP.NET Core, use middleware to set culture per-request from query string or cookie.
///
/// Supported Cultures: Should include common languages and regions (en-US, en-GB, de-DE,
/// fr-FR, es-ES, ja-JP, etc.). Consult SupportedCultures to discover available options.
/// </remarks>
public interface ICultureService : IService
{
    /// <summary>
    /// Gets the current culture setting for the application.
    /// </summary>
    /// <value>
    /// The culture code as a string (e.g., "en-US", "de-DE").
    /// Never null or empty.
    /// </value>
    /// <remarks>
    /// The current culture affects number formatting, date/time formatting, and currency symbols.
    /// Example values:
    /// - "en-US" (English - United States)
    /// - "en-GB" (English - Great Britain)
    /// - "de-DE" (German - Germany)
    /// - "fr-FR" (French - France)
    /// </remarks>
    string CurrentCulture { get; }

    /// <summary>
    /// Gets the current UI culture setting for the application.
    /// </summary>
    /// <value>
    /// The UI culture code as a string (e.g., "en-US", "de-DE").
    /// Never null or empty.
    /// </value>
    /// <remarks>
    /// The current UI culture determines which language is used for UI elements,
    /// resource strings, labels, and messages. This may differ from the data culture
    /// (numbers, dates) to support scenarios like German UI with US number formatting.
    /// </remarks>
    string CurrentUICulture { get; }

    /// <summary>
    /// Gets the list of cultures supported by the application.
    /// </summary>
    /// <value>
    /// An enumerable of supported culture codes (e.g., ["en-US", "de-DE", "fr-FR"]).
    /// The collection is not empty and is read-only from the caller's perspective.
    /// </value>
    /// <remarks>
    /// This list defines which cultures can be set via SetCulture and SetUICulture.
    /// Applications should query this to populate language selector dropdowns in UI.
    ///
    /// Typical contents:
    /// - en-US, en-GB (English variants)
    /// - de-DE, de-AT (German variants)
    /// - fr-FR, fr-CA (French variants)
    /// - es-ES, es-MX (Spanish variants)
    /// - ja-JP (Japanese)
    /// - zh-CN, zh-TW (Chinese variants)
    /// </remarks>
    IEnumerable<string> SupportedCultures { get; }

    /// <summary>
    /// Sets the current culture for number, date, and currency formatting.
    /// </summary>
    /// <param name="culture">The culture code to set (e.g., "en-US", "de-DE"). Must not be null or empty.</param>
    /// <remarks>
    /// Effect: Changes the culture used for formatting numbers (decimal separator),
    /// dates (date format), and currency (symbol, placement). Does not affect UI language.
    ///
    /// Validation: If the culture is not in SupportedCultures, the call may silently
    /// fail or fall back to the default culture. Consult implementation docs.
    ///
    /// Common Usage:
    /// - Set to user's locale when loading user preferences
    /// - Change based on query string parameter: ?culture=de-DE
    /// - Respect browser Accept-Language header in web applications
    ///
    /// Example Cultures:
    /// - "en-US": 1,234.56 (dot decimal)
    /// - "de-DE": 1.234,56 (comma decimal)
    /// - "fr-FR": 1 234,56 (space thousands, comma decimal)
    /// </remarks>
    /// <example>
    /// <code>
    /// // In a controller or service
    /// cultureService.SetCulture("de-DE");
    /// var decimalString = (1234.56).ToString(); // "1.234,56"
    ///
    /// // Based on user preference
    /// var userCulture = await GetUserCulturePreference(userId);
    /// cultureService.SetCulture(userCulture);
    /// </code>
    /// </example>
    void SetCulture(string culture);

    /// <summary>
    /// Sets the current UI culture for resource strings and UI language.
    /// </summary>
    /// <param name="culture">The culture code to set (e.g., "en-US", "de-DE"). Must not be null or empty.</param>
    /// <remarks>
    /// Effect: Changes the culture used to load UI resources, translations, and labels.
    /// Does not affect data formatting (numbers, dates, currency).
    ///
    /// Validation: If the culture is not in SupportedCultures, the call may silently
    /// fail or fall back to the default culture. Consult implementation docs.
    ///
    /// Localization Integration: Works with ILocalizationService to determine which
    /// resource strings are retrieved. Setting UI culture to "de-DE" will cause
    /// GetString("greeting") to return German translations.
    ///
    /// Common Scenarios:
    /// - Set to user's language preference on login
    /// - Change based on language selector button click
    /// - Respect browser Accept-Language header
    /// </remarks>
    /// <example>
    /// <code>
    /// // Change UI language to German
    /// cultureService.SetUICulture("de-DE");
    /// var greeting = localizationService.GetString("greeting");
    /// // Returns German version if available
    ///
    /// // Change UI language based on user selection
    /// public void ChangeLanguage(string languageCode)
    /// {
    ///     if (!cultureService.SupportedCultures.Contains(languageCode))
    ///         return; // Invalid culture
    ///
    ///     cultureService.SetUICulture(languageCode);
    ///     // UI components now display in new language
    /// }
    /// </code>
    /// </example>
    void SetUICulture(string culture);
}
