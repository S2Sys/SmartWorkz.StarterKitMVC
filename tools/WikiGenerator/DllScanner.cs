using System.Text.RegularExpressions;

namespace SmartWorkz.Tools.WikiGenerator;

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
        // Normalize path for matching - use forward slashes
        var normalizedPath = filePath.Replace("\\", "/");

        return patterns.Any(pattern =>
        {
            // Convert glob pattern to regex
            var regexPattern = GlobToRegex(pattern);
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(normalizedPath);
        });
    }

    private string GlobToRegex(string glob)
    {
        var sb = new System.Text.StringBuilder();
        var i = 0;

        while (i < glob.Length)
        {
            var c = glob[i];

            if (c == '*')
            {
                // Check for **
                if (i + 1 < glob.Length && glob[i + 1] == '*')
                {
                    sb.Append(".*");
                    i += 2;
                }
                else
                {
                    // Single * matches anything except path separators
                    sb.Append("[^/]*");
                    i++;
                }
            }
            else if (c == '?')
            {
                sb.Append(".");
                i++;
            }
            else if (c == '\\' || c == '/' || c == '.' || c == '+' || c == '^' || c == '$' || c == '(' || c == ')' || c == '[' || c == ']' || c == '{' || c == '}' || c == '|')
            {
                // Escape regex special characters (but not /)
                if (c != '/')
                    sb.Append('\\');
                sb.Append(c);
                i++;
            }
            else
            {
                sb.Append(c);
                i++;
            }
        }

        return $"^{sb.ToString()}$";
    }
}
