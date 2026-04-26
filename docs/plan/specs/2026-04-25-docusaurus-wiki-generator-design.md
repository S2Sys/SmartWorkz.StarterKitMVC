# Docusaurus Wiki Generator Design

**Date:** 2026-04-25  
**Status:** Design Review  
**Scope:** Auto-generate wiki from DLL XML comments, reorganize docs into Plan/Framework groups

---

## Overview

Build a Docusaurus-powered wiki system that automatically generates API Reference and Usage Guides from C# DLL XML comments, integrated into the build process via MSBuild. Reorganize existing documentation into semantic groups (Plan and Framework) with intelligent hierarchy.

---

## Architecture

**High-level flow:**
```
DLL XML Comments → Wiki Generator Tool → Markdown Files → Docusaurus Build → Static Site
                                            ↓
                                    docs/framework/ (auto-gen)
                                    docs/plan/ (existing, regrouped)
                                    docs/ (other guides)
```

**Key components:**
1. **Wiki Generator** — C# console app that parses DLL XML and generates markdown
2. **MSBuild Task** — post-build target that triggers generator automatically
3. **Docusaurus Config** — sidebar organization and site setup
4. **Output Structure** — markdown files organized by platform, layer, and feature

---

## Documentation Organization

### Plan Group (`docs/plan/`)

Contains all planning and design documentation:

```
plan/
├── index.md (overview)
├── specs/
│   └── [All files from docs/superpowers/specs/]
└── implementations/
    └── [All files from docs/superpowers/plans/]
```

**Contents:**
- Design specifications (*.md from superpowers/specs/)
- Implementation plans (*.md from superpowers/plans/)
- Indexed and cross-linked for easy navigation

### Framework Group (`docs/framework/`)

Hierarchical organization of frameworks by platform, layer, and feature:

```
framework/
├── index.md (overview of all frameworks)
├── mobile/
│   ├── core/
│   │   ├── database.md (auto-gen from SmartWorkz.Core.Mobile)
│   │   ├── caching.md
│   │   ├── state-management.md
│   │   └── [other features]
│   ├── infrastructure/
│   │   ├── http-client.md
│   │   ├── logging.md
│   │   └── [other features]
│   └── application/
│       ├── viewmodels.md
│       ├── commands.md
│       └── [other features]
└── web/
    ├── core/
    │   ├── database.md
    │   ├── caching.md
    │   └── [other features]
    ├── infrastructure/
    │   ├── services.md
    │   ├── middleware.md
    │   └── [other features]
    └── application/
        ├── controllers.md
        ├── view-models.md
        └── [other features]
```

**Structure:** Platform (Mobile/Web) → Layer (Core/Infrastructure/Application/Domain) → Feature (Database, Caching, Logging, etc.)

### Other Guides (`docs/`)

Existing documentation migrated to Docusaurus:
- DEVELOPER.md
- QUICK_REFERENCE.md
- SECURITY.md
- DEPLOYMENT guides
- Configuration guides
- etc.

---

## Wiki Generator Tool

**Location:** `tools/WikiGenerator/` (new project)

**Type:** .NET 8 Console Application

### Input Processing

**DLL Scanning:**
- Scans `bin/` directories of all solution projects
- Collects paired XML documentation files (`*.xml` alongside DLLs)
- Projects included: SmartWorkz.Core, Core.Web, Core.Mobile, Core.Shared, StarterKitMVC, Sample.ECommerce, Admin, and all related projects

**Configuration File** (`tools/WikiGenerator/config.json`):
```json
{
  "projectMappings": {
    "SmartWorkz.Core.Mobile": { "platform": "mobile", "layers": ["Core", "Infrastructure", "Application"] },
    "SmartWorkz.Core.Web": { "platform": "web", "layers": ["Core", "Infrastructure", "Application"] },
    "SmartWorkz.Core.Shared": { "platform": "shared", "layers": ["Core"] },
    "SmartWorkz.StarterKitMVC": { "platform": "web", "layers": ["Application", "Infrastructure"] }
  },
  "featurePatterns": {
    "Database": ".*\\.Data\\.|.*\\.Repository\\.|.*\\.Migrations",
    "Caching": ".*\\.Caching",
    "Logging": ".*\\.Logging",
    "State Management": ".*\\.State|.*\\.Redux",
    "Http Client": ".*\\.Http",
    "Services": ".*\\.Services",
    "ViewModels": ".*\\.ViewModels",
    "Commands": ".*\\.Commands|.*\\.Queries"
  },
  "outputPath": "docs/"
}
```

