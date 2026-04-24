# Form Builder Component

A powerful Blazor-based dynamic form builder that supports field validation, conditional visibility, and flexible configuration. Perfect for creating dynamic registration forms, surveys, multi-step wizards, and complex data entry interfaces.

## Table of Contents
1. [Components Overview](#components-overview)
2. [Field Types](#field-types)
3. [Validation Rules](#validation-rules)
4. [Conditional Field Visibility](#conditional-field-visibility)
5. [Usage Examples](#usage-examples)
6. [Integration Guide](#integration-guide)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

## Components Overview

### FormBuilderComponent
The main form container that manages field rendering, validation, and submission.

**Parameters:**
- `FormDef` (FormDefinition): The form definition containing title, fields, and configuration
- `OnSubmit` (EventCallback<FormSubmissionResult>): Callback when form is submitted successfully
- `OnCancel` (EventCallback): Callback when cancel button is clicked

**Features:**
- Dynamic field rendering based on configuration
- Built-in validation with error display
- Conditional field visibility using DependsOn logic
- Success/error message display
- Form submission handling
- CSRF protection through standard Blazor mechanisms

### FormFieldComponent
Individual field renderer that handles different input types and validation.

**Parameters:**
- `Field` (FormField): The field definition
- `OnFieldValueChanged` (EventCallback<FormFieldValueChangedArgs>): Callback when field value changes
- `Errors` (List<string>): Validation errors for the field

## Field Types

The FormBuilder supports 8 field types, each with specific use cases:

### 1. Text Field
Standard text input for general text entry.

```csharp
new FormField
{
    Name = "firstName",
    Label = "First Name",
    Type = "text",
    IsRequired = true,
    Placeholder = "Enter your first name",
    HelpText = "Your legal first name as it appears on official documents",
    Order = 1,
    ValidationRules = new List<FormValidationRule>
    {
        new FormValidationRule
        {
            Type = "required",
            Message = "First name is required"
        },
        new FormValidationRule
        {
            Type = "minLength",
            Value = "2",
            Message = "First name must be at least 2 characters"
        },
        new FormValidationRule
        {
            Type = "maxLength",
            Value = "50",
            Message = "First name cannot exceed 50 characters"
        }
    }
}
```

### 2. Email Field
Email input with built-in email validation.

```csharp
new FormField
{
    Name = "email",
    Label = "Email Address",
    Type = "email",
    IsRequired = true,
    Placeholder = "user@example.com",
    HelpText = "We'll never share your email with others",
    Order = 2,
    ValidationRules = new List<FormValidationRule>
    {
        new FormValidationRule
        {
            Type = "required",
            Message = "Email is required"
        },
        new FormValidationRule
        {
            Type = "email",
            Message = "Please enter a valid email address"
        }
    }
}
```

### 3. Password Field
Secure password input with masked display.

```csharp
new FormField
{
    Name = "password",
    Label = "Password",
    Type = "password",
    IsRequired = true,
    Placeholder = "Enter a strong password",
    HelpText = "Minimum 8 characters with uppercase, number, and special character",
    Order = 3,
    ValidationRules = new List<FormValidationRule>
    {
        new FormValidationRule
        {
            Type = "required",
            Message = "Password is required"
        },
        new FormValidationRule
        {
            Type = "minLength",
            Value = "8",
            Message = "Password must be at least 8 characters long"
        },
        new FormValidationRule
        {
            Type = "pattern",
            Value = "(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])",
            Message = "Password must contain uppercase letter, number, and special character"
        }
    }
}
```

### 4. Number Field
Numeric input for quantity, age, or numeric data.

```csharp
new FormField
{
    Name = "age",
    Label = "Age",
    Type = "number",
    IsRequired = true,
    Placeholder = "Enter your age",
    Order = 4,
    ValidationRules = new List<FormValidationRule>
    {
        new FormValidationRule
        {
            Type = "required",
            Message = "Age is required"
        },
        new FormValidationRule
        {
            Type = "min",
            Value = "18",
            Message = "You must be at least 18 years old"
        },
        new FormValidationRule
        {
            Type = "max",
            Value = "120",
            Message = "Please enter a valid age"
        }
    }
}
```

### 5. Select Field (Dropdown)
Dropdown selection with predefined options.

```csharp
new FormField
{
    Name = "country",
    Label = "Country",
    Type = "select",
    IsRequired = true,
    Order = 5,
    Options = new List<FormFieldOption>
    {
        new FormFieldOption { Label = "United States", Value = "US" },
        new FormFieldOption { Label = "Canada", Value = "CA" },
        new FormFieldOption { Label = "United Kingdom", Value = "UK" },
        new FormFieldOption { Label = "Australia", Value = "AU" }
    },
    ValidationRules = new List<FormValidationRule>
    {
        new FormValidationRule
        {
            Type = "required",
            Message = "Please select a country"
        }
    }
}
```

### 6. Checkbox Field
Boolean checkbox for accepting terms or enabling options.

```csharp
new FormField
{
    Name = "agreeToTerms",
    Label = "I agree to the Terms of Service",
    Type = "checkbox",
    IsRequired = true,
    HelpText = "You must agree to continue",
    Order = 6,
    ValidationRules = new List<FormValidationRule>
    {
        new FormValidationRule
        {
            Type = "required",
            Message = "You must agree to the terms"
        }
    }
}
```

### 7. Textarea Field
Multi-line text input for longer content.

```csharp
new FormField
{
    Name = "bio",
    Label = "Biography",
    Type = "textarea",
    IsRequired = false,
    Placeholder = "Tell us about yourself...",
    HelpText = "Maximum 500 characters",
    Order = 7,
    ValidationRules = new List<FormValidationRule>
    {
        new FormValidationRule
        {
            Type = "maxLength",
            Value = "500",
            Message = "Biography cannot exceed 500 characters"
        }
    }
}
```

### 8. Date Field
Date picker for date selection.

```csharp
new FormField
{
    Name = "dateOfBirth",
    Label = "Date of Birth",
    Type = "date",
    IsRequired = true,
    Order = 8,
    ValidationRules = new List<FormValidationRule>
    {
        new FormValidationRule
        {
            Type = "required",
            Message = "Date of birth is required"
        }
    }
}
```

## Validation Rules

The FormBuilder supports comprehensive validation with multiple rule types:

### Validation Rule Types

**Required**
Ensures field has a value.
```csharp
new FormValidationRule
{
    Type = "required",
    Message = "This field is required"
}
```

**Email**
Validates email format.
```csharp
new FormValidationRule
{
    Type = "email",
    Message = "Please enter a valid email address"
}
```

**Minimum Length**
Ensures minimum string length.
```csharp
new FormValidationRule
{
    Type = "minLength",
    Value = "3",
    Message = "Must be at least 3 characters"
}
```

**Maximum Length**
Ensures maximum string length.
```csharp
new FormValidationRule
{
    Type = "maxLength",
    Value = "100",
    Message = "Cannot exceed 100 characters"
}
```

**Minimum Value**
Ensures minimum numeric value.
```csharp
new FormValidationRule
{
    Type = "min",
    Value = "0",
    Message = "Value must be at least 0"
}
```

**Maximum Value**
Ensures maximum numeric value.
```csharp
new FormValidationRule
{
    Type = "max",
    Value = "100",
    Message = "Value cannot exceed 100"
}
```

**Pattern (Regex)**
Validates against regex pattern.
```csharp
new FormValidationRule
{
    Type = "pattern",
    Value = "^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Z|a-z]{2,}$",
    Message = "Invalid format"
}
```

**Custom**
Custom validation function name.
```csharp
new FormValidationRule
{
    Type = "custom",
    CustomFunction = "ValidateUsername",
    Message = "Username is already taken"
}
```

## Conditional Field Visibility

Show/hide fields based on other field values using DependsOn logic.

```csharp
// Parent field
new FormField
{
    Name = "userType",
    Label = "Are you a business?",
    Type = "select",
    IsRequired = true,
    Options = new List<FormFieldOption>
    {
        new FormFieldOption { Label = "Individual", Value = "individual" },
        new FormFieldOption { Label = "Business", Value = "business" }
    },
    Order = 1
},
// Child field - only shows when userType = "business"
new FormField
{
    Name = "companyName",
    Label = "Company Name",
    Type = "text",
    IsRequired = true,
    DependsOn = "userType",
    DependsOnValue = "business",
    Order = 2
}
```

## Usage Examples

### Example 1: Complete Registration Form

```csharp
@page "/register"
@using SmartWorkz.Web.Models

<FormBuilderComponent FormDef="registrationForm" 
                      OnSubmit="HandleFormSubmit"
                      OnCancel="HandleCancel" />

@code {
    private FormDefinition registrationForm = new()
    {
        Title = "Create Your Account",
        Description = "Join our community in just a few minutes",
        Fields = new List<FormField>
        {
            new FormField
            {
                Name = "firstName",
                Label = "First Name",
                Type = "text",
                IsRequired = true,
                Placeholder = "John",
                Order = 1,
                ValidationRules = new List<FormValidationRule>
                {
                    new FormValidationRule { Type = "required", Message = "First name is required" },
                    new FormValidationRule { Type = "minLength", Value = "2", Message = "Must be at least 2 characters" }
                }
            },
            new FormField
            {
                Name = "lastName",
                Label = "Last Name",
                Type = "text",
                IsRequired = true,
                Placeholder = "Doe",
                Order = 2,
                ValidationRules = new List<FormValidationRule>
                {
                    new FormValidationRule { Type = "required", Message = "Last name is required" }
                }
            },
            new FormField
            {
                Name = "email",
                Label = "Email Address",
                Type = "email",
                IsRequired = true,
                Placeholder = "john@example.com",
                Order = 3,
                ValidationRules = new List<FormValidationRule>
                {
                    new FormValidationRule { Type = "required", Message = "Email is required" },
                    new FormValidationRule { Type = "email", Message = "Invalid email format" }
                }
            },
            new FormField
            {
                Name = "password",
                Label = "Password",
                Type = "password",
                IsRequired = true,
                HelpText = "Minimum 8 characters, uppercase, number, and special character required",
                Order = 4,
                ValidationRules = new List<FormValidationRule>
                {
                    new FormValidationRule { Type = "required", Message = "Password is required" },
                    new FormValidationRule { Type = "minLength", Value = "8", Message = "Minimum 8 characters" },
                    new FormValidationRule { Type = "pattern", Value = "(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])", Message = "Must contain uppercase, number, and special character" }
                }
            },
            new FormField
            {
                Name = "agreeToTerms",
                Label = "I agree to the Terms of Service and Privacy Policy",
                Type = "checkbox",
                IsRequired = true,
                Order = 5,
                ValidationRules = new List<FormValidationRule>
                {
                    new FormValidationRule { Type = "required", Message = "You must agree to continue" }
                }
            }
        },
        SubmitConfig = new FormSubmitConfig
        {
            SubmitButtonText = "Create Account",
            ShowCancelButton = true,
            CancelButtonText = "Back to Login"
        }
    };

    private async Task HandleFormSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            var firstName = result.Data["firstName"];
            var lastName = result.Data["lastName"];
            var email = result.Data["email"];
            var password = result.Data["password"];

            // Register user via API
            await UserService.RegisterAsync(firstName, lastName, email, password);
            await Navigation.NavigateTo("/login-success");
        }
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/login");
    }
}
```

### Example 2: Multi-Step Form with Conditional Fields

```csharp
@page "/business-registration"

<FormBuilderComponent FormDef="businessForm" 
                      OnSubmit="HandleFormSubmit"
                      OnCancel="HandleCancel" />

@code {
    private FormDefinition businessForm = new()
    {
        Title = "Register Your Business",
        Description = "Tell us about your business to get started",
        Fields = new List<FormField>
        {
            new FormField
            {
                Name = "businessType",
                Label = "Business Type",
                Type = "select",
                IsRequired = true,
                Order = 1,
                Options = new List<FormFieldOption>
                {
                    new FormFieldOption { Label = "Sole Proprietor", Value = "sole" },
                    new FormFieldOption { Label = "Partnership", Value = "partnership" },
                    new FormFieldOption { Label = "Corporation", Value = "corp" },
                    new FormFieldOption { Label = "LLC", Value = "llc" }
                }
            },
            new FormField
            {
                Name = "businessName",
                Label = "Business Name",
                Type = "text",
                IsRequired = true,
                Order = 2
            },
            // Only shows for partnerships
            new FormField
            {
                Name = "numberOfPartners",
                Label = "Number of Partners",
                Type = "number",
                IsRequired = true,
                DependsOn = "businessType",
                DependsOnValue = "partnership",
                Order = 3
            },
            // Only shows for corporations
            new FormField
            {
                Name = "taxId",
                Label = "Tax ID (EIN)",
                Type = "text",
                IsRequired = true,
                DependsOn = "businessType",
                DependsOnValue = "corp",
                Order = 4
            },
            new FormField
            {
                Name = "industry",
                Label = "Industry",
                Type = "select",
                IsRequired = true,
                Order = 5,
                Options = new List<FormFieldOption>
                {
                    new FormFieldOption { Label = "Technology", Value = "tech" },
                    new FormFieldOption { Label = "Retail", Value = "retail" },
                    new FormFieldOption { Label = "Services", Value = "services" },
                    new FormFieldOption { Label = "Other", Value = "other" }
                }
            }
        },
        SubmitConfig = new FormSubmitConfig
        {
            SubmitButtonText = "Register Business",
            ShowCancelButton = true
        }
    };

    private async Task HandleFormSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            // Process business registration
            var businessData = result.Data;
            await BusinessService.RegisterAsync(businessData);
        }
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }
}
```

### Example 3: Survey Form with Multiple Validations

```csharp
var surveyForm = new FormDefinition
{
    Title = "Customer Satisfaction Survey",
    Description = "Help us improve our service",
    Fields = new List<FormField>
    {
        new FormField
        {
            Name = "rating",
            Label = "Overall Rating",
            Type = "select",
            IsRequired = true,
            Order = 1,
            Options = new List<FormFieldOption>
            {
                new FormFieldOption { Label = "Excellent (5)", Value = "5" },
                new FormFieldOption { Label = "Good (4)", Value = "4" },
                new FormFieldOption { Label = "Average (3)", Value = "3" },
                new FormFieldOption { Label = "Poor (2)", Value = "2" },
                new FormFieldOption { Label = "Very Poor (1)", Value = "1" }
            }
        },
        new FormField
        {
            Name = "feedback",
            Label = "Your Feedback",
            Type = "textarea",
            IsRequired = true,
            Placeholder = "Please provide detailed feedback...",
            Order = 2,
            ValidationRules = new List<FormValidationRule>
            {
                new FormValidationRule { Type = "required", Message = "Feedback is required" },
                new FormValidationRule { Type = "minLength", Value = "10", Message = "Minimum 10 characters" },
                new FormValidationRule { Type = "maxLength", Value = "1000", Message = "Maximum 1000 characters" }
            }
        },
        new FormField
        {
            Name = "contactEmail",
            Label = "Email (optional)",
            Type = "email",
            IsRequired = false,
            Placeholder = "your@email.com",
            Order = 3,
            ValidationRules = new List<FormValidationRule>
            {
                new FormValidationRule { Type = "email", Message = "Invalid email format" }
            }
        }
    }
};
```

## Integration Guide

### Basic Integration into Blazor Pages

To use FormBuilder in your Blazor pages, follow these steps:

**Step 1: Add the component to your page**
```blazor
@page "/contact"
@using SmartWorkz.Web.Models
@using SmartWorkz.Core.Web.Components.FormBuilder

<FormBuilderComponent FormDef="myForm" 
                      OnSubmit="HandleSubmit"
                      OnCancel="HandleCancel" />
```

**Step 2: Define form in @code block**
```csharp
@code {
    private FormDefinition myForm;

    protected override void OnInitialized()
    {
        myForm = new FormDefinition
        {
            Title = "My Form",
            Fields = new List<FormField> { /* ... */ }
        };
    }
}
```

**Step 3: Handle form submission**
```csharp
private async Task HandleSubmit(FormSubmissionResult result)
{
    if (result.IsSuccess)
    {
        // Access form data via result.Data dictionary
        var value = result.Data["fieldName"];
        
        // Save to database, call API, etc.
        await SaveFormData(result.Data);
    }
}

private async Task HandleCancel()
{
    // Handle cancellation
    await Navigation.NavigateTo("/");
}
```

### Integration with ASP.NET Core Models

Map form submission to your domain models:

```csharp
// Your domain model
public class RegistrationModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
}

// Handle submission
private async Task HandleFormSubmit(FormSubmissionResult result)
{
    if (result.IsSuccess)
    {
        var model = new RegistrationModel
        {
            FirstName = result.Data["firstName"]?.ToString(),
            LastName = result.Data["lastName"]?.ToString(),
            Email = result.Data["email"]?.ToString(),
            DateOfBirth = DateTime.Parse(result.Data["dateOfBirth"]?.ToString())
        };

        await UserService.RegisterAsync(model);
    }
}
```

### Saving Form Data to Database

```csharp
private async Task SaveFormData(Dictionary<string, object> formData)
{
    try
    {
        var response = await HttpClient.PostAsJsonAsync("/api/forms/submit", new
        {
            FormId = "registration-form",
            SubmissionData = formData,
            SubmittedAt = DateTime.UtcNow
        });

        if (response.IsSuccessStatusCode)
        {
            await Notification.ShowSuccess("Data saved successfully!");
        }
    }
    catch (Exception ex)
    {
        await Notification.ShowError("Failed to save data: " + ex.Message);
    }
}
```

### Dynamic Form Generation

Load form definitions from database or configuration:

```csharp
@inject IFormService FormService

@code {
    private FormDefinition dynamicForm;

    protected override async Task OnInitializedAsync()
    {
        // Load form definition from service
        dynamicForm = await FormService.GetFormDefinitionAsync("registration-form-v2");
    }
}
```

## Best Practices

### 1. Field Naming
- Use camelCase for field names
- Keep names concise but descriptive
- Avoid special characters

```csharp
// Good
new FormField { Name = "firstName", Label = "First Name" }

// Bad
new FormField { Name = "fn", Label = "First Name" }
new FormField { Name = "first-name", Label = "First Name" }
```

### 2. Validation Strategy
- Always validate required fields
- Add format validation (email, pattern)
- Include helpful error messages
- Use multiple validation rules when needed

```csharp
ValidationRules = new List<FormValidationRule>
{
    new FormValidationRule { Type = "required", Message = "Required" },
    new FormValidationRule { Type = "minLength", Value = "5", Message = "Too short" },
    new FormValidationRule { Type = "maxLength", Value = "50", Message = "Too long" }
}
```

### 3. User Experience
- Provide placeholder text and help text
- Order fields logically (top to bottom)
- Group related fields together
- Use conditional fields to reduce complexity
- Show progress in multi-step forms

```csharp
new FormField
{
    Name = "email",
    Label = "Email Address",
    Type = "email",
    Placeholder = "user@example.com",
    HelpText = "We never share your email",
    Order = 2
}
```

### 4. Error Handling
- Show clear, actionable error messages
- Display errors at the field level
- Highlight invalid fields visually
- Prevent submission of invalid forms

```csharp
private async Task HandleFormSubmit(FormSubmissionResult result)
{
    if (!result.IsSuccess)
    {
        foreach (var error in result.FieldErrors)
        {
            // Display field-specific errors
            await Notification.ShowError($"{error.Key}: {error.Value}");
        }
        return;
    }
}
```

### 5. Accessibility
- Use semantic HTML labels
- Provide descriptive placeholders
- Include help text for complex fields
- Ensure keyboard navigation works
- Use aria-* attributes appropriately

```csharp
new FormField
{
    Name = "password",
    Label = "Password",
    Type = "password",
    HelpText = "Minimum 8 characters. Include uppercase, number, and special character.",
    IsRequired = true
}
```

## Model Classes Reference

### FormDefinition
Complete form structure and configuration.

**Key Properties:**
- `Id`: Unique identifier
- `Title`: Form title
- `Description`: Form description
- `Fields`: List of form fields
- `SubmitConfig`: Button configuration
- `IsDisabled`: Disable entire form

### FormField
Individual form field definition.

**Key Properties:**
- `Name`: Field identifier
- `Label`: Display label
- `Type`: Field type (text, email, password, number, select, checkbox, textarea, date)
- `IsRequired`: Required field
- `Placeholder`: Input placeholder text
- `HelpText`: Help text below field
- `ValidationRules`: List of validation rules
- `Options`: Select/dropdown options
- `DependsOn`: Parent field name for conditional visibility
- `DependsOnValue`: Value to trigger visibility
- `Order`: Display order

### FormValidationRule
Validation rule configuration.

**Key Properties:**
- `Type`: Rule type (required, email, minLength, maxLength, min, max, pattern, custom)
- `Value`: Rule parameter
- `Message`: Error message
- `CustomFunction`: Custom validator name

### FormSubmitConfig
Form submission configuration.

**Key Properties:**
- `SubmitButtonText`: Submit button label
- `ShowCancelButton`: Show cancel button
- `CancelButtonText`: Cancel button label
- `ShowResetButton`: Show reset button
- `SubmitButtonClass`: CSS class for submit button
- `CancelButtonClass`: CSS class for cancel button

### FormSubmissionResult
Result after form submission.

**Key Properties:**
- `IsSuccess`: Submission successful
- `Message`: Result message
- `Data`: Form data as dictionary
- `FieldErrors`: Field-level errors
- `Metadata`: Additional metadata
- `RedirectUrl`: Optional redirect URL

## Styling

### Bootstrap Integration

FormBuilder uses Bootstrap classes for styling:
- `.form-control` - Text inputs, textarea, selects
- `.form-select` - Dropdown selects
- `.form-check` - Checkboxes
- `.is-invalid` - Invalid field state
- `.invalid-feedback` - Error message display

### Custom Styling

Override with CSS classes:

```csharp
new FormField
{
    Name = "email",
    Label = "Email",
    Type = "email",
    CssClass = "my-custom-field-class"
}
```

## Troubleshooting

### Form Not Displaying
- Check that FormBuilderComponent is imported
- Verify FormDef is properly initialized
- Ensure OnSubmit callback is defined
- Check browser console for JavaScript errors

### Validation Not Working
- Confirm ValidationRules are added to field
- Verify rule type is valid
- Check that error messages are defined
- Ensure pattern regex is valid (for pattern rules)

### Conditional Fields Not Showing
- Verify DependsOn field name matches exactly
- Check DependsOnValue matches the field's actual value
- Ensure both fields have proper Order values
- Test in browser console that dependency condition is met

### Data Not Being Captured
- Check field Name values are unique
- Verify FormSubmissionResult.IsSuccess before accessing data
- Confirm field values are populated before submission
- Use browser dev tools to inspect form values

## Accessibility Features

FormBuilder includes:
- Proper label associations
- ARIA attributes for error states
- Keyboard navigation support
- Semantic HTML structure
- Error messages linked to fields
- Focus management

## Examples Directory

Complete working examples are available in the `Examples` folder:
- Contact form
- User registration form with conditional fields
- Multi-step business registration
- Survey form with complex validation
- Login form with password requirements
