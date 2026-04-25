namespace SmartWorkz.Tools.WikiGenerator.Models;

public class XmlDocumentation
{
    public List<TypeInfo> Types { get; set; } = new();
}

public class TypeInfo
{
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Remarks { get; set; } = "";
    public string Example { get; set; } = "";
    public List<MemberInfo> Members { get; set; } = new();
}

public class MemberInfo
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
