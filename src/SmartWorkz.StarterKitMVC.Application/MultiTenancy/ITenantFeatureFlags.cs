namespace SmartWorkz.StarterKitMVC.Application.MultiTenancy;

public interface ITenantFeatureFlags
{
    bool IsEnabled(string tenantId, string flagName);
}
