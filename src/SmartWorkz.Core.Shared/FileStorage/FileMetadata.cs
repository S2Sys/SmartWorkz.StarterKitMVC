namespace SmartWorkz.Core.Services.FileStorage;

public class FileMetadata
{
    public required string Path { get; set; }
    public required string FileName { get; set; }
    public long SizeBytes { get; set; }
    public string? ContentType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}
