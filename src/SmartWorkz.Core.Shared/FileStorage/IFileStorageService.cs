namespace SmartWorkz.Shared;

/// <summary>
/// Interface for file storage operations supporting both local and cloud providers.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage.
    /// </summary>
    /// <param name="path">The relative path or blob name for the file.</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="metadata">The file metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The full path or URI of the uploaded file.</returns>
    Task<string> UploadAsync(string path, Stream content, FileMetadata metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from storage.
    /// </summary>
    /// <param name="path">The relative path or blob name for the file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A stream containing the file content. Caller must dispose using 'using' statement.</returns>
    /// <remarks>
    /// IMPORTANT: The returned stream must be disposed by the caller using a 'using' statement or by explicitly calling Dispose().
    /// Example usage:
    /// <code>
    /// using (var stream = await fileStorageService.DownloadAsync("path/to/file"))
    /// {
    ///     // Use stream
    /// }
    /// </code>
    /// </remarks>
    Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="path">The relative path or blob name for the file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists in storage.
    /// </summary>
    /// <param name="path">The relative path or blob name for the file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata for a file in storage.
    /// </summary>
    /// <param name="path">The relative path or blob name for the file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>FileMetadata if file exists, null otherwise.</returns>
    Task<FileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists files in a directory or container prefix.
    /// </summary>
    /// <param name="folderPath">The relative folder path or blob prefix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only collection of FileMetadata for files in the directory/prefix.</returns>
    Task<IReadOnlyCollection<FileMetadata>> ListAsync(string folderPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a temporary download URL for a file.
    /// </summary>
    /// <param name="path">The relative path or blob name for the file.</param>
    /// <param name="expiration">The expiration duration from now.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A URL that can be used to download the file. For local storage, returns the full file path.</returns>
    Task<string> GenerateTemporaryUrlAsync(string path, TimeSpan expiration, CancellationToken cancellationToken = default);
}
