# SmartWorkz StarterKitMVC

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 9" />
  <img src="https://img.shields.io/badge/Bootstrap-5.3.3-7952B3?style=for-the-badge&logo=bootstrap" alt="Bootstrap 5.3.3" />
  <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="MIT License" />
</p>

**Enterprise-grade ASP.NET Core MVC boilerplate** with Clean Architecture, plug-and-play features, and modern responsive UI. Accelerate your development with a production-ready foundation.

---

## ✨ Features

### 🏗️ Architecture
- **Clean Architecture** - Domain, Application, Infrastructure, Shared, Web layers
- **SOLID Principles** - Dependency injection, interface segregation
- **Repository Pattern** - Abstracted data access
- **CQRS Ready** - Command/Query separation support

### 🔐 Authentication & Security
- **ASP.NET Identity** - User management, roles, claims
- **JWT Authentication** - Token-based API authentication
- **OAuth Providers** - Google, Microsoft, GitHub, Facebook
- **Two-Factor Auth** - Email, Authenticator app support
- **Rate Limiting** - Protect against abuse

### 🏢 Multi-Tenancy
- **Multiple Strategies** - Subdomain, Header, Query parameter
- **Tenant Isolation** - Data separation per tenant
- **Tenant Branding** - Custom logos, colors per tenant

### 📦 Infrastructure
- **Caching** - Memory cache, Redis support
- **Background Jobs** - InMemory, Hangfire, Quartz
- **Event Bus** - InMemory, RabbitMQ, Azure Service Bus, Kafka
- **File Storage** - Local, Azure Blob, AWS S3
- **Logging** - Serilog, Seq, Application Insights, ElasticSearch

### 📧 Notifications
- **Email** - SMTP, SendGrid
- **SMS** - Twilio
- **Push** - Firebase, OneSignal
- **Real-time** - SignalR

### 🎨 UI/UX
- **Bootstrap 5.3.3** - Latest responsive framework
- **Dark/Light Mode** - Theme toggle with persistence
- **Partial Views** - Reusable UI components
- **Admin Dashboard** - Collapsible sidebar, responsive design
- **Toast Notifications** - Beautiful alerts
- **Loading Overlays** - User feedback

### 🛠️ Developer Experience
- **Swagger/OpenAPI** - API documentation
- **Health Checks** - Application monitoring
- **XML Documentation** - IntelliSense support
- **Project Rename Scripts** - Easy customization

---

