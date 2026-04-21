namespace SmartWorkz.Core.Tests.Services.FileStorage;

using SmartWorkz.Core;
using SmartWorkz.Shared;
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

    #region Upload Tests

    [Fact]
    public async Task UploadAsync_WithValidStream_CreatesFile()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello World";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var metadata = new FileMetadata { Path = fileName, FileName = fileName };

        // Act
        var result = await _service.UploadAsync(fileName, stream, metadata);

        // Assert
        Assert.True(await _service.ExistsAsync(fileName));
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task UploadAsync_WithNullPath_ThrowsArgumentException()
    {
        // Arrange
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        var metadata = new FileMetadata { Path = "test.txt", FileName = "test.txt" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UploadAsync(null, stream, metadata));
    }

    [Fact]
    public async Task UploadAsync_WithEmptyPath_ThrowsArgumentException()
    {
        // Arrange
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        var metadata = new FileMetadata { Path = "test.txt", FileName = "test.txt" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UploadAsync("", stream, metadata));
    }

    [Fact]
    public async Task UploadAsync_WithPathTraversal_ThrowsArgumentException()
    {
        // Arrange
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        var metadata = new FileMetadata { Path = "../../../etc/passwd", FileName = "passwd" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UploadAsync("../../../etc/passwd", stream, metadata));
    }

    [Fact]
    public async Task UploadAsync_WithNullStream_ThrowsArgumentNullException()
    {
        // Arrange
        var metadata = new FileMetadata { Path = "test.txt", FileName = "test.txt" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.UploadAsync("test.txt", null, metadata));
    }

    #endregion

    #region Download Tests

    [Fact]
    public async Task DownloadAsync_WithExistingFile_ReturnsStream()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello World";
        var uploadStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _service.UploadAsync(fileName, uploadStream, new FileMetadata { Path = fileName, FileName = fileName });

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
    public async Task DownloadAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _service.DownloadAsync("nonexistent.txt"));
    }

    [Fact]
    public async Task DownloadAsync_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.DownloadAsync(null));
    }

    [Fact]
    public async Task DownloadAsync_WithPathTraversal_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.DownloadAsync("../../../etc/passwd"));
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_WithExistingFile_RemovesFile()
    {
        // Arrange
        var fileName = "test.txt";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        await _service.UploadAsync(fileName, stream, new FileMetadata { Path = fileName, FileName = fileName });

        // Act
        await _service.DeleteAsync(fileName);

        // Assert
        Assert.False(await _service.ExistsAsync(fileName));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentFile_DoesNotThrow()
    {
        // Act & Assert - should not throw
        await _service.DeleteAsync("nonexistent.txt");
    }

    [Fact]
    public async Task DeleteAsync_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.DeleteAsync(null));
    }

    [Fact]
    public async Task DeleteAsync_WithPathTraversal_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.DeleteAsync("../../../etc/passwd"));
    }

    #endregion

    #region Exists Tests

    [Fact]
    public async Task ExistsAsync_WithExistingFile_ReturnsTrue()
    {
        // Arrange
        var fileName = "test.txt";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        await _service.UploadAsync(fileName, stream, new FileMetadata { Path = fileName, FileName = fileName });

        // Act
        var exists = await _service.ExistsAsync(fileName);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentFile_ReturnsFalse()
    {
        // Act
        var exists = await _service.ExistsAsync("nonexistent.txt");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ExistsAsync(null));
    }

    [Fact]
    public async Task ExistsAsync_WithPathTraversal_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ExistsAsync("../../../etc/passwd"));
    }

    #endregion

    #region GetMetadata Tests

    [Fact]
    public async Task GetMetadataAsync_WithExistingFile_ReturnsMetadata()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello World";
        var uploadStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _service.UploadAsync(fileName, uploadStream, new FileMetadata { Path = fileName, FileName = fileName });

        // Act
        var metadata = await _service.GetMetadataAsync(fileName);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal(fileName, metadata.FileName);
        Assert.Equal(fileName, metadata.Path);
        Assert.Equal(content.Length, metadata.SizeBytes);
        Assert.NotNull(metadata.ContentType);
    }

    [Fact]
    public async Task GetMetadataAsync_WithNonExistentFile_ReturnsNull()
    {
        // Act
        var metadata = await _service.GetMetadataAsync("nonexistent.txt");

        // Assert
        Assert.Null(metadata);
    }

    [Fact]
    public async Task GetMetadataAsync_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetMetadataAsync(null));
    }

    [Fact]
    public async Task GetMetadataAsync_WithPathTraversal_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetMetadataAsync("../../../etc/passwd"));
    }

    #endregion

    #region List Tests

    [Fact]
    public async Task ListAsync_WithFilesInFolder_ReturnsMetadata()
    {
        // Arrange
        var folderPath = "testfolder";
        Directory.CreateDirectory(Path.Combine(_testDirectory, folderPath));

        var file1 = Path.Combine(folderPath, "file1.txt");
        var file2 = Path.Combine(folderPath, "file2.txt");

        await _service.UploadAsync(file1, new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content1")), new FileMetadata { Path = file1, FileName = "file1.txt" });
        await _service.UploadAsync(file2, new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content2")), new FileMetadata { Path = file2, FileName = "file2.txt" });

        // Act
        var files = await _service.ListAsync(folderPath);

        // Assert
        Assert.Equal(2, files.Count);
        Assert.Contains(files, f => f.FileName == "file1.txt");
        Assert.Contains(files, f => f.FileName == "file2.txt");
    }

    [Fact]
    public async Task ListAsync_WithEmptyFolder_ReturnsEmptyCollection()
    {
        // Arrange
        var folderPath = "emptyfolder";
        Directory.CreateDirectory(Path.Combine(_testDirectory, folderPath));

        // Act
        var files = await _service.ListAsync(folderPath);

        // Assert
        Assert.Empty(files);
    }

    [Fact]
    public async Task ListAsync_WithNonExistentFolder_ReturnsEmptyCollection()
    {
        // Act
        var files = await _service.ListAsync("nonexistentfolder");

        // Assert
        Assert.Empty(files);
    }

    [Fact]
    public async Task ListAsync_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ListAsync(null));
    }

    [Fact]
    public async Task ListAsync_WithPathTraversal_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ListAsync("../../../etc"));
    }

    #endregion

    #region GenerateTemporaryUrl Tests

    [Fact]
    public async Task GenerateTemporaryUrlAsync_WithValidPath_ReturnsFullPath()
    {
        // Arrange
        var fileName = "test.txt";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        await _service.UploadAsync(fileName, stream, new FileMetadata { Path = fileName, FileName = fileName });

        // Act
        var url = await _service.GenerateTemporaryUrlAsync(fileName, TimeSpan.FromHours(1));

        // Assert
        Assert.NotEmpty(url);
        Assert.True(url.Contains(fileName));
    }

    [Fact]
    public async Task GenerateTemporaryUrlAsync_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GenerateTemporaryUrlAsync(null, TimeSpan.FromHours(1)));
    }

    [Fact]
    public async Task GenerateTemporaryUrlAsync_WithPathTraversal_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GenerateTemporaryUrlAsync("../../../etc/passwd", TimeSpan.FromHours(1)));
    }

    #endregion

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }
}

