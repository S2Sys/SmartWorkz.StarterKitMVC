using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.Requests;

namespace SmartWorkz.Sample.ECommerce.Application.Validators;

public class CreateProductValidator : ValidatorBase<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaxLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaxLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqual(0m);
        RuleFor(x => x.CategoryId).Custom(id => Task.FromResult(id > 0), "CategoryId must be positive");
    }
}
