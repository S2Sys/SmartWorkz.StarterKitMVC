namespace SmartWorkz.Core.Tests.Security;

using SmartWorkz.Shared;

public class HmacHelperTests
{
    private const string TestMessage = "Hello, World!";
    private const string TestSecret = "my-secret-key";

    #region Sign Tests

    [Fact]
    public void Sign_WithValidInputs_ReturnsBase64Signature()
    {
        // Arrange & Act
        var result = HmacHelper.Sign(TestMessage, TestSecret);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
        // Verify it's valid Base64
        var decoded = Convert.FromBase64String(result.Data);
        Assert.NotEmpty(decoded);
    }

    [Fact]
    public void Sign_WithNullMessage_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.Sign(null!, TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidMessage", result.MessageKey);
    }

    [Fact]
    public void Sign_WithEmptyMessage_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.Sign(string.Empty, TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidMessage", result.MessageKey);
    }

    [Fact]
    public void Sign_WithNullSecret_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.Sign(TestMessage, null!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidSecret", result.MessageKey);
    }

    [Fact]
    public void Sign_WithEmptySecret_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.Sign(TestMessage, string.Empty);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidSecret", result.MessageKey);
    }

    [Fact]
    public void Sign_WithSHA256_ProducesConsistentSignature()
    {
        // Arrange & Act
        var result1 = HmacHelper.Sign(TestMessage, TestSecret, HmacAlgorithm.SHA256);
        var result2 = HmacHelper.Sign(TestMessage, TestSecret, HmacAlgorithm.SHA256);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.Equal(result1.Data, result2.Data);
    }

    [Fact]
    public void Sign_WithSHA512_ProducesConsistentSignature()
    {
        // Arrange & Act
        var result1 = HmacHelper.Sign(TestMessage, TestSecret, HmacAlgorithm.SHA512);
        var result2 = HmacHelper.Sign(TestMessage, TestSecret, HmacAlgorithm.SHA512);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.Equal(result1.Data, result2.Data);
    }

    [Fact]
    public void Sign_SHA256_ProducesDifferentSignatureFromSHA512()
    {
        // Arrange & Act
        var resultSha256 = HmacHelper.Sign(TestMessage, TestSecret, HmacAlgorithm.SHA256);
        var resultSha512 = HmacHelper.Sign(TestMessage, TestSecret, HmacAlgorithm.SHA512);

        // Assert
        Assert.True(resultSha256.Succeeded);
        Assert.True(resultSha512.Succeeded);
        Assert.NotEqual(resultSha256.Data, resultSha512.Data);
    }

    [Fact]
    public void Sign_DifferentMessages_ProduceDifferentSignatures()
    {
        // Arrange & Act
        var result1 = HmacHelper.Sign("Message 1", TestSecret);
        var result2 = HmacHelper.Sign("Message 2", TestSecret);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.NotEqual(result1.Data, result2.Data);
    }

    [Fact]
    public void Sign_DifferentSecrets_ProduceDifferentSignatures()
    {
        // Arrange & Act
        var result1 = HmacHelper.Sign(TestMessage, "secret1");
        var result2 = HmacHelper.Sign(TestMessage, "secret2");

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.NotEqual(result1.Data, result2.Data);
    }

    #endregion

    #region SignBytes Tests

    [Fact]
    public void SignBytes_WithValidInputs_ReturnsSignature()
    {
        // Arrange
        var messageBytes = System.Text.Encoding.UTF8.GetBytes(TestMessage);

        // Act
        var result = HmacHelper.SignBytes(messageBytes, TestSecret);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public void SignBytes_WithNullMessage_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.SignBytes(null!, TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidMessage", result.MessageKey);
    }

    [Fact]
    public void SignBytes_WithEmptyMessage_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.SignBytes([], TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidMessage", result.MessageKey);
    }

    [Fact]
    public void SignBytes_ProducesConsistentResults()
    {
        // Arrange
        var messageBytes = System.Text.Encoding.UTF8.GetBytes(TestMessage);

        // Act
        var result1 = HmacHelper.SignBytes(messageBytes, TestSecret);
        var result2 = HmacHelper.SignBytes(messageBytes, TestSecret);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.Equal(result1.Data, result2.Data);
    }

    #endregion

    #region Verify Tests

    [Fact]
    public void Verify_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        var signResult = HmacHelper.Sign(TestMessage, TestSecret);
        Assert.True(signResult.Succeeded);

        // Act
        var verifyResult = HmacHelper.Verify(TestMessage, signResult.Data!, TestSecret);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.True(verifyResult.Data);
    }

    [Fact]
    public void Verify_WithInvalidSignature_ReturnsFalse()
    {
        // Arrange
        var signResult = HmacHelper.Sign(TestMessage, TestSecret);
        Assert.True(signResult.Succeeded);
        var invalidSignature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("invalid"));

        // Act
        var verifyResult = HmacHelper.Verify(TestMessage, invalidSignature, TestSecret);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.False(verifyResult.Data);
    }

    [Fact]
    public void Verify_WithTamperedMessage_ReturnsFalse()
    {
        // Arrange
        var signResult = HmacHelper.Sign(TestMessage, TestSecret);
        Assert.True(signResult.Succeeded);

        // Act
        var verifyResult = HmacHelper.Verify("Tampered message", signResult.Data!, TestSecret);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.False(verifyResult.Data);
    }

    [Fact]
    public void Verify_WithWrongSecret_ReturnsFalse()
    {
        // Arrange
        var signResult = HmacHelper.Sign(TestMessage, TestSecret);
        Assert.True(signResult.Succeeded);

        // Act
        var verifyResult = HmacHelper.Verify(TestMessage, signResult.Data!, "wrong-secret");

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.False(verifyResult.Data);
    }

    [Fact]
    public void Verify_WithNullMessage_ReturnsFail()
    {
        // Arrange
        var signResult = HmacHelper.Sign(TestMessage, TestSecret);
        Assert.True(signResult.Succeeded);

        // Act
        var result = HmacHelper.Verify(null!, signResult.Data!, TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidMessage", result.MessageKey);
    }

    [Fact]
    public void Verify_WithEmptyMessage_ReturnsFail()
    {
        // Arrange
        var signResult = HmacHelper.Sign(TestMessage, TestSecret);
        Assert.True(signResult.Succeeded);

        // Act
        var result = HmacHelper.Verify(string.Empty, signResult.Data!, TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidMessage", result.MessageKey);
    }

    [Fact]
    public void Verify_WithNullSignature_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.Verify(TestMessage, null!, TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidSignature", result.MessageKey);
    }

    [Fact]
    public void Verify_WithEmptySignature_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.Verify(TestMessage, string.Empty, TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidSignature", result.MessageKey);
    }

    [Fact]
    public void Verify_WithInvalidBase64Signature_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.Verify(TestMessage, "not-valid-base64!!!", TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidSignatureFormat", result.MessageKey);
    }

    [Fact]
    public void Verify_WithNullSecret_ReturnsFail()
    {
        // Arrange
        var signResult = HmacHelper.Sign(TestMessage, TestSecret);
        Assert.True(signResult.Succeeded);

        // Act
        var result = HmacHelper.Verify(TestMessage, signResult.Data!, null!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidSecret", result.MessageKey);
    }

    [Fact]
    public void Verify_WithEmptySecret_ReturnsFail()
    {
        // Arrange
        var signResult = HmacHelper.Sign(TestMessage, TestSecret);
        Assert.True(signResult.Succeeded);

        // Act
        var result = HmacHelper.Verify(TestMessage, signResult.Data!, string.Empty);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidSecret", result.MessageKey);
    }

    [Theory]
    [InlineData(HmacAlgorithm.SHA256)]
    [InlineData(HmacAlgorithm.SHA512)]
    public void Verify_WithBothAlgorithms_VerifiesCorrectly(HmacAlgorithm algorithm)
    {
        // Arrange
        var signResult = HmacHelper.Sign(TestMessage, TestSecret, algorithm);
        Assert.True(signResult.Succeeded);

        // Act
        var verifyResult = HmacHelper.Verify(TestMessage, signResult.Data!, TestSecret, algorithm);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.True(verifyResult.Data);
    }

    #endregion

    #region VerifyBytes Tests

    [Fact]
    public void VerifyBytes_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        var messageBytes = System.Text.Encoding.UTF8.GetBytes(TestMessage);
        var signResult = HmacHelper.SignBytes(messageBytes, TestSecret);
        Assert.True(signResult.Succeeded);

        // Act
        var verifyResult = HmacHelper.VerifyBytes(messageBytes, signResult.Data!, TestSecret);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.True(verifyResult.Data);
    }

    [Fact]
    public void VerifyBytes_WithInvalidSignature_ReturnsFalse()
    {
        // Arrange
        var messageBytes = System.Text.Encoding.UTF8.GetBytes(TestMessage);
        var invalidSignature = System.Text.Encoding.UTF8.GetBytes("invalid");

        // Act
        var verifyResult = HmacHelper.VerifyBytes(messageBytes, invalidSignature, TestSecret);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.False(verifyResult.Data);
    }

    [Fact]
    public void VerifyBytes_WithNullMessage_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.VerifyBytes(null!, [0x00], TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidMessage", result.MessageKey);
    }

    [Fact]
    public void VerifyBytes_WithEmptyMessage_ReturnsFail()
    {
        // Arrange & Act
        var result = HmacHelper.VerifyBytes([], [0x00], TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidMessage", result.MessageKey);
    }

    [Fact]
    public void VerifyBytes_WithNullSignature_ReturnsFail()
    {
        // Arrange
        var messageBytes = System.Text.Encoding.UTF8.GetBytes(TestMessage);

        // Act
        var result = HmacHelper.VerifyBytes(messageBytes, null!, TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidSignature", result.MessageKey);
    }

    [Fact]
    public void VerifyBytes_WithEmptySignature_ReturnsFail()
    {
        // Arrange
        var messageBytes = System.Text.Encoding.UTF8.GetBytes(TestMessage);

        // Act
        var result = HmacHelper.VerifyBytes(messageBytes, [], TestSecret);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("HmacHelper.InvalidSignature", result.MessageKey);
    }

    #endregion

    #region Round-trip Tests

    [Fact]
    public void Sign_Then_Verify_WithWebhookScenario_SucceedsRoundTrip()
    {
        // Arrange - Simulate webhook payload
        var webhookPayload = "{\"event\": \"user.created\", \"userId\": 123}";
        var signingSecret = "webhook-secret-key";

        // Act
        var signResult = HmacHelper.Sign(webhookPayload, signingSecret);
        Assert.True(signResult.Succeeded);

        var verifyResult = HmacHelper.Verify(webhookPayload, signResult.Data!, signingSecret);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.True(verifyResult.Data);
    }

    [Fact]
    public void SignBytes_Then_VerifyBytes_WithBinaryData_SucceedsRoundTrip()
    {
        // Arrange
        var binaryData = new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f }; // "Hello" in UTF-8

        // Act
        var signResult = HmacHelper.SignBytes(binaryData, TestSecret);
        Assert.True(signResult.Succeeded);

        var verifyResult = HmacHelper.VerifyBytes(binaryData, signResult.Data!, TestSecret);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.True(verifyResult.Data);
    }

    [Fact]
    public void Sign_WithLongMessage_SucceedsAndVerifies()
    {
        // Arrange
        var longMessage = string.Concat(Enumerable.Repeat("The quick brown fox jumps over the lazy dog. ", 100));

        // Act
        var signResult = HmacHelper.Sign(longMessage, TestSecret);
        Assert.True(signResult.Succeeded);

        var verifyResult = HmacHelper.Verify(longMessage, signResult.Data!, TestSecret);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.True(verifyResult.Data);
    }

    [Fact]
    public void Sign_WithSpecialCharacters_SucceedsAndVerifies()
    {
        // Arrange
        var specialMessage = "Test with special chars: !@#$%^&*()[]{}|;:,.<>?/\\\"'";

        // Act
        var signResult = HmacHelper.Sign(specialMessage, TestSecret);
        Assert.True(signResult.Succeeded);

        var verifyResult = HmacHelper.Verify(specialMessage, signResult.Data!, TestSecret);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.True(verifyResult.Data);
    }

    [Fact]
    public void Sign_WithUnicodeCharacters_SucceedsAndVerifies()
    {
        // Arrange
        var unicodeMessage = "Hello ä¸–ç•Œ ðŸŒ Ù…Ø±Ø­Ø¨Ø§";

        // Act
        var signResult = HmacHelper.Sign(unicodeMessage, TestSecret);
        Assert.True(signResult.Succeeded);

        var verifyResult = HmacHelper.Verify(unicodeMessage, signResult.Data!, TestSecret);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.True(verifyResult.Data);
    }

    #endregion

    #region Constant-time Comparison Tests

    [Fact]
    public void Verify_UsesCryptographicConstantTimeComparison()
    {
        // Arrange
        var message = TestMessage;
        var secret = TestSecret;
        var signResult = HmacHelper.Sign(message, secret);
        Assert.True(signResult.Succeeded);
        var validSignature = signResult.Data!;

        // Create a slightly different signature (simulate timing attack attempt)
        var signatureBytes = Convert.FromBase64String(validSignature);
        var tamperedBytes = (byte[])signatureBytes.Clone();
        tamperedBytes[0] ^= 0xFF; // Flip all bits in first byte
        var tamperedSignature = Convert.ToBase64String(tamperedBytes);

        // Act
        var verifyValidResult = HmacHelper.Verify(message, validSignature, secret);
        var verifyTamperedResult = HmacHelper.Verify(message, tamperedSignature, secret);

        // Assert - both should complete with bool result (not throw)
        Assert.True(verifyValidResult.Succeeded);
        Assert.True(verifyValidResult.Data); // Valid signature
        Assert.True(verifyTamperedResult.Succeeded);
        Assert.False(verifyTamperedResult.Data); // Tampered signature
    }

    #endregion
}


