namespace SmartWorkz.Core.Wiki.Models;

public class WikiCategory
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public List<WikiDocument> Documents { get; set; } = new();
}
