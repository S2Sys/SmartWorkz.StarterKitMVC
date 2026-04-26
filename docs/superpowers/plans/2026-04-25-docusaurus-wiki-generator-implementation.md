# Docusaurus Wiki Generator Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build and integrate a Docusaurus wiki system that auto-generates API Reference and Usage Guides from DLL XML comments, reorganizes docs into Plan/Framework groups, and integrates into the build process via MSBuild.

**Architecture:** Create a C# console tool (WikiGenerator) that parses compiled DLL XML documentation, categorizes by platform/layer/feature, generates markdown files organized into Framework (auto) and Plan (migrated) groups. Integrate via MSBuild post-build task and configure Docusaurus to serve the unified documentation site.

**Tech Stack:** .NET 8 (WikiGenerator), Node.js 18+ (Docusaurus 3), MSBuild (integration), Git (version control)

**Worktree:** Use isolated git worktree for this implementation

---

## Phase 1: WikiGenerator Project Setup

### Task 1: Create WikiGenerator Console Project

**Files:**
- Create: `tools/WikiGenerator/WikiGenerator.csproj`
- Create: `tools/WikiGenerator/Program.cs`
- Create: `tools/WikiGenerator/config.json`
- Create: `tools/WikiGenerator/README.md`

- [ ] **Step 1: Create project directory and .csproj file**

```bash
mkdir -p tools/WikiGenerator
```

Create `tools/WikiGenerator/WikiGenerator.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Reflection.Metadata" Version="8.0.0" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create minimal Program.cs with argument parsing**

Create `tools/WikiGenerator/Program.cs`:
```csharp
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Generate wiki documentation from DLL XML comments");

        var outputOption = new Option<string>(
            new[] { "--output", "-o" },
            description: "Output directory for generated wiki files",
            getDefaultValue: () => "docs"
        );

        var configOption = new Option<string>(
            new[] { "--config", "-c" },
            description: "Path to config.json file",
            getDefaultValue: () => "tools/WikiGenerator/config.json"
        );

        var logLevelOption = new Option<string>(
            new[] { "--log-level" },
            description: "Logging level (info, warning, error)",
            getDefaultValue: () => "info"
        );

        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(logLevelOption);

        rootCommand.SetHandler(async (output, config, logLevel) =>
        {
            var generator = new WikiGenerator(output, config, logLevel);
            await generator.GenerateAsync();
        }, outputOption, configOption, logLevelOption);

        return await rootCommand.InvokeAsync(args);
    }
}

class WikiGenerator
{
    private readonly string _outputPath;
    private readonly string _configPath;
    private readonly string _logLevel;

    public WikiGenerator(string outputPath, string configPath, string logLevel)
    {
        _outputPath = outputPath;
        _configPath = configPath;
        _logLevel = logLevel;
    }

    public async Task GenerateAsync()
    {
        Console.WriteLine("WikiGenerator initialized");
        Console.WriteLine($"Output: {_outputPath}");
        Console.WriteLine($"Config: {_configPath}");
        Console.WriteLine($"Log Level: {_logLevel}");
        
        // Placeholder for implementation
        await Task.CompletedTask;
    }
}
```

- [ ] **Step 3: Add System.CommandLine NuGet package**

```bash
cd tools/WikiGenerator
dotnet add package System.CommandLine --version 2.0.0-beta4.22272.1
cd ../..
```

- [ ] **Step 4: Create config.json template**

Create `tools/WikiGenerator/config.json`:
```json
{
  "projectMappings": {
    "SmartWorkz.Core.Mobile": {
      "platform": "mobile",
      "layers": ["Core", "Infrastructure", "Application"]
    },
    "SmartWorkz.Core.Web": {
      "platform": "web",
      "layers": ["Core", "Infrastructure", "Application"]
    },
    "SmartWorkz.Core.Shared": {
      "platform": "shared",
      "layers": ["Core"]
    },
    "SmartWorkz.StarterKitMVC": {
      "platform": "web",
      "layers": ["Application", "Infrastructure"]
    },
    "SmartWorkz.Core.External": {
      "platform": "shared",
      "layers": ["Core"]
    },
    "SmartWorkz.Sample.ECommerce": {
      "platform": "web",
      "layers": ["Application"]
    },
    "SmartWorkz.Sample.ECommerce.Mobile": {
      "platform": "mobile",
      "layers": ["Application"]
    }
  },
  "featurePatterns": {
    "Database": ".*\\.Data\\.|.*\\.Repository\\.|.*\\.Migrations",
    "Caching": ".*\\.Caching",
    "Logging": ".*\\.Logging",
    "State Management": ".*\\.State|.*\\.Redux",
    "Http Client": ".*\\.Http",
    "Services": ".*\\.Services",
    "ViewModels": ".*\\.ViewModels",
    "Commands": ".*\\.Commands|.*\\.Queries",
    "Webhooks": ".*\\.Webhooks",
    "Authentication": ".*\\.Auth|.*\\.Identity",
    "Models": ".*\\.Models"
  },
  "outputPath": "docs/framework",
  "includeDllPatterns": [
    "**/bin/**/SmartWorkz*.dll",
    "**/bin/**/SmartWorkz*.pdb"
  ],
  "excludeDllPatterns": [
    "**/*Tests.dll",
    "**/bin/Debug/**"
  ]
}
```

- [ ] **Step 5: Create README for WikiGenerator**

Create `tools/WikiGenerator/README.md`:
```markdown
# WikiGenerator

