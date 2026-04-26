# Web API Reference

## Classes & Interfaces

### CacheAttribute

- **Namespace:** `SmartWorkz.Web.CacheAttribute`
- **Summary:** Specifies response caching for MVC action methods and controllers.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.

### DataContext`1

- **Namespace:** `SmartWorkz.Web.DataContext`1`
- **Summary:** Manages state and state transitions for grid/list components with async data operations.
            
             DataContext is a state container that coordinates sorting, filtering, pagination, row selection,
             and loading/error states for data display components. It acts as a bridge between the UI
             (through Razor components) and the data layer, managing the current request parameters and
             response data along with user interactions (row selection, state changes).
- **Example:**
```csharp
// Initialize with data
             var context = new DataContext<Product>();
             await context.Initialize(products);
            
             // Subscribe to state changes
             context.OnStateChanged += () => Console.WriteLine("State changed!");
            
             // Filter by category - async operation
             await context.UpdateFilter("Category", "equals", "Electronics");
             // CurrentRequest.Filters now contains { "Category": "Electronics" }
             // CurrentResponse.Data reloaded asynchronously
            
             // Change sort - resets to page 1
             await context.UpdateSort("Price", isDescending: true);
            
             // Change pagination
             await context.UpdatePagination(pageNumber: 2, pageSize: 50);
            
             // Row selection - synchronous operations
             context.ToggleRowSelection(1);          // Select row with ID=1
             context.ToggleSelectAll(true);          // Select all visible rows
             var selected = context.SelectedRowIds;  // List<object> containing IDs
            
             // Check loading state
             if (context.IsLoading)
             {
                 // Show spinner
             }
            
             // Handle errors
             if (context.Error != null)
             {
                 Console.WriteLine($"Error: {context.Error}");
             }
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the DataContext class with default pagination (page 1, page size 20).
            Caches the row ID property via reflection during construction.
- **CacheIdProperty** - Caches the row ID property using reflection. Called during construction.
            Attempts to locate the ID property in this order:
            1. Property decorated with [Key] attribute
            2. Property named "Id" (case-insensitive)
            3. First property on the type
- **Initialize** - Initializes the context with data from a source and applies the current request parameters.
            Sets IsLoading to true during operation, updates CurrentResponse with the result,
            and raises OnStateChanged upon completion.
  - Parameters:
    - `dataSource`: The collection of items to load. Can be empty.
  - Returns: A task representing the asynchronous initialization.
- **UpdateSort** - Updates the sort parameters and reloads data.
            Resets pagination to page 1 when sort changes. Sets IsLoading to true during operation
            and raises OnStateChanged upon completion.
  - Parameters:
    - `propertyName`: The property name to sort by (must exist on type T).
    - `isDescending`: True for descending sort order, false for ascending.
  - Returns: A task representing the asynchronous update operation.
- **UpdateFilter** - Adds, updates, or removes a simple filter and reloads data.
            If value is null, removes the filter for this property. If value is not null, adds or updates
            the filter. Resets pagination to page 1 when filter changes. Sets IsLoading to true during
            operation and raises OnStateChanged upon completion.
  - Parameters:
    - `property`: The property name to filter on (must exist on type T).
    - `filterOperator`: The filter operator (e.g., "equals", "contains"). Currently stored but not used;
            reserved for future operator-based filtering.
    - `value`: The filter value. If null, removes the filter for this property.
  - Returns: A task representing the asynchronous update operation.
- **UpdatePagination** - Updates pagination parameters (page number and page size) and reloads data.
            Page number is coerced to minimum of 1. Sets IsLoading to true during operation
            and raises OnStateChanged upon completion.
  - Parameters:
    - `pageNumber`: The 1-based page number. If less than 1, coerced to 1.
    - `pageSize`: The number of items per page.
  - Returns: A task representing the asynchronous update operation.
- **ToggleRowSelection** - Toggles the selection state of a single row.
            If the row ID is currently selected, removes it; otherwise adds it.
            Raises OnStateChanged synchronously upon completion.
  - Parameters:
    - `rowId`: The row identifier (extracted via reflection from the data object).
- **SetSelectedRows** - Replaces the entire selection with the specified row IDs.
            Clears any previously selected rows and sets the selection to exactly these IDs.
            Raises OnStateChanged synchronously upon completion.
  - Parameters:
    - `rowIds`: List of row identifiers to select. Can be empty to clear selection.
- **ToggleSelectAll** - Selects or deselects all rows on the current page.
            If isChecked is true, selects all rows in CurrentResponse. If false, clears selection.
            Calls SetSelectedRows internally, which raises OnStateChanged.
  - Parameters:
    - `isChecked`: True to select all visible rows, false to clear selection.
- **ClearFilters** - Removes all active filters and reloads data.
            Sets CurrentRequest.Filters to null and resets pagination to page 1.
            Note: Sort order is preserved (not cleared by this operation).
            Sets IsLoading to true during operation and raises OnStateChanged upon completion.
  - Returns: A task representing the asynchronous clear operation.
- **ExecuteWithStateManagement** - Wraps an async operation with state management: sets IsLoading, clears errors,
            executes the operation, and ensures IsLoading is cleared even if an exception occurs.
            Errors are captured and stored in Error property.
  - Parameters:
    - `operation`: The async operation to execute.
- **RefreshData** - Refreshes the data based on current request parameters.
            Currently a placeholder that ensures CurrentResponse is initialized.
            Future implementation will apply filtering/sorting and may integrate with GridDataProvider for API calls.
- **SetLoading** - Sets the IsLoading state.
- **SetError** - Sets the Error message.
- **ClearError** - Clears the Error message (sets to null).
- **RaiseStateChanged** - Raises the OnStateChanged event if subscribers exist.
- **GetRowId** - Extracts the row ID from an item using the cached ID property.
  - Parameters:
    - `item`: The data item to extract the ID from.
  - Returns: The row ID value, or the item itself if ID extraction fails.

### GridFilter

- **Namespace:** `SmartWorkz.Web.GridFilter`
- **Summary:** Represents a single filter condition with property, operator, and value.
            Reserved for future advanced filtering support (Phase 2 or later).
            Currently, simple equality filters are used via DataContext.Filters dictionary.

### IDataContext`1

- **Namespace:** `SmartWorkz.Web.IDataContext`1`
- **Summary:** Provides unified state management for multi-view data components (Grid, List, etc).
            Manages sorting, filtering, pagination, row selection, and loading/error states.

#### Methods & Properties

- **UpdateSort** - Update sort column and direction; triggers data fetch.
- **UpdateFilter** - Add or replace a filter; resets to page 1; triggers data fetch.
- **UpdatePagination** - Change current page number; triggers data fetch.
- **ToggleRowSelection** - Toggle selection state for a single row.
- **SetSelectedRows** - Replace all selected rows.
- **ToggleSelectAll** - Select/deselect all visible rows on current page.
- **ClearFilters** - Reset filters to default state and refetch data.
- **Initialize** - Initialize data from datasource or API endpoint.

### AccessibilityService

- **Namespace:** `SmartWorkz.Web.AccessibilityService`
- **Summary:** Service for generating accessible ARIA IDs and labels for form components.
            Provides WCAG-compliant identifiers and labels for accessible form rendering.

#### Methods & Properties

- **GenerateFieldId** - Generate unique ID for form field (for aria-labelledby, aria-describedby).
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized field ID in the format "field_{name}".
- **GenerateErrorId** - Generate error message ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized error ID in the format "error_{name}".
- **GenerateHintId** - Generate hint ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized hint ID in the format "hint_{name}".
- **GenerateAriaLabel** - Generate ARIA label text.
  - Parameters:
    - `fieldName`: The name of the form field.
    - `required`: Whether the field is required (appends "(required)" if true).
  - Returns: A formatted ARIA label text.
- **SanitizeName** - Sanitize field names by converting to lowercase and replacing special characters with underscores.
  - Parameters:
    - `name`: The field name to sanitize.
  - Returns: A sanitized field name containing only lowercase alphanumeric characters, underscores, and hyphens.

### FormComponentConfig

- **Namespace:** `SmartWorkz.Web.FormComponentConfig`
- **Summary:** Configuration class for form component styling with customizable Bootstrap CSS classes.
            Contains 18 properties for various form components and their variants.

### FormComponentProvider

- **Namespace:** `SmartWorkz.Web.FormComponentProvider`
- **Summary:** Provides form component styling configuration management.
            Manages Bootstrap CSS classes used throughout the form system.
            Allows customization of default Bootstrap styling.

#### Methods & Properties

- **GetConfiguration** - Get current form component configuration
  - Returns: The current FormComponentConfig instance containing all CSS class configurations.
- **UpdateConfiguration** - Update configuration with new values
  - Parameters:
    - `config`: The new FormComponentConfig to apply. Cannot be null.

### IAccessibilityService

- **Namespace:** `SmartWorkz.Web.IAccessibilityService`
- **Summary:** Service for generating accessible ARIA IDs and labels for form components.
            Supports WCAG compliance by providing consistent, properly formatted identifiers.

#### Methods & Properties

- **GenerateFieldId** - Generate unique ID for form field (for aria-labelledby, aria-describedby).
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized field ID in the format "field_{name}".
- **GenerateErrorId** - Generate error message ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized error ID in the format "error_{name}".
- **GenerateHintId** - Generate hint ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized hint ID in the format "hint_{name}".
- **GenerateAriaLabel** - Generate ARIA label text.
  - Parameters:
    - `fieldName`: The name of the form field.
    - `required`: Whether the field is required (appends "(required)" if true).
  - Returns: A formatted ARIA label text.

### IconProvider

- **Namespace:** `SmartWorkz.Web.IconProvider`
- **Summary:** Implementation of IIconProvider that provides Bootstrap icon CSS classes and HTML markup.

#### Methods & Properties

- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
  - Returns: A string containing the icon CSS classes (e.g., "bi bi-check-circle-fill").
- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type with an optional size modifier.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
    - `sizeClass`: Optional CSS class for sizing (e.g., "fs-5", "fs-6").
  - Returns: A string containing the icon CSS classes and size class if provided.
- **GetIconHtml** - Gets the complete HTML markup for the specified icon.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
    - `cssClass`: Optional CSS class to apply to the icon element.
  - Returns: HTML string containing the icon element.

### IFormComponentProvider

- **Namespace:** `SmartWorkz.Web.IFormComponentProvider`
- **Summary:** Provides access to form component styling configuration.
            Allows retrieval and updating of Bootstrap CSS classes used throughout the form system.

#### Methods & Properties

- **GetConfiguration** - Get current form component configuration
  - Returns: The current FormComponentConfig instance containing all CSS class configurations.
- **UpdateConfiguration** - Update configuration
  - Parameters:
    - `config`: The new FormComponentConfig to apply. Cannot be null.

### IconType

- **Namespace:** `SmartWorkz.Web.IconType`
- **Summary:** Enum representing common Bootstrap icons used throughout the application.

### IIconProvider

- **Namespace:** `SmartWorkz.Web.IIconProvider`
- **Summary:** Service interface for providing icon-related utilities.
            Centralizes icon management and provides methods to get icon CSS classes and HTML markup.

#### Methods & Properties

- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
  - Returns: A string containing the icon CSS classes (e.g., "bi bi-check-circle-fill").
- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type with an optional size modifier.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
    - `sizeClass`: Optional CSS class for sizing (e.g., "fs-5", "fs-6").
  - Returns: A string containing the icon CSS classes and size class if provided (e.g., "bi bi-check-circle-fill fs-5").

### IValidationMessageProvider

- **Namespace:** `SmartWorkz.Web.IValidationMessageProvider`
- **Summary:** Provides localized validation error messages with support for custom registration.

#### Methods & Properties

- **GetMessage** - Get validation message for given error type.
  - Parameters:
    - `errorType`: The type of validation error.
  - Returns: The validation message for the error type.
- **GetMessage** - Get validation message with property name included.
  - Parameters:
    - `errorType`: The type of validation error.
    - `propertyName`: The name of the property being validated.
  - Returns: The validation message with property name included.
- **RegisterMessage** - Register custom validation message.
  - Parameters:
    - `errorType`: The type of validation error to register.
    - `message`: The custom message for the error type.

### ValidationMessageProvider

- **Namespace:** `SmartWorkz.Web.ValidationMessageProvider`
- **Summary:** Provides localized validation error messages with support for custom registration.
            Includes 14 built-in validation messages for common validation scenarios.

#### Methods & Properties

- **GetMessage** - Get validation message for given error type.
  - Parameters:
    - `errorType`: The type of validation error.
  - Returns: The validation message for the error type.
- **GetMessage** - Get validation message with property name included.
  - Parameters:
    - `errorType`: The type of validation error.
    - `propertyName`: The name of the property being validated.
  - Returns: The validation message with property name included.
- **RegisterMessage** - Register custom validation message.
  - Parameters:
    - `errorType`: The type of validation error to register.
    - `message`: The custom message for the error type.

### IListViewFormatter

- **Namespace:** `SmartWorkz.Web.IListViewFormatter`
- **Summary:** Formats data for List/Card view display (dates, currency, text truncation, etc).

#### Methods & Properties

- **FormatDate** - Format a date value for display.
- **FormatCurrency** - Format a decimal value as currency.
- **TruncateText** - Truncate text to max length with ellipsis.
- **FormatBoolean** - Format a boolean as human-readable text.
- **FormatValue** - Format any object using type-aware rules.

### ListViewFormatter

- **Namespace:** `SmartWorkz.Web.ListViewFormatter`
- **Summary:** Formats raw data values for display in list and grid components.
            
             ListViewFormatter provides type-aware formatting for common data types encountered in
             data display scenarios: dates, currency amounts, text, booleans, and generic objects.
             It handles null values gracefully by displaying a dash (-) instead of empty/null text.

#### Methods & Properties

- **FormatDate** - Formats a date/time value using the specified .NET format string.
            Returns "-" if the date is null.
  - Parameters:
    - `date`: The date value to format. Can be null.
    - `format`: The .NET format string (default: "MMM dd, yyyy" for "Apr 22, 2026").
  - Returns: Formatted date string, or "-" if date is null.
- **FormatCurrency** - Formats a decimal value as currency with the specified symbol prefix.
            Returns "-" if the value is null. Always formats to 2 decimal places.
  - Parameters:
    - `value`: The decimal amount to format. Can be null.
    - `currencySymbol`: The currency symbol to prefix (default: "$").
  - Returns: Formatted currency string (e.g., "$1,234.50"), or "-" if value is null.
- **TruncateText** - Truncates text to a maximum length and appends "..." if truncated.
            Returns "-" if the text is null or empty.
  - Parameters:
    - `text`: The text to truncate. Can be null or empty.
    - `maxLength`: Maximum length before truncation (default: 100 characters).
  - Returns: Truncated text with "..." appended if over max length, "-" if null/empty, or original text if shorter.
- **FormatBoolean** - Formats a boolean value as human-readable text.
            Returns "Yes" for true, "No" for false, and "-" for null.
  - Parameters:
    - `value`: The boolean value to format. Can be null.
  - Returns: "Yes" for true, "No" for false, "-" for null.
- **FormatValue** - Formats any object value using type-aware detection and appropriate formatter.
            Uses pattern matching to dispatch to specialized formatters:
            - null → "-"
            - DateTime → FormatDate with default format
            - decimal → FormatCurrency with default symbol
            - bool → FormatBoolean (Yes/No)
            - string → TruncateText with default limit
            - Other → ToString() or "-" if ToString returns null
  - Parameters:
    - `value`: The value to format. Can be any type or null.
  - Returns: Formatted string appropriate to the value's type, or "-" if null or formatting fails.

### ViewConfiguration

- **Namespace:** `SmartWorkz.Web.ViewConfiguration`
- **Summary:** Stores view-specific configuration (visible columns, item layout, formatting rules).
            Allows different views to display the same data differently.

### GridDataProvider

- **Namespace:** `SmartWorkz.Web.GridDataProvider`
- **Summary:** Web-specific implementation of grid data fetching via HTTP API or in-memory sources.

#### Methods & Properties

- **GetDataAsync``1** - Fetch data from HTTP API endpoint.
- **ApplyGridLogic``1** - Apply sorting, filtering, and paging to an in-memory IEnumerable.
            Used when grid is bound to local data instead of an API.

### GridExportService

- **Namespace:** `SmartWorkz.Web.GridExportService`
- **Summary:** Service for exporting grid data to various formats (CSV, Excel).
            
             GridExportService exports tabular data from grid components to CSV or Excel formats.
             It handles column filtering, header inclusion, and proper escaping of special characters
             according to CSV standards (RFC 4180).

#### Methods & Properties

- **ExportToCsv``1** - Exports grid data to CSV format with proper escaping and optional column filtering.
  - Parameters:
    - `data`: The collection of items to export.
    - `columns`: The list of GridColumn definitions (PropertyName, DisplayName, IsVisible, etc).
    - `options`: Export options including Format, IncludeHeaders, IncludeColumns, ExcludeColumns.
  - Returns: A CSV-formatted string ready for file download or further processing.
- **ExportToExcel``1** - Exports grid data to Excel format. Currently not implemented.
  - Parameters:
    - `data`: The collection of items to export.
    - `columns`: The list of GridColumn definitions.
    - `options`: Export options (same as CSV).
  - Returns: Empty byte array. Actual implementation pending EPPlus dependency.
- **GetColumnsToExport** - Determines which columns to include in the export based on visibility and options.
  - Parameters:
    - `columns`: The complete list of available columns.
    - `options`: Export options specifying IncludeColumns and ExcludeColumns.
  - Returns: List of columns that should be included in the export.
- **EscapeCsv** - Escapes a value for CSV output according to RFC 4180 standard.
  - Parameters:
    - `value`: The raw value to escape. Can be null or empty.
  - Returns: The escaped value, quoted if necessary, ready for CSV output.

### GridStateManager

- **Namespace:** `SmartWorkz.Web.GridStateManager`
- **Summary:** Manages grid state (current page, sorting, filters, selected rows).
            Optionally persists state to browser localStorage.

#### Methods & Properties

- **UpdateRequest** - Update the current grid request and notify listeners.
- **UpdatePagination** - Update pagination (page and pageSize).
- **UpdateSort** - Update sorting.
- **UpdateFilters** - Update filters (replaces entire filter dictionary).
- **SetFilter** - Add or update a single filter.
- **RemoveFilter** - Remove a filter by column name.
- **ClearFilters** - Clear all filters.
- **SetSelectedRows** - Update selected row IDs.
- **ToggleRowSelection** - Toggle row selection.
- **SetLoading** - Set loading state.
- **SetError** - Set error message.
- **ClearError** - Clear error message.
- **Reset** - Reset all state to defaults.

### WebComponentExtensions

- **Namespace:** `SmartWorkz.Web.WebComponentExtensions`
- **Summary:** Extension methods for registering SmartWorkz.Web services with dependency injection.

#### Methods & Properties

- **AddSmartWorkzCoreWeb** - Register all SmartWorkz.Web services and TagHelpers with the dependency injection container.
            Registers component services as singletons for optimal performance.
            Note: TagHelpers are auto-discovered by ASP.NET Core and do not require explicit registration.
  - Parameters:
    - `services`: The IServiceCollection to register services with.
  - Returns: The IServiceCollection for method chaining.
- **AddSmartWorkzWebComponents** - Register all SmartWorkz.Web data view components and services.
  - Parameters:
    - `services`: The IServiceCollection to register services with.
  - Returns: The IServiceCollection for method chaining.

### ButtonTagHelper

- **Namespace:** `SmartWorkz.Web.ButtonTagHelper`
- **Summary:** TagHelper for rendering HTML button and link elements with Bootstrap button styling, size variants, and loading states.
             Targets the <button> and <a> elements when the Variant attribute is present and applies Bootstrap button CSS classes.
- **Example:**
```csharp
<!-- Primary button (default submit button) -->
             <button type="submit" variant="primary">Submit</button>
             <!-- Generates: <button class="btn btn-primary" type="submit">Submit</button> -->
            
             <!-- Small secondary button -->
             <button variant="secondary" size="sm">Cancel</button>
             <!-- Generates: <button class="btn btn-secondary btn-sm">Cancel</button> -->
            
             <!-- Large danger button -->
             <button variant="danger" size="lg">Delete</button>
             <!-- Generates: <button class="btn btn-danger btn-lg" disabled="disabled">Delete</button> -->
            
             <!-- Success button with loading state -->
             <button variant="success" is-loading="true">Processing...</button>
             <!-- Generates: <button class="btn btn-success disabled" disabled="disabled">Processing...</button> -->
            
             <!-- Link styled as a button -->
             <a href="/dashboard" variant="info">Go to Dashboard</a>
             <!-- Generates: <a class="btn btn-info" href="/dashboard">Go to Dashboard</a> -->
            
             <!-- Warning button with custom CSS class -->
             <button variant="warning" class="mt-2">Warning Action</button>
             <!-- Generates: <button class="mt-2 btn btn-warning">Warning Action</button> -->
