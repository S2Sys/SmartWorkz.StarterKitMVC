namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

using SmartWorkz.Core.Entities;

public class FileStorage : AuditableEntity<int>
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public string MimeType { get; set; }
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public string UploadedBy { get; set; }
    public bool IsActive { get; set; } = true;
}
