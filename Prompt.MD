# SmartWorkz StarterKitMVC - Complete Feature Prompt

> **Purpose**: This document serves as a comprehensive prompt/specification for building an enterprise-grade ASP.NET Core MVC boilerplate with Clean Architecture, plug-and-play features, and modern responsive UI.

---

## 🎯 Project Overview

Build an **enterprise-grade ASP.NET Core MVC boilerplate** called **SmartWorkz.StarterKitMVC** with the following characteristics:

- **Framework**: .NET 9
- **UI Framework**: Bootstrap 5.3.3
- **Architecture**: Clean Architecture (5-layer)
- **Design Principles**: SOLID, DRY, KISS
- **Pattern**: Repository Pattern, CQRS-ready
- **Approach**: Plug & Play feature configuration via `appsettings.json`

---

## 📁 Solution Structure

Create a solution with the following projects under `src/`:

```
SmartWorkz.StarterKitMVC/
├── src/
│   ├── SmartWorkz.StarterKitMVC.Domain/        # Entities, Value Objects, Domain Events
│   ├── SmartWorkz.StarterKitMVC.Application/   # Services, Interfaces, DTOs, Use Cases
│   ├── SmartWorkz.StarterKitMVC.Infrastructure/# Data Access, External Services, Implementations
│   ├── SmartWorkz.StarterKitMVC.Shared/        # Extensions, Utilities, Primitives
│   └── SmartWorkz.StarterKitMVC.Web/           # MVC Controllers, Views, API, Middleware
├── tests/
│   ├── SmartWorkz.StarterKitMVC.Tests.Unit/
│   └── SmartWorkz.StarterKitMVC.Tests.Integration/
├── docs/                                        # Documentation
├── tools/                                       # DocGenerator, utilities
├── build/                                       # Build scripts
├── devops/                                      # CI/CD pipelines
├── k8s/                                         # Kubernetes manifests
├── Dockerfile
├── docker-compose.yml
└── README.md
```

### Dependency Rules

- **Web** → Application, Infrastructure, Shared
- **Application** → Domain, Shared
- **Infrastructure** → Domain, Shared
- **Domain** → Shared (minimal)
- **Shared** → None (standalone)

---

## 🏗️ Feature Categories

### 1. Cross-Cutting Infrastructure

| Feature | Description |
|---------|-------------|
| **Structured Logging** | Serilog with multiple sinks (Console, File, Seq, Application Insights, ElasticSearch) |
| **Correlation ID** | Request correlation tracking with middleware |
| **OpenTelemetry** | Distributed tracing, metrics, and logging hooks |
| **Resilience** | Retry/timeout policies using Polly |
| **Feature Flags** | Runtime feature toggle abstraction |
| **Audit Logging** | User action tracking abstraction |
| **Config Provider** | Multi-environment configuration support |
| **Background Jobs** | InMemory, Hangfire, Quartz providers |
| **API Versioning** | URL, Header, Query parameter versioning |
| **Localization (i18n)** | Multi-language support with resource files |
| **Local Storage** | SQLite/JSON/SecureStorage abstractions |
| **Plugin/Module System** | Discovery and registration of modules |
| **AI Integration** | OpenAI, Azure OpenAI, Gemini, Claude abstractions |
| **HttpClient Pipeline** | IHttpService with handlers, unified result model |

### 2. Category/SubCategory LoV (List of Values) System

| Feature | Description |
|---------|-------------|
| **Domain Models** | Category, SubCategory, LovItem entities |
| **Hierarchical LoV** | Tree structure, lists, filtering, tags |
| **Localization** | Multi-language support for LoV items |
| **Tenant Override** | Tenant-specific LoV overrides (contracts only) |
| **Caching** | LoV caching abstractions |
| **Dynamic Dropdowns** | Generator service contracts |
| **Admin UI Shell** | Web UI for LoV management |

### 3. Global Settings System

| Feature | Description |
|---------|-------------|
| **Domain Models** | SettingCategory, SettingDefinition, SettingValue |
| **Value Types** | string, int, bool, double, datetime, list<string>, JSON, encrypted string |
| **Override Model** | System → Tenant → User hierarchy |
| **Validation Engine** | Setting validation abstraction |
| **Caching** | Settings caching abstraction |
| **Export/Import** | Settings portability contracts |
| **Environment Override** | Environment-based setting overrides |
| **Admin UI Shell** | Web UI for settings management |

