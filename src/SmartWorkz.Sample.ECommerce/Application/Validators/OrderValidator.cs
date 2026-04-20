using SmartWorkz.Core.Shared.Validation;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

namespace SmartWorkz.Sample.ECommerce.Application.Validators;

public class OrderValidator : ValidatorBase<OrderDto>
{
    public OrderValidator()
    {
        RuleFor(x => x.Id).Custom(id => Task.FromResult(id > 0), "Order ID must be greater than zero");
        RuleFor(x => x.CustomerId).Custom(custId => Task.FromResult(custId > 0), "Customer ID must be greater than zero");
        RuleFor(x => x.Status).NotEmpty();
        RuleFor(x => x.Total).Custom(total => Task.FromResult(total > 0), "Total must be greater than zero");
        RuleFor(x => x.Items).Custom(items => Task.FromResult(items.Count > 0), "Order must have at least one item");
    }
}
