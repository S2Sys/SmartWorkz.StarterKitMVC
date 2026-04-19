namespace SmartWorkz.Core.Tests.Security;

using System.Text;
using SmartWorkz.Core.Shared.Security;

public class CryptHelperTests
{
    private const string TestKey = "MySecretKey123456789012345678901";
    private const string TestPlaintext = "Hello, World!";
    private const string LongTestPlaintext = "The quick brown fox jumps over the lazy dog. This is a longer test string to ensure encryption and decryption work correctly with more data.";

    // ===== String Encryption/Decryption Tests =====

    [Fact]
    public void EncryptString_WithValidInputs_ReturnsSuccessResult()
    {
        // Act
        var result = CryptHelper.EncryptString(TestPlaintext, TestKey);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public void EncryptString_WithNullPlaintext_ReturnsFails()
    {
        // Act
        var result = CryptHelper.EncryptString(null, TestKey);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidInput", result.MessageKey);
    }

    [Fact]
    public void EncryptString_WithEmptyPlaintext_ReturnsFails()
    {
        // Act
        var result = CryptHelper.EncryptString(string.Empty, TestKey);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidInput", result.MessageKey);
    }

    [Fact]
    public void EncryptString_WithNullKey_ReturnsFails()
    {
        // Act
        var result = CryptHelper.EncryptString(TestPlaintext, null);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidKey", result.MessageKey);
    }

    [Fact]
    public void EncryptString_WithEmptyKey_ReturnsFails()
    {
        // Act
        var result = CryptHelper.EncryptString(TestPlaintext, string.Empty);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidKey", result.MessageKey);
    }

    [Fact]
    public void DecryptString_WithValidCiphertext_ReturnsSuccessResult()
    {
        // Arrange
        var encryptResult = CryptHelper.EncryptString(TestPlaintext, TestKey);
        Assert.True(encryptResult.Succeeded);

        // Act
        var decryptResult = CryptHelper.DecryptString(encryptResult.Data, TestKey);

        // Assert
        Assert.True(decryptResult.Succeeded);
        Assert.Equal(TestPlaintext, decryptResult.Data);
    }

    [Fact]
    public void DecryptString_WithNullCiphertext_ReturnsFails()
    {
        // Act
        var result = CryptHelper.DecryptString(null, TestKey);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidInput", result.MessageKey);
    }

    [Fact]
    public void DecryptString_WithEmptyCiphertext_ReturnsFails()
    {
        // Act
        var result = CryptHelper.DecryptString(string.Empty, TestKey);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void DecryptString_WithNullKey_ReturnsFails()
    {
        // Arrange
        var encryptResult = CryptHelper.EncryptString(TestPlaintext, TestKey);

        // Act
        var result = CryptHelper.DecryptString(encryptResult.Data, null);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidKey", result.MessageKey);
    }

    [Fact]
    public void DecryptString_WithWrongKey_ReturnsFails()
    {
        // Arrange
        var encryptResult = CryptHelper.EncryptString(TestPlaintext, TestKey);
        var wrongKey = "WrongKeyValue123456789012345678901";

        // Act
        var decryptResult = CryptHelper.DecryptString(encryptResult.Data, wrongKey);

        // Assert
        Assert.False(decryptResult.Succeeded);
        Assert.Equal("Crypt.DecryptionError", decryptResult.MessageKey);
    }

    [Fact]
    public void DecryptString_WithInvalidBase64_ReturnsFails()
    {
        // Act
        var result = CryptHelper.DecryptString("!!!NotBase64!!!", TestKey);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidFormat", result.MessageKey);
    }

    // ===== Byte Encryption/Decryption Tests =====

    [Fact]
    public void EncryptBytes_WithValidInputs_ReturnsSuccessResult()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes(TestPlaintext);
        var key = Encoding.UTF8.GetBytes(TestKey);

        // Act
        var result = CryptHelper.EncryptBytes(plaintext, key);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public void EncryptBytes_WithNullPlaintext_ReturnsFails()
    {
        // Arrange
        var key = Encoding.UTF8.GetBytes(TestKey);

        // Act
        var result = CryptHelper.EncryptBytes(null, key);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidInput", result.MessageKey);
    }

    [Fact]
    public void EncryptBytes_WithEmptyPlaintext_ReturnsFails()
    {
        // Arrange
        var key = Encoding.UTF8.GetBytes(TestKey);

        // Act
        var result = CryptHelper.EncryptBytes(Array.Empty<byte>(), key);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidInput", result.MessageKey);
    }

    [Fact]
    public void EncryptBytes_WithNullKey_ReturnsFails()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes(TestPlaintext);

        // Act
        var result = CryptHelper.EncryptBytes(plaintext, null);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidKey", result.MessageKey);
    }

    [Fact]
    public void EncryptBytes_WithEmptyKey_ReturnsFails()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes(TestPlaintext);

        // Act
        var result = CryptHelper.EncryptBytes(plaintext, Array.Empty<byte>());

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidKey", result.MessageKey);
    }

