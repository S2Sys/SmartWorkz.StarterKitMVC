namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

/// <summary>
/// Cross-platform file helper utilities.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <returns>True if the file exists; otherwise false.</returns>
    /// <example>
    /// <code>
    /// if (FileHelper.FileExists("config.json"))
    ///     Console.WriteLine("Config file found!");
    /// </code>
    /// </example>
    public static bool FileExists(string path) => File.Exists(path);
}
