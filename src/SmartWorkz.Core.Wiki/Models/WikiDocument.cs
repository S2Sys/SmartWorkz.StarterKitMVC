namespace SmartWorkz.Core.Wiki.Models;

public class WikiDocument
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string RelativePath { get; set; } = "";
    public string? Description { get; set; }
}
