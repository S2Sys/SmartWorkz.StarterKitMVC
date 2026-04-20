using System.Text.RegularExpressions;
using SmartWorkz.Core.Shared.Validation;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

namespace SmartWorkz.Sample.ECommerce.Application.Validators;

public class RegisterValidator : ValidatorBase<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaxLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaxLength(50);
        RuleFor(x => x.Email).NotEmpty()
            .Custom(email => Task.FromResult(Regex.IsMatch(email, ValidationRules.EmailPattern)), "Email format is invalid");
        RuleFor(x => x.Password).NotEmpty()
            .Custom(pwd => Task.FromResult(pwd.Length >= 8), "Password must be at least 8 characters");
        RuleFor(x => x.ConfirmPassword).NotEmpty();
    }
}
