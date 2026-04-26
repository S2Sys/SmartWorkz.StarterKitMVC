using System.Text.RegularExpressions;
using SmartWorkz.Tools.WikiGenerator.Models;

namespace SmartWorkz.Tools.WikiGenerator;

internal class Categorizer
{
    private readonly GeneratorConfig _config;
    private readonly string _logLevel;

    public Categorizer(GeneratorConfig config, string logLevel)
    {
        _config = config;
        _logLevel = logLevel;
    }

    public List<CategorizedItem> Categorize(string projectName, XmlDocumentation xmlDocs)
    {
        var result = new List<CategorizedItem>();

        // Get project mapping
        if (!_config.ProjectMappings.TryGetValue(projectName, out var mapping))
        {
            if (_logLevel == "warning" || _logLevel == "info")
                Console.WriteLine($"[Warning] No mapping for project: {projectName}");
            return result;
        }

        int skippedCount = 0;
        foreach (var type in xmlDocs.Types)
        {
            var feature = DetermineFeature(type.FullName);
            var layer = DetermineLayer(type.FullName, mapping.Layers);

            if (string.IsNullOrEmpty(layer))
            {
                skippedCount++;
                if (_logLevel == "info")
                    Console.WriteLine($"[Categorizer] Skipping {type.FullName} (no matching layer from {string.Join(", ", mapping.Layers)})");
                continue;
            }

            result.Add(new CategorizedItem
            {
                Type = type,
                Platform = mapping.Platform,
                Layer = layer,
                Feature = feature,
                ProjectName = projectName
            });
        }

        if (_logLevel == "info")
            Console.WriteLine($"[Categorizer] {projectName}: Processed {xmlDocs.Types.Count} types, Categorized {result.Count}, Skipped {skippedCount}");

        return result;
    }

    private string DetermineFeature(string fullName)
    {
        foreach (var (featureName, pattern) in _config.FeaturePatterns)
        {
            try
            {
                if (Regex.IsMatch(fullName, pattern, RegexOptions.IgnoreCase))
                    return featureName;
            }
            catch (RegexParseException ex)
            {
                Console.Error.WriteLine($"[Error] Invalid pattern for feature '{featureName}': {ex.Message}");
            }
        }

        // Default feature based on last namespace segment
        var segments = fullName.Split('.');
        var lastSegment = segments.Length > 1 ? segments[^2] : "Other";
        return lastSegment;
    }

    private string DetermineLayer(string fullName, List<string> availableLayers)
    {
        var segments = fullName.Split('.');
        foreach (var segment in segments)
        {
            foreach (var layer in availableLayers)
            {
                if (segment.Equals(layer, StringComparison.OrdinalIgnoreCase))
                    return layer;
            }
        }

        // Default to first available layer
        return availableLayers.FirstOrDefault() ?? "";
    }
}
