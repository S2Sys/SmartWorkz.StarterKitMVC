using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Validators;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Domain.Specifications;

namespace SmartWorkz.Sample.ECommerce.Application.Services;

public class ECommerceAuthService(
    IRepository<Customer, int> customerRepo,
    JwtSettings jwtSettings,
    RegisterValidator registerValidator)
{
    public async Task<Result<string>> RegisterAsync(RegisterDto dto)
    {
        Guard.NotNull(dto, nameof(dto));
        var validation = await registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result<string>.Failure(new Error("Validation.Failed", validation.Failures.First().Message));

        var emailResult = EmailAddress.Create(dto.Email);
        if (!emailResult.Succeeded)
            return Result<string>.Failure(emailResult.Error!);

        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = emailResult.Data!,
            PasswordHash = EncryptionHelper.HashPassword(dto.Password)
        };
        await customerRepo.AddAsync(customer);

        var jwtClaims = new JwtClaims
        {
            Sub = customer.Id.ToString(),
            Email = customer.Email.Value,
            Roles = new List<string> { "customer" }
        };
        var tokenResult = JwtHelper.GenerateToken(jwtClaims, jwtSettings);

        if (!tokenResult.Succeeded)
            return Result<string>.Failure(new Error("Auth.TokenGenerationFailed", "Failed to generate authentication token"));

        return Result<string>.Success(tokenResult.Data!);
    }

    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        Guard.NotEmpty(email, nameof(email));
        Guard.NotEmpty(password, nameof(password));

        var spec = new CustomerByEmailSpecification(email);
        var customers = await customerRepo.FindAllAsync(spec);
        var customer = customers.FirstOrDefault();

        if (customer == null || !EncryptionHelper.VerifyPassword(password, customer.PasswordHash))
            return Result<string>.Failure(new Error("Auth.InvalidCredentials", "Invalid email or password"));

        var jwtClaims = new JwtClaims
        {
            Sub = customer.Id.ToString(),
            Email = customer.Email.Value,
            Roles = new List<string> { "customer" }
        };
        var tokenResult = JwtHelper.GenerateToken(jwtClaims, jwtSettings);

        if (!tokenResult.Succeeded)
            return Result<string>.Failure(new Error("Auth.TokenGenerationFailed", "Failed to generate authentication token"));

        return Result<string>.Success(tokenResult.Data!);
    }
}

