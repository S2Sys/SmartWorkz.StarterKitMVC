# FormBuilder Practical Examples

Collection of complete, ready-to-use FormBuilder implementations for common scenarios.

## Table of Contents

1. [Contact Form](#contact-form)
2. [User Registration](#user-registration)
3. [Product Feedback Survey](#product-feedback-survey)
4. [Job Application](#job-application)
5. [Newsletter Signup](#newsletter-signup)
6. [Expense Report](#expense-report)

## Contact Form

Simple contact form for website visitors to send messages.

**Use Case:** Website contact page, inquiry submission

**Features:**
- Name validation (required, minLength)
- Email validation
- Message validation (required, minLength, maxLength)
- Success notification

### Component

```blazor
@page "/contact"
@inject IEmailService EmailService
@inject NavigationManager Navigation
@inject NotificationService Notification
@using SmartWorkz.Web.Models

<div class="container mt-5 mb-5">
    <FormBuilderComponent FormDef="contactForm" 
                          OnSubmit="HandleSubmit"
                          OnCancel="HandleCancel" />
</div>

@code {
    private FormDefinition contactForm;

    protected override void OnInitialized()
    {
        contactForm = new FormDefinition
        {
            Title = "Contact Us",
            Description = "We'd love to hear from you. Send us a message and we'll respond within 24 hours.",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "name",
                    Label = "Full Name",
                    Type = "text",
                    IsRequired = true,
                    Placeholder = "John Doe",
                    HelpText = "Your full name",
                    Order = 1,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Name is required" },
                        new FormValidationRule { Type = "minLength", Value = "3", Message = "Minimum 3 characters" }
                    }
                },
                new FormField
                {
                    Name = "email",
                    Label = "Email Address",
                    Type = "email",
                    IsRequired = true,
                    Placeholder = "john@example.com",
                    HelpText = "We'll use this to respond to you",
                    Order = 2,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Email is required" },
                        new FormValidationRule { Type = "email", Message = "Invalid email format" }
                    }
                },
                new FormField
                {
                    Name = "subject",
                    Label = "Subject",
                    Type = "text",
                    IsRequired = true,
                    Placeholder = "Inquiry about...",
                    Order = 3,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Subject is required" }
                    }
                },
                new FormField
                {
                    Name = "message",
                    Label = "Message",
                    Type = "textarea",
                    IsRequired = true,
                    Placeholder = "Please tell us...",
                    HelpText = "Maximum 1000 characters",
                    Order = 4,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Message is required" },
                        new FormValidationRule { Type = "minLength", Value = "10", Message = "Minimum 10 characters" },
                        new FormValidationRule { Type = "maxLength", Value = "1000", Message = "Maximum 1000 characters" }
                    }
                }
            },
            SubmitConfig = new FormSubmitConfig
            {
                SubmitButtonText = "Send Message",
                ShowCancelButton = true,
                CancelButtonText = "Cancel"
            }
        };
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (!result.IsSuccess)
        {
            await Notification.ShowError("Please fix the errors in the form");
            return;
        }

        try
        {
            var name = result.Data["name"].ToString();
            var email = result.Data["email"].ToString();
            var subject = result.Data["subject"].ToString();
            var message = result.Data["message"].ToString();

            // Send email
            await EmailService.SendContactEmailAsync(name, email, subject, message);
            
            await Notification.ShowSuccess("Thank you! We've received your message and will respond soon.");
            await Navigation.NavigateTo("/");
        }
        catch (Exception ex)
        {
            await Notification.ShowError("Failed to send message: " + ex.Message);
        }
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }
}
```

## User Registration

Complete user registration form with password validation and terms acceptance.

**Use Case:** New user signup, account creation

**Features:**
- Email uniqueness validation
- Password strength validation
- Password confirmation
- Terms acceptance checkbox
- Conditional marketing preference

### Component

```blazor
@page "/register"
@inject IUserService UserService
@inject NavigationManager Navigation
@inject NotificationService Notification
@using SmartWorkz.Web.Models

<div class="container mt-5 mb-5">
    <FormBuilderComponent FormDef="registrationForm" 
                          OnSubmit="HandleSubmit"
                          OnCancel="HandleCancel" />
</div>

@code {
    private FormDefinition registrationForm;

    protected override void OnInitialized()
    {
        registrationForm = new FormDefinition
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
                        new FormValidationRule { Type = "minLength", Value = "2", Message = "Minimum 2 characters" }
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
                    Name = "confirmPassword",
                    Label = "Confirm Password",
                    Type = "password",
                    IsRequired = true,
                    Order = 5,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Password confirmation is required" }
                    }
                },
                new FormField
                {
                    Name = "agreeToTerms",
                    Label = "I agree to the Terms of Service and Privacy Policy",
                    Type = "checkbox",
                    IsRequired = true,
                    Order = 6,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "You must agree to continue" }
                    }
                },
                new FormField
                {
                    Name = "marketingOptIn",
                    Label = "I'd like to receive marketing emails and updates",
                    Type = "checkbox",
                    IsRequired = false,
                    Order = 7
                }
            },
            SubmitConfig = new FormSubmitConfig
            {
                SubmitButtonText = "Create Account",
                ShowCancelButton = true
            }
        };
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (!result.IsSuccess)
        {
            await Notification.ShowError("Please fix the errors in the form");
            return;
        }

        try
        {
            // Validate password match
            var password = result.Data["password"].ToString();
            var confirmPassword = result.Data["confirmPassword"].ToString();
            if (password != confirmPassword)
            {
                await Notification.ShowError("Passwords do not match");
                return;
            }

            // Check email uniqueness
            var email = result.Data["email"].ToString();
            if (await UserService.EmailExistsAsync(email))
            {
                await Notification.ShowError("This email is already registered");
                return;
            }

            // Create user
            var user = new UserRegistrationModel
            {
                FirstName = result.Data["firstName"].ToString(),
                LastName = result.Data["lastName"].ToString(),
                Email = email,
                Password = password,
                OptInMarketing = (bool)result.Data["marketingOptIn"]
            };

            await UserService.RegisterAsync(user);
            await Notification.ShowSuccess("Account created successfully!");
            await Navigation.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            await Notification.ShowError("Registration failed: " + ex.Message);
        }
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }
}
```

## Product Feedback Survey

Survey form for collecting structured customer feedback.

**Use Case:** Product surveys, customer satisfaction, feature requests

**Features:**
- Rating scale dropdown
- Conditional questions based on rating
- Free text feedback with length validation
- Contact information (optional)

### Component

```blazor
@page "/feedback"
@inject IFeedbackService FeedbackService
@inject NavigationManager Navigation
@using SmartWorkz.Web.Models

<div class="container mt-5 mb-5">
    <FormBuilderComponent FormDef="feedbackForm" 
                          OnSubmit="HandleSubmit"
                          OnCancel="HandleCancel" />
</div>

@code {
    private FormDefinition feedbackForm;

    protected override void OnInitialized()
    {
        feedbackForm = new FormDefinition
        {
            Title = "Product Feedback",
            Description = "Help us improve by sharing your feedback",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "overallRating",
                    Label = "Overall Product Rating",
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
                    },
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Please select a rating" }
                    }
                },
                new FormField
                {
                    Name = "whatWentWell",
                    Label = "What did you like most?",
                    Type = "textarea",
                    IsRequired = true,
                    DependsOn = "overallRating",
                    DependsOnValue = "5",
                    Order = 2,
                    Placeholder = "Tell us what you enjoyed...",
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "minLength", Value = "10", Message = "Minimum 10 characters" }
                    }
                },
                new FormField
                {
                    Name = "improvementArea",
                    Label = "What needs improvement?",
                    Type = "textarea",
                    IsRequired = true,
                    DependsOn = "overallRating",
                    DependsOnValue = "2",
                    Order = 3,
                    Placeholder = "Tell us what we can improve...",
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "minLength", Value = "10", Message = "Minimum 10 characters" }
                    }
                },
                new FormField
                {
                    Name = "feedback",
                    Label = "Additional Comments",
                    Type = "textarea",
                    IsRequired = false,
                    Order = 4,
                    Placeholder = "Any other feedback?",
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "maxLength", Value = "500", Message = "Maximum 500 characters" }
                    }
                },
                new FormField
                {
                    Name = "contactEmail",
                    Label = "Email (optional - if you'd like us to follow up)",
                    Type = "email",
                    IsRequired = false,
                    Order = 5,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "email", Message = "Invalid email format" }
                    }
                }
            },
            SubmitConfig = new FormSubmitConfig
            {
                SubmitButtonText = "Submit Feedback",
                ShowCancelButton = true
            }
        };
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (!result.IsSuccess)
        {
            return;
        }

        var feedback = new FeedbackSubmission
        {
            Rating = int.Parse(result.Data["overallRating"].ToString()),
            Feedback = result.Data["feedback"]?.ToString() ?? "",
            ContactEmail = result.Data["contactEmail"]?.ToString(),
            SubmittedAt = DateTime.UtcNow,
            IpAddress = await GetUserIpAsync()
        };

        await FeedbackService.SaveAsync(feedback);
        await Navigation.NavigateTo("/feedback-thank-you");
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }

    private async Task<string> GetUserIpAsync()
    {
        // Implementation to get user IP
        return "unknown";
    }
}
```

## Job Application

Comprehensive job application form with position selection and experience validation.

**Use Case:** Job application portal, recruitment

**Features:**
- Position selection dropdown
- Experience level assessment
- Conditional required fields based on position
- Resume/attachment handling
- Availability date selection

### Component

```blazor
@page "/apply"
@using SmartWorkz.Web.Models
@inject NavigationManager Navigation
@inject NotificationService Notification

<div class="container mt-5 mb-5">
    <FormBuilderComponent FormDef="applicationForm" 
                          OnSubmit="HandleSubmit"
                          OnCancel="HandleCancel" />
</div>

@code {
    private FormDefinition applicationForm;

    protected override void OnInitialized()
    {
        applicationForm = new FormDefinition
        {
            Title = "Job Application",
            Description = "Apply for an open position at our company",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "position",
                    Label = "Position",
                    Type = "select",
                    IsRequired = true,
                    Order = 1,
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Label = "Software Engineer", Value = "engineer" },
                        new FormFieldOption { Label = "Product Manager", Value = "pm" },
                        new FormFieldOption { Label = "Designer", Value = "designer" },
                        new FormFieldOption { Label = "Sales Representative", Value = "sales" }
                    }
                },
                new FormField
                {
                    Name = "firstName",
                    Label = "First Name",
                    Type = "text",
                    IsRequired = true,
                    Order = 2,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Required" }
                    }
                },
                new FormField
                {
                    Name = "lastName",
                    Label = "Last Name",
                    Type = "text",
                    IsRequired = true,
                    Order = 3,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Required" }
                    }
                },
                new FormField
                {
                    Name = "email",
                    Label = "Email",
                    Type = "email",
                    IsRequired = true,
                    Order = 4,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Required" },
                        new FormValidationRule { Type = "email", Message = "Invalid email" }
                    }
                },
                new FormField
                {
                    Name = "phone",
                    Label = "Phone Number",
                    Type = "text",
                    IsRequired = true,
                    Order = 5,
                    Placeholder = "(555) 123-4567",
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "pattern", Value = @"^\(?[0-9]{3}\)?[-. ]?[0-9]{3}[-. ]?[0-9]{4}$", Message = "Invalid phone format" }
                    }
                },
                new FormField
                {
                    Name = "yearsExperience",
                    Label = "Years of Professional Experience",
                    Type = "number",
                    IsRequired = true,
                    Order = 6,
                    DependsOn = "position",
                    DependsOnValue = "engineer",
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Required" },
                        new FormValidationRule { Type = "min", Value = "1", Message = "Minimum 1 year" }
                    }
                },
                new FormField
                {
                    Name = "availableDate",
                    Label = "Available Start Date",
                    Type = "date",
                    IsRequired = true,
                    Order = 7,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Required" }
                    }
                },
                new FormField
                {
                    Name = "coverLetter",
                    Label = "Cover Letter",
                    Type = "textarea",
                    IsRequired = true,
                    Order = 8,
                    Placeholder = "Tell us why you're a great fit for this position...",
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "minLength", Value = "50", Message = "Minimum 50 characters" },
                        new FormValidationRule { Type = "maxLength", Value = "2000", Message = "Maximum 2000 characters" }
                    }
                }
            },
            SubmitConfig = new FormSubmitConfig
            {
                SubmitButtonText = "Submit Application",
                ShowCancelButton = true
            }
        };
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (!result.IsSuccess)
        {
            await Notification.ShowError("Please complete the form");
            return;
        }

        // Process application
        var application = new JobApplication
        {
            Position = result.Data["position"].ToString(),
            FirstName = result.Data["firstName"].ToString(),
            LastName = result.Data["lastName"].ToString(),
            Email = result.Data["email"].ToString(),
            Phone = result.Data["phone"].ToString(),
            AvailableDate = DateTime.Parse(result.Data["availableDate"].ToString()),
            CoverLetter = result.Data["coverLetter"].ToString(),
            SubmittedAt = DateTime.UtcNow
        };

        await Navigation.NavigateTo("/application-received");
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }
}
```

## Newsletter Signup

Simple newsletter subscription form.

**Use Case:** Email list growth, marketing

**Features:**
- Email-only entry
- Double opt-in pattern
- Interest categories
- Privacy acknowledgment

### Component

```blazor
@page "/subscribe"
@using SmartWorkz.Web.Models

<FormBuilderComponent FormDef="newsletterForm" 
                      OnSubmit="HandleSubmit"
                      OnCancel="HandleCancel" />

@code {
    private FormDefinition newsletterForm;

    protected override void OnInitialized()
    {
        newsletterForm = new FormDefinition
        {
            Title = "Subscribe to Our Newsletter",
            Description = "Get the latest updates delivered to your inbox",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "email",
                    Label = "Email Address",
                    Type = "email",
                    IsRequired = true,
                    Placeholder = "you@example.com",
                    Order = 1,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Email required" },
                        new FormValidationRule { Type = "email", Message = "Invalid email" }
                    }
                },
                new FormField
                {
                    Name = "agreeToPrivacy",
                    Label = "I agree to the Privacy Policy",
                    Type = "checkbox",
                    IsRequired = true,
                    Order = 2,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Required to continue" }
                    }
                }
            },
            SubmitConfig = new FormSubmitConfig
            {
                SubmitButtonText = "Subscribe",
                ShowCancelButton = false
            }
        };
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            // Subscribe user
            var email = result.Data["email"].ToString();
            // Call subscription service
            await Navigation.NavigateTo("/subscription-confirmed");
        }
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }
}
```

## Expense Report

Complex expense report form with dynamic line items and conditional validations.

**Use Case:** Expense management, accounting, business submissions

**Features:**
- Project/cost center selection
- Multiple expense categories
- Receipt attachment notes
- Total calculation
- Manager approval

### Component

```blazor
@page "/expense-report"
@using SmartWorkz.Web.Models

<FormBuilderComponent FormDef="expenseForm" 
                      OnSubmit="HandleSubmit"
                      OnCancel="HandleCancel" />

@code {
    private FormDefinition expenseForm;

    protected override void OnInitialized()
    {
        expenseForm = new FormDefinition
        {
            Title = "Submit Expense Report",
            Description = "Report business expenses for reimbursement",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "reportDate",
                    Label = "Report Date",
                    Type = "date",
                    IsRequired = true,
                    Order = 1
                },
                new FormField
                {
                    Name = "project",
                    Label = "Project",
                    Type = "select",
                    IsRequired = true,
                    Order = 2,
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Label = "Internal", Value = "internal" },
                        new FormFieldOption { Label = "Client A", Value = "clientA" },
                        new FormFieldOption { Label = "Client B", Value = "clientB" }
                    }
                },
                new FormField
                {
                    Name = "category",
                    Label = "Expense Category",
                    Type = "select",
                    IsRequired = true,
                    Order = 3,
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Label = "Travel", Value = "travel" },
                        new FormFieldOption { Label = "Meals", Value = "meals" },
                        new FormFieldOption { Label = "Office Supplies", Value = "supplies" },
                        new FormFieldOption { Label = "Equipment", Value = "equipment" }
                    }
                },
                new FormField
                {
                    Name = "amount",
                    Label = "Amount (USD)",
                    Type = "number",
                    IsRequired = true,
                    Order = 4,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Required" },
                        new FormValidationRule { Type = "min", Value = "0.01", Message = "Must be greater than $0" },
                        new FormValidationRule { Type = "max", Value = "10000", Message = "Exceeds limit" }
                    }
                },
                new FormField
                {
                    Name = "description",
                    Label = "Description",
                    Type = "textarea",
                    IsRequired = true,
                    Order = 5,
                    HelpText = "Provide details about the expense",
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "minLength", Value = "10", Message = "Minimum 10 chars" }
                    }
                },
                new FormField
                {
                    Name = "receiptsAttached",
                    Label = "Receipts Attached",
                    Type = "checkbox",
                    IsRequired = true,
                    Order = 6,
                    ValidationRules = new List<FormValidationRule>
                    {
                        new FormValidationRule { Type = "required", Message = "Receipts required" }
                    }
                }
            },
            SubmitConfig = new FormSubmitConfig
            {
                SubmitButtonText = "Submit Expense Report",
                ShowCancelButton = true
            }
        };
    }

    private async Task HandleSubmit(FormSubmissionResult result)
    {
        if (result.IsSuccess)
        {
            // Process expense report
        }
    }

    private async Task HandleCancel()
    {
        await Navigation.NavigateTo("/");
    }
}
```

These examples can be adapted to your specific needs. Each example demonstrates different validation patterns, conditional logic, and integration approaches.