```

### IconTagHelper

- **Namespace:** `SmartWorkz.Web.IconTagHelper`
- **Summary:** TagHelper for rendering Bootstrap Icon library icons with size and color customization.
             Targets the <icon> element and generates Bootstrap Icon HTML markup (<i class="bi bi-{name}"></i>).
- **Example:**
```csharp
<!-- Standalone success icon -->
             <icon name="Success" />
             <!-- Generates: <i class="bi bi-check-circle-fill"></i> -->
            
             <!-- Small icon with custom CSS class -->
             <icon name="Info" size="sm" css-class="text-info" />
             <!-- Generates: <i class="bi bi-info-circle me-1 text-info"></i> -->
            
             <!-- Large error icon in red -->
             <icon name="Error" size="lg" css-class="text-danger" />
             <!-- Generates: <i class="bi bi-exclamation-circle fs-5 text-danger"></i> -->
            
             <!-- Search icon in a button -->
             <button type="button" class="btn btn-primary">
               <icon name="Search" size="sm" /> Search
             </button>
            
             <!-- Warning icon with emphasis -->
             <icon name="Warning" size="lg" css-class="text-warning me-2" />
             <span>Please verify your information</span>
            
             <!-- Home navigation icon -->
             <a href="/"><icon name="Home" /> Home</a>
            
             <!-- User account icon -->
             <a href="/settings"><icon name="User" size="sm" /> Settings</a>
```

### AlertTagHelper

- **Namespace:** `SmartWorkz.Web.AlertTagHelper`
- **Summary:** TagHelper for rendering Bootstrap alert components with optional dismiss functionality.
             Targets the <alert> element and generates <div class="alert alert-{type}">.
- **Example:**
```csharp
<!-- Simple success alert -->
             <alert type="success" message="Profile updated successfully!" />
            
             <!-- Non-dismissible danger alert -->
             <alert type="danger" message="An error occurred while saving." dismissible="false" />
            
             <!-- Warning alert with dismiss button -->
             <alert type="warning" message="This action cannot be undone." />
            
             <!-- Default info alert -->
             <alert message="Remember to save your changes regularly." />
```

### BadgeTagHelper

- **Namespace:** `SmartWorkz.Web.BadgeTagHelper`
- **Summary:** TagHelper for rendering Bootstrap badge components for labels, counts, and status indicators.
             Targets the <badge> element and generates <span class="badge bg-{type}">.
- **Example:**
```csharp
<!-- Simple primary badge -->
             <badge type="primary" text="New" />
            
             <!-- Count badge -->
             <badge type="success" text="5 items" />
            
             <!-- Danger badge for inactive status -->
             <badge type="danger" text="Inactive" />
            
             <!-- Warning badge -->
             <badge type="warning" text="Pending Review" />
            
             <!-- Default secondary badge -->
             <badge text="Badge" />
            
             <!-- Pill-shaped badge -->
             <span class="badge bg-primary rounded-pill">@notificationCount</span>
```

### PaginationTagHelper

- **Namespace:** `SmartWorkz.Web.PaginationTagHelper`
- **Summary:** TagHelper for rendering Bootstrap pagination controls with automatic page calculation and link generation.
             Targets the <pagination> element and generates <nav><ul class="pagination"> navigation.
- **Example:**
```csharp
<!-- Basic pagination with defaults (shows 5 pages max) -->
             <pagination current-page="2" total-pages="10" />
            
             <!-- Pagination with custom page URL pattern -->
             <pagination current-page="1" total-pages="5" page-url="/products?page={0}" />
            
             <!-- Pagination with more visible pages -->
             <pagination current-page="3" total-pages="15" max-visible="7" />
            
             <!-- Pagination on last page (Next is disabled) -->
             <pagination current-page="10" total-pages="10" />
            
             <!-- Pagination with single page (not rendered) -->
             <pagination current-page="1" total-pages="1" />
```

### CheckboxTagHelper

- **Namespace:** `SmartWorkz.Web.CheckboxTagHelper`
- **Summary:** TagHelper for rendering HTML checkbox inputs with Bootstrap styling and label support.
             Targets the <checkbox-tag> element and generates <div class="form-check"> with checkbox input.
- **Example:**
```csharp
<!-- Simple checkbox with label -->
             <checkbox-tag for="User.IsSubscribed" label="Subscribe to newsletter" />
            
             <!-- Checkbox with custom value -->
             <checkbox-tag for="User.AgreedToTerms" label="I agree to the terms" value="1" />
            
             <!-- Pre-checked checkbox -->
             <checkbox-tag for="User.IsActive" label="Active" checked="true" />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.IsSubscribed" label="Newsletter Subscription" help-text="Get updates via email">
               <checkbox-tag for="User.IsSubscribed" label="Subscribe" />
             </form-group>
```

### FileInputTagHelper

- **Namespace:** `SmartWorkz.Web.FileInputTagHelper`
- **Summary:** TagHelper for rendering HTML file input elements with Bootstrap styling and validation support.
             Targets the <file-input-tag> element and generates <input type="file"> with form-control class.
- **Example:**
```csharp
<!-- Simple file input -->
             <file-input-tag for="Model.ProfilePhoto" />
            
             <!-- File input accepting specific types -->
             <file-input-tag for="Model.Document" accept=".pdf,.docx" required="true" />
            
             <!-- Multiple file upload -->
             <file-input-tag for="Model.Attachments" accept="image/*" multiple="true" />
            
             <!-- In form-group wrapper -->
             <form-group for="Model.ProfilePhoto" label="Upload Photo" required="true" help-text="JPG or PNG, max 5MB">
               <file-input-tag for="Model.ProfilePhoto" accept="image/jpeg,image/png" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new FileInputTagHelper with dependency injection.
  - Parameters:
    - `formComponentProvider`: Service for form component styling and configuration.

### FormGroupTagHelper

- **Namespace:** `SmartWorkz.Web.FormGroupTagHelper`
- **Summary:** TagHelper that wraps form controls with Bootstrap form-group styling, label, and help text support.
             Targets custom <form-group> element and renders a div wrapper with optional label and help text.
- **Example:**
```csharp
<!-- Simple form-group with text input -->
             <form-group for="Model.Name" label="Full Name" required="true">
               <input-tag for="Model.Name" placeholder="Enter your full name" />
             </form-group>
            
             <!-- Form-group with help text -->
             <form-group for="Model.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="Model.Email" type="email" placeholder="you@example.com" />
             </form-group>
            
             <!-- Form-group with select control -->
             <form-group for="Model.CountryId" label="Country" required="true" help-text="Select your country of residence">
               <select-tag for="Model.CountryId" items="@countries" />
             </form-group>
            
             <!-- Form-group with textarea -->
             <form-group for="Model.Comments" label="Comments" help-text="Optional feedback (max 500 characters)">
               <textarea-tag for="Model.Comments" rows="4" placeholder="Share your thoughts..." />
             </form-group>
            
             <!-- Form-group with checkbox -->
             <form-group for="Model.IsSubscribed" label="Newsletter">
               <checkbox-tag for="Model.IsSubscribed" label="Subscribe to newsletter" />
             </form-group>
            
             <!-- Form-group with validation error state -->
             <!-- When ModelState contains error for Model.Age, child controls show .is-invalid state -->
             <form-group for="Model.Age" label="Age" required="true" help-text="Must be 18 or older">
               <input-tag for="Model.Age" type="number" placeholder="Enter your age" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FormGroupTagHelper.
  - Parameters:
    - `formComponentProvider`: Provider for form component configuration.
    - `accessibilityService`: Service for generating accessible ARIA IDs.
- **Process** - Processes the form-group tag and renders a div wrapper with label and help text.

### FormTagHelper

- **Namespace:** `SmartWorkz.Web.FormTagHelper`
- **Summary:** TagHelper for rendering HTML form elements with Bootstrap validation styling and class management.
             Targets the <form-tag> element and generates <form class="needs-validation">.
- **Example:**
```csharp
<!-- Simple form with default POST method -->
             <form-tag>
               <form-group for="Model.Name" label="Full Name" required="true">
                 <input-tag for="Model.Name" placeholder="Enter your name" />
               </form-group>
               <button type="submit" class="btn btn-primary">Submit</button>
             </form-tag>
            
             <!-- Form with custom action and GET method -->
             <form-tag method="get" action="/search">
               <input-tag for="Model.SearchTerm" placeholder="Search..." />
               <button type="submit" class="btn btn-primary">Search</button>
             </form-tag>
            
             <!-- Form with custom CSS class and validation disabled -->
             <form-tag class="login-form" novalidate="true">
               <form-group for="Model.Email" label="Email" required="true">
                 <input-tag for="Model.Email" type="email" />
               </form-group>
               <button type="submit" class="btn btn-primary">Login</button>
             </form-tag>
```

### InputTagHelper

- **Namespace:** `SmartWorkz.Web.InputTagHelper`
- **Summary:** TagHelper for rendering HTML input elements with Bootstrap styling, optional icon support, and form component styling configuration.
             Targets the <input-tag> element and generates <input class="form-control"> with optional icon wrappers.
- **Example:**
```csharp
<!-- Simple text input -->
             <input-tag for="User.Name" placeholder="Enter your name" />
            
             <!-- Email input with validation -->
             <input-tag for="User.Email" type="email" placeholder="Enter email" required="true" />
            
             <!-- Password input -->
             <input-tag for="User.Password" type="password" placeholder="Enter password" />
            
             <!-- Number input with icon -->
             <input-tag for="Product.Price" type="number" step="0.01" icon-prefix="DollarSign" />
            
             <!-- Search input with icon -->
             <input-tag for="Model.SearchTerm" type="search" icon-suffix="Search" placeholder="Search..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="User.Email" type="email" icon-prefix="AtSign" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InputTagHelper class.
  - Parameters:
    - `formComponentProvider`: Provider for form component styling configuration.
    - `iconProvider`: Provider for icon rendering.
- **Process** - Processes the input-tag element and renders an HTML input element with optional icons and styling.
  - Parameters:
    - `context`: The TagHelperContext.
    - `output`: The TagHelperOutput.

### LabelTagHelper

- **Namespace:** `SmartWorkz.Web.LabelTagHelper`
- **Summary:** TagHelper for rendering HTML label elements with Bootstrap styling and required field indicators.
             Targets the <label-tag> element and generates <label class="form-label"> with optional required asterisk.
- **Example:**
```csharp
<!-- Simple label -->
             <label-tag for="User.Name" text="Full Name" />
            
             <!-- Required field with asterisk -->
             <label-tag for="User.Email" text="Email Address" required="true" />
            
             <!-- Optional field without asterisk -->
             <label-tag for="User.PhoneNumber" text="Phone Number" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <label-tag for="User.Email" text="Email Address" required="true" />
               <input-tag for="User.Email" type="email" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new LabelTagHelper with dependency injection.
  - Parameters:
    - `formComponentProvider`: Service for form component styling and configuration.
    - `accessibilityService`: Service for accessibility attributes and ARIA support.

### RadioButtonTagHelper

- **Namespace:** `SmartWorkz.Web.RadioButtonTagHelper`
- **Summary:** TagHelper for rendering HTML radio button inputs with Bootstrap styling, label support, and grouping.
             Targets the <radio-button-tag> element and generates <div class="form-check"> with radio input.
- **Example:**
```csharp
<!-- Single radio button -->
             <radio-button-tag for="Model.Status" group-name="Status" label="Active" value="active" />
            
             <!-- Radio button group (render multiple radio-button-tags with same group-name) -->
             <div>
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" checked="true" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="PayPal" value="paypal" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Bank Transfer" value="bank" />
             </div>
            
             <!-- Required radio button group -->
             <div>
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Standard (5-7 days)" value="standard" checked="true" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Express (2-3 days)" value="express" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Overnight" value="overnight" />
             </div>
            
             <!-- In form-group wrapper -->
             <form-group for="Model.PaymentMethod" label="Payment Method" required="true">
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Debit Card" value="dc" />
             </form-group>
```

### SelectTagHelper

- **Namespace:** `SmartWorkz.Web.SelectTagHelper`
- **Summary:** TagHelper for rendering HTML select elements with support for list items, enum binding, blank option handling, and Bootstrap styling.
             Targets the <select-tag> element and generates <select class="form-control"> with options.
- **Example:**
```csharp
<!-- Simple select with SelectListItem collection -->
             <select-tag for="User.CountryId" items="@countries" />
            
             <!-- Select with enum binding -->
             <select-tag for="Order.Status" enum-type="typeof(OrderStatus)" />
            
             <!-- Select without blank option -->
             <select-tag for="Product.Category" items="@categories" add-blank="false" />
            
             <!-- Select with custom blank text -->
             <select-tag for="User.Department" items="@departments" blank-text="Choose a department..." />
            
             <!-- With initial selection -->
             @{ var selected = categories.First(c => c.Value == "electronics"); selected.Selected = true; }
             <select-tag for="Product.Category" items="@categories" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.CountryId" label="Country" required="true" help-text="Select your country">
               <select-tag for="User.CountryId" items="@countries" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SelectTagHelper class.
  - Parameters:
    - `formComponentProvider`: Provider for form component styling configuration.
- **Process** - Processes the select-tag element and renders an HTML select element with options.
  - Parameters:
    - `context`: The TagHelperContext.
    - `output`: The TagHelperOutput.
- **GetSelectItems** - Gets the select items from either the Items property or EnumType property.
            If both Items and EnumType are provided, Items takes precedence.
  - Returns: A list of SelectListItem objects to render as options.

### TextAreaTagHelper

- **Namespace:** `SmartWorkz.Web.TextAreaTagHelper`
- **Summary:** TagHelper for rendering HTML textarea elements with Bootstrap styling, row configuration, and validation support.
             Targets the <textarea-tag> element and generates <textarea class="form-control">.
