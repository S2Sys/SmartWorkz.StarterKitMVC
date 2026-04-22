namespace SmartWorkz.Core;

/// <summary>
/// Application-wide constants for globalization, pagination, caching, validation, and messaging.
/// </summary>
/// <remarks>
/// Purpose and Organization:
/// AppConstants centralizes magic values across the application, eliminating hard-coded literals
/// and enabling consistent behavior across all layers. Constants are organized by functional category:
/// Globalization, Pagination, Caching, Validation, and Messaging.
///
/// Best Practices:
/// 1. Use Constants Instead of Magic Numbers: Always reference AppConstants instead of inline values.
///    Example: Use AppConstants.DefaultPageSize instead of the literal 10.
/// 2. Thread Safety: All constants are thread-safe and immutable by definition.
/// 3. Compile-Time Resolution: Constants are resolved at compile time, providing zero runtime overhead.
/// 4. Per-Entity Overrides: Override global constants in specific services when domain logic requires it,
///    but document the rationale. Example: Premium users might have MaxPageSize = 500 vs standard MaxPageSize = 100.
/// 5. Configuration Synchronization: For values that may change per environment (cache durations, page sizes),
///    consider moving to appsettings.json and loading at startup. Current design supports hard-coded defaults.
///
/// Categories:
/// - Globalization: Culture and timezone defaults for international support
/// - Pagination: Page size constraints for list queries (balanced for UX and performance)
/// - Caching: Cache key naming conventions and duration strategies (short, default, long)
/// - Validation: Input constraints for security and data quality (NIST-compliant password length)
/// - Messaging: Standard user-facing messages for error and validation scenarios
///
/// Security Considerations:
/// - Password constraints follow NIST SP 800-63 guidelines (8-char minimum)
/// - Never expose sensitive thresholds in UI (e.g., max login attempts) to prevent brute-force optimization
/// - Cache durations balance data freshness with server load (short=5min, default=30min, long=24hr)
///
/// Performance Considerations:
/// - PageSize defaults selected for typical web UI (10 rows per page)
/// - MaxPageSize prevents excessive data transfer and query load (100 items maximum per request)
/// - Cache durations tune freshness vs. database load based on typical access patterns
/// - Override on per-entity basis if specific entities have different performance profiles
///
/// Example Usage:
/// // Pagination with AppConstants
/// var query = dbContext.Customers
///     .Where(c => c.Status == EntityStatus.Active)
///     .OrderBy(c => c.Name)
///     .Skip((pageNumber - 1) * AppConstants.DefaultPageSize)
///     .Take(AppConstants.DefaultPageSize);
///
/// // Validation with AppConstants
/// if (password.Length < AppConstants.Validation.MinPasswordLength)
///     return Result.Failure("Password must be at least 8 characters");
///
/// // Caching with AppConstants
/// var cacheKey = AppConstants.Cache.KeyPrefix + "customers:" + customerId;
/// _cache.Set(cacheKey, customer, TimeSpan.FromMinutes(AppConstants.Cache.DefaultDurationMinutes));
/// </remarks>
public static class AppConstants
{
    /// <summary>
    /// Default culture code for application UI and localization (en-US).
    /// </summary>
    /// <remarks>
    /// Purpose: Establishes the default language and regional format (date, currency, number formatting).
    /// Usage: Used by globalization middleware to set thread culture if user hasn't explicitly selected a culture.
    /// Value Rationale: "en-US" selected as English is widely understood and US conventions are internationally common.
    /// Override: User preferences override this default. Enterprise applications may default to company locale.
    /// Integration: Passed to CultureInfo.GetCultureInfo(DefaultCulture) for localization.
    /// Example: DateTime.Now.ToString() uses DefaultCulture formats (MM/dd/yyyy in US, dd/MM/yyyy in other locales).
    /// </remarks>
    public const string DefaultCulture = "en-US";

    /// <summary>
    /// Default timezone for application time operations (UTC).
    /// </summary>
    /// <remarks>
    /// Purpose: Establishes the reference timezone for all server-side time calculations and storage.
    /// Usage: System times are stored in UTC for consistency. UI displays times in user's local timezone.
    /// Value Rationale: UTC eliminates ambiguity and enables consistent auditing across global deployments.
    /// Integration: Used with TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZone) or directly as UTC offset.
    /// Best Practice: Always store server times in UTC, convert to user timezone only in presentation layer.
    /// Example: Order timestamps stored as 2024-03-15 14:30:00 UTC, displayed as 3/15/2024 9:30 AM EST to user.
    /// </remarks>
    public const string DefaultTimeZone = "UTC";

