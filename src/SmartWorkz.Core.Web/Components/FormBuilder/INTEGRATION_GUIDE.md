# FormBuilder Integration Guide

A comprehensive guide for integrating the FormBuilder component into Blazor applications, connecting to services, databases, and APIs.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Page Integration](#page-integration)
3. [Service Integration](#service-integration)
4. [Database Integration](#database-integration)
5. [API Integration](#api-integration)
6. [Validation Patterns](#validation-patterns)
7. [Error Handling](#error-handling)
8. [Advanced Scenarios](#advanced-scenarios)

## Quick Start

### Minimal Setup

```blazor
@page "/my-form"
@using SmartWorkz.Web.Models
@using SmartWorkz.Core.Web.Components.FormBuilder

<FormBuilderComponent FormDef="myForm" 
                      OnSubmit="HandleSubmit"
                      OnCancel="HandleCancel" />

@code {
    private FormDefinition myForm;

    protected override void OnInitialized()
    {
        myForm = new FormDefinition
        {
            Title = "My Form",
            Fields = new List<FormField>
            {
                new FormField { Name = "name", Label = "Name", Type = "text", IsRequired = true }
            }
        };
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            var name = result.Data["name"];
            await Notification.ShowSuccess("Form submitted!");
        }
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }
}
```

## Page Integration

### Adding to a Blazor Page

```blazor
@page "/contact-us"
@attribute [RenderModeInteractive]
@inject NavigationManager Navigation
@inject NotificationService Notification
@using SmartWorkz.Web.Models
@using SmartWorkz.Core.Web.Components.FormBuilder

<div class="container mt-5">
    <FormBuilderComponent FormDef="contactForm" 
                          OnSubmit="HandleSubmit"
                          OnCancel="HandleCancel" />
</div>

@code {
    private FormDefinition contactForm;

    protected override void OnInitialized()
    {
        contactForm = BuildContactForm();
    }

    private FormDefinition BuildContactForm()
    {
        return new FormDefinition
        {
            Title = "Contact Us",
            Description = "Send us a message",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "name",
                    Label = "Full Name",
                    Type = "text",
                    IsRequired = true,
                    Order = 1,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Name required" },
                        new FormValidationRule { Type = "minLength", Value = "2", Message = "Too short" }
                    }
                },
                new FormField
                {
                    Name = "email",
                    Label = "Email",
                    Type = "email",
                    IsRequired = true,
                    Order = 2,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Email required" },
                        new FormValidationRule { Type = "email", Message = "Invalid email" }
                    }
                },
                new FormField
                {
                    Name = "message",
                    Label = "Message",
                    Type = "textarea",
                    IsRequired = true,
                    Order = 3,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "minLength", Value = "10", Message = "Minimum 10 chars" }
                    }
                }
            },
            SubmitConfig = new FormSubmitConfig
            {
                SubmitButtonText = "Send Message",
                ShowCancelButton = true
            }
        };
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            var name = result.Data["name"];
            var email = result.Data["email"];
            var message = result.Data["message"];

            // Process the submission
            await SendContactMessage(name.ToString(), email.ToString(), message.ToString());
        }
    }

    private async Task SendContactMessage(string name, string email, string message)
    {
        try
        {
            // TODO: Call service or API
            await Notification.ShowSuccess("Message sent successfully!");
            await Navigation.NavigateTo("/");
        }
        catch (Exception ex)
        {
            await Notification.ShowError("Failed to send message: " + ex.Message);
        }
    }

    private async Task HandleCancel()
    {
        if (await Notification.Confirm("Discard changes?"))
        {
            await Navigation.NavigateTo("/");
        }
    }
}
```

## Service Integration

### Using a FormService

```csharp
// Define service interface
public interface IFormService
{
    Task<FormDefinition> GetFormDefinitionAsync(string formId);
    Task<FormSubmissionResult> SubmitFormAsync(string formId, Dictionary<string, object> data);
}

// Implement service
public class FormService : IFormService
{
    private readonly HttpClient _httpClient;

    public FormService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FormDefinition> GetFormDefinitionAsync(string formId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/forms/{formId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<FormDefinition>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load form {formId}", ex);
        }
    }

    public async Task<FormSubmissionResult> SubmitFormAsync(string formId, Dictionary<string, object> data)
    {
        try
        {
            var request = new { FormId = formId, Data = data, SubmittedAt = DateTime.UtcNow };
            var response = await _httpClient.PostAsJsonAsync("/api/forms/submit", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<FormSubmissionResult>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Form submission failed", ex);
        }
    }
}

// Register in Program.cs
builder.Services.AddScoped<IFormService, FormService>();
```

### Using Service in Component

```blazor
@page "/register"
@inject IFormService FormService
@inject NavigationManager Navigation

<FormBuilderComponent FormDef="registrationForm" 
                      OnSubmit="HandleSubmit"
                      OnCancel="HandleCancel" />

@code {
    private FormDefinition registrationForm;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            registrationForm = await FormService.GetFormDefinitionAsync("user-registration");
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            var submitResult = await FormService.SubmitFormAsync("user-registration", result.Data);
            if (submitResult.IsSuccess)
            {
                await Navigation.NavigateTo("/success");
            }
        }
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }
}
```

## Database Integration

### Saving Form Submissions

```csharp
// Define submission model
public class FormSubmission
{
    public int Id { get; set; }
    public string FormId { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string SubmittedBy { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
}

// Define repository
public interface IFormSubmissionRepository
{
    Task<int> SaveAsync(FormSubmission submission);
    Task<FormSubmission> GetAsync(int id);
    Task<List<FormSubmission>> GetByFormIdAsync(string formId);
}

public class FormSubmissionRepository : IFormSubmissionRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public FormSubmissionRepository(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<int> SaveAsync(FormSubmission submission)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        context.FormSubmissions.Add(submission);
        await context.SaveChangesAsync();
        return submission.Id;
    }

    public async Task<FormSubmission> GetAsync(int id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.FormSubmissions.FindAsync(id);
    }

    public async Task<List<FormSubmission>> GetByFormIdAsync(string formId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.FormSubmissions
            .Where(x => x.FormId == formId)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync();
    }
}

// Register in Program.cs
builder.Services.AddScoped<IFormSubmissionRepository, FormSubmissionRepository>();
```

### Updating Records from Form

```csharp
// Update existing record
public interface IUserService
{
    Task<User> GetAsync(int id);
    Task UpdateAsync(int id, Dictionary<string, object> data);
}

public class UserService : IUserService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public async Task UpdateAsync(int id, Dictionary<string, object> data)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FindAsync(id);
        if (user == null) throw new InvalidOperationException("User not found");

        // Map form data to user
        if (data.TryGetValue("firstName", out var firstName))
            user.FirstName = firstName?.ToString();
        if (data.TryGetValue("lastName", out var lastName))
            user.LastName = lastName?.ToString();
        if (data.TryGetValue("email", out var email))
            user.Email = email?.ToString();

        context.Users.Update(user);
        await context.SaveChangesAsync();
    }
}
```

## API Integration

### POST Form Data

```csharp
private async Task SubmitToApi(FormSubmissionResult result)
{
    try
    {
        var response = await HttpClient.PostAsJsonAsync("/api/forms/submit", new
        {
            FormId = "user-registration",
            SubmissionData = result.Data,
            SubmittedAt = DateTime.UtcNow
        });

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsAsync<dynamic>();
            await Notification.ShowSuccess("Success! ID: " + responseData.Id);
            await Navigation.NavigateTo("/confirmation/" + responseData.Id);
        }
        else
        {
            await Notification.ShowError("Submission failed");
        }
    }
    catch (Exception ex)
    {
        await Notification.ShowError("Error: " + ex.Message);
    }
}
```

### GET Form Definition from API

```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        var response = await HttpClient.GetAsync("/api/forms/definition/user-registration");
        response.EnsureSuccessStatusCode();
        myForm = await response.Content.ReadAsAsync<FormDefinition>();
    }
    catch (Exception ex)
    {
        // Handle error
    }
}
```

## Validation Patterns

### Client-Side Validation

```csharp
// Built-in validations
new FormValidationRule { Type = "required", Message = "Required" }
new FormValidationRule { Type = "email", Message = "Invalid email" }
new FormValidationRule { Type = "minLength", Value = "5", Message = "Too short" }
new FormValidationRule { Type = "pattern", Value = @"^\d{3}-\d{3}-\d{4}$", Message = "Invalid format" }
```

### Server-Side Validation

```csharp
private async Task HandleSubmit(FormSubmissionResult result)
{
    if (!result.IsSuccess)
    {
        // Show validation errors
        foreach (var error in result.FieldErrors)
        {
            await Notification.ShowError($"{error.Key}: {error.Value}");
        }
        return;
    }

    // Additional server-side validation
    try
    {
        var email = result.Data["email"].ToString();
        var existingUser = await UserService.GetByEmailAsync(email);
        if (existingUser != null)
        {
            await Notification.ShowError("Email already registered");
            return;
        }

        // Proceed with submission
        await SubmitFormData(result.Data);
    }
    catch (Exception ex)
    {
        await Notification.ShowError("Validation error: " + ex.Message);
    }
}
```

## Error Handling

### Comprehensive Error Handling

```blazor
@page "/form"
@using SmartWorkz.Web.Models

<div class="alert alert-danger" role="alert" hidden="@(!hasError)">
    <strong>Error!</strong> @errorMessage
</div>

<FormBuilderComponent FormDef="myForm" 
                      OnSubmit="HandleSubmit"
                      OnCancel="HandleCancel" />

@code {
    private FormDefinition myForm;
    private bool hasError;
    private string errorMessage;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            myForm = await LoadFormDefinition();
            hasError = false;
        }
        catch (Exception ex)
        {
            hasError = true;
            errorMessage = "Failed to load form: " + ex.Message;
        }
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        try
        {
            if (!result.IsSuccess)
            {
                hasError = true;
                errorMessage = result.Message ?? "Validation failed";
                return;
            }

            var response = await HttpClient.PostAsJsonAsync("/api/submit", result.Data);
            
            if (response.IsSuccessStatusCode)
            {
                await Navigation.NavigateTo("/success");
            }
            else
            {
                hasError = true;
                errorMessage = $"Server error: {response.StatusCode}";
            }
        }
        catch (HttpRequestException ex)
        {
            hasError = true;
            errorMessage = "Network error: " + ex.Message;
        }
        catch (Exception ex)
        {
            hasError = true;
            errorMessage = "Unexpected error: " + ex.Message;
        }
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }

    private async Task<FormDefinition> LoadFormDefinition()
    {
        // Load from API or create manually
        throw new NotImplementedException();
    }
}
```

## Advanced Scenarios

### Multi-Step Form

```blazor
@page "/multi-step-form"

@if (currentStep == 1)
{
    <FormBuilderComponent FormDef="step1Form" 
                          OnSubmit="HandleStep1Submit"
                          OnCancel="HandleCancel" />
}
else if (currentStep == 2)
{
    <FormBuilderComponent FormDef="step2Form" 
                          OnSubmit="HandleStep2Submit"
                          OnCancel="HandleCancel" />
}
else if (currentStep == 3)
{
    <FormBuilderComponent FormDef="step3Form" 
                          OnSubmit="HandleStep3Submit"
                          OnCancel="HandleCancel" />
}

<div class="progress">
    <div class="progress-bar" style="width: @((currentStep / 3) * 100)%">
        Step @currentStep of 3
    </div>
</div>

@code {
    private int currentStep = 1;
    private FormDefinition step1Form;
    private FormDefinition step2Form;
    private FormDefinition step3Form;
    private Dictionary<string, object> allStepData = new();

    protected override void OnInitialized()
    {
        step1Form = BuildStep1Form();
        step2Form = BuildStep2Form();
        step3Form = BuildStep3Form();
    }

    private async Task HandleStep1Submit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            allStepData = result.Data;
            currentStep = 2;
        }
    }

    private async Task HandleStep2Submit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            foreach (var kvp in result.Data)
                allStepData[kvp.Key] = kvp.Value;
            currentStep = 3;
        }
    }

    private async Task HandleStep3Submit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            foreach (var kvp in result.Data)
                allStepData[kvp.Key] = kvp.Value;
            
            // Submit all data
            await SubmitAllSteps(allStepData);
        }
    }

    private async Task SubmitAllSteps(Dictionary<string, object> data)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("/api/multi-step-form", data);
            if (response.IsSuccessStatusCode)
            {
                await Navigation.NavigateTo("/success");
            }
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }

    private async Task HandleCancel()
    {
        currentStep = 1;
        allStepData.Clear();
        await Navigation.NavigateTo("/");
    }

    private FormDefinition BuildStep1Form() => new() { /* ... */ };
    private FormDefinition BuildStep2Form() => new() { /* ... */ };
    private FormDefinition BuildStep3Form() => new() { /* ... */ };
}
```

### Dynamic Form Loading

```blazor
@page "/dynamic-form/{formId}"

@if (myForm == null)
{
    <p>Loading form...</p>
}
else
{
    <FormBuilderComponent FormDef="myForm" 
                          OnSubmit="HandleSubmit"
                          OnCancel="HandleCancel" />
}

@code {
    [Parameter]
    public string FormId { get; set; }

    private FormDefinition myForm;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"/api/forms/{FormId}");
            response.EnsureSuccessStatusCode();
            myForm = await response.Content.ReadAsAsync<FormDefinition>();
        }
        catch (Exception ex)
        {
            await Notification.ShowError("Failed to load form");
        }
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            await HttpClient.PostAsJsonAsync($"/api/forms/{FormId}/submit", result.Data);
        }
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }
}
```

### Form with Default Values

```csharp
// Pre-populate form from existing data
private async Task InitializeFormWithUserData(int userId)
{
    var user = await UserService.GetAsync(userId);
    
    myForm = new FormDefinition
    {
        Fields = new List<FormField>
        {
            new FormField
            {
                Name = "firstName",
                Label = "First Name",
                Type = "text",
                Value = user.FirstName // Set default value
            },
            new FormField
            {
                Name = "email",
                Label = "Email",
                Type = "email",
                Value = user.Email
            }
        }
    };
}
```

## Testing Forms

### Unit Testing FormBuilder

```csharp
[TestClass]
public class FormBuilderTests
{
    [TestMethod]
    public void Form_WithRequiredField_ShouldValidate()
    {
        // Arrange
        var form = new FormDefinition
        {
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "email",
                    IsRequired = true,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required" }
                    }
                }
            }
        };

        // Act
        var validator = new FormValidator();
        var result = validator.Validate(form, new Dictionary<string, object>());

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.ContainsKey("email"));
    }
}
```

## Performance Considerations

1. **Lazy load large forms** - Load form definition only when needed
2. **Cache form definitions** - Store frequently used forms in memory
3. **Pagination** - For forms with many fields, use multi-step approach
4. **Async operations** - Always use async/await for I/O
5. **Debounce validation** - Validate on submit, not every keystroke

## Security Considerations

1. **Validate server-side** - Never trust client-side validation alone
2. **Sanitize input** - Clean user input before storage
3. **CSRF protection** - Use Blazor's built-in CSRF tokens
4. **Rate limiting** - Limit form submissions per user/IP
5. **Audit logging** - Log all form submissions
6. **Input constraints** - Enforce maxLength and type constraints
