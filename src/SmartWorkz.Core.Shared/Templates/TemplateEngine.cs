namespace SmartWorkz.Core.Shared.Templates;

using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;

public partial class TemplateEngine : ITemplateEngine
{
    [GeneratedRegex(@"\{\{(\w+)\}\}|\{(\w+)\}")]
    private static partial Regex PlaceholderRegex();

    public string Render(string content, IDictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        return PlaceholderRegex().Replace(content, match =>
        {
            // Group 1: {{}} placeholders, Group 2: {} placeholders
            string key = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;

            return values.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    public string Render(string content, object model)
    {
        var dict = ReflectModel(model);
        return Render(content, dict);
    }

    public async Task<Result<string>> RenderFileAsync(string filePath, IDictionary<string, string> values, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Result.Fail<string>("Error.InvalidFilePath", "File path cannot be null or empty");

            if (!System.IO.File.Exists(filePath))
                return Result.Fail<string>("Error.FileNotFound", $"File not found: {filePath}");

            var content = await System.IO.File.ReadAllTextAsync(filePath, ct);
            var rendered = Render(content, values);
            return Result.Ok(rendered);
        }
        catch (OperationCanceledException)
        {
            return Result.Fail<string>("Error.OperationCancelled", "File read operation was cancelled");
        }
        catch (IOException ex)
        {
            return Result.Fail<string>("Error.IOException", $"IO error reading file: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Error.UnexpectedException", $"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<string>> RenderFileAsync(string filePath, object model, CancellationToken ct = default)
    {
        var dict = ReflectModel(model);
        return await RenderFileAsync(filePath, dict, ct);
    }

    public async Task<Result<Dictionary<string, string>>> LoadDirectoryAsync(string directoryPath, string searchPattern = "*.html", CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return Result.Fail<Dictionary<string, string>>("Error.InvalidDirectoryPath", "Directory path cannot be null or empty");

            if (!System.IO.Directory.Exists(directoryPath))
                return Result.Fail<Dictionary<string, string>>("Error.DirectoryNotFound", $"Directory not found: {directoryPath}");

            var files = System.IO.Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                var fileName = Path.GetFileNameWithoutExtension(file);
                var content = await System.IO.File.ReadAllTextAsync(file, ct);
                result[fileName] = content;
            }

            return Result.Ok(result);
        }
        catch (OperationCanceledException)
        {
            return Result.Fail<Dictionary<string, string>>("Error.OperationCancelled", "Directory load operation was cancelled");
        }
        catch (IOException ex)
        {
            return Result.Fail<Dictionary<string, string>>("Error.IOException", $"IO error reading directory: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail<Dictionary<string, string>>("Error.UnexpectedException", $"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, string>>> RenderDirectoryAsync(string directoryPath, IDictionary<string, string> values, string searchPattern = "*.html", CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return Result.Fail<Dictionary<string, string>>("Error.InvalidDirectoryPath", "Directory path cannot be null or empty");

            if (!System.IO.Directory.Exists(directoryPath))
                return Result.Fail<Dictionary<string, string>>("Error.DirectoryNotFound", $"Directory not found: {directoryPath}");

            var files = System.IO.Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                var fileName = Path.GetFileNameWithoutExtension(file);
                var content = await System.IO.File.ReadAllTextAsync(file, ct);
                var rendered = Render(content, values);
                result[fileName] = rendered;
            }

            return Result.Ok(result);
        }
        catch (OperationCanceledException)
        {
            return Result.Fail<Dictionary<string, string>>("Error.OperationCancelled", "Directory render operation was cancelled");
        }
        catch (IOException ex)
        {
            return Result.Fail<Dictionary<string, string>>("Error.IOException", $"IO error reading directory: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail<Dictionary<string, string>>("Error.UnexpectedException", $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Reflects over a model object and builds a case-insensitive dictionary of public properties
    /// mapped to their string values.
    /// </summary>
    private static Dictionary<string, string> ReflectModel(object model)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (model == null)
            return dict;

        var properties = model.GetType().GetProperties(
            BindingFlags.Public | BindingFlags.IgnoreCase);

        foreach (var property in properties)
        {
            if (!property.CanRead)
                continue;

            try
            {
                var value = property.GetValue(model);
                dict[property.Name] = value?.ToString() ?? string.Empty;
            }
            catch
            {
                // Skip properties that throw on GetValue
            }
        }

        return dict;
    }
}
