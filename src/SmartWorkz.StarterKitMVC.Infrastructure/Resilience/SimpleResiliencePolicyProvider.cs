using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Resilience;

public sealed class SimpleResiliencePolicyProvider : IResiliencePolicyProvider
{
    public TimeSpan GetTimeout(string policyName) => TimeSpan.FromSeconds(30);

    public int GetRetryCount(string policyName) => 3;
}
