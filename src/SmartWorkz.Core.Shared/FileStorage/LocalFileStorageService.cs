namespace SmartWorkz.Shared;

using Microsoft.Extensions.Logging;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseDirectory;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(string baseDirectory, ILogger<LocalFileStorageService> logger)
    {
        _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        System.IO.Directory.CreateDirectory(_baseDirectory);
    }

    /// <summary>
    /// Uploads a file to local storage.
    /// </summary>
    /// <param name="path">The relative path within the base directory.</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="metadata">The file metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The full path to the uploaded file.</returns>
    /// <exception cref="ArgumentException">Thrown if path is null/empty or contains path traversal attempt.</exception>
    public async Task<string> UploadAsync(string path, Stream content, FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        ValidatePath(path);

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, path));

            // Verify path is within base directory (prevents path traversal)
            if (!fullPath.StartsWith(_baseDirectory))
                throw new ArgumentException("Path traversal not allowed", nameof(path));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
                System.IO.Directory.CreateDirectory(directory);

            using (var fileStream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }

            _logger.LogInformation("File uploaded successfully: {Path}", path);
            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Downloads a file from local storage.
    /// </summary>
    /// <param name="path">The relative path within the base directory.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A FileStream that must be disposed by the caller using a 'using' statement.</returns>
    /// <remarks>Caller must dispose the returned stream using 'using' statement or call Dispose().</remarks>
    /// <exception cref="ArgumentException">Thrown if path is null/empty or contains path traversal attempt.</exception>
    /// <exception cref="FileNotFoundException">Thrown if file does not exist.</exception>
    public async Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        ValidatePath(path);

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, path));

            // Verify path is within base directory (prevents path traversal)
            if (!fullPath.StartsWith(_baseDirectory))
                throw new ArgumentException("Path traversal not allowed", nameof(path));

            if (!System.IO.File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {path}");

            var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            return await Task.FromResult(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Deletes a file from local storage.
    /// </summary>
    /// <param name="path">The relative path within the base directory.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown if path is null/empty or contains path traversal attempt.</exception>
    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        ValidatePath(path);

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, path));

            // Verify path is within base directory (prevents path traversal)
            if (!fullPath.StartsWith(_baseDirectory))
                throw new ArgumentException("Path traversal not allowed", nameof(path));

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            _logger.LogInformation("File deleted successfully: {Path}", path);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Checks if a file exists in local storage.
    /// </summary>
    /// <param name="path">The relative path within the base directory.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if file exists, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if path is null/empty or contains path traversal attempt.</exception>
    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        ValidatePath(path);

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, path));

            // Verify path is within base directory (prevents path traversal)
            if (!fullPath.StartsWith(_baseDirectory))
                throw new ArgumentException("Path traversal not allowed", nameof(path));

            return Task.FromResult(System.IO.File.Exists(fullPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Gets metadata for a file in local storage.
    /// </summary>
    /// <param name="path">The relative path within the base directory.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>FileMetadata if file exists, null otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if path is null/empty or contains path traversal attempt.</exception>
    public Task<FileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken = default)
    {
        ValidatePath(path);

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, path));

            // Verify path is within base directory (prevents path traversal)
            if (!fullPath.StartsWith(_baseDirectory))
                throw new ArgumentException("Path traversal not allowed", nameof(path));

            if (!System.IO.File.Exists(fullPath))
                return Task.FromResult((FileMetadata?)null);

            var fileInfo = new System.IO.FileInfo(fullPath);
            var metadata = new FileMetadata
            {
                Path = path,
                FileName = fileInfo.Name,
                SizeBytes = fileInfo.Length,
                ContentType = GetContentType(path),
                CreatedAt = fileInfo.CreationTimeUtc,
                ModifiedAt = fileInfo.LastWriteTimeUtc
            };

            _logger.LogInformation("File metadata retrieved: {Path}", path);
            return Task.FromResult((FileMetadata?)metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file metadata: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Lists files in a directory.
    /// </summary>
    /// <param name="folderPath">The relative folder path within the base directory.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of FileMetadata for files in the folder.</returns>
    /// <exception cref="ArgumentException">Thrown if folderPath is null/empty or contains path traversal attempt.</exception>
    public Task<IReadOnlyCollection<FileMetadata>> ListAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        ValidatePath(folderPath);

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, folderPath));

            // Verify path is within base directory (prevents path traversal)
            if (!fullPath.StartsWith(_baseDirectory))
                throw new ArgumentException("Path traversal not allowed", nameof(folderPath));

            if (!System.IO.Directory.Exists(fullPath))
                return Task.FromResult((IReadOnlyCollection<FileMetadata>)new List<FileMetadata>());

            var files = System.IO.Directory.GetFiles(fullPath)
                .Select(f => new System.IO.FileInfo(f))
                .Select(fi => new FileMetadata
                {
                    Path = Path.Combine(folderPath, fi.Name),
                    FileName = fi.Name,
                    SizeBytes = fi.Length,
                    ContentType = GetContentType(fi.Name),
                    CreatedAt = fi.CreationTimeUtc,
                    ModifiedAt = fi.LastWriteTimeUtc
                })
                .ToList();

            _logger.LogInformation("Folder listed: {FolderPath}, Files: {FileCount}", folderPath, files.Count);
            return Task.FromResult((IReadOnlyCollection<FileMetadata>)files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files in folder: {FolderPath}", folderPath);
            throw;
        }
    }

    /// <summary>
    /// Generates a temporary URL for downloading a file (returns full path for local storage).
    /// </summary>
    /// <param name="path">The relative path within the base directory.</param>
    /// <param name="expiration">The expiration duration (not used for local storage).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The full path to the file.</returns>
    /// <exception cref="ArgumentException">Thrown if path is null/empty or contains path traversal attempt.</exception>
    public Task<string> GenerateTemporaryUrlAsync(string path, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        ValidatePath(path);

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, path));

            // Verify path is within base directory (prevents path traversal)
            if (!fullPath.StartsWith(_baseDirectory))
                throw new ArgumentException("Path traversal not allowed", nameof(path));

            return Task.FromResult(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating temporary URL: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Validates that the path is not null or empty.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <exception cref="ArgumentException">Thrown if path is null or empty.</exception>
    private static void ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