    [Fact]
    public void EncryptBytes_WithCustomIv_UsesProvidedIv()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes(TestPlaintext);
        var key = Encoding.UTF8.GetBytes(TestKey);
        var customIvResult = CryptHelper.GenerateIv();
        Assert.True(customIvResult.Succeeded);
        var customIv = Convert.FromBase64String(customIvResult.Data!);

        // Act
        var result = CryptHelper.EncryptBytes(plaintext, key, customIv);

        // Assert
        Assert.True(result.Succeeded);
        // Verify IV is at the beginning of the encrypted data
        Assert.Equal(customIv, result.Data!.Take(16).ToArray());
    }

    [Fact]
    public void DecryptBytes_WithValidCiphertext_ReturnsSuccessResult()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes(TestPlaintext);
        var key = Encoding.UTF8.GetBytes(TestKey);
        var encryptResult = CryptHelper.EncryptBytes(plaintext, key);
        Assert.True(encryptResult.Succeeded);

        // Act
        var decryptResult = CryptHelper.DecryptBytes(encryptResult.Data, key);

        // Assert
        Assert.True(decryptResult.Succeeded);
        Assert.Equal(plaintext, decryptResult.Data);
    }

    [Fact]
    public void DecryptBytes_WithNullCiphertext_ReturnsFails()
    {
        // Arrange
        var key = Encoding.UTF8.GetBytes(TestKey);

        // Act
        var result = CryptHelper.DecryptBytes(null, key);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidInput", result.MessageKey);
    }

    [Fact]
    public void DecryptBytes_WithShortCiphertext_ReturnsFails()
    {
        // Arrange
        var key = Encoding.UTF8.GetBytes(TestKey);
        var shortCiphertext = Encoding.UTF8.GetBytes("short");

        // Act
        var result = CryptHelper.DecryptBytes(shortCiphertext, key);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidInput", result.MessageKey);
    }

    [Fact]
    public void DecryptBytes_WithNullKey_ReturnsFails()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes(TestPlaintext);
        var key = Encoding.UTF8.GetBytes(TestKey);
        var encryptResult = CryptHelper.EncryptBytes(plaintext, key);

        // Act
        var result = CryptHelper.DecryptBytes(encryptResult.Data, null);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidKey", result.MessageKey);
    }

    [Fact]
    public void DecryptBytes_WithWrongKey_ReturnsFails()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes(TestPlaintext);
        var key = Encoding.UTF8.GetBytes(TestKey);
        var wrongKey = Encoding.UTF8.GetBytes("WrongKeyValue123456789012345678901");
        var encryptResult = CryptHelper.EncryptBytes(plaintext, key);

        // Act
        var decryptResult = CryptHelper.DecryptBytes(encryptResult.Data, wrongKey);

        // Assert
        Assert.False(decryptResult.Succeeded);
    }

    // ===== Key Generation Tests =====

    [Fact]
    public void GenerateKey_WithDefaultSize_ReturnsValidBase64Key()
    {
        // Act
        var result = CryptHelper.GenerateKey();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        // Verify it's valid Base64 and correct size
        var keyBytes = Convert.FromBase64String(result.Data!);
        Assert.Equal(32, keyBytes.Length); // 256 bits = 32 bytes
    }

    [Theory]
    [InlineData(16)] // 128-bit key
    [InlineData(24)] // 192-bit key
    [InlineData(32)] // 256-bit key
    public void GenerateKey_WithValidSizes_ReturnsKeyOfCorrectSize(int keySize)
    {
        // Act
        var result = CryptHelper.GenerateKey(keySize);

        // Assert
        Assert.True(result.Succeeded);
        var keyBytes = Convert.FromBase64String(result.Data!);
        Assert.Equal(keySize, keyBytes.Length);
    }

    [Theory]
    [InlineData(8)]  // Too small
    [InlineData(15)] // Too small
    [InlineData(17)] // Invalid
    [InlineData(33)] // Too large
    [InlineData(64)] // Way too large
    public void GenerateKey_WithInvalidSize_ReturnsFails(int keySize)
    {
        // Act
        var result = CryptHelper.GenerateKey(keySize);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Crypt.InvalidKeySize", result.MessageKey);
    }

    [Fact]
    public void GenerateKey_ProducesRandomKeys()
    {
        // Act
        var result1 = CryptHelper.GenerateKey();
        var result2 = CryptHelper.GenerateKey();

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.NotEqual(result1.Data, result2.Data); // Keys should be different
    }

    // ===== IV Generation Tests =====

    [Fact]
    public void GenerateIv_ReturnsValidBase64Iv()
    {
        // Act
        var result = CryptHelper.GenerateIv();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        // Verify it's valid Base64 and correct size
        var ivBytes = Convert.FromBase64String(result.Data!);
        Assert.Equal(16, ivBytes.Length); // Standard AES IV is 16 bytes
    }

    [Fact]
    public void GenerateIv_ProducesRandomIvs()
    {
        // Act
        var result1 = CryptHelper.GenerateIv();
        var result2 = CryptHelper.GenerateIv();

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.NotEqual(result1.Data, result2.Data); // IVs should be different
    }

    // ===== Round-Trip Tests =====

    [Fact]
    public void EncryptDecrypt_StringRoundTrip_PreservesPlaintext()
    {
        // Act
        var encryptResult = CryptHelper.EncryptString(TestPlaintext, TestKey);
        var decryptResult = CryptHelper.DecryptString(encryptResult.Data, TestKey);

        // Assert
        Assert.True(encryptResult.Succeeded);
        Assert.True(decryptResult.Succeeded);
        Assert.Equal(TestPlaintext, decryptResult.Data);
    }

    [Fact]
    public void EncryptDecrypt_BytesRoundTrip_PreservesData()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes(TestPlaintext);
        var key = Encoding.UTF8.GetBytes(TestKey);

        // Act
        var encryptResult = CryptHelper.EncryptBytes(plaintext, key);
        var decryptResult = CryptHelper.DecryptBytes(encryptResult.Data, key);

        // Assert
        Assert.True(encryptResult.Succeeded);
        Assert.True(decryptResult.Succeeded);
        Assert.Equal(plaintext, decryptResult.Data);
    }

    [Fact]
    public void EncryptDecrypt_LongText_PreservesContent()
    {
        // Act
        var encryptResult = CryptHelper.EncryptString(LongTestPlaintext, TestKey);
        var decryptResult = CryptHelper.DecryptString(encryptResult.Data, TestKey);

        // Assert
        Assert.True(encryptResult.Succeeded);
        Assert.True(decryptResult.Succeeded);
        Assert.Equal(LongTestPlaintext, decryptResult.Data);
    }

    [Fact]
    public void EncryptDecrypt_WithSpecialCharacters_PreservesContent()
    {
        // Arrange
        var specialText = "Special chars: !@#$%^&*()_+-=[]{}|;:,.<>? \n\t\r";

        // Act
        var encryptResult = CryptHelper.EncryptString(specialText, TestKey);
        var decryptResult = CryptHelper.DecryptString(encryptResult.Data, TestKey);

        // Assert
        Assert.True(encryptResult.Succeeded);
        Assert.True(decryptResult.Succeeded);
        Assert.Equal(specialText, decryptResult.Data);
    }

    [Fact]
    public void EncryptDecrypt_WithUnicode_PreservesContent()
    {
        // Arrange
        var unicodeText = "Unicode: 你好, мир, مرحبا, שלום, 🔒🔐🗝️";

        // Act
        var encryptResult = CryptHelper.EncryptString(unicodeText, TestKey);
        var decryptResult = CryptHelper.DecryptString(encryptResult.Data, TestKey);

        // Assert
        Assert.True(encryptResult.Succeeded);
        Assert.True(decryptResult.Succeeded);
        Assert.Equal(unicodeText, decryptResult.Data);
    }

    [Fact]
    public void EncryptDecrypt_WithKeyNormalization_Works()
    {
        // Arrange - Use a key shorter than 32 bytes
        var shortKey = "shortkey";

        // Act
        var encryptResult = CryptHelper.EncryptString(TestPlaintext, shortKey);
        var decryptResult = CryptHelper.DecryptString(encryptResult.Data, shortKey);

        // Assert
        Assert.True(encryptResult.Succeeded);
        Assert.True(decryptResult.Succeeded);
        Assert.Equal(TestPlaintext, decryptResult.Data);
    }

    [Fact]
    public void EncryptDecrypt_WithCustomIv_Works()
    {
        // Arrange
        var plaintext = Encoding.UTF8.GetBytes(TestPlaintext);
        var key = Encoding.UTF8.GetBytes(TestKey);
        var ivResult = CryptHelper.GenerateIv();
        var customIv = Convert.FromBase64String(ivResult.Data!);

        // Act
        var encryptResult = CryptHelper.EncryptBytes(plaintext, key, customIv);
        var decryptResult = CryptHelper.DecryptBytes(encryptResult.Data, key);

        // Assert
        Assert.True(encryptResult.Succeeded);
        Assert.True(decryptResult.Succeeded);
        Assert.Equal(plaintext, decryptResult.Data);
    }

    // ===== Edge Case Tests =====

    [Fact]
    public void EncryptDecrypt_WithEmptyString_Fails()
    {
        // Act
        var result = CryptHelper.EncryptString(string.Empty, TestKey);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void EncryptDecrypt_WithVeryLongKey_Works()
    {
        // Arrange
        var veryLongKey = new string('x', 1000);
        var plaintext = "Test";

        // Act
        var encryptResult = CryptHelper.EncryptString(plaintext, veryLongKey);
        var decryptResult = CryptHelper.DecryptString(encryptResult.Data, veryLongKey);

        // Assert
        Assert.True(encryptResult.Succeeded);
        Assert.True(decryptResult.Succeeded);
        Assert.Equal(plaintext, decryptResult.Data);
    }

    [Fact]
    public void EncryptDecrypt_SamePlaintextProducesDifferentCiphertext()
    {
        // This tests that the IV is actually being randomly generated and used

        // Act
        var encrypt1 = CryptHelper.EncryptString(TestPlaintext, TestKey);
        var encrypt2 = CryptHelper.EncryptString(TestPlaintext, TestKey);

        // Assert
        Assert.True(encrypt1.Succeeded);
        Assert.True(encrypt2.Succeeded);
        Assert.NotEqual(encrypt1.Data, encrypt2.Data); // Different IVs should produce different ciphertexts
    }

    [Fact]
    public void EncryptString_EncryptedDataIsBase64_CanBeTransmitted()
    {
        // Act
        var encryptResult = CryptHelper.EncryptString(TestPlaintext, TestKey);

        // Assert
        Assert.True(encryptResult.Succeeded);
        // Should be valid Base64 (can be decoded without throwing)
        var _ = Convert.FromBase64String(encryptResult.Data!);
        Assert.True(true); // If we got here, it's valid Base64
    }
}
