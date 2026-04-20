# SmartWorkz TagHelpers & UI Components Guide

A complete reference for all 15 HTML TagHelpers plus 4 UI component services in `SmartWorkz.Core.Web`.

---

## Overview

SmartWorkz provides a comprehensive set of **custom HTML TagHelpers** that extend ASP.NET Core Razor Pages with Bootstrap 5.3.3-integrated form controls, displays, and navigation elements. Every TagHelper is **fully accessible** (WCAG 2.1 AA) with ARIA attributes and semantic HTML.

### Quick Start: DI Setup

First, register the services in `Program.cs`:

```csharp
builder.Services.AddSmartWorkzCoreWeb();
```

This single extension method registers:
- `IIconProvider` → `IconProvider` (25 Bootstrap icon types)
- `IValidationMessageProvider` → `ValidationMessageProvider` (14 built-in error messages)
- `IFormComponentProvider` → `FormComponentProvider` (runtime CSS theming)
- `IAccessibilityService` → `AccessibilityService` (ARIA ID generation)

---

## Form TagHelpers

All form TagHelpers use Bootstrap 5.3.3 CSS classes and integrate with ASP.NET Core model binding.

### 1. `<form-tag>` — Bootstrap Form Wrapper

Renders an HTML `<form>` with Bootstrap's `needs-validation` class for client-side validation styling.

**Attributes:**
- `method` (optional) — HTTP method: `"get"` | `"post"` (default: `"post"`)
- `action` (optional) — Form action URL (default: current page)
- `novalidate` (optional) — Disable browser validation (default: `false`)

**Example:**

```html
<form-tag method="post" action="/products/save">
    <!-- form controls go here -->
</form-tag>
```

**Rendered HTML:**

```html
<form method="post" action="/products/save" class="needs-validation" novalidate>
    <!-- ... -->
</form>
```

---

### 2. `<form-group>` — Field Wrapper with Label & Help Text

Wraps a form control in a `<div class="mb-3">` Bootstrap form group. Automatically generates accessible label and help text.

**Attributes:**
- `for` (required) — Model property (e.g., `Model.Name`)
- `label` (optional) — Label text to display
- `required` (optional) — Marks field as required with red asterisk (default: `false`)
- `help-text` (optional) — Small help text below the control

**Example:**

```html
<form-group for="Model.Email" label="Email Address" required="true" help-text="We'll never share your email.">
    <input-tag for="Email" type="email" />
</form-group>
```

**Rendered HTML:**

```html
<div class="mb-3">
    <label for="field_email" class="form-label">Email Address<span class="text-danger">*</span></label>
    <input type="email" id="field_email" class="form-control" />
    <small id="hint_email" class="form-text text-muted">We'll never share your email.</small>
</div>
```

---

### 3. `<input-tag>` — Bootstrap Text Input with Optional Icons

Renders a Bootstrap `<input>` element with optional prefix/suffix icons.

**Attributes:**
- `for` (optional) — Field name for ID generation
- `type` (optional) — HTML input type: `"text"`, `"email"`, `"password"`, `"number"`, `"date"` (default: `"text"`)
- `placeholder` (optional) — Placeholder text
- `required` (optional) — Required attribute (default: `false`)
- `icon-prefix` (optional) — `IconType` to display before the input (wraps in `<input-group>`)
- `icon-suffix` (optional) — `IconType` to display after the input (wraps in `<input-group>`)

**Example:**

```html
<input-tag for="SearchQuery" type="text" placeholder="Search..." icon-prefix="Search" />
<input-tag for="Password" type="password" placeholder="Enter password" icon-suffix="Lock" />
```

**Rendered HTML:**

```html
<div class="input-group">
    <span class="input-group-text"><i class="bi bi-search"></i></span>
    <input type="text" id="field_searchquery" class="form-control" placeholder="Search..." />
</div>
```

---

### 4. `<label-tag>` — Accessible Label

Renders a Bootstrap label with automatic ARIA ID generation and optional required indicator.

