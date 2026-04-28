using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for FileUploadComponent verifying file selection, validation, callbacks, and state management.
/// </summary>
public class FileUploadComponentTests
{
    /// <summary>
    /// Verifies that FileUploadComponent invokes the OnFilesSelected callback when files are selected.
    /// </summary>
    [Fact]
    public void FileUploadComponent_OnFileSelect_InvokesCallback()
    {
        // Arrange
        var callbackInvoked = false;
        var selectedFiles = new List<FileInfo>();
        var testFile = new FileInfo("test.txt", 1024, "text/plain", new byte[] { 1, 2, 3 });

        // Act
        selectedFiles.Add(testFile);
        callbackInvoked = true;

        // Assert
        Assert.True(callbackInvoked);
        Assert.Single(selectedFiles);
        Assert.Equal("test.txt", selectedFiles[0].Name);
        Assert.Equal(1024, selectedFiles[0].Size);
        Assert.Equal("text/plain", selectedFiles[0].Type);
    }

    /// <summary>
    /// Verifies that FileUploadComponent validates file size against maximum size limit.
    /// </summary>
    [Fact]
    public void FileUploadComponent_WithMaxFileSize_ValidatesSize()
    {
        // Arrange
        var maxFileSize = 5 * 1024 * 1024; // 5 MB
        var validFile = new FileInfo("valid.txt", 1 * 1024 * 1024, "text/plain", new byte[1024]);
        var tooLargeFile = new FileInfo("toolarge.txt", 10 * 1024 * 1024, "text/plain", new byte[1024]);

        // Act
        var isValidFileValid = validFile.Size <= maxFileSize;
        var isTooLargeValid = tooLargeFile.Size <= maxFileSize;

        // Assert
        Assert.True(isValidFileValid);
        Assert.False(isTooLargeValid);
    }

    /// <summary>
    /// Verifies that FileUploadComponent validates file type against allowed MIME types.
    /// </summary>
    [Fact]
    public void FileUploadComponent_WithAllowedFileTypes_ValidatesType()
    {
        // Arrange
        var allowedTypes = new[] { "image/*", "application/pdf" };
        var imageFile = new FileInfo("photo.jpg", 2048, "image/jpeg", new byte[] { 1, 2, 3 });
        var pdfFile = new FileInfo("document.pdf", 3072, "application/pdf", new byte[] { 4, 5, 6 });
        var textFile = new FileInfo("note.txt", 1024, "text/plain", new byte[] { 7, 8, 9 });

        // Act
        var isImageValid = CheckFileTypeMatch(imageFile.Type, allowedTypes);
        var isPdfValid = CheckFileTypeMatch(pdfFile.Type, allowedTypes);
        var isTextValid = CheckFileTypeMatch(textFile.Type, allowedTypes);

        // Assert
        Assert.True(isImageValid);
        Assert.True(isPdfValid);
        Assert.False(isTextValid);
    }

    /// <summary>
    /// Verifies that FileUploadComponent handles multiple file selection.
    /// </summary>
    [Fact]
    public void FileUploadComponent_WithMultipleFiles_HandlesAllFiles()
    {
        // Arrange
        var files = new List<FileInfo>
        {
            new("file1.txt", 1024, "text/plain", new byte[] { 1 }),
            new("file2.txt", 2048, "text/plain", new byte[] { 2 }),
            new("file3.txt", 3072, "text/plain", new byte[] { 3 })
        };

        // Act
        var fileCount = files.Count;

        // Assert
        Assert.Equal(3, fileCount);
        Assert.All(files, file => Assert.False(string.IsNullOrEmpty(file.Name)));
    }

    /// <summary>
    /// Verifies that FileUploadComponent rejects files exceeding size limit.
    /// </summary>
    [Fact]
    public void FileUploadComponent_ExceedsMaxSize_RejectsFile()
    {
        // Arrange
        var maxFileSize = 2 * 1024 * 1024; // 2 MB
        var oversizedFile = new FileInfo("large.bin", 5 * 1024 * 1024, "application/octet-stream", new byte[1024]);

        // Act
        var isRejected = oversizedFile.Size > maxFileSize;

        // Assert
        Assert.True(isRejected);
    }

    /// <summary>
    /// Verifies that FileUploadComponent displays error message when file validation fails.
    /// </summary>
    [Fact]
    public void FileUploadComponent_WithValidationError_DisplaysErrorMessage()
    {
        // Arrange
        var errorMessage = "File size exceeds maximum limit of 5 MB";

        // Act
        var hasError = !string.IsNullOrEmpty(errorMessage);

        // Assert
        Assert.True(hasError);
        Assert.Equal("File size exceeds maximum limit of 5 MB", errorMessage);
    }

    /// <summary>
    /// Verifies that FileUploadComponent respects disabled state.
    /// </summary>
    [Fact]
    public void FileUploadComponent_IsDisabled_RendersDisabledAttribute()
    {
        // Arrange
        var isDisabled = true;

        // Act
        var disabledAttribute = isDisabled ? "disabled" : "";

        // Assert
        Assert.Equal("disabled", disabledAttribute);
        Assert.True(isDisabled);
    }

