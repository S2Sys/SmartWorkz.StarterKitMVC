namespace SmartWorkz.Mobile;

/// <summary>
/// Represents the result of a file operation (photo/video capture or selection).
/// </summary>
public class FileResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileResult"/> class.
    /// </summary>
    /// <param name="fullPath">The full file path to the result file.</param>
    public FileResult(string fullPath)
    {
        Guard.NotNullOrWhiteSpace(fullPath, nameof(fullPath));
        FullPath = fullPath;
    }

    /// <summary>
    /// Gets the full file system path to the file.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// Gets the file name with extension.
    /// </summary>
    public string FileName => Path.GetFileName(FullPath);

    /// <summary>
    /// Gets the content type (MIME type) based on file extension.
    /// </summary>
    public string? ContentType => GetContentType(FullPath);

    /// <summary>
    /// Gets the content type based on file extension.
    /// </summary>
    private static string? GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".mkv" => "video/x-matroska",
            ".webm" => "video/webm",
            _ => null
        };
    }
}