**Attributes:**
- `for` (required) — Field name
- `text` (optional) — Label text (alternative: use inner content)

**Example:**

```html
<label-tag for="ProductName">Product Name</label-tag>
```

**Rendered HTML:**

```html
<label for="field_productname" class="form-label">Product Name</label>
```

---

### 5. `<select-tag>` — Bootstrap Dropdown Select

Renders a Bootstrap `<select>` from a list of items or enum values.

**Attributes:**
- `for` (required) — Field name
- `items` (optional) — `IEnumerable<SelectListItem>` or `IEnumerable<string>`
- `enum-type` (optional) — `Type` for enum binding (e.g., `typeof(OrderStatus)`)
- `blank-option` (optional) — Text for blank option (default: none)
- `required` (optional) — Required attribute

**Example with List:**

```html
@{
    var statusOptions = new List<SelectListItem>
    {
        new SelectListItem("Active", "active"),
        new SelectListItem("Inactive", "inactive")
    };
}

<select-tag for="Status" items="@statusOptions" />
```

**Example with Enum:**

```html
<select-tag for="Priority" enum-type="typeof(OrderPriority)" blank-option="-- Select --" />
```

**Rendered HTML:**

```html
<select id="field_status" class="form-select">
    <option value="active">Active</option>
    <option value="inactive">Inactive</option>
</select>
```

---

### 6. `<checkbox-tag>` — Bootstrap Checkbox

Renders a Bootstrap form-check checkbox with optional label.

**Attributes:**
- `for` (required) — Field name
- `label` (optional) — Checkbox label text
- `checked` (optional) — Pre-checked (default: `false`)

**Example:**

```html
<checkbox-tag for="IsActive" label="Mark as active" checked="true" />
```

**Rendered HTML:**

```html
<div class="form-check">
    <input type="checkbox" id="field_isactive" class="form-check-input" checked />
    <label for="field_isactive" class="form-check-label">Mark as active</label>
</div>
```

---

### 7. `<radio-button-tag>` — Bootstrap Radio Button

Renders a Bootstrap form-check radio button in a named group.

**Attributes:**
- `for` (required) — Field name / group name
- `value` (required) — Option value
- `label` (optional) — Radio label text
- `checked` (optional) — Pre-checked (default: `false`)

**Example:**

```html
<radio-button-tag for="Shipping" value="standard" label="Standard (5-7 days)" />
<radio-button-tag for="Shipping" value="express" label="Express (2-3 days)" />
<radio-button-tag for="Shipping" value="overnight" label="Overnight" />
```

**Rendered HTML:**

```html
<div class="form-check">
    <input type="radio" name="shipping" id="field_shipping" value="standard" class="form-check-input" />
    <label for="field_shipping" class="form-check-label">Standard (5-7 days)</label>
</div>
```

---

### 8. `<textarea-tag>` — Bootstrap Textarea

Renders a Bootstrap `<textarea>` with optional rows.

**Attributes:**
- `for` (required) — Field name
- `placeholder` (optional) — Placeholder text
- `rows` (optional) — Number of rows (default: `3`)
- `required` (optional) — Required attribute

**Example:**

```html
<textarea-tag for="Description" placeholder="Enter product description..." rows="5" />
```

**Rendered HTML:**

```html
<textarea id="field_description" class="form-control" rows="5" placeholder="Enter product description..."></textarea>
```

---

### 9. `<validation-message>` — Bootstrap Error Message

Renders an invalid-feedback div with ARIA attributes for displaying validation errors.

**Attributes:**
- `for` (required) — Field name
- `message` (optional) — Pre-filled error message (or resolved from validation result)

**Example:**

```html
<validation-message for="Email" />
```

**Rendered HTML:**

```html
<div id="error_email" class="invalid-feedback">
    Email address is required.
</div>
```

---

## Display TagHelpers

These render read-only UI elements (alerts, badges, pagination, breadcrumbs).

### 10. `<alert>` — Bootstrap Alert Box

Renders a dismissible Bootstrap alert with an icon.

