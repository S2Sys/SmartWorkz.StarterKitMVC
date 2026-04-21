namespace SmartWorkz.Core.Shared.FileStorage;

using SmartWorkz.Core.Services.FileStorage;
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

    public async Task<string> UploadAsync(string path, Stream content, FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
            System.IO.Directory.CreateDirectory(directory);

        using (var fileStream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        _logger.LogInformation("File uploaded: {Path}", path);
        return fullPath;
    }

    public async Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, path);

        if (!System.IO.File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {path}");

        var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        return await Task.FromResult(stream);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, path);

        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);

        _logger.LogInformation("File deleted: {Path}", path);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, path);
        return Task.FromResult(System.IO.File.Exists(fullPath));
    }

    public Task<FileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, path);

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

        return Task.FromResult((FileMetadata?)metadata);
    }

    public Task<IReadOnlyCollection<FileMetadata>> ListAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, folderPath);

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

        return Task.FromResult((IReadOnlyCollection<FileMetadata>)files);
    }

    public Task<string> GenerateTemporaryUrlAsync(string path, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        // Local storage doesn't generate URLs; return file path
        var fullPath = Path.Combine(_baseDirectory, path);
        return Task.FromResult(fullPath);
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
