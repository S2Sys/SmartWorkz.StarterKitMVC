using System.CommandLine;
using SmartWorkz.Tools.WikiGenerator.Models;

namespace SmartWorkz.Tools.WikiGenerator;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Quick test flag for parser
        if (args.Contains("--test-parser"))
        {
            var xmlPath = "./src/SmartWorkz.Core/bin/Debug/net9.0/SmartWorkz.Core.Web.xml";
            if (!File.Exists(xmlPath))
            {
                Console.WriteLine($"Test XML not found at {xmlPath}");
                return 1;
            }

            var parser = new XmlCommentParser("info");
            var doc = parser.ParseXmlFile(xmlPath);

            Console.WriteLine($"\n=== PARSER TEST RESULTS ===");
            Console.WriteLine($"Total types extracted: {doc.Types.Count}");

            if (doc.Types.Count > 0)
            {
                var firstType = doc.Types[0];
                Console.WriteLine($"\nFirst type: {firstType.Name}");
                Console.WriteLine($"Full name: {firstType.FullName}");
                Console.WriteLine($"Summary: {firstType.Summary.Substring(0, Math.Min(50, firstType.Summary.Length))}...");
                Console.WriteLine($"Members: {firstType.Members.Count}");

                if (firstType.Members.Count > 0)
                {
                    var firstMember = firstType.Members[0];
                    Console.WriteLine($"\nFirst member: {firstMember.Name}");
                    Console.WriteLine($"Member summary: {firstMember.Summary.Substring(0, Math.Min(50, firstMember.Summary.Length))}...");
                    Console.WriteLine($"Parameters: {firstMember.Parameters.Count}");
                }
            }

            return 0;
        }

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
            try
            {
                var generator = new WikiGenerator(output, config, logLevel);
                await generator.GenerateAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Error] {ex.Message}");
                if (logLevel == "info")
                    Console.Error.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
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

        var totalTypes = 0;
        var allParsedDocs = new List<(string Project, XmlDocumentation Docs)>();

        foreach (var (dllPath, xmlPath) in dllsAndXml)
        {
            var projectName = Path.GetFileNameWithoutExtension(dllPath);
            var xmlDocs = parser.ParseXmlFile(xmlPath);
            totalTypes += xmlDocs.Types.Count;
            allParsedDocs.Add((projectName, xmlDocs));
        }

        if (_logLevel == "info")
            Console.WriteLine($"[Parser] Extracted {totalTypes} types total");

        // TODO: Task 4 - Implement categorization and write markdown output files
        if (_logLevel == "info")
            Console.WriteLine($"[Output] Writing {allParsedDocs.Count} projects to {_outputPath}...");

        Console.WriteLine("[WikiGenerator] Generation complete");
    }
}
