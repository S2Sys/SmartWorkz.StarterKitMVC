using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.AI;

/// <summary>
/// No-op AI client implementation.
/// </summary>
public sealed class NoOpAiClient : IAiClient
{
    public Task<string> GenerateCompletionAsync(string prompt, CancellationToken cancellationToken = default)
        => Task.FromResult(string.Empty);
    
    public Task<float[]> GenerateEmbeddingsAsync(string text, CancellationToken cancellationToken = default)
        => Task.FromResult(Array.Empty<float>());
}
