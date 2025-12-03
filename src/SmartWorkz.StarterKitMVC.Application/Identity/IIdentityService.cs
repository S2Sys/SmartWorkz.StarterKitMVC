using SmartWorkz.StarterKitMVC.Domain.Identity;
using SmartWorkz.StarterKitMVC.Shared.Primitives;

namespace SmartWorkz.StarterKitMVC.Application.Identity;

/// <summary>
/// Service for user authentication and identity management.
/// </summary>
/// <example>
/// <code>
/// // Inject IIdentityService via DI
/// public class AuthController : Controller
/// {
///     private readonly IIdentityService _identity;
///     
///     public AuthController(IIdentityService identity) => _identity = identity;
///     
///     [HttpPost("login")]
///     public async Task&lt;IActionResult&gt; Login(LoginRequest request)
///     {
///         var result = await _identity.LoginAsync(request.UserName, request.Password);
///         
///         if (result.IsSuccess)
///             return Ok(new { result.Value.AccessToken, result.Value.RefreshToken });
///         
///         return Unauthorized(result.Error.Message);
///     }
///     
///     [HttpPost("register")]
///     public async Task&lt;IActionResult&gt; Register(RegisterRequest request)
///     {
///         var result = await _identity.RegisterAsync(request.UserName, request.Email, request.Password);
///         return result.IsSuccess ? Ok() : BadRequest(result.Error.Message);
///     }
/// }
/// </code>
/// </example>
public interface IIdentityService
{
    /// <summary>
    /// Authenticates a user and returns access/refresh tokens.
    /// </summary>
    /// <param name="userName">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing tokens on success, or error on failure.</returns>
    Task<Result<TokenResult>> LoginAsync(string userName, string password, CancellationToken ct = default);
    
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="userName">The username.</param>
    /// <param name="email">The email address.</param>
    /// <param name="password">The password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> RegisterAsync(string userName, string email, string password, CancellationToken ct = default);
    
    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing new tokens on success.</returns>
    Task<Result<TokenResult>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    
    /// <summary>
    /// Gets the currently authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current user or null if not authenticated.</returns>
    Task<AppUser?> GetCurrentUserAsync(CancellationToken ct = default);
}
