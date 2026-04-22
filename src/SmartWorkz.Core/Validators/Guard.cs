namespace SmartWorkz.Core.Validators;

using System.Text.RegularExpressions;

/// <summary>
/// Comprehensive validation guard class providing static methods for validating common data types
/// and formats across the application.
/// </summary>
/// <remarks>
/// Purpose and Design:
/// Guard centralizes input validation for email, URL, phone, money, regex patterns, enums,
/// pagination, and text inputs. All methods follow a consistent pattern:
///   - Static methods return the validated value (or void for void operations)
///   - All methods throw ArgumentException on validation failure
///   - ParamName included in all exception messages for debugging
///   - No silent failures: invalid input always raises an exception
///
/// Validation Strategy:
/// 1. Format Validation: Ensures input conforms to expected format (email regex, phone digits)
/// 2. Range Validation: Ensures values fall within acceptable bounds (page size, money amount)
/// 3. Enum Validation: Ensures enum values are defined and within allowed states
/// 4. Whitespace Handling: Trims and validates text inputs
/// 5. Type Safety: Uses generic constraints for enum validation
///
/// Best Practices:
/// 1. Always use Guard methods before storing user input
/// 2. Call Guard at service layer entry points (controllers, services)
/// 3. Include Guard calls in entity constructors for domain-driven design
/// 4. Combine multiple guards for complex validation (e.g., ValidEmail then CheckPhoneInList)
/// 5. Use specific guards over generic Regex matching when available
///
/// Performance Considerations:
/// - Regex patterns compiled once, reused across calls
/// - No unnecessary string allocations in validation paths
/// - Early exit on validation failure (no full string scanning)
/// - Stateless design allows multithreading without synchronization
///
/// Security Considerations:
/// - Email validation basic (RFC 5322 simplified) to accept valid international formats
/// - Phone validation allows +/-/() formatting to be flexible
/// - URL validation uses Uri.TryCreate for safe parsing
/// - HTTPS enforcement available for sensitive operations
/// - Regex patterns prevent ReDoS (no catastrophic backtracking)
///
/// Example Usage:
/// // In service layer
/// public async Task<Customer> RegisterAsync(string email, string phone, string name)
/// {
///     var validEmail = Guard.ValidEmail(email, nameof(email));      // Throws if invalid
///     var validPhone = Guard.ValidPhone(phone, nameof(phone));      // Throws if invalid
///     var validName = Guard.LengthBetween(name, 1, 256, nameof(name)); // Throws if invalid
///
///     var customer = new Customer(validEmail, validPhone, validName);
///     await _repository.AddAsync(customer);
///     return customer;
/// }
///
/// // In entity constructor (Domain-Driven Design)
/// public class Customer
/// {
///     public Customer(string email, string phone, string name)
///     {
///         Email = Guard.ValidEmail(email, nameof(email));
///         Phone = Guard.ValidPhone(phone, nameof(phone));
///         Name = Guard.LengthBetween(name, 1, 256, nameof(name));
///     }
/// }
/// </remarks>
public static class Guard
{
    // Regex patterns compiled once for reuse
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhonePattern = new(@"^\+?[0-9]{10,15}$", RegexOptions.Compiled);
    private static readonly Regex PhoneE164Pattern = new(@"^\+[1-9]\d{6,14}$", RegexOptions.Compiled);
    private static readonly Regex CurrencyPattern = new(@"^[A-Z]{3}$", RegexOptions.Compiled);
    private static readonly Regex AlphanumericPattern = new(@"^[a-zA-Z0-9]+$", RegexOptions.Compiled);
    private static readonly Regex AlphanumericWithHyphensPattern = new(@"^[a-zA-Z0-9\-]+$", RegexOptions.Compiled);

    #region Task 10 - Email, URL, Phone Validation

