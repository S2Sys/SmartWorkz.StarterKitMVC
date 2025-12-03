namespace SmartWorkz.StarterKitMVC.Application.MultiTenancy;

/// <summary>
/// Provides access to the current tenant context.
/// </summary>
/// <example>
/// <code>
/// // Inject ITenantContext via DI
/// public class TenantAwareService
/// {
///     private readonly ITenantContext _tenant;
///     
///     public TenantAwareService(ITenantContext tenant) => _tenant = tenant;
///     
///     public void DoWork()
///     {
///         var tenantId = _tenant.TenantId;
///         Console.WriteLine($"Working for tenant: {tenantId}");
///     }
/// }
/// </code>
/// </example>
public interface ITenantContext
{
    /// <summary>Gets or sets the current tenant ID.</summary>
    string? TenantId { get; set; }
}
