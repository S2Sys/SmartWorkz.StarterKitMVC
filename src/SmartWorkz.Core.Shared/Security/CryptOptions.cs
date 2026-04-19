namespace SmartWorkz.Core.Shared.Security;

using System.Security.Cryptography;

/// <summary>
/// Configuration options for AES cryptographic operations.
/// </summary>
public sealed class CryptOptions
{
    /// <summary>
    /// Default key size in bytes (32 = 256 bits).
    /// </summary>
    public const int DefaultKeySize = 32;

    /// <summary>
    /// IV (Initialization Vector) size in bytes (16 = 128 bits, standard for AES).
    /// </summary>
    public const int IvSize = 16;

    /// <summary>
    /// The cipher mode to use (CBC is the default and recommended mode).
    /// </summary>
    public CipherMode Mode { get; set; } = CipherMode.CBC;

    /// <summary>
    /// The padding mode to use (PKCS7 is the standard).
    /// </summary>
    public PaddingMode Padding { get; set; } = PaddingMode.PKCS7;

    /// <summary>
    /// The key size in bytes (256-bit = 32 bytes is the default).
    /// </summary>
    public int KeySize { get; set; } = DefaultKeySize;

    /// <summary>
    /// Validates the options for correctness.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid()
    {
        return KeySize == 16 || KeySize == 24 || KeySize == 32;
    }
}
