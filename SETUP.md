# SmartWorkz StarterKitMVC - Setup Guide

This guide walks you through setting up the SmartWorkz StarterKitMVC application from scratch.

---

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

| Requirement | Version | Download |
|-------------|---------|----------|
| .NET SDK | 9.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/9.0) |
| SQL Server | 2019+ | [Download](https://www.microsoft.com/sql-server) |
| Node.js | 18+ (optional) | [Download](https://nodejs.org/) |
| Git | Latest | [Download](https://git-scm.com/) |

---

## 🚀 Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/S2Sys/SmartWorkz.StarterKitMVC.git
cd SmartWorkz.StarterKitMVC
```

### 2. Database Setup

#### Option A: Using SQL Server Management Studio (SSMS)

1. Open SSMS and connect to your SQL Server instance
2. Create a new database:
   ```sql
   CREATE DATABASE StarterKitMVC;
   GO
   ```
3. Run the SQL scripts in order:
   - `database/001_CreateTables.sql` - Creates all tables
   - `database/002_SeedData.sql` - Seeds initial data

#### Option B: Using Command Line

```bash
# Using sqlcmd (Windows)
sqlcmd -S localhost -d master -Q "CREATE DATABASE StarterKitMVC"
sqlcmd -S localhost -d StarterKitMVC -i database/001_CreateTables.sql
sqlcmd -S localhost -d StarterKitMVC -i database/002_SeedData.sql

# Using Docker SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### 3. Configure Connection String

Edit `src/SmartWorkz.StarterKitMVC.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StarterKitMVC;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

**Connection String Examples:**

| Provider | Connection String |
|----------|-------------------|
| SQL Server (Windows Auth) | `Server=localhost;Database=StarterKitMVC;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Server (SQL Auth) | `Server=localhost;Database=StarterKitMVC;User Id=sa;Password=YourPassword;TrustServerCertificate=True;` |
| LocalDB | `Server=(localdb)\\mssqllocaldb;Database=StarterKitMVC;Trusted_Connection=True;` |
| Azure SQL | `Server=tcp:yourserver.database.windows.net,1433;Database=StarterKitMVC;User ID=admin;Password=YourPassword;Encrypt=True;` |

### 4. Run the Application

```bash
cd src/SmartWorkz.StarterKitMVC.Web
dotnet run
```

Or with hot reload:
```bash
dotnet watch run
```

### 5. Access the Application

| URL | Description |
|-----|-------------|
| https://localhost:5001 | Home Page |
| https://localhost:5001/Admin/Dashboard | Admin Dashboard |
| https://localhost:5001/Account/Login | Login Page |
| https://localhost:5001/swagger | API Documentation |
| https://localhost:5001/health | Health Check |

---

## 🗄️ Database Schema

### Tables Overview

| Table | Description |
|-------|-------------|
| `Users` | User accounts |
| `Roles` | User roles |
| `UserRoles` | User-role mappings |
| `Claims` | User claims |
| `RoleClaims` | Role-claim mappings |
| `Permissions` | System permissions |
| `Tenants` | Multi-tenant organizations |
| `TenantBranding` | Tenant customization |
| `SettingCategories` | Setting categories |
| `SettingDefinitions` | Setting definitions |
| `SettingValues` | Setting values per tenant |
| `LovCategories` | List of Values categories |
| `LovSubCategories` | LOV sub-categories |
| `LovItems` | LOV items |
| `LovItemLocalizations` | LOV translations |
| `NotificationTemplates` | Notification templates |
| `Notifications` | User notifications |
| `AuditLogs` | Audit trail |

### Default Seed Data

After running `002_SeedData.sql`, you'll have:

**Default Tenant:**
- ID: `default`
- Name: `Default Tenant`

**Default Roles:**
- `SuperAdmin` - Full system access
- `Admin` - Administrative access
- `User` - Standard user access

**Default Users:**
- Username: `admin@example.com`
- Password: `Admin@123` (hashed)
- Role: SuperAdmin

**Default Settings:**
- General settings (Site Name, Timezone, etc.)
- Email settings (SMTP configuration)
- Security settings (Password policy, etc.)

---

## ⚙️ Configuration

### Feature Toggles

Edit `appsettings.json` to enable/disable features:

```json
{
  "Features": {
    "Identity": {
      "Enabled": true
    },
    "Authentication": {
      "Jwt": {
        "Enabled": true,
        "Secret": "your-secret-key-min-32-chars",
        "Issuer": "StarterKitMVC",
        "Audience": "StarterKitMVC",
        "ExpiryMinutes": 60
      },
      "OAuth": {
        "Google": {
          "Enabled": false,
          "ClientId": "",
          "ClientSecret": ""
        }
      }
    },
    "MultiTenancy": {
      "Enabled": true,
      "Strategy": "Subdomain"
    },
    "Caching": {
      "Enabled": true,
      "Provider": "Memory"
    },
    "BackgroundJobs": {
      "Enabled": true,
      "Provider": "InMemory"
    },
    "Email": {
      "Enabled": true,
      "Provider": "Smtp",
      "Smtp": {
        "Host": "smtp.example.com",
        "Port": 587,
        "Username": "",
        "Password": "",
        "FromEmail": "noreply@example.com",
        "FromName": "StarterKitMVC"
      }
    },
    "Storage": {
      "Enabled": true,
      "Provider": "Local",
      "Local": {
        "BasePath": "wwwroot/uploads"
      }
    },
    "Swagger": {
      "Enabled": true
    },
    "HealthChecks": {
      "Enabled": true
    },
    "RateLimiting": {
      "Enabled": true,
      "PermitLimit": 100,
      "WindowSeconds": 60
    }
  }
}
```

---

## 🔧 Development

### Project Structure

```
src/
├── SmartWorkz.StarterKitMVC.Domain/        # Entities, Value Objects
├── SmartWorkz.StarterKitMVC.Application/   # Services, Interfaces, DTOs
├── SmartWorkz.StarterKitMVC.Infrastructure/# Data Access, External Services
├── SmartWorkz.StarterKitMVC.Shared/        # Extensions, Utilities
└── SmartWorkz.StarterKitMVC.Web/           # MVC Controllers, Views, API
```

### Building

```bash
# Build all projects
dotnet build

# Build specific project
dotnet build src/SmartWorkz.StarterKitMVC.Web
```

### Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Migrations (if using EF Core)

```bash
# Add migration
dotnet ef migrations add InitialCreate -p src/SmartWorkz.StarterKitMVC.Infrastructure -s src/SmartWorkz.StarterKitMVC.Web

# Update database
dotnet ef database update -p src/SmartWorkz.StarterKitMVC.Infrastructure -s src/SmartWorkz.StarterKitMVC.Web
```

---

## 🐳 Docker

### Build and Run

```bash
# Build image
docker build -t starterkitmvc .

# Run container
docker run -p 8080:80 -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal;Database=StarterKitMVC;..." starterkitmvc
```

### Docker Compose

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

---

## 🔐 Security Checklist

Before deploying to production:

- [ ] Change default admin password
- [ ] Update JWT secret key (min 32 characters)
- [ ] Configure HTTPS/SSL
- [ ] Set secure connection strings
- [ ] Enable rate limiting
- [ ] Configure CORS properly
- [ ] Review and restrict permissions
- [ ] Enable audit logging
- [ ] Set up backup strategy

---

## 📞 Support

- **Documentation:** [docs/](docs/)
- **Issues:** [GitHub Issues](https://github.com/S2Sys/SmartWorkz.StarterKitMVC/issues)
- **Email:** support@smartworkz.com

---

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.
