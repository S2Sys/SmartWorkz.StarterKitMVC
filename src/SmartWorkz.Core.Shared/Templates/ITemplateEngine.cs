namespace SmartWorkz.Shared;

/// <summary>Defines operations for rendering templates with placeholder substitution.</summary>
public interface ITemplateEngine
{
    /// <summary>Renders a template string by replacing placeholders with values from a dictionary.</summary>
    /// <param name="content">The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).</param>
    /// <param name="values">Dictionary of key-value pairs to substitute into the template.</param>
    /// <returns>The rendered content with placeholders replaced. Unmatched placeholders remain unchanged.</returns>
    string Render(string content, IDictionary<string, string> values);

    /// <summary>Renders a template string by extracting properties from an object model.</summary>
    /// <param name="content">The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).</param>
    /// <param name="model">The model object whose public properties are used for placeholder substitution.</param>
    /// <returns>The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.</returns>
    string Render(string content, object model);

    /// <summary>Asynchronously renders a template file by replacing placeholders with values from a dictionary.</summary>
    /// <param name="filePath">The path to the template file.</param>
    /// <param name="values">Dictionary of key-value pairs to substitute into the template.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A result containing the rendered file content, or an error if the file operation fails.</returns>
    Task<Result<string>> RenderFileAsync(string filePath, IDictionary<string, string> values, CancellationToken ct = default);

    /// <summary>Asynchronously renders a template file by extracting properties from an object model.</summary>
    /// <param name="filePath">The path to the template file.</param>
    /// <param name="model">The model object whose public properties are used for placeholder substitution.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A result containing the rendered file content, or an error if the file operation fails.</returns>
    Task<Result<string>> RenderFileAsync(string filePath, object model, CancellationToken ct = default);

    /// <summary>Asynchronously loads all template files from a directory into a dictionary.</summary>
    /// <param name="directoryPath">The directory path to scan for template files.</param>
    /// <param name="searchPattern">The search pattern for files (default "*.html"). Supports wildcards like "*.txt".</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.</returns>
    Task<Result<Dictionary<string, string>>> LoadDirectoryAsync(string directoryPath, string searchPattern = "*.html", CancellationToken ct = default);

    /// <summary>Asynchronously loads and renders all template files from a directory using values from a dictionary.</summary>
    /// <param name="directoryPath">The directory path to scan for template files.</param>
    /// <param name="values">Dictionary of key-value pairs to substitute into each template.</param>
    /// <param name="searchPattern">The search pattern for files (default "*.html"). Supports wildcards like "*.txt".</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.</returns>
    Task<Result<Dictionary<string, string>>> RenderDirectoryAsync(string directoryPath, IDictionary<string, string> values, string searchPattern = "*.html", CancellationToken ct = default);
}
