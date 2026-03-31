namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

public class Tag
{
    public int TagId { get; set; }
    public string TagName { get; set; }
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