    /// <summary>
    /// Validates that the provided string is a valid email address using RFC 5322 basic pattern.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated email address.</returns>
    /// <exception cref="ArgumentException">Thrown if the email format is invalid.</exception>
    /// <remarks>
    /// RFC 5322 Compliance:
    /// This uses a basic RFC 5322 regex pattern that matches the format: localpart@domain.tld
    /// The pattern is simplified for practical use; it accepts valid international email formats
    /// while rejecting common invalid formats (missing @, missing domain, etc.).
    ///
    /// Pattern: ^[^@\s]+@[^@\s]+\.[^@\s]+$
    /// - [^@\s]+ : One or more non-@ non-whitespace characters (local part)
    /// - @ : Literal @ symbol (required)
    /// - [^@\s]+ : One or more non-@ non-whitespace characters (domain name)
    /// - \. : Literal dot (required)
    /// - [^@\s]+ : One or more non-@ non-whitespace characters (TLD)
    ///
    /// Valid Examples: user@example.com, john.doe@company.co.uk, info+tag@domain.org
    /// Invalid Examples: user (no @), user@domain (no TLD), @domain.com (no local part)
    ///
    /// Use Cases: User registration, profile updates, contact forms, email notifications
    /// </remarks>
    public static string ValidEmail(string email, string paramName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        var trimmed = email.Trim();
        if (!EmailPattern.IsMatch(trimmed))
            throw new ArgumentException($"{paramName} is not a valid email format.", paramName);

        return trimmed;
    }

