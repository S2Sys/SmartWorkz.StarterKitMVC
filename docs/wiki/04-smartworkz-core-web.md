# SmartWorkz.Core.Web — Web Components & Services

## Assembly Reference

**ProjectReference:** `src/SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj`  
**Namespace:** `SmartWorkz.Core.Web`  
**Target Framework:** .NET 9 (Razor SDK)  
**Key Dependencies:** HotChocolate v14, JWT parsing, Bootstrap 5

---

## Tag Helpers (XML Documented)

### FormGroupTagHelper — Bootstrap Form Fields

Renders form group with label, input, error messages. Auto-applies validation classes.

**Attributes:**  
`label` — Label text  
`asp-for` — Model expression  
`css-class` — Additional CSS classes

Features: Error message display, `is-invalid` class on validation failure

### StatusBadgeTagHelper — Status Indicators

Bootstrap badge colored by status. Optional icon.

**Statuses:** active (green ✓), inactive (gray ○), pending (warning ⏳), error (red ✗), processing (info ⟳)

---

## Blazor Components (XML Documented)

### BaseRazorComponent — Component Base

Abstract base with parameter validation hook.

Method: OnParameterValidation() — override to validate parameters

### GridComponent — Data Grid

Displays and interacts with tabular data. Sorting, filtering, virtual scroll, pagination.

**Parameters:**  
Data (IQueryable), Columns (GridColumn[]), Options (GridOptions), OnRowSelected

**Public methods:** SortByColumn, FilterData, GetPagedData, PreviousPage, NextPage

**GridOptions:** PageSize, AllowSorting, AllowFiltering, VirtualizationEnabled, AlternateRowColors

---

## Validation Services (XML Documented)

### IValidationService

ValidateModel<T> → Dictionary<string, List<string>>  
ValidateProperty<T> → bool, out errors

### ValidationExtensions Helper Methods

GetValidationErrorsSummary, HasErrors, GetPropertyErrors, GetFirstPropertyError

---

## GraphQL Setup (XML Documented)

### AddGraphQLServices Extension

Registers HotChocolate GraphQL with query type, built-in types (User, Product, etc.), error filter, introspection control.

**Usage in Program.cs:**

```csharp
builder.Services.AddGraphQLServices(configuration);
app.MapGraphQL();  // Endpoint at /graphql
```

### GraphQLAuthenticationMiddleware — JWT Validation

ExtractBearerToken(HttpContext) → string  
ValidateToken(token, issuer, audience) → ClaimsPrincipal

---

## SortOrder Enum

None, Ascending, Descending — used by GridComponent

---

**Next:** [05-smartworkz-core-shared.md](./05-smartworkz-core-shared.md)
