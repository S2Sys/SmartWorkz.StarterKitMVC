namespace SmartWorkz.Core.Tests.Security;

using SmartWorkz.Core.Shared.Security;

public class HashHelperTests
{
    #region Sha256 Tests

    [Fact]
    public void Sha256_WithValidText_ReturnsHexHash()
    {
        // Arrange
        const string text = "Hello World";

        // Act
        var result = HashHelper.Sha256(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Data!);
        Assert.Equal(64, result.Data!.Length); // SHA256 is 64 hex characters
        Assert.All(result.Data!, c => Assert.True(char.IsAsciiHexDigit(c)));
    }

    [Fact]
    public void Sha256_WithKnownValue_ReturnsExpectedHash()
    {
        // Arrange
        const string text = "test";
        const string expectedHash = "9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08";

        // Act
        var result = HashHelper.Sha256(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(expectedHash, result.Data, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sha256_WithEmptyString_ReturnsFail()
    {
        // Act
        var result = HashHelper.Sha256("");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Sha256_WithNull_ReturnsFail()
    {
        // Act
        var result = HashHelper.Sha256(null!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Sha256_WithDifferentInputs_ReturnsDifferentHashes()
    {
        // Arrange
        const string text1 = "Hello";
        const string text2 = "World";

        // Act
        var result1 = HashHelper.Sha256(text1);
        var result2 = HashHelper.Sha256(text2);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.NotEqual(result1.Data, result2.Data, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sha256_WithSameInput_ReturnsSameHash()
    {
        // Arrange
        const string text = "ConsistentInput";

        // Act
        var result1 = HashHelper.Sha256(text);
        var result2 = HashHelper.Sha256(text);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.Equal(result1.Data, result2.Data, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sha256_WithUnicode_WorksCorrectly()
    {
        // Arrange
        const string text = "Hello 世界 🌍";

        // Act
        var result = HashHelper.Sha256(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Data!);
        Assert.Equal(64, result.Data!.Length);
    }

    #endregion

    #region Sha256Bytes Tests

    [Fact]
    public void Sha256Bytes_WithValidData_ReturnsByteHash()
    {
        // Arrange
        var data = System.Text.Encoding.UTF8.GetBytes("Hello World");

        // Act
        var result = HashHelper.Sha256Bytes(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(32, result.Data!.Length); // SHA256 is 32 bytes
    }

    [Fact]
    public void Sha256Bytes_WithEmptyArray_ReturnsFail()
    {
        // Act
        var result = HashHelper.Sha256Bytes([]);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Sha256Bytes_WithNull_ReturnsFail()
    {
        // Act
        var result = HashHelper.Sha256Bytes(null!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Sha256Bytes_ConvertingToHex_MatchesSha256()
    {
        // Arrange
        const string text = "test";
        var data = System.Text.Encoding.UTF8.GetBytes(text);

        // Act
        var sha256Result = HashHelper.Sha256(text);
        var sha256BytesResult = HashHelper.Sha256Bytes(data);

        // Assert
        Assert.True(sha256Result.Succeeded);
        Assert.True(sha256BytesResult.Succeeded);
        var hexFromBytes = Convert.ToHexString(sha256BytesResult.Data!);
        Assert.Equal(sha256Result.Data, hexFromBytes, StringComparer.OrdinalIgnoreCase);
    }

    #endregion

    #region Md5 Tests

    [Fact]
    public void Md5_WithValidText_ReturnsHexHash()
    {
        // Arrange
        const string text = "Hello World";

        // Act
        var result = HashHelper.Md5(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Data!);
        Assert.Equal(32, result.Data!.Length); // MD5 is 32 hex characters
        Assert.All(result.Data!, c => Assert.True(char.IsAsciiHexDigit(c)));
    }

    [Fact]
    public void Md5_WithKnownValue_ReturnsExpectedHash()
    {
        // Arrange
        const string text = "test";
        const string expectedHash = "098F6BCD4621D373CADE4E832627B4F6";

        // Act
        var result = HashHelper.Md5(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(expectedHash, result.Data, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Md5_WithEmptyString_ReturnsFail()
    {
        // Act
        var result = HashHelper.Md5("");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Md5_WithNull_ReturnsFail()
    {
        // Act
        var result = HashHelper.Md5(null!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Md5_WithDifferentInputs_ReturnsDifferentHashes()
    {
        // Arrange
        const string text1 = "Hello";
        const string text2 = "World";

        // Act
        var result1 = HashHelper.Md5(text1);
        var result2 = HashHelper.Md5(text2);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.NotEqual(result1.Data, result2.Data, StringComparer.OrdinalIgnoreCase);
    }

    #endregion

    #region VerifyHash Tests

    [Fact]
    public void VerifyHash_WithMatchingHash_ReturnsTrue()
    {
        // Arrange
        const string text = "SecurePassword123";
        var hashResult = HashHelper.Sha256(text);

        // Act
        var verifyResult = HashHelper.VerifyHash(text, hashResult.Data!);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.True(verifyResult.Data);
    }

    [Fact]
    public void VerifyHash_WithNonMatchingHash_ReturnsFalse()
    {
        // Arrange
        const string text = "SecurePassword123";
        const string wrongHash = "0000000000000000000000000000000000000000000000000000000000000000";

        // Act
        var verifyResult = HashHelper.VerifyHash(text, wrongHash);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.False(verifyResult.Data);
    }

    [Fact]
    public void VerifyHash_WithDifferentText_ReturnsFalse()
    {
        // Arrange
        const string originalText = "SecurePassword123";
        const string differentText = "DifferentPassword456";
        var hashResult = HashHelper.Sha256(originalText);

        // Act
        var verifyResult = HashHelper.VerifyHash(differentText, hashResult.Data!);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.False(verifyResult.Data);
    }

    [Fact]
    public void VerifyHash_WithEmptyText_ReturnsFail()
    {
        // Act
        var result = HashHelper.VerifyHash("", "somehash");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void VerifyHash_WithEmptyHash_ReturnsFail()
    {
        // Act
        var result = HashHelper.VerifyHash("sometext", "");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void VerifyHash_IsCaseInsensitive()
    {
        // Arrange
        const string text = "TestData";
        var hashResult = HashHelper.Sha256(text);
        var lowercaseHash = hashResult.Data!.ToLower();

        // Act
        var verifyResult = HashHelper.VerifyHash(text, lowercaseHash);

        // Assert
        Assert.True(verifyResult.Succeeded);
        Assert.True(verifyResult.Data);
    }

    #endregion
}
