namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public class MenuDto
{
    public int MenuId { get; set; }
    public string TenantId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MenuItemDto
{
    public int MenuItemId { get; set; }
    public int MenuId { get; set; }
    public string Label { get; set; }
    public string Url { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateMenuDto
{
    public string Name { get; set; }
    public string Slug { get; set; }
}

public class UpdateMenuDto
{
    public string Name { get; set; }
    public string Slug { get; set; }
}

public class CreateMenuItemDto
{
    public string Label { get; set; }
    public string Url { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateMenuItemDto
{
    public string Label { get; set; }
    public string Url { get; set; }
    public int DisplayOrder { get; set; }
}
