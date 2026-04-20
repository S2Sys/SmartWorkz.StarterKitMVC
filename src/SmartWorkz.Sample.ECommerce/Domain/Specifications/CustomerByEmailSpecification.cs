using SmartWorkz.Core.Shared.Specifications;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Domain.Specifications;

public class CustomerByEmailSpecification : Specification<Customer>
{
    public CustomerByEmailSpecification(string email)
    {
        AddCriteria(c => c.Email.Value == email);
    }
}
