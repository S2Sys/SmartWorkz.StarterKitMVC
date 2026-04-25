# Getting Started — SmartWorkz Starter Kit

## What is SmartWorkz?

SmartWorkz is a comprehensive **.NET 9 starter kit** combining a complete enterprise-grade MVC application with a reusable library suite (SmartWorkz.Core). It demonstrates production-ready patterns: domain-driven design, multi-tenancy, localization, CQRS, pagination, HTMX integration, and export features — all wired up and tested.

## Prerequisites

- **.NET 9 SDK** or later ([download](https://dotnet.microsoft.com/download/dotnet/9.0))
- **SQL Server 2019+** or **LocalDB** (included with Visual Studio)
- **Visual Studio 2022** (Community or higher) OR **VS Code** with C# Dev Kit extension
- **Git** for cloning the repository
- A text editor + terminal for command-line work

## Quick Start (5 Minutes)

### Step 1: Clone the Repository
```bash
git clone https://github.com/S2Sys/SmartWorkz.StarterKitMVC.git
cd SmartWorkz.StarterKitMVC
```

### Step 2: Set Up appsettings.json
Copy the template below into `src/SmartWorkz.StarterKitMVC.Admin/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartWorkz_Dev;Integrated Security=true;TrustServerCertificate=true;",
    "Redis": ""
  },
  "Features": {
    "Authentication": {
      "Jwt": {
        "Issuer": "SmartWorkz",
        "Audience": "SmartWorkz.Users",
        "Secret": "your-super-secret-key-min-32-chars-long!!!!!"
      }
    },
    "Swagger": { "Enabled": true, "Title": "SmartWorkz API" },
    "MultiTenancy": { "Enabled": true }
  },
  "MessageBroker": { "Type": "InMemory" },
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/app-.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

### Step 3: Run Migrations
```bash
cd src/SmartWorkz.StarterKitMVC.Admin
dotnet ef database update --context AuthDbContext
dotnet ef database update --context MasterDbContext
dotnet ef database update --context SharedDbContext
```

Or let the app auto-migrate on first run (see Step 4).

### Step 4: Run the Admin Portal
```bash
dotnet run
```
The app will:
1. Auto-apply any pending database migrations
2. Warm up in-memory caches
3. Start listening on `https://localhost:5001`

### Step 5: Verify
1. Open **https://localhost:5001** in your browser
2. You should see the **SmartWorkz Admin Portal** login page (no certificate warnings if using `TrustServerCertificate=true`)
3. Logs appear in both console and rolling file at `logs/app-20260425.txt`

> **Troubleshooting:**
> - **"Cannot access localdb"** → Ensure `(localdb)\\mssqllocaldb` exists: `sqllocaldb i` lists your LocalDB instances
> - **"Pending migrations"** → Run `dotnet ef database update` for each DbContext
> - **"HTTPS certificate error"** → Set `ASPNETCORE_ENVIRONMENT=Development` in your shell

## Verify It Works — Checklist

- [ ] Clone succeeded, no Git errors
- [ ] appsettings.json exists in Admin folder
- [ ] `dotnet build` completes with 0 errors (warnings are OK)
- [ ] Migrations ran (check logs for "Migration applied")
- [ ] **https://localhost:5001** loads without 500 errors
- [ ] Browser console has no fetch errors (F12 → Console tab)
- [ ] StaticFiles load (check favicon presence)

## Project Structure at a Glance

```
SmartWorkz.StarterKitMVC/
├── src/
│   ├── SmartWorkz.StarterKitMVC.Admin/          ← Run this
│   ├── SmartWorkz.StarterKitMVC.Public/         ← Or this
│   ├── SmartWorkz.StarterKitMVC.Application/    ← Business logic
│   ├── SmartWorkz.StarterKitMVC.Domain/         ← Entities
│   ├── SmartWorkz.StarterKitMVC.Infrastructure/ ← DB, repos, services
│   ├── SmartWorkz.StarterKitMVC.Shared/         ← DTOs, models, base pages
│   └── SmartWorkz.Core*/                        ← Reusable libraries
├── tests/                                        ← Unit & integration tests
├── docs/wiki/                                    ← This wiki
└── docs/superpowers/                             ← Capability documentation

```

## What's Next?

You've successfully run the starter kit. Now:

1. **Understand the architecture** → Read **[Project Overview](02-project-overview.md)**
2. **Learn the Core libraries** → Start with **[SmartWorkz.Core](03-smartworkz-core.md)**, then explore Web, Shared, External, and Mobile
3. **Build your first feature** → Jump to **[Step-by-Step Guide](08-step-by-step-guide.md)**
4. **Deep-dive into patterns** → Explore the [pattern library](#pattern-library-below) for advanced topics

## FAQ

**Q: Can I use SQL Server instead of LocalDB?**
A: Yes. Update `ConnectionStrings:DefaultConnection` to your SQL Server instance. Example: `Server=localhost;Database=SmartWorkz_Dev;Integrated Security=true;TrustServerCertificate=true;`

**Q: How do I run the Public portal instead of Admin?**
A: `cd src/SmartWorkz.StarterKitMVC.Public` and `dotnet run`. It listens on a different port (typically 5002).

**Q: Where are the test accounts?**
A: Seed data is applied during migrations. Check `Infrastructure/Data/Seeders/` folder for default users/roles. (Feature not yet demonstrated in this Getting Started.)

**Q: How do I configure external services (email, payment)?**
A: See the `Features:` block in `appsettings.json` and **[Infrastructure](https://github.com/S2Sys/SmartWorkz.StarterKitMVC)** project source code.

**Q: Can I remove some features (e.g., localization, multi-tenancy)?**
A: Yes, but they're deeply integrated. Start a project without them as safer. Use this kit as reference for **how** they're done, not as a starting template if you don't need them.

---

**Next:** [Project Overview — Architecture & DLL Map](02-project-overview.md)
