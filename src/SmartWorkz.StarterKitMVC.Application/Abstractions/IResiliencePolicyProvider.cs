namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

public interface IResiliencePolicyProvider
{
    TimeSpan GetTimeout(string policyName);
    int GetRetryCount(string policyName);
}