**Attributes:**
- `type` — Alert type: `"success"` | `"danger"` | `"warning"` | `"info"` (required)
- `message` — Alert message text (required)
- `title` (optional) — Alert title (rendered as `<strong>`)
- `dismissible` (optional) — Show close button (default: `true`)

**Example:**

```html
<alert type="success" title="Success!" message="Your changes have been saved." />
<alert type="danger" message="An error occurred. Please try again." />
```

**Rendered HTML:**

```html
<div class="alert alert-success alert-dismissible fade show" role="alert">
    <i class="bi bi-check-circle-fill"></i>
    <strong>Success!</strong> Your changes have been saved.
    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
</div>
```

---

### 11. `<badge>` — Bootstrap Badge

Renders a Bootstrap badge (small label tag).

**Attributes:**
- `type` — Badge type: `"primary"` | `"secondary"` | `"success"` | `"danger"` | `"warning"` | `"info"` | `"light"` | `"dark"` (required)
- `text` — Badge text (required)

**Example:**

```html
<badge type="primary" text="New" />
<badge type="success" text="Active" />
<badge type="danger" text="Urgent" />
```

**Rendered HTML:**

```html
<span class="badge bg-primary">New</span>
<span class="badge bg-success">Active</span>
<span class="badge bg-danger">Urgent</span>
```

---

### 12. `<pagination>` — Bootstrap Pagination

Renders a Bootstrap pagination nav with page links.

**Attributes:**
- `total-pages` — Total number of pages (required)
- `current-page` — Currently active page (required)
- `on-page-change` (optional) — JavaScript function to call on page click

**Example:**

```html
<pagination total-pages="10" current-page="3" />
```

**Rendered HTML:**

```html
<nav aria-label="Page navigation">
    <ul class="pagination">
        <li class="page-item"><a class="page-link" href="?page=2">Previous</a></li>
        <li class="page-item"><a class="page-link" href="?page=1">1</a></li>
        <li class="page-item"><a class="page-link" href="?page=2">2</a></li>
        <li class="page-item active"><span class="page-link">3</span></li>
        <li class="page-item"><a class="page-link" href="?page=4">4</a></li>
        <li class="page-item"><a class="page-link" href="?page=4">Next</a></li>
    </ul>
</nav>
```

---

### 13. `<breadcrumb>` — Bootstrap Breadcrumb Navigation

Renders a Bootstrap breadcrumb trail (semantic `<nav>` with `<ol>`).

**Attributes:**
- `items` — `IEnumerable<BreadcrumbItem>` with `Label` (string) and `Url` (nullable string) (required)

**Example:**

```csharp
// In page model
var breadcrumbs = new List<BreadcrumbItem>
{
    new BreadcrumbItem { Label = "Home", Url = "/" },
    new BreadcrumbItem { Label = "Products", Url = "/products" },
    new BreadcrumbItem { Label = "Widget Pro", Url = null } // current page, no link
};
```

```html
<breadcrumb items="@breadcrumbs" />
```

**Rendered HTML:**

```html
<nav aria-label="breadcrumb">
    <ol class="breadcrumb">
        <li class="breadcrumb-item"><a href="/">Home</a></li>
        <li class="breadcrumb-item"><a href="/products">Products</a></li>
        <li class="breadcrumb-item active" aria-current="page">Widget Pro</li>
    </ol>
</nav>
```

---

## Common TagHelpers

### 14. `<button>` — Bootstrap Button

Renders a Bootstrap button with variant styling and size.

**Attributes:**
- `variant` — Button color: `"primary"` | `"secondary"` | `"success"` | `"danger"` | `"warning"` | `"info"` | `"light"` | `"dark"` (default: `"primary"`)
- `size` (optional) — Button size: `"sm"` | `"lg"` (default: normal)
- `type` (optional) — Button type: `"button"` | `"submit"` | `"reset"` (default: `"button"`)
- `disabled` (optional) — Disabled state

**Example:**

```html
<button variant="primary">Save</button>
<button variant="danger" type="submit">Delete</button>
<button variant="secondary" size="sm">Cancel</button>
```

