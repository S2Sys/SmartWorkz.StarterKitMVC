namespace SmartWorkz.Core.Shared.Templates;

public interface ITemplateEngine
{
    string Render(string content, IDictionary<string, string> values);
    string Render(string content, object model);
    Task<Result<string>> RenderFileAsync(string filePath, IDictionary<string, string> values, CancellationToken ct = default);
    Task<Result<string>> RenderFileAsync(string filePath, object model, CancellationToken ct = default);
    Task<Result<Dictionary<string, string>>> LoadDirectoryAsync(string directoryPath, string searchPattern = "*.html", CancellationToken ct = default);
    Task<Result<Dictionary<string, string>>> RenderDirectoryAsync(string directoryPath, IDictionary<string, string> values, string searchPattern = "*.html", CancellationToken ct = default);
}