- **Example:**
```csharp
<!-- Simple textarea with default 3 rows -->
             <textarea-tag for="Model.Comments" placeholder="Enter your comments..." />
            
             <!-- Textarea with custom height -->
             <textarea-tag for="Model.Description" rows="6" placeholder="Enter detailed description" />
            
             <!-- Required textarea -->
             <textarea-tag for="Model.Feedback" placeholder="Your feedback is important" required="true" />
            
             <!-- Large textarea for longer content -->
             <textarea-tag for="Model.BioOrNotes" rows="10" placeholder="Tell us about yourself..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="Model.Comments" label="Comments" required="true" help-text="Please provide at least 10 characters">
               <textarea-tag for="Model.Comments" rows="5" placeholder="Share your thoughts..." />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new TextAreaTagHelper with dependency injection.
  - Parameters:
    - `formComponentProvider`: Service for form component styling and configuration.

### ValidationMessageTagHelper

- **Namespace:** `SmartWorkz.Web.ValidationMessageTagHelper`
- **Summary:** TagHelper for rendering HTML validation error messages with Bootstrap styling.
             Targets the <validation-message> element and generates <div class="invalid-feedback">.
- **Example:**
```csharp
<!-- Validation message for a field (after input element) -->
             <input-tag for="User.Email" type="email" />
             <validation-message for="User.Email" />
            
             <!-- Validation message with custom message -->
             <input-tag for="User.Age" type="number" />
             <validation-message for="User.Age" message="Age must be between 18 and 100" />
            
             <!-- Typical usage in form-group -->
             <form-group for="User.Email" label="Email" required="true">
               <input-tag for="User.Email" type="email" />
               <validation-message for="User.Email" />
             </form-group>
            
             <!-- Multiple fields with validation -->
             <form-group for="User.Password" label="Password" required="true">
               <input-tag for="User.Password" type="password" />
               <validation-message for="User.Password" />
             </form-group>
             <form-group for="User.ConfirmPassword" label="Confirm Password" required="true">
               <input-tag for="User.ConfirmPassword" type="password" />
               <validation-message for="User.ConfirmPassword" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new ValidationMessageTagHelper with dependency injection.
  - Parameters:
    - `accessibilityService`: Service for generating accessible error message IDs and ARIA support.

### GridTagHelper

- **Namespace:** `SmartWorkz.Web.GridTagHelper`
- **Summary:** TagHelper for rendering data grids with sorting, filtering, row selection, and pagination support.
             Targets the <grid> element and provides a high-level API for binding to IDataContext<T>
             and rendering grid data with Bootstrap table styling.
- **Example:**
```csharp
<!-- Basic grid with data binding -->
             <grid data-source="@Model.Products" data-page-size="20">
               <grid-column property="Name" sortable="true">Product Name</grid-column>
               <grid-column property="Price" sortable="true" format="currency">Price</grid-column>
             </grid>
            
             <!-- Grid with row selection enabled -->
             <grid data-source="@Model.Orders" data-page-size="50" data-allow-selection="true">
               <grid-column property="OrderId">Order ID</grid-column>
               <grid-column property="OrderDate" sortable="true" format="date">Date</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
             </grid>
            
             <!-- Grid with custom CSS class and export option -->
             <grid data-source="@Model.Customers"
                   data-page-size="25"
                   data-allow-export="true"
                   data-allow-column-toggle="true"
                   data-css-class="compact-grid">
               <grid-column property="FirstName">First Name</grid-column>
               <grid-column property="Email" sortable="true">Email</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
            
             <!-- Complete example with IDataContext<Product> binding -->
             @{
               var productDataContext = new DataContext<Product>(productService);
               await productDataContext.Initialize(await productService.GetProductsAsync());
             }
             <grid data-source="@productDataContext.Items"
                   data-page-size="20"
                   data-allow-selection="true">
               <grid-column property="Name" sortable="true">Product</grid-column>
               <grid-column property="Category" filterable="true">Category</grid-column>
               <grid-column property="Price" format="currency" css-class="text-end">Price</grid-column>
               <grid-column property="Stock" sortable="true">Stock</grid-column>
             </grid>
            
             <!-- Grid with filtering and sorting -->
             <div class="mb-3">
               <label for="statusFilter">Filter by Status:</label>
               <select id="statusFilter" onchange="updateFilter(this.value)">
                 <option value="">All</option>
                 <option value="Active">Active</option>
                 <option value="Inactive">Inactive</option>
               </select>
             </div>
            
             <grid data-source="@Model.Items" data-page-size="20">
               <grid-column property="Name" sortable="true">Name</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
```

### BreadcrumbItem

- **Namespace:** `SmartWorkz.Web.BreadcrumbItem`
- **Summary:** Data model representing a single item in a breadcrumb navigation trail.
             Used with BreadcrumbTagHelper to construct hierarchical navigation paths.
- **Example:**
```csharp
<!-- Create breadcrumb items for product details page -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Home", Url = "/" },
                 new() { Label = "Products", Url = "/products" },
                 new() { Label = "Electronics", Url = "/products/electronics" },
                 new() { Label = "Laptops", Url = "/products/electronics/laptops" },
                 new() { Label = "Dell XPS 13" }  // No URL - this is the current page
               };
             }
             <breadcrumb items="breadcrumbs" />
            
             <!-- Create from dynamic data -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" }
               };
            
               foreach (var folder in Model.FolderHierarchy)
               {
                 breadcrumbs.Add(new()
                 {
                   Label = folder.Name,
                   Url = folder.Depth < Model.FolderHierarchy.Count - 1 ? folder.Url : null
                 });
               }
             }
             <breadcrumb items="breadcrumbs" />
```

### BreadcrumbTagHelper

- **Namespace:** `SmartWorkz.Web.BreadcrumbTagHelper`
- **Summary:** TagHelper for rendering Bootstrap breadcrumb navigation showing the user's current location in site hierarchy.
             Targets the <breadcrumb> element and generates <nav><ol class="breadcrumb"> navigation.
- **Example:**
```csharp
<!-- Basic breadcrumb navigation -->
             <breadcrumb items="new List<BreadcrumbItem> {
               new() { Label = "Home", Url = "/" },
               new() { Label = "Products", Url = "/products" },
               new() { Label = "Electronics" }
             }" />
            
             <!-- Breadcrumb from controller action -->
             <breadcrumb items="Model.Breadcrumbs" />
            
             <!-- Programmatically constructed breadcrumb in view -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" },
                 new() { Label = "Reports", Url = "/reports" },
                 new() { Label = "Monthly Summary" }
               };
             }
             <breadcrumb items="breadcrumbs" />
```

### IfAuthorizedTagHelper

- **Namespace:** `SmartWorkz.Web.IfAuthorizedTagHelper`
- **Summary:** TagHelper for rendering content only if the user is authenticated.
             Targets the <if-authorized> element and conditionally renders its content based on authentication status.
- **Example:**
```csharp
<!-- Simple logout button, shown only if authenticated -->
             <if-authorized>
               <button class="btn btn-danger">Logout</button>
             </if-authorized>
            
             <!-- User profile section in header -->
             <if-authorized>
               <div class="user-profile">
                 <span>Welcome, @Model.User.Name!</span>
                 <a href="/profile">View Profile</a>
               </div>
             </if-authorized>
            
             <!-- Multiple buttons and links for authenticated users -->
             <if-authorized>
               <div class="authenticated-menu">
                 <a href="/dashboard" class="btn btn-primary">Dashboard</a>
                 <a href="/orders" class="btn btn-info">My Orders</a>
                 <button onclick="logout()" class="btn btn-danger">Logout</button>
               </div>
             </if-authorized>
            
             <!-- Unauthenticated content shown separately -->
             <div>
               <if-authorized>
                 <p>You are logged in.</p>
               </if-authorized>
               <!-- Note: For not-authenticated content, use negation in IfAuthorizedTagHelper
                    or implement a separate IfNotAuthorizedTagHelper -->
             </div>
```

### IfClaimTagHelper

- **Namespace:** `SmartWorkz.Web.IfClaimTagHelper`
- **Summary:** TagHelper for rendering content only if the user has a specific claim with a matching value.
             Targets the <if-claim> element and conditionally renders content based on claim verification.
- **Example:**
```csharp
<!-- Simple claim check: show if user has "role" claim with value "Admin" -->
             <if-claim type="role" value="Admin">
               <button class="btn btn-danger">Delete User</button>
             </if-claim>
            
             <!-- Multiple values (OR logic): show if user is Admin or Manager -->
             <if-claim type="role" value="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-claim>
            
             <!-- Department-based access: show if in IT or Finance department -->
             <if-claim type="department" value="IT,Finance">
               <div class="reports-section">
                 <h3>Financial Reports</h3>
                 <a href="/reports/finance">View Reports</a>
               </div>
             </if-claim>
            
             <!-- Subscription level: show premium features if subscription is Premium or Enterprise -->
             <if-claim type="subscription_level" value="Premium,Enterprise">
               <div class="premium-features">
                 <h3>Advanced Analytics</h3>
                 <p>You have access to advanced reporting features.</p>
               </div>
             </if-claim>
            
             <!-- Claim existence without value check: show if user has ANY custom-permission claim -->
             <if-claim type="custom-permission">
               <p>You have special permissions.</p>
             </if-claim>
            
             <!-- Combined with other markup: admin toolbar with multiple restrictions -->
             <div class="admin-toolbar">
               <if-claim type="role" value="Admin">
                 <button class="btn btn-danger" onclick="deleteAll()">Delete All</button>
               </if-claim>
               <if-claim type="role" value="Admin,Moderator">
                 <button class="btn btn-warning" onclick="moderate()">Moderate</button>
               </if-claim>
             </div>
```

### IfRoleTagHelper

- **Namespace:** `SmartWorkz.Web.IfRoleTagHelper`
- **Summary:** TagHelper for rendering content only if the user belongs to a specific role.
             Targets the <if-role> element and conditionally renders content based on role membership.
- **Example:**
```csharp
<!-- Simple role check: show delete button if user is Admin -->
             <if-role role="Admin">
               <button class="btn btn-danger" onclick="deleteItem()">Delete</button>
             </if-role>
            
             <!-- Multiple roles (OR logic): show if user is Admin or Manager -->
             <if-role role="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-role>
            
             <!-- Editor dashboard with edit/delete options -->
             <if-role role="Editor">
               <div class="editor-toolbar">
                 <button class="btn btn-info" onclick="editItem()">Edit</button>
                 <button class="btn btn-warning" onclick="publishItem()">Publish</button>
               </div>
             </if-role>
            
             <!-- Multiple role sections with different features -->
             <div class="dashboard">
               <if-role role="Admin">
                 <div class="admin-section">
                   <h3>System Administration</h3>
                   <a href="/admin/users">Manage Users</a>
                   <a href="/admin/settings">System Settings</a>
                 </div>
               </if-role>
               <if-role role="Manager,Editor">
                 <div class="content-section">
                   <h3>Content Management</h3>
                   <a href="/content">View Content</a>
                 </div>
               </if-role>
             </div>
            
             <!-- Nested role checks in a complex menu -->
             <nav class="sidebar">
               <ul>
                 <li><a href="/home">Home</a></li>
                 <if-role role="User,Admin,Manager">
                   <li><a href="/dashboard">Dashboard</a></li>
                   <li><a href="/profile">My Profile</a></li>
                 </if-role>
                 <if-role role="Admin">
                   <li><a href="/admin">Administration</a></li>
                 </if-role>
               </ul>
             </nav>
            
             <!-- Contributor role with limited permissions -->
             <if-role role="Contributor,Editor,Admin">
               <div class="publish-section">
                 <button class="btn btn-success" onclick="submit()">Submit for Review</button>
               </div>
             </if-role>
```

### CacheAttribute

- **Namespace:** `SmartWorkz.Web.CacheAttribute`
- **Summary:** Specifies response caching for MVC action methods and controllers.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.

### DataContext`1

- **Namespace:** `SmartWorkz.Web.DataContext`1`
- **Summary:** Manages state and state transitions for grid/list components with async data operations.
            
             DataContext is a state container that coordinates sorting, filtering, pagination, row selection,
             and loading/error states for data display components. It acts as a bridge between the UI
             (through Razor components) and the data layer, managing the current request parameters and
             response data along with user interactions (row selection, state changes).
- **Example:**
```csharp
// Initialize with data
             var context = new DataContext<Product>();
             await context.Initialize(products);
            
             // Subscribe to state changes
             context.OnStateChanged += () => Console.WriteLine("State changed!");
            
             // Filter by category - async operation
             await context.UpdateFilter("Category", "equals", "Electronics");
             // CurrentRequest.Filters now contains { "Category": "Electronics" }
             // CurrentResponse.Data reloaded asynchronously
            
             // Change sort - resets to page 1
             await context.UpdateSort("Price", isDescending: true);
            
             // Change pagination
             await context.UpdatePagination(pageNumber: 2, pageSize: 50);
            
             // Row selection - synchronous operations
             context.ToggleRowSelection(1);          // Select row with ID=1
             context.ToggleSelectAll(true);          // Select all visible rows
             var selected = context.SelectedRowIds;  // List<object> containing IDs
            
             // Check loading state
             if (context.IsLoading)
             {
                 // Show spinner
             }
            
             // Handle errors
             if (context.Error != null)
             {
                 Console.WriteLine($"Error: {context.Error}");
             }
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the DataContext class with default pagination (page 1, page size 20).
            Caches the row ID property via reflection during construction.
- **CacheIdProperty** - Caches the row ID property using reflection. Called during construction.
            Attempts to locate the ID property in this order:
            1. Property decorated with [Key] attribute
            2. Property named "Id" (case-insensitive)
            3. First property on the type
- **Initialize** - Initializes the context with data from a source and applies the current request parameters.
            Sets IsLoading to true during operation, updates CurrentResponse with the result,
            and raises OnStateChanged upon completion.
  - Parameters:
    - `dataSource`: The collection of items to load. Can be empty.
  - Returns: A task representing the asynchronous initialization.
- **UpdateSort** - Updates the sort parameters and reloads data.
            Resets pagination to page 1 when sort changes. Sets IsLoading to true during operation
            and raises OnStateChanged upon completion.
  - Parameters:
    - `propertyName`: The property name to sort by (must exist on type T).
    - `isDescending`: True for descending sort order, false for ascending.
  - Returns: A task representing the asynchronous update operation.
- **UpdateFilter** - Adds, updates, or removes a simple filter and reloads data.
            If value is null, removes the filter for this property. If value is not null, adds or updates
            the filter. Resets pagination to page 1 when filter changes. Sets IsLoading to true during
            operation and raises OnStateChanged upon completion.
  - Parameters:
    - `property`: The property name to filter on (must exist on type T).
    - `filterOperator`: The filter operator (e.g., "equals", "contains"). Currently stored but not used;
            reserved for future operator-based filtering.
    - `value`: The filter value. If null, removes the filter for this property.
  - Returns: A task representing the asynchronous update operation.
- **UpdatePagination** - Updates pagination parameters (page number and page size) and reloads data.
            Page number is coerced to minimum of 1. Sets IsLoading to true during operation
            and raises OnStateChanged upon completion.
  - Parameters:
    - `pageNumber`: The 1-based page number. If less than 1, coerced to 1.
    - `pageSize`: The number of items per page.
  - Returns: A task representing the asynchronous update operation.
- **ToggleRowSelection** - Toggles the selection state of a single row.
            If the row ID is currently selected, removes it; otherwise adds it.
            Raises OnStateChanged synchronously upon completion.
  - Parameters:
    - `rowId`: The row identifier (extracted via reflection from the data object).
- **SetSelectedRows** - Replaces the entire selection with the specified row IDs.
            Clears any previously selected rows and sets the selection to exactly these IDs.
            Raises OnStateChanged synchronously upon completion.
  - Parameters:
    - `rowIds`: List of row identifiers to select. Can be empty to clear selection.
- **ToggleSelectAll** - Selects or deselects all rows on the current page.
            If isChecked is true, selects all rows in CurrentResponse. If false, clears selection.
            Calls SetSelectedRows internally, which raises OnStateChanged.
  - Parameters:
    - `isChecked`: True to select all visible rows, false to clear selection.
- **ClearFilters** - Removes all active filters and reloads data.
            Sets CurrentRequest.Filters to null and resets pagination to page 1.
            Note: Sort order is preserved (not cleared by this operation).
            Sets IsLoading to true during operation and raises OnStateChanged upon completion.
  - Returns: A task representing the asynchronous clear operation.
- **ExecuteWithStateManagement** - Wraps an async operation with state management: sets IsLoading, clears errors,
            executes the operation, and ensures IsLoading is cleared even if an exception occurs.
            Errors are captured and stored in Error property.
  - Parameters:
    - `operation`: The async operation to execute.
- **RefreshData** - Refreshes the data based on current request parameters.
            Currently a placeholder that ensures CurrentResponse is initialized.
            Future implementation will apply filtering/sorting and may integrate with GridDataProvider for API calls.
- **SetLoading** - Sets the IsLoading state.
- **SetError** - Sets the Error message.
- **ClearError** - Clears the Error message (sets to null).
- **RaiseStateChanged** - Raises the OnStateChanged event if subscribers exist.
- **GetRowId** - Extracts the row ID from an item using the cached ID property.
  - Parameters:
    - `item`: The data item to extract the ID from.
  - Returns: The row ID value, or the item itself if ID extraction fails.

### GridFilter

- **Namespace:** `SmartWorkz.Web.GridFilter`
- **Summary:** Represents a single filter condition with property, operator, and value.
            Reserved for future advanced filtering support (Phase 2 or later).
            Currently, simple equality filters are used via DataContext.Filters dictionary.

### IDataContext`1