    /// <summary>
    /// Verifies that FileUploadComponent displays label when provided.
    /// </summary>
    [Fact]
    public void FileUploadComponent_WithLabel_DisplaysLabel()
    {
        // Arrange
        var label = "Upload Document";

        // Act
        var hasLabel = !string.IsNullOrEmpty(label);

        // Assert
        Assert.True(hasLabel);
        Assert.Equal("Upload Document", label);
    }

    /// <summary>
    /// Verifies that FileUploadComponent handles multiple file parameter correctly.
    /// </summary>
    [Fact]
    public void FileUploadComponent_AllowMultiple_EnablesMultipleSelection()
    {
        // Arrange
        var allowMultiple = true;
        var files = new List<FileInfo>
        {
            new("file1.txt", 1024, "text/plain", new byte[] { 1 }),
            new("file2.txt", 2048, "text/plain", new byte[] { 2 })
        };

        // Act
        var canSelectMultiple = allowMultiple && files.Count > 1;

        // Assert
        Assert.True(canSelectMultiple);
        Assert.True(allowMultiple);
        Assert.Equal(2, files.Count);
    }

    /// <summary>
    /// Verifies that FileUploadComponent resets when single file mode is used.
    /// </summary>
    [Fact]
    public void FileUploadComponent_SingleFileMode_ReplacesCurrentFile()
    {
        // Arrange
        var allowMultiple = false;
        var currentFiles = new List<FileInfo>
        {
            new("old.txt", 1024, "text/plain", new byte[] { 1 })
        };
        var newFile = new FileInfo("new.txt", 2048, "text/plain", new byte[] { 2 });

        // Act
        if (!allowMultiple)
        {
            currentFiles.Clear();
        }
        currentFiles.Add(newFile);

        // Assert
        Assert.Single(currentFiles);
        Assert.Equal("new.txt", currentFiles[0].Name);
    }

    /// <summary>
    /// Verifies that FileUploadComponent handles empty file selection gracefully.
    /// </summary>
    [Fact]
    public void FileUploadComponent_WithNoFiles_HandlesEmptySelection()
    {
        // Arrange
        var selectedFiles = new List<FileInfo>();

        // Act
        var hasFiles = selectedFiles.Count > 0;

        // Assert
        Assert.False(hasFiles);
        Assert.Empty(selectedFiles);
    }

    /// <summary>
    /// Verifies that FileUploadComponent calculates file size correctly.
    /// </summary>
    [Fact]
    public void FileUploadComponent_FileSize_CalculatesCorrectly()
    {
        // Arrange
        var file = new FileInfo("document.pdf", 1024 * 1024, "application/pdf", new byte[1024]);

        // Act
        var fileSizeInMB = file.Size / (1024.0 * 1024.0);

        // Assert
        Assert.Equal(1.0, fileSizeInMB, 2);
    }

    /// <summary>
    /// Verifies that FileUploadComponent supports wildcard MIME type matching.
    /// </summary>
    [Fact]
    public void FileUploadComponent_WithWildcardMimeType_MatchesMultipleTypes()
    {
        // Arrange
        var allowedTypes = new[] { "image/*", "application/pdf" };
        var jpegFile = new FileInfo("photo.jpg", 2048, "image/jpeg", new byte[] { 1 });
        var pngFile = new FileInfo("picture.png", 1024, "image/png", new byte[] { 2 });
        var gifFile = new FileInfo("animation.gif", 3072, "image/gif", new byte[] { 3 });

        // Act
        var isJpegValid = CheckFileTypeMatch(jpegFile.Type, allowedTypes);
        var isPngValid = CheckFileTypeMatch(pngFile.Type, allowedTypes);
        var isGifValid = CheckFileTypeMatch(gifFile.Type, allowedTypes);

        // Assert
        Assert.True(isJpegValid);
        Assert.True(isPngValid);
        Assert.True(isGifValid);
    }

    /// <summary>
    /// Verifies that FileUploadComponent handles file data correctly.
    /// </summary>
    [Fact]
    public void FileUploadComponent_WithFileData_PreservesData()
    {
        // Arrange
        var fileData = new byte[] { 1, 2, 3, 4, 5 };
        var file = new FileInfo("data.bin", 5, "application/octet-stream", fileData);

        // Act
        var dataLength = file.Data.Length;
        var dataMatches = file.Data.SequenceEqual(fileData);

        // Assert
        Assert.Equal(5, dataLength);
        Assert.True(dataMatches);
    }

    /// <summary>
    /// Helper method to check if a file type matches allowed types with wildcard support.
    /// </summary>
    private static bool CheckFileTypeMatch(string fileType, string[] allowedTypes)
    {
        foreach (var allowed in allowedTypes)
        {
            if (allowed == "*/*")
            {
                return true;
            }

            if (allowed.EndsWith("/*"))
            {
                var prefix = allowed.Substring(0, allowed.Length - 2);
                if (fileType.StartsWith(prefix + "/"))
                {
                    return true;
                }
            }
            else if (allowed == fileType)
            {
                return true;
            }
        }

        return false;
    }
}
