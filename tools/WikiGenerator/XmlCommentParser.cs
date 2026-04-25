using System.Xml.Linq;
using SmartWorkz.Tools.WikiGenerator.Models;

namespace SmartWorkz.Tools.WikiGenerator;

class XmlCommentParser
{
    private readonly string _logLevel;

    public XmlCommentParser(string logLevel)
    {
        _logLevel = logLevel;
    }

    public XmlDocumentation ParseXmlFile(string xmlPath)
    {
        var doc = new XmlDocumentation();

        try
        {
            var xdoc = XDocument.Load(xmlPath);
            var members = xdoc.Descendants("member").ToList();

            // First pass: extract all types
            var typeMembers = members.Where(m => m.Attribute("name")?.Value.StartsWith("T:") ?? false).ToList();

            foreach (var member in typeMembers)
            {
                var nameAttr = member.Attribute("name")?.Value ?? "";
                var fullName = nameAttr.Substring(2); // Remove "T:" prefix

                var type = new TypeInfo
                {
                    FullName = fullName,
                    Name = fullName.Split('.').Last(),
                    Summary = ExtractText(member, "summary"),
                    Remarks = ExtractText(member, "remarks"),
                    Example = ExtractText(member, "example")
                };

                doc.Types.Add(type);
            }

            // Second pass: extract methods and assign to types
            var methodMembers = members.Where(m => m.Attribute("name")?.Value.StartsWith("M:") ?? false).ToList();

            foreach (var type in doc.Types)
            {
                foreach (var methodMember in methodMembers)
                {
                    var nameAttr = methodMember.Attribute("name")?.Value ?? "";
                    var methodFullName = nameAttr.Substring(2); // Remove "M:" prefix

                    // Check if method belongs to this type
                    if (methodFullName.StartsWith(type.FullName + "."))
                    {
                        var methodName = methodFullName.Substring(type.FullName.Length + 1);
                        // Extract just the method name (before parentheses)
                        methodName = methodName.Split('(')[0];

                        var memberInfo = new MemberInfo
                        {
                            Name = methodName,
                            Summary = ExtractText(methodMember, "summary"),
                            Returns = ExtractText(methodMember, "returns")
                        };

                        // Parse parameters
                        foreach (var param in methodMember.Descendants("param"))
                        {
                            var paramName = param.Attribute("name")?.Value ?? "";
                            memberInfo.Parameters.Add(new ParamInfo
                            {
                                Name = paramName,
                                Description = param.Value.Trim()
                            });
                        }

                        type.Members.Add(memberInfo);
                    }
                }
            }

            if (_logLevel == "info")
                Console.WriteLine($"[Parser] Extracted {doc.Types.Count} types from {Path.GetFileName(xmlPath)}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Error] Failed to parse {xmlPath}: {ex.Message}");
        }

        return doc;
    }

    private string ExtractText(XElement element, string tagName)
    {
        var elem = element.Element(tagName);
        return elem?.Value.Trim() ?? "";
    }
}
