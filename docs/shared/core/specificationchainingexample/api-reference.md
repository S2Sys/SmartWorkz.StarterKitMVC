# SpecificationChainingExample API Reference

## Classes & Interfaces

### Product

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.Product`
- **Summary:** Product entity for e-commerce domain.

### Category

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.Category`
- **Summary:** Product category entity.

### ProductSpecification

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.ProductSpecification`
- **Summary:** Specification for Product queries with fluent filter methods.
            Each method adds a filter criterion and returns this for chaining.

#### Methods & Properties

- **WithCategory** - Adds filter: WHERE CategoryId = categoryId
- **WithPriceRange** - Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
- **WithAvailableOnly** - Adds filter: WHERE IsAvailable = true
- **WithInStock** - Adds filter: WHERE StockQuantity > minStock
- **OrderByPrice** - Adds ordering: ORDER BY Price ASC
- **OrderByPriceDesc** - Adds ordering: ORDER BY Price DESC
- **OrderByName** - Adds ordering: ORDER BY Name ASC
- **WithPaging** - Applies pagination: OFFSET skip LIMIT take

### SpecificationUsagePatterns

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.SpecificationUsagePatterns`
- **Summary:** Demonstrates building and using specifications.

#### Methods & Properties

- **Example_SimpleFilter** - Example 1: Simple specification - find available products in category.
- **Example_ComplexFilter** - Example 2: Complex specification - affordable, available products with pagination.
- **Example_PremiumProducts** - Example 3: Premium products - expensive, in-stock items.
- **Example_SpecificationReuse** - Example 4: Specification reuse across multiple queries.
            Single specification definition, many uses.

### SpecificationCombination

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.SpecificationCombination`
- **Summary:** Demonstrates combining specifications with And/Or/Not operators.

#### Methods & Properties

- **Example_And** - Example: Combine multiple specifications with AND (all conditions must match).
- **Example_Or** - Example: Combine specifications with OR (at least one condition must match).
- **Example_Not** - Example: Negate specification with NOT (inverse matching).

### Product

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.Product`
- **Summary:** Product entity for e-commerce domain.

### Category

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.Category`
- **Summary:** Product category entity.

### ProductSpecification

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.ProductSpecification`
- **Summary:** Specification for Product queries with fluent filter methods.
            Each method adds a filter criterion and returns this for chaining.

#### Methods & Properties

- **WithCategory** - Adds filter: WHERE CategoryId = categoryId
- **WithPriceRange** - Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
- **WithAvailableOnly** - Adds filter: WHERE IsAvailable = true
- **WithInStock** - Adds filter: WHERE StockQuantity > minStock
- **OrderByPrice** - Adds ordering: ORDER BY Price ASC
- **OrderByPriceDesc** - Adds ordering: ORDER BY Price DESC
- **OrderByName** - Adds ordering: ORDER BY Name ASC
- **WithPaging** - Applies pagination: OFFSET skip LIMIT take

### SpecificationUsagePatterns

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.SpecificationUsagePatterns`
- **Summary:** Demonstrates building and using specifications.

#### Methods & Properties

- **Example_SimpleFilter** - Example 1: Simple specification - find available products in category.
- **Example_ComplexFilter** - Example 2: Complex specification - affordable, available products with pagination.
- **Example_PremiumProducts** - Example 3: Premium products - expensive, in-stock items.
- **Example_SpecificationReuse** - Example 4: Specification reuse across multiple queries.
            Single specification definition, many uses.

### SpecificationCombination

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.SpecificationCombination`
- **Summary:** Demonstrates combining specifications with And/Or/Not operators.

#### Methods & Properties

- **Example_And** - Example: Combine multiple specifications with AND (all conditions must match).
- **Example_Or** - Example: Combine specifications with OR (at least one condition must match).
- **Example_Not** - Example: Negate specification with NOT (inverse matching).

### Product

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.Product`
- **Summary:** Product entity for e-commerce domain.

### Category

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.Category`
- **Summary:** Product category entity.

### ProductSpecification

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.ProductSpecification`
- **Summary:** Specification for Product queries with fluent filter methods.
            Each method adds a filter criterion and returns this for chaining.

#### Methods & Properties

- **WithCategory** - Adds filter: WHERE CategoryId = categoryId
- **WithPriceRange** - Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
- **WithAvailableOnly** - Adds filter: WHERE IsAvailable = true
- **WithInStock** - Adds filter: WHERE StockQuantity > minStock
- **OrderByPrice** - Adds ordering: ORDER BY Price ASC
- **OrderByPriceDesc** - Adds ordering: ORDER BY Price DESC
- **OrderByName** - Adds ordering: ORDER BY Name ASC
- **WithPaging** - Applies pagination: OFFSET skip LIMIT take

### SpecificationUsagePatterns

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.SpecificationUsagePatterns`
- **Summary:** Demonstrates building and using specifications.

