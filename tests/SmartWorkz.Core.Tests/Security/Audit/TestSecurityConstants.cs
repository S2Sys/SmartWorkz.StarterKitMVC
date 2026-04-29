namespace SmartWorkz.Core.Tests.Security.Audit;

/// <summary>
/// Test constants for security audit testing.
/// Contains sanitized test data patterns for redaction testing.
/// </summary>
public static class TestSecurityConstants
{
    /// <summary>Sample API key pattern for testing redaction (non-functional test data).</summary>
    public const string TestApiKeyPattern = "api_key=key_test_abcdefghijklmnopqrstuvwxyz";

    /// <summary>Redacted marker for API keys.</summary>
    public const string RedactedMarker = "[REDACTED]";

    /// <summary>Sample email for testing redaction.</summary>
    public const string TestEmail = "user@example.com";

    /// <summary>Sample ID for testing redaction.</summary>
    public const string TestId = "3F2504E0-4F89-41D3-9A0C-0305E8EA9698";

    /// <summary>Sample token for testing redaction.</summary>
    public const string TestToken = "token123456789abcdefghijklmnop.token123456789abcdefghijklmnop.token123456789abcdefghijklmnop";
}
