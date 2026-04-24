# Blazor Form Builder

## Overview

The Blazor Form Builder (`FormBuilderComponent`) is a dynamic form generation and validation system that converts structured form definitions into fully functional, interactive forms with client-side validation, conditional visibility, and submission handling. Use Form Builder when you need user-defined forms without hardcoding HTML, want to manage complex validation rules dynamically, or need to show/hide fields based on user selections.

---

## Architecture

### Component Hierarchy

```
FormBuilderComponent
├── EditForm (Blazor)
│   ├── DataAnnotationsValidator
│   └── FormFieldComponent (one per field)
│       ├── Input (based on field type)
│       └── ValidationMessage
└── Form Actions
    ├── Submit Button
    ├── Reset Button (optional)
    └── Cancel Button (optional)
```

### Model Tree

```
FormDefinition
├── Id: string
├── Title: string
├── Description: string
├── Fields: List<FormField>
│   ├── Name: string
│   ├── Label: string
│   ├── Type: string (field type)
│   ├── Value: object
│   ├── Placeholder: string
│   ├── HelpText: string
│   ├── IsRequired: bool
│   ├── IsDisabled: bool
│   ├── IsVisible: bool (computed)
│   ├── DependsOn: string (field name for visibility)
│   ├── DependsOnValue: object (value that triggers visibility)
│   ├── CssClass: string
│   ├── Order: int
│   ├── Options: List<FormFieldOption>
│   └── ValidationRules: List<FormValidationRule>
│       ├── Type: string (validation type)
│       ├── Value: string (rule parameter)
│       ├── Message: string
│       └── CustomFunction: string
├── SubmitConfig: FormSubmitConfig
│   ├── SubmitButtonText: string
│   ├── CancelButtonText: string
│   ├── ShowCancelButton: bool
│   ├── ShowResetButton: bool
│   ├── SubmitButtonClass: string
│   ├── CancelButtonClass: string
│   └── ResetButtonClass: string
├── IsDisabled: bool
└── CssClass: string
```

### Field Types

| Type | Description | Use Case | Input Element |
|------|-------------|----------|---------------|
| text | Single-line text input | Names, addresses, generic text | `<input type="text">` |
| email | Email address with validation | User email, contact email | `<input type="email">` |
| password | Masked password input | User passwords, secrets | `<input type="password">` |
| number | Numeric input with constraints | Quantities, ages, prices | `<input type="number">` |
| select | Dropdown list selection | Choices from predefined options | `<select>` |
| checkbox | Boolean toggle | Agreements, preferences, flags | `<input type="checkbox">` |
| textarea | Multi-line text input | Comments, descriptions, long text | `<textarea>` |
| date | Date picker input | Birth dates, event dates, deadlines | `<input type="date">` |

### Validation Rule Types

| Type | Description | Parameters | Example |
|------|-------------|-----------|---------|
| required | Field cannot be empty | None | User must enter value |
| email | Value must be valid email format | None | user@example.com validation |
| minlength | String must be at least N characters | Value = minimum length | "abc" for minlength=3 ✓ |
| maxlength | String cannot exceed N characters | Value = maximum length | "ab" for maxlength=2 ✓ |
| pattern | Value must match regex pattern | Value = regex pattern | Phone: `^\d{3}-\d{3}-\d{4}$` |
| custom | Custom validation function (see note) | CustomFunction = function name | User-defined validation logic |
| min | Numeric value must be >= N | Value = minimum number | 18 for min=18 ✓ |
| max | Numeric value must be <= N | Value = maximum number | 100 for max=100 ✓ |

