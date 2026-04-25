using System.Text;
using SmartWorkz.Tools.WikiGenerator.Models;

namespace SmartWorkz.Tools.WikiGenerator;

internal class MarkdownGenerator
{
    private readonly string _outputPath;
    private readonly string _logLevel;

    public MarkdownGenerator(string outputPath, string logLevel)
    {
        _outputPath = outputPath;
        _logLevel = logLevel;
    }

    public void GenerateApiReference(List<CategorizedItem> items)
    {
        var grouped = items.GroupBy(x => new { x.Platform, x.Layer, x.Feature });

        foreach (var group in grouped)
        {
            var dirPath = Path.Combine(_outputPath, group.Key.Platform, group.Key.Layer.ToLower(),
                group.Key.Feature.ToLower().Replace(" ", "-"));
            Directory.CreateDirectory(dirPath);

            var filePath = Path.Combine(dirPath, "api-reference.md");
            var content = GenerateApiReferenceContent(group.Key.Feature, group.ToList());

            File.WriteAllText(filePath, content);
            if (_logLevel == "info")
                Console.WriteLine($"[Generated] {filePath}");
        }
    }

    public void GenerateUsageGuides(List<CategorizedItem> items)
    {
        var grouped = items.GroupBy(x => new { x.Platform, x.Layer, x.Feature });

        foreach (var group in grouped)
        {
            var dirPath = Path.Combine(_outputPath, group.Key.Platform, group.Key.Layer.ToLower(),
                group.Key.Feature.ToLower().Replace(" ", "-"));
            Directory.CreateDirectory(dirPath);

            var filePath = Path.Combine(dirPath, "guide.md");
            var content = GenerateUsageGuideContent(group.Key.Feature, group.ToList());

            File.WriteAllText(filePath, content);
            if (_logLevel == "info")
                Console.WriteLine($"[Generated] {filePath}");
        }
    }

    private string GenerateApiReferenceContent(string feature, List<CategorizedItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {feature} API Reference\n");
        sb.AppendLine("## Classes & Interfaces\n");

        foreach (var item in items)
        {
            sb.AppendLine($"### {item.Type.Name}\n");
            sb.AppendLine($"- **Namespace:** `{item.Type.FullName}`");

            if (!string.IsNullOrEmpty(item.Type.Summary))
                sb.AppendLine($"- **Summary:** {item.Type.Summary}");

            if (!string.IsNullOrEmpty(item.Type.Example))
                sb.AppendLine($"- **Example:**\n```csharp\n{item.Type.Example}\n```");

            if (item.Type.Members.Any())
            {
                sb.AppendLine("\n#### Methods & Properties\n");
                foreach (var member in item.Type.Members)
                {
                    sb.AppendLine($"- **{member.Name}** - {member.Summary}");
                    if (member.Parameters.Any())
                    {
                        sb.AppendLine("  - Parameters:");
                        foreach (var param in member.Parameters)
                        {
                            sb.AppendLine($"    - `{param.Name}`: {param.Description}");
                        }
                    }
                    if (!string.IsNullOrEmpty(member.Returns))
                        sb.AppendLine($"  - Returns: {member.Returns}");
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GenerateUsageGuideContent(string feature, List<CategorizedItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {feature} Usage Guide\n");

        var summary = items.FirstOrDefault()?.Type.Summary ?? $"Framework guide for {feature}";
        sb.AppendLine($"## Overview\n\n{summary}\n");

        var hasExamples = items.Any(x => !string.IsNullOrEmpty(x.Type.Example));
        if (hasExamples)
        {
            sb.AppendLine("## Examples\n");
            foreach (var item in items.Where(x => !string.IsNullOrEmpty(x.Type.Example)))
            {
                sb.AppendLine($"### {item.Type.Name}\n");
                sb.AppendLine($"```csharp\n{item.Type.Example}\n```\n");
            }
        }

        sb.AppendLine("## API Reference\n");
        sb.AppendLine("See [API Reference](./api-reference.md) for complete documentation.\n");

        return sb.ToString();
    }
}
