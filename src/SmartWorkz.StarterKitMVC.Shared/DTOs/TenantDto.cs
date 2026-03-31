namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public class TenantDto
{
    public string TenantId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTenantDto
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
}

public class UpdateTenantDto
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
}