**Rendered HTML:**

```html
<button class="btn btn-primary">Save</button>
<button type="submit" class="btn btn-danger">Delete</button>
<button class="btn btn-secondary btn-sm">Cancel</button>
```

---

### 15. `<icon>` — Bootstrap Icon

Renders a Bootstrap icon (`<i>` element) from the `IconType` enum.

**Attributes:**
- `name` — Icon name (as `IconType` enum value): `"Success"`, `"Error"`, `"Warning"`, `"Info"`, `"Search"`, `"Menu"`, `"Close"`, `"User"`, `"Settings"`, `"Home"`, `"Logout"`, `"Plus"`, `"Minus"`, `"Edit"`, `"Delete"`, `"Save"`, and more (required)
- `size` (optional) — Icon size class: `"sm"` | `"lg"` (applies Bootstrap font-size utility)
- `class` (optional) — Additional CSS classes

**Example:**

```html
<icon name="Search" />
<icon name="Edit" size="lg" class="text-primary" />
<icon name="Delete" class="text-danger" />
```

**Rendered HTML:**

```html
<i class="bi bi-search"></i>
<i class="bi bi-pencil-square fs-5 text-primary"></i>
<i class="bi bi-trash text-danger"></i>
```

---

## UI Component Services

These services power the TagHelpers and provide runtime configuration.

### IIconProvider — 25 Bootstrap Icons

All 25 available icon types:

| Icon Type | Bootstrap Class | Use Case |
|-----------|-----------------|----------|
| **Status Icons** |
| `Success` | `bi-check-circle-fill` | Success messages, completed tasks |
| `Error` | `bi-exclamation-triangle-fill` | Errors, warnings |
| `Warning` | `bi-exclamation-triangle` | Warnings, caution |
| `Info` | `bi-info-circle` | Information, help |
| `CheckCircle` | `bi-check-circle` | Confirmed, valid |
| `ExclamationTriangle` | `bi-exclamation-triangle` | Alert, warning |
| `ExclamationCircle` | `bi-exclamation-circle` | Error, critical |
| `InformationCircle` | `bi-info-circle-fill` | Important info |
| **Navigation Icons** |
| `ChevronLeft` | `bi-chevron-left` | Previous, back |
| `ChevronRight` | `bi-chevron-right` | Next, forward |
| `ChevronUp` | `bi-chevron-up` | Collapse, scroll up |
| `ChevronDown` | `bi-chevron-down` | Expand, scroll down |
| `User` | `bi-person-circle` | User profile, account |
| `Settings` | `bi-gear` | Settings, configuration |
| `Home` | `bi-house` | Home page |
| `Logout` | `bi-box-arrow-right` | Sign out, logout |
| **Action Icons** |
| `Search` | `bi-search` | Search, filter |
| `Menu` | `bi-list` | Menu, hamburger |
| `Close` | `bi-x-circle-fill` | Close, dismiss, delete |
| **Form Icons** |
| `Plus` | `bi-plus` | Add, create, new |
| `Minus` | `bi-dash` | Remove, delete |
| `Edit` | `bi-pencil-square` | Edit, modify |
| `Delete` | `bi-trash` | Delete, remove |
| `Save` | `bi-floppy` | Save, submit |

**Usage:**

```csharp
// Inject into service
private readonly IIconProvider _iconProvider;

public MyService(IIconProvider iconProvider)
{
    _iconProvider = iconProvider;
}

// Get icon class
var cssClass = _iconProvider.GetIconClass(IconType.Save);        // "bi bi-floppy"

// Get icon HTML
var html = _iconProvider.GetIconHtml(IconType.Delete);           // "<i class=\"bi bi-trash\"></i>"
var htmlWithClass = _iconProvider.GetIconHtml(IconType.Edit, "text-primary"); // with extra class
```

---

### IValidationMessageProvider — 14 Built-In Messages

Pre-configured validation error messages (localized):

