using SmartWorkz.Core.Shared.Validation;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

namespace SmartWorkz.Sample.ECommerce.Application.Validators;

public class CheckoutValidator : ValidatorBase<CheckoutDto>
{
    public CheckoutValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaxLength(100);
        RuleFor(x => x.City).NotEmpty().MaxLength(50);
        RuleFor(x => x.State).NotEmpty().MaxLength(50);
        RuleFor(x => x.PostalCode).NotEmpty().MaxLength(20);
        RuleFor(x => x.Country).NotEmpty().MaxLength(50);
    }
}