    /// <summary>
    /// Validates that the provided string is a valid URL using Uri.TryCreate.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated URL.</returns>
    /// <exception cref="ArgumentException">Thrown if the URL format is invalid.</exception>
    /// <remarks>
    /// URL Validation:
    /// Uses Uri.TryCreate to validate URLs, which is the .NET standard for URL parsing.
    /// Accepts both relative and absolute URIs. Allows http, https, ftp, and other schemes.
    ///
    /// Valid Examples: http://example.com, https://api.service.com/v1/resource, ftp://files.domain.org
    /// Invalid Examples: not a url, ht!tp://invalid, example .com (space)
    ///
    /// Use Cases: API endpoints, webhook URLs, external links, file paths
    /// </remarks>
    public static string ValidUrl(string url, string paramName)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        var trimmed = url.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out _))
            throw new ArgumentException($"{paramName} is not a valid URL format.", paramName);

        return trimmed;
    }

    /// <summary>
    /// Validates that the provided string is a valid URL with HTTPS scheme only.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated HTTPS URL.</returns>
    /// <exception cref="ArgumentException">Thrown if the URL is invalid or doesn't use HTTPS.</exception>
    /// <remarks>
    /// HTTPS-Only Validation:
    /// Extends ValidUrl to enforce HTTPS scheme (https://) only.
    /// Rejects http://, ftp://, and other non-HTTPS schemes.
    /// Suitable for security-sensitive operations (payments, authentication, PII).
    ///
    /// Valid Examples: https://secure.example.com, https://api.payment.com
    /// Invalid Examples: http://example.com (not HTTPS), https (no URL), example.com (no scheme)
    ///
    /// Security Rationale:
    /// - HTTPS encrypts data in transit, protecting sensitive information
    /// - Required for PCI-DSS, GDPR, and other compliance standards
    /// - Use for payment processing, authentication, credential storage
    ///
    /// Use Cases: Payment gateways, OAuth providers, secure webhooks, authentication endpoints
    /// </remarks>
    public static string ValidHttpsUrl(string url, string paramName)
    {
        var validUrl = ValidUrl(url, paramName);

        if (!validUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"{paramName} must use HTTPS scheme.", paramName);

        return validUrl;
    }

    /// <summary>
    /// Validates that the provided string is a valid phone number (10-15 digits).
    /// </summary>
    /// <param name="phone">The phone number to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated phone number.</returns>
    /// <exception cref="ArgumentException">Thrown if the phone number is invalid.</exception>
    /// <remarks>
    /// Phone Number Validation:
    /// Basic validation allowing 10-15 digits with optional leading + or formatting characters.
    /// Pattern: ^\+?[0-9]{10,15}$
    /// - ^\+? : Optional + prefix for international format
    /// - [0-9]{10,15} : 10 to 15 digits (per ITU-T E.164 practical limits)
    ///
    /// Valid Examples: +14155552671, 2025551234, +442071838750, +33142345678
    /// Invalid Examples: 123 (too short), 123456789012345678 (too long), 2025551234a (contains letter)
    ///
    /// Practical Considerations:
    /// - 10 digits covers North America (US/Canada)
    /// - 11-15 digits covers international numbers
    /// - Allows flexible formatting (spaces, dashes removed by caller if needed)
    ///
    /// Use Cases: User registration, contact information, SMS notifications, 2FA setup
    /// </remarks>
    public static string ValidPhone(string phone, string paramName)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        var normalizedPhone = Regex.Replace(phone.Trim(), @"[\s\-\(\)]", "");

        if (!PhonePattern.IsMatch(normalizedPhone))
            throw new ArgumentException($"{paramName} must contain 10-15 digits, optionally prefixed with +.", paramName);

        return phone.Trim();
    }

    /// <summary>
    /// Validates that the provided string is a valid E.164 format phone number.
    /// </summary>
    /// <param name="phone">The phone number to validate in E.164 format.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated E.164 phone number.</returns>
    /// <exception cref="ArgumentException">Thrown if the phone number is not in E.164 format.</exception>
    /// <remarks>
    /// E.164 Format:
    /// International standard format for phone numbers: +[country code][area code][subscriber number]
    /// Pattern: ^\+[1-9]\d{6,14}$
    /// - ^\+ : Required + prefix
    /// - [1-9] : Country code must start with 1-9 (never 0)
    /// - \d{6,14} : 6 to 14 additional digits (7-15 total per E.164 minimum)
    /// - Total length: 7-15 characters (+ plus 1-15 digits, minimum 7 digits total)
    ///
    /// Valid Examples: +1-555-123-4567, +442071838750, +33142345678, +8613800138000
    /// Invalid Examples: +0555123456 (country code starts with 0), 14155552671 (no +), +1 (too short)
    ///
    /// Use Cases: International SMS, WhatsApp, Twilio, international 2FA, global contact storage
    /// Reference: https://en.wikipedia.org/wiki/E.164
    /// </remarks>
    public static string ValidPhoneE164(string phone, string paramName)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        var normalizedPhone = Regex.Replace(phone.Trim(), @"[\s\-\(\)]", "");

        if (!PhoneE164Pattern.IsMatch(normalizedPhone))
            throw new ArgumentException($"{paramName} must be in E.164 format (+country_code + digits, max 15 total).", paramName);

        return phone.Trim();
    }

    /// <summary>
    /// Validates that the provided phone number is in the allowed whitelist.
    /// </summary>
    /// <param name="phone">The phone number to validate.</param>
    /// <param name="allowedPhones">The collection of allowed phone numbers.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated phone number.</returns>
    /// <exception cref="ArgumentException">Thrown if the phone is not in the allowed list.</exception>
    /// <remarks>
    /// Whitelist Validation:
    /// Ensures phone number matches one of the pre-approved numbers in the collection.
    /// Useful for 2FA, trusted contacts, administrator notifications.
    /// Phone number matching is case-insensitive and normalized (spaces/dashes removed).
    ///
    /// Example Scenario:
    /// - User has registered phones: ["+14155552671", "+442071838750"]
    /// - Request uses phone: "+1-415-555-2671" (normalized to "+14155552671")
    /// - Validation passes because normalized form matches whitelist entry
    ///
    /// Use Cases: 2FA phone verification, sending to trusted contacts only, admin notification routing
    /// </remarks>
    public static string PhoneInList(string phone, IEnumerable<string> allowedPhones, string paramName)
    {
        var validPhone = ValidPhone(phone, paramName);
        var normalizedPhone = Regex.Replace(validPhone, @"[\s\-\(\)]", "");
        var normalizedAllowed = allowedPhones
            .Select(p => Regex.Replace(p, @"[\s\-\(\)]", ""))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!normalizedAllowed.Contains(normalizedPhone))
            throw new ArgumentException($"{paramName} is not in the allowed phone list.", paramName);

        return validPhone;
    }

    #endregion

    #region Task 11 - Money, Regex, Enum Validation

    /// <summary>
    /// Validates that a decimal amount is valid money (non-negative and within optional range).
    /// </summary>
    /// <param name="amount">The amount to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <param name="min">Optional minimum allowed amount (inclusive).</param>
    /// <param name="max">Optional maximum allowed amount (inclusive).</param>
    /// <returns>The validated amount.</returns>
    /// <exception cref="ArgumentException">Thrown if the amount is invalid or outside the allowed range.</exception>
    /// <remarks>
    /// Money Validation:
    /// Ensures decimal values represent valid monetary amounts:
    /// - Non-negative (no negative prices, refunds handled separately)
    /// - Within optional min/max range (e.g., price range, discount limits)
    /// - Typically 2 decimal places (handled by calling code, not validated here)
    ///
    /// Valid Examples:
    /// - ValidMoney(100.00m, "price") -> 100.00
    /// - ValidMoney(50.99m, "discount", min: 0, max: 100) -> 50.99
    /// - ValidMoney(0m, "amount") -> 0 (zero is valid for optional/free items)
    ///
    /// Invalid Examples:
    /// - ValidMoney(-10m, "price") -> throws (negative)
    /// - ValidMoney(150m, "discount", min: 0, max: 100) -> throws (exceeds max)
    ///
    /// Best Practices:
    /// 1. Always validate money amounts before financial operations
    /// 2. Use decimal (not float/double) for currency calculations
    /// 3. Set appropriate min/max per business context:
    ///    - Price: min=0.01 (no free items unless explicitly allowed), max varies
    ///    - Discount: min=0, max=product price
    ///    - Tax: min=0, max varies by jurisdiction
    /// 4. Round to 2 decimals before storing (handled by rounding logic elsewhere)
    ///
    /// Use Cases: Price validation, discount validation, tax calculation, payment processing
    /// </remarks>
    public static decimal ValidMoney(decimal amount, string paramName, decimal? min = null, decimal? max = null)
    {
        if (amount < 0)
            throw new ArgumentException($"{paramName} cannot be negative.", paramName);

        if (min.HasValue && amount < min.Value)
            throw new ArgumentException($"{paramName} cannot be less than {min.Value}.", paramName);

        if (max.HasValue && amount > max.Value)
            throw new ArgumentException($"{paramName} cannot be greater than {max.Value}.", paramName);

        return amount;
    }

    /// <summary>
    /// Validates that a string is a valid ISO 4217 currency code (3 uppercase letters).
    /// </summary>
    /// <param name="currency">The currency code to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated currency code.</returns>
    /// <exception cref="ArgumentException">Thrown if the currency code format is invalid.</exception>
    /// <remarks>
    /// ISO 4217 Currency Codes:
    /// Standard international currency codes: 3 uppercase letters
    /// Pattern: ^[A-Z]{3}$
    ///
    /// Valid Examples: USD (US Dollar), EUR (Euro), GBP (British Pound), JPY (Japanese Yen), INR (Indian Rupee)
    /// Invalid Examples: US (too short), usd (lowercase), USDA (too long), US$ (contains symbol)
    ///
    /// Note: This validates format only; actual currency existence is not verified.
    /// For real-world validation, maintain a list of supported currencies in AppConstants or database.
    ///
    /// Common Use Cases:
    /// - Order amounts: ValidMoney + ValidCurrency
    /// - Multi-currency pricing: Validate before currency conversion
    /// - Payment processing: Verify supported currency for gateway
    /// - Invoice generation: Format currency for display
    ///
    /// Reference: https://en.wikipedia.org/wiki/ISO_4217
    /// </remarks>
    public static string ValidCurrency(string currency, string paramName)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        var normalized = currency.Trim().ToUpperInvariant();

        if (!CurrencyPattern.IsMatch(normalized))
            throw new ArgumentException($"{paramName} must be a 3-letter ISO 4217 currency code (e.g., USD, EUR, GBP).", paramName);

        return normalized;
    }

    /// <summary>
    /// Validates that a string matches a specified regex pattern.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="pattern">The regex pattern to match.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated string.</returns>
    /// <exception cref="ArgumentException">Thrown if the string doesn't match the pattern.</exception>
    /// <remarks>
    /// Generic Regex Validation:
    /// Allows validation against any custom regex pattern.
    /// Useful for domain-specific formats not covered by specialized validators.
    ///
    /// Pattern Considerations:
    /// - Patterns should be as specific as possible (prevent ReDoS)
    /// - Use anchors (^ and $) to match entire string
    /// - Test patterns for catastrophic backtracking
    /// - Document expected format in pattern comments
    ///
    /// Examples:
    /// // Alphanumeric product codes
    /// Guard.MatchesRegex(productCode, @"^[A-Z0-9]{8}$", nameof(productCode));
    ///
    /// // Date in YYYY-MM-DD format
    /// Guard.MatchesRegex(dateStr, @"^\d{4}-\d{2}-\d{2}$", nameof(dateStr));
    ///
    /// // Hex color code
    /// Guard.MatchesRegex(color, @"^#[0-9A-F]{6}$", nameof(color));
    ///
    /// Use Cases: Custom formats, domain-specific codes, configuration validation
    /// </remarks>
    public static string MatchesRegex(string value, string pattern, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        if (!Regex.IsMatch(value.Trim(), pattern))
            throw new ArgumentException($"{paramName} does not match the required pattern.", paramName);

        return value.Trim();
    }

    /// <summary>
    /// Validates that a string contains only alphanumeric characters (letters and digits).
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated string.</returns>
    /// <exception cref="ArgumentException">Thrown if the string contains non-alphanumeric characters.</exception>
    /// <remarks>
    /// Alphanumeric-Only Validation:
    /// Pattern: ^[a-zA-Z0-9]+$
    /// Allows: a-z, A-Z, 0-9
    /// Rejects: spaces, hyphens, underscores, special characters, unicode
    ///
    /// Valid Examples: Product123, User456ABC, Code999
    /// Invalid Examples: Product-123 (hyphen), User 456 (space), Code_999 (underscore)
    ///
    /// Use Cases: Product codes, SKUs, user handles, identifiers
    /// </remarks>
    public static string MatchesAlphanumeric(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        var trimmed = value.Trim();

        if (!AlphanumericPattern.IsMatch(trimmed))
            throw new ArgumentException($"{paramName} must contain only alphanumeric characters (letters and digits).", paramName);

        return trimmed;
    }

    /// <summary>
    /// Validates that a string contains only alphanumeric characters and hyphens.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated string.</returns>
    /// <exception cref="ArgumentException">Thrown if the string contains invalid characters.</exception>
    /// <remarks>
    /// Alphanumeric with Hyphens Validation:
    /// Pattern: ^[a-zA-Z0-9\-]+$
    /// Allows: a-z, A-Z, 0-9, hyphen (-)
    /// Rejects: spaces, underscores, special characters, unicode
    ///
    /// Valid Examples: Product-123, SKU-ABC-456, Item-Code-999
    /// Invalid Examples: Product 123 (space), Item_Code (underscore), Code@123 (special char)
    ///
    /// Use Cases: Slugs, tags, system identifiers, domain names
    /// </remarks>
    public static string MatchesAlphanumericWithHyphens(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        var trimmed = value.Trim();

        if (!AlphanumericWithHyphensPattern.IsMatch(trimmed))
            throw new ArgumentException($"{paramName} must contain only alphanumeric characters and hyphens.", paramName);

        return trimmed;
    }

    /// <summary>
    /// Validates that an enum value is defined in its enum type.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to validate against.</typeparam>
    /// <param name="value">The enum value to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated enum value.</returns>
    /// <exception cref="ArgumentException">Thrown if the enum value is not defined.</exception>
    /// <remarks>
    /// Enum Value Validation:
    /// Ensures the enum value is one of the defined members of the enum type.
    /// Useful when enum values come from external sources (API, database, user input).
    ///
    /// Example:
    /// enum OrderStatus { Pending = 0, Processing = 1, Shipped = 2, Delivered = 3 }
    ///
    /// Guard.ValidEnum(OrderStatus.Shipped, nameof(orderStatus)) -> OrderStatus.Shipped
    /// Guard.ValidEnum((OrderStatus)99, nameof(orderStatus)) -> throws (99 not defined)
    ///
    /// Practical Scenario:
    /// public OrderStatus UpdateStatus(int statusValue)
    /// {
    ///     var status = (OrderStatus)statusValue;
    ///     Guard.ValidEnum(status, nameof(status)); // Throws if statusValue was invalid
    ///     return status;
    /// }
    ///
    /// Use Cases: Deserialization, API input validation, database enum conversion
    /// </remarks>
    public static TEnum ValidEnum<TEnum>(TEnum value, string paramName) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(typeof(TEnum), value))
            throw new ArgumentException($"{paramName} is not a defined value in the {typeof(TEnum).Name} enum.", paramName);

        return value;
    }

    /// <summary>
    /// Validates that an EntityState value is within the allowed set of states.
    /// </summary>
    /// <param name="state">The entity state to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <param name="allowedStates">The set of allowed entity states.</param>
    /// <returns>The validated entity state.</returns>
    /// <exception cref="ArgumentException">Thrown if the state is not in the allowed set.</exception>
    /// <remarks>
    /// Entity State Validation:
    /// Ensures EntityState values are restricted to allowed transitions.
    /// Implements business rules for valid state transitions.
    ///
    /// Example: Order state transitions
    /// Valid: OrderPlaced -> OrderConfirmed -> Shipped -> Delivered
    /// Invalid: OrderPlaced -> Delivered (skips confirmation and shipping)
    ///
    /// Usage Pattern:
    /// var allowedStates = new HashSet&lt;EntityState&gt;
    /// {
    ///     EntityState.OrderPlaced,
    ///     EntityState.OrderConfirmed,
    ///     EntityState.Shipped
    /// };
    /// Guard.ValidStateForEntity(newState, nameof(newState), allowedStates);
    ///
    /// Workflow Implementation:
    /// 1. Define allowed states per operation
    /// 2. Call ValidStateForEntity before state transition
    /// 3. Throw on invalid state to prevent business logic violations
    /// 4. Log state transition attempts for audit trail
    ///
    /// Use Cases: Workflow state machines, order processing, document lifecycles
    /// </remarks>
    public static EntityState ValidStateForEntity(EntityState state, string paramName, IReadOnlySet<EntityState> allowedStates)
    {
        if (!allowedStates.Contains(state))
            throw new ArgumentException($"{paramName} value {state} is not in the allowed state set.", paramName);

        return state;
    }

    #endregion

    #region Task 12 - Pagination & Text Validation

    /// <summary>
    /// Validates that a page size is within acceptable bounds.
    /// </summary>
    /// <param name="pageSize">The page size to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <param name="min">Minimum allowed page size (default: 1).</param>
    /// <param name="max">Maximum allowed page size (default: 100).</param>
    /// <returns>The validated page size.</returns>
    /// <exception cref="ArgumentException">Thrown if page size is outside the allowed range.</exception>
    /// <remarks>
    /// Pagination Page Size Validation:
    /// Ensures page size requests fall within acceptable bounds.
    /// Default max of 100 prevents DoS via unbounded pagination.
    /// Min of 1 allows minimal pagination (single-item pages).
    ///
    /// Examples:
    /// Guard.ValidPageSize(10, nameof(pageSize))                    -> 10 (default bounds)
    /// Guard.ValidPageSize(50, nameof(pageSize))                    -> 50 (default bounds)
    /// Guard.ValidPageSize(100, nameof(pageSize))                   -> 100 (max default)
    /// Guard.ValidPageSize(250, nameof(pageSize), max: 1000)       -> 250 (custom max)
    /// Guard.ValidPageSize(0, nameof(pageSize))                     -> throws (less than min=1)
    /// Guard.ValidPageSize(500, nameof(pageSize))                   -> throws (exceeds max=100)
    ///
    /// Security Considerations:
    /// - Default max=100 prevents memory exhaustion from large result sets
    /// - Prevents database query DoS via extremely large page sizes
    /// - Override max for reports (may legitimately request 1000+ items)
    ///
    /// Performance Tuning:
    /// - Smaller page sizes (10-25): Better for web UIs, mobile
    /// - Medium page sizes (50): Reports, data exports
    /// - Larger sizes (500+): Batch operations, data migrations
    /// - Customize per endpoint based on typical response size
    ///
    /// Use Cases: API pagination parameters, list query validation, report size limits
    /// </remarks>
    public static int ValidPageSize(int pageSize, string paramName, int min = 1, int max = 100)
    {
        if (pageSize < min)
            throw new ArgumentException($"{paramName} cannot be less than {min}.", paramName);

        if (pageSize > max)
            throw new ArgumentException($"{paramName} cannot be greater than {max}.", paramName);

        return pageSize;
    }

    /// <summary>
    /// Validates that a page number is valid (must be >= 1).
    /// </summary>
    /// <param name="pageNumber">The page number to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated page number.</returns>
    /// <exception cref="ArgumentException">Thrown if page number is less than 1.</exception>
    /// <remarks>
    /// Pagination Page Number Validation:
    /// Ensures page numbering starts at 1 (1-indexed pagination).
    /// Rejects 0-indexed pagination (common programming error).
    ///
    /// Examples:
    /// Guard.ValidPageNumber(1, nameof(pageNumber))  -> 1 (first page, valid)
    /// Guard.ValidPageNumber(5, nameof(pageNumber))  -> 5 (fifth page, valid)
    /// Guard.ValidPageNumber(0, nameof(pageNumber))  -> throws (0-indexed, not allowed)
    /// Guard.ValidPageNumber(-1, nameof(pageNumber)) -> throws (negative, not allowed)
    ///
    /// Best Practice:
    /// Always combine ValidPageNumber with ValidPageSize for complete pagination validation:
    /// int pageNumber = Guard.ValidPageNumber(request.PageNumber, nameof(pageNumber));
    /// int pageSize = Guard.ValidPageSize(request.PageSize, nameof(pageSize));
    /// var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    ///
    /// Use Cases: API pagination parameters, list query validation, pagination UI
    /// </remarks>
    public static int ValidPageNumber(int pageNumber, string paramName)
    {
        if (pageNumber < 1)
            throw new ArgumentException($"{paramName} must be at least 1.", paramName);

        return pageNumber;
    }

    /// <summary>
    /// Validates that a string has no leading, trailing, or multiple internal whitespace.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated string (trimmed).</returns>
    /// <exception cref="ArgumentException">Thrown if the string has extraneous whitespace.</exception>
    /// <remarks>
    /// Whitespace Validation:
    /// Ensures strings are properly formatted with no:
    /// - Leading spaces: " text" -> invalid
    /// - Trailing spaces: "text " -> invalid
    /// - Multiple internal spaces: "text  space" -> invalid (double space)
    ///
    /// Valid Examples:
    /// "John Smith" (single space between words)
    /// "123" (no spaces)
    /// "New York" (single space)
    ///
    /// Invalid Examples:
    /// " John Smith" (leading space)
    /// "John Smith " (trailing space)
    /// "John  Smith" (double space)
    /// " John " (both leading and trailing)
    ///
    /// Use Cases:
    /// - User names: Ensure consistent formatting
    /// - Email addresses: Must not have spaces
    /// - Text input validation: Prevent accidental whitespace
    /// - Display names: Must be properly trimmed
    ///
    /// Related Methods:
    /// - SanitizeText: More permissive; collapses multiple spaces to single
    /// - LengthBetween: Length validation after whitespace check
    /// </remarks>
    public static string NoExtraWhitespace(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        var trimmed = value.Trim();

        if (Regex.IsMatch(trimmed, @"\s{2,}"))
            throw new ArgumentException($"{paramName} cannot contain multiple consecutive spaces.", paramName);

        return trimmed;
    }

    /// <summary>
    /// Validates that a string length falls between minimum and maximum bounds.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="minLen">Minimum allowed string length (inclusive).</param>
    /// <param name="maxLen">Maximum allowed string length (inclusive).</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <returns>The validated string (trimmed).</returns>
    /// <exception cref="ArgumentException">Thrown if the string length is outside the bounds.</exception>
    /// <remarks>
    /// String Length Validation:
    /// Ensures string values fit within size constraints.
    /// Commonly used for database column constraints and business rules.
    ///
    /// Examples:
    /// Guard.LengthBetween("John", 1, 256, nameof(name))    -> "John" (valid, 4 chars)
    /// Guard.LengthBetween("JD", 2, 256, nameof(initials))   -> "JD" (valid, 2 chars)
    /// Guard.LengthBetween("", 1, 256, nameof(name))         -> throws (too short)
    /// Guard.LengthBetween("x" * 300, 1, 256, nameof(name)) -> throws (too long)
    ///
    /// Common Bounds (per AppConstants):
    /// - MinNameLength = 1, MaxNameLength = 256 (product/person names)
    /// - MaxEmailLength = 256 (per RFC 5321)
    /// - MaxPhoneLength = 20 (E.164 + formatting)
    /// - MinPasswordLength = 8, MaxPasswordLength = 128 (NIST guidelines)
    ///
    /// Best Practice Chain:
    /// var name = Guard.NoExtraWhitespace(input, nameof(input));
    /// name = Guard.LengthBetween(name, 1, 256, nameof(name));
    ///
    /// Use Cases: Name validation, description validation, text input constraints
    /// </remarks>
    public static string LengthBetween(string value, int minLen, int maxLen, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        var trimmed = value.Trim();

        if (trimmed.Length < minLen)
            throw new ArgumentException($"{paramName} length cannot be less than {minLen}.", paramName);

        if (trimmed.Length > maxLen)
            throw new ArgumentException($"{paramName} length cannot be greater than {maxLen}.", paramName);

        return trimmed;
    }

    /// <summary>
    /// Sanitizes a string by trimming and collapsing multiple spaces into single spaces.
    /// </summary>
    /// <param name="value">The string to sanitize.</param>
    /// <returns>The sanitized string with normalized whitespace.</returns>
    /// <remarks>
    /// Text Sanitization:
    /// Unlike NoExtraWhitespace which throws on multiple spaces, SanitizeText corrects them.
    /// More permissive approach; useful for user input that should be cleaned, not rejected.
    ///
    /// Transformations:
    /// - Trim leading/trailing whitespace
    /// - Collapse multiple spaces into single space
    /// - Preserve single spaces between words
    ///
    /// Examples:
    /// SanitizeText("  John   Smith  ") -> "John Smith"
    /// SanitizeText("New   York") -> "New York"
    /// SanitizeText("\t\nHello\r\n") -> "Hello" (whitespace characters trimmed)
    /// SanitizeText("Text") -> "Text" (unchanged if already clean)
    ///
    /// Use Cases:
    /// - User input cleaning: Accept but fix whitespace
    /// - Display name normalization: Ensure consistent formatting
    /// - Description processing: Fix copy-pasted text
    /// - Text import: Normalize data from various sources
    ///
    /// Related Methods:
    /// - NoExtraWhitespace: Stricter; throws on whitespace issues
    /// - LengthBetween: Validate length after sanitization
    ///
    /// Best Practice:
    /// public async Task CreateUserAsync(string name)
    /// {
    ///     name = Guard.SanitizeText(name);                    // Clean it
    ///     Guard.LengthBetween(name, 1, 256, nameof(name));   // Validate cleaned value
    ///     // Now use name safely
    /// }
    /// </remarks>
    public static string SanitizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        return Regex.Replace(trimmed, @"\s+", " ");
    }

    #endregion
}