- **Namespace:** `SmartWorkz.Web.IDataContext`1`
- **Summary:** Provides unified state management for multi-view data components (Grid, List, etc).
            Manages sorting, filtering, pagination, row selection, and loading/error states.

#### Methods & Properties

- **UpdateSort** - Update sort column and direction; triggers data fetch.
- **UpdateFilter** - Add or replace a filter; resets to page 1; triggers data fetch.
- **UpdatePagination** - Change current page number; triggers data fetch.
- **ToggleRowSelection** - Toggle selection state for a single row.
- **SetSelectedRows** - Replace all selected rows.
- **ToggleSelectAll** - Select/deselect all visible rows on current page.
- **ClearFilters** - Reset filters to default state and refetch data.
- **Initialize** - Initialize data from datasource or API endpoint.

### AccessibilityService

- **Namespace:** `SmartWorkz.Web.AccessibilityService`
- **Summary:** Service for generating accessible ARIA IDs and labels for form components.
            Provides WCAG-compliant identifiers and labels for accessible form rendering.

#### Methods & Properties

- **GenerateFieldId** - Generate unique ID for form field (for aria-labelledby, aria-describedby).
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized field ID in the format "field_{name}".
- **GenerateErrorId** - Generate error message ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized error ID in the format "error_{name}".
- **GenerateHintId** - Generate hint ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized hint ID in the format "hint_{name}".
- **GenerateAriaLabel** - Generate ARIA label text.
  - Parameters:
    - `fieldName`: The name of the form field.
    - `required`: Whether the field is required (appends "(required)" if true).
  - Returns: A formatted ARIA label text.
- **SanitizeName** - Sanitize field names by converting to lowercase and replacing special characters with underscores.
  - Parameters:
    - `name`: The field name to sanitize.
  - Returns: A sanitized field name containing only lowercase alphanumeric characters, underscores, and hyphens.

### FormComponentConfig

- **Namespace:** `SmartWorkz.Web.FormComponentConfig`
- **Summary:** Configuration class for form component styling with customizable Bootstrap CSS classes.
            Contains 18 properties for various form components and their variants.

### FormComponentProvider

- **Namespace:** `SmartWorkz.Web.FormComponentProvider`
- **Summary:** Provides form component styling configuration management.
            Manages Bootstrap CSS classes used throughout the form system.
            Allows customization of default Bootstrap styling.

#### Methods & Properties

- **GetConfiguration** - Get current form component configuration
  - Returns: The current FormComponentConfig instance containing all CSS class configurations.
- **UpdateConfiguration** - Update configuration with new values
  - Parameters:
    - `config`: The new FormComponentConfig to apply. Cannot be null.

### IAccessibilityService

- **Namespace:** `SmartWorkz.Web.IAccessibilityService`
- **Summary:** Service for generating accessible ARIA IDs and labels for form components.
            Supports WCAG compliance by providing consistent, properly formatted identifiers.

#### Methods & Properties

- **GenerateFieldId** - Generate unique ID for form field (for aria-labelledby, aria-describedby).
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized field ID in the format "field_{name}".
- **GenerateErrorId** - Generate error message ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized error ID in the format "error_{name}".
- **GenerateHintId** - Generate hint ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized hint ID in the format "hint_{name}".
- **GenerateAriaLabel** - Generate ARIA label text.
  - Parameters:
    - `fieldName`: The name of the form field.
    - `required`: Whether the field is required (appends "(required)" if true).
  - Returns: A formatted ARIA label text.

### IconProvider

- **Namespace:** `SmartWorkz.Web.IconProvider`
- **Summary:** Implementation of IIconProvider that provides Bootstrap icon CSS classes and HTML markup.

#### Methods & Properties

- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
  - Returns: A string containing the icon CSS classes (e.g., "bi bi-check-circle-fill").
- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type with an optional size modifier.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
    - `sizeClass`: Optional CSS class for sizing (e.g., "fs-5", "fs-6").
  - Returns: A string containing the icon CSS classes and size class if provided.
- **GetIconHtml** - Gets the complete HTML markup for the specified icon.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
    - `cssClass`: Optional CSS class to apply to the icon element.
  - Returns: HTML string containing the icon element.

### IFormComponentProvider

- **Namespace:** `SmartWorkz.Web.IFormComponentProvider`
- **Summary:** Provides access to form component styling configuration.
            Allows retrieval and updating of Bootstrap CSS classes used throughout the form system.

#### Methods & Properties

- **GetConfiguration** - Get current form component configuration
  - Returns: The current FormComponentConfig instance containing all CSS class configurations.
- **UpdateConfiguration** - Update configuration
  - Parameters:
    - `config`: The new FormComponentConfig to apply. Cannot be null.

### IconType

- **Namespace:** `SmartWorkz.Web.IconType`
- **Summary:** Enum representing common Bootstrap icons used throughout the application.

### IIconProvider

- **Namespace:** `SmartWorkz.Web.IIconProvider`
- **Summary:** Service interface for providing icon-related utilities.
            Centralizes icon management and provides methods to get icon CSS classes and HTML markup.

#### Methods & Properties

- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
  - Returns: A string containing the icon CSS classes (e.g., "bi bi-check-circle-fill").
- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type with an optional size modifier.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
    - `sizeClass`: Optional CSS class for sizing (e.g., "fs-5", "fs-6").
  - Returns: A string containing the icon CSS classes and size class if provided (e.g., "bi bi-check-circle-fill fs-5").

### IValidationMessageProvider

- **Namespace:** `SmartWorkz.Web.IValidationMessageProvider`
- **Summary:** Provides localized validation error messages with support for custom registration.

#### Methods & Properties

- **GetMessage** - Get validation message for given error type.
  - Parameters:
    - `errorType`: The type of validation error.
  - Returns: The validation message for the error type.
- **GetMessage** - Get validation message with property name included.
  - Parameters:
    - `errorType`: The type of validation error.
    - `propertyName`: The name of the property being validated.
  - Returns: The validation message with property name included.
- **RegisterMessage** - Register custom validation message.
  - Parameters:
    - `errorType`: The type of validation error to register.
    - `message`: The custom message for the error type.

### ValidationMessageProvider

- **Namespace:** `SmartWorkz.Web.ValidationMessageProvider`
- **Summary:** Provides localized validation error messages with support for custom registration.
            Includes 14 built-in validation messages for common validation scenarios.

#### Methods & Properties

- **GetMessage** - Get validation message for given error type.
  - Parameters:
    - `errorType`: The type of validation error.
  - Returns: The validation message for the error type.
- **GetMessage** - Get validation message with property name included.
  - Parameters:
    - `errorType`: The type of validation error.
    - `propertyName`: The name of the property being validated.
  - Returns: The validation message with property name included.
- **RegisterMessage** - Register custom validation message.
  - Parameters:
    - `errorType`: The type of validation error to register.
    - `message`: The custom message for the error type.

### IListViewFormatter

- **Namespace:** `SmartWorkz.Web.IListViewFormatter`
- **Summary:** Formats data for List/Card view display (dates, currency, text truncation, etc).

#### Methods & Properties

- **FormatDate** - Format a date value for display.
- **FormatCurrency** - Format a decimal value as currency.
- **TruncateText** - Truncate text to max length with ellipsis.
- **FormatBoolean** - Format a boolean as human-readable text.
- **FormatValue** - Format any object using type-aware rules.

### ListViewFormatter

- **Namespace:** `SmartWorkz.Web.ListViewFormatter`
- **Summary:** Formats raw data values for display in list and grid components.
            
             ListViewFormatter provides type-aware formatting for common data types encountered in
             data display scenarios: dates, currency amounts, text, booleans, and generic objects.
             It handles null values gracefully by displaying a dash (-) instead of empty/null text.

#### Methods & Properties

- **FormatDate** - Formats a date/time value using the specified .NET format string.
            Returns "-" if the date is null.
  - Parameters:
    - `date`: The date value to format. Can be null.
    - `format`: The .NET format string (default: "MMM dd, yyyy" for "Apr 22, 2026").
  - Returns: Formatted date string, or "-" if date is null.
- **FormatCurrency** - Formats a decimal value as currency with the specified symbol prefix.
            Returns "-" if the value is null. Always formats to 2 decimal places.
  - Parameters:
    - `value`: The decimal amount to format. Can be null.
    - `currencySymbol`: The currency symbol to prefix (default: "$").
  - Returns: Formatted currency string (e.g., "$1,234.50"), or "-" if value is null.
- **TruncateText** - Truncates text to a maximum length and appends "..." if truncated.
            Returns "-" if the text is null or empty.
  - Parameters:
    - `text`: The text to truncate. Can be null or empty.
    - `maxLength`: Maximum length before truncation (default: 100 characters).
  - Returns: Truncated text with "..." appended if over max length, "-" if null/empty, or original text if shorter.
- **FormatBoolean** - Formats a boolean value as human-readable text.
            Returns "Yes" for true, "No" for false, and "-" for null.
  - Parameters:
    - `value`: The boolean value to format. Can be null.
  - Returns: "Yes" for true, "No" for false, "-" for null.
- **FormatValue** - Formats any object value using type-aware detection and appropriate formatter.
            Uses pattern matching to dispatch to specialized formatters:
            - null → "-"
            - DateTime → FormatDate with default format
            - decimal → FormatCurrency with default symbol
            - bool → FormatBoolean (Yes/No)
            - string → TruncateText with default limit
            - Other → ToString() or "-" if ToString returns null
  - Parameters:
    - `value`: The value to format. Can be any type or null.
  - Returns: Formatted string appropriate to the value's type, or "-" if null or formatting fails.

### ViewConfiguration

- **Namespace:** `SmartWorkz.Web.ViewConfiguration`
- **Summary:** Stores view-specific configuration (visible columns, item layout, formatting rules).
            Allows different views to display the same data differently.

### GridDataProvider

- **Namespace:** `SmartWorkz.Web.GridDataProvider`
- **Summary:** Web-specific implementation of grid data fetching via HTTP API or in-memory sources.

#### Methods & Properties

- **GetDataAsync``1** - Fetch data from HTTP API endpoint.
- **ApplyGridLogic``1** - Apply sorting, filtering, and paging to an in-memory IEnumerable.
            Used when grid is bound to local data instead of an API.

### GridExportService

- **Namespace:** `SmartWorkz.Web.GridExportService`
- **Summary:** Service for exporting grid data to various formats (CSV, Excel).
            
             GridExportService exports tabular data from grid components to CSV or Excel formats.
             It handles column filtering, header inclusion, and proper escaping of special characters
             according to CSV standards (RFC 4180).

#### Methods & Properties

- **ExportToCsv``1** - Exports grid data to CSV format with proper escaping and optional column filtering.
  - Parameters:
    - `data`: The collection of items to export.
    - `columns`: The list of GridColumn definitions (PropertyName, DisplayName, IsVisible, etc).
    - `options`: Export options including Format, IncludeHeaders, IncludeColumns, ExcludeColumns.
  - Returns: A CSV-formatted string ready for file download or further processing.
- **ExportToExcel``1** - Exports grid data to Excel format. Currently not implemented.
  - Parameters:
    - `data`: The collection of items to export.
    - `columns`: The list of GridColumn definitions.
    - `options`: Export options (same as CSV).
  - Returns: Empty byte array. Actual implementation pending EPPlus dependency.
- **GetColumnsToExport** - Determines which columns to include in the export based on visibility and options.
  - Parameters:
    - `columns`: The complete list of available columns.
    - `options`: Export options specifying IncludeColumns and ExcludeColumns.
  - Returns: List of columns that should be included in the export.
- **EscapeCsv** - Escapes a value for CSV output according to RFC 4180 standard.
  - Parameters:
    - `value`: The raw value to escape. Can be null or empty.
  - Returns: The escaped value, quoted if necessary, ready for CSV output.

### GridStateManager

- **Namespace:** `SmartWorkz.Web.GridStateManager`
- **Summary:** Manages grid state (current page, sorting, filters, selected rows).
            Optionally persists state to browser localStorage.

#### Methods & Properties

- **UpdateRequest** - Update the current grid request and notify listeners.
- **UpdatePagination** - Update pagination (page and pageSize).
- **UpdateSort** - Update sorting.
- **UpdateFilters** - Update filters (replaces entire filter dictionary).
- **SetFilter** - Add or update a single filter.
- **RemoveFilter** - Remove a filter by column name.
- **ClearFilters** - Clear all filters.
- **SetSelectedRows** - Update selected row IDs.
- **ToggleRowSelection** - Toggle row selection.
- **SetLoading** - Set loading state.
- **SetError** - Set error message.
- **ClearError** - Clear error message.
- **Reset** - Reset all state to defaults.

### WebComponentExtensions

- **Namespace:** `SmartWorkz.Web.WebComponentExtensions`
- **Summary:** Extension methods for registering SmartWorkz.Web services with dependency injection.

#### Methods & Properties

- **AddSmartWorkzCoreWeb** - Register all SmartWorkz.Web services and TagHelpers with the dependency injection container.
            Registers component services as singletons for optimal performance.
            Note: TagHelpers are auto-discovered by ASP.NET Core and do not require explicit registration.
  - Parameters:
    - `services`: The IServiceCollection to register services with.
  - Returns: The IServiceCollection for method chaining.
- **AddSmartWorkzWebComponents** - Register all SmartWorkz.Web data view components and services.
  - Parameters:
    - `services`: The IServiceCollection to register services with.
  - Returns: The IServiceCollection for method chaining.

### ButtonTagHelper

- **Namespace:** `SmartWorkz.Web.ButtonTagHelper`
- **Summary:** TagHelper for rendering HTML button and link elements with Bootstrap button styling, size variants, and loading states.
             Targets the <button> and <a> elements when the Variant attribute is present and applies Bootstrap button CSS classes.
- **Example:**
```csharp
<!-- Primary button (default submit button) -->
             <button type="submit" variant="primary">Submit</button>
             <!-- Generates: <button class="btn btn-primary" type="submit">Submit</button> -->
            
             <!-- Small secondary button -->
             <button variant="secondary" size="sm">Cancel</button>
             <!-- Generates: <button class="btn btn-secondary btn-sm">Cancel</button> -->
            
             <!-- Large danger button -->
             <button variant="danger" size="lg">Delete</button>
             <!-- Generates: <button class="btn btn-danger btn-lg" disabled="disabled">Delete</button> -->
            
             <!-- Success button with loading state -->
             <button variant="success" is-loading="true">Processing...</button>
             <!-- Generates: <button class="btn btn-success disabled" disabled="disabled">Processing...</button> -->
            
             <!-- Link styled as a button -->
             <a href="/dashboard" variant="info">Go to Dashboard</a>
             <!-- Generates: <a class="btn btn-info" href="/dashboard">Go to Dashboard</a> -->
            
             <!-- Warning button with custom CSS class -->
             <button variant="warning" class="mt-2">Warning Action</button>
             <!-- Generates: <button class="mt-2 btn btn-warning">Warning Action</button> -->
```

### IconTagHelper

- **Namespace:** `SmartWorkz.Web.IconTagHelper`
- **Summary:** TagHelper for rendering Bootstrap Icon library icons with size and color customization.
             Targets the <icon> element and generates Bootstrap Icon HTML markup (<i class="bi bi-{name}"></i>).
- **Example:**
```csharp
<!-- Standalone success icon -->
             <icon name="Success" />
             <!-- Generates: <i class="bi bi-check-circle-fill"></i> -->
            
             <!-- Small icon with custom CSS class -->
             <icon name="Info" size="sm" css-class="text-info" />
             <!-- Generates: <i class="bi bi-info-circle me-1 text-info"></i> -->
            
             <!-- Large error icon in red -->
             <icon name="Error" size="lg" css-class="text-danger" />
             <!-- Generates: <i class="bi bi-exclamation-circle fs-5 text-danger"></i> -->
            
             <!-- Search icon in a button -->
             <button type="button" class="btn btn-primary">
               <icon name="Search" size="sm" /> Search
             </button>
            
             <!-- Warning icon with emphasis -->
             <icon name="Warning" size="lg" css-class="text-warning me-2" />
             <span>Please verify your information</span>
            
             <!-- Home navigation icon -->
             <a href="/"><icon name="Home" /> Home</a>
            
             <!-- User account icon -->
             <a href="/settings"><icon name="User" size="sm" /> Settings</a>
```

### AlertTagHelper

- **Namespace:** `SmartWorkz.Web.AlertTagHelper`
- **Summary:** TagHelper for rendering Bootstrap alert components with optional dismiss functionality.
             Targets the <alert> element and generates <div class="alert alert-{type}">.
- **Example:**
```csharp
<!-- Simple success alert -->
             <alert type="success" message="Profile updated successfully!" />
            
             <!-- Non-dismissible danger alert -->
             <alert type="danger" message="An error occurred while saving." dismissible="false" />
            
             <!-- Warning alert with dismiss button -->
             <alert type="warning" message="This action cannot be undone." />
            
             <!-- Default info alert -->
             <alert message="Remember to save your changes regularly." />
```

### BadgeTagHelper

- **Namespace:** `SmartWorkz.Web.BadgeTagHelper`
- **Summary:** TagHelper for rendering Bootstrap badge components for labels, counts, and status indicators.
             Targets the <badge> element and generates <span class="badge bg-{type}">.
- **Example:**
```csharp
<!-- Simple primary badge -->
             <badge type="primary" text="New" />
            
             <!-- Count badge -->
             <badge type="success" text="5 items" />
            
             <!-- Danger badge for inactive status -->
             <badge type="danger" text="Inactive" />
            
             <!-- Warning badge -->
             <badge type="warning" text="Pending Review" />
            
             <!-- Default secondary badge -->
             <badge text="Badge" />
            
             <!-- Pill-shaped badge -->
             <span class="badge bg-primary rounded-pill">@notificationCount</span>
