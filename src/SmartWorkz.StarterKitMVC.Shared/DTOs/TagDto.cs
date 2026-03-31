namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public class TagDto
{
    public int TagId { get; set; }
    public string TenantId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string EntityType { get; set; }
    public int? EntityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTagDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class UpdateTagDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class TagAssignmentDto
{
    public string EntityType { get; set; }
    public int EntityId { get; set; }
}
