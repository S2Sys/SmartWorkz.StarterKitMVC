using System.Security.Cryptography;
using System.Text;
using SmartWorkz.Core.Shared.Webhooks.Models;

namespace SmartWorkz.Core.Shared.Tests;

/// <summary>
/// Test suite for webhook signature verification using HMAC-SHA256
/// Covers signing, verification, tampering detection, and edge cases.
/// </summary>
public class WebhookSignatureTests
{
    private const string TestSecret = "test-webhook-secret-key";
    private const string TestPayload = "{\"event\":\"order.created\",\"orderId\":12345}";

    /// <summary>
    /// Helper method to compute HMAC-SHA256 signature
    /// </summary>
    private static string ComputeSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    #region Signature Generation

    [Fact]
    public void ComputeSignature_WithValidPayloadAndSecret_ProducesConsistentHash()
    {
        // Arrange
        var payload = TestPayload;
        var secret = TestSecret;

        // Act
        var signature1 = ComputeSignature(payload, secret);
        var signature2 = ComputeSignature(payload, secret);

        // Assert - Same input should produce same signature
        Assert.Equal(signature1, signature2);
        Assert.NotEmpty(signature1);
    }

    [Fact]
    public void ComputeSignature_WithDifferentPayloads_ProducesDifferentHashes()
    {
        // Arrange
        var payload1 = TestPayload;
        var payload2 = "{\"event\":\"order.cancelled\",\"orderId\":12346}";
        var secret = TestSecret;

        // Act
        var signature1 = ComputeSignature(payload1, secret);
        var signature2 = ComputeSignature(payload2, secret);

        // Assert
        Assert.NotEqual(signature1, signature2);
    }

    [Fact]
    public void ComputeSignature_WithDifferentSecrets_ProducesDifferentHashes()
    {
        // Arrange
        var payload = TestPayload;
        var secret1 = TestSecret;
        var secret2 = "different-secret-key";

        // Act
        var signature1 = ComputeSignature(payload, secret1);
        var signature2 = ComputeSignature(payload, secret2);

        // Assert
        Assert.NotEqual(signature1, signature2);
    }

    [Fact]
    public void ComputeSignature_WithEmptyPayload_StillProducesValidSignature()
    {
        // Arrange
        var payload = "";
        var secret = TestSecret;

        // Act
        var signature = ComputeSignature(payload, secret);

        // Assert
        Assert.NotEmpty(signature);
        Assert.True(signature.Length > 0);
    }

    #endregion

    #region Signature Verification

    [Fact]
    public void VerifySignature_WithMatchingPayloadAndSecret_ReturnsTrue()
    {
        // Arrange
        var payload = TestPayload;
        var secret = TestSecret;
        var expectedSignature = ComputeSignature(payload, secret);

        // Act
        var computedSignature = ComputeSignature(payload, secret);
        var isValid = expectedSignature == computedSignature;

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifySignature_WithTamperedPayload_ReturnsFalse()
    {
        // Arrange
        var originalPayload = TestPayload;
        var tamperedPayload = "{\"event\":\"order.created\",\"orderId\":99999}";
        var secret = TestSecret;
        var expectedSignature = ComputeSignature(originalPayload, secret);

        // Act
        var computedSignature = ComputeSignature(tamperedPayload, secret);
        var isValid = expectedSignature == computedSignature;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_WithIncorrectSecret_ReturnsFalse()
    {
        // Arrange
        var payload = TestPayload;
        var correctSecret = TestSecret;
        var wrongSecret = "wrong-secret";
        var expectedSignature = ComputeSignature(payload, correctSecret);

        // Act
        var computedSignature = ComputeSignature(payload, wrongSecret);
        var isValid = expectedSignature == computedSignature;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_WithCaseDifference_StillMatches()
    {
        // Arrange
        var payload = TestPayload;
        var secret = TestSecret;
        var signature1 = ComputeSignature(payload, secret).ToLowerInvariant();
        var signature2 = ComputeSignature(payload, secret).ToUpperInvariant();

        // Act
        var isValid = signature1.Equals(signature2, StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.True(isValid);
    }

    #endregion

    #region Edge Cases & Security

    [Fact]
    public void ComputeSignature_WithLargePayload_ProducesValidSignature()
    {
        // Arrange
        var largePayload = new string('x', 1000000); // 1MB payload
        var secret = TestSecret;

        // Act
        var signature = ComputeSignature(largePayload, secret);

        // Assert
        Assert.NotEmpty(signature);
        Assert.True(signature.Length > 0);
    }

    [Fact]
    public void ComputeSignature_WithSpecialCharacters_ProducesValidSignature()
    {
        // Arrange
        var payload = "{\"data\":\"special chars: !@#$%^&*()\"}";
        var secret = TestSecret;

        // Act
        var signature = ComputeSignature(payload, secret);

        // Assert
        Assert.NotEmpty(signature);
    }

    [Fact]
    public void ComputeSignature_WithUnicodeCharacters_ProducesValidSignature()
    {
        // Arrange
        var payload = "{\"data\":\"unicode: 你好世界 🌍\"}";
        var secret = TestSecret;

        // Act
        var signature = ComputeSignature(payload, secret);

        // Assert
        Assert.NotEmpty(signature);
    }

    #endregion
}
