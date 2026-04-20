# NuGet Package Publishing Strategy

Guide for publishing SmartWorkz core libraries to NuGet.org or private NuGet feeds.

## Package Versioning Strategy

SmartWorkz follows **Semantic Versioning (SemVer)** for all NuGet packages.

### Version Format: MAJOR.MINOR.PATCH[-PRERELEASE]

- **MAJOR** — Breaking changes to public API
- **MINOR** — New features, backward compatible
- **PATCH** — Bug fixes, internal improvements
- **PRERELEASE** — alpha, beta, rc (e.g., `1.0.0-beta1`)

### Versioning Examples

```
0.1.0          → Initial release
0.2.0          → Add new features (no breaking changes)
0.2.1          → Bug fix
1.0.0          → First stable release
1.1.0          → New features
2.0.0          → Major breaking changes
2.0.0-rc1      → Release candidate
```

## Package Definitions

### SmartWorkz.Core

**Current Version:** 1.0.0

Domain models, entities, and core business logic.

```xml
<Package>
    <PackageId>SmartWorkz.Core</PackageId>
    <Title>SmartWorkz Core</Title>
    <Version>1.0.0</Version>
    <Authors>S2Sys</Authors>
    <Description>Domain-driven design core library with entities, services, and result patterns.</Description>
    <ProjectUrl>https://github.com/S2Sys/SmartWorkz</ProjectUrl>
    <RepositoryUrl>https://github.com/S2Sys/SmartWorkz</RepositoryUrl>
    <License>MIT</License>
    <Tags>domain-driven-design ddd entities services</Tags>
    <TargetFramework>net9.0</TargetFramework>
</Package>
```

**Dependencies:**
- Microsoft.Extensions.DependencyInjection.Abstractions (9.0.0)

### SmartWorkz.Core.Shared

**Current Version:** 1.0.0

Data access, caching, file operations, and shared utilities.

```xml
<Package>
    <PackageId>SmartWorkz.Core.Shared</PackageId>
    <Title>SmartWorkz Core Shared</Title>
    <Version>1.0.0</Version>
    <Description>Shared utilities including database access with Dapper, caching, and file operations.</Description>
    <Tags>data-access caching dapper sqlserver utilities</Tags>
    <TargetFramework>net9.0</TargetFramework>
</Package>
```

**Key Dependencies:**
- SmartWorkz.Core (1.0.0)
- Microsoft.Data.SqlClient (5.1.5)
- Dapper (2.1.15)

### SmartWorkz.Core.Web

**Current Version:** 1.0.0

ASP.NET Core web components, Razor components, and UI utilities.

```xml
<Package>
    <PackageId>SmartWorkz.Core.Web</PackageId>
    <Title>SmartWorkz Core Web</Title>
    <Version>1.0.0</Version>
    <Description>Reusable Blazor/Razor components including Grid, ListView, and DataViewer.</Description>
    <Tags>aspnetcore blazor razor components grid</Tags>
    <TargetFramework>net9.0</TargetFramework>
</Package>
```

**Key Dependencies:**
- SmartWorkz.Core (1.0.0)
- SmartWorkz.Core.Shared (1.0.0)
- Microsoft.AspNetCore.Components (2.3.0)

### SmartWorkz.Core.External

**Current Version:** 1.0.0

Export services for Excel and PDF document generation.

```xml
<Package>
    <PackageId>SmartWorkz.Core.External</PackageId>
    <Title>SmartWorkz Core External</Title>
    <Version>1.0.0</Version>
    <Description>Export services for Excel and PDF generation using ClosedXML and QuestPDF.</Description>
    <Tags>export excel pdf closedxml questpdf</Tags>
    <TargetFramework>net9.0</TargetFramework>
</Package>
```

**Key Dependencies:**
- SmartWorkz.Core.Shared (1.0.0)
- ClosedXML (0.101.0)
- QuestPDF (2024.12.2)

## Publishing Workflow

### Prerequisites

1. **NuGet API Key** — Register on nuget.org and create API key
2. **.csproj Updates** — Ensure package metadata is complete
3. **Version Bump** — Update version in .csproj files
4. **Git Tag** — Create release tag for version

### Step-by-Step Publishing

#### 1. Update .csproj with Package Metadata

```xml
<PropertyGroup>
    <PackageId>SmartWorkz.Core</PackageId>
    <PackageVersion>1.0.0</PackageVersion>
    <Title>SmartWorkz Core</Title>
    <Authors>S2Sys</Authors>
    <PackageDescription>Domain entities and business logic</PackageDescription>
    <RepositoryUrl>https://github.com/S2Sys/SmartWorkz</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageTags>domain-driven-design entities</PackageTags>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

#### 2. Build Package

```bash
dotnet pack SmartWorkz.Core/SmartWorkz.Core.csproj -c Release
```

Output: `bin/Release/SmartWorkz.Core.1.0.0.nupkg`

#### 3. Publish to NuGet

```bash
dotnet nuget push bin/Release/SmartWorkz.Core.1.0.0.nupkg \
    --api-key YOUR_API_KEY \
    --source https://api.nuget.org/v3/index.json