    /// <summary>
    /// Default page size for paginated queries (10 items per page).
    /// </summary>
    /// <remarks>
    /// Purpose: Establishes the standard number of records returned per page in paginated list queries.
    /// Value Rationale: 10 items selected for optimal balance:
    ///   - Small enough for responsive UI (minimal scroll on desktop/tablet)
    ///   - Large enough to reduce number of requests (better network efficiency)
    ///   - Tested for acceptable query performance on typical business entity queries
    /// Usage: Used in LINQ .Take(AppConstants.DefaultPageSize) and database queries.
    /// Override: Specific queries may override (e.g., customer dashboard shows 25 items, reports show 50).
    /// Performance: 10 items typically loads in <100ms on well-indexed databases. Monitor slow queries.
    /// User Experience: Avoid forcing users to click "next" excessively (pagination fatigue).
    /// Example: GetCustomers(pageNumber=1) returns customers 1-10; pageNumber=2 returns 11-20.
    /// </remarks>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Maximum page size allowed in paginated queries (100 items per page).
    /// </summary>
    /// <remarks>
    /// Purpose: Prevents abuse or misconfiguration from requesting excessive data in a single query.
    /// Value Rationale: 100 items selected as a reasonable upper limit for most use cases:
    ///   - Prevents database overload from unlimited queries
    ///   - Prevents network bandwidth exhaustion (typical item ~1-2KB)
    ///   - Maintains acceptable response times even on slower connections
    /// Usage: Enforce in service layer: pageSize = Math.Min(requestedPageSize, AppConstants.MaxPageSize).
    /// Business Impact: Requests for 1000+ items are rejected; user must use export/report functionality instead.
    /// Performance: 100 items typically loads in <500ms. Monitor slow queries and optimize indexes if exceeded.
    /// Security: Prevents DoS attacks via unbounded pagination requests.
    /// Example: API request with pageSize=500 is clamped to 100 items maximum.
    /// </remarks>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Minimum page size allowed in paginated queries (1 item per page).
    /// </summary>
    /// <remarks>
    /// Purpose: Establishes the practical lower bound for pagination to prevent configuration errors.
    /// Value Rationale: 1 item allows testing and edge-case scenarios (single-record display).
    /// Usage: Enforce in service layer: pageSize = Math.Max(requestedPageSize, AppConstants.MinPageSize).
    /// Typical Use: Rarely used in production; prevents pageSize=0 errors in UI.
    /// Example: Mobile UI shows 1 item per swipe; desktop shows 10; reports show 50 (all bounded by Min/Max).
    /// </remarks>
    public const int MinPageSize = 1;

    /// <summary>
    /// Caching configuration for distributed and in-memory cache strategies.
    /// </summary>
    /// <remarks>
    /// Purpose: Centralizes cache key naming and duration settings for consistent cache behavior.
    /// Strategy: Differentiate durations based on data volatility:
    ///   - ShortDuration (5 min): Volatile data, frequently changing (user activity, stock prices)
    ///   - DefaultDuration (30 min): Moderately stable data (customer profiles, orders)
    ///   - LongDuration (24 hr): Stable reference data (products, categories, configurations)
    /// Invalidation: Cache is typically invalidated on data changes (not time-based expiry).
    ///   Duration acts as a safety net for missed invalidation events.
    /// Implementation: Redis, Memcached, or in-memory IMemoryCache support these durations.
    /// Monitoring: Track cache hit rates. <70% hit rate suggests durations are too short.
    /// </remarks>
    public static class Cache
    {
        /// <summary>
        /// Cache key prefix for all application cache entries ("smartworkz:").
        /// </summary>
        /// <remarks>
        /// Purpose: Namespaces all cache keys to prevent collisions with other applications sharing the cache.
        /// Format: Prefix followed by entity type and identifier. Example: "smartworkz:customer:123"
        /// Usage: Concatenate with entity type and ID. cache.Set(KeyPrefix + "customer:" + customerId, data).
        /// Distributed Cache: Required when using shared cache systems (Redis, Memcached) to isolate this app.
        /// Invalidation: Simplifies clearing all app cache by prefix: cache.RemoveByPattern("smartworkz:*").
        /// Example: Customer cache key = "smartworkz:customer:456", Product cache key = "smartworkz:product:789".
        /// </remarks>
        public const string KeyPrefix = "smartworkz:";

