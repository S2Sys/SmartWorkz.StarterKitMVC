namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

using SmartWorkz.Core.Entities;

public class Translation : AuditableEntity<int>
{
    public int LanguageId { get; set; }
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public string FieldName { get; set; }
    public string TranslatedValue { get; set; }
    public bool IsActive { get; set; } = true;
}
