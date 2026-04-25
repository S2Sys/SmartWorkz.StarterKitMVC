using System.CommandLine;

namespace SmartWorkz.Tools.WikiGenerator;

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

        Console.WriteLine("[WikiGenerator] Generation complete");
    }
}
