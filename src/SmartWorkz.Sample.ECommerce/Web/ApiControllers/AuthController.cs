using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;

namespace SmartWorkz.Sample.ECommerce.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ECommerceAuthService authService,
    JwtSettings jwtSettings) : ControllerBase
{
    /// <summary>
    /// Authenticates a user with email and password, returning a JWT token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (loginDto == null)
            return BadRequest(new { error = "Login credentials are required" });

        if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            return BadRequest(new { error = "Email and password are required" });

        var result = await authService.LoginAsync(loginDto.Email, loginDto.Password);

        if (!result.Succeeded)
        {
            if (result.Error?.Code == "Auth.InvalidCredentials")
                return Unauthorized(new { error = result.Error.Message });

            return BadRequest(new { error = result.Error?.Message });
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes);

        // Extract customer info from token or use email as fallback
        var response = new LoginResponseDto(
            Token: result.Data!,
            ExpiresAt: expiresAt,
            Email: loginDto.Email,
            FullName: string.Empty // Customer name is not available in the auth service, can be enhanced later
        );

        return Ok(response);
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (registerDto == null)
            return BadRequest(new { error = "Registration data is required" });

        if (string.IsNullOrWhiteSpace(registerDto.FirstName) ||
            string.IsNullOrWhiteSpace(registerDto.LastName) ||
            string.IsNullOrWhiteSpace(registerDto.Email) ||
            string.IsNullOrWhiteSpace(registerDto.Password) ||
            string.IsNullOrWhiteSpace(registerDto.ConfirmPassword))
            return BadRequest(new { error = "All fields are required" });

        if (registerDto.Password != registerDto.ConfirmPassword)
            return BadRequest(new { error = "Passwords do not match" });

        var result = await authService.RegisterAsync(registerDto);

        if (!result.Succeeded)
        {
            if (result.Error?.Code == "Validation.Failed")
                return Conflict(new { error = result.Error.Message });

            return BadRequest(new { error = result.Error?.Message });
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes);

        var response = new LoginResponseDto(
            Token: result.Data!,
            ExpiresAt: expiresAt,
            Email: registerDto.Email,
            FullName: $"{registerDto.FirstName} {registerDto.LastName}"
        );

        return CreatedAtAction(nameof(Register), response);
    }
}