        /// <summary>
        /// Default cache duration for moderately volatile data (30 minutes).
        /// </summary>
        /// <remarks>
        /// Purpose: Caches data that changes occasionally but not constantly.
        /// Value Rationale: 30 minutes selected as balance between freshness and cache efficiency:
        ///   - Reduces database queries for repeat views of same data
        ///   - Tolerates 30-min data staleness for most business contexts
        ///   - Typical session duration aligns with cache expiry (user closes browser, cache expires)
        /// Use Cases:
        ///   - Customer profiles (change infrequently)
        ///   - Order summaries (change on status update, not continuously)
        ///   - Product details (change on inventory update)
        ///   - User preferences (change rarely)
        /// Invalidation Strategy: Event-driven (update cache on data change) preferred; duration acts as fallback.
        /// Example: Customer profile cached for 30 min; if customer updates profile, clear cache immediately.
        /// </remarks>
        public const int DefaultDurationMinutes = 30;

        /// <summary>
        /// Long cache duration for relatively stable reference data (24 hours).
        /// </summary>
        /// <remarks>
        /// Purpose: Caches data that rarely changes, maximizing cache efficiency.
        /// Value Rationale: 24 hours selected for reference data with daily update cadence:
        ///   - Typically updated once daily (batch jobs, admin actions)
        ///   - Tolerate up to 24-hour staleness acceptable for reference data
        ///   - Significant database load reduction for heavily-accessed data (product catalog, localization)
        /// Use Cases:
        ///   - Product catalog (changes in scheduled batches)
        ///   - Category lists (rarely change)
        ///   - Localization/translation strings (updated in maintenance windows)
        ///   - Configuration metadata (updated by admins)
        ///   - Report definitions and templates (static once created)
        /// Invalidation Strategy: Scheduled clearing (e.g., midnight daily) or explicit admin action.
        /// Performance: Reduces database load by 80-95% for heavily-accessed reference data.
        /// Example: Product catalog cached for 24 hours; admin updates trigger immediate cache clear.
        /// </remarks>
        public const int LongDurationHours = 24;

        /// <summary>
        /// Short cache duration for volatile data (5 minutes).
        /// </summary>
        /// <remarks>
        /// Purpose: Caches data that changes frequently, balancing freshness and efficiency.
        /// Value Rationale: 5 minutes selected for data with high change frequency:
        ///   - Reduces database queries during rapid changes
        ///   - 5-minute staleness acceptable for volatile data (activity feeds, real-time counters)
        ///   - Short enough for user to perceive near-real-time updates
        /// Use Cases:
        ///   - Active user counts (updated frequently by events)
        ///   - Real-time dashboards (refreshed by polling every 5-10 sec)
        ///   - Temporary operation results (hold briefly to avoid duplicate processing)
        ///   - Session state aggregates (combined from multiple sources)
        ///   - Cached API responses for external services (fresh data important)
        /// Invalidation Strategy: Event-driven (clear immediately on data change) strongly recommended.
        ///   Duration acts as fallback for missed events.
        /// Example: Active order count cached for 5 min; when order created, clear cache immediately.
        /// </remarks>
        public const int ShortDurationMinutes = 5;
    }

    /// <summary>
    /// Validation constraints for user input and data integrity.
    /// </summary>
    /// <remarks>
    /// Purpose: Centralizes validation thresholds for consistent user input validation across all services.
    /// Strategy: Balance security (minimum lengths prevent weak inputs) with usability (reasonable maximums).
    /// Integration: Used in data annotations, service validation methods, and API models.
    /// Security Considerations:
    ///   - MinPasswordLength follows NIST SP 800-63 guideline (8 characters minimum)
    ///   - MaxPasswordLength (128) accommodates passphrases while preventing DoS (excessively long hash computation)
    ///   - Email/phone lengths accommodate valid international formats
    /// Database Design: MaxNameLength, MaxEmailLength drive VARCHAR column sizes in database schema.
    /// Best Practice: Apply these constraints in:
    ///   1. Data annotations on model properties ([StringLength(AppConstants.Validation.MaxNameLength)])
    ///   2. Service validation methods (if (name.Length > MaxNameLength) return error)
    ///   3. API request validation (ASP.NET ModelState validates attribute constraints)
    /// Example: User registration validates password against MinPasswordLength and MaxPasswordLength.
    /// </remarks>
    public static class Validation
    {
        /// <summary>
        /// Minimum password length (8 characters, per NIST guidelines).
        /// </summary>
        /// <remarks>
        /// Standard: NIST SP 800-63B specifies 8-character minimum for user-chosen passwords.
        /// Rationale: 8 characters provides practical security against brute-force attacks when combined with:
        ///   - Rate limiting (max 10 attempts per account, then lockout)
        ///   - Strong hashing (bcrypt, scrypt, Argon2)
        ///   - Account recovery mechanisms (2FA, security questions)
        /// Recommendation: Encourage 12+ character passphrases instead of complex symbols. Length > complexity.
        /// High-Security Applications: Consider 12+ for privileged accounts (admins, financial users).
        /// Integration: Used in Password property validation: [StringLength(Max, MinimumLength = MinPasswordLength)]
        /// Example: "password" (8 chars) allowed; "pass" (4 chars) rejected.
        /// </remarks>
        public const int MinPasswordLength = 8;

