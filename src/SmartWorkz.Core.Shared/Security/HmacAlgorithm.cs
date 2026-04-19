namespace SmartWorkz.Core.Shared.Security;

/// <summary>
/// Specifies the HMAC algorithm to use for message signing and verification.
/// </summary>
public enum HmacAlgorithm
{
    /// <summary>HMAC-SHA256 (default, recommended).</summary>
    SHA256 = 0,

    /// <summary>HMAC-SHA512 (higher security).</summary>
    SHA512 = 1
}