### 4. Identity System

| Feature | Description |
|---------|-------------|
| **Domain Abstractions** | Users, Roles, Claims, Permissions, Profiles |
| **ASP.NET Identity** | Full integration with password policies |
| **JWT Authentication** | Access/refresh token contracts |
| **OAuth Providers** | Google, Microsoft, GitHub, Facebook |
| **Two-Factor Auth** | Email, Authenticator app support |
| **Identity Middleware** | Authentication/authorization integration |
| **Admin UI Shell** | Users, Roles, Claims management views |
| **MVC Pages** | Login, Register, Profile pages skeleton |

### 5. Multi-Tenancy (SaaS-Ready)

| Feature | Description |
|---------|-------------|
| **Tenant Context** | Current tenant resolution and context |
| **Resolver Strategies** | Subdomain, Header, Query parameter |
| **Tenant Settings** | Per-tenant configuration and branding |
| **Feature Flags** | Tenant-specific feature toggles |
| **Data Isolation** | Connection/storage isolation contracts |
| **Admin UI Shell** | Tenant management (disabled by default) |

### 6. Event Bus

| Feature | Description |
|---------|-------------|
| **Event Contracts** | Base event types and interfaces |
| **Publisher/Subscriber** | IEventPublisher, IEventSubscriber |
| **Event Router** | Message routing abstraction |
| **DLQ Strategy** | Dead letter queue contracts |
| **Retry Policies** | Integration with resilience policies |
| **Provider Adapters** | InMemory, RabbitMQ, Azure Service Bus, Kafka |

### 7. Notification Hub

| Feature | Description |
|---------|-------------|
| **Notification Models** | Email, SMS, Push notification types |
| **Templates** | Notification template abstraction |
| **Routing Engine** | Channel routing contracts |
| **Queue Delivery** | Async delivery abstraction |
| **Email Providers** | SMTP, SendGrid |
| **SMS Providers** | Twilio |
| **Push Providers** | Firebase, OneSignal |
| **Real-time** | SignalR integration |
| **Notification Center UI** | Web UI for notifications |

### 8. File Storage

| Feature | Description |
|---------|-------------|
| **Storage Abstraction** | IStorageService interface |
| **Local Storage** | File system storage |
| **Azure Blob** | Azure Blob Storage integration |
| **AWS S3** | Amazon S3 integration |

### 9. Caching

| Feature | Description |
|---------|-------------|
| **Cache Abstraction** | ICacheService interface |
| **Memory Cache** | In-memory caching |
| **Redis** | Distributed Redis caching |

### 10. Email Template Builder

| Feature | Description |
|---------|-------------|
| **Domain Models** | EmailTemplate, EmailTemplateSection, TemplatePlaceholder |
| **Placeholder Types** | Text, HTML, Date, URL, Image, Number, Currency, Boolean, List |
| **System Placeholders** | AppName, AppUrl, UserName, UserEmail, CurrentDate, CurrentYear, CompanyName, SupportEmail, UnsubscribeUrl, LogoUrl, PrivacyUrl, TermsUrl |
| **Reusable Sections** | Header and Footer sections shared across templates |
| **Template Categories** | Organize templates by category (Onboarding, Security, etc.) |
| **Tags** | Tag-based filtering and organization |
| **Live Preview** | Real-time preview with sample data |
| **Clone Templates** | Duplicate existing templates |
| **Import/Export** | JSON-based backup and migration |
| **Local JSON Storage** | Templates saved to App_Data/EmailTemplates/ |
| **Tenant Isolation** | Tenant-specific templates (contracts ready) |
| **Templated Email Sender** | Integration with notification queue |
| **Default Templates** | Welcome Email, Password Reset, Email Verification |
| **Admin UI** | Full CRUD for templates and sections |

---

## 🎨 UI/UX Features

### Bootstrap 5.3.3 Responsive UI

| Component | Description |
|-----------|-------------|
| **Bootstrap 5.3.3** | Latest responsive framework with integrity hashes |
| **Bootstrap Icons 1.11.3** | Icon library |
| **Dark/Light Mode** | Theme toggle with localStorage persistence |
| **CSS Variables** | Customizable theming via CSS custom properties |
| **Animations** | Smooth transitions and hover effects |