        /// <summary>
        /// Maximum password length (128 characters).
        /// </summary>
        /// <remarks>
        /// Purpose: Prevents excessively long passwords that could cause performance issues or DoS attacks.
        /// Value Rationale: 128 characters accommodates typical passphrases while preventing abuse:
        ///   - Typical passphrase: "The quick brown fox jumps over the lazy dog 123" = 50+ characters
        ///   - Most password hashes (bcrypt, SHA-256) process 128 chars without performance degradation
        ///   - Prevents hash computation DoS (very long strings slow down hashing algorithms)
        /// Real-World Scenario: "MyP@ssw0rd!" (typical) vs "a"*10000 (DoS attempt)
        /// Integration: Used in Password property validation: [StringLength(MaxPasswordLength)]
        /// Database: VARCHAR(128) stores hashed passwords (bcrypt output ~60 chars, room for safety)
        /// Example: "MyP@ssw0rd123" (13 chars) allowed; 1000-character string rejected.
        /// </remarks>
        public const int MaxPasswordLength = 128;

        /// <summary>
        /// Minimum name length (1 character).
        /// </summary>
        /// <remarks>
        /// Purpose: Allows minimal but non-empty names (full names, product names, etc.).
        /// Value Rationale: 1 character accommodates single-letter names (rare but valid):
        ///   - Single-letter names exist (e.g., "X Æ A-12" company name, "T" as nickname)
        ///   - Prevents empty strings while allowing flexibility for various name formats
        ///   - Practical minimum for database constraint (non-null, not empty)
        /// Database: NOT NULL constraint enforced at column level; uniqueness varies by entity.
        /// Integration: Used in Name properties: [StringLength(MaxNameLength, MinimumLength = MinNameLength)]
        /// Example: "Bob" (3 chars) allowed; "" (empty string) rejected; "X" (1 char) allowed.
        /// </remarks>
        public const int MinNameLength = 1;

        /// <summary>
        /// Maximum name length (256 characters).
        /// </summary>
        /// <remarks>
        /// Purpose: Constrains name fields to reasonable length while accommodating various naming conventions.
        /// Value Rationale: 256 characters accommodates:
        ///   - Full legal names with titles: "Dr. Johann Chrysostom Wolfgang Amadeus Mozart von Salzburg III" (75 chars)
        ///   - Business entity names with long descriptors
        ///   - International names with prefixes/suffixes
        ///   - Product names with descriptions appended
        /// Database: VARCHAR(256) column size; supports efficient indexing.
        /// Real-World: Longest name on record ~200 characters (rare edge case).
        /// Integration: Used in Name properties: [StringLength(MaxNameLength)]
        /// Example: "Alice Elizabeth Johnson" (23 chars) allowed; 500-char string rejected.
        /// </remarks>
        public const int MaxNameLength = 256;

        /// <summary>
        /// Maximum email address length (256 characters).
        /// </summary>
        /// <remarks>
        /// Purpose: Constrains email fields to accommodate valid email formats worldwide.
        /// Standard: RFC 5321 allows local part (64 chars) + domain (255 chars) = 319 chars maximum.
        ///   This constant (256) is practical limit, longer addresses are rare/non-compliant.
        /// Value Rationale: 256 characters accommodates:
        ///   - Typical emails: "john.smith@company.co.uk" (24 chars)
        ///   - Long domain names: "firstname.lastname@subdomain.company.government.country.org" (60+ chars)
        ///   - Safety buffer for international domains (IDN, longer TLDs)
        /// Database: VARCHAR(256) column size; email typically indexed for login/lookup.
        /// Validation: Email format validated with regex/EmailAddressAttribute separately.
        ///   This constraint prevents extremely long invalid formats.
        /// Integration: Used in Email properties: [StringLength(MaxEmailLength), EmailAddress]
        /// Example: "john.doe@example.com" (20 chars) allowed; 300-char string rejected.
        /// </remarks>
        public const int MaxEmailLength = 256;