```

### PaginationTagHelper

- **Namespace:** `SmartWorkz.Web.PaginationTagHelper`
- **Summary:** TagHelper for rendering Bootstrap pagination controls with automatic page calculation and link generation.
             Targets the <pagination> element and generates <nav><ul class="pagination"> navigation.
- **Example:**
```csharp
<!-- Basic pagination with defaults (shows 5 pages max) -->
             <pagination current-page="2" total-pages="10" />
            
             <!-- Pagination with custom page URL pattern -->
             <pagination current-page="1" total-pages="5" page-url="/products?page={0}" />
            
             <!-- Pagination with more visible pages -->
             <pagination current-page="3" total-pages="15" max-visible="7" />
            
             <!-- Pagination on last page (Next is disabled) -->
             <pagination current-page="10" total-pages="10" />
            
             <!-- Pagination with single page (not rendered) -->
             <pagination current-page="1" total-pages="1" />
```

### CheckboxTagHelper

- **Namespace:** `SmartWorkz.Web.CheckboxTagHelper`
- **Summary:** TagHelper for rendering HTML checkbox inputs with Bootstrap styling and label support.
             Targets the <checkbox-tag> element and generates <div class="form-check"> with checkbox input.
- **Example:**
```csharp
<!-- Simple checkbox with label -->
             <checkbox-tag for="User.IsSubscribed" label="Subscribe to newsletter" />
            
             <!-- Checkbox with custom value -->
             <checkbox-tag for="User.AgreedToTerms" label="I agree to the terms" value="1" />
            
             <!-- Pre-checked checkbox -->
             <checkbox-tag for="User.IsActive" label="Active" checked="true" />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.IsSubscribed" label="Newsletter Subscription" help-text="Get updates via email">
               <checkbox-tag for="User.IsSubscribed" label="Subscribe" />
             </form-group>
```

### FileInputTagHelper

- **Namespace:** `SmartWorkz.Web.FileInputTagHelper`
- **Summary:** TagHelper for rendering HTML file input elements with Bootstrap styling and validation support.
             Targets the <file-input-tag> element and generates <input type="file"> with form-control class.
- **Example:**
```csharp
<!-- Simple file input -->
             <file-input-tag for="Model.ProfilePhoto" />
            
             <!-- File input accepting specific types -->
             <file-input-tag for="Model.Document" accept=".pdf,.docx" required="true" />
            
             <!-- Multiple file upload -->
             <file-input-tag for="Model.Attachments" accept="image/*" multiple="true" />
            
             <!-- In form-group wrapper -->
             <form-group for="Model.ProfilePhoto" label="Upload Photo" required="true" help-text="JPG or PNG, max 5MB">
               <file-input-tag for="Model.ProfilePhoto" accept="image/jpeg,image/png" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new FileInputTagHelper with dependency injection.
  - Parameters:
    - `formComponentProvider`: Service for form component styling and configuration.

### FormGroupTagHelper

- **Namespace:** `SmartWorkz.Web.FormGroupTagHelper`
- **Summary:** TagHelper that wraps form controls with Bootstrap form-group styling, label, and help text support.
             Targets custom <form-group> element and renders a div wrapper with optional label and help text.
- **Example:**
```csharp
<!-- Simple form-group with text input -->
             <form-group for="Model.Name" label="Full Name" required="true">
               <input-tag for="Model.Name" placeholder="Enter your full name" />
             </form-group>
            
             <!-- Form-group with help text -->
             <form-group for="Model.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="Model.Email" type="email" placeholder="you@example.com" />
             </form-group>
            
             <!-- Form-group with select control -->
             <form-group for="Model.CountryId" label="Country" required="true" help-text="Select your country of residence">
               <select-tag for="Model.CountryId" items="@countries" />
             </form-group>
            
             <!-- Form-group with textarea -->
             <form-group for="Model.Comments" label="Comments" help-text="Optional feedback (max 500 characters)">
               <textarea-tag for="Model.Comments" rows="4" placeholder="Share your thoughts..." />
             </form-group>
            
             <!-- Form-group with checkbox -->
             <form-group for="Model.IsSubscribed" label="Newsletter">
               <checkbox-tag for="Model.IsSubscribed" label="Subscribe to newsletter" />
             </form-group>
            
             <!-- Form-group with validation error state -->
             <!-- When ModelState contains error for Model.Age, child controls show .is-invalid state -->
             <form-group for="Model.Age" label="Age" required="true" help-text="Must be 18 or older">
               <input-tag for="Model.Age" type="number" placeholder="Enter your age" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FormGroupTagHelper.
  - Parameters:
    - `formComponentProvider`: Provider for form component configuration.
    - `accessibilityService`: Service for generating accessible ARIA IDs.
- **Process** - Processes the form-group tag and renders a div wrapper with label and help text.

### FormTagHelper

- **Namespace:** `SmartWorkz.Web.FormTagHelper`
- **Summary:** TagHelper for rendering HTML form elements with Bootstrap validation styling and class management.
             Targets the <form-tag> element and generates <form class="needs-validation">.
- **Example:**
```csharp
<!-- Simple form with default POST method -->
             <form-tag>
               <form-group for="Model.Name" label="Full Name" required="true">
                 <input-tag for="Model.Name" placeholder="Enter your name" />
               </form-group>
               <button type="submit" class="btn btn-primary">Submit</button>
             </form-tag>
            
             <!-- Form with custom action and GET method -->
             <form-tag method="get" action="/search">
               <input-tag for="Model.SearchTerm" placeholder="Search..." />
               <button type="submit" class="btn btn-primary">Search</button>
             </form-tag>
            
             <!-- Form with custom CSS class and validation disabled -->
             <form-tag class="login-form" novalidate="true">
               <form-group for="Model.Email" label="Email" required="true">
                 <input-tag for="Model.Email" type="email" />
               </form-group>
               <button type="submit" class="btn btn-primary">Login</button>
             </form-tag>
```

### InputTagHelper

- **Namespace:** `SmartWorkz.Web.InputTagHelper`
- **Summary:** TagHelper for rendering HTML input elements with Bootstrap styling, optional icon support, and form component styling configuration.
             Targets the <input-tag> element and generates <input class="form-control"> with optional icon wrappers.
- **Example:**
```csharp
<!-- Simple text input -->
             <input-tag for="User.Name" placeholder="Enter your name" />
            
             <!-- Email input with validation -->
             <input-tag for="User.Email" type="email" placeholder="Enter email" required="true" />
            
             <!-- Password input -->
             <input-tag for="User.Password" type="password" placeholder="Enter password" />
            
             <!-- Number input with icon -->
             <input-tag for="Product.Price" type="number" step="0.01" icon-prefix="DollarSign" />
            
             <!-- Search input with icon -->
             <input-tag for="Model.SearchTerm" type="search" icon-suffix="Search" placeholder="Search..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="User.Email" type="email" icon-prefix="AtSign" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InputTagHelper class.
  - Parameters:
    - `formComponentProvider`: Provider for form component styling configuration.
    - `iconProvider`: Provider for icon rendering.
- **Process** - Processes the input-tag element and renders an HTML input element with optional icons and styling.
  - Parameters:
    - `context`: The TagHelperContext.
    - `output`: The TagHelperOutput.

### LabelTagHelper

- **Namespace:** `SmartWorkz.Web.LabelTagHelper`
- **Summary:** TagHelper for rendering HTML label elements with Bootstrap styling and required field indicators.
             Targets the <label-tag> element and generates <label class="form-label"> with optional required asterisk.
- **Example:**
```csharp
<!-- Simple label -->
             <label-tag for="User.Name" text="Full Name" />
            
             <!-- Required field with asterisk -->
             <label-tag for="User.Email" text="Email Address" required="true" />
            
             <!-- Optional field without asterisk -->
             <label-tag for="User.PhoneNumber" text="Phone Number" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <label-tag for="User.Email" text="Email Address" required="true" />
               <input-tag for="User.Email" type="email" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new LabelTagHelper with dependency injection.
  - Parameters:
    - `formComponentProvider`: Service for form component styling and configuration.
    - `accessibilityService`: Service for accessibility attributes and ARIA support.

### RadioButtonTagHelper

- **Namespace:** `SmartWorkz.Web.RadioButtonTagHelper`
- **Summary:** TagHelper for rendering HTML radio button inputs with Bootstrap styling, label support, and grouping.
             Targets the <radio-button-tag> element and generates <div class="form-check"> with radio input.
- **Example:**
```csharp
<!-- Single radio button -->
             <radio-button-tag for="Model.Status" group-name="Status" label="Active" value="active" />
            
             <!-- Radio button group (render multiple radio-button-tags with same group-name) -->
             <div>
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" checked="true" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="PayPal" value="paypal" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Bank Transfer" value="bank" />
             </div>
            
             <!-- Required radio button group -->
             <div>
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Standard (5-7 days)" value="standard" checked="true" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Express (2-3 days)" value="express" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Overnight" value="overnight" />
             </div>
            
             <!-- In form-group wrapper -->
             <form-group for="Model.PaymentMethod" label="Payment Method" required="true">
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Debit Card" value="dc" />
             </form-group>
```

### SelectTagHelper

- **Namespace:** `SmartWorkz.Web.SelectTagHelper`
- **Summary:** TagHelper for rendering HTML select elements with support for list items, enum binding, blank option handling, and Bootstrap styling.
             Targets the <select-tag> element and generates <select class="form-control"> with options.
- **Example:**
```csharp
<!-- Simple select with SelectListItem collection -->
             <select-tag for="User.CountryId" items="@countries" />
            
             <!-- Select with enum binding -->
             <select-tag for="Order.Status" enum-type="typeof(OrderStatus)" />
            
             <!-- Select without blank option -->
             <select-tag for="Product.Category" items="@categories" add-blank="false" />
            
             <!-- Select with custom blank text -->
             <select-tag for="User.Department" items="@departments" blank-text="Choose a department..." />
            
             <!-- With initial selection -->
             @{ var selected = categories.First(c => c.Value == "electronics"); selected.Selected = true; }
             <select-tag for="Product.Category" items="@categories" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.CountryId" label="Country" required="true" help-text="Select your country">
               <select-tag for="User.CountryId" items="@countries" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SelectTagHelper class.
  - Parameters:
    - `formComponentProvider`: Provider for form component styling configuration.
- **Process** - Processes the select-tag element and renders an HTML select element with options.
  - Parameters:
    - `context`: The TagHelperContext.
    - `output`: The TagHelperOutput.
- **GetSelectItems** - Gets the select items from either the Items property or EnumType property.
            If both Items and EnumType are provided, Items takes precedence.
  - Returns: A list of SelectListItem objects to render as options.

### TextAreaTagHelper

- **Namespace:** `SmartWorkz.Web.TextAreaTagHelper`
- **Summary:** TagHelper for rendering HTML textarea elements with Bootstrap styling, row configuration, and validation support.
             Targets the <textarea-tag> element and generates <textarea class="form-control">.
- **Example:**
```csharp
<!-- Simple textarea with default 3 rows -->
             <textarea-tag for="Model.Comments" placeholder="Enter your comments..." />
            
             <!-- Textarea with custom height -->
             <textarea-tag for="Model.Description" rows="6" placeholder="Enter detailed description" />
            
             <!-- Required textarea -->
             <textarea-tag for="Model.Feedback" placeholder="Your feedback is important" required="true" />
            
             <!-- Large textarea for longer content -->
             <textarea-tag for="Model.BioOrNotes" rows="10" placeholder="Tell us about yourself..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="Model.Comments" label="Comments" required="true" help-text="Please provide at least 10 characters">
               <textarea-tag for="Model.Comments" rows="5" placeholder="Share your thoughts..." />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new TextAreaTagHelper with dependency injection.
  - Parameters:
    - `formComponentProvider`: Service for form component styling and configuration.

### ValidationMessageTagHelper

- **Namespace:** `SmartWorkz.Web.ValidationMessageTagHelper`
- **Summary:** TagHelper for rendering HTML validation error messages with Bootstrap styling.
             Targets the <validation-message> element and generates <div class="invalid-feedback">.
- **Example:**
```csharp
<!-- Validation message for a field (after input element) -->
             <input-tag for="User.Email" type="email" />
             <validation-message for="User.Email" />
            
             <!-- Validation message with custom message -->
             <input-tag for="User.Age" type="number" />
             <validation-message for="User.Age" message="Age must be between 18 and 100" />
            
             <!-- Typical usage in form-group -->
             <form-group for="User.Email" label="Email" required="true">
               <input-tag for="User.Email" type="email" />
               <validation-message for="User.Email" />
             </form-group>
            
             <!-- Multiple fields with validation -->
             <form-group for="User.Password" label="Password" required="true">
               <input-tag for="User.Password" type="password" />
               <validation-message for="User.Password" />
             </form-group>
             <form-group for="User.ConfirmPassword" label="Confirm Password" required="true">
               <input-tag for="User.ConfirmPassword" type="password" />
               <validation-message for="User.ConfirmPassword" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new ValidationMessageTagHelper with dependency injection.
  - Parameters:
    - `accessibilityService`: Service for generating accessible error message IDs and ARIA support.

### GridTagHelper

- **Namespace:** `SmartWorkz.Web.GridTagHelper`
- **Summary:** TagHelper for rendering data grids with sorting, filtering, row selection, and pagination support.
             Targets the <grid> element and provides a high-level API for binding to IDataContext<T>
             and rendering grid data with Bootstrap table styling.
- **Example:**
```csharp
<!-- Basic grid with data binding -->
             <grid data-source="@Model.Products" data-page-size="20">
               <grid-column property="Name" sortable="true">Product Name</grid-column>
               <grid-column property="Price" sortable="true" format="currency">Price</grid-column>
             </grid>
            
             <!-- Grid with row selection enabled -->
             <grid data-source="@Model.Orders" data-page-size="50" data-allow-selection="true">
               <grid-column property="OrderId">Order ID</grid-column>
               <grid-column property="OrderDate" sortable="true" format="date">Date</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
             </grid>
            
             <!-- Grid with custom CSS class and export option -->
             <grid data-source="@Model.Customers"
                   data-page-size="25"
                   data-allow-export="true"
                   data-allow-column-toggle="true"
                   data-css-class="compact-grid">
               <grid-column property="FirstName">First Name</grid-column>
               <grid-column property="Email" sortable="true">Email</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
            
             <!-- Complete example with IDataContext<Product> binding -->
             @{
               var productDataContext = new DataContext<Product>(productService);
               await productDataContext.Initialize(await productService.GetProductsAsync());
             }
             <grid data-source="@productDataContext.Items"
                   data-page-size="20"
                   data-allow-selection="true">
               <grid-column property="Name" sortable="true">Product</grid-column>
               <grid-column property="Category" filterable="true">Category</grid-column>
               <grid-column property="Price" format="currency" css-class="text-end">Price</grid-column>
               <grid-column property="Stock" sortable="true">Stock</grid-column>
             </grid>
            
             <!-- Grid with filtering and sorting -->
             <div class="mb-3">
               <label for="statusFilter">Filter by Status:</label>
               <select id="statusFilter" onchange="updateFilter(this.value)">
                 <option value="">All</option>
                 <option value="Active">Active</option>
                 <option value="Inactive">Inactive</option>
               </select>
             </div>
            
             <grid data-source="@Model.Items" data-page-size="20">
               <grid-column property="Name" sortable="true">Name</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
```

### BreadcrumbItem

- **Namespace:** `SmartWorkz.Web.BreadcrumbItem`
- **Summary:** Data model representing a single item in a breadcrumb navigation trail.
             Used with BreadcrumbTagHelper to construct hierarchical navigation paths.
- **Example:**
```csharp
<!-- Create breadcrumb items for product details page -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Home", Url = "/" },
                 new() { Label = "Products", Url = "/products" },
                 new() { Label = "Electronics", Url = "/products/electronics" },
                 new() { Label = "Laptops", Url = "/products/electronics/laptops" },
                 new() { Label = "Dell XPS 13" }  // No URL - this is the current page
               };
             }
             <breadcrumb items="breadcrumbs" />
            
             <!-- Create from dynamic data -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" }
               };
            
               foreach (var folder in Model.FolderHierarchy)
               {
                 breadcrumbs.Add(new()
                 {
                   Label = folder.Name,
                   Url = folder.Depth < Model.FolderHierarchy.Count - 1 ? folder.Url : null
                 });
               }
             }
             <breadcrumb items="breadcrumbs" />
```

### BreadcrumbTagHelper

- **Namespace:** `SmartWorkz.Web.BreadcrumbTagHelper`
- **Summary:** TagHelper for rendering Bootstrap breadcrumb navigation showing the user's current location in site hierarchy.
             Targets the <breadcrumb> element and generates <nav><ol class="breadcrumb"> navigation.
- **Example:**
```csharp
<!-- Basic breadcrumb navigation -->
             <breadcrumb items="new List<BreadcrumbItem> {
               new() { Label = "Home", Url = "/" },
               new() { Label = "Products", Url = "/products" },
               new() { Label = "Electronics" }
             }" />
            
             <!-- Breadcrumb from controller action -->
             <breadcrumb items="Model.Breadcrumbs" />
            
             <!-- Programmatically constructed breadcrumb in view -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" },
                 new() { Label = "Reports", Url = "/reports" },
                 new() { Label = "Monthly Summary" }
               };
             }
             <breadcrumb items="breadcrumbs" />
```