**Note on Custom Validation:** The FormBuilderComponent declares support for type="custom" validation rules (CustomFunction property exists), but the ValidateField() method does NOT implement a case for "custom" — it falls through to the default case and returns null. This is a known limitation. See [Troubleshooting](#troubleshooting) for extension patterns.

---

## Quick Start

Get a basic form working in 5 minutes:

```razor
@page "/contact-form"
@using SmartWorkz.Web.Models
@inject NavigationManager Nav

<FormBuilderComponent FormDef="ContactForm" OnSubmit="HandleSubmit" />

@code {
    private FormDefinition ContactForm = new()
    {
        Title = "Contact Us",
        Fields = new()
        {
            new FormField { Name = "name", Label = "Your Name", Type = "text", IsRequired = true, Order = 1 },
            new FormField { Name = "email", Label = "Email", Type = "email", IsRequired = true, Order = 2, 
                ValidationRules = new() { new FormValidationRule { Type = "email", Message = "Invalid email" } } },
            new FormField { Name = "message", Label = "Message", Type = "textarea", IsRequired = true, Order = 3 }
        },
        SubmitConfig = new() { SubmitButtonText = "Send" }
    };

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            var name = result.Data["name"];
            var email = result.Data["email"];
            var message = result.Data["message"];
            // Process form data
        }
    }
}
```

---

## Configuration

### Supported Field Types and Options

**Text Field:**
```csharp
new FormField
{
    Name = "firstName",
    Label = "First Name",
    Type = "text",
    Placeholder = "Enter your first name",
    IsRequired = true,
    CssClass = "form-control"
}
```

**Email Field:**
```csharp
new FormField
{
    Name = "email",
    Label = "Email Address",
    Type = "email",
    Placeholder = "user@example.com",
    IsRequired = true,
    ValidationRules = new()
    {
        new FormValidationRule { Type = "email", Message = "Please enter a valid email" }
    }
}
```

**Password Field:**
```csharp
new FormField
{
    Name = "password",
    Label = "Password",
    Type = "password",
    IsRequired = true,
    ValidationRules = new()
    {
        new FormValidationRule { Type = "minlength", Value = "8", Message = "Password must be at least 8 characters" }
    }
}
```

**Number Field:**
```csharp
new FormField
{
    Name = "quantity",
    Label = "Quantity",
    Type = "number",
    Value = 1,
    ValidationRules = new()
    {
        new FormValidationRule { Type = "min", Value = "1", Message = "Quantity must be at least 1" },
        new FormValidationRule { Type = "max", Value = "999", Message = "Quantity cannot exceed 999" }
    }
}
```

**Select/Dropdown Field:**
```csharp
new FormField
{
    Name = "country",
    Label = "Country",
    Type = "select",
    IsRequired = true,
    Options = new()
    {
        new FormFieldOption { Label = "United States", Value = "US" },
        new FormFieldOption { Label = "Canada", Value = "CA" },
        new FormFieldOption { Label = "Mexico", Value = "MX" }
    }
}
```

**Checkbox Field:**
```csharp
new FormField
{
    Name = "agreeToTerms",
    Label = "I agree to the terms and conditions",
    Type = "checkbox",
    Value = false,
    IsRequired = true
}
```

**Textarea Field:**
```csharp
new FormField
{
    Name = "comments",
    Label = "Comments",
    Type = "textarea",
    Placeholder = "Enter your feedback here...",
    HelpText = "Maximum 500 characters",
    ValidationRules = new()
    {
        new FormValidationRule { Type = "maxlength", Value = "500", Message = "Comments cannot exceed 500 characters" }
    }
}
```

**Date Field:**
```csharp
new FormField
{
    Name = "birthDate",
    Label = "Date of Birth",
    Type = "date",
    IsRequired = true,
    ValidationRules = new()
    {
        new FormValidationRule { Type = "pattern", Value = @"^\d{4}-\d{2}-\d{2}$", Message = "Invalid date format" }
    }
}
```

### Validation Configuration

All validation rules are applied in the ValidateForm() method. Rules are evaluated in order:

```csharp
var field = new FormField
{
    Name = "username",
    Label = "Username",
    Type = "text",
    IsRequired = true,
    ValidationRules = new()
    {
        new FormValidationRule
        {
            Type = "minlength",
            Value = "3",
            Message = "Username must be at least 3 characters"
        },
        new FormValidationRule
        {
            Type = "maxlength",
            Value = "20",
            Message = "Username cannot exceed 20 characters"
        },
        new FormValidationRule
        {
            Type = "pattern",
            Value = @"^[a-zA-Z0-9_]+$",
            Message = "Username can only contain letters, numbers, and underscores"
        }
    }
};
```

---

## Usage Examples

### Sample 1: Basic Contact Form

```razor
@page "/contact"
@using SmartWorkz.Web.Models

<FormBuilderComponent FormDef="ContactFormDef" OnSubmit="HandleContactSubmit" />

@code {
    private FormDefinition ContactFormDef = new()
    {
        Id = "contact-form",
        Title = "Contact Us",
        Description = "We'd love to hear from you. Please fill out this form.",
        Fields = new()
        {
            new FormField
            {
                Name = "fullName",
                Label = "Full Name",
                Type = "text",
                Placeholder = "John Doe",
                IsRequired = true,
                Order = 1
            },
            new FormField
            {
                Name = "email",
                Label = "Email Address",
                Type = "email",
                Placeholder = "john@example.com",
                IsRequired = true,
                Order = 2,
                ValidationRules = new()
                {
                    new FormValidationRule { Type = "email", Message = "Please enter a valid email address" }
                }
            },
            new FormField
            {
                Name = "phone",
                Label = "Phone Number",
                Type = "text",
                Placeholder = "555-123-4567",
                Order = 3
            },
            new FormField
            {
                Name = "message",
                Label = "Message",
                Type = "textarea",
                Placeholder = "How can we help?",
                IsRequired = true,
                Order = 4,
                ValidationRules = new()
                {
                    new FormValidationRule { Type = "minlength", Value = "10", Message = "Message must be at least 10 characters" }
                }
            }
        },
        SubmitConfig = new()
        {
            SubmitButtonText = "Send Message",
            ShowResetButton = true,
            ShowCancelButton = true
        }
    };

    private async Task HandleContactSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            var name = result.Data["fullName"];
            var email = result.Data["email"];
            var message = result.Data["message"];
            // Send email or save to database
        }
    }
}
```

### Sample 2: Conditional Fields — Business Account Registration

```razor
@page "/register"
@using SmartWorkz.Web.Models

<FormBuilderComponent FormDef="RegistrationForm" OnSubmit="HandleRegistration" />

@code {
    private FormDefinition RegistrationForm = new()
    {
        Title = "Create Account",
        Fields = new()
        {
            new FormField
            {
                Name = "accountType",
                Label = "Account Type",
                Type = "select",
                IsRequired = true,
                Order = 1,
                Options = new()
                {
                    new FormFieldOption { Label = "Personal", Value = "personal" },
                    new FormFieldOption { Label = "Business", Value = "business" }
                },
                Value = "personal"
            },
            new FormField
            {
                Name = "ownerName",
                Label = "Your Name",
                Type = "text",
                IsRequired = true,
                Order = 2,
                Placeholder = "John Doe"
            },
            new FormField
            {
                Name = "companyName",
                Label = "Company Name",
                Type = "text",
                IsRequired = true,
                Order = 3,
                Placeholder = "ACME Corp",
                // Show only when accountType = "business"
                DependsOn = "accountType",
                DependsOnValue = "business"
            },
            new FormField
            {
                Name = "taxId",
                Label = "Tax ID",
                Type = "text",
                IsRequired = true,
                Order = 4,
                Placeholder = "XX-XXXXXXX",
                // Show only when accountType = "business"
                DependsOn = "accountType",
                DependsOnValue = "business",
                ValidationRules = new()
                {
                    new FormValidationRule
                    {
                        Type = "pattern",
                        Value = @"^\d{2}-\d{7}$",
                        Message = "Tax ID must be in format XX-XXXXXXX"
                    }
                }
            },
            new FormField
            {
                Name = "email",
                Label = "Email Address",
                Type = "email",
                IsRequired = true,
                Order = 5,
                ValidationRules = new()
                {
                    new FormValidationRule { Type = "email", Message = "Invalid email address" }
                }
            },
            new FormField
            {
                Name = "agreeToTerms",
                Label = "I agree to the terms and conditions",
                Type = "checkbox",
                IsRequired = true,
                Order = 6
            }
        },
        SubmitConfig = new()
        {
            SubmitButtonText = "Create Account",
            ShowCancelButton = true
        }
    };

    private async Task HandleRegistration(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            var accountType = result.Data["accountType"];
            var name = result.Data["ownerName"];
            var email = result.Data["email"];
            
            if (accountType?.ToString() == "business")
            {
                var company = result.Data["companyName"];
                var taxId = result.Data["taxId"];
                // Save business account
            }
            else
            {
                // Save personal account
            }
        }
    }
}
```

### Sample 3: Comprehensive Validation Example

```razor
@page "/advanced-form"
@using SmartWorkz.Web.Models

<FormBuilderComponent FormDef="ValidationShowcaseForm" OnSubmit="HandleSubmit" />

@code {
    private FormDefinition ValidationShowcaseForm = new()
    {
        Title = "Advanced Validation Showcase",
        Description = "Form demonstrating all 8 validation rule types",
        Fields = new()
        {
            // Required validation (built-in)
            new FormField
            {
                Name = "firstName",
                Label = "First Name",
                Type = "text",
                IsRequired = true,
                Order = 1,
                HelpText = "Required field (IsRequired = true)"
            },

            // Email validation
            new FormField
            {
                Name = "email",
                Label = "Email Address",
                Type = "email",
                IsRequired = true,
                Order = 2,
                ValidationRules = new()
                {
                    new FormValidationRule { Type = "email", Message = "Invalid email format" }
                },
                HelpText = "Validated with email type rule"
            },

            // Min/Max length validation
            new FormField
            {
                Name = "username",
                Label = "Username",
                Type = "text",
                IsRequired = true,
                Order = 3,
                ValidationRules = new()
                {
                    new FormValidationRule { Type = "minlength", Value = "3", Message = "Minimum 3 characters" },
                    new FormValidationRule { Type = "maxlength", Value = "20", Message = "Maximum 20 characters" }
                },
                HelpText = "Validated with minlength and maxlength rules"
            },

            // Pattern validation
            new FormField
            {
                Name = "phone",
                Label = "Phone Number",
                Type = "text",
                IsRequired = true,
                Order = 4,
                Placeholder = "555-123-4567",
                ValidationRules = new()
                {
                    new FormValidationRule
                    {
                        Type = "pattern",
                        Value = @"^\d{3}-\d{3}-\d{4}$",
                        Message = "Phone must match format: XXX-XXX-XXXX"
                    }
                },
                HelpText = "Validated with pattern rule (regex)"
            },

            // Min/Max numeric validation
            new FormField
            {
                Name = "age",
                Label = "Age",
                Type = "number",
                IsRequired = true,
                Order = 5,
                ValidationRules = new()
                {
                    new FormValidationRule { Type = "min", Value = "18", Message = "Must be at least 18 years old" },
                    new FormValidationRule { Type = "max", Value = "120", Message = "Must be 120 or younger" }
                },
                HelpText = "Validated with min and max numeric rules"
            },

            // Multiple validation rules on textarea
            new FormField
            {
                Name = "bio",
                Label = "Biography",
                Type = "textarea",
                IsRequired = true,
                Order = 6,
                ValidationRules = new()
                {
                    new FormValidationRule { Type = "minlength", Value = "20", Message = "Bio must be at least 20 characters" },
                    new FormValidationRule { Type = "maxlength", Value = "500", Message = "Bio cannot exceed 500 characters" }
                },
                HelpText = "Validated with minlength and maxlength rules"
            },

            // Custom validation (note: currently not implemented, see Troubleshooting)
            new FormField
            {
                Name = "customField",
                Label = "Custom Validation Field",
                Type = "text",
                Order = 7,
                ValidationRules = new()
                {
                    new FormValidationRule
                    {
                        Type = "custom",
                        CustomFunction = "ValidateCustomFormat",
                        Message = "Custom validation function not yet implemented"
                    }
                },
                HelpText = "Attempted custom validation (not yet functional)"
            }
        },
        SubmitConfig = new()
        {
            SubmitButtonText = "Validate All",
            ShowResetButton = true
        }
    };

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            // All validations passed
        }
        else if (result.FieldErrors.Any())
        {
            // Display field errors
        }
    }
}
```

### Sample 4: Handling FormSubmissionResult

```razor
@page "/result-handling"
@using SmartWorkz.Web.Models

<div>
    @if (!string.IsNullOrEmpty(SubmitMessage))
    {
        <div class="alert alert-@MessageType">
            @SubmitMessage
        </div>
    }

    @if (FormErrors.Any())
    {
        <div class="alert alert-warning">
            <h5>Validation Errors:</h5>
            <ul>
                @foreach (var kvp in FormErrors)
                {
                    <li>
                        <strong>@kvp.Key:</strong>
                        @string.Join(", ", kvp.Value)
                    </li>
                }
            </ul>
        </div>
    }

    @if (LastSubmittedData != null)
    {
        <div class="alert alert-info">
            <h5>Last Submitted Data:</h5>
            <pre>@System.Text.Json.JsonSerializer.Serialize(LastSubmittedData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })</pre>
        </div>
    }
</div>

<FormBuilderComponent FormDef="SampleForm" OnSubmit="HandleFormSubmit" />

@code {
    private string SubmitMessage = string.Empty;
    private string MessageType = "info";
    private Dictionary<string, List<string>> FormErrors = new();
    private Dictionary<string, object?>? LastSubmittedData;

    private FormDefinition SampleForm = new()
    {
        Title = "Sample Form",
        Fields = new()
        {
            new FormField { Name = "email", Label = "Email", Type = "email", IsRequired = true },
            new FormField { Name = "message", Label = "Message", Type = "textarea", IsRequired = true }
        }
    };

    private async Task HandleFormSubmit(FormSubmissionResult result)
    {
        // Check if submission was successful
        if (result.IsSuccess)
        {
            SubmitMessage = result.Message;
            MessageType = "success";
            LastSubmittedData = result.Data;
            FormErrors.Clear();

            // Access submitted data by field name
            var email = result.Data["email"]?.ToString();
            var message = result.Data["message"]?.ToString();

            // You can also iterate through all data
            foreach (var kvp in result.Data)
            {
                System.Diagnostics.Debug.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }
        else
        {
            // Handle validation errors
            SubmitMessage = result.Message;
            MessageType = "danger";
            FormErrors = result.FieldErrors;
            LastSubmittedData = null;

            // Process field-level errors
            foreach (var fieldError in result.FieldErrors)
            {
                var fieldName = fieldError.Key;
                var errors = fieldError.Value;
                
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Field '{fieldName}': {error}");
                }
            }
        }

        // Optional: Use metadata for additional information
        if (result.Metadata.Any())
        {
            foreach (var meta in result.Metadata)
            {
                System.Diagnostics.Debug.WriteLine($"Metadata: {meta.Key} = {meta.Value}");
            }
        }

        // Optional: Redirect after successful submission
        if (result.IsSuccess && !string.IsNullOrEmpty(result.RedirectUrl))
        {
            // NavigationManager.NavigateTo(result.RedirectUrl);
        }
    }
}
```

---

## API Reference

### FormBuilderComponent

The main Blazor component that renders and manages forms.

**Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `FormDef` | `FormDefinition?` | The form definition containing all fields and configuration |
| `OnSubmit` | `EventCallback<FormSubmissionResult>` | Callback invoked when form is validly submitted |
| `OnCancel` | `EventCallback` | Callback invoked when cancel button is clicked |

**Properties:**

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `SuccessMessage` | `string` | Private | Message displayed on successful submission |
| `ErrorMessage` | `string` | Private | Message displayed when validation fails |
| `IsSubmitting` | `bool` | Private | Flag indicating submission is in progress |
| `FieldErrors` | `Dictionary<string, List<string>>` | Private | Collection of field-level validation errors |

**Methods:**

| Method | Returns | Description |
|--------|---------|-------------|
| `HandleValidSubmit()` | `Task` | Invoked when EditForm validation passes |
| `HandleInvalidSubmit()` | `Task` | Invoked when EditForm validation fails |
| `ValidateForm()` | `Dictionary<string, List<string>>` | Executes all custom validation rules |
| `UpdateFieldVisibility()` | `void` | Recalculates field visibility based on conditional logic |
| `EvaluateVisibility(FormField)` | `bool` | Determines if a field should be visible |
| `ResetForm()` | `Task` | Clears all data and messages |

### FormDefinition

Defines the complete structure and configuration of a form.

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Id` | `string` | `Guid.NewGuid()` | Unique form identifier |
| `Title` | `string` | `""` | Form title displayed at top |
| `Description` | `string?` | `null` | Form description/instructions |
| `Fields` | `List<FormField>` | `[]` | Collection of form fields |
| `SubmitConfig` | `FormSubmitConfig` | New instance | Button text and display settings |
| `IsDisabled` | `bool` | `false` | Disables entire form when true |
| `CssClass` | `string?` | `null` | CSS class for styling form container |

### FormField

Represents a single field in the form.

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Name` | `string` | `""` | Unique field identifier (used as key in submission data) |
| `Label` | `string` | `""` | Display label shown to user |
| `Type` | `string` | `"text"` | Field type (text, email, password, number, select, checkbox, textarea, date) |
| `Value` | `object?` | `null` | Current or default field value |
| `Placeholder` | `string?` | `null` | Placeholder text shown when empty |
| `HelpText` | `string?` | `null` | Help text displayed below field |
| `IsRequired` | `bool` | `false` | Whether field is required |
| `IsDisabled` | `bool` | `false` | Whether field is disabled for input |
| `ValidationRules` | `List<FormValidationRule>` | `[]` | Collection of validation rules |
| `Options` | `List<FormFieldOption>` | `[]` | Options for select/dropdown fields |
| `DependsOn` | `string?` | `null` | Name of field this depends on for visibility |
| `DependsOnValue` | `object?` | `null` | Value required for this field to be visible |
| `CssClass` | `string?` | `null` | CSS class for field styling |
| `Order` | `int` | `0` | Sort order (ascending) |
| `IsVisible` | `bool` | `true` | Computed visibility (don't set directly) |

### FormFieldOption

Represents an option in a select/dropdown field.

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Label` | `string` | `""` | Text displayed to user |
| `Value` | `object` | `""` | Value submitted with form |
| `IsDisabled` | `bool` | `false` | Whether option is disabled |

### FormValidationRule

Represents a validation rule applied to a field.

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Type` | `string` | `""` | Validation type (required, email, minlength, maxlength, pattern, custom, min, max) |
| `Value` | `string?` | `null` | Rule parameter (length, number, regex pattern) |
| `Message` | `string` | `"This field is invalid"` | Error message if validation fails |
| `CustomFunction` | `string?` | `null` | Function name for custom validation (not yet implemented) |

### FormSubmitConfig

Configuration for form submission buttons.

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SubmitButtonText` | `string` | `"Submit"` | Text on submit button |
| `CancelButtonText` | `string` | `"Cancel"` | Text on cancel button |
| `ShowCancelButton` | `bool` | `true` | Whether to display cancel button |
| `ShowResetButton` | `bool` | `false` | Whether to display reset button |
| `SubmitButtonClass` | `string` | `"btn btn-primary"` | CSS class for submit button |
| `CancelButtonClass` | `string` | `"btn btn-secondary"` | CSS class for cancel button |
| `ResetButtonClass` | `string` | `"btn btn-outline-secondary"` | CSS class for reset button |

### FormSubmissionResult

Result object returned after form submission.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `IsSuccess` | `bool` | Whether submission was successful (all validations passed) |
| `Message` | `string` | Status message to display to user |
| `Data` | `Dictionary<string, object?>` | Submitted form data, keyed by field name |
| `FieldErrors` | `Dictionary<string, List<string>>` | Field-level validation errors |
| `Metadata` | `Dictionary<string, object?>` | Additional metadata from submission |
| `RedirectUrl` | `string?` | Optional URL to redirect after successful submission |

---

## Integration Notes

### Cross-References

- **[Template Engine](13-template-engine.md)** — Use with Form Builder for dynamic email templates based on form submissions
- **[Email Service](../SMARTWORKZ_SERVICES_COMPLETE.md)** — Send confirmation/notification emails after form submission
- **[Cache Attribute](12-cache-attribute.md)** — Cache form definitions if dynamically loaded from database
- **[Logging](../LOGGING_GUIDE.md)** — Log form submissions and validation errors

### Integration Pattern

**Step 1: Define your form**

```csharp
private FormDefinition MyForm = new()
{
    Title = "My Form",
    Fields = new() { /* ... */ }
};
```

**Step 2: Add to Razor page**

```razor
<FormBuilderComponent FormDef="MyForm" OnSubmit="HandleSubmit" OnCancel="HandleCancel" />
```

**Step 3: Handle submission**

```csharp
private async Task HandleSubmit(FormSubmissionResult result)
{
    if (result.IsSuccess)
    {
        // Process result.Data
    }
}
```

### Loading Forms from Database

```csharp
@inject IFormRepository FormRepository

@code {
    private FormDefinition? LoadedForm;

    protected override async Task OnInitializedAsync()
    {
        LoadedForm = await FormRepository.GetFormAsync("form-id");
    }
}
```

### Custom Form Services

```csharp
public interface IFormSubmissionHandler
{
    Task<FormSubmissionResult> HandleAsync(FormDefinition form, Dictionary<string, object?> data);
}

public class OrderFormHandler : IFormSubmissionHandler
{
    public async Task<FormSubmissionResult> HandleAsync(FormDefinition form, Dictionary<string, object?> data)
    {
        try
        {
            var order = CreateOrderFromFormData(data);
            await _orderService.SaveAsync(order);

            return new FormSubmissionResult
            {
                IsSuccess = true,
                Message = "Order created successfully",
                Data = data,
                RedirectUrl = $"/orders/{order.Id}"
            };
        }
        catch (Exception ex)
        {
            return new FormSubmissionResult
            {
                IsSuccess = false,
                Message = "Failed to create order",
                Data = data
            };
        }
    }
}
```

---

## Troubleshooting

### Issue 1: Custom Validation Not Working

**Problem:** You set `ValidationRule.Type = "custom"` with a `CustomFunction` name, but validation is never executed.

**Root Cause:** The ValidateField() method in FormBuilderComponent has a switch statement on rule.Type that does NOT include a case for "custom". It falls through to the default case and returns null, so custom validation is bypassed.

**Code Location:** `src/SmartWorkz.Core.Web/Components/FormBuilder/FormBuilderComponent.razor:248-262`

```csharp
private string? ValidateField(FormField field, FormValidationRule rule)
{
    var value = field.Value?.ToString() ?? string.Empty;

    return rule.Type?.ToLower() switch
    {
        "email" => /* validation */,
        "minlength" => /* validation */,
        "maxlength" => /* validation */,
        "pattern" => /* validation */,
        "min" => /* validation */,
        "max" => /* validation */,
        _ => null  // ← "custom" falls through here!
    };
}
```

**Solution:** Until custom validation is implemented, use **pattern-based validation** for custom rules:

```csharp
// Instead of custom validation:
new FormValidationRule
{
    Type = "custom",
    CustomFunction = "ValidateSSN",
    Message = "Invalid SSN"
}

// Use pattern validation:
new FormValidationRule
{
    Type = "pattern",
    Value = @"^\d{3}-\d{2}-\d{4}$",
    Message = "SSN must be in format XXX-XX-XXXX"
}
```

**Extension Pattern (when custom validation is needed):**

```csharp
// Create a subclass of FormBuilderComponent
@inherits FormBuilderComponent
@namespace MyApp.Components.FormBuilder

@* Override ValidateField to add custom validation *@

@code {
    protected override string? ValidateField(FormField field, FormValidationRule rule)
    {
        return rule.Type?.ToLower() switch
        {
            // Call base for standard rules
            var type when base.ValidateField(field, rule) is not null => base.ValidateField(field, rule),
            
            // Add custom validation
            "custom" => rule.CustomFunction switch
            {
                "ValidateSSN" => ValidateSSN(field.Value?.ToString() ?? ""),
                "ValidateZipCode" => ValidateZipCode(field.Value?.ToString() ?? ""),
                _ => null
            },
            
            _ => null
        };
    }

    private string? ValidateSSN(string value)
    {
        // Custom validation logic
        return System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{3}-\d{2}-\d{4}$")
            ? null
            : "Invalid SSN format";
    }

    private string? ValidateZipCode(string value)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{5}(-\d{4})?$")
            ? null
            : "Invalid ZIP code format";
    }
}
```

### Issue 2: Conditional Fields Not Showing/Hiding

**Problem:** Fields with DependsOn/DependsOnValue don't appear/disappear when dependent field changes.

**Solution:** The component uses EvaluateVisibility() to determine visibility. Ensure:

1. DependsOn field name matches exactly (case-sensitive)
2. DependsOnValue type matches field value type (string, int, bool, etc.)
3. The dependent field is rendered BEFORE the conditional field (set Order accordingly)

```csharp
// Correct:
new FormField { Name = "accountType", Type = "select", Value = "personal", Order = 1 },
new FormField { 
    Name = "companyName", 
    DependsOn = "accountType",      // Must match exactly!
    DependsOnValue = "business",    // Must match the value type
    Order = 2 
}

// Incorrect:
new FormField { 
    Name = "companyName", 
    DependsOn = "AccountType",      // Wrong case!
    DependsOnValue = "business",
    Order = 2 
}
```

### Issue 3: Validation Errors Not Showing for Required Fields

**Problem:** IsRequired = true on a field but validation errors don't display.

**Solution:** The ValidateForm() method checks IsRequired separately from ValidationRules. Both are evaluated:

```csharp
// From ValidateForm():
if (field.IsRequired && string.IsNullOrEmpty(field.Value?.ToString()))
{
    fieldErrors.Add($"{field.Label} is required");
}
```

Ensure the field is visible when submitted (hidden required fields are still validated). If you want optional fields, set IsRequired = false:

```csharp
// Correct:
new FormField { Name = "phone", IsRequired = false }

// If field should be visible and required:
new FormField { Name = "email", IsRequired = true, DependsOn = null }
```

### Issue 4: Form Data Not Persisting After Reset

**Problem:** After clicking Reset, form shows initial values, but FormModel retains old data.

**Solution:** This is expected behavior — ResetForm() calls InitializeForm() which resets FormModel.Data from current FormDef.Fields values. To persist values across resets, save them before resetting:

```csharp
private Dictionary<string, object?> SavedData = new();

private async Task HandleBeforeReset()
{
    SavedData = new Dictionary<string, object?>(FormModel.Data);
    // Then call ResetForm()
}

private void RestoreSavedData()
{
    FormModel.Data = new Dictionary<string, object?>(SavedData);
}
```

---

