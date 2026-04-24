namespace SmartWorkz.Core.Tests.Security.Encryption;

using System.Text;
using Xunit;
using SmartWorkz.Shared.Security.Encryption;

public class EncryptionServiceTests
{
    [Fact]
    public void Encrypt_ValidPlaintext_ReturnsEncryptedBytes()
    {
        var service = new EncryptionService();
        var plaintext = "sensitive-data-123";
        var key = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef"); // 32 bytes for AES-256

        var encrypted = service.Encrypt(plaintext, key);

        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
        Assert.NotEqual(plaintext, Encoding.UTF8.GetString(encrypted));
    }

    [Fact]
    public void Decrypt_ValidEncryptedBytes_ReturnsOriginalPlaintext()
    {
        var service = new EncryptionService();
        var plaintext = "sensitive-data-123";
        var key = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef");

        var encrypted = service.Encrypt(plaintext, key);
        var decrypted = service.Decrypt(encrypted, key);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Decrypt_InvalidKey_ThrowsException()
    {
        var service = new EncryptionService();
        var plaintext = "sensitive-data-123";
        var correctKey = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef");
        var wrongKey = Encoding.UTF8.GetBytes("fedcba9876543210fedcba9876543210");

        var encrypted = service.Encrypt(plaintext, correctKey);

        Assert.Throws<Exception>(() => service.Decrypt(encrypted, wrongKey));
    }

    [Fact]
    public void Encrypt_EmptyPlaintext_ReturnsEncryptedBytes()
    {
        var service = new EncryptionService();
        var plaintext = "";
        var key = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef");

        var encrypted = service.Encrypt(plaintext, key);

        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
    }

    [Fact]
    public void Decrypt_EmptyEncryptedBytes_ThrowsException()
    {
        var service = new EncryptionService();
        var key = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef");

        Assert.Throws<Exception>(() => service.Decrypt(Array.Empty<byte>(), key));
    }

    [Fact]
    public void Encrypt_ShortKey_ThrowsException()
    {
        var service = new EncryptionService();
        var plaintext = "data";
        var shortKey = Encoding.UTF8.GetBytes("short");

        Assert.Throws<ArgumentException>(() => service.Encrypt(plaintext, shortKey));
    }
}
