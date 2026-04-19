namespace SmartWorkz.Core.Tests.Utilities;

using SmartWorkz.Core.Shared.Utilities;

public class CompressHelperTests
{
    #region CompressString Tests

    [Fact]
    public void CompressString_WithValidText_ReturnsCompressedBytes()
    {
        // Arrange
        const string text = "Hello World! This is a test string for compression. " +
            "This text needs to be long enough to compress effectively.";

        // Act
        var result = CompressHelper.CompressString(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data!);
        Assert.True(result.Data!.Length < text.Length);
    }

    [Fact]
    public void CompressString_WithEmptyString_ReturnsFail()
    {
        // Act
        var result = CompressHelper.CompressString("");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void CompressString_WithNull_ReturnsFail()
    {
        // Act
        var result = CompressHelper.CompressString(null!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void CompressString_WithLongText_CompressesSignificantly()
    {
        // Arrange
        var text = string.Concat(Enumerable.Repeat("This is a repeated line. ", 100));

        // Act
        var result = CompressHelper.CompressString(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data!.Length < text.Length / 2);
    }

    #endregion

    #region DecompressString Tests

    [Fact]
    public void DecompressString_WithCompressedData_ReturnsOriginalString()
    {
        // Arrange
        const string originalText = "Hello World! This is a test string for compression.";
        var compressed = CompressHelper.CompressBytes(System.Text.Encoding.UTF8.GetBytes(originalText));

        // Act
        var result = CompressHelper.DecompressString(compressed);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(originalText, result.Data);
    }

    [Fact]
    public void DecompressString_WithEmptyData_ReturnsFail()
    {
        // Act
        var result = CompressHelper.DecompressString([]);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void DecompressString_WithNull_ReturnsFail()
    {
        // Act
        var result = CompressHelper.DecompressString(null!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void DecompressString_WithInvalidData_ReturnsFail()
    {
        // Arrange
        var invalidData = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var result = CompressHelper.DecompressString(invalidData);

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion

    #region CompressBytes Tests

    [Fact]
    public void CompressBytes_WithValidData_ReturnsCompressed()
    {
        // Arrange
        var data = System.Text.Encoding.UTF8.GetBytes(
            "Test data for compression. Repeated: " +
            string.Concat(Enumerable.Repeat("compress ", 50)));

        // Act
        var result = CompressHelper.CompressBytes(data);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length < data.Length);
    }

    [Fact]
    public void CompressBytes_WithLargeData_CompressesEffectively()
    {
        // Arrange
        var data = new byte[10000];
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)(i % 256);

        // Act
        var result = CompressHelper.CompressBytes(data);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length < data.Length);
    }

    #endregion

    #region DecompressBytes Tests

    [Fact]
    public void DecompressBytes_WithCompressedData_ReturnsOriginal()
    {
        // Arrange
        var originalData = System.Text.Encoding.UTF8.GetBytes("Test data for compression");
        var compressed = CompressHelper.CompressBytes(originalData);

        // Act
        var result = CompressHelper.DecompressBytes(compressed);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalData, result);
    }

    [Fact]
    public void DecompressBytes_WithInvalidData_ThrowsException()
    {
        // Arrange
        var invalidData = new byte[] { 1, 2, 3, 4, 5 };

        // Act & Assert
        // InvalidDataException is thrown for invalid gzip data
        Assert.Throws<System.IO.InvalidDataException>(() => CompressHelper.DecompressBytes(invalidData));
    }

    #endregion

    #region Round-trip Tests

    [Fact]
    public void CompressDecompress_String_RoundTripSucceeds()
    {
        // Arrange
        const string originalText = "The quick brown fox jumps over the lazy dog. " +
            "This text should compress and decompress correctly.";

        // Act
        var compressResult = CompressHelper.CompressString(originalText);
        var decompressResult = CompressHelper.DecompressString(compressResult.Data!);

        // Assert
        Assert.True(compressResult.Succeeded);
        Assert.True(decompressResult.Succeeded);
        Assert.Equal(originalText, decompressResult.Data);
    }

    [Fact]
    public void CompressDecompress_Bytes_RoundTripSucceeds()
    {
        // Arrange
        var originalData = System.Text.Encoding.UTF8.GetBytes(
            "The quick brown fox jumps over the lazy dog. This is a test.");

        // Act
        var compressed = CompressHelper.CompressBytes(originalData);
        var decompressed = CompressHelper.DecompressBytes(compressed);

        // Assert
        Assert.Equal(originalData, decompressed);
    }

    [Fact]
    public void CompressDecompress_LongText_RoundTripSucceeds()
    {
        // Arrange
        var longText = string.Concat(
            Enumerable.Repeat("Lorem ipsum dolor sit amet, consectetur adipiscing elit. ", 50));

        // Act
        var compressResult = CompressHelper.CompressString(longText);
        var decompressResult = CompressHelper.DecompressString(compressResult.Data!);

        // Assert
        Assert.True(compressResult.Succeeded);
        Assert.True(decompressResult.Succeeded);
        Assert.Equal(longText, decompressResult.Data);
    }

    #endregion
}
