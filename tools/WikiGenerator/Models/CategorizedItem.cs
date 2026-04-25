namespace SmartWorkz.Tools.WikiGenerator.Models;

internal class CategorizedItem
{
    public XmlTypeInfo Type { get; set; } = new();
    public string Platform { get; set; } = "";
    public string Layer { get; set; } = "";
    public string Feature { get; set; } = "";
    public string ProjectName { get; set; } = "";
}
