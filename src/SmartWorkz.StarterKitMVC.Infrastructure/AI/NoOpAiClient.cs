using SmartWorkz.StarterKitMVC.Application.Abstractions.AI;

namespace SmartWorkz.StarterKitMVC.Infrastructure.AI;

public sealed class NoOpAiClient : IAiClient
{
    public Task<string> GetCompletionAsync(string prompt, string model, CancellationToken cancellationToken = default)
        => Task.FromResult(string.Empty);
}
