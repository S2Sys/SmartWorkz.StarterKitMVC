namespace SmartWorkz.Core.Examples;

/// <summary>
/// Demonstrates DTO mapping patterns for API contracts and data transformation.
/// DTOs decouple internal domain models from external API representations,
/// enabling flexible data transformation without exposing internal structures.
/// </summary>
public class DTOMappingExample
{
    public class Customer : AuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    // API Contract DTOs
    public record CreateCustomerDto(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Street,
        string City);

    public record CustomerDto(
        int Id,
        string FirstName,
        string LastName,
        string Email,
        string FullAddress);

    public record UpdateCustomerDto(
        string FirstName,
        string LastName,
        string Email);

    /// <summary>
    /// DTO mapping patterns in service layer.
    /// </summary>
    public class DTOMappingPatterns
    {
        /// <summary>
        /// Example 1: Entity to DTO mapping (for Read operations).
        /// </summary>
        public CustomerDto MapToDto(Customer entity)
        {
            return new CustomerDto(
                Id: entity.Id,
                FirstName: entity.FirstName,
                LastName: entity.LastName,
                Email: entity.Email,
                FullAddress: $"{entity.Street}, {entity.City}");
        }

        /// <summary>
        /// Example 2: DTO to Entity mapping (for Create operations).
        /// </summary>
        public Customer MapToEntity(CreateCustomerDto dto)
        {
            return new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Street = dto.Street,
                City = dto.City
            };
        }

        /// <summary>
        /// Example 3: Batch mapping (collection).
        /// </summary>
        public IReadOnlyCollection<CustomerDto> MapToDto(IEnumerable<Customer> entities)
        {
            return entities
                .Select(e => MapToDto(e))
                .ToList();
        }

        /// <summary>
        /// Example 4: Selective field exposure (security).
        /// </summary>
        public record CustomerPublicDto(
            int Id,
            string FullName,
            string City);

        public CustomerPublicDto MapToPublicDto(Customer entity)
        {
            // Only expose non-sensitive fields
            return new CustomerPublicDto(
                Id: entity.Id,
                FullName: $"{entity.FirstName} {entity.LastName}",
                City: entity.City);
        }

        /// <summary>
        /// Example 5: Flattening nested value objects.
        /// </summary>
        public void Example_FlatteningValueObjects()
        {
            // If Customer had nested value objects:
            // - PersonName { FirstName, LastName }
            // - Address { Street, City, PostalCode, Country }
            // - EmailAddress { Value }
            //
            // Mapping flattens them to simple strings in DTO:
            // DTO: FirstName, LastName, Street, City, Email
            // Entity: PersonName.FirstName, Address.Street, EmailAddress.Value

            System.Console.WriteLine("Flatten: Nested value objects -> flat DTO");
        }
    }

    /// <summary>
    /// Service layer with DTO mapping.
    /// </summary>
    public class CustomerService
    {
        private readonly DTOMappingPatterns _mapper;

        public CustomerService()
        {
            _mapper = new DTOMappingPatterns();
        }

        /// <summary>
        /// CREATE: DTO -> Entity -> Persist -> DTO
        /// </summary>
        public Result<CustomerDto> CreateCustomer(CreateCustomerDto dto)
        {
            // Map DTO to entity
            var customer = _mapper.MapToEntity(dto);

            // In real usage:
            // await repository.AddAsync(customer);
            // await unitOfWork.SaveAsync();

            // Map back to DTO
            var result = _mapper.MapToDto(customer);
            return Result.Ok(result);
        }

        /// <summary>
        /// READ: Entity -> DTO
        /// </summary>
        public Result<CustomerDto> GetCustomer(int id)
        {
            // In real usage: var customer = await repository.GetByIdAsync(id);
            if (id <= 0) return Result.Fail<CustomerDto>("NOT_FOUND", "Customer not found");

            var customer = new Customer { Id = id, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
            return Result.Ok(_mapper.MapToDto(customer));
        }

        /// <summary>
        /// UPDATE: DTO -> Update Entity -> Persist -> DTO
        /// </summary>
        public Result<CustomerDto> UpdateCustomer(int id, UpdateCustomerDto dto)
        {
            // In real usage:
            // var customer = await repository.GetByIdAsync(id);
            // customer.FirstName = dto.FirstName;
            // customer.LastName = dto.LastName;
            // customer.Email = dto.Email;
            // await repository.UpdateAsync(customer);

            var customer = new Customer { Id = id, FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email };
            return Result.Ok(_mapper.MapToDto(customer));
        }

        /// <summary>
        /// LIST: Entities -> DTOs
        /// </summary>
        public Result<IReadOnlyCollection<CustomerDto>> GetAllCustomers()
        {
            // In real usage:
            // var customers = await repository.GetAllAsync();
            // return _mapper.MapToDto(customers);

            var customers = new List<Customer> { };
            return Result.Ok(_mapper.MapToDto(customers));
        }
    }

    /// <summary>
    /// ASP.NET controller using DTOs.
    /// </summary>
    public class CustomerController
    {
        private readonly CustomerService _service;

        public CustomerController(CustomerService service)
        {
            _service = service;
        }

        /// <summary>
        /// POST /customers - Create from DTO
        /// </summary>
        public object CreateCustomer(CreateCustomerDto dto)
        {
            var result = _service.CreateCustomer(dto);
            if (!result.Succeeded)
                return new { success = false, error = result.Error?.Message };
            return new { success = true, data = result.Data };
        }

        /// <summary>
        /// GET /customers/{id} - Return as DTO
        /// </summary>
        public object GetCustomer(int id)
        {
            var result = _service.GetCustomer(id);
            if (!result.Succeeded)
                return new { success = false, error = result.Error?.Message };
            return new { success = true, data = result.Data };
        }

        /// <summary>
        /// PUT /customers/{id} - Update from DTO
        /// </summary>
        public object UpdateCustomer(int id, UpdateCustomerDto dto)
        {
            var result = _service.UpdateCustomer(id, dto);
            if (!result.Succeeded)
                return new { success = false, error = result.Error?.Message };
            return new { success = true, data = result.Data };
        }

        /// <summary>
        /// GET /customers - Return as DTOs
        /// </summary>
        public object GetAllCustomers()
        {
            var result = _service.GetAllCustomers();
            if (!result.Succeeded)
                return new { success = false, error = result.Error?.Message };
            return new { success = true, data = result.Data };
        }
    }
}