        /// <summary>
        /// Maximum phone number length (20 characters).
        /// </summary>
        /// <remarks>
        /// Purpose: Constrains phone number fields to accommodate international phone formats.
        /// Standard: E.164 format allows max 15 digits plus optional formatting = 20 characters.
        /// Value Rationale: 20 characters accommodates:
        ///   - International format: "+1 (555) 123-4567" (18 chars)
        ///   - Extension: "+1-555-123-4567 x1234" (22 chars, slightly over, but most implementations extend)
        ///   - Various formats (spaces, dashes, parentheses, country codes)
        ///   - Safety buffer for regional variations
        /// Database: VARCHAR(20) column; phone typically indexed for contact lookups.
        /// Validation: Phone format validated with regex/PhoneAttribute separately (this constraint prevents outliers).
        /// Note: Longer values (extensions, descriptive notes) should be stored in separate field.
        /// Integration: Used in Phone properties: [StringLength(MaxPhoneLength)]
        /// Example: "+1-555-123-4567" (16 chars) allowed; "+1 (555) 123-4567 EXT: 99999" (rejected).
        /// </remarks>
        public const int MaxPhoneLength = 20;
    }

    /// <summary>
    /// Standard user-facing messages for validation, error, and informational scenarios.
    /// </summary>
    /// <remarks>
    /// Purpose: Centralizes common UI messages for localization and consistency.
    /// Implementation: These are base English messages; translate via localization middleware.
    /// Best Practice: Use these for common scenarios; customize messages for specific validations.
    /// Localization: Pass message keys (not text) to frontend, translate per user culture.
    /// Integration: Used in validation results, error responses, and API error messages.
    /// Example Scenario:
    ///   Service returns: Result.Failure(AppConstants.Messages.Required)
    ///   Localization middleware translates: "Ce champ est requis" (French)
    ///   UI displays to user: "This field is required" (or localized equivalent)
    /// </remarks>
    public static class Messages
    {
        /// <summary>
        /// Required field validation message ("This field is required").
        /// </summary>
        /// <remarks>
        /// When to use: When a required field (email, password, name) is missing or empty.
        /// Context: Generic message suitable for any required field validation.
        /// Localization: Translate to "Ce champ est requis" (FR), "Este campo es requerido" (ES).
        /// Example: User submits registration without entering email address.
        /// </remarks>
        public const string Required = "This field is required";

        /// <summary>
        /// Invalid format validation message ("Invalid format").
        /// </summary>
        /// <remarks>
        /// When to use: When input format/pattern doesn't match expected (email format, phone format, date format).
        /// Context: Generic message; consider appending specific format guidance ("must be MM/DD/YYYY").
        /// Localization: Translate to "Format invalide" (FR), "Formato inválido" (ES).
        /// Example: User enters "john.smith" as email (missing @domain).
        /// Better Practice: Include expected format in message: "Invalid email format. Expected: user@domain.com"
        /// </remarks>
        public const string InvalidFormat = "Invalid format";

        /// <summary>
        /// Resource not found message ("Resource not found").
        /// </summary>
        /// <remarks>
        /// When to use: When a requested resource doesn't exist (GetCustomer returns null, order not found).
        /// Context: Generic message; consider appending what was requested ("Customer #123 not found").
        /// Localization: Translate to "Ressource non trouvée" (FR), "Recurso no encontrado" (ES).
        /// Example: User requests order details for order ID that doesn't exist.
        /// Better Practice: Include context: "Order #567 not found. Verify order number or create new order."
        /// HTTP Mapping: This message typically maps to HTTP 404 Not Found response.
        /// </remarks>
        public const string NotFound = "Resource not found";

        /// <summary>
        /// Authorization denied message ("Unauthorized access").
        /// </summary>
        /// <remarks>
        /// When to use: When user lacks permission/authentication for a requested action.
        /// Context: Generic message; typically maps to HTTP 401 (Unauthorized) or 403 (Forbidden).
        /// Localization: Translate to "Accès non autorisé" (FR), "Acceso no autorizado" (ES).
        /// Examples:
        ///   - User not logged in attempting to access protected resource
        ///   - User lacks Admin role attempting to delete users
        ///   - API token missing or invalid
        /// Better Practice: Distinguish between authentication vs authorization:
        ///   - Authentication (401): "Please log in to continue"
        ///   - Authorization (403): "You don't have permission to perform this action"
        /// </remarks>
        public const string Unauthorized = "Unauthorized access";
    }
}
