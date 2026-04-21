namespace SmartWorkz.Sample.ECommerce.Web.ApiControllers;

using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ECommerceAuthService authService,
    JwtSettings jwtSettings) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(
        [FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto.Email, dto.Password);
        if (!result.Succeeded)
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail(
                ApiError.FromError(result.Error ?? new Error("Auth.InvalidCredentials",
                    "Invalid email or password"))));

        var response = new LoginResponseDto(
            Token: result.Data!,
            ExpiresAt: DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
            Email: dto.Email,
            FullName: string.Empty);

        return Ok(ApiResponse<LoginResponseDto>.Ok(response));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Register(
        [FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<CustomerDto>.Fail(
                ApiError.FromError(result.Error ?? new Error("Registration.Failed",
                    "Registration failed"))));

        var customerDto = new CustomerDto(0, dto.FirstName, dto.LastName, dto.Email);
        return CreatedAtAction(null, null,
            ApiResponse<CustomerDto>.Ok(customerDto));
    }
}
