namespace SmartWorkz.Core.Shared.FileStorage;

using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using SmartWorkz.Core.Services.FileStorage;
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

    public async Task<string> UploadAsync(string path, Stream content, FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);

        await blobClient.UploadAsync(content, overwrite: true, cancellationToken);

        if (metadata?.Tags != null)
        {
            await blobClient.SetTagsAsync(metadata.Tags, cancellationToken: cancellationToken);
        }

        _logger.LogInformation("File uploaded to Azure Blob: {Path}", path);
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        var download = await blobClient.DownloadAsync(cancellationToken);
        return download.Value.Content;
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        await blobClient.DeleteAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("File deleted from Azure Blob: {Path}", path);
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        return await blobClient.ExistsAsync(cancellationToken);
    }

    public async Task<FileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);

        if (!await blobClient.ExistsAsync(cancellationToken))
            return null;

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        return new FileMetadata
        {
            Path = path,
            FileName = Path.GetFileName(path),
            SizeBytes = properties.Value.ContentLength,
            ContentType = properties.Value.ContentType,
            CreatedAt = properties.Value.CreatedOn,
            ModifiedAt = properties.Value.LastModified
        };
    }

    public async Task<IReadOnlyCollection<FileMetadata>> ListAsync(string folderPath, CancellationToken cancellationToken = default)
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

        return results;
    }

    public async Task<string> GenerateTemporaryUrlAsync(string path, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);

        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiration));
        return await Task.FromResult(sasUri.ToString());
    }
}