```

#### 4. Create Git Tag

```bash
git tag -a v1.0.0 -m "Release SmartWorkz.Core 1.0.0"
git push origin v1.0.0
```

### Batch Publishing Script

```bash
#!/bin/bash

PROJECTS=(
    "src/SmartWorkz.Core"
    "src/SmartWorkz.Core.Shared"
    "src/SmartWorkz.Core.Web"
    "src/SmartWorkz.Core.External"
)

API_KEY="your-api-key"
VERSION="1.0.0"

for project in "${PROJECTS[@]}"; do
    echo "Publishing $project..."
    dotnet pack "$project/$project.csproj" -c Release
    dotnet nuget push "$project/bin/Release/*.nupkg" \
        --api-key "$API_KEY" \
        --source https://api.nuget.org/v3/index.json
done
```

## Dependency Constraints

### Pinned Versions

```
Dapper:                         >= 2.1.15, < 3.0.0
Microsoft.Data.SqlClient:       >= 5.1.0, < 6.0.0
ClosedXML:                      >= 0.101.0, < 1.0.0
QuestPDF:                       >= 2024.0.0, < 2025.0.0
Microsoft.AspNetCore.*:         >= 2.3.0, < 3.0.0
Microsoft.Extensions.*:         >= 9.0.0, < 10.0.0
```

### Dependency Tree

```
SmartWorkz.Core (no external dependencies except MS.Extensions)
    ↓
SmartWorkz.Core.Shared (depends on Core)
    ↓
SmartWorkz.Core.Web (depends on Core + Core.Shared)
    ↓
SmartWorkz.Core.External (depends on Core.Shared)
```

## Release Checklist

- [ ] Unit tests pass (`dotnet test`)
- [ ] Integration tests pass
- [ ] Documentation updated in README files
- [ ] CHANGELOG.md updated with version and features
- [ ] Version numbers bumped in all .csproj files
- [ ] Package metadata verified (.csproj)
- [ ] API surface reviewed for breaking changes
- [ ] NuGet package built successfully (`dotnet pack`)
- [ ] Package uploaded to NuGet.org
- [ ] Git tag created and pushed
- [ ] GitHub Release created with notes
- [ ] Announcement posted to team/community

## Private NuGet Feed (Alternative)

For internal/private packages:

### Azure Artifacts Setup

```bash
# Add private feed source
dotnet nuget add source \
    "https://pkgs.dev.azure.com/yourorg/yourproject/_packaging/yourfeed/nuget/v3/index.json" \
    -n "YourFeed" \
    -u "myusername" \
    -p "yourpat"
```

### Push to Private Feed

```bash
dotnet nuget push SmartWorkz.Core.1.0.0.nupkg \
    --api-key "anyvalue" \
    --source "https://pkgs.dev.azure.com/yourorg/yourproject/_packaging/yourfeed/nuget/v3/index.json"
```

## Version Bumping Strategy

### When to Bump MAJOR

- Remove public APIs
- Change method signatures (incompatible)
- Change return types
- Major architecture changes

**Example:** SmartWorkz.Core 1.x → 2.0.0

### When to Bump MINOR

- Add new public methods
- Add new classes/interfaces
- Add new features
- Backward compatible enhancements

**Example:** SmartWorkz.Core 1.0.0 → 1.1.0

### When to Bump PATCH

- Bug fixes
- Internal refactoring (no API changes)
- Documentation improvements
- Performance optimizations

**Example:** SmartWorkz.Core 1.0.0 → 1.0.1

## Troubleshooting

### Package Already Exists Error

NuGet packages are immutable. Cannot republish same version.

**Solution:** Bump patch version and republish.

### Missing Dependencies

Ensure all dependencies are listed in .csproj:

```xml
<ItemGroup>
    <PackageReference Include="Dapper" Version="2.1.15" />
</ItemGroup>
```

### API Key Expired

Create new API key at https://www.nuget.org/account/apikeys

## Resources

- [NuGet Official Docs](https://docs.microsoft.com/nuget/)
- [Semantic Versioning](https://semver.org/)
- [Creating NuGet Packages](https://docs.microsoft.com/nuget/create-packages/overview-and-workflow)
- [Package Metadata Reference](https://docs.microsoft.com/nuget/reference/nuspec)
