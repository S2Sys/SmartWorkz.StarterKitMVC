namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

public interface IFeatureFlagService
{
    bool IsEnabled(string flagName);
}
