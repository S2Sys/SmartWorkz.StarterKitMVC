namespace SmartWorkz.Core;

/// <summary>
/// Marker interface for all domain service classes.
/// </summary>
/// <remarks>
/// This is the base contract for all service implementations in SmartWorkz.Core.
///
/// Service Registration: Services implementing IService should be registered in the DI container
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
/// Typical Usage: This interface is typically not implemented directly. Instead, implement
/// specific service interfaces (IEmailService, ICacheService, etc.) that inherit from IService
/// or use the generic IService&lt;TEntity, TDto&gt; for CRUD operations.
/// </remarks>
public interface IService
{
}

/// <summary>
/// Generic service interface for CRUD operations on domain entities.
/// </summary>
/// <typeparam name="TEntity">The domain entity type (must implement IEntity&lt;int&gt;).</typeparam>
/// <typeparam name="TDto">The data transfer object type used in API contracts.</typeparam>
/// <remarks>
/// This interface provides standard Create, Read, Update, Delete (CRUD) operations with
/// automatic mapping between domain entities and DTOs.
///
/// Entity vs DTO: TEntity represents the domain model stored in the database, while TDto
/// represents the API contract exposed to clients. The service automatically maps between
/// these two models, keeping domain logic separate from API concerns.
///
/// Error Handling: All operations return Result&lt;TDto&gt; or Result&lt;bool&gt; to communicate
/// failures. Common error codes include ENTITY_NOT_FOUND (when an entity cannot be located),
/// and INVALID_INPUT (when validation fails).
///
/// Async Behavior: All operations are Task-based and support CancellationToken for cancellation.
/// Operations are not fire-and-forget; callers must await them to ensure completion.
///
/// Pagination: This interface does not include pagination methods. For large datasets, consider
/// creating a specialized service interface that adds GetPageAsync(int pageNumber, int pageSize)
/// or GetPageAsync(Expression&lt;Func&lt;TEntity, bool&gt;&gt; predicate, int pageNumber, int pageSize).
///
/// Example:
/// <code>
/// public class ProductService : ServiceBase&lt;Product, ProductDto&gt;
/// {
///     public ProductService(IRepository&lt;Product, int&gt; repository) : base(repository) { }
///
///     protected override ProductDto Map(Product entity) => new ProductDto { ... };
///     protected override Product MapToEntity(ProductDto dto) => new Product { ... };
/// }
///
/// // Usage
/// var result = await productService.GetByIdAsync(1);
/// if (result.IsSuccess)
/// {
///     var productDto = result.Value;
///     logger.LogInformation($"Retrieved product: {productDto.Name}");
/// }
/// else
/// {
///     logger.LogError($"Failed to retrieve product: {result.Error.Message}");
/// }
/// </code>
/// </remarks>
public interface IService<TEntity, TDto> where TEntity : class, IEntity<int>
{
    /// <summary>
    /// Retrieves an entity by its identifier and returns the corresponding DTO.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// Success: A Result containing the mapped DTO.
    /// Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
    /// </returns>
    /// <remarks>
    /// This method performs an asynchronous database lookup and maps the result to a DTO.
    /// If the entity is not found, it returns a failed Result with error code ENTITY_NOT_FOUND
    /// rather than throwing an exception.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await userService.GetByIdAsync(userId);
    /// if (result.IsSuccess)
    /// {
    ///     var userDto = result.Value;
    ///     Console.WriteLine($"User: {userDto.Name}");
    /// }
    /// else
    /// {
    ///     logger.LogWarning($"User not found: {result.Error.Message}");
    /// }
    /// </code>
    /// </example>
    Task<Result<TDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities and returns them as a read-only collection of DTOs.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// Success: A Result containing a read-only collection of mapped DTOs.
    /// This collection is never null; an empty collection is returned if no entities exist.
    /// </returns>
    /// <remarks>
    /// This method retrieves all entities from the database without filtering.
    /// For large tables, consider adding a pagination method to GetAllAsync to avoid
    /// loading all records into memory.
    ///
    /// Warning: Use with caution on large datasets. The entire collection is loaded
    /// into memory before being returned.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await productService.GetAllAsync();
    /// if (result.IsSuccess)
    /// {
    ///     foreach (var productDto in result.Value)
    ///     {
    ///         Console.WriteLine($"Product: {productDto.Name}");
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<Result<IReadOnlyCollection<TDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity from the provided DTO and persists it to the database.
    /// </summary>
    /// <param name="dto">The data transfer object containing entity data. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// Success: A Result containing the newly created entity mapped to a DTO.
    /// Failure: A Result with error details if creation fails (e.g., validation errors).
    /// </returns>
    /// <remarks>
    /// This method maps the DTO to a domain entity, persists it to the database,
    /// and returns the created entity (with any database-generated values like ID or timestamps).
    ///
    /// Validation: The service should validate the DTO before mapping and persisting.
    /// If validation fails, a failed Result is returned.
    ///
    /// Error Handling: Throws ArgumentNullException if dto is null (this is checked before
    /// mapping to the domain model).
    /// </remarks>
    /// <example>
    /// <code>
    /// var newProductDto = new ProductDto { Name = "Widget", Price = 9.99m };
    /// var result = await productService.CreateAsync(newProductDto);
    /// if (result.IsSuccess)
    /// {
    ///     var createdDto = result.Value;
    ///     logger.LogInformation($"Created product with ID: {createdDto.Id}");
    /// }
    /// else
    /// {
    ///     logger.LogError($"Failed to create product: {result.Error.Message}");
    /// }
    /// </code>
    /// </example>
    Task<Result<TDto>> CreateAsync(TDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity with data from the provided DTO.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to update.</param>
    /// <param name="dto">The data transfer object containing updated values. Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// Success: A Result containing the updated entity mapped to a DTO.
    /// Failure: A Result with error code ENTITY_NOT_FOUND if the entity does not exist,
    /// or with other error details if the update fails.
    /// </returns>
    /// <remarks>
    /// This method retrieves the entity, applies updates from the DTO, and persists the changes.
    /// If the entity is not found, a failed Result is returned with code ENTITY_NOT_FOUND.
    ///
    /// Update Logic: The actual update logic is delegated to the ApplyUpdates method,
    /// which can be overridden in derived classes for custom update behavior.
    ///
    /// Error Handling: Throws ArgumentNullException if dto is null.
    /// </remarks>
    /// <example>
    /// <code>
    /// var updatedDto = new ProductDto { Name = "Updated Widget", Price = 12.99m };
    /// var result = await productService.UpdateAsync(productId, updatedDto);
    /// if (result.IsSuccess)
    /// {
    ///     logger.LogInformation($"Updated product: {result.Value.Name}");
    /// }
    /// else if (result.Error.Code == "ENTITY_NOT_FOUND")
    /// {
    ///     logger.LogWarning($"Product not found for update");
    /// }
    /// </code>
    /// </example>
    Task<Result<TDto>> UpdateAsync(int id, TDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// Success: A Result containing true if deletion succeeded.
    /// Failure: A Result with error code ENTITY_NOT_FOUND if the entity does not exist.
    /// </returns>
    /// <remarks>
    /// This method removes the entity from the database.
    /// If the entity is not found, a failed Result is returned.
    ///
    /// Soft Delete: If your application uses soft deletes, override this method in
    /// derived classes to mark the entity as deleted instead of removing it.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await productService.DeleteAsync(productId);
    /// if (result.IsSuccess)
    /// {
    ///     logger.LogInformation($"Product deleted successfully");
    /// }
    /// else if (result.Error.Code == "ENTITY_NOT_FOUND")
    /// {
    ///     logger.LogWarning($"Product not found for deletion");
    /// }
    /// </code>
    /// </example>
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
