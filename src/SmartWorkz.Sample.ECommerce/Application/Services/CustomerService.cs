using SmartWorkz.Core.Services;
using SmartWorkz.Core.Shared.Mapping;
using SmartWorkz.Core.Abstractions;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Application.Services;

public class CustomerService(IRepository<Customer, int> repo, IMapper mapper)
    : ServiceBase<Customer, CustomerDto>(repo)
{
    protected override CustomerDto Map(Customer entity) =>
        mapper.Map<Customer, CustomerDto>(entity);

    protected override Customer MapToEntity(CustomerDto dto) =>
        throw new NotImplementedException("Use domain constructors");
}
