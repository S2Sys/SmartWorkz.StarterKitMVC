namespace SmartWorkz.Core.Services.FileStorage;

public interface IFileStorageService
{
    /// <summary>Upload a file to storage.</summary>
    Task<string> UploadAsync(string path, Stream content, FileMetadata metadata, CancellationToken cancellationToken = default);

    /// <summary>Download a file from storage.</summary>
    Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Delete a file from storage.</summary>
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Check if file exists.</summary>
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Get file metadata (size, modified date, etc).</summary>
    Task<FileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>List files in a directory/folder.</summary>
    Task<IReadOnlyCollection<FileMetadata>> ListAsync(string folderPath, CancellationToken cancellationToken = default);

    /// <summary>Generate a temporary download URL (for cloud providers).</summary>
    Task<string> GenerateTemporaryUrlAsync(string path, TimeSpan expiration, CancellationToken cancellationToken = default);
}