Generates wiki documentation from C# DLL XML comments.

## Usage

```bash
dotnet run -- --output docs --config tools/WikiGenerator/config.json --log-level info
```

## Options

- `--output, -o`: Output directory (default: docs)
- `--config, -c`: Config file path (default: tools/WikiGenerator/config.json)
- `--log-level`: Logging level (info, warning, error; default: info)

## Configuration

See `config.json` for project mappings and feature patterns.
```

- [ ] **Step 6: Test project builds**

```bash
cd tools/WikiGenerator
dotnet build
cd ../..
```

Expected: Build succeeds with no errors.

- [ ] **Step 7: Commit**

```bash
git add tools/WikiGenerator/
git commit -m "feat(wiki): create WikiGenerator project structure with CLI"
```

---

## Phase 2: DLL Scanning & XML Parsing

### Task 2: Implement DLL Scanner

**Files:**
- Create: `tools/WikiGenerator/DllScanner.cs`
- Modify: `tools/WikiGenerator/Program.cs` (call scanner)

- [ ] **Step 1: Create DllScanner class**

Create `tools/WikiGenerator/DllScanner.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class DllScanner
{
    private readonly GeneratorConfig _config;
    private readonly string _logLevel;

    public DllScanner(GeneratorConfig config, string logLevel)
    {
        _config = config;
        _logLevel = logLevel;
    }

    public List<(string DllPath, string XmlPath)> ScanForDllsAndXml(string rootPath = ".")
    {
        var result = new List<(string, string)>();
        var rootDir = new DirectoryInfo(rootPath);

        var allDlls = rootDir.GetFiles("*.dll", SearchOption.AllDirectories)
            .Where(f => MatchesPattern(f.FullName, _config.IncludeDllPatterns))
            .Where(f => !MatchesPattern(f.FullName, _config.ExcludeDllPatterns))
            .ToList();

        foreach (var dll in allDlls)
        {
            var xmlPath = Path.ChangeExtension(dll.FullName, ".xml");
            if (File.Exists(xmlPath))
            {
                result.Add((dll.FullName, xmlPath));
                if (_logLevel == "info")
                    Console.WriteLine($"Found: {dll.Name} → {Path.GetFileName(xmlPath)}");
            }
            else
            {
                if (_logLevel == "warning" || _logLevel == "info")
                    Console.WriteLine($"Warning: XML not found for {dll.Name}");
            }
        }

        return result;
    }

    private bool MatchesPattern(string filePath, List<string> patterns)
    {
        return patterns.Any(pattern =>
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(filePath);
        });
    }
}
```

- [ ] **Step 2: Create GeneratorConfig class**

Create `tools/WikiGenerator/GeneratorConfig.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

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
```

- [ ] **Step 3: Update Program.cs to use DllScanner**

Update `tools/WikiGenerator/Program.cs`:
```csharp
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Generate wiki documentation from DLL XML comments");

        var outputOption = new Option<string>(
            new[] { "--output", "-o" },
            description: "Output directory for generated wiki files",
            getDefaultValue: () => "docs"
        );

        var configOption = new Option<string>(
            new[] { "--config", "-c" },
            description: "Path to config.json file",
            getDefaultValue: () => "tools/WikiGenerator/config.json"
        );

        var logLevelOption = new Option<string>(
            new[] { "--log-level" },
            description: "Logging level (info, warning, error)",
            getDefaultValue: () => "info"
        );

        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(logLevelOption);

        rootCommand.SetHandler(async (output, config, logLevel) =>
        {
            var generator = new WikiGenerator(output, config, logLevel);
            await generator.GenerateAsync();
        }, outputOption, configOption, logLevelOption);

        return await rootCommand.InvokeAsync(args);
    }
}

