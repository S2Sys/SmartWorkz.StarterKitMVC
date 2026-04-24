namespace SmartWorkz.Shared.Security.Encryption;

/// <summary>
/// Service for encrypting and decrypting sensitive data using AES-256.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts plaintext using AES-256 CBC mode with PKCS7 padding.
    /// </summary>
    /// <param name="plaintext">The text to encrypt</param>
    /// <param name="key">32-byte AES-256 key</param>
    /// <returns>Encrypted bytes (includes IV prepended)</returns>
    /// <exception cref="ArgumentException">Key length invalid</exception>
    byte[] Encrypt(string plaintext, byte[] key);

    /// <summary>
    /// Decrypts ciphertext encrypted with Encrypt method.
    /// </summary>
    /// <param name="ciphertext">Encrypted bytes (with IV prepended)</param>
    /// <param name="key">32-byte AES-256 key used for encryption</param>
    /// <returns>Original plaintext</returns>
    /// <exception cref="Exception">Decryption failed or corrupted data</exception>
    string Decrypt(byte[] ciphertext, byte[] key);
}
