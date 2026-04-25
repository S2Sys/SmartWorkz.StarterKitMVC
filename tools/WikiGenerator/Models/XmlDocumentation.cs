namespace SmartWorkz.Tools.WikiGenerator.Models;

public class XmlDocumentation
{
    public List<XmlTypeInfo> Types { get; set; } = new();
}

public class XmlTypeInfo
{
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Remarks { get; set; } = "";
    public string Example { get; set; } = "";
    public List<XmlMemberInfo> Members { get; set; } = new();
}

public class XmlMemberInfo
{
    public string Name { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Returns { get; set; } = "";
    public List<ParamInfo> Parameters { get; set; } = new();
}

public class ParamInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}