### Layouts

| Layout | Description |
|--------|-------------|
| **_Layout.cshtml** | Main responsive layout |
| **_AdminLayout.cshtml** | Admin layout with collapsible sidebar |

### Partial Views

| Partial | Description |
|---------|-------------|
| **_Navbar.cshtml** | Responsive navigation with search, theme toggle |
| **_Footer.cshtml** | Footer with links, newsletter |
| **_ToastContainer.cshtml** | Toast notification container |
| **_LoadingOverlay.cshtml** | Full-screen loading spinner |
| **_ConfirmModal.cshtml** | Confirmation dialog |
| **_Pagination.cshtml** | Pagination component |
| **_Alert.cshtml** | Alert with icons |
| **_Card.cshtml** | Card component |
| **_DataTable.cshtml** | Responsive data table |

### JavaScript Utilities (SW Namespace)

```javascript
// Utilities
SW.utils.debounce(fn, delay)
SW.utils.throttle(fn, delay)
SW.utils.formatDate(date, format)
SW.utils.formatNumber(num)
SW.utils.copyToClipboard(text)
SW.utils.generateId()

// Storage with expiry
SW.storage.set(key, value, expiryMinutes)
SW.storage.get(key)
SW.storage.remove(key)

// Toast notifications
SW.toast.success(message)
SW.toast.error(message)
SW.toast.warning(message)
SW.toast.info(message)

// Loading overlay
SW.loading.show()
SW.loading.hide()

// Confirmation dialogs
const confirmed = await SW.confirm.show({
  title: 'Delete Item?',
  message: 'This action cannot be undone.',
  confirmText: 'Delete',
  cancelText: 'Cancel'
})

// HTTP helpers (fetch wrapper)
const data = await SW.http.get('/api/users')
await SW.http.post('/api/users', { name: 'John' })
await SW.http.put('/api/users/1', { name: 'Jane' })
await SW.http.delete('/api/users/1')

// Form helpers
SW.form.serialize(form)
SW.form.populate(form, data)
SW.form.validate(form)
SW.form.showError(input, message)
SW.form.clearErrors(form)

// Table helpers
SW.table.sort(table, column, direction)
SW.table.filter(table, query)
SW.table.exportCsv(table, filename)

// Theme toggle
SW.theme.toggle()
SW.theme.set('dark' | 'light')
SW.theme.get()

// Event delegation
SW.on('click', '.btn-delete', handler)
```

### Admin Dashboard

| Feature | Description |
|---------|-------------|
| **Collapsible Sidebar** | Responsive sidebar with toggle |
| **Active Menu Highlighting** | Current page indication |
| **Dashboard Shell** | Placeholder for business widgets |
| **Settings Management** | Settings admin views |
| **LoV Management** | LoV admin views |
| **Identity Admin** | Users/Roles/Claims views |
| **Notification Center** | Notification management views |
| **Theme Designer** | Theme customization shell |

---

## ⚙️ Plug & Play Configuration

All features are configured via `appsettings.json` with `"Enabled": true/false` toggles:

