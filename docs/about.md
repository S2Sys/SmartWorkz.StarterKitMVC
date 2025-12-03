# SmartWorkz StarterKitMVC

## About

**SmartWorkz StarterKitMVC** is an enterprise-grade ASP.NET Core MVC boilerplate designed to accelerate development of scalable, maintainable web applications. It provides a solid foundation with clean architecture, plug-and-play features, and modern UI components.

---

## Version

- **Version**: 1.0.0
- **Framework**: .NET 9
- **UI Framework**: Bootstrap 5.3.3
- **Database**: SQL Server (configurable)
- **Cache**: Redis (optional)

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Web Layer                            │
│  (Controllers, Views, Middleware, API Endpoints)            │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                        │
│  (Services, DTOs, Interfaces, Use Cases)                    │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                           │
│  (Entities, Value Objects, Domain Events)                   │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                      │
│  (Data Access, External Services, Implementations)          │
├─────────────────────────────────────────────────────────────┤
│                      Shared Layer                           │
│  (Extensions, Primitives, Common Utilities)                 │
└─────────────────────────────────────────────────────────────┘
```

---

## Plug & Play Features

All features can be enabled/disabled via `appsettings.json`. The system automatically configures services based on your settings.

### Feature Configuration

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
        "Google": {
          "Enabled": false,
          "ClientId": "",
          "ClientSecret": ""
        },
        "Microsoft": {
          "Enabled": false,
          "ClientId": "",
          "ClientSecret": ""
        },
        "GitHub": {
          "Enabled": false,
          "ClientId": "",
          "ClientSecret": ""
        }
      },
      "TwoFactor": {
        "Enabled": false,
        "Provider": "Email"
      }
    },
    "MultiTenancy": {
      "Enabled": true,
      "Strategy": "Subdomain",
      "DefaultTenantId": "default"
    },
    "Caching": {
      "Enabled": true,
      "Provider": "Memory",
      "Redis": {
        "ConnectionString": "",
        "InstanceName": "StarterKit"
      }
    },
    "Logging": {
      "Enabled": true,
      "Provider": "Serilog",
      "Seq": {
        "Enabled": false,
        "ServerUrl": ""
      },
      "ApplicationInsights": {
        "Enabled": false,
        "ConnectionString": ""
      }
    },
    "BackgroundJobs": {
      "Enabled": true,
      "Provider": "InMemory",
      "Hangfire": {
        "Enabled": false,
        "Dashboard": true
      }
    },
    "EventBus": {
      "Enabled": true,
      "Provider": "InMemory",
      "RabbitMQ": {
        "Enabled": false,
        "HostName": "localhost",
        "UserName": "guest",
        "Password": "guest"
      },
      "AzureServiceBus": {
        "Enabled": false,
        "ConnectionString": ""
      }
    },
    "Notifications": {
      "Enabled": true,
      "Email": {
        "Enabled": true,
        "Provider": "Smtp",
        "SendGrid": {
          "Enabled": false,
          "ApiKey": ""
        }
      },
      "Sms": {
        "Enabled": false,
        "Provider": "Twilio",
        "Twilio": {
          "AccountSid": "",
          "AuthToken": "",
          "FromNumber": ""
        }
      },
      "Push": {
        "Enabled": false,
        "Provider": "Firebase",
        "Firebase": {
          "ServerKey": ""
        }
      }
    },
    "Storage": {
      "Enabled": true,
      "Provider": "Local",
      "Azure": {
        "Enabled": false,
        "ConnectionString": "",
        "ContainerName": "uploads"
      },
      "S3": {
        "Enabled": false,
        "AccessKey": "",
        "SecretKey": "",
        "BucketName": "",
        "Region": "us-east-1"
      }
    },
    "AI": {
      "Enabled": false,
      "Provider": "OpenAI",
      "OpenAI": {
        "ApiKey": "",
        "Model": "gpt-4"
      },
      "AzureOpenAI": {
        "Enabled": false,
        "Endpoint": "",
        "ApiKey": "",
        "DeploymentName": ""
      }
    },
    "ApiVersioning": {
      "Enabled": true,
      "DefaultVersion": "1.0"
    },
    "RateLimiting": {
      "Enabled": true,
      "RequestsPerMinute": 100
    },
    "HealthChecks": {
      "Enabled": true,
      "UI": true
    },
    "Swagger": {
      "Enabled": true,
      "Title": "StarterKitMVC API",
      "Version": "v1"
    },
    "Localization": {
      "Enabled": true,
      "DefaultCulture": "en-US",
      "SupportedCultures": ["en-US", "es-ES", "fr-FR", "de-DE"]
    },
    "Compression": {
      "Enabled": true
    },
    "Cors": {
      "Enabled": true,
      "AllowedOrigins": ["*"]
    }
  }
}
```

---

