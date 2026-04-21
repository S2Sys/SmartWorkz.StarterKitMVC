using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;

namespace SmartWorkz.Sample.ECommerce.Web.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly ECommerceAuthService _auth;

    public AuthApiController(ECommerceAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _auth.LoginAsync(dto.Email, dto.Password);
        if (!result.Succeeded) return Unauthorized(result.Error?.Message);
        return Ok(result.Data);   // LoginResponseDto { Token, ExpiresAt, Email, FullName }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _auth.RegisterAsync(dto);
        if (!result.Succeeded) return BadRequest(result.Error?.Message);
        return Ok(result.Data);
    }
}
