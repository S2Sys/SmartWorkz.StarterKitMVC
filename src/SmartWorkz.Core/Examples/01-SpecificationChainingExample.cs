namespace SmartWorkz.Core.Examples;

using SmartWorkz.Shared;
using System.Linq.Expressions;

/// <summary>
/// Demonstrates the Specification pattern for building composable, type-safe queries.
/// Specifications encapsulate query logic (filtering, sorting, eager loading) into
/// reusable, testable components without coupling to specific repositories.
/// </summary>
/// <remarks>
/// The Specification pattern provides:
/// - Reusable query logic across multiple services
/// - Testable queries without database dependencies
/// - Composable filters through And/Or/Not
/// - Transparent eager loading (Includes)
/// - Consistent sorting, pagination, and filtering
///
/// Specification Pattern Flow:
/// 1. Define entity-specific Specification class (ProductSpecification)
/// 2. Add fluent methods for common filters (WithCategory, WithPriceRange)
/// 3. Combine methods with chaining (spec.WithCategory(id).WithAvailable())
/// 4. Pass spec to repository (repository.FindAllAsync(spec))
/// 5. Repository applies filters, includes, sorting, pagination transparently
///
/// Benefits Over Raw Queries:
/// - Query logic is testable without database
/// - Reusable across multiple services/repositories
/// - Type-safe: Compile-time checking of filter properties
/// - Maintainable: Changes to filters centralized in Specification
/// - Discoverable: IDE autocomplete for available filters
/// </remarks>
public class SpecificationChainingExample
{
    /// <summary>
    /// Product entity for e-commerce domain.
    /// </summary>
    public class Product : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; }
        public int StockQuantity { get; set; }
    }

    /// <summary>
    /// Product category entity.
    /// </summary>
    public class Category : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Specification for Product queries with fluent filter methods.
    /// Each method adds a filter criterion and returns this for chaining.
    /// </summary>
    public class ProductSpecification : Specification<Product>
    {
        /// <summary>
        /// Adds filter: WHERE CategoryId = categoryId
        /// </summary>
        public ProductSpecification WithCategory(int categoryId)
        {
            AddCriteria(p => p.CategoryId == categoryId);
            return this;
        }

        /// <summary>
        /// Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
        /// </summary>
        public ProductSpecification WithPriceRange(decimal minPrice, decimal maxPrice)
        {
            AddCriteria(p => p.Price >= minPrice && p.Price <= maxPrice);
            return this;
        }

        /// <summary>
        /// Adds filter: WHERE IsAvailable = true
        /// </summary>
        public ProductSpecification WithAvailableOnly()
        {
            AddCriteria(p => p.IsAvailable);
            return this;
        }

        /// <summary>
        /// Adds filter: WHERE StockQuantity > minStock
        /// </summary>
        public ProductSpecification WithInStock(int minStock = 1)
        {
            AddCriteria(p => p.StockQuantity >= minStock);
            return this;
        }

        /// <summary>
        /// Adds ordering: ORDER BY Price ASC
        /// </summary>
        public ProductSpecification OrderByPrice()
        {
            ApplyOrderBy(p => p.Price);
            return this;
        }

        /// <summary>
        /// Adds ordering: ORDER BY Price DESC
        /// </summary>
        public ProductSpecification OrderByPriceDesc()
        {
            ApplyOrderByDescending(p => p.Price);
            return this;
        }

        /// <summary>
        /// Adds ordering: ORDER BY Name ASC
        /// </summary>
        public ProductSpecification OrderByName()
        {
            ApplyOrderBy(p => p.Name);
            return this;
        }

        /// <summary>
        /// Applies pagination: OFFSET skip LIMIT take
        /// </summary>
        public ProductSpecification WithPaging(int pageNumber, int pageSize)
        {
            // Convert page number to skip: Page 1 = skip 0, Page 2 = skip pageSize, etc.
            var skip = (pageNumber - 1) * pageSize;
            ApplyPaging(skip, pageSize);
            return this;
        }
    }

    /// <summary>
    /// Demonstrates building and using specifications.
    /// </summary>
    public class SpecificationUsagePatterns
    {
        /// <summary>
        /// Example 1: Simple specification - find available products in category.
        /// </summary>
        public void Example_SimpleFilter()
        {
            // Build specification with chained filters
            var spec = new ProductSpecification()
                .WithCategory(categoryId: 5)
                .WithAvailableOnly();

            // Usage in repository (conceptual):
            // var products = await repository.FindAllAsync(spec);
            // SQL: WHERE CategoryId = 5 AND IsAvailable = true

            PrintExample(nameof(Example_SimpleFilter), spec);
        }

        /// <summary>
        /// Example 2: Complex specification - affordable, available products with pagination.
        /// </summary>
        public void Example_ComplexFilter()
        {
            // Combine multiple filters
            var spec = new ProductSpecification()
                .WithPriceRange(minPrice: 10m, maxPrice: 50m)
                .WithAvailableOnly()
                .WithInStock(minStock: 5)
                .OrderByPrice()
                .WithPaging(pageNumber: 1, pageSize: 20);

            // Usage:
            // var products = await repository.FindAllAsync(spec);
            // SQL: WHERE Price BETWEEN 10 AND 50 AND IsAvailable = true AND StockQuantity >= 5
            //      ORDER BY Price ASC OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY

            PrintExample(nameof(Example_ComplexFilter), spec);
        }

        /// <summary>
        /// Example 3: Premium products - expensive, in-stock items.
        /// </summary>
        public void Example_PremiumProducts()
        {
            var spec = new ProductSpecification()
                .WithCategory(categoryId: 10)
                .WithPriceRange(minPrice: 100m, maxPrice: decimal.MaxValue)
                .WithInStock()
                .OrderByPriceDesc();

            // Usage:
            // var premiumProducts = await repository.FindAllAsync(spec);

            PrintExample(nameof(Example_PremiumProducts), spec);
        }

        /// <summary>
        /// Example 4: Specification reuse across multiple queries.
        /// Single specification definition, many uses.
        /// </summary>
        public void Example_SpecificationReuse()
        {
            // Define once
            var baseSpec = new ProductSpecification()
                .WithAvailableOnly()
                .WithInStock();

            // Use 1: Count items matching spec
            // var count = await repository.CountAsync(baseSpec);

            // Use 2: Check if exists
            // var exists = await repository.ExistsAsync(baseSpec);

            // Use 3: Find first matching
            // var first = await repository.FindAsync(baseSpec);

            // Use 4: Find all matching
            // var all = await repository.FindAllAsync(baseSpec);

            PrintExample(nameof(Example_SpecificationReuse), baseSpec);
        }

        private static void PrintExample(string name, Specification<Product> spec)
        {
            System.Diagnostics.Debug.WriteLine($"Example: {name}");
            System.Diagnostics.Debug.WriteLine($"  Criteria count: {spec.Criteria.Count}");
            System.Diagnostics.Debug.WriteLine($"  Has ordering: {spec.OrderBy != null || spec.OrderByDescending != null}");
            System.Diagnostics.Debug.WriteLine($"  Paging enabled: {spec.IsPagingEnabled}");
        }
    }

    /// <summary>
    /// Demonstrates combining specifications with And/Or/Not operators.
    /// </summary>
    public class SpecificationCombination
    {
        /// <summary>
        /// Example: Combine multiple specifications with AND (all conditions must match).
        /// </summary>
        public void Example_And()
        {
            var categorySpec = new ProductSpecification()
                .WithCategory(5);

            var priceSpec = new ProductSpecification()
                .WithPriceRange(10m, 50m);

            // Combine with AND: Products in category 5 AND price between 10-50
            var combined = categorySpec.And(priceSpec);

            // Usage: var products = await repository.FindAllAsync(combined);
        }

        /// <summary>
        /// Example: Combine specifications with OR (at least one condition must match).
        /// </summary>
        public void Example_Or()
        {
            var expensiveSpec = new ProductSpecification()
                .WithPriceRange(100m, decimal.MaxValue);

            var baseSpec = new ProductSpecification()
                .WithCategory(5);

            // Combine with OR: Category 5 OR Price > 100
            var combined = baseSpec.Or(expensiveSpec);

            // Usage: var products = await repository.FindAllAsync(combined);
        }

        /// <summary>
        /// Example: Negate specification with NOT (inverse matching).
        /// </summary>
        public void Example_Not()
        {
            var availableSpec = new ProductSpecification()
                .WithAvailableOnly();

            // Negate: Find products that are NOT available
            var unavailableSpec = availableSpec.Not();

            // Usage: var products = await repository.FindAllAsync(unavailableSpec);
        }
    }
}