### IfAuthorizedTagHelper

- **Namespace:** `SmartWorkz.Web.IfAuthorizedTagHelper`
- **Summary:** TagHelper for rendering content only if the user is authenticated.
             Targets the <if-authorized> element and conditionally renders its content based on authentication status.
- **Example:**
```csharp
<!-- Simple logout button, shown only if authenticated -->
             <if-authorized>
               <button class="btn btn-danger">Logout</button>
             </if-authorized>
            
             <!-- User profile section in header -->
             <if-authorized>
               <div class="user-profile">
                 <span>Welcome, @Model.User.Name!</span>
                 <a href="/profile">View Profile</a>
               </div>
             </if-authorized>
            
             <!-- Multiple buttons and links for authenticated users -->
             <if-authorized>
               <div class="authenticated-menu">
                 <a href="/dashboard" class="btn btn-primary">Dashboard</a>
                 <a href="/orders" class="btn btn-info">My Orders</a>
                 <button onclick="logout()" class="btn btn-danger">Logout</button>
               </div>
             </if-authorized>
            
             <!-- Unauthenticated content shown separately -->
             <div>
               <if-authorized>
                 <p>You are logged in.</p>
               </if-authorized>
               <!-- Note: For not-authenticated content, use negation in IfAuthorizedTagHelper
                    or implement a separate IfNotAuthorizedTagHelper -->
             </div>
```

### IfClaimTagHelper

- **Namespace:** `SmartWorkz.Web.IfClaimTagHelper`
- **Summary:** TagHelper for rendering content only if the user has a specific claim with a matching value.
             Targets the <if-claim> element and conditionally renders content based on claim verification.
- **Example:**
```csharp
<!-- Simple claim check: show if user has "role" claim with value "Admin" -->
             <if-claim type="role" value="Admin">
               <button class="btn btn-danger">Delete User</button>
             </if-claim>
            
             <!-- Multiple values (OR logic): show if user is Admin or Manager -->
             <if-claim type="role" value="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-claim>
            
             <!-- Department-based access: show if in IT or Finance department -->
             <if-claim type="department" value="IT,Finance">
               <div class="reports-section">
                 <h3>Financial Reports</h3>
                 <a href="/reports/finance">View Reports</a>
               </div>
             </if-claim>
            
             <!-- Subscription level: show premium features if subscription is Premium or Enterprise -->
             <if-claim type="subscription_level" value="Premium,Enterprise">
               <div class="premium-features">
                 <h3>Advanced Analytics</h3>
                 <p>You have access to advanced reporting features.</p>
               </div>
             </if-claim>
            
             <!-- Claim existence without value check: show if user has ANY custom-permission claim -->
             <if-claim type="custom-permission">
               <p>You have special permissions.</p>
             </if-claim>
            
             <!-- Combined with other markup: admin toolbar with multiple restrictions -->
             <div class="admin-toolbar">
               <if-claim type="role" value="Admin">
                 <button class="btn btn-danger" onclick="deleteAll()">Delete All</button>
               </if-claim>
               <if-claim type="role" value="Admin,Moderator">
                 <button class="btn btn-warning" onclick="moderate()">Moderate</button>
               </if-claim>
             </div>
```

### IfRoleTagHelper

- **Namespace:** `SmartWorkz.Web.IfRoleTagHelper`
- **Summary:** TagHelper for rendering content only if the user belongs to a specific role.
             Targets the <if-role> element and conditionally renders content based on role membership.
- **Example:**
```csharp
<!-- Simple role check: show delete button if user is Admin -->
             <if-role role="Admin">
               <button class="btn btn-danger" onclick="deleteItem()">Delete</button>
             </if-role>
            
             <!-- Multiple roles (OR logic): show if user is Admin or Manager -->
             <if-role role="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-role>
            
             <!-- Editor dashboard with edit/delete options -->
             <if-role role="Editor">
               <div class="editor-toolbar">
                 <button class="btn btn-info" onclick="editItem()">Edit</button>
                 <button class="btn btn-warning" onclick="publishItem()">Publish</button>
               </div>
             </if-role>
            
             <!-- Multiple role sections with different features -->
             <div class="dashboard">
               <if-role role="Admin">
                 <div class="admin-section">
                   <h3>System Administration</h3>
                   <a href="/admin/users">Manage Users</a>
                   <a href="/admin/settings">System Settings</a>
                 </div>
               </if-role>
               <if-role role="Manager,Editor">
                 <div class="content-section">
                   <h3>Content Management</h3>
                   <a href="/content">View Content</a>
                 </div>
               </if-role>
             </div>
            
             <!-- Nested role checks in a complex menu -->
             <nav class="sidebar">
               <ul>
                 <li><a href="/home">Home</a></li>
                 <if-role role="User,Admin,Manager">
                   <li><a href="/dashboard">Dashboard</a></li>
                   <li><a href="/profile">My Profile</a></li>
                 </if-role>
                 <if-role role="Admin">
                   <li><a href="/admin">Administration</a></li>
                 </if-role>
               </ul>
             </nav>
            
             <!-- Contributor role with limited permissions -->
             <if-role role="Contributor,Editor,Admin">
               <div class="publish-section">
                 <button class="btn btn-success" onclick="submit()">Submit for Review</button>
               </div>
             </if-role>
```

### CacheAttribute

- **Namespace:** `SmartWorkz.Web.CacheAttribute`
- **Summary:** Specifies response caching for MVC action methods and controllers.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.

### DataContext`1

- **Namespace:** `SmartWorkz.Web.DataContext`1`
- **Summary:** Manages state and state transitions for grid/list components with async data operations.
            
             DataContext is a state container that coordinates sorting, filtering, pagination, row selection,
             and loading/error states for data display components. It acts as a bridge between the UI
             (through Razor components) and the data layer, managing the current request parameters and
             response data along with user interactions (row selection, state changes).
- **Example:**
```csharp
// Initialize with data
             var context = new DataContext<Product>();
             await context.Initialize(products);
            
             // Subscribe to state changes
             context.OnStateChanged += () => Console.WriteLine("State changed!");
            
             // Filter by category - async operation
             await context.UpdateFilter("Category", "equals", "Electronics");
             // CurrentRequest.Filters now contains { "Category": "Electronics" }
             // CurrentResponse.Data reloaded asynchronously
            
             // Change sort - resets to page 1
             await context.UpdateSort("Price", isDescending: true);
            
             // Change pagination
             await context.UpdatePagination(pageNumber: 2, pageSize: 50);
            
             // Row selection - synchronous operations
             context.ToggleRowSelection(1);          // Select row with ID=1
             context.ToggleSelectAll(true);          // Select all visible rows
             var selected = context.SelectedRowIds;  // List<object> containing IDs
            
             // Check loading state
             if (context.IsLoading)
             {
                 // Show spinner
             }
            
             // Handle errors
             if (context.Error != null)
             {
                 Console.WriteLine($"Error: {context.Error}");
             }
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the DataContext class with default pagination (page 1, page size 20).
            Caches the row ID property via reflection during construction.
- **CacheIdProperty** - Caches the row ID property using reflection. Called during construction.
            Attempts to locate the ID property in this order:
            1. Property decorated with [Key] attribute
            2. Property named "Id" (case-insensitive)
            3. First property on the type
- **Initialize** - Initializes the context with data from a source and applies the current request parameters.
            Sets IsLoading to true during operation, updates CurrentResponse with the result,
            and raises OnStateChanged upon completion.
  - Parameters:
    - `dataSource`: The collection of items to load. Can be empty.
  - Returns: A task representing the asynchronous initialization.
- **UpdateSort** - Updates the sort parameters and reloads data.
            Resets pagination to page 1 when sort changes. Sets IsLoading to true during operation
            and raises OnStateChanged upon completion.
  - Parameters:
    - `propertyName`: The property name to sort by (must exist on type T).
    - `isDescending`: True for descending sort order, false for ascending.
  - Returns: A task representing the asynchronous update operation.
- **UpdateFilter** - Adds, updates, or removes a simple filter and reloads data.
            If value is null, removes the filter for this property. If value is not null, adds or updates
            the filter. Resets pagination to page 1 when filter changes. Sets IsLoading to true during
            operation and raises OnStateChanged upon completion.
  - Parameters:
    - `property`: The property name to filter on (must exist on type T).
    - `filterOperator`: The filter operator (e.g., "equals", "contains"). Currently stored but not used;
            reserved for future operator-based filtering.
    - `value`: The filter value. If null, removes the filter for this property.
  - Returns: A task representing the asynchronous update operation.
- **UpdatePagination** - Updates pagination parameters (page number and page size) and reloads data.
            Page number is coerced to minimum of 1. Sets IsLoading to true during operation
            and raises OnStateChanged upon completion.
  - Parameters:
    - `pageNumber`: The 1-based page number. If less than 1, coerced to 1.
    - `pageSize`: The number of items per page.
  - Returns: A task representing the asynchronous update operation.
- **ToggleRowSelection** - Toggles the selection state of a single row.
            If the row ID is currently selected, removes it; otherwise adds it.
            Raises OnStateChanged synchronously upon completion.
  - Parameters:
    - `rowId`: The row identifier (extracted via reflection from the data object).
- **SetSelectedRows** - Replaces the entire selection with the specified row IDs.
            Clears any previously selected rows and sets the selection to exactly these IDs.
            Raises OnStateChanged synchronously upon completion.
  - Parameters:
    - `rowIds`: List of row identifiers to select. Can be empty to clear selection.
- **ToggleSelectAll** - Selects or deselects all rows on the current page.
            If isChecked is true, selects all rows in CurrentResponse. If false, clears selection.
            Calls SetSelectedRows internally, which raises OnStateChanged.
  - Parameters:
    - `isChecked`: True to select all visible rows, false to clear selection.
- **ClearFilters** - Removes all active filters and reloads data.
            Sets CurrentRequest.Filters to null and resets pagination to page 1.
            Note: Sort order is preserved (not cleared by this operation).
            Sets IsLoading to true during operation and raises OnStateChanged upon completion.
  - Returns: A task representing the asynchronous clear operation.
- **ExecuteWithStateManagement** - Wraps an async operation with state management: sets IsLoading, clears errors,
            executes the operation, and ensures IsLoading is cleared even if an exception occurs.
            Errors are captured and stored in Error property.
  - Parameters:
    - `operation`: The async operation to execute.
- **RefreshData** - Refreshes the data based on current request parameters.
            Currently a placeholder that ensures CurrentResponse is initialized.
            Future implementation will apply filtering/sorting and may integrate with GridDataProvider for API calls.
- **SetLoading** - Sets the IsLoading state.
- **SetError** - Sets the Error message.
- **ClearError** - Clears the Error message (sets to null).
- **RaiseStateChanged** - Raises the OnStateChanged event if subscribers exist.
- **GetRowId** - Extracts the row ID from an item using the cached ID property.
  - Parameters:
    - `item`: The data item to extract the ID from.
  - Returns: The row ID value, or the item itself if ID extraction fails.

### GridFilter

- **Namespace:** `SmartWorkz.Web.GridFilter`
- **Summary:** Represents a single filter condition with property, operator, and value.
            Reserved for future advanced filtering support (Phase 2 or later).
            Currently, simple equality filters are used via DataContext.Filters dictionary.

### IDataContext`1