### Processing Steps

1. **XML Parsing**
   - Read `*.xml` files using `XDocument`
   - Extract namespaces, types (classes/interfaces), members (methods/properties)
   - Capture: `<summary>`, `<remarks>`, `<param>`, `<returns>`, `<example>` elements

2. **Categorization**
   - Match namespace against feature patterns (regex)
   - Assign to platform and layer from project mapping
   - Group by feature

3. **Markdown Generation** (two formats per namespace)

   **API Reference** (`{feature}-api-reference.md`):
   ```markdown
   # {Feature} API Reference
   
   ## Classes/Interfaces
   
   ### ClassName
   - Namespace: `Full.Namespace`
   - Summary: [from XML]
   - Example: [from <example> tag, if present]
   
   #### Methods
   - `ReturnType MethodName(params)` — [summary]
   ```

   **Usage Guide** (`{feature}-guide.md`):
   ```markdown
   # {Feature} Usage Guide
   
   ## Overview
   [Feature summary from primary namespace]
   
   ## Common Patterns
   [Extracted from <example> tags]
   
   ## API Reference
   [Link to {feature}-api-reference.md]
   ```

4. **Output to Markdown**
   - Write files to `docs/framework/{platform}/{layer}/{feature}/`
   - Create `index.md` for each layer/feature with overview
   - Generate updated `sidebars.js` reflecting new structure

### Error Handling

| Scenario | Behavior |
|----------|----------|
| Missing XML file | Log warning, skip DLL |
| Malformed XML | Log error, skip namespace |
| Unmapped namespace | Log warning (suggests config update) |
| Invalid markdown | Validate before write, fail loudly |
| Build-time failure | Warn, don't block build |

### Output Artifacts

- `docs/framework/` — complete API reference + guides (auto-generated)
- `docs/sidebars.js` — updated sidebar configuration
- `docs/logs/wiki-generation-{timestamp}.log` — generation report

---

## Docusaurus Configuration

**Location:** `docusaurus.config.js` (root)

### Site Config

```javascript
module.exports = {
  title: 'SmartWorkz Documentation',
  tagline: 'Comprehensive framework and platform documentation',
  url: 'https://docs.smartworkz.dev',
  baseUrl: '/',
  staticDirectories: ['static'],
  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          editUrl: 'https://github.com/S2Sys/SmartWorkz.StarterKitMVC/edit/main/',
        },
      },
    ],
  ],
  themeConfig: {
    navbar: {
      title: 'SmartWorkz',
      items: [
        { to: '/docs/plan', label: 'Planning', position: 'left' },
        { to: '/docs/framework', label: 'Framework', position: 'left' },
        { to: '/docs/guides', label: 'Guides', position: 'left' },
      ],
    },
    footer: {
      copyright: `© ${new Date().getFullYear()} S2Sys. Built with Docusaurus.`,
    },
  },
};
```

### Sidebar Config (`sidebars.js`)

**Static sections** (manually maintained):
- Plan (generated with file structure)
- Guides (existing docs)

**Dynamic section** (auto-generated by WikiGenerator):
- Framework (Mobile/Web with auto-discovered layers/features)

