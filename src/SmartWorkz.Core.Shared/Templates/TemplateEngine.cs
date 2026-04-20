namespace SmartWorkz.Core.Shared.Templates;

using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Collections.Concurrent;

/// <summary>Provides template rendering services with support for placeholder substitution.</summary>
/// <remarks>
/// This engine supports two placeholder formats: {Key} and {{Key}}, both case-insensitive.
/// When rendering with a dictionary, all matching placeholders are replaced with corresponding values.
/// When rendering with an object model, public properties are used as replacement values.
/// Unmatched placeholders remain unchanged in the rendered output.
/// </remarks>
public partial class TemplateEngine : ITemplateEngine
{
    /// <summary>Regular expression pattern for matching placeholders in both {{}} and {} formats (case-insensitive).</summary>
    [GeneratedRegex(@"\{\{(\w+)\}\}|\{(\w+)\}")]
    private static partial Regex PlaceholderRegex();

    /// <summary>Cache of property metadata by type for performance optimization.</summary>
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    /// <summary>Renders a template string by replacing placeholders with values from a dictionary.</summary>
    /// <param name="content">The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).</param>
    /// <param name="values">Dictionary of key-value pairs to substitute into the template.</param>
    /// <returns>The rendered content with placeholders replaced. Unmatched placeholders and null/empty content remain unchanged.</returns>
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

    /// <summary>Renders a template string by extracting properties from an object model.</summary>
    /// <param name="content">The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).</param>
    /// <param name="model">The model object whose public properties are used for placeholder substitution.</param>
    /// <returns>The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.</returns>
    public string Render(string content, object model)
    {
        var dict = ReflectModel(model);
        return Render(content, dict);
    }

    /// <summary>Asynchronously renders a template file by replacing placeholders with values from a dictionary.</summary>
    /// <param name="filePath">The path to the template file.</param>
    /// <param name="values">Dictionary of key-value pairs to substitute into the template.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A result containing the rendered file content, or an error if the file operation fails.</returns>
    public async Task<Result<string>> RenderFileAsync(string filePath, IDictionary<string, string> values, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Result.Fail<string>("Error.InvalidFilePath", "File path cannot be null or empty");

            // Security: Validate path to prevent directory traversal attacks
            var pathValidation = ValidateFilePath(filePath);
            if (!pathValidation.Succeeded)
                return Result.Fail<string>(pathValidation.Error.Code, pathValidation.Error.Message);

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

    /// <summary>Asynchronously renders a template file by extracting properties from an object model.</summary>
    /// <param name="filePath">The path to the template file.</param>
    /// <param name="model">The model object whose public properties are used for placeholder substitution.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A result containing the rendered file content, or an error if the file operation fails.</returns>
    public async Task<Result<string>> RenderFileAsync(string filePath, object model, CancellationToken ct = default)
    {
        var dict = ReflectModel(model);
        return await RenderFileAsync(filePath, dict, ct);
    }

    /// <summary>Asynchronously loads all template files from a directory into a dictionary.</summary>
    /// <param name="directoryPath">The directory path to scan for template files.</param>
    /// <param name="searchPattern">The search pattern for files (default "*.html"). Supports wildcards like "*.txt".</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.</returns>
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

    /// <summary>Asynchronously loads and renders all template files from a directory using values from a dictionary.</summary>
    /// <param name="directoryPath">The directory path to scan for template files.</param>
    /// <param name="values">Dictionary of key-value pairs to substitute into each template.</param>
    /// <param name="searchPattern">The search pattern for files (default "*.html"). Supports wildcards like "*.txt".</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.</returns>
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
    /// mapped to their string values. Uses cached property metadata for performance.
    /// </summary>
    private static Dictionary<string, string> ReflectModel(object model)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (model == null)
            return dict;

        var modelType = model.GetType();

        // Use cached property metadata to avoid reflection on every call
        var properties = PropertyCache.GetOrAdd(modelType, type =>
            type.GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase));

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

    /// <summary>
    /// Validates a file path to prevent directory traversal attacks.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns>A result indicating if the path is valid and safe.</returns>
    private static Result<bool> ValidateFilePath(string filePath)
    {
        try
        {
            // Reject obvious traversal patterns
            if (filePath.Contains("..") || filePath.Contains("~"))
                return Result.Fail<bool>("Error.InvalidPath", "Path traversal patterns detected");

            // Normalize and resolve the full path
            var fullPath = System.IO.Path.GetFullPath(filePath);

            // Ensure the resolved path is an absolute path and doesn't escape current directory context
            if (!System.IO.Path.IsPathRooted(fullPath))
                return Result.Fail<bool>("Error.InvalidPath", "Path must be absolute or within current context");

            return Result.Ok(true);
        }
        catch (ArgumentException)
        {
            return Result.Fail<bool>("Error.InvalidPath", "Invalid path characters detected");
        }
        catch (Exception ex)
        {
            return Result.Fail<bool>("Error.PathValidationFailed", $"Path validation failed: {ex.Message}");
        }
    }
}