class WikiGenerator
{
    private readonly string _outputPath;
    private readonly string _configPath;
    private readonly string _logLevel;

    public WikiGenerator(string outputPath, string configPath, string logLevel)
    {
        _outputPath = outputPath;
        _configPath = configPath;
        _logLevel = logLevel;
    }

    public async Task GenerateAsync()
    {
        try
        {
            if (_logLevel == "info")
                Console.WriteLine("[WikiGenerator] Starting generation...");

            // Load config
            var config = GeneratorConfig.LoadFromFile(_configPath);
            if (_logLevel == "info")
                Console.WriteLine($"[Config] Loaded {config.ProjectMappings.Count} project mappings");

            // Scan for DLLs
            var scanner = new DllScanner(config, _logLevel);
            var dllsAndXml = scanner.ScanForDllsAndXml(".");

            if (_logLevel == "info")
                Console.WriteLine($"[Scanner] Found {dllsAndXml.Count} DLL+XML pairs");

            Console.WriteLine("[WikiGenerator] Generation complete");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Error] {ex.Message}");
            if (_logLevel == "info")
                Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }

        await Task.CompletedTask;
    }
}
```

- [ ] **Step 4: Test DllScanner**

```bash
cd tools/WikiGenerator
dotnet run -- --log-level info --config config.json
cd ../..
```

Expected: Output shows "Found X DLL+XML pairs" where X > 0.

- [ ] **Step 5: Commit**

```bash
git add tools/WikiGenerator/
git commit -m "feat(wiki): implement DLL scanner and config loading"
```

---

### Task 3: Implement XML Parser

**Files:**
- Create: `tools/WikiGenerator/XmlCommentParser.cs`
- Create: `tools/WikiGenerator/Models/XmlDocumentation.cs`

- [ ] **Step 1: Create XmlDocumentation models**

Create `tools/WikiGenerator/Models/XmlDocumentation.cs`:
```csharp
using System;
using System.Collections.Generic;

