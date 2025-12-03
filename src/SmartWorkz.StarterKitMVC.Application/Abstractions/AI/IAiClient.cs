namespace SmartWorkz.StarterKitMVC.Application.Abstractions.AI;

public interface IAiClient
{
    Task<string> GetCompletionAsync(string prompt, string model, CancellationToken cancellationToken = default);
}
