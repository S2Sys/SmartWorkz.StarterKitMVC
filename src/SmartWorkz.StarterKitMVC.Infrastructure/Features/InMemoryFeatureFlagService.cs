using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Features;

public sealed class InMemoryFeatureFlagService : IFeatureFlagService
{
    private readonly ISet<string> _enabledFlags;

    public InMemoryFeatureFlagService(IEnumerable<string>? enabledFlags = null)
    {
        _enabledFlags = new HashSet<string>(enabledFlags ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
    }

    public bool IsEnabled(string flagName) => _enabledFlags.Contains(flagName);
}