## How Features Work

### Enabling a Feature

1. Open `appsettings.json`
2. Set `"Enabled": true` for the feature
3. Configure any required settings (API keys, connection strings)
4. Restart the application

### Example: Enable Google OAuth

```json
"OAuth": {
  "Google": {
    "Enabled": true,
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  }
}
```

### Example: Switch to Redis Caching

```json
"Caching": {
  "Enabled": true,
  "Provider": "Redis",
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "StarterKit"
  }
}
```

### Example: Enable Hangfire for Background Jobs

```json
"BackgroundJobs": {
  "Enabled": true,
  "Provider": "Hangfire",
  "Hangfire": {
    "Enabled": true,
    "Dashboard": true
  }
}
```

---

## Project Structure

```
SmartWorkz.StarterKitMVC/
├── src/
│   ├── SmartWorkz.StarterKitMVC.Web/           # Web layer
│   │   ├── Controllers/                         # MVC Controllers
│   │   ├── Views/                               # Razor Views
│   │   ├── Areas/Admin/                         # Admin area
│   │   ├── Middleware/                          # Custom middleware
│   │   ├── wwwroot/                             # Static files
│   │   └── Program.cs                           # App entry point
│   │
│   ├── SmartWorkz.StarterKitMVC.Application/   # Application layer
│   │   ├── Abstractions/                        # Service interfaces
│   │   ├── LoV/                                 # List of Values
│   │   ├── Settings/                            # Settings system
│   │   ├── Identity/                            # Identity contracts
│   │   ├── Events/                              # Event bus
│   │   ├── Notifications/                       # Notification hub
│   │   └── MultiTenancy/                        # Multi-tenant
│   │
│   ├── SmartWorkz.StarterKitMVC.Domain/        # Domain layer
│   │   ├── LoV/                                 # LoV entities
│   │   ├── Settings/                            # Settings entities
│   │   ├── Identity/                            # Identity entities
│   │   └── MultiTenancy/                        # Tenant entities
│   │
│   ├── SmartWorkz.StarterKitMVC.Infrastructure/# Infrastructure
│   │   ├── Http/                                # HTTP client
│   │   ├── Logging/                             # Logging
│   │   ├── Storage/                             # File storage
│   │   ├── Events/                              # Event bus impl
│   │   └── ...                                  # Other implementations
│   │
│   └── SmartWorkz.StarterKitMVC.Shared/        # Shared utilities
│       ├── Extensions/                          # Extension methods
│       └── Primitives/                          # Common types
│
├── tests/
│   ├── SmartWorkz.StarterKitMVC.Tests.Unit/
│   └── SmartWorkz.StarterKitMVC.Tests.Integration/
│
├── docs/                                        # Documentation
├── tools/                                       # Build tools
├── devops/                                      # CI/CD configs
└── k8s/                                         # Kubernetes manifests
```

---

## Quick Start

### 1. Clone & Rename

```powershell
git clone https://github.com/smartworkz/starterkitmvc.git
cd starterkitmvc
.\rename-project.ps1 -NewCompany "YourCompany" -NewProject "YourProject"
```

### 2. Configure Features

Edit `appsettings.json` to enable/disable features as needed.

### 3. Run

```bash
dotnet run --project src/SmartWorkz.StarterKitMVC.Web
```

### 4. Access

- **Home**: https://localhost:5001
- **Admin**: https://localhost:5001/Admin/Dashboard
- **API Docs**: https://localhost:5001/swagger

---

## Key Features Summary

| Feature | Default | Providers |
|---------|---------|-----------|
| Identity | ✅ Enabled | ASP.NET Identity |
| JWT Auth | ✅ Enabled | Built-in |
| OAuth | ❌ Disabled | Google, Microsoft, GitHub |
| Multi-Tenancy | ✅ Enabled | Subdomain, Header, Query |
| Caching | ✅ Enabled | Memory, Redis |
| Background Jobs | ✅ Enabled | InMemory, Hangfire |
| Event Bus | ✅ Enabled | InMemory, RabbitMQ, Azure |
| Notifications | ✅ Enabled | SMTP, SendGrid, Twilio |
| Storage | ✅ Enabled | Local, Azure Blob, S3 |
| AI Integration | ❌ Disabled | OpenAI, Azure OpenAI |
| API Versioning | ✅ Enabled | URL, Header, Query |
| Rate Limiting | ✅ Enabled | Fixed Window |
| Health Checks | ✅ Enabled | Built-in |
| Swagger | ✅ Enabled | Swashbuckle |
| Localization | ✅ Enabled | Resource files |

---

## License

MIT License - See LICENSE file for details.

---

## Support

- **Documentation**: `/docs` folder
- **Issues**: GitHub Issues
- **Email**: support@smartworkz.com

---

© 2024 SmartWorkz. All rights reserved.