```javascript
module.exports = {
  sidebar: [
    {
      label: 'Plan',
      items: [
        { type: 'doc', id: 'plan/index' },
        {
          label: 'Design Specs',
          collapsed: true,
          items: [ /* auto-generated from docs/superpowers/specs/ */ ]
        },
        {
          label: 'Implementation Plans',
          collapsed: true,
          items: [ /* auto-generated from docs/superpowers/plans/ */ ]
        }
      ]
    },
    {
      label: 'Framework',
      items: [
        {
          label: 'Mobile',
          items: [
            {
              label: 'Core Layer',
              items: [ /* auto-generated features */ ]
            },
            {
              label: 'Infrastructure Layer',
              items: [ /* auto-generated features */ ]
            },
            {
              label: 'Application Layer',
              items: [ /* auto-generated features */ ]
            }
          ]
        },
        {
          label: 'Web',
          items: [ /* Same as Mobile */ ]
        },
        {
          label: 'Shared',
          items: [ /* Cross-platform utilities */ ]
        }
      ]
    },
    {
      label: 'Guides',
      items: [
        { type: 'doc', id: 'DEVELOPER' },
        { type: 'doc', id: 'QUICK_REFERENCE' },
        { type: 'doc', id: 'SECURITY' },
        // ... other guides
      ]
    }
  ]
};
```

---

## MSBuild Integration

**File:** `Directory.Build.props` (project root)

**Custom Target: GenerateWiki**

```xml
<Target Name="GenerateWiki" 
        AfterTargets="Build"
        Condition="'$(Configuration)' == 'Release' Or '$(IsLocalBuild)' == 'true'">
  
  <Message Text="Generating wiki from DLL XML comments..." Importance="high" />
  
  <Exec Command="dotnet run --project tools/WikiGenerator -- 
                 --output $(MSBuildProjectDirectory)/docs
                 --log-level info"
        ContinueOnError="true" />
  
  <Message Text="Wiki generation complete." Importance="high" />
</Target>
```

**Behavior:**
- Runs after successful build
- Only on local dev builds (`IsLocalBuild=true`) and Release builds
- Fails gracefully (warns, doesn't block build)
- Generates `docs/framework/`, updates `docs/sidebars.js`

**CI/CD Pipeline:**
- Wiki generation validates markdown syntax
- Changes staged for git commit or manual review
- PR shows generated changes for approval

---

## Build & Deployment

### Local Workflow

1. **Build solution** → MSBuild target triggers WikiGenerator
2. **WikiGenerator runs** → parses DLLs, generates markdown
3. **Docusaurus builds** (manual or CI) → outputs static site to `build/`
4. **Developer reviews** generated changes, commits if correct

### CI/CD Workflow

1. **Build + GenerateWiki** runs in CI
2. **Markdown validation** checks syntax
3. **Docusaurus build** succeeds or fails
4. **Generated files staged** for developer review in PR

---

## Success Criteria

- ✅ Wiki auto-generates on every build without manual intervention
- ✅ API Reference is complete and searchable (all public types/methods documented)
- ✅ Usage Guides include code examples and are beginner-friendly
- ✅ Framework hierarchy is intuitive (Mobile/Web → Layer → Feature)
- ✅ Plan group consolidates all planning docs in one place
- ✅ Docusaurus site is mobile-responsive and has search
- ✅ Generation completes in < 30 seconds (doesn't slow dev cycle)

---

## Migration Plan (High-Level)

1. Create WikiGenerator tool and MSBuild task
2. Migrate existing docs to Docusaurus structure
3. Move `docs/superpowers/specs/` and `docs/superpowers/plans/` to `docs/plan/`
4. Run generator, review output
5. Commit design and generated wiki
6. Set up Docusaurus site locally and in CI

---

## Known Unknowns / Future Iterations

- **Versioning:** Should wiki track multiple versions of frameworks? (Deferred to Phase 3)
- **Search:** Docusaurus includes search; expand to external search (Algolia) if needed later
- **Analytics:** Track which docs are most accessed (future enhancement)
- **Multi-language:** Internationalization support (deferred)

---

## Appendix: Feature Pattern Examples

Based on project structure, typical patterns:

| Feature | Namespace Pattern |
|---------|-------------------|
| Database | `*.Data`, `*.Repository`, `*.Migrations` |
| Caching | `*.Caching` |
| Logging | `*.Logging` |
| State Management | `*.State`, `*.Redux` |
| HTTP Client | `*.Http`, `*.HttpClient` |
| Services | `*.Services` |
| ViewModels | `*.ViewModels` |
| Commands/Queries | `*.Commands`, `*.Queries` |
| Webhooks | `*.Webhooks` |
| Authentication | `*.Auth`, `*.Identity` |

(Configurable in `config.json`)
