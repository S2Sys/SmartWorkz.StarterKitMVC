# Getting Started — Installation & Quick Start

Get SmartWorkz Starter Kit running in 5 steps.

## Prerequisites

- **.NET 9 SDK** — [Download](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- **SQL Server / LocalDB** — Included with VS 2022, or download [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
- **Visual Studio 2022 / VS Code** — Community edition or higher

## Quick Start (5 Steps)

**Step 1: Clone the repository**

```bash
git clone https://github.com/S2Sys/SmartWorkz.StarterKitMVC.git
cd SmartWorkz.StarterKitMVC
```

**Step 2: Create appsettings.json**

Create or update `src/SmartWorkz.StarterKitMVC.Admin/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartWorkzDb;Trusted_Connection=true;"
  },
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/app-.txt", "rollingInterval": "Day" } }
    ]
  },
  "AllowedHosts": "*"
}
```

**Step 3: Run database migrations**

```bash
cd src/SmartWorkz.StarterKitMVC.Admin
dotnet ef database update --context ApplicationDbContext
```

**Step 4: Start the application**

```bash
dotnet run
```

**Step 5: Verify it works**

Open https://localhost:5001 in your browser. You should see the application home page.

## Verification Checklist

- [ ] Database created in LocalDB
- [ ] Application starts without errors
- [ ] Home page loads in browser
- [ ] Logs appear in `logs/` folder
- [ ] No SQL Server connection errors

## FAQ

**Q: Which database should I use?**  
A: Use LocalDB for development (included with VS 2022). For production, use SQL Server 2019+.

**Q: Can I use PostgreSQL?**  
A: Currently only SQL Server/LocalDB. Migrations exist for EF Core to support other databases.

**Q: What port does it run on?**  
A: HTTPS on 5001, HTTP on 5000. Change in `launchSettings.json` if needed.

**Q: What's the default test account?**  
A: See the seed data section in `Program.cs` for default accounts created on first run.

**Q: Can I remove feature X?**  
A: Most features are optional and can be removed from `Program.cs` dependency injection.

---

**Next:** [02-project-overview.md](./02-project-overview.md)
