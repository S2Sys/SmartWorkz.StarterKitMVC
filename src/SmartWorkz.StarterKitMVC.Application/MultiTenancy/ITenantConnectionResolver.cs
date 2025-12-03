namespace SmartWorkz.StarterKitMVC.Application.MultiTenancy;

public interface ITenantConnectionResolver
{
    string GetConnectionString(string tenantId);
    string GetStoragePath(string tenantId);
}