| Error Key | Default Message |
|-----------|-----------------|
| `required` | `Field is required.` |
| `email` | `Enter a valid email address.` |
| `minlength` | `Minimum length is {0} characters.` |
| `maxlength` | `Maximum length is {0} characters.` |
| `pattern` | `Format is invalid.` |
| `min` | `Minimum value is {0}.` |
| `max` | `Maximum value is {0}.` |
| `unique` | `This value already exists.` |
| `invalid` | `Value is invalid.` |
| `match` | `Values do not match.` |
| `regex` | `Format is invalid.` |
| `url` | `Enter a valid URL.` |
| `number` | `Enter a valid number.` |
| `date` | `Enter a valid date.` |

**Custom Message Registration:**

```csharp
var validationProvider = serviceProvider.GetRequiredService<IValidationMessageProvider>();

validationProvider.RegisterMessage("custom_rule", "This field failed custom validation.");
```

---

### IFormComponentProvider — Runtime CSS Theming

Override default Bootstrap classes at runtime without changing HTML. Useful for white-label applications.

**Available Properties (all strings):**

```csharp
public class FormComponentConfig
{
    public string FormControlClass { get; set; } = "form-control";
    public string FormSelectClass { get; set; } = "form-select";
    public string FormCheckClass { get; set; } = "form-check";
    public string FormCheckInputClass { get; set; } = "form-check-input";
    public string FormCheckLabelClass { get; set; } = "form-check-label";
    public string InputGroupClass { get; set; } = "input-group";
    public string InputGroupTextClass { get; set; } = "input-group-text";
    public string LabelClass { get; set; } = "form-label";
    public string FormGroupClass { get; set; } = "mb-3";
    public string ButtonPrimaryClass { get; set; } = "btn btn-primary";
    public string ButtonSecondaryClass { get; set; } = "btn btn-secondary";
    public string ButtonDangerClass { get; set; } = "btn btn-danger";
    public string ButtonWarningClass { get; set; } = "btn btn-warning";
    public string ButtonSuccessClass { get; set; } = "btn btn-success";
    public string BadgeClass { get; set; } = "badge";
    public string AlertClass { get; set; } = "alert";
    public string PaginationClass { get; set; } = "pagination";
}
```

**Customization Example:**

```csharp
// At startup, customize for Material Design:
var formProvider = serviceProvider.GetRequiredService<IFormComponentProvider>();
var config = new FormComponentConfig
{
    FormControlClass = "mdc-text-field form-control",
    ButtonPrimaryClass = "mdc-button mdc-button--raised btn-primary",
    // ... customize all classes
};
formProvider.UpdateConfiguration(config);
```

---

### IAccessibilityService — WCAG 2.1 AA Compliance

Generates accessible ARIA IDs automatically.

**Methods:**

```csharp
var a11yService = serviceProvider.GetRequiredService<IAccessibilityService>();

// Generate ID for form field
var fieldId = a11yService.GenerateFieldId("email");       // "field_email"

// Generate ID for error message
var errorId = a11yService.GenerateErrorId("email");       // "error_email"

// Generate ID for help text / hint
var hintId = a11yService.GenerateHintId("email");         // "hint_email"

// Generate ARIA label
var label = a11yService.GenerateAriaLabel("email");       // "email_label"
```

---

## Complete Form Example

Here's a full Razor Page form using all TagHelpers together:

