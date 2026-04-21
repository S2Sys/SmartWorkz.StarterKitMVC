namespace SmartWorkz.Shared;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides AES-256-CBC encryption and decryption utilities with secure key and IV generation.
///
/// All operations support both string and byte array inputs/outputs.
/// Keys are normalized to 32 bytes (256 bits) via padding/trimming as needed.
/// IVs are auto-generated if not provided and embedded in the ciphertext (IV:Ciphertext format).
/// </summary>
public sealed class CryptHelper
{
    private static readonly CryptOptions DefaultOptions = new();

    /// <summary>
    /// Encrypts plaintext using AES-256-CBC with a Base64-encoded output.
    /// </summary>
    /// <param name="plaintext">The plaintext to encrypt.</param>
    /// <param name="key">The encryption key (will be normalized to 32 bytes).</param>
    /// <param name="iv">Optional IV; auto-generated if null.</param>
    /// <returns>A Result containing Base64-encoded ciphertext in "IV:Ciphertext" format or an error.</returns>
    public static Result<string> EncryptString(string? plaintext, string? key, string? iv = null)
    {
        if (string.IsNullOrEmpty(plaintext))
            return Result.Fail<string>("Crypt.InvalidInput", "Plaintext cannot be null or empty");

        if (string.IsNullOrEmpty(key))
            return Result.Fail<string>("Crypt.InvalidKey", "Key cannot be null or empty");

        try
        {
            // Convert inputs to bytes
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var ivBytes = iv != null ? Convert.FromBase64String(iv) : null;

            // Encrypt
            var encryptedBytes = EncryptBytes(plaintextBytes, keyBytes, ivBytes).Data;
            if (encryptedBytes == null)
                return Result.Fail<string>("Crypt.EncryptionFailed", "Encryption operation failed");

            return Result.Ok(Convert.ToBase64String(encryptedBytes));
        }
        catch (FormatException ex)
        {
            return Result.Fail<string>("Crypt.InvalidFormat", $"Invalid Base64 format: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Crypt.EncryptionError", $"Encryption failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Encrypts byte data using AES-256-CBC.
    /// </summary>
    /// <param name="plaintext">The plaintext bytes to encrypt.</param>
    /// <param name="key">The encryption key (will be normalized to 32 bytes).</param>
    /// <param name="iv">Optional IV; auto-generated if null.</param>
    /// <returns>A Result containing encrypted bytes with embedded IV (IV || Ciphertext) or an error.</returns>
    public static Result<byte[]> EncryptBytes(byte[]? plaintext, byte[]? key, byte[]? iv = null)
    {
        if (plaintext == null || plaintext.Length == 0)
            return Result.Fail<byte[]>("Crypt.InvalidInput", "Plaintext bytes cannot be null or empty");

        if (key == null || key.Length == 0)
            return Result.Fail<byte[]>("Crypt.InvalidKey", "Key bytes cannot be null or empty");

        try
        {
            // Normalize key to 32 bytes
            var normalizedKey = NormalizeKey(key);

            // Generate or use provided IV
            byte[] actualIv;
            if (iv == null || iv.Length != CryptOptions.IvSize)
            {
                var ivResult = GenerateRandomBytes(CryptOptions.IvSize);
                if (!ivResult.Succeeded)
                    return Result.Fail<byte[]>("Crypt.IvGenerationFailed", "Failed to generate IV");
                actualIv = ivResult.Data!;
            }
            else
            {
                actualIv = iv;
            }

            using var aes = Aes.Create();
            aes.KeySize = normalizedKey.Length * 8;
            aes.Key = normalizedKey;
            aes.IV = actualIv;
            aes.Mode = DefaultOptions.Mode;
            aes.Padding = DefaultOptions.Padding;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();

            // Write IV to output (for embedding in ciphertext)
            ms.Write(actualIv, 0, actualIv.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plaintext, 0, plaintext.Length);
                cs.FlushFinalBlock();
            }

            return Result.Ok(ms.ToArray());
        }
        catch (Exception ex)
        {
            return Result.Fail<byte[]>("Crypt.EncryptionError", $"Encryption failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Decrypts Base64-encoded ciphertext using AES-256-CBC.
    /// </summary>
    /// <param name="ciphertext">The Base64-encoded ciphertext in "IV:Ciphertext" format.</param>
    /// <param name="key">The decryption key (will be normalized to 32 bytes).</param>
    /// <returns>A Result containing the decrypted plaintext or an error.</returns>
    public static Result<string> DecryptString(string? ciphertext, string? key)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return Result.Fail<string>("Crypt.InvalidInput", "Ciphertext cannot be null or empty");

        if (string.IsNullOrEmpty(key))
            return Result.Fail<string>("Crypt.InvalidKey", "Key cannot be null or empty");

        try
        {
            // Convert inputs to bytes
            var ciphertextBytes = Convert.FromBase64String(ciphertext);
            var keyBytes = Encoding.UTF8.GetBytes(key);

            // Decrypt
            var decryptedBytes = DecryptBytes(ciphertextBytes, keyBytes);
            if (!decryptedBytes.Succeeded)
                return Result.Fail<string>(decryptedBytes.MessageKey!, decryptedBytes.Errors.FirstOrDefault() ?? "Decryption failed");

            var plaintext = Encoding.UTF8.GetString(decryptedBytes.Data!);
            return Result.Ok(plaintext);
        }
        catch (FormatException ex)
        {
            return Result.Fail<string>("Crypt.InvalidFormat", $"Invalid Base64 format: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Crypt.DecryptionError", $"Decryption failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Decrypts byte data using AES-256-CBC.
    /// </summary>
    /// <param name="ciphertext">The encrypted bytes with embedded IV (IV || Ciphertext).</param>
    /// <param name="key">The decryption key (will be normalized to 32 bytes).</param>
    /// <returns>A Result containing decrypted bytes or an error.</returns>
    public static Result<byte[]> DecryptBytes(byte[]? ciphertext, byte[]? key)
    {
        if (ciphertext == null || ciphertext.Length <= CryptOptions.IvSize)
            return Result.Fail<byte[]>("Crypt.InvalidInput", "Ciphertext must contain IV and data");

        if (key == null || key.Length == 0)
            return Result.Fail<byte[]>("Crypt.InvalidKey", "Key bytes cannot be null or empty");

        try
        {
            // Normalize key to 32 bytes
            var normalizedKey = NormalizeKey(key);

            // Extract IV from beginning of ciphertext
            var iv = new byte[CryptOptions.IvSize];
            Array.Copy(ciphertext, 0, iv, 0, CryptOptions.IvSize);

            // Extract actual ciphertext
            var actualCiphertext = new byte[ciphertext.Length - CryptOptions.IvSize];
            Array.Copy(ciphertext, CryptOptions.IvSize, actualCiphertext, 0, actualCiphertext.Length);

            using var aes = Aes.Create();
            aes.KeySize = normalizedKey.Length * 8;
            aes.Key = normalizedKey;
            aes.IV = iv;
            aes.Mode = DefaultOptions.Mode;
            aes.Padding = DefaultOptions.Padding;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(actualCiphertext);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var resultMs = new MemoryStream();

            cs.CopyTo(resultMs);
            return Result.Ok(resultMs.ToArray());
        }
        catch (Exception ex)
        {
            return Result.Fail<byte[]>("Crypt.DecryptionError", $"Decryption failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a random cryptographic key of the specified size.
    /// </summary>
    /// <param name="keySize">The key size in bytes (default 32 for AES-256). Must be 16, 24, or 32.</param>
    /// <returns>A Result containing Base64-encoded random key or an error.</returns>
    public static Result<string> GenerateKey(int keySize = CryptOptions.DefaultKeySize)
    {
        if (keySize != 16 && keySize != 24 && keySize != 32)
            return Result.Fail<string>("Crypt.InvalidKeySize", "Key size must be 16, 24, or 32 bytes");

        try
        {
            var keyResult = GenerateRandomBytes(keySize);
            if (!keyResult.Succeeded)
                return Result.Fail<string>("Crypt.KeyGenerationFailed", "Failed to generate key");

            var base64Key = Convert.ToBase64String(keyResult.Data!);
            return Result.Ok(base64Key);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Crypt.KeyGenerationError", $"Key generation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a random cryptographic IV (Initialization Vector).
    /// </summary>
    /// <returns>A Result containing Base64-encoded random IV or an error.</returns>
    public static Result<string> GenerateIv()
    {
        try
        {
            var ivResult = GenerateRandomBytes(CryptOptions.IvSize);
            if (!ivResult.Succeeded)
                return Result.Fail<string>("Crypt.IvGenerationFailed", "Failed to generate IV");

            var base64Iv = Convert.ToBase64String(ivResult.Data!);
            return Result.Ok(base64Iv);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Crypt.IvGenerationError", $"IV generation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates cryptographically secure random bytes.
    /// </summary>
    private static Result<byte[]> GenerateRandomBytes(int length)
    {
        try
        {
            var buffer = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);
            return Result.Ok(buffer);
        }
        catch (Exception ex)
        {
            return Result.Fail<byte[]>("Crypt.RandomGenerationError", $"Random generation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Normalizes a key to exactly 32 bytes (256 bits).
    /// If the key is shorter, it's padded with zeros. If longer, it's trimmed.
    /// </summary>
    private static byte[] NormalizeKey(byte[] key)
    {
        const int targetSize = CryptOptions.DefaultKeySize;

        if (key.Length == targetSize)
            return key;

        var normalized = new byte[targetSize];

        if (key.Length < targetSize)
        {
            // Pad with zeros
            Array.Copy(key, normalized, key.Length);
        }
        else
        {
            // Trim to target size
            Array.Copy(key, normalized, targetSize);
        }

        return normalized;
    }
}
