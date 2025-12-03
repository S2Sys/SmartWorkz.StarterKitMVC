using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Controllers;

/// <summary>
/// Health check endpoints for Kubernetes probes and load balancers.
/// </summary>
/// <example>
/// <code>
/// // Liveness probe
/// GET /health
/// Response: { "status": "Healthy" }
/// 
/// // Readiness probe
/// GET /health/ready
/// Response: { "status": "Ready" }
/// </code>
/// </example>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Liveness probe - indicates the application is running.
    /// </summary>
    /// <returns>Health status.</returns>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "Healthy" });

    /// <summary>
    /// Readiness probe - indicates the application is ready to serve requests.
    /// </summary>
    /// <returns>Ready status.</returns>
    [HttpGet("ready")]
    public IActionResult Ready() => Ok(new { status = "Ready" });
}