- **Namespace:** `SmartWorkz.Web.IDataContext`1`
- **Summary:** Provides unified state management for multi-view data components (Grid, List, etc).
            Manages sorting, filtering, pagination, row selection, and loading/error states.

#### Methods & Properties

- **UpdateSort** - Update sort column and direction; triggers data fetch.
- **UpdateFilter** - Add or replace a filter; resets to page 1; triggers data fetch.
- **UpdatePagination** - Change current page number; triggers data fetch.
- **ToggleRowSelection** - Toggle selection state for a single row.
- **SetSelectedRows** - Replace all selected rows.
- **ToggleSelectAll** - Select/deselect all visible rows on current page.
- **ClearFilters** - Reset filters to default state and refetch data.
- **Initialize** - Initialize data from datasource or API endpoint.

### AccessibilityService

- **Namespace:** `SmartWorkz.Web.AccessibilityService`
- **Summary:** Service for generating accessible ARIA IDs and labels for form components.
            Provides WCAG-compliant identifiers and labels for accessible form rendering.

#### Methods & Properties

- **GenerateFieldId** - Generate unique ID for form field (for aria-labelledby, aria-describedby).
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized field ID in the format "field_{name}".
- **GenerateErrorId** - Generate error message ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized error ID in the format "error_{name}".
- **GenerateHintId** - Generate hint ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized hint ID in the format "hint_{name}".
- **GenerateAriaLabel** - Generate ARIA label text.
  - Parameters:
    - `fieldName`: The name of the form field.
    - `required`: Whether the field is required (appends "(required)" if true).
  - Returns: A formatted ARIA label text.
- **SanitizeName** - Sanitize field names by converting to lowercase and replacing special characters with underscores.
  - Parameters:
    - `name`: The field name to sanitize.
  - Returns: A sanitized field name containing only lowercase alphanumeric characters, underscores, and hyphens.

### FormComponentConfig

- **Namespace:** `SmartWorkz.Web.FormComponentConfig`
- **Summary:** Configuration class for form component styling with customizable Bootstrap CSS classes.
            Contains 18 properties for various form components and their variants.

### FormComponentProvider

- **Namespace:** `SmartWorkz.Web.FormComponentProvider`
- **Summary:** Provides form component styling configuration management.
            Manages Bootstrap CSS classes used throughout the form system.
            Allows customization of default Bootstrap styling.

#### Methods & Properties

- **GetConfiguration** - Get current form component configuration
  - Returns: The current FormComponentConfig instance containing all CSS class configurations.
- **UpdateConfiguration** - Update configuration with new values
  - Parameters:
    - `config`: The new FormComponentConfig to apply. Cannot be null.

### IAccessibilityService

- **Namespace:** `SmartWorkz.Web.IAccessibilityService`
- **Summary:** Service for generating accessible ARIA IDs and labels for form components.
            Supports WCAG compliance by providing consistent, properly formatted identifiers.

#### Methods & Properties

- **GenerateFieldId** - Generate unique ID for form field (for aria-labelledby, aria-describedby).
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized field ID in the format "field_{name}".
- **GenerateErrorId** - Generate error message ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized error ID in the format "error_{name}".
- **GenerateHintId** - Generate hint ID.
  - Parameters:
    - `fieldName`: The name of the form field.
  - Returns: A sanitized hint ID in the format "hint_{name}".
- **GenerateAriaLabel** - Generate ARIA label text.
  - Parameters:
    - `fieldName`: The name of the form field.
    - `required`: Whether the field is required (appends "(required)" if true).
  - Returns: A formatted ARIA label text.

### IconProvider

- **Namespace:** `SmartWorkz.Web.IconProvider`
- **Summary:** Implementation of IIconProvider that provides Bootstrap icon CSS classes and HTML markup.

#### Methods & Properties

- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
  - Returns: A string containing the icon CSS classes (e.g., "bi bi-check-circle-fill").
- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type with an optional size modifier.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
    - `sizeClass`: Optional CSS class for sizing (e.g., "fs-5", "fs-6").
  - Returns: A string containing the icon CSS classes and size class if provided.
- **GetIconHtml** - Gets the complete HTML markup for the specified icon.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
    - `cssClass`: Optional CSS class to apply to the icon element.
  - Returns: HTML string containing the icon element.

### IFormComponentProvider

- **Namespace:** `SmartWorkz.Web.IFormComponentProvider`
- **Summary:** Provides access to form component styling configuration.
            Allows retrieval and updating of Bootstrap CSS classes used throughout the form system.

#### Methods & Properties

- **GetConfiguration** - Get current form component configuration
  - Returns: The current FormComponentConfig instance containing all CSS class configurations.
- **UpdateConfiguration** - Update configuration
  - Parameters:
    - `config`: The new FormComponentConfig to apply. Cannot be null.

### IconType

- **Namespace:** `SmartWorkz.Web.IconType`
- **Summary:** Enum representing common Bootstrap icons used throughout the application.

### IIconProvider

- **Namespace:** `SmartWorkz.Web.IIconProvider`
- **Summary:** Service interface for providing icon-related utilities.
            Centralizes icon management and provides methods to get icon CSS classes and HTML markup.

#### Methods & Properties

- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
  - Returns: A string containing the icon CSS classes (e.g., "bi bi-check-circle-fill").
- **GetIconClass** - Gets the Bootstrap icon CSS class for the specified icon type with an optional size modifier.
  - Parameters:
    - `iconType`: The type of icon to retrieve.
    - `sizeClass`: Optional CSS class for sizing (e.g., "fs-5", "fs-6").
  - Returns: A string containing the icon CSS classes and size class if provided (e.g., "bi bi-check-circle-fill fs-5").

### IValidationMessageProvider

- **Namespace:** `SmartWorkz.Web.IValidationMessageProvider`
- **Summary:** Provides localized validation error messages with support for custom registration.

#### Methods & Properties

- **GetMessage** - Get validation message for given error type.
  - Parameters:
    - `errorType`: The type of validation error.
  - Returns: The validation message for the error type.
- **GetMessage** - Get validation message with property name included.
  - Parameters:
    - `errorType`: The type of validation error.
    - `propertyName`: The name of the property being validated.
  - Returns: The validation message with property name included.
- **RegisterMessage** - Register custom validation message.
  - Parameters:
    - `errorType`: The type of validation error to register.
    - `message`: The custom message for the error type.

### ValidationMessageProvider

- **Namespace:** `SmartWorkz.Web.ValidationMessageProvider`
- **Summary:** Provides localized validation error messages with support for custom registration.
            Includes 14 built-in validation messages for common validation scenarios.

#### Methods & Properties

- **GetMessage** - Get validation message for given error type.
  - Parameters:
    - `errorType`: The type of validation error.
  - Returns: The validation message for the error type.
- **GetMessage** - Get validation message with property name included.
  - Parameters:
    - `errorType`: The type of validation error.
    - `propertyName`: The name of the property being validated.
  - Returns: The validation message with property name included.
- **RegisterMessage** - Register custom validation message.
  - Parameters:
    - `errorType`: The type of validation error to register.
    - `message`: The custom message for the error type.

### IListViewFormatter

- **Namespace:** `SmartWorkz.Web.IListViewFormatter`
- **Summary:** Formats data for List/Card view display (dates, currency, text truncation, etc).

#### Methods & Properties

- **FormatDate** - Format a date value for display.
- **FormatCurrency** - Format a decimal value as currency.
- **TruncateText** - Truncate text to max length with ellipsis.
- **FormatBoolean** - Format a boolean as human-readable text.
- **FormatValue** - Format any object using type-aware rules.

### ListViewFormatter

- **Namespace:** `SmartWorkz.Web.ListViewFormatter`
- **Summary:** Formats raw data values for display in list and grid components.
            
             ListViewFormatter provides type-aware formatting for common data types encountered in
             data display scenarios: dates, currency amounts, text, booleans, and generic objects.
             It handles null values gracefully by displaying a dash (-) instead of empty/null text.

#### Methods & Properties

- **FormatDate** - Formats a date/time value using the specified .NET format string.
            Returns "-" if the date is null.
  - Parameters:
    - `date`: The date value to format. Can be null.
    - `format`: The .NET format string (default: "MMM dd, yyyy" for "Apr 22, 2026").
  - Returns: Formatted date string, or "-" if date is null.
- **FormatCurrency** - Formats a decimal value as currency with the specified symbol prefix.
            Returns "-" if the value is null. Always formats to 2 decimal places.
  - Parameters:
    - `value`: The decimal amount to format. Can be null.
    - `currencySymbol`: The currency symbol to prefix (default: "$").
  - Returns: Formatted currency string (e.g., "$1,234.50"), or "-" if value is null.
- **TruncateText** - Truncates text to a maximum length and appends "..." if truncated.
            Returns "-" if the text is null or empty.
  - Parameters:
    - `text`: The text to truncate. Can be null or empty.
    - `maxLength`: Maximum length before truncation (default: 100 characters).
  - Returns: Truncated text with "..." appended if over max length, "-" if null/empty, or original text if shorter.
- **FormatBoolean** - Formats a boolean value as human-readable text.
            Returns "Yes" for true, "No" for false, and "-" for null.
  - Parameters:
    - `value`: The boolean value to format. Can be null.
  - Returns: "Yes" for true, "No" for false, "-" for null.
- **FormatValue** - Formats any object value using type-aware detection and appropriate formatter.
            Uses pattern matching to dispatch to specialized formatters:
            - null → "-"
            - DateTime → FormatDate with default format
            - decimal → FormatCurrency with default symbol
            - bool → FormatBoolean (Yes/No)
            - string → TruncateText with default limit
            - Other → ToString() or "-" if ToString returns null
  - Parameters:
    - `value`: The value to format. Can be any type or null.
  - Returns: Formatted string appropriate to the value's type, or "-" if null or formatting fails.

### ViewConfiguration

- **Namespace:** `SmartWorkz.Web.ViewConfiguration`
- **Summary:** Stores view-specific configuration (visible columns, item layout, formatting rules).
            Allows different views to display the same data differently.

### GridDataProvider

- **Namespace:** `SmartWorkz.Web.GridDataProvider`
- **Summary:** Web-specific implementation of grid data fetching via HTTP API or in-memory sources.

#### Methods & Properties

- **GetDataAsync``1** - Fetch data from HTTP API endpoint.
- **ApplyGridLogic``1** - Apply sorting, filtering, and paging to an in-memory IEnumerable.
            Used when grid is bound to local data instead of an API.

### GridExportService

- **Namespace:** `SmartWorkz.Web.GridExportService`
- **Summary:** Service for exporting grid data to various formats (CSV, Excel).
            
             GridExportService exports tabular data from grid components to CSV or Excel formats.
             It handles column filtering, header inclusion, and proper escaping of special characters
             according to CSV standards (RFC 4180).

#### Methods & Properties

- **ExportToCsv``1** - Exports grid data to CSV format with proper escaping and optional column filtering.
  - Parameters:
    - `data`: The collection of items to export.
    - `columns`: The list of GridColumn definitions (PropertyName, DisplayName, IsVisible, etc).
    - `options`: Export options including Format, IncludeHeaders, IncludeColumns, ExcludeColumns.
  - Returns: A CSV-formatted string ready for file download or further processing.
- **ExportToExcel``1** - Exports grid data to Excel format. Currently not implemented.
  - Parameters:
    - `data`: The collection of items to export.
    - `columns`: The list of GridColumn definitions.
    - `options`: Export options (same as CSV).
  - Returns: Empty byte array. Actual implementation pending EPPlus dependency.
- **GetColumnsToExport** - Determines which columns to include in the export based on visibility and options.
  - Parameters:
    - `columns`: The complete list of available columns.
    - `options`: Export options specifying IncludeColumns and ExcludeColumns.
  - Returns: List of columns that should be included in the export.
- **EscapeCsv** - Escapes a value for CSV output according to RFC 4180 standard.
  - Parameters:
    - `value`: The raw value to escape. Can be null or empty.
  - Returns: The escaped value, quoted if necessary, ready for CSV output.

### GridStateManager

- **Namespace:** `SmartWorkz.Web.GridStateManager`
- **Summary:** Manages grid state (current page, sorting, filters, selected rows).
            Optionally persists state to browser localStorage.

#### Methods & Properties

- **UpdateRequest** - Update the current grid request and notify listeners.
- **UpdatePagination** - Update pagination (page and pageSize).
- **UpdateSort** - Update sorting.
- **UpdateFilters** - Update filters (replaces entire filter dictionary).
- **SetFilter** - Add or update a single filter.
- **RemoveFilter** - Remove a filter by column name.
- **ClearFilters** - Clear all filters.
- **SetSelectedRows** - Update selected row IDs.
- **ToggleRowSelection** - Toggle row selection.
- **SetLoading** - Set loading state.
- **SetError** - Set error message.
- **ClearError** - Clear error message.
- **Reset** - Reset all state to defaults.

### WebComponentExtensions

- **Namespace:** `SmartWorkz.Web.WebComponentExtensions`
- **Summary:** Extension methods for registering SmartWorkz.Web services with dependency injection.

#### Methods & Properties

- **AddSmartWorkzCoreWeb** - Register all SmartWorkz.Web services and TagHelpers with the dependency injection container.
            Registers component services as singletons for optimal performance.
            Note: TagHelpers are auto-discovered by ASP.NET Core and do not require explicit registration.
  - Parameters:
    - `services`: The IServiceCollection to register services with.
  - Returns: The IServiceCollection for method chaining.
- **AddSmartWorkzWebComponents** - Register all SmartWorkz.Web data view components and services.
  - Parameters:
    - `services`: The IServiceCollection to register services with.
  - Returns: The IServiceCollection for method chaining.

### ButtonTagHelper

- **Namespace:** `SmartWorkz.Web.ButtonTagHelper`
- **Summary:** TagHelper for rendering HTML button and link elements with Bootstrap button styling, size variants, and loading states.
             Targets the <button> and <a> elements when the Variant attribute is present and applies Bootstrap button CSS classes.
- **Example:**
```csharp
<!-- Primary button (default submit button) -->
             <button type="submit" variant="primary">Submit</button>
             <!-- Generates: <button class="btn btn-primary" type="submit">Submit</button> -->
            
             <!-- Small secondary button -->
             <button variant="secondary" size="sm">Cancel</button>
             <!-- Generates: <button class="btn btn-secondary btn-sm">Cancel</button> -->
            
             <!-- Large danger button -->
             <button variant="danger" size="lg">Delete</button>
             <!-- Generates: <button class="btn btn-danger btn-lg" disabled="disabled">Delete</button> -->
            
             <!-- Success button with loading state -->
             <button variant="success" is-loading="true">Processing...</button>
             <!-- Generates: <button class="btn btn-success disabled" disabled="disabled">Processing...</button> -->
            
             <!-- Link styled as a button -->
             <a href="/dashboard" variant="info">Go to Dashboard</a>
             <!-- Generates: <a class="btn btn-info" href="/dashboard">Go to Dashboard</a> -->
            
             <!-- Warning button with custom CSS class -->
             <button variant="warning" class="mt-2">Warning Action</button>
             <!-- Generates: <button class="mt-2 btn btn-warning">Warning Action</button> -->
```

### IconTagHelper

- **Namespace:** `SmartWorkz.Web.IconTagHelper`
- **Summary:** TagHelper for rendering Bootstrap Icon library icons with size and color customization.
             Targets the <icon> element and generates Bootstrap Icon HTML markup (<i class="bi bi-{name}"></i>).
- **Example:**
```csharp
<!-- Standalone success icon -->
             <icon name="Success" />
             <!-- Generates: <i class="bi bi-check-circle-fill"></i> -->
            
             <!-- Small icon with custom CSS class -->
             <icon name="Info" size="sm" css-class="text-info" />
             <!-- Generates: <i class="bi bi-info-circle me-1 text-info"></i> -->
            
             <!-- Large error icon in red -->
             <icon name="Error" size="lg" css-class="text-danger" />
             <!-- Generates: <i class="bi bi-exclamation-circle fs-5 text-danger"></i> -->
            
             <!-- Search icon in a button -->
             <button type="button" class="btn btn-primary">
               <icon name="Search" size="sm" /> Search
             </button>
            
             <!-- Warning icon with emphasis -->
             <icon name="Warning" size="lg" css-class="text-warning me-2" />
             <span>Please verify your information</span>
            
             <!-- Home navigation icon -->
             <a href="/"><icon name="Home" /> Home</a>
            
             <!-- User account icon -->
             <a href="/settings"><icon name="User" size="sm" /> Settings</a>
```

### AlertTagHelper

- **Namespace:** `SmartWorkz.Web.AlertTagHelper`
- **Summary:** TagHelper for rendering Bootstrap alert components with optional dismiss functionality.
             Targets the <alert> element and generates <div class="alert alert-{type}">.
- **Example:**
```csharp
<!-- Simple success alert -->
             <alert type="success" message="Profile updated successfully!" />
            
             <!-- Non-dismissible danger alert -->
             <alert type="danger" message="An error occurred while saving." dismissible="false" />
            
             <!-- Warning alert with dismiss button -->
             <alert type="warning" message="This action cannot be undone." />
            
             <!-- Default info alert -->
             <alert message="Remember to save your changes regularly." />
```

### BadgeTagHelper

- **Namespace:** `SmartWorkz.Web.BadgeTagHelper`
- **Summary:** TagHelper for rendering Bootstrap badge components for labels, counts, and status indicators.
             Targets the <badge> element and generates <span class="badge bg-{type}">.
- **Example:**
```csharp
<!-- Simple primary badge -->
             <badge type="primary" text="New" />
            
             <!-- Count badge -->
             <badge type="success" text="5 items" />
            
             <!-- Danger badge for inactive status -->
             <badge type="danger" text="Inactive" />
            
             <!-- Warning badge -->
             <badge type="warning" text="Pending Review" />
            
             <!-- Default secondary badge -->
             <badge text="Badge" />
            
             <!-- Pill-shaped badge -->
             <span class="badge bg-primary rounded-pill">@notificationCount</span>
```

### PaginationTagHelper

- **Namespace:** `SmartWorkz.Web.PaginationTagHelper`
- **Summary:** TagHelper for rendering Bootstrap pagination controls with automatic page calculation and link generation.
             Targets the <pagination> element and generates <nav><ul class="pagination"> navigation.
- **Example:**
```csharp
<!-- Basic pagination with defaults (shows 5 pages max) -->
             <pagination current-page="2" total-pages="10" />
            
             <!-- Pagination with custom page URL pattern -->
             <pagination current-page="1" total-pages="5" page-url="/products?page={0}" />
            
             <!-- Pagination with more visible pages -->
             <pagination current-page="3" total-pages="15" max-visible="7" />
            
             <!-- Pagination on last page (Next is disabled) -->
             <pagination current-page="10" total-pages="10" />
            
             <!-- Pagination with single page (not rendered) -->
             <pagination current-page="1" total-pages="1" />
```

### CheckboxTagHelper

- **Namespace:** `SmartWorkz.Web.CheckboxTagHelper`
- **Summary:** TagHelper for rendering HTML checkbox inputs with Bootstrap styling and label support.
             Targets the <checkbox-tag> element and generates <div class="form-check"> with checkbox input.
- **Example:**
```csharp
<!-- Simple checkbox with label -->
             <checkbox-tag for="User.IsSubscribed" label="Subscribe to newsletter" />
            
             <!-- Checkbox with custom value -->
             <checkbox-tag for="User.AgreedToTerms" label="I agree to the terms" value="1" />
            
             <!-- Pre-checked checkbox -->
             <checkbox-tag for="User.IsActive" label="Active" checked="true" />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.IsSubscribed" label="Newsletter Subscription" help-text="Get updates via email">
               <checkbox-tag for="User.IsSubscribed" label="Subscribe" />
             </form-group>
```

### FileInputTagHelper

- **Namespace:** `SmartWorkz.Web.FileInputTagHelper`
- **Summary:** TagHelper for rendering HTML file input elements with Bootstrap styling and validation support.
             Targets the <file-input-tag> element and generates <input type="file"> with form-control class.
- **Example:**
```csharp
<!-- Simple file input -->
             <file-input-tag for="Model.ProfilePhoto" />
            
             <!-- File input accepting specific types -->
             <file-input-tag for="Model.Document" accept=".pdf,.docx" required="true" />
            
             <!-- Multiple file upload -->
             <file-input-tag for="Model.Attachments" accept="image/*" multiple="true" />
            
             <!-- In form-group wrapper -->
             <form-group for="Model.ProfilePhoto" label="Upload Photo" required="true" help-text="JPG or PNG, max 5MB">
               <file-input-tag for="Model.ProfilePhoto" accept="image/jpeg,image/png" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new FileInputTagHelper with dependency injection.
  - Parameters:
    - `formComponentProvider`: Service for form component styling and configuration.

### FormGroupTagHelper

- **Namespace:** `SmartWorkz.Web.FormGroupTagHelper`
- **Summary:** TagHelper that wraps form controls with Bootstrap form-group styling, label, and help text support.
             Targets custom <form-group> element and renders a div wrapper with optional label and help text.
- **Example:**
```csharp
<!-- Simple form-group with text input -->
             <form-group for="Model.Name" label="Full Name" required="true">
               <input-tag for="Model.Name" placeholder="Enter your full name" />
             </form-group>
            
             <!-- Form-group with help text -->
             <form-group for="Model.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="Model.Email" type="email" placeholder="you@example.com" />
             </form-group>
            
             <!-- Form-group with select control -->
             <form-group for="Model.CountryId" label="Country" required="true" help-text="Select your country of residence">
               <select-tag for="Model.CountryId" items="@countries" />
             </form-group>
            
             <!-- Form-group with textarea -->
             <form-group for="Model.Comments" label="Comments" help-text="Optional feedback (max 500 characters)">
               <textarea-tag for="Model.Comments" rows="4" placeholder="Share your thoughts..." />
             </form-group>
            
             <!-- Form-group with checkbox -->
             <form-group for="Model.IsSubscribed" label="Newsletter">
               <checkbox-tag for="Model.IsSubscribed" label="Subscribe to newsletter" />
             </form-group>
            
             <!-- Form-group with validation error state -->
             <!-- When ModelState contains error for Model.Age, child controls show .is-invalid state -->
             <form-group for="Model.Age" label="Age" required="true" help-text="Must be 18 or older">
               <input-tag for="Model.Age" type="number" placeholder="Enter your age" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FormGroupTagHelper.
  - Parameters:
    - `formComponentProvider`: Provider for form component configuration.
    - `accessibilityService`: Service for generating accessible ARIA IDs.
- **Process** - Processes the form-group tag and renders a div wrapper with label and help text.

### FormTagHelper

- **Namespace:** `SmartWorkz.Web.FormTagHelper`
- **Summary:** TagHelper for rendering HTML form elements with Bootstrap validation styling and class management.
             Targets the <form-tag> element and generates <form class="needs-validation">.
- **Example:**
```csharp
<!-- Simple form with default POST method -->
             <form-tag>
               <form-group for="Model.Name" label="Full Name" required="true">
                 <input-tag for="Model.Name" placeholder="Enter your name" />
               </form-group>
               <button type="submit" class="btn btn-primary">Submit</button>
             </form-tag>
            
             <!-- Form with custom action and GET method -->
             <form-tag method="get" action="/search">
               <input-tag for="Model.SearchTerm" placeholder="Search..." />
               <button type="submit" class="btn btn-primary">Search</button>
             </form-tag>
            
             <!-- Form with custom CSS class and validation disabled -->
             <form-tag class="login-form" novalidate="true">
               <form-group for="Model.Email" label="Email" required="true">
                 <input-tag for="Model.Email" type="email" />
               </form-group>
               <button type="submit" class="btn btn-primary">Login</button>
             </form-tag>