namespace WikiGenerator.Models
{
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
}
```

- [ ] **Step 2: Create XmlCommentParser class**

Create `tools/WikiGenerator/XmlCommentParser.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using WikiGenerator.Models;

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

            foreach (var member in members)
            {
                var nameAttr = member.Attribute("name")?.Value ?? "";
                
                // Only process types (T:) and methods (M:)
                if (!nameAttr.StartsWith("T:") && !nameAttr.StartsWith("M:"))
                    continue;

                if (nameAttr.StartsWith("T:"))
                {
                    var type = new TypeInfo
                    {
                        FullName = nameAttr.Substring(2),
                        Name = nameAttr.Substring(2).Split('.').Last(),
                        Summary = ExtractText(member, "summary"),
                        Remarks = ExtractText(member, "remarks"),
                        Example = ExtractText(member, "example")
                    };
                    doc.Types.Add(type);
                }
            }

            // Parse members (methods/properties)
            foreach (var type in doc.Types)
            {
                var typeNamespace = type.FullName.Substring(0, type.FullName.LastIndexOf('.'));
                foreach (var member in members)
                {
                    var nameAttr = member.Attribute("name")?.Value ?? "";
                    if (!nameAttr.StartsWith("M:"))
                        continue;

                    var methodName = nameAttr.Substring(2);
                    if (methodName.StartsWith(type.FullName + "."))
                    {
                        var memberInfo = new MemberInfo
                        {
                            Name = methodName.Split('.').Last().Split('(')[0],
                            Summary = ExtractText(member, "summary"),
                            Returns = ExtractText(member, "returns")
                        };

                        // Parse parameters
                        foreach (var param in member.Descendants("param"))
                        {
                            memberInfo.Parameters.Add(new ParamInfo
                            {
                                Name = param.Attribute("name")?.Value ?? "",
                                Description = param.Value
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
```

- [ ] **Step 3: Test XmlCommentParser**

```bash
cd tools/WikiGenerator
dotnet build
cd ../..
```

Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add tools/WikiGenerator/
git commit -m "feat(wiki): implement XML comment parser"
```

---

### Task 4: Implement Categorization Logic

**Files:**
- Create: `tools/WikiGenerator/Categorizer.cs`
- Create: `tools/WikiGenerator/Models/CategorizedItem.cs`

- [ ] **Step 1: Create CategorizedItem model**

Create `tools/WikiGenerator/Models/CategorizedItem.cs`:
```csharp
using System;
using WikiGenerator.Models;

class CategorizedItem
{
    public TypeInfo Type { get; set; } = new();
    public string Platform { get; set; } = "";
    public string Layer { get; set; } = "";
    public string Feature { get; set; } = "";
    public string ProjectName { get; set; } = "";
}
```

- [ ] **Step 2: Create Categorizer class**

Create `tools/WikiGenerator/Categorizer.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WikiGenerator.Models;

class Categorizer
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

        foreach (var type in xmlDocs.Types)
        {
            var feature = DetermineFeature(type.FullName);
            var layer = DetermineLayer(type.FullName, mapping.Layers);

            if (string.IsNullOrEmpty(layer))
            {
                if (_logLevel == "info")
                    Console.WriteLine($"[Categorizer] Skipping {type.FullName} (no matching layer)");
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

        return result;
    }

    private string DetermineFeature(string fullName)
    {
        foreach (var (featureName, pattern) in _config.FeaturePatterns)
        {
            if (Regex.IsMatch(fullName, pattern, RegexOptions.IgnoreCase))
                return featureName;
        }

        // Default feature based on last namespace segment
        var lastSegment = fullName.Split('.').Reverse().Skip(1).FirstOrDefault() ?? "Other";
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
```

- [ ] **Step 3: Test Categorizer**

```bash
cd tools/WikiGenerator
dotnet build
cd ../..
```

Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add tools/WikiGenerator/
git commit -m "feat(wiki): implement categorization logic (platform/layer/feature)"
```

---

### Task 5: Implement Markdown Generators

**Files:**
- Create: `tools/WikiGenerator/MarkdownGenerator.cs`

- [ ] **Step 1: Create MarkdownGenerator class**

Create `tools/WikiGenerator/MarkdownGenerator.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WikiGenerator.Models;

class MarkdownGenerator
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

        var summary = items.FirstOrDefault()?.Type.Summary ?? "Framework guide for " + feature;
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
```

- [ ] **Step 2: Update Program.cs to use MarkdownGenerator**

Update `WikiGenerator.GenerateAsync()` method in `tools/WikiGenerator/Program.cs`:
```csharp
public async Task GenerateAsync()
{
    try
    {
        if (_logLevel == "info")
            Console.WriteLine("[WikiGenerator] Starting generation...");

        var config = GeneratorConfig.LoadFromFile(_configPath);
        if (_logLevel == "info")
            Console.WriteLine($"[Config] Loaded {config.ProjectMappings.Count} project mappings");

        var scanner = new DllScanner(config, _logLevel);
        var dllsAndXml = scanner.ScanForDllsAndXml(".");

        if (_logLevel == "info")
            Console.WriteLine($"[Scanner] Found {dllsAndXml.Count} DLL+XML pairs");

        var parser = new XmlCommentParser(_logLevel);
        var categorizer = new Categorizer(config, _logLevel);
        var markdownGen = new MarkdownGenerator(_outputPath, _logLevel);

        var allItems = new List<CategorizedItem>();

        foreach (var (dllPath, xmlPath) in dllsAndXml)
        {
            var projectName = Path.GetFileNameWithoutExtension(dllPath);
            var xmlDocs = parser.ParseXmlFile(xmlPath);
            var categorized = categorizer.Categorize(projectName, xmlDocs);
            allItems.AddRange(categorized);
        }

        if (allItems.Any())
        {
            markdownGen.GenerateApiReference(allItems);
            markdownGen.GenerateUsageGuides(allItems);
        }

        if (_logLevel == "info")
            Console.WriteLine($"[WikiGenerator] Generated docs for {allItems.Count} items");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[Error] {ex.Message}");
        if (_logLevel == "info")
            Console.Error.WriteLine(ex.StackTrace);
        Environment.Exit(1);
    }

    await Task.CompletedTask;
}
```

- [ ] **Step 3: Test markdown generation**

```bash
dotnet build -c Release
dotnet run --project tools/WikiGenerator -- --output docs/framework --log-level info
```

Expected: Output shows "Generated docs for X items" where X > 0, files created in `docs/framework/`.

- [ ] **Step 4: Verify generated files**

```bash
ls -la docs/framework/*/
```

Expected: Directories like `mobile/core/`, `web/core/` with `api-reference.md` and `guide.md` files.

- [ ] **Step 5: Commit**

```bash
git add tools/WikiGenerator/
git commit -m "feat(wiki): implement markdown generation for API reference and guides"
```

---

## Phase 3: Docusaurus Setup & Integration

### Task 6: Set Up Docusaurus Project

**Files:**
- Create: `docusaurus.config.js`
- Create: `sidebars.js`
- Create: `package.json` (update if exists)
- Create: `docs/index.md`

- [ ] **Step 1: Initialize Docusaurus**

```bash
npm init -y docusaurus@latest docusaurus
```

Choose template: `classic`. Answer prompts for site name, tagline, etc.

- [ ] **Step 2: Create docusaurus.config.js**

Create `docusaurus.config.js` (replace root level):
```javascript
const fs = require('fs');
const path = require('path');

module.exports = {
  title: 'SmartWorkz Documentation',
  tagline: 'Comprehensive framework and platform documentation',
  favicon: 'img/favicon.ico',
  url: 'https://docs.smartworkz.dev',
  baseUrl: '/',
  organizationName: 'S2Sys',
  projectName: 'SmartWorkz.StarterKitMVC',

  onBrokenLinks: 'warn',
  onBrokenMarkdownLinks: 'warn',

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          editUrl: 'https://github.com/S2Sys/SmartWorkz.StarterKitMVC/edit/main/',
        },
        blog: false,
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],

  themeConfig: {
    image: 'img/docusaurus-social-card.jpg',
    navbar: {
      title: 'SmartWorkz',
      logo: {
        alt: 'SmartWorkz Logo',
        src: 'img/logo.svg',
      },
      items: [
        { to: '/docs/plan', label: '📋 Planning', position: 'left' },
        { to: '/docs/framework', label: '🏗️ Framework', position: 'left' },
        { to: '/docs/guides', label: '📖 Guides', position: 'left' },
        {
          href: 'https://github.com/S2Sys/SmartWorkz.StarterKitMVC',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Documentation',
          items: [
            { label: 'Planning', to: '/docs/plan' },
            { label: 'Frameworks', to: '/docs/framework' },
            { label: 'Guides', to: '/docs/guides' },
          ],
        },
      ],
      copyright: `© ${new Date().getFullYear()} S2Sys. Built with Docusaurus.`,
    },
    prism: {
      theme: require('prism-react-renderer/themes/github'),
      darkTheme: require('prism-react-renderer/themes/dracula'),
      additionalLanguages: ['csharp', 'xml'],
    },
  },
};
```

- [ ] **Step 3: Create sidebars.js**

Create `sidebars.js`:
```javascript
module.exports = {
  sidebar: [
    {
      label: 'Planning',
      collapsed: false,
      items: [
        { type: 'doc', id: 'plan/index' },
        {
          label: 'Design Specs',
          collapsed: true,
          items: [
            // Auto-generated - populated by WikiGenerator
          ]
        },
        {
          label: 'Implementation Plans',
          collapsed: true,
          items: [
            // Auto-generated - populated by WikiGenerator
          ]
        }
      ]
    },
    {
      label: 'Framework',
      collapsed: false,
      items: [
        { type: 'doc', id: 'framework/index', label: 'Overview' },
        {
          label: 'Mobile',
          collapsed: true,
          items: [
            // Auto-generated by WikiGenerator
          ]
        },
        {
          label: 'Web',
          collapsed: true,
          items: [
            // Auto-generated by WikiGenerator
          ]
        },
        {
          label: 'Shared',
          collapsed: true,
          items: [
            // Auto-generated by WikiGenerator
          ]
        }
      ]
    },
    {
      label: 'Guides',
      collapsed: false,
      items: [
        { type: 'doc', id: 'DEVELOPER' },
        { type: 'doc', id: 'QUICK_REFERENCE' },
        { type: 'doc', id: 'SECURITY' },
      ]
    }
  ]
};
```

- [ ] **Step 4: Create initial docs**

Create `docs/plan/index.md`:
```markdown
# Planning Documentation

This section contains all design specifications and implementation plans for the SmartWorkz ecosystem.

## Contents

- **Design Specs** - Architectural decisions and design patterns
- **Implementation Plans** - Step-by-step task lists for building features

## How to Read

1. Start with a design spec to understand the architecture
2. Follow the implementation plan for step-by-step execution
3. Refer to framework documentation for API details
```

Create `docs/framework/index.md`:
```markdown
# Framework Documentation

Complete API reference and usage guides for all SmartWorkz frameworks.

## Platforms

### Mobile
MAUI-based cross-platform mobile framework. Used by SmartWorkz.Core.Mobile and sample apps.

### Web
ASP.NET Core web framework. Used by SmartWorkz.Core.Web and MVC applications.

### Shared
Cross-platform utilities and abstractions. Used by all projects.

## Organization

Each platform is organized by architectural layers:
- **Core** - Domain models, business logic
- **Infrastructure** - Data access, external services
- **Application** - Controllers, ViewModels, Commands

Within each layer, documentation is grouped by feature (Database, Caching, Services, etc.).
```

- [ ] **Step 5: Install dependencies and build**

```bash
npm install
npm run build
```

Expected: Build succeeds with no errors.

- [ ] **Step 6: Commit**

```bash
git add docusaurus.config.js sidebars.js package.json docs/
git commit -m "feat(docs): set up Docusaurus static site generator"
```

---

### Task 7: Create MSBuild Integration Task

**Files:**
- Modify: `Directory.Build.props`

- [ ] **Step 1: Check if Directory.Build.props exists**

```bash
ls -la Directory.Build.props
```

If not found, create it at project root.

- [ ] **Step 2: Add GenerateWiki target**

If `Directory.Build.props` exists, read it first. Then add to `Directory.Build.props`:

```xml
<!-- Wiki Generation Task -->
<Target Name="GenerateWiki" 
        AfterTargets="Build"
        Condition="'$(Configuration)' == 'Release' Or '$(IsLocalBuild)' == 'true'">
  
  <Message Text="[MSBuild] Generating wiki from DLL XML comments..." Importance="high" />
  
  <Exec Command="dotnet run --project $(MSBuildThisFileDirectory)tools/WikiGenerator -- --output $(MSBuildThisFileDirectory)docs/framework --config $(MSBuildThisFileDirectory)tools/WikiGenerator/config.json --log-level info"
        ContinueOnError="true" />
  
  <Message Text="[MSBuild] Wiki generation completed" Importance="high" />
</Target>
```

If file is new, create complete file:
```xml
<Project>

  <PropertyGroup>
    <IsLocalBuild Condition="'$(GITHUB_ACTIONS)' != 'true' And '$(TF_BUILD)' != 'true'">true</IsLocalBuild>
  </PropertyGroup>

  <!-- Wiki Generation Task -->
  <Target Name="GenerateWiki" 
          AfterTargets="Build"
          Condition="'$(Configuration)' == 'Release' Or '$(IsLocalBuild)' == 'true'">
    
    <Message Text="[MSBuild] Generating wiki from DLL XML comments..." Importance="high" />
    
    <Exec Command="dotnet run --project $(MSBuildThisFileDirectory)tools/WikiGenerator -- --output $(MSBuildThisFileDirectory)docs/framework --config $(MSBuildThisFileDirectory)tools/WikiGenerator/config.json --log-level info"
          ContinueOnError="true" />
    
    <Message Text="[MSBuild] Wiki generation completed" Importance="high" />
  </Target>

</Project>
```

- [ ] **Step 3: Test MSBuild integration**

```bash
dotnet build -c Release src/SmartWorkz.Core/SmartWorkz.Core.csproj
```

Expected: Build output includes "[MSBuild] Generating wiki..." and "[MSBuild] Wiki generation completed".

- [ ] **Step 4: Verify generated files**

```bash
ls -la docs/framework/*/
```

Expected: Markdown files exist from previous build.

- [ ] **Step 5: Commit**

```bash
git add Directory.Build.props
git commit -m "feat(build): integrate wiki generation into MSBuild"
```

---

## Phase 4: Documentation Migration

### Task 8: Migrate Plan Group Docs

**Files:**
- Create: `docs/plan/index.md` (update if exists)
- Create: `docs/plan/specs/` (symlink or copy)
- Create: `docs/plan/implementations/` (symlink or copy)
- Modify: `sidebars.js`

- [ ] **Step 1: Create plan directory structure**

```bash
mkdir -p docs/plan/specs
mkdir -p docs/plan/implementations
```

- [ ] **Step 2: Move design specs**

```bash
# Copy spec files (keep originals for now)
cp docs/superpowers/specs/*.md docs/plan/specs/
```

Expected: All markdown files from `docs/superpowers/specs/` are copied to `docs/plan/specs/`.

- [ ] **Step 3: Move implementation plans**

```bash
# Copy plan files
cp docs/superpowers/plans/*.md docs/plan/implementations/
```

Expected: All markdown files from `docs/superpowers/plans/` are copied to `docs/plan/implementations/`.

- [ ] **Step 4: Update sidebars.js to reference moved files**

Update `sidebars.js` (specs section):
```javascript
{
  label: 'Design Specs',
  collapsed: true,
  items: [
    { type: 'doc', id: 'plan/specs/2026-04-25-docusaurus-wiki-generator-design' },
    { type: 'doc', id: 'plan/specs/2026-04-24-core-web-phase2-blazor-design' },
    { type: 'doc', id: 'plan/specs/2026-04-23-core-web-framework-design' },
    // ... add all specs
  ]
},
```

Update `sidebars.js` (implementations section):
```javascript
{
  label: 'Implementation Plans',
  collapsed: true,
  items: [
    { type: 'doc', id: 'plan/implementations/2026-04-25-docusaurus-wiki-generator-implementation' },
    { type: 'doc', id: 'plan/implementations/2026-04-24-phase2-week1-infrastructure' },
    { type: 'doc', id: 'plan/implementations/2026-04-24-core-web-phase2-blazor-implementation' },
    // ... add all plans
  ]
}
```

- [ ] **Step 5: Build Docusaurus to verify**

```bash
npm run build
```

Expected: Build succeeds, no broken links.

- [ ] **Step 6: Commit**

```bash
git add docs/plan/ sidebars.js
git commit -m "feat(docs): migrate planning docs to plan group structure"
```

---

### Task 9: End-to-End Test

**Files:**
- Test: Full build and wiki generation

- [ ] **Step 1: Clean and full rebuild**

```bash
dotnet clean
dotnet build -c Release
```

Expected: Build succeeds, wiki generation runs.

- [ ] **Step 2: Verify wiki output**

```bash
# Check framework docs generated
ls -la docs/framework/mobile/core/
ls -la docs/framework/web/core/
ls -la docs/framework/shared/core/
```

Expected: Multiple `.md` files in each directory.

- [ ] **Step 3: Verify plan docs exist**

```bash
ls -la docs/plan/specs/ | wc -l
ls -la docs/plan/implementations/ | wc -l
```

Expected: Both directories have multiple `.md` files.

- [ ] **Step 4: Build Docusaurus site**

```bash
npm run build
```

Expected: Build succeeds, static site generated in `build/` folder.

- [ ] **Step 5: Serve locally and verify**

```bash
npm run serve
```

Open `http://localhost:3000` in browser. Verify:
- Navigation shows Plan, Framework, Guides tabs
- Framework section shows Mobile, Web, Shared subsections
- Plan section shows Design Specs and Implementation Plans
- Links between docs work
- API reference and guides are present

- [ ] **Step 6: Final commit**

```bash
git add -A
git commit -m "feat(wiki): complete docusaurus wiki system with full integration"
```

---

## Success Verification

After all tasks complete:

- ✅ WikiGenerator builds and runs without errors
- ✅ DLL scanner finds all projects' compiled DLLs
- ✅ XML parser extracts types and members
- ✅ Categorization assigns correct platform/layer/feature
- ✅ Markdown files generated in `docs/framework/`
- ✅ MSBuild integration runs post-build automatically
- ✅ Docusaurus site builds successfully
- ✅ All docs (Plan, Framework, Guides) accessible via navigation
- ✅ Cross-doc links work correctly
- ✅ Search functionality works
- ✅ Site responsive on mobile
