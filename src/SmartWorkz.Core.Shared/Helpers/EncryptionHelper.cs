namespace SmartWorkz.Core.Shared.Helpers;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Cryptographic utilities for hashing and encryption.
/// Uses PBKDF2 for password hashing and AES-256 for data encryption.
/// </summary>
public static class EncryptionHelper
{
    private const int SaltLength = 16; // 128 bits
    private const int HashLength = 32; // 256 bits
    private const int Iterations = 10000;

    /// <summary>Hash password using PBKDF2 with SHA256.</summary>
    public static string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltLength];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashLength);

        var hashWithSalt = new byte[SaltLength + HashLength];
        Buffer.BlockCopy(salt, 0, hashWithSalt, 0, SaltLength);
        Buffer.BlockCopy(hash, 0, hashWithSalt, SaltLength, HashLength);

        return Convert.ToBase64String(hashWithSalt);
    }

    /// <summary>Verify password against hash.</summary>
    public static bool VerifyPassword(string password, string hash)
    {
        try
        {
            var hashWithSalt = Convert.FromBase64String(hash);
            var salt = new byte[SaltLength];
            Buffer.BlockCopy(hashWithSalt, 0, salt, 0, SaltLength);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(HashLength);

            for (int i = 0; i < HashLength; i++)
            {
                if (hashWithSalt[i + SaltLength] != computedHash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Encrypt text using AES-256-GCM with provided key.</summary>
    public static string Encrypt(string plaintext, string base64Key)
    {
        var key = Convert.FromBase64String(base64Key);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var iv = aes.IV;
        using var encryptor = aes.CreateEncryptor(aes.Key, iv);
        using var ms = new MemoryStream();

        ms.Write(iv, 0, iv.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            using var writer = new StreamWriter(cs);
            writer.Write(plaintext);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>Decrypt text using AES-256-GCM with provided key.</summary>
    public static string Decrypt(string ciphertext, string base64Key)
    {
        var key = Convert.FromBase64String(base64Key);
        var buffer = Convert.FromBase64String(ciphertext);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var iv = new byte[aes.IV.Length];
        Buffer.BlockCopy(buffer, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);

        return reader.ReadToEnd();
    }

    /// <summary>Generate cryptographically secure random string.</summary>
    public static string GenerateRandomString(int length = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[length];
        rng.GetBytes(buffer);
        return Convert.ToBase64String(buffer);
    }

    /// <summary>Generate random encryption key (Base64 encoded).</summary>
    public static string GenerateEncryptionKey(int lengthBytes = 32)
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(lengthBytes));

    /// <summary>Compute SHA256 hash of text for integrity checking.</summary>
    public static string ComputeSha256(string text)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash).ToLower();
    }
}
