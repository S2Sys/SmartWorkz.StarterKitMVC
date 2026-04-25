using System.Text.Json;

namespace SmartWorkz.Tools.WikiGenerator;

class GeneratorConfig
{
    public Dictionary<string, ProjectMapping> ProjectMappings { get; set; } = new();
    public Dictionary<string, string> FeaturePatterns { get; set; } = new();
    public string OutputPath { get; set; } = "docs/framework";
    public List<string> IncludeDllPatterns { get; set; } = new();
    public List<string> ExcludeDllPatterns { get; set; } = new();

    public static GeneratorConfig LoadFromFile(string configPath)
    {
        if (!File.Exists(configPath))
            throw new FileNotFoundException($"Config file not found: {configPath}");

        var json = File.ReadAllText(configPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var config = new GeneratorConfig();

        // Load projectMappings
        if (root.TryGetProperty("projectMappings", out var projectMappingsEl))
        {
            foreach (var prop in projectMappingsEl.EnumerateObject())
            {
                var mapping = new ProjectMapping
                {
                    Platform = prop.Value.GetProperty("platform").GetString() ?? "unknown",
                    Layers = new List<string>()
                };

                if (prop.Value.TryGetProperty("layers", out var layersEl))
                {
                    foreach (var layer in layersEl.EnumerateArray())
                    {
                        mapping.Layers.Add(layer.GetString() ?? "");
                    }
                }

                config.ProjectMappings[prop.Name] = mapping;
            }
        }

        // Load featurePatterns
        if (root.TryGetProperty("featurePatterns", out var featureEl))
        {
            foreach (var prop in featureEl.EnumerateObject())
            {
                config.FeaturePatterns[prop.Name] = prop.Value.GetString() ?? "";
            }
        }

        if (root.TryGetProperty("outputPath", out var outputEl))
            config.OutputPath = outputEl.GetString() ?? "docs/framework";

        if (root.TryGetProperty("includeDllPatterns", out var includeEl))
        {
            foreach (var item in includeEl.EnumerateArray())
            {
                config.IncludeDllPatterns.Add(item.GetString() ?? "");
            }
        }

        if (root.TryGetProperty("excludeDllPatterns", out var excludeEl))
        {
            foreach (var item in excludeEl.EnumerateArray())
            {
                config.ExcludeDllPatterns.Add(item.GetString() ?? "");
            }
        }

        return config;
    }
}

class ProjectMapping
{
    public string Platform { get; set; } = "unknown";
    public List<string> Layers { get; set; } = new();
}