## 🚀 Quick Start

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server) (or LocalDB)
- [Node.js](https://nodejs.org/) (optional, for frontend tooling)

### 1. Clone the Repository

```bash
git clone https://github.com/S2Sys/SmartWorkz.StarterKitMVC.git
cd SmartWorkz.StarterKitMVC
```

### 2. Rename the Project (Optional)

**PowerShell:**
```powershell
.\rename-project.ps1 -NewCompany "YourCompany" -NewProject "YourProject"
```

**Command Prompt:**
```cmd
rename-project.bat YourCompany YourProject
```

### 3. Configure Features

Edit `src/SmartWorkz.StarterKitMVC.Web/appsettings.json` to enable/disable features:

```json
{
  "Features": {
    "Identity": { "Enabled": true },
    "Authentication": {
      "Jwt": { "Enabled": true },
      "OAuth": {
        "Google": { "Enabled": false }
      }
    },
    "Caching": { "Provider": "Memory" },
    "Swagger": { "Enabled": true }
  }
}
```

### 4. Run the Application

```bash
cd src/SmartWorkz.StarterKitMVC.Web
dotnet run
```

### 5. Access the Application

| URL | Description |
|-----|-------------|
| https://localhost:5001 | Home Page |
| https://localhost:5001/Admin/Dashboard | Admin Panel |
| https://localhost:5001/swagger | API Documentation |
| https://localhost:5001/health | Health Check |

---

## 📁 Project Structure

```
SmartWorkz.StarterKitMVC/
├── 📂 src/
│   ├── 📂 SmartWorkz.StarterKitMVC.Domain/        # Entities, Value Objects
│   ├── 📂 SmartWorkz.StarterKitMVC.Application/   # Services, Interfaces, DTOs
│   ├── 📂 SmartWorkz.StarterKitMVC.Infrastructure/# Data Access, External Services
│   ├── 📂 SmartWorkz.StarterKitMVC.Shared/        # Extensions, Utilities
│   └── 📂 SmartWorkz.StarterKitMVC.Web/           # MVC Controllers, Views, API
│       ├── 📂 Controllers/
│       ├── 📂 Views/
│       │   └── 📂 Shared/                         # Layouts, Partial Views
│       ├── 📂 wwwroot/
│       │   ├── 📂 css/                            # site.css, admin.css
│       │   └── 📂 js/                             # site.js, admin.js
│       └── appsettings.json                       # Feature Configuration
├── 📂 tests/
│   ├── 📂 SmartWorkz.StarterKitMVC.Tests.Unit/
│   └── 📂 SmartWorkz.StarterKitMVC.Tests.Integration/
├── 📂 docs/                                       # Documentation
├── 📂 tools/                                      # DocGenerator
├── 📂 build/                                      # Build scripts
├── 📂 devops/                                     # CI/CD pipelines
├── 📂 k8s/                                        # Kubernetes manifests
├── Dockerfile
├── docker-compose.yml
└── README.md
```

---

## ⚙️ Configuration

### Plug & Play Features

All features can be toggled via `appsettings.json`. Set `"Enabled": true/false` to activate/deactivate.

| Feature | Default | Providers |
|---------|---------|-----------|
| Identity | ✅ On | ASP.NET Identity |
| JWT | ✅ On | Built-in |
| OAuth | ❌ Off | Google, Microsoft, GitHub, Facebook |
| Multi-Tenancy | ✅ On | Subdomain, Header, Query |
| Caching | ✅ On | Memory, Redis |
| Background Jobs | ✅ On | InMemory, Hangfire, Quartz |
| Event Bus | ✅ On | InMemory, RabbitMQ, Azure, Kafka |
| Email | ✅ On | SMTP, SendGrid |
| SMS | ❌ Off | Twilio |
| Push | ❌ Off | Firebase, OneSignal |
| Storage | ✅ On | Local, Azure Blob, S3 |
| AI | ❌ Off | OpenAI, Azure OpenAI |
| Swagger | ✅ On | Swashbuckle |
| Health Checks | ✅ On | Built-in |
| Rate Limiting | ✅ On | Fixed Window |

### Example: Enable Google OAuth

```json
"OAuth": {
  "Google": {
    "Enabled": true,
    "ClientId": "your-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-client-secret"
  }
}
```

### Example: Switch to Redis Caching

```json
"Caching": {
  "Enabled": true,
  "Provider": "Redis",
  "Redis": {
    "Enabled": true,
    "InstanceName": "MyApp_"
  }
},
"ConnectionStrings": {
  "Redis": "localhost:6379"
}
```

---

## 🎨 UI Components

### Partial Views

| Partial | Description |
|---------|-------------|
| `_Navbar.cshtml` | Responsive navigation with search, theme toggle |
| `_Footer.cshtml` | Footer with links, newsletter |
| `_ToastContainer.cshtml` | Toast notification container |
| `_LoadingOverlay.cshtml` | Full-screen loading spinner |
| `_ConfirmModal.cshtml` | Confirmation dialog |
| `_Pagination.cshtml` | Pagination component |
| `_Alert.cshtml` | Alert with icons |
| `_Card.cshtml` | Card component |
| `_DataTable.cshtml` | Responsive data table |

### JavaScript Utilities (site.js)

```javascript
// Toast notifications
SW.toast.success('Saved successfully!');
SW.toast.error('Something went wrong');

// Loading overlay
SW.loading.show();
SW.loading.hide();

// Confirmation dialog
const confirmed = await SW.confirm.show({
  title: 'Delete Item?',
  message: 'This action cannot be undone.'
});

// AJAX helpers
const result = await SW.http.get('/api/users');
await SW.http.post('/api/users', { name: 'John' });

// Form helpers
SW.form.serialize(form);
SW.form.validate(form);

// Theme toggle
SW.theme.toggle();

// Utilities
SW.utils.debounce(fn, 300);
SW.utils.formatDate(date);
SW.utils.copyToClipboard(text);
```

---

## 🐳 Docker

### Build and Run

```bash
docker build -t starterkitmvc .
docker run -p 8080:80 starterkitmvc
```

### Docker Compose

```bash
docker-compose up -d
```

---

## ☸️ Kubernetes

```bash
kubectl apply -f k8s/
```

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/SmartWorkz.StarterKitMVC.Tests.Unit

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📚 Documentation

- [README.md](README.md) - Project overview and getting started
- [CHANGELOG.md](CHANGELOG.md) - Version history and release notes
- [TODO.md](TODO.md) - Current tasks, bugs, and feature roadmap
- [About & Features](docs/about.md)
- [How to Use](docs/how-to-use.md)
- [API Reference](https://localhost:5001/swagger)

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Bootstrap](https://getbootstrap.com/)
- [Bootstrap Icons](https://icons.getbootstrap.com/)

---

<p align="center">
  Made with ❤️ by <a href="https://github.com/S2Sys">SmartWorkz</a>
</p>