#### Methods & Properties

- **Example_SimpleFilter** - Example 1: Simple specification - find available products in category.
- **Example_ComplexFilter** - Example 2: Complex specification - affordable, available products with pagination.
- **Example_PremiumProducts** - Example 3: Premium products - expensive, in-stock items.
- **Example_SpecificationReuse** - Example 4: Specification reuse across multiple queries.
            Single specification definition, many uses.

### SpecificationCombination

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.SpecificationCombination`
- **Summary:** Demonstrates combining specifications with And/Or/Not operators.

#### Methods & Properties

- **Example_And** - Example: Combine multiple specifications with AND (all conditions must match).
- **Example_Or** - Example: Combine specifications with OR (at least one condition must match).
- **Example_Not** - Example: Negate specification with NOT (inverse matching).

### Product

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.Product`
- **Summary:** Product entity for e-commerce domain.

### Category

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.Category`
- **Summary:** Product category entity.

### ProductSpecification

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.ProductSpecification`
- **Summary:** Specification for Product queries with fluent filter methods.
            Each method adds a filter criterion and returns this for chaining.

#### Methods & Properties

- **WithCategory** - Adds filter: WHERE CategoryId = categoryId
- **WithPriceRange** - Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
- **WithAvailableOnly** - Adds filter: WHERE IsAvailable = true
- **WithInStock** - Adds filter: WHERE StockQuantity > minStock
- **OrderByPrice** - Adds ordering: ORDER BY Price ASC
- **OrderByPriceDesc** - Adds ordering: ORDER BY Price DESC
- **OrderByName** - Adds ordering: ORDER BY Name ASC
- **WithPaging** - Applies pagination: OFFSET skip LIMIT take

### SpecificationUsagePatterns

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.SpecificationUsagePatterns`
- **Summary:** Demonstrates building and using specifications.

#### Methods & Properties

- **Example_SimpleFilter** - Example 1: Simple specification - find available products in category.
- **Example_ComplexFilter** - Example 2: Complex specification - affordable, available products with pagination.
- **Example_PremiumProducts** - Example 3: Premium products - expensive, in-stock items.
- **Example_SpecificationReuse** - Example 4: Specification reuse across multiple queries.
            Single specification definition, many uses.

### SpecificationCombination

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.SpecificationCombination`
- **Summary:** Demonstrates combining specifications with And/Or/Not operators.

#### Methods & Properties

- **Example_And** - Example: Combine multiple specifications with AND (all conditions must match).
- **Example_Or** - Example: Combine specifications with OR (at least one condition must match).
- **Example_Not** - Example: Negate specification with NOT (inverse matching).

### Product

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.Product`
- **Summary:** Product entity for e-commerce domain.

### Category

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.Category`
- **Summary:** Product category entity.

### ProductSpecification

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.ProductSpecification`
- **Summary:** Specification for Product queries with fluent filter methods.
            Each method adds a filter criterion and returns this for chaining.

#### Methods & Properties

- **WithCategory** - Adds filter: WHERE CategoryId = categoryId
- **WithPriceRange** - Adds filter: WHERE Price BETWEEN minPrice AND maxPrice
- **WithAvailableOnly** - Adds filter: WHERE IsAvailable = true
- **WithInStock** - Adds filter: WHERE StockQuantity > minStock
- **OrderByPrice** - Adds ordering: ORDER BY Price ASC
- **OrderByPriceDesc** - Adds ordering: ORDER BY Price DESC
- **OrderByName** - Adds ordering: ORDER BY Name ASC
- **WithPaging** - Applies pagination: OFFSET skip LIMIT take

### SpecificationUsagePatterns

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.SpecificationUsagePatterns`
- **Summary:** Demonstrates building and using specifications.

#### Methods & Properties

- **Example_SimpleFilter** - Example 1: Simple specification - find available products in category.
- **Example_ComplexFilter** - Example 2: Complex specification - affordable, available products with pagination.
- **Example_PremiumProducts** - Example 3: Premium products - expensive, in-stock items.
- **Example_SpecificationReuse** - Example 4: Specification reuse across multiple queries.
            Single specification definition, many uses.

### SpecificationCombination

- **Namespace:** `SmartWorkz.Core.Examples.SpecificationChainingExample.SpecificationCombination`
- **Summary:** Demonstrates combining specifications with And/Or/Not operators.

#### Methods & Properties

- **Example_And** - Example: Combine multiple specifications with AND (all conditions must match).
- **Example_Or** - Example: Combine specifications with OR (at least one condition must match).
- **Example_Not** - Example: Negate specification with NOT (inverse matching).