```json
{
  "Features": {
    "Identity": {
      "Enabled": true,
      "Provider": "AspNetIdentity",
      "RequireConfirmedEmail": false,
      "PasswordPolicy": {
        "RequireDigit": true,
        "RequireLowercase": true,
        "RequireUppercase": true,
        "RequireNonAlphanumeric": false,
        "MinimumLength": 8
      }
    },
    "Authentication": {
      "Jwt": {
        "Enabled": true,
        "Secret": "your-secret-key-min-32-chars",
        "Issuer": "SmartWorkz",
        "Audience": "StarterKitMVC",
        "ExpiryMinutes": 60,
        "RefreshTokenExpiryDays": 7
      },
      "OAuth": {
        "Google": { "Enabled": false, "ClientId": "", "ClientSecret": "" },
        "Microsoft": { "Enabled": false, "ClientId": "", "ClientSecret": "" },
        "GitHub": { "Enabled": false, "ClientId": "", "ClientSecret": "" },
        "Facebook": { "Enabled": false, "ClientId": "", "ClientSecret": "" }
      },
      "TwoFactor": { "Enabled": false, "Provider": "Email" }
    },
    "MultiTenancy": {
      "Enabled": true,
      "Strategy": "Subdomain",
      "DefaultTenantId": "default"
    },
    "Caching": {
      "Enabled": true,
      "Provider": "Memory",
      "Redis": { "Enabled": false, "InstanceName": "StarterKit" }
    },
    "Logging": {
      "Enabled": true,
      "Provider": "Serilog",
      "Seq": { "Enabled": false, "ServerUrl": "" },
      "ApplicationInsights": { "Enabled": false, "ConnectionString": "" },
      "ElasticSearch": { "Enabled": false, "Uri": "" }
    },
    "BackgroundJobs": {
      "Enabled": true,
      "Provider": "InMemory",
      "Hangfire": { "Enabled": false, "Dashboard": true },
      "Quartz": { "Enabled": false }
    },
    "EventBus": {
      "Enabled": true,
      "Provider": "InMemory",
      "RabbitMQ": { "Enabled": false, "HostName": "localhost" },
      "AzureServiceBus": { "Enabled": false, "ConnectionString": "" },
      "Kafka": { "Enabled": false, "BootstrapServers": "" }
    },
    "Notifications": {
      "Enabled": true,
      "Email": {
        "Enabled": true,
        "Provider": "Smtp",
        "SendGrid": { "Enabled": false, "ApiKey": "" }
      },
      "Sms": {
        "Enabled": false,
        "Provider": "Twilio",
        "Twilio": { "AccountSid": "", "AuthToken": "", "FromNumber": "" }
      },
      "Push": {
        "Enabled": false,
        "Provider": "Firebase",
        "Firebase": { "ServerKey": "" },
        "OneSignal": { "AppId": "", "ApiKey": "" }
      },
      "SignalR": { "Enabled": true }
    },
    "Storage": {
      "Enabled": true,
      "Provider": "Local",
      "Azure": { "Enabled": false, "ConnectionString": "", "ContainerName": "uploads" },
      "S3": { "Enabled": false, "AccessKey": "", "SecretKey": "", "BucketName": "", "Region": "us-east-1" }
    },
    "AI": {
      "Enabled": false,
      "Provider": "OpenAI",
      "OpenAI": { "ApiKey": "", "Model": "gpt-4" },
      "AzureOpenAI": { "Enabled": false, "Endpoint": "", "ApiKey": "", "DeploymentName": "" }
    },
    "ApiVersioning": { "Enabled": true, "DefaultVersion": "1.0" },
    "RateLimiting": { "Enabled": true, "RequestsPerMinute": 100 },
    "HealthChecks": { "Enabled": true, "UI": true },
    "Swagger": { "Enabled": true, "Title": "StarterKitMVC API", "Version": "v1" },
    "Localization": {
      "Enabled": true,
      "DefaultCulture": "en-US",
      "SupportedCultures": ["en-US", "es-ES", "fr-FR", "de-DE"]
    },
    "Compression": { "Enabled": true },
    "Cors": { "Enabled": true, "AllowedOrigins": ["*"] },
    "Security": {
      "Https": { "Enabled": true },
      "Hsts": { "Enabled": true },
      "XssProtection": { "Enabled": true }
    }
  }
}
```

---

## 🛠️ Developer Experience

### Extension Libraries (Shared Layer)

| Extension | Methods |
|-----------|---------|
| **DateTimeExtensions** | ToRelativeTime, StartOfDay, EndOfDay, IsWeekend, etc. |
| **StringExtensions** | ToSlug, Truncate, ToTitleCase, IsNullOrEmpty, etc. |
| **CollectionExtensions** | IsNullOrEmpty, ForEach, DistinctBy, Chunk, etc. |
| **EnumExtensions** | GetDescription, ToList, Parse, etc. |
| **ObjectExtensions** | ToJson, Clone, IsNull, etc. |
| **FileHelper** | Cross-platform file operations |
| **JsonExtensions** | Serialize, Deserialize, Pretty print |
| **ValidationExtensions** | IsEmail, IsUrl, IsPhone, etc. |

### Documentation

| Item | Description |
|------|-------------|
| **XML Comments** | IntelliSense support on all public APIs |
| **Code Examples** | Usage examples in XML comments |
| **DocGenerator Tool** | Automated documentation generation |
| **How-to-Use Guide** | Step-by-step usage documentation |

