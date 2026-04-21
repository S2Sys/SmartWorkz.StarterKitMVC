namespace SmartWorkz.Core.Tests.Services.FileStorage;

using SmartWorkz.Core.Services.FileStorage;
using SmartWorkz.Core.Shared.FileStorage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        var mockLogger = new Mock<ILogger<LocalFileStorageService>>();
        _service = new LocalFileStorageService(_testDirectory, mockLogger.Object);
    }

    [Fact]
    public async Task UploadAsync_WithValidStream_CreatesFile()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello World";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var metadata = new FileMetadata { FileName = fileName };

        // Act
        await _service.UploadAsync(fileName, stream, metadata);

        // Assert
        Assert.True(await _service.ExistsAsync(fileName));
    }

    [Fact]
    public async Task DownloadAsync_WithExistingFile_ReturnsStream()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello World";
        var uploadStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _service.UploadAsync(fileName, uploadStream, new FileMetadata { FileName = fileName });

        // Act
        var downloadStream = await _service.DownloadAsync(fileName);

        // Assert
        using (var reader = new StreamReader(downloadStream))
        {
            var downloadedContent = await reader.ReadToEndAsync();
            Assert.Equal(content, downloadedContent);
        }
    }

    [Fact]
    public async Task DeleteAsync_WithExistingFile_RemovesFile()
    {
        // Arrange
        var fileName = "test.txt";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        await _service.UploadAsync(fileName, stream, new FileMetadata { FileName = fileName });

        // Act
        await _service.DeleteAsync(fileName);

        // Assert
        Assert.False(await _service.ExistsAsync(fileName));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }
}
