namespace SmartWorkz.Sample.ECommerce.Tests.Unit.Validators;

using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Validators;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public async Task Valid_register_request_passes_validation()
    {
        var dto = new RegisterDto("John", "Doe", "john@example.com", "Password123!", "Password123!");
        var result = await _validator.ValidateAsync(dto);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Empty_first_name_fails_validation()
    {
        var dto = new RegisterDto("", "Doe", "john@example.com", "Password123!", "Password123!");
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Failures, f => f.PropertyName == "FirstName");
    }

    [Fact]
    public async Task Invalid_email_format_fails_validation()
    {
        var dto = new RegisterDto("John", "Doe", "notanemail", "Password123!", "Password123!");
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Failures, f => f.PropertyName == "Email");
    }

    [Fact]
    public async Task Password_too_short_fails_validation()
    {
        var dto = new RegisterDto("John", "Doe", "john@example.com", "short", "short");
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Failures, f => f.PropertyName == "Password");
    }

    [Fact]
    public async Task Empty_last_name_fails_validation()
    {
        var dto = new RegisterDto("John", "", "john@example.com", "Password123!", "Password123!");
        var result = await _validator.ValidateAsync(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Failures, f => f.PropertyName == "LastName");
    }
}
