namespace SmartWorkz.Shared.Security.Encryption;

using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// AES-256 encryption service using CBC mode with PKCS7 padding.
/// IV is prepended to ciphertext for decryption.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private const int AesKeySize = 256; // bits
    private const int AesBlockSize = 128; // bits

    public byte[] Encrypt(string plaintext, byte[] key)
    {
        Guard.NotNull(plaintext, nameof(plaintext));
        Guard.NotNull(key, nameof(key));

        if (key.Length != 32)
            throw new ArgumentException("AES-256 requires 32-byte key", nameof(key));

        using (var aes = Aes.Create())
        {
            aes.KeySize = AesKeySize;
            aes.BlockSize = AesBlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;

            // Generate random IV
            aes.GenerateIV();

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                var encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

                // Prepend IV to ciphertext for transmission
                var result = new byte[aes.IV.Length + encryptedBytes.Length];
                Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                return result;
            }
        }
    }

    public string Decrypt(byte[] ciphertext, byte[] key)
    {
        Guard.NotNull(ciphertext, nameof(ciphertext));
        Guard.NotNull(key, nameof(key));

        if (key.Length != 32)
            throw new ArgumentException("AES-256 requires 32-byte key", nameof(key));

        if (ciphertext.Length < 16)
            throw new ArgumentException("Ciphertext too short (must include IV)", nameof(ciphertext));

        using (var aes = Aes.Create())
        {
            aes.KeySize = AesKeySize;
            aes.BlockSize = AesBlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;

            // Extract IV from ciphertext
            var iv = new byte[16];
            Buffer.BlockCopy(ciphertext, 0, iv, 0, 16);
            aes.IV = iv;

            var encryptedData = new byte[ciphertext.Length - 16];
            Buffer.BlockCopy(ciphertext, 16, encryptedData, 0, encryptedData.Length);

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