```

### InputTagHelper

- **Namespace:** `SmartWorkz.Web.InputTagHelper`
- **Summary:** TagHelper for rendering HTML input elements with Bootstrap styling, optional icon support, and form component styling configuration.
             Targets the <input-tag> element and generates <input class="form-control"> with optional icon wrappers.
- **Example:**
```csharp
<!-- Simple text input -->
             <input-tag for="User.Name" placeholder="Enter your name" />
            
             <!-- Email input with validation -->
             <input-tag for="User.Email" type="email" placeholder="Enter email" required="true" />
            
             <!-- Password input -->
             <input-tag for="User.Password" type="password" placeholder="Enter password" />
            
             <!-- Number input with icon -->
             <input-tag for="Product.Price" type="number" step="0.01" icon-prefix="DollarSign" />
            
             <!-- Search input with icon -->
             <input-tag for="Model.SearchTerm" type="search" icon-suffix="Search" placeholder="Search..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <input-tag for="User.Email" type="email" icon-prefix="AtSign" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InputTagHelper class.
  - Parameters:
    - `formComponentProvider`: Provider for form component styling configuration.
    - `iconProvider`: Provider for icon rendering.
- **Process** - Processes the input-tag element and renders an HTML input element with optional icons and styling.
  - Parameters:
    - `context`: The TagHelperContext.
    - `output`: The TagHelperOutput.

### LabelTagHelper

- **Namespace:** `SmartWorkz.Web.LabelTagHelper`
- **Summary:** TagHelper for rendering HTML label elements with Bootstrap styling and required field indicators.
             Targets the <label-tag> element and generates <label class="form-label"> with optional required asterisk.
- **Example:**
```csharp
<!-- Simple label -->
             <label-tag for="User.Name" text="Full Name" />
            
             <!-- Required field with asterisk -->
             <label-tag for="User.Email" text="Email Address" required="true" />
            
             <!-- Optional field without asterisk -->
             <label-tag for="User.PhoneNumber" text="Phone Number" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email">
               <label-tag for="User.Email" text="Email Address" required="true" />
               <input-tag for="User.Email" type="email" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new LabelTagHelper with dependency injection.
  - Parameters:
    - `formComponentProvider`: Service for form component styling and configuration.
    - `accessibilityService`: Service for accessibility attributes and ARIA support.

### RadioButtonTagHelper

- **Namespace:** `SmartWorkz.Web.RadioButtonTagHelper`
- **Summary:** TagHelper for rendering HTML radio button inputs with Bootstrap styling, label support, and grouping.
             Targets the <radio-button-tag> element and generates <div class="form-check"> with radio input.
- **Example:**
```csharp
<!-- Single radio button -->
             <radio-button-tag for="Model.Status" group-name="Status" label="Active" value="active" />
            
             <!-- Radio button group (render multiple radio-button-tags with same group-name) -->
             <div>
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" checked="true" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="PayPal" value="paypal" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Bank Transfer" value="bank" />
             </div>
            
             <!-- Required radio button group -->
             <div>
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Standard (5-7 days)" value="standard" checked="true" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Express (2-3 days)" value="express" />
               <radio-button-tag for="Model.Shipping" group-name="Shipping" label="Overnight" value="overnight" />
             </div>
            
             <!-- In form-group wrapper -->
             <form-group for="Model.PaymentMethod" label="Payment Method" required="true">
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" />
               <radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Debit Card" value="dc" />
             </form-group>
```

### SelectTagHelper

- **Namespace:** `SmartWorkz.Web.SelectTagHelper`
- **Summary:** TagHelper for rendering HTML select elements with support for list items, enum binding, blank option handling, and Bootstrap styling.
             Targets the <select-tag> element and generates <select class="form-control"> with options.
- **Example:**
```csharp
<!-- Simple select with SelectListItem collection -->
             <select-tag for="User.CountryId" items="@countries" />
            
             <!-- Select with enum binding -->
             <select-tag for="Order.Status" enum-type="typeof(OrderStatus)" />
            
             <!-- Select without blank option -->
             <select-tag for="Product.Category" items="@categories" add-blank="false" />
            
             <!-- Select with custom blank text -->
             <select-tag for="User.Department" items="@departments" blank-text="Choose a department..." />
            
             <!-- With initial selection -->
             @{ var selected = categories.First(c => c.Value == "electronics"); selected.Selected = true; }
             <select-tag for="Product.Category" items="@categories" />
            
             <!-- In form-group wrapper -->
             <form-group for="User.CountryId" label="Country" required="true" help-text="Select your country">
               <select-tag for="User.CountryId" items="@countries" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SelectTagHelper class.
  - Parameters:
    - `formComponentProvider`: Provider for form component styling configuration.
- **Process** - Processes the select-tag element and renders an HTML select element with options.
  - Parameters:
    - `context`: The TagHelperContext.
    - `output`: The TagHelperOutput.
- **GetSelectItems** - Gets the select items from either the Items property or EnumType property.
            If both Items and EnumType are provided, Items takes precedence.
  - Returns: A list of SelectListItem objects to render as options.

### TextAreaTagHelper

- **Namespace:** `SmartWorkz.Web.TextAreaTagHelper`
- **Summary:** TagHelper for rendering HTML textarea elements with Bootstrap styling, row configuration, and validation support.
             Targets the <textarea-tag> element and generates <textarea class="form-control">.
- **Example:**
```csharp
<!-- Simple textarea with default 3 rows -->
             <textarea-tag for="Model.Comments" placeholder="Enter your comments..." />
            
             <!-- Textarea with custom height -->
             <textarea-tag for="Model.Description" rows="6" placeholder="Enter detailed description" />
            
             <!-- Required textarea -->
             <textarea-tag for="Model.Feedback" placeholder="Your feedback is important" required="true" />
            
             <!-- Large textarea for longer content -->
             <textarea-tag for="Model.BioOrNotes" rows="10" placeholder="Tell us about yourself..." />
            
             <!-- In form-group wrapper for complete form control -->
             <form-group for="Model.Comments" label="Comments" required="true" help-text="Please provide at least 10 characters">
               <textarea-tag for="Model.Comments" rows="5" placeholder="Share your thoughts..." />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new TextAreaTagHelper with dependency injection.
  - Parameters:
    - `formComponentProvider`: Service for form component styling and configuration.

### ValidationMessageTagHelper

- **Namespace:** `SmartWorkz.Web.ValidationMessageTagHelper`
- **Summary:** TagHelper for rendering HTML validation error messages with Bootstrap styling.
             Targets the <validation-message> element and generates <div class="invalid-feedback">.
- **Example:**
```csharp
<!-- Validation message for a field (after input element) -->
             <input-tag for="User.Email" type="email" />
             <validation-message for="User.Email" />
            
             <!-- Validation message with custom message -->
             <input-tag for="User.Age" type="number" />
             <validation-message for="User.Age" message="Age must be between 18 and 100" />
            
             <!-- Typical usage in form-group -->
             <form-group for="User.Email" label="Email" required="true">
               <input-tag for="User.Email" type="email" />
               <validation-message for="User.Email" />
             </form-group>
            
             <!-- Multiple fields with validation -->
             <form-group for="User.Password" label="Password" required="true">
               <input-tag for="User.Password" type="password" />
               <validation-message for="User.Password" />
             </form-group>
             <form-group for="User.ConfirmPassword" label="Confirm Password" required="true">
               <input-tag for="User.ConfirmPassword" type="password" />
               <validation-message for="User.ConfirmPassword" />
             </form-group>
```

#### Methods & Properties

- **#ctor** - Creates a new ValidationMessageTagHelper with dependency injection.
  - Parameters:
    - `accessibilityService`: Service for generating accessible error message IDs and ARIA support.

### GridTagHelper

- **Namespace:** `SmartWorkz.Web.GridTagHelper`
- **Summary:** TagHelper for rendering data grids with sorting, filtering, row selection, and pagination support.
             Targets the <grid> element and provides a high-level API for binding to IDataContext<T>
             and rendering grid data with Bootstrap table styling.
- **Example:**
```csharp
<!-- Basic grid with data binding -->
             <grid data-source="@Model.Products" data-page-size="20">
               <grid-column property="Name" sortable="true">Product Name</grid-column>
               <grid-column property="Price" sortable="true" format="currency">Price</grid-column>
             </grid>
            
             <!-- Grid with row selection enabled -->
             <grid data-source="@Model.Orders" data-page-size="50" data-allow-selection="true">
               <grid-column property="OrderId">Order ID</grid-column>
               <grid-column property="OrderDate" sortable="true" format="date">Date</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
             </grid>
            
             <!-- Grid with custom CSS class and export option -->
             <grid data-source="@Model.Customers"
                   data-page-size="25"
                   data-allow-export="true"
                   data-allow-column-toggle="true"
                   data-css-class="compact-grid">
               <grid-column property="FirstName">First Name</grid-column>
               <grid-column property="Email" sortable="true">Email</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
            
             <!-- Complete example with IDataContext<Product> binding -->
             @{
               var productDataContext = new DataContext<Product>(productService);
               await productDataContext.Initialize(await productService.GetProductsAsync());
             }
             <grid data-source="@productDataContext.Items"
                   data-page-size="20"
                   data-allow-selection="true">
               <grid-column property="Name" sortable="true">Product</grid-column>
               <grid-column property="Category" filterable="true">Category</grid-column>
               <grid-column property="Price" format="currency" css-class="text-end">Price</grid-column>
               <grid-column property="Stock" sortable="true">Stock</grid-column>
             </grid>
            
             <!-- Grid with filtering and sorting -->
             <div class="mb-3">
               <label for="statusFilter">Filter by Status:</label>
               <select id="statusFilter" onchange="updateFilter(this.value)">
                 <option value="">All</option>
                 <option value="Active">Active</option>
                 <option value="Inactive">Inactive</option>
               </select>
             </div>
            
             <grid data-source="@Model.Items" data-page-size="20">
               <grid-column property="Name" sortable="true">Name</grid-column>
               <grid-column property="Status" filterable="true">Status</grid-column>
               <grid-column property="CreatedDate" sortable="true" format="date">Created</grid-column>
             </grid>
```

### BreadcrumbItem

- **Namespace:** `SmartWorkz.Web.BreadcrumbItem`
- **Summary:** Data model representing a single item in a breadcrumb navigation trail.
             Used with BreadcrumbTagHelper to construct hierarchical navigation paths.
- **Example:**
```csharp
<!-- Create breadcrumb items for product details page -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Home", Url = "/" },
                 new() { Label = "Products", Url = "/products" },
                 new() { Label = "Electronics", Url = "/products/electronics" },
                 new() { Label = "Laptops", Url = "/products/electronics/laptops" },
                 new() { Label = "Dell XPS 13" }  // No URL - this is the current page
               };
             }
             <breadcrumb items="breadcrumbs" />
            
             <!-- Create from dynamic data -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" }
               };
            
               foreach (var folder in Model.FolderHierarchy)
               {
                 breadcrumbs.Add(new()
                 {
                   Label = folder.Name,
                   Url = folder.Depth < Model.FolderHierarchy.Count - 1 ? folder.Url : null
                 });
               }
             }
             <breadcrumb items="breadcrumbs" />
```

### BreadcrumbTagHelper

- **Namespace:** `SmartWorkz.Web.BreadcrumbTagHelper`
- **Summary:** TagHelper for rendering Bootstrap breadcrumb navigation showing the user's current location in site hierarchy.
             Targets the <breadcrumb> element and generates <nav><ol class="breadcrumb"> navigation.
- **Example:**
```csharp
<!-- Basic breadcrumb navigation -->
             <breadcrumb items="new List<BreadcrumbItem> {
               new() { Label = "Home", Url = "/" },
               new() { Label = "Products", Url = "/products" },
               new() { Label = "Electronics" }
             }" />
            
             <!-- Breadcrumb from controller action -->
             <breadcrumb items="Model.Breadcrumbs" />
            
             <!-- Programmatically constructed breadcrumb in view -->
             @{
               var breadcrumbs = new List<BreadcrumbItem>
               {
                 new() { Label = "Dashboard", Url = "/dashboard" },
                 new() { Label = "Reports", Url = "/reports" },
                 new() { Label = "Monthly Summary" }
               };
             }
             <breadcrumb items="breadcrumbs" />
```

### IfAuthorizedTagHelper

- **Namespace:** `SmartWorkz.Web.IfAuthorizedTagHelper`
- **Summary:** TagHelper for rendering content only if the user is authenticated.
             Targets the <if-authorized> element and conditionally renders its content based on authentication status.
- **Example:**
```csharp
<!-- Simple logout button, shown only if authenticated -->
             <if-authorized>
               <button class="btn btn-danger">Logout</button>
             </if-authorized>
            
             <!-- User profile section in header -->
             <if-authorized>
               <div class="user-profile">
                 <span>Welcome, @Model.User.Name!</span>
                 <a href="/profile">View Profile</a>
               </div>
             </if-authorized>
            
             <!-- Multiple buttons and links for authenticated users -->
             <if-authorized>
               <div class="authenticated-menu">
                 <a href="/dashboard" class="btn btn-primary">Dashboard</a>
                 <a href="/orders" class="btn btn-info">My Orders</a>
                 <button onclick="logout()" class="btn btn-danger">Logout</button>
               </div>
             </if-authorized>
            
             <!-- Unauthenticated content shown separately -->
             <div>
               <if-authorized>
                 <p>You are logged in.</p>
               </if-authorized>
               <!-- Note: For not-authenticated content, use negation in IfAuthorizedTagHelper
                    or implement a separate IfNotAuthorizedTagHelper -->
             </div>
```

### IfClaimTagHelper

- **Namespace:** `SmartWorkz.Web.IfClaimTagHelper`
- **Summary:** TagHelper for rendering content only if the user has a specific claim with a matching value.
             Targets the <if-claim> element and conditionally renders content based on claim verification.
- **Example:**
```csharp
<!-- Simple claim check: show if user has "role" claim with value "Admin" -->
             <if-claim type="role" value="Admin">
               <button class="btn btn-danger">Delete User</button>
             </if-claim>
            
             <!-- Multiple values (OR logic): show if user is Admin or Manager -->
             <if-claim type="role" value="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-claim>
            
             <!-- Department-based access: show if in IT or Finance department -->
             <if-claim type="department" value="IT,Finance">
               <div class="reports-section">
                 <h3>Financial Reports</h3>
                 <a href="/reports/finance">View Reports</a>
               </div>
             </if-claim>
            
             <!-- Subscription level: show premium features if subscription is Premium or Enterprise -->
             <if-claim type="subscription_level" value="Premium,Enterprise">
               <div class="premium-features">
                 <h3>Advanced Analytics</h3>
                 <p>You have access to advanced reporting features.</p>
               </div>
             </if-claim>
            
             <!-- Claim existence without value check: show if user has ANY custom-permission claim -->
             <if-claim type="custom-permission">
               <p>You have special permissions.</p>
             </if-claim>
            
             <!-- Combined with other markup: admin toolbar with multiple restrictions -->
             <div class="admin-toolbar">
               <if-claim type="role" value="Admin">
                 <button class="btn btn-danger" onclick="deleteAll()">Delete All</button>
               </if-claim>
               <if-claim type="role" value="Admin,Moderator">
                 <button class="btn btn-warning" onclick="moderate()">Moderate</button>
               </if-claim>
             </div>
```

### IfRoleTagHelper

- **Namespace:** `SmartWorkz.Web.IfRoleTagHelper`
- **Summary:** TagHelper for rendering content only if the user belongs to a specific role.
             Targets the <if-role> element and conditionally renders content based on role membership.
- **Example:**
```csharp
<!-- Simple role check: show delete button if user is Admin -->
             <if-role role="Admin">
               <button class="btn btn-danger" onclick="deleteItem()">Delete</button>
             </if-role>
            
             <!-- Multiple roles (OR logic): show if user is Admin or Manager -->
             <if-role role="Admin,Manager">
               <a href="/admin" class="btn btn-primary">Admin Panel</a>
             </if-role>
            
             <!-- Editor dashboard with edit/delete options -->
             <if-role role="Editor">
               <div class="editor-toolbar">
                 <button class="btn btn-info" onclick="editItem()">Edit</button>
                 <button class="btn btn-warning" onclick="publishItem()">Publish</button>
               </div>
             </if-role>
            
             <!-- Multiple role sections with different features -->
             <div class="dashboard">
               <if-role role="Admin">
                 <div class="admin-section">
                   <h3>System Administration</h3>
                   <a href="/admin/users">Manage Users</a>
                   <a href="/admin/settings">System Settings</a>
                 </div>
               </if-role>
               <if-role role="Manager,Editor">
                 <div class="content-section">
                   <h3>Content Management</h3>
                   <a href="/content">View Content</a>
                 </div>
               </if-role>
             </div>
            
             <!-- Nested role checks in a complex menu -->
             <nav class="sidebar">
               <ul>
                 <li><a href="/home">Home</a></li>
                 <if-role role="User,Admin,Manager">
                   <li><a href="/dashboard">Dashboard</a></li>
                   <li><a href="/profile">My Profile</a></li>
                 </if-role>
                 <if-role role="Admin">
                   <li><a href="/admin">Administration</a></li>
                 </if-role>
               </ul>
             </nav>
            
             <!-- Contributor role with limited permissions -->
             <if-role role="Contributor,Editor,Admin">
               <div class="publish-section">
                 <button class="btn btn-success" onclick="submit()">Submit for Review</button>
               </div>
             </if-role>
```

