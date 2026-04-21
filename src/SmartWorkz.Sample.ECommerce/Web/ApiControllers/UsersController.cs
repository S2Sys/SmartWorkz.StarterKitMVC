namespace SmartWorkz.Sample.ECommerce.Web.ApiControllers;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UsersController : ControllerBase
{
    [HttpGet("me")]
    public ActionResult<ApiResponse<UserProfileDto>> GetCurrentUser()
    {
        var sub = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value ?? string.Empty;
        var name = User.FindFirst("name")?.Value ?? string.Empty;
        var roles = User.FindAll("roles").Select(c => c.Value).ToArray();

        if (sub == null || !int.TryParse(sub, out var customerId))
            return Unauthorized(ApiResponse<UserProfileDto>.Fail(
                ApiError.FromError(Error.Unauthorized())));

        var profile = new UserProfileDto(customerId, email, name, roles);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }
}