```html
@page "/products/create"
@model ProductCreateModel

<div class="container mt-5">
    <div class="row">
        <div class="col-md-8">
            <h1>Create Product</h1>

            <form-tag method="post" action="/products/save">
                <!-- Product Name -->
                <form-group for="Model.Name" label="Product Name" required="true" help-text="Enter a unique product name">
                    <input-tag for="Name" type="text" placeholder="e.g., Premium Widget" />
                    <validation-message for="Name" />
                </form-group>

                <!-- Category Select -->
                <form-group for="Model.CategoryId" label="Category" required="true">
                    <select-tag for="CategoryId" items="@Model.Categories" blank-option="-- Select --" />
                    <validation-message for="CategoryId" />
                </form-group>

                <!-- Email with Icon -->
                <form-group for="Model.Email" label="Support Email" required="true">
                    <input-tag for="Email" type="email" placeholder="support@example.com" icon-prefix="Search" />
                    <validation-message for="Email" />
                </form-group>

                <!-- Password -->
                <form-group for="Model.Password" label="Password" required="true" help-text="Minimum 8 characters">
                    <input-tag for="Password" type="password" placeholder="••••••••" icon-suffix="Lock" />
                    <validation-message for="Password" />
                </form-group>

                <!-- Description Textarea -->
                <form-group for="Model.Description" label="Description" help-text="Enter a brief product description">
                    <textarea-tag for="Description" rows="4" placeholder="Describe the product..." />
                </form-group>

                <!-- Checkboxes -->
                <form-group label="Options">
                    <checkbox-tag for="IsActive" label="Active (available for sale)" checked="true" />
                    <checkbox-tag for="IsFeatured" label="Feature on homepage" />
                </form-group>

                <!-- Radio Buttons -->
                <form-group label="Shipping Method" required="true">
                    <radio-button-tag for="ShippingMethod" value="standard" label="Standard (Free)" checked="true" />
                    <radio-button-tag for="ShippingMethod" value="express" label="Express ($5)" />
                    <radio-button-tag for="ShippingMethod" value="overnight" label="Overnight ($15)" />
                </form-group>

                <!-- Buttons -->
                <div class="mt-4">
                    <button variant="primary" type="submit">
                        <icon name="Save" /> Save Product
                    </button>
                    <button variant="secondary" onclick="history.back()">
                        <icon name="Close" /> Cancel
                    </button>
                </div>
            </form-tag>

            <!-- Alert Examples -->
            @if (Model.SuccessMessage != null)
            {
                <alert type="success" title="Success!" message="@Model.SuccessMessage" />
            }
        </div>
    </div>
</div>
```

---

## Best Practices

### 1. Always Use `<form-group>` Wrapper

Wrapping inputs in `<form-group>` ensures consistent spacing, accessible labels, and help text support.

```html
<!-- ✓ Good -->
<form-group for="Email" label="Email" required="true">
    <input-tag for="Email" type="email" />
</form-group>

<!-- ✗ Bad -->
<input-tag for="Email" type="email" />
```

### 2. Use Icons Sparingly and Meaningfully

Icons should reinforce the meaning, not clutter the UI.

```html
<!-- ✓ Good: Icon clarifies input purpose -->
<input-tag for="Email" type="email" icon-prefix="Search" placeholder="Search emails..." />

<!-- ✗ Bad: Random icon adds no meaning -->
<input-tag for="Phone" type="tel" icon-prefix="Home" />
```

### 3. Provide Help Text for Complex Fields

```html
<form-group for="TaxId" label="Tax ID" help-text="Format: XX-XXXXXXXXX">
    <input-tag for="TaxId" type="text" placeholder="XX-XXXXXXXXX" />
</form-group>
```

### 4. Mark Required Fields Clearly

```html
<form-group for="Email" label="Email" required="true">
    <input-tag for="Email" type="email" required="true" />
</form-group>
```

---

## Troubleshooting

### `InvalidOperationException: No service for type 'IIconProvider'`

**Solution:** Ensure `AddSmartWorkzCoreWeb()` is called in `Program.cs`.

```csharp
builder.Services.AddSmartWorkzCoreWeb();
```

### TagHelper not rendering

**Solution:** Check that the TagHelper assembly is imported in `_ViewImports.cshtml`:

```csharp
@using SmartWorkz.Core.Web
@addTagHelper *, SmartWorkz.Core.Web
```

### Icons not displaying

**Solution:** Ensure Bootstrap Icons CSS is included in the layout:

```html
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet">
```

---

## See Also

- [SmartWorkz.Core Developer Guide](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md) — Full SmartWorkz infrastructure overview
- [Validation Complete Guide](VALIDATION_GUIDE.md) — Input validation patterns
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.3/)
