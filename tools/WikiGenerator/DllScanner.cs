using System.Text.RegularExpressions;

namespace SmartWorkz.Tools.WikiGenerator;

class DllScanner
{
    private readonly GeneratorConfig _config;
    private readonly string _logLevel;
    private readonly List<Regex> _includePatterns;
    private readonly List<Regex> _excludePatterns;

    public DllScanner(GeneratorConfig config, string logLevel)
    {
        _config = config;
        _logLevel = logLevel;

        _includePatterns = config.IncludeDllPatterns
            .Select(p => new Regex(GlobToRegex(p), RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToList();

        _excludePatterns = config.ExcludeDllPatterns
            .Select(p => new Regex(GlobToRegex(p), RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToList();
    }

    public List<(string DllPath, string XmlPath)> ScanForDllsAndXml(string rootPath = ".")
    {
        if (_includePatterns.Count == 0)
        {
            if (_logLevel == "warning" || _logLevel == "info")
                Console.WriteLine("[Warning] IncludeDllPatterns is empty - no DLLs will be scanned");
            return new();
        }

        var result = new List<(string, string)>();
        var rootDir = new DirectoryInfo(rootPath);

        var allDlls = rootDir.GetFiles("*.dll", SearchOption.AllDirectories).ToList();

        if (_logLevel == "info")
            Console.WriteLine($"[Scanner] Found {allDlls.Count} total DLL files, filtering by pattern...");

        var filtered = allDlls
            .Where(f => MatchesPattern(f.FullName, _includePatterns))
            .Where(f => !MatchesPattern(f.FullName, _excludePatterns))
            .ToList();

        if (_logLevel == "info" && filtered.Count != allDlls.Count)
            Console.WriteLine($"[Scanner] Filtered to {filtered.Count} DLLs after pattern matching");

        allDlls = filtered;

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

    private bool MatchesPattern(string filePath, List<Regex> patterns)
    {
        return patterns.Any(regex => regex.IsMatch(filePath));
    }

    private static string GlobToRegex(string glob)
    {
        var sb = new System.Text.StringBuilder("^");
        var normalized = glob.Replace("\\", "/");
        int i = 0;

        while (i < normalized.Length)
        {
            if (i + 1 < normalized.Length && normalized[i..].StartsWith("**"))
            {
                if ((i == 0 || normalized[i - 1] == '/') &&
                    (i + 2 >= normalized.Length || normalized[i + 2] == '/'))
                {
                    sb.Append(".*");
                    i += 2;
                    if (i < normalized.Length && normalized[i] == '/')
                        i++;
                }
                else
                {
                    sb.Append(System.Text.RegularExpressions.Regex.Escape("*"));
                    i++;
                }
            }
            else if (normalized[i] == '*')
            {
                sb.Append("[^/]*");
                i++;
            }
            else if (normalized[i] == '?')
            {
                sb.Append("[^/]");
                i++;
            }
            else
            {
                var c = normalized[i];
                if ("\\^$.|+()[]{}".Contains(c))
                    sb.Append('\\');
                sb.Append(c);
                i++;
            }
        }

        sb.Append("$");
        return sb.ToString();
    }
}