### Rename Scripts

| Script | Description |
|--------|-------------|
| **rename-project.ps1** | PowerShell script to rename company/project |
| **rename-project.bat** | Batch script wrapper |

---

## 🐳 DevOps & Deployment

### Docker

| File | Description |
|------|-------------|
| **Dockerfile** | Multi-stage build for production |
| **docker-compose.yml** | Local development with dependencies |

### CI/CD Pipelines

| File | Description |
|------|-------------|
| **GitHub Actions** | `.github/workflows/` pipeline YAML |
| **Azure DevOps** | `devops/azure-pipelines.yml` |

### Kubernetes

| Manifest | Description |
|----------|-------------|
| **Deployment** | Application deployment |
| **Service** | Load balancer service |
| **ConfigMap** | Configuration |
| **Secrets** | Sensitive data |
| **Ingress** | HTTP routing |

### Health Checks

| Endpoint | Description |
|----------|-------------|
| `/health` | Basic health check |
| `/health/ready` | Readiness probe |
| `/health/live` | Liveness probe |

---

## 🧪 Testing

### Test Projects

| Project | Description |
|---------|-------------|
| **Tests.Unit** | Unit tests with xUnit |
| **Tests.Integration** | Integration tests |

### Test Scaffolding

- Localization tests
- Multi-tenant tests
- Identity tests
- Plugin/module tests

### Quality

| Tool | Description |
|------|-------------|
| **Static Analysis** | .editorconfig, analyzers |
| **Coding Standards** | `docs/coding-standards.md` |

---

## 📊 Feature Summary Table

| Feature | Default | Providers |
|---------|---------|-----------|
| Identity | ✅ On | ASP.NET Identity |
| JWT Auth | ✅ On | Built-in |
| OAuth | ❌ Off | Google, Microsoft, GitHub, Facebook |
| Two-Factor | ❌ Off | Email, Authenticator |
| Multi-Tenancy | ✅ On | Subdomain, Header, Query |
| Caching | ✅ On | Memory, Redis |
| Logging | ✅ On | Serilog, Seq, AppInsights, ElasticSearch |
| Background Jobs | ✅ On | InMemory, Hangfire, Quartz |
| Event Bus | ✅ On | InMemory, RabbitMQ, Azure, Kafka |
| Email | ✅ On | SMTP, SendGrid |
| SMS | ❌ Off | Twilio |
| Push | ❌ Off | Firebase, OneSignal |
| SignalR | ✅ On | Built-in |
| Storage | ✅ On | Local, Azure Blob, S3 |
| AI | ❌ Off | OpenAI, Azure OpenAI |
| API Versioning | ✅ On | URL, Header, Query |
| Rate Limiting | ✅ On | Fixed Window |
| Health Checks | ✅ On | Built-in |
| Swagger | ✅ On | Swashbuckle |
| Localization | ✅ On | Resource files |
| Compression | ✅ On | Gzip, Brotli |
| CORS | ✅ On | Configurable |
| Email Templates | ✅ On | Local JSON Storage |

---

## 🚀 Quick Start Commands

```bash
# Clone
git clone https://github.com/S2Sys/SmartWorkz.StarterKitMVC.git
cd SmartWorkz.StarterKitMVC

# Rename (optional)
.\rename-project.ps1 -NewCompany "YourCompany" -NewProject "YourProject"

# Run
cd src/SmartWorkz.StarterKitMVC.Web
dotnet run

# Access
# Home: https://localhost:5001
# Admin: https://localhost:5001/Admin/Dashboard
# API Docs: https://localhost:5001/swagger
# Health: https://localhost:5001/health

# Docker
docker build -t starterkitmvc .
docker run -p 8080:80 starterkitmvc

# Docker Compose
docker-compose up -d

# Kubernetes
kubectl apply -f k8s/

# Tests
dotnet test
```

---

## 📝 Notes

- This boilerplate is **domain-agnostic** - no business CRUD logic included
- All features are **contracts/abstractions first** with implementations
- **Tenant-aware** but multi-tenancy is optional
- **Mobile/desktop-safe** contracts for future clients
- Focus on **reusable enterprise-grade infrastructure**

---

*Generated for SmartWorkz StarterKitMVC v1.0.0 | .NET 9 | Bootstrap 5.3.3*
