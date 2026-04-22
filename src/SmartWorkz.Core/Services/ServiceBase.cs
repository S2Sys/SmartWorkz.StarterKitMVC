using System.Linq.Expressions;

namespace SmartWorkz.Core;

/// <summary>
/// Marker base class for all domain services.
/// </summary>
/// <remarks>
/// This is the base contract for all service implementations in SmartWorkz.Core.
///
/// Service Registration: Services implementing ServiceBase should be registered in the DI container
/// with the appropriate lifetime (Transient, Scoped, or Singleton) based on their dependencies
/// and usage patterns.
///
/// Error Handling: Services typically use the Result pattern to communicate failures without
/// throwing exceptions for expected error scenarios. Unexpected exceptions may still be thrown
/// for critical, non-recoverable errors.
///
/// Async Behavior: Most services are async-first, using Task-based methods with proper
/// CancellationToken support for graceful cancellation and resource cleanup.
///
/// Typical Usage: This class is typically not used directly. Instead, inherit from
/// ServiceBase&lt;TEntity, TDto&gt; to implement CRUD operations on domain entities,
/// or create specialized service classes for specific domain logic.
/// </remarks>
public abstract class ServiceBase : IService
{
    protected ServiceBase()
    {
    }
}

/// <summary>
/// Generic base class for CRUD services implementing domain entity operations.
/// </summary>
/// <typeparam name="TEntity">The domain entity type (must implement IEntity&lt;int&gt;).</typeparam>
/// <typeparam name="TDto">The data transfer object type used in API contracts.</typeparam>
/// <remarks>
/// This class provides a reusable foundation for implementing CRUD (Create, Read, Update, Delete)
/// operations on domain entities with automatic mapping between entities and DTOs.
///
/// Architecture:
/// - Async-First: All operations are Task-based to prevent thread pool exhaustion and support
///   proper cancellation via CancellationToken.
/// - Dependency Injection: IRepository&lt;TEntity, int&gt; is injected via constructor, following
///   constructor-based DI patterns for predictable initialization.
/// - Error Handling: Business logic failures (e.g., entity not found) return Result&lt;T&gt;
///   with error codes rather than throwing exceptions. Infrastructure failures (connection
///   errors, etc.) may still throw and should be logged.
///
/// Entity vs DTO Philosophy:
/// - TEntity: Domain entity with business logic, validation, and state management.
///   Stored in the database and not exposed directly to API clients.
/// - TDto: Data transfer object used in HTTP requests and responses.
///   Represents the public API contract and may omit sensitive properties.
/// - Mapping: Derived classes implement Map() and MapToEntity() to convert between
///   entities and DTOs. Use a mapping library (e.g., AutoMapper) if handling many entities.
///
/// CRUD Lifecycle:
/// 1. Create: DTO -> Entity (MapToEntity) -> Repository.AddAsync -> Result with mapped DTO
/// 2. Read: Repository.GetByIdAsync -> Map to DTO -> Result with DTO
/// 3. Update: Existing Entity + DTO -> ApplyUpdates -> Repository.UpdateAsync -> Result with mapped DTO
/// 4. Delete: Repository.GetByIdAsync -> Repository.DeleteAsync -> Result&lt;bool&gt;
///
/// Customization: Derive from this class and:
/// - Implement Map() to convert TEntity -> TDto
/// - Implement MapToEntity() to convert TDto -> TEntity
/// - Override ApplyUpdates() to apply DTO properties to an existing entity (default is no-op)
/// - Override any method to add custom validation or business logic
///
/// Example:
/// <code>
/// public class ProductService : ServiceBase&lt;Product, ProductDto&gt;
/// {
///     public ProductService(IRepository&lt;Product, int&gt; repository) : base(repository) { }
///
///     protected override ProductDto Map(Product entity) =>
///         new ProductDto { Id = entity.Id, Name = entity.Name, Price = entity.Price };
///
///     protected override Product MapToEntity(ProductDto dto) =>
///         new Product { Name = dto.Name, Price = dto.Price };
///
///     protected override void ApplyUpdates(Product entity, ProductDto dto)
///     {
///         entity.Name = dto.Name;
///         entity.Price = dto.Price;
///     }
/// }
///
/// // Usage
/// var result = await productService.GetByIdAsync(123);
/// if (result.IsSuccess)
///     Console.WriteLine($"Product: {result.Value.Name}");
/// else
///     logger.LogError($"Failed: {result.Error.Message}");
/// </code>
/// </remarks>
public abstract class ServiceBase<TEntity, TDto> : IService<TEntity, TDto>
    where TEntity : class, IEntity<int>
{
    protected readonly IRepository<TEntity, int> Repository;

    /// <summary>
    /// Initializes a new instance of the ServiceBase class.
    /// </summary>
    /// <param name="repository">The repository for entity data access. Must not be null.</param>
    /// <remarks>
    /// The repository is stored as a protected field for use by derived classes.
    /// Guard.NotNull() validates the repository at construction time to fail fast
    /// if a null repository is provided.
    /// </remarks>
    protected ServiceBase(IRepository<TEntity, int> repository)
    {
        Repository = Guard.NotNull(repository, nameof(repository));
    }

    /// <summary>
    /// Retrieves an entity by its identifier and returns the corresponding DTO.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve. Must not be zero or default.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation. Defaults to no cancellation.</param>
    /// <returns>
    /// Success: A Result containing the mapped DTO of the entity.
    /// Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
    /// </returns>
    /// <remarks>
    /// This is a read-only operation that performs an asynchronous database lookup.
    /// The entity is mapped to a DTO before being returned, ensuring the domain model
    /// is not exposed directly. If the entity is not found, a failed Result is returned
    /// with error code ENTITY_NOT_FOUND rather than throwing an exception.
    ///
    /// Validation: The id parameter is validated to ensure it is not the default value
    /// (typically 0 for integers), using Guard.NotDefault().
    ///
    /// Database Query: The actual database query is deferred to the Repository.GetByIdAsync()
    /// method, which may use indexed lookups for efficient retrieval.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Retrieve a product by ID
    /// var result = await productService.GetByIdAsync(123);
    /// if (result.IsSuccess)
    /// {
    ///     var productDto = result.Value;
    ///     Console.WriteLine($"Product: {productDto.Name} - {productDto.Price:C}");
    /// }
    /// else
    /// {
    ///     logger.LogWarning($"Product not found: {result.Error.Message}");
    /// }
    /// </code>
    /// </example>
    public virtual async Task<Result<TDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.NotDefault(id, nameof(id));

        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Fail<TDto>(new Error("ENTITY_NOT_FOUND", $"{typeof(TEntity).Name} not found"));

        return Result.Ok(Map(entity));
    }

    /// <summary>
    /// Retrieves all entities and returns them as a read-only collection of DTOs.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation. Defaults to no cancellation.</param>
    /// <returns>
    /// Success: A Result containing a read-only collection of mapped DTOs representing all entities.
    /// If no entities exist, an empty read-only collection is returned.
    /// </returns>
    /// <remarks>
    /// This operation retrieves all entities from the database without filtering or pagination.
    /// For large datasets, consider creating a specialized service interface with GetPageAsync()
    /// or GetFilteredAsync(Expression&lt;Func&lt;TEntity, bool&gt;&gt; predicate) methods.
    ///
    /// Mapping: Each entity is mapped to a DTO using the Map() method before being returned.
    /// This ensures the domain model is not exposed directly to API clients.
    ///
    /// Collection Immutability: The returned collection is wrapped with AsReadOnly() to prevent
    /// accidental modifications by the caller.
    ///
    /// Performance: This operation loads all entities into memory. For large tables, consider
    /// adding pagination or filtering to reduce the dataset size.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Retrieve all products
    /// var result = await productService.GetAllAsync();
    /// if (result.IsSuccess)
    /// {
    ///     foreach (var productDto in result.Value)
    ///         Console.WriteLine($"Product: {productDto.Name} - {productDto.Price:C}");
    /// }
    /// else
    /// {
    ///     logger.LogError("Failed to retrieve products");
    /// }
    /// </code>
    /// </example>
    public virtual async Task<Result<IReadOnlyCollection<TDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetAllAsync(cancellationToken);
        var dtos = entities.Select(Map).ToList().AsReadOnly();
        return Result.Ok<IReadOnlyCollection<TDto>>(dtos);
    }

    /// <summary>
    /// Creates a new entity from the provided DTO and persists it to the database.
    /// </summary>
    /// <param name="dto">The data transfer object containing the entity properties. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation. Defaults to no cancellation.</param>
    /// <returns>
    /// Success: A Result containing the mapped DTO of the newly created entity (with database-generated values).
    /// Failure: The method throws ArgumentNullException if dto is null (infrastructure failure).
    /// </returns>
    /// <remarks>
    /// Creation Workflow:
    /// 1. MapToEntity(): Converts the input DTO to a domain entity
    /// 2. Repository.AddAsync(): Adds the entity to the database (may not commit immediately)
    /// 3. Map(): Converts the entity back to a DTO, including any database-generated values (e.g., Id, timestamps)
    /// 4. Result.Ok(): Returns the mapped DTO
    ///
    /// Database-Generated Values: The returned DTO includes values generated by the database during insertion,
    /// such as the primary key (if using identity columns) or audit fields (CreatedAt, CreatedBy).
    ///
    /// Validation: The DTO is validated to ensure it is not null. Additional entity validation should be
    /// implemented in MapToEntity() or in the entity constructor if using Domain-Driven Design principles.
    ///
    /// Unit of Work Pattern: The actual database commit depends on the underlying repository and Unit of Work
    /// implementation. The caller may need to call SaveChangesAsync() or similar on a Unit of Work to commit
    /// the transaction.
    ///
    /// Error Handling: If dto is null, ArgumentNullException is thrown (fail-fast). Database constraints
    /// violations may raise database-specific exceptions that propagate to the caller.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a new product
    /// var newProductDto = new ProductDto { Name = "Laptop", Price = 999.99m };
    /// var result = await productService.CreateAsync(newProductDto);
    /// if (result.IsSuccess)
    /// {
    ///     var createdProduct = result.Value;
    ///     Console.WriteLine($"Created product with ID: {createdProduct.Id}");
    /// }
    /// else
    /// {
    ///     logger.LogError("Failed to create product");
    /// }
    /// </code>
    /// </example>
    public virtual async Task<Result<TDto>> CreateAsync(TDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var entity = MapToEntity(dto);
        await Repository.AddAsync(entity, cancellationToken);
        return Result.Ok(Map(entity));
    }

    /// <summary>
    /// Updates an existing entity with values from the provided DTO.
    /// </summary>
    /// <param name="id">The identifier of the entity to update. Must not be zero or default.</param>
    /// <param name="dto">The data transfer object containing the updated entity properties. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation. Defaults to no cancellation.</param>
    /// <returns>
    /// Success: A Result containing the mapped DTO of the updated entity.
    /// Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
    /// </returns>
    /// <remarks>
    /// Update Workflow:
    /// 1. Validate id is not default and dto is not null
    /// 2. Repository.GetByIdAsync(): Retrieves the existing entity
    /// 3. ApplyUpdates(): Applies DTO properties to the existing entity
    /// 4. Repository.UpdateAsync(): Persists the changes
    /// 5. Map(): Converts the updated entity back to a DTO
    /// 6. Result.Ok(): Returns the mapped DTO
    ///
    /// Concurrency: This implementation uses "Last-Write-Wins" concurrency. If another user updates
    /// the entity between the read and write, the latest update will be overwritten. For optimistic
    /// concurrency control, add a ConcurrencyToken (e.g., RowVersion) to the entity and check it
    /// before applying updates.
    ///
    /// ApplyUpdates(): Derived classes must override ApplyUpdates() to apply DTO properties to the
    /// entity. The default implementation is a no-op. Example:
    /// <code>
    /// protected override void ApplyUpdates(Product entity, ProductDto dto)
    /// {
    ///     entity.Name = dto.Name;
    ///     entity.Price = dto.Price;
    /// }
    /// </code>
    ///
    /// Audit Trail: If the entity implements IAuditable, the UpdatedAt and UpdatedBy fields should
    /// be set by the ApplyUpdates() method or the application layer before calling UpdateAsync().
    ///
    /// Validation: If dto is null, ArgumentNullException is thrown (fail-fast).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Update an existing product
    /// var updatedProductDto = new ProductDto { Name = "Updated Laptop", Price = 1099.99m };
    /// var result = await productService.UpdateAsync(123, updatedProductDto);
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine($"Product updated: {result.Value.Name}");
    /// }
    /// else
    /// {
    ///     logger.LogWarning($"Failed to update product: {result.Error.Message}");
    /// }
    /// </code>
    /// </example>
    public virtual async Task<Result<TDto>> UpdateAsync(int id, TDto dto, CancellationToken cancellationToken = default)
    {
        Guard.NotDefault(id, nameof(id));
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Fail<TDto>(new Error("ENTITY_NOT_FOUND", $"{typeof(TEntity).Name} not found"));

        ApplyUpdates(entity, dto);
        await Repository.UpdateAsync(entity, cancellationToken);
        return Result.Ok(Map(entity));
    }

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete. Must not be zero or default.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation. Defaults to no cancellation.</param>
    /// <returns>
    /// Success: A Result containing true if the entity was successfully deleted.
    /// Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
    /// </returns>
    /// <remarks>
    /// Delete Workflow:
    /// 1. Validate id is not default
    /// 2. Repository.GetByIdAsync(): Verifies the entity exists before attempting deletion
    /// 3. Repository.DeleteAsync(): Removes the entity from the database
    /// 4. Result.Ok(true): Returns success
    ///
    /// Soft Delete: If the entity implements ISoftDeletable (has IsDeleted property), the repository
    /// may perform a soft delete (mark IsDeleted = true, set DeletedAt) instead of a hard delete
    /// (remove from database). Verify the repository implementation for specific behavior.
    ///
    /// Cascade Behavior: Database cascade rules (e.g., ON DELETE CASCADE) apply if configured in
    /// the database schema or Entity Framework. Orphaned child entities may be deleted automatically.
    ///
    /// Verification: The method verifies the entity exists before attempting deletion to provide
    /// a clearer error message (ENTITY_NOT_FOUND) rather than a silent no-op or constraint violation.
    ///
    /// Audit Trail: If the entity implements IAuditable, the DeletedAt and DeletedBy fields (if present)
    /// should be set by the repository during soft delete.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Delete a product
    /// var result = await productService.DeleteAsync(123);
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine("Product deleted successfully");
    /// }
    /// else
    /// {
    ///     logger.LogWarning($"Failed to delete product: {result.Error.Message}");
    /// }
    /// </code>
    /// </example>
    public virtual async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.NotDefault(id, nameof(id));

        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Fail<bool>(new Error("ENTITY_NOT_FOUND", $"{typeof(TEntity).Name} not found"));

        await Repository.DeleteAsync(id, cancellationToken);
        return Result.Ok(true);
    }

    /// <summary>
    /// Maps a domain entity to its corresponding DTO.
    /// </summary>
    /// <param name="entity">The domain entity to map. Must not be null.</param>
    /// <returns>The mapped data transfer object.</returns>
    /// <remarks>
    /// Mapping Strategy: This abstract method must be implemented by derived classes to convert
    /// a domain entity to a DTO. The implementation should map all relevant entity properties
    /// to the DTO.
    ///
    /// One-Way Mapping: The DTO may omit sensitive properties (e.g., passwords, audit fields)
    /// that should not be exposed to API clients. This is a one-way projection, not a round-trip mapping.
    ///
    /// Example Implementation:
    /// <code>
    /// protected override ProductDto Map(Product entity) =>
    ///     new ProductDto
    ///     {
    ///         Id = entity.Id,
    ///         Name = entity.Name,
    ///         Price = entity.Price,
    ///         CategoryId = entity.CategoryId
    ///     };
    /// </code>
    ///
    /// Alternative: For complex mappings, consider using a mapping library like AutoMapper:
    /// <code>
    /// protected override ProductDto Map(Product entity) =>
    ///     _mapper.Map<ProductDto>(entity);
    /// </code>
    /// </remarks>
    protected abstract TDto Map(TEntity entity);

    /// <summary>
    /// Maps a data transfer object to its corresponding domain entity.
    /// </summary>
    /// <param name="dto">The data transfer object to map. Must not be null.</param>
    /// <returns>The mapped domain entity.</returns>
    /// <remarks>
    /// Mapping Strategy: This abstract method must be implemented by derived classes to convert
    /// a DTO to a domain entity. The implementation should map all relevant DTO properties
    /// to the entity and initialize default values for properties not provided by the DTO.
    ///
    /// Default Values: Properties not provided by the DTO (e.g., Id, audit fields, timestamps)
    /// should be initialized to their default values. The database (or repository layer) may
    /// override these values during persistence (e.g., generating an Id).
    ///
    /// Validation: Entity validation should be performed during construction or in a separate
    /// validation method called before persistence.
    ///
    /// Example Implementation:
    /// <code>
    /// protected override Product MapToEntity(ProductDto dto) =>
    ///     new Product
    ///     {
    ///         Name = dto.Name,
    ///         Price = dto.Price,
    ///         CategoryId = dto.CategoryId
    ///     };
    /// </code>
    ///
    /// Alternative: For complex mappings, consider using a mapping library like AutoMapper:
    /// <code>
    /// protected override Product MapToEntity(ProductDto dto) =>
    ///     _mapper.Map<Product>(dto);
    /// </code>
    /// </remarks>
    protected abstract TEntity MapToEntity(TDto dto);

    /// <summary>
    /// Applies properties from a DTO to an existing entity.
    /// </summary>
    /// <param name="entity">The existing domain entity to update. Must not be null.</param>
    /// <param name="dto">The data transfer object containing updated properties. Must not be null.</param>
    /// <remarks>
    /// Update Strategy: This method is called during UpdateAsync() to apply DTO properties to an
    /// existing entity. The default implementation is a no-op (does nothing), so derived classes
    /// must override this method to apply updates.
    ///
    /// Selective Updates: This method allows selective property updates without recreating the entity.
    /// Only properties that should be updatable should be applied here. Read-only properties
    /// (e.g., Id, CreatedAt, CreatedBy) should be excluded.
    ///
    /// Audit Fields: Audit fields (UpdatedAt, UpdatedBy) should typically be set by the application
    /// layer (e.g., in a service method) or by an interceptor before SaveAsync() is called, not here.
    ///
    /// Example Implementation:
    /// <code>
    /// protected override void ApplyUpdates(Product entity, ProductDto dto)
    /// {
    ///     entity.Name = dto.Name;
    ///     entity.Price = dto.Price;
    ///     entity.CategoryId = dto.CategoryId;
    ///     entity.UpdatedAt = DateTime.UtcNow;
    ///     entity.UpdatedBy = GetCurrentUserId();
    /// }
    /// </code>
    /// </remarks>
    protected virtual void ApplyUpdates(TEntity entity, TDto dto) { }
}
