using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SmartWorkz.StarterKitMVC.Tools.DocGenerator;

/// <summary>
/// Generates markdown documentation from XML comments in C# source files.
/// </summary>
/// <example>
/// <code>
/// // Run from command line:
/// // dotnet run --project tools/DocGenerator -- --source src --output docs/api-reference.md
/// 
/// // Or use programmatically:
/// var generator = new DocumentationGenerator();
/// generator.GenerateFromDirectory("src", "docs/api-reference.md");
/// </code>
/// </example>
public class DocumentationGenerator
{
    private readonly StringBuilder _output = new();

    /// <summary>
    /// Generates documentation from all C# files in a directory.
    /// </summary>
    /// <param name="sourceDirectory">Source directory containing .cs files.</param>
    /// <param name="outputPath">Output markdown file path.</param>
    public void GenerateFromDirectory(string sourceDirectory, string outputPath)
    {
        _output.Clear();
        _output.AppendLine("# SmartWorkz.StarterKitMVC API Reference");
        _output.AppendLine();
        _output.AppendLine("*Auto-generated from XML documentation comments*");
        _output.AppendLine();
        _output.AppendLine("---");
        _output.AppendLine();

        var files = Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("obj") && !f.Contains("bin"))
            .OrderBy(f => f);

        var currentNamespace = "";

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var docs = ExtractDocumentation(content, file);
            
            if (docs.Any())
            {
                var ns = ExtractNamespace(content);
                if (ns != currentNamespace)
                {
                    currentNamespace = ns;
                    _output.AppendLine($"## {ns}");
                    _output.AppendLine();
                }

                foreach (var doc in docs)
                {
                    _output.AppendLine(doc);
                    _output.AppendLine();
                }
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, _output.ToString());
        Console.WriteLine($"Documentation generated: {outputPath}");
    }

    private string ExtractNamespace(string content)
    {
        var match = Regex.Match(content, @"namespace\s+([\w.]+)");
        return match.Success ? match.Groups[1].Value : "Unknown";
    }

    private IEnumerable<string> ExtractDocumentation(string content, string filePath)
    {
        var results = new List<string>();
        
        // Match XML doc comments followed by type/method declarations
        var pattern = @"(///.*?(?=\s*(?:public|internal|private|protected)))(\s*(?:public|internal)\s+(?:sealed\s+|abstract\s+|static\s+|readonly\s+)*(?:class|interface|record|struct|enum)\s+(\w+)|\s*(?:public|internal)\s+(?:static\s+|async\s+|virtual\s+|override\s+)*(?:\w+(?:<[^>]+>)?(?:\?)?)\s+(\w+)\s*(?:<[^>]+>)?\s*\()";
        
        var matches = Regex.Matches(content, pattern, RegexOptions.Singleline);
        
        foreach (Match match in matches)
        {
            var xmlComment = match.Groups[1].Value;
            var typeName = match.Groups[3].Value;
            var methodName = match.Groups[4].Value;
            var name = !string.IsNullOrEmpty(typeName) ? typeName : methodName;
            
            if (string.IsNullOrEmpty(name)) continue;

            var sb = new StringBuilder();
            sb.AppendLine($"### `{name}`");
            sb.AppendLine();

            // Extract summary
            var summaryMatch = Regex.Match(xmlComment, @"<summary>\s*(.*?)\s*</summary>", RegexOptions.Singleline);
            if (summaryMatch.Success)
            {
                var summary = CleanXmlContent(summaryMatch.Groups[1].Value);
                sb.AppendLine(summary);
                sb.AppendLine();
            }

            // Extract parameters
            var paramMatches = Regex.Matches(xmlComment, @"<param name=""(\w+)"">\s*(.*?)\s*</param>", RegexOptions.Singleline);
            if (paramMatches.Count > 0)
            {
                sb.AppendLine("**Parameters:**");
                foreach (Match pm in paramMatches)
                {
                    sb.AppendLine($"- `{pm.Groups[1].Value}`: {CleanXmlContent(pm.Groups[2].Value)}");
                }
                sb.AppendLine();
            }

            // Extract returns
            var returnsMatch = Regex.Match(xmlComment, @"<returns>\s*(.*?)\s*</returns>", RegexOptions.Singleline);
            if (returnsMatch.Success)
            {
                sb.AppendLine($"**Returns:** {CleanXmlContent(returnsMatch.Groups[1].Value)}");
                sb.AppendLine();
            }

            // Extract example
            var exampleMatch = Regex.Match(xmlComment, @"<example>\s*<code>\s*(.*?)\s*</code>\s*</example>", RegexOptions.Singleline);
            if (exampleMatch.Success)
            {
                sb.AppendLine("**Example:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(CleanXmlContent(exampleMatch.Groups[1].Value));
                sb.AppendLine("```");
            }

            results.Add(sb.ToString());
        }

        return results;
    }

    private string CleanXmlContent(string content)
    {
        // Remove /// prefixes and clean up whitespace
        var lines = content.Split('\n')
            .Select(l => Regex.Replace(l.Trim(), @"^///\s*", ""))
            .Where(l => !string.IsNullOrWhiteSpace(l));
        return string.Join("\n", lines);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var sourceDir = args.Length > 1 && args[0] == "--source" ? args[1] : "src";
        var outputPath = args.Length > 3 && args[2] == "--output" ? args[3] : "docs/api-reference.md";

        var generator = new DocumentationGenerator();
        generator.GenerateFromDirectory(sourceDir, outputPath);
    }
}
