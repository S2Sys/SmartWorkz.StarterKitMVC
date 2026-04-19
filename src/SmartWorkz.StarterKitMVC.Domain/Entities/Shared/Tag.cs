namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

using SmartWorkz.Core.Entities;

public class Tag : AuditableEntity<int>
{
    public string TagName { get; set; }
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public bool IsActive { get; set; } = true;
}
