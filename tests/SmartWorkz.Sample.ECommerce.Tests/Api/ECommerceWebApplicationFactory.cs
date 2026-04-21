namespace SmartWorkz.Sample.ECommerce.Tests.Api;

using Microsoft.AspNetCore.Mvc.Testing;

/// <summary>
/// Custom WebApplicationFactory for testing the ECommerce API.
/// Uses the application's standard configuration from Program.cs.
/// </summary>
public class ECommerceWebApplicationFactory : WebApplicationFactory<Program>
{
    // No customization needed - the application's service registration handles everything
}
