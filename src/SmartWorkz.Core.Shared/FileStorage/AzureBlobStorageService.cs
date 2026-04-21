namespace SmartWorkz.Shared;

using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(BlobContainerClient containerClient, ILogger<AzureBlobStorageService> logger)
    {
        _containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Uploads a file to Azure Blob Storage.
    /// </summary>
    /// <param name="path">The blob path/name.</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="metadata">The file metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The URI of the uploaded blob.</returns>
    /// <exception cref="ArgumentException">Thrown if path is null or empty.</exception>
    public async Task<string> UploadAsync(string path, Stream content, FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        if (content == null)
            throw new ArgumentNullException(nameof(content));

        try
        {
            var blobClient = _containerClient.GetBlobClient(path);

            await blobClient.UploadAsync(content, overwrite: true, cancellationToken);

            if (metadata?.Tags != null)
            {
                await blobClient.SetTagsAsync(metadata.Tags, cancellationToken: cancellationToken);
            }

            _logger.LogInformation("File uploaded successfully to Azure Blob: {Path}", path);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Azure Blob: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Downloads a file from Azure Blob Storage.
    /// </summary>
    /// <param name="path">The blob path/name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A stream containing the blob content. The caller must dispose the stream.</returns>
    /// <remarks>Caller must dispose the returned stream using 'using' statement or call Dispose().</remarks>
    /// <exception cref="ArgumentException">Thrown if path is null or empty.</exception>
    /// <exception cref="Azure.RequestFailedException">Thrown if blob does not exist.</exception>
    public async Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        try
        {
            var blobClient = _containerClient.GetBlobClient(path);
            var download = await blobClient.DownloadAsync(cancellationToken);
            _logger.LogInformation("File downloaded from Azure Blob: {Path}", path);
            return download.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from Azure Blob: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Deletes a file from Azure Blob Storage.
    /// </summary>
    /// <param name="path">The blob path/name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown if path is null or empty.</exception>
    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        try
        {
            var blobClient = _containerClient.GetBlobClient(path);
            await blobClient.DeleteAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("File deleted successfully from Azure Blob: {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Azure Blob: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Checks if a file exists in Azure Blob Storage.
    /// </summary>
    /// <param name="path">The blob path/name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if blob exists, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if path is null or empty.</exception>
    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        try
        {
            var blobClient = _containerClient.GetBlobClient(path);
            return await blobClient.ExistsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking blob existence: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Gets metadata for a file in Azure Blob Storage.
    /// </summary>
    /// <param name="path">The blob path/name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>FileMetadata if blob exists, null otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if path is null or empty.</exception>
    public async Task<FileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        try
        {
            var blobClient = _containerClient.GetBlobClient(path);

            if (!await blobClient.ExistsAsync(cancellationToken))
                return null;

            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            var metadata = new FileMetadata
            {
                Path = path,
                FileName = Path.GetFileName(path),
                SizeBytes = properties.Value.ContentLength,
                ContentType = properties.Value.ContentType,
                CreatedAt = properties.Value.CreatedOn,
                ModifiedAt = properties.Value.LastModified
            };

            _logger.LogInformation("Blob metadata retrieved: {Path}", path);
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blob metadata: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Lists blobs in a container prefix (folder).
    /// </summary>
    /// <param name="folderPath">The prefix (folder path) to list blobs from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of FileMetadata for blobs with the given prefix.</returns>
    /// <exception cref="ArgumentException">Thrown if folderPath is null or empty.</exception>
    public async Task<IReadOnlyCollection<FileMetadata>> ListAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Folder path cannot be null or empty", nameof(folderPath));

        try
        {
            var results = new List<FileMetadata>();

            await foreach (var blob in _containerClient.GetBlobsAsync(prefix: folderPath, cancellationToken: cancellationToken))
            {
                results.Add(new FileMetadata
                {
                    Path = blob.Name,
                    FileName = Path.GetFileName(blob.Name),
                    SizeBytes = blob.Properties.ContentLength ?? 0,
                    ContentType = blob.Properties.ContentType,
                    CreatedAt = blob.Properties.CreatedOn ?? DateTimeOffset.UtcNow,
                    ModifiedAt = blob.Properties.LastModified ?? DateTimeOffset.UtcNow
                });
            }

            _logger.LogInformation("Container prefix listed: {FolderPath}, Blobs: {BlobCount}", folderPath, results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing blobs with prefix: {FolderPath}", folderPath);
            throw;
        }
    }

    /// <summary>
    /// Generates a temporary SAS URI for downloading a blob.
    /// </summary>
    /// <param name="path">The blob path/name.</param>
    /// <param name="expiration">The expiration duration from now.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A SAS URI that can be used to download the blob.</returns>
    /// <exception cref="ArgumentException">Thrown if path is null or empty.</exception>
    public async Task<string> GenerateTemporaryUrlAsync(string path, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        try
        {
            var blobClient = _containerClient.GetBlobClient(path);

            var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiration));
            _logger.LogInformation("Temporary SAS URI generated for blob: {Path}", path);
            return await Task.FromResult(sasUri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating temporary SAS URI: {Path}", path);
            throw;
        }
    }
}
