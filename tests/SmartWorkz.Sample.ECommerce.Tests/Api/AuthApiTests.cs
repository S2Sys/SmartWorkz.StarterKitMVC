namespace SmartWorkz.Sample.ECommerce.Tests.Api;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

public class AuthApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_api_auth_login_with_bad_credentials_returns_401()
    {
        var loginDto = new LoginDto("nobody@example.com", "wrongpass");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task POST_api_auth_register_with_valid_data_returns_201_or_200()
    {
        var registerDto = new RegisterDto("John", "Doe", "john@example.com", "ValidPass123!", "ValidPass123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_api_auth_register_with_missing_fields_returns_400()
    {
        var registerDto = new RegisterDto("", "", "", "", "");
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_api_auth_login_with_empty_body_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_api_categories_returns_200_ok()
    {
        var response = await _client.GetAsync("/api/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<CategoryDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.Success);
    }
}
