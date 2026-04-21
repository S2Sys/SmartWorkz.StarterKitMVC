using SmartWorkz.Core;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Application.Services;

public class CustomerService(IRepository<Customer, int> repo, IMapper mapper)
    : ServiceBase<Customer, CustomerDto>(repo)
{
    private readonly IMapper _mapper = mapper;

    protected override CustomerDto Map(Customer entity) =>
        _mapper.Map<Customer, CustomerDto>(entity);

    protected override Customer MapToEntity(CustomerDto dto) =>
        throw new NotImplementedException("Use domain constructors");
}
