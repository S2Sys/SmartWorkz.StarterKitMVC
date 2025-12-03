namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

/// <summary>
/// Interface for AI client operations.
/// </summary>
public interface IAiClient
{
    /// <summary>
    /// Generates a completion based on the provided prompt.
    /// </summary>
    Task<string> GenerateCompletionAsync(string prompt, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates embeddings for the provided text.
    /// </summary>
    Task<float[]> GenerateEmbeddingsAsync(string text, CancellationToken cancellationToken = default);
}
