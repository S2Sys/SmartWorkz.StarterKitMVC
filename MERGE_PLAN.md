# SmartWorkz Consolidation Merge Plan

**Date:** 2026-05-09
**Goal:** Merge all unmerged worktree branches into main, delete root duplicate projects, clean up worktrees.

---

## Current Repository State

### Root-Level Duplicate Projects (to delete after merge)
| Project | Root Files | Main Files | Decision |
|---------|-----------|-----------|----------|
| SmartWorkz.Core.External | 0 (empty) | Complete | DELETE root |
| SmartWorkz.Core.Shared | 14 basic | 199 complete | DELETE root |
| SmartWorkz.Core.Mobile | 28 basic | 265 in MAUI | DELETE root |
| SmartWorkz.Core.Web | 20 (GraphQL+Blazor) | 82 (TagHelpers) | MIGRATE GraphQL, keep root for Blazor or DELETE |

### Worktrees — Merge Status
| Worktree | Branch | Commits Ahead | Status |
|----------|--------|---------------|--------|
| phase1-mobile-location | feature/phase1-mobile-location-service | 0 | ALREADY MERGED |
| phase2-mobile-camera | feature/phase2-mobile-camera-service | 0 | ALREADY MERGED |
| phase3-mobile-biometric | worktree-phase3-mobile-biometric | 0 | ALREADY MERGED |
| phase3-mobile-biometric-android | worktree-phase3-mobile-biometric-android | 0 | ALREADY MERGED |
| phase3-mobile-biometric-windows | worktree-phase3-mobile-biometric-windows | 0 | ALREADY MERGED |
| phase3-mobile-filepicker | worktree-phase3-mobile-filepicker | 0 | ALREADY MERGED |
| phase4-redis-cqrs-observability | worktree-phase4-redis-cqrs-observability | 5 | NEEDS MERGE |
| feat-p1-critical-gaps | feat/p1-critical-gaps | 17 | NEEDS MERGE |
| core-documentation | worktree-core-documentation | 519 | NEEDS MERGE |
| phase4-biometric-platform-tests | worktree-phase4-biometric-platform-tests | 515 | NEEDS MERGE |
| phase4-biometric-tests | worktree-phase4-biometric-tests | 515 | NEEDS MERGE |
| phase4-bluetooth-refinements | worktree-phase4-bluetooth-refinements | 515 | NEEDS MERGE |
| phase4-bluetooth-service-interface | worktree-phase4-bluetooth-service-interface | 515 | NEEDS MERGE |
| smartworkz-mobile-phases | worktree-smartworkz-mobile-phases | 498 | NEEDS MERGE |
| component-libraries | feature/component-libraries | 641 | NEEDS MERGE |
| phase2-part2 | feature/phase2-part2 | 647 | NEEDS MERGE |
| feature/lov-consolidation | feature/lov-consolidation | — | REVIEW |
| feature/multi-view-components | feature/multi-view-components | — | REVIEW |

---

## Features NOT Yet in Main (by worktree)

### phase4-redis-cqrs-observability (5 commits) — PRIORITY 1
**New files unique to this branch:**
- `.github/workflows/ci.yml` — GitHub Actions CI pipeline
- `.github/workflows/cd.yml` — GitHub Actions CD pipeline
- `Dockerfile.public` — Docker container definition
- `docker-compose.yml` — Docker Compose services
- OpenTelemetry distributed tracing configuration
- Health checks and audit logging
- CQRS dispatchers with automatic handler discovery
- Redis L2 cache RemoveByPrefixAsync

**Merge command:**
```
git merge worktree-phase4-redis-cqrs-observability
```

---

### feat-p1-critical-gaps (17 commits) — PRIORITY 2
**New files unique to this branch:**
- Integration test suite (cross-module)
- XML documentation across 10 files in 3 projects
- Android/iOS/Windows platform service implementations (AudioService, CalendarService, SensorService)
- ContactsService test suite
- Webhook signature + cache key validation tests
- Core.Shared test project (22 passing tests)

**Merge command:**
```
git merge feat/p1-critical-gaps
```

---

### core-documentation + phase4-bluetooth-* (515–519 commits) — PRIORITY 3
These 5 branches share the same base. Merge core-documentation first, others are likely subsets.

**New files unique to these branches:**
- `src/SmartWorkz.Core.Web/TagHelpers/Auth/IfAuthorizedTagHelper.cs`
- `src/SmartWorkz.Core.Web/TagHelpers/Auth/IfClaimTagHelper.cs`
- `src/SmartWorkz.Core.Web/TagHelpers/Auth/IfRoleTagHelper.cs`
- `src/SmartWorkz.Core.MAUI/Services/ILocationService.cs`
- `src/SmartWorkz.Core.MAUI/Services/Implementations/LocationService.cs` (System.Reactive streaming)
- `src/SmartWorkz.Core.MAUI/Models/GpsLocation.cs`
- Platform-specific location providers (Android, iOS, Windows)

**Merge command:**
```
git merge worktree-core-documentation
```
Then verify phase4-bluetooth-* branches add anything beyond core-documentation before merging those.

---

### smartworkz-mobile-phases (498 commits) — PRIORITY 4
**New files unique to this branch:**
- `src/SmartWorkz.Core.MAUI/Services/IRequestInterceptor.cs`
- `src/SmartWorkz.Core.MAUI/Services/Implementations/TokenRefreshInterceptor.cs`
- `src/SmartWorkz.Core.MAUI/Services/Implementations/RequestLoggingInterceptor.cs`
- `src/SmartWorkz.Core.MAUI/Services/Implementations/RequestDeduplicationService.cs`
- `src/SmartWorkz.Core.MAUI/Services/Implementations/CorrelationInterceptor.cs`
- `src/SmartWorkz.Core.MAUI/Services/Implementations/DeviceInfoInterceptor.cs`
- `src/SmartWorkz.Sample.ECommerce.Mobile/` — Full MAUI ECommerce demo app

**Merge command:**
```
git merge worktree-smartworkz-mobile-phases
```

---

### component-libraries + phase2-part2 (641–647 commits) — PRIORITY 5
Largest branches. Contains real-time, MassTransit, and security features. Merge last due to size.

**New files unique to these branches:**
```
Core.MAUI/Services/:
  IRealtimeService + RealtimeService (SignalR-based)
  IAutoReconnectService + AutoReconnectService (exponential backoff)
  IDeduplicationService + DeduplicationService
  IRetryPolicy + RetryPolicy
  OfflineMessageQueue
  RealtimeConnectionManager + RealtimeMessageHandler

Core.Shared/Resilience/:
  ICircuitBreaker + CircuitBreaker (NEW feature)

Core.Shared/Security/:
  RateLimit/ — IRateLimitService, RateLimitService, RateLimitStatus
  CertificatePinning/ — CertificatePinningService
  Encryption/ — IEncryptionService, EncryptionService
  Audit/ — ISecurityAuditLogger, SecurityAuditLogger

Core.MacOS/ (Swift):
  App/Services/SignalR/ — MacOSSignalRClient.swift, SignalRConnectionManager.swift
  App/Models/RealtimeMessage.swift

MassTransit/ — Message queue consumers for async event processing
```

**Merge command:**
```
git merge feature/component-libraries
# resolve any conflicts
git merge feature/phase2-part2
# verify no duplicate features
```

---

### Root-Level GraphQL Migration (from SmartWorkz.Core.Web root project)
These files exist in root `SmartWorkz.Core.Web` but NOT in main:
```
src/SmartWorkz.Core.Web/GraphQL/Configuration/GraphQLSetup.cs
src/SmartWorkz.Core.Web/GraphQL/Configuration/RelayConnectionTypes.cs
src/SmartWorkz.Core.Web/GraphQL/DataLoaders/ProductDataLoader.cs
src/SmartWorkz.Core.Web/GraphQL/DataLoaders/UserDataLoader.cs
src/SmartWorkz.Core.Web/GraphQL/Middleware/GraphQLAuthenticationMiddleware.cs
src/SmartWorkz.Core.Web/GraphQL/Middleware/GraphQLMiddlewareExtensions.cs
src/SmartWorkz.Core.Web/GraphQL/Query.cs
```
**Action:** Copy files manually, update namespaces, test build, commit.

---

## Merge Execution Order

```
Step 1: git merge worktree-phase4-redis-cqrs-observability   (5 commits, CI/CD+Docker)
Step 2: git merge feat/p1-critical-gaps                       (17 commits, tests+docs)
Step 3: git merge worktree-core-documentation                 (519 commits, TagHelpers+Location)
Step 4: git merge worktree-phase4-biometric-platform-tests   (515 commits, verify unique)
Step 5: git merge worktree-phase4-biometric-tests            (515 commits, verify unique)
Step 6: git merge worktree-phase4-bluetooth-refinements      (515 commits, verify unique)
Step 7: git merge worktree-phase4-bluetooth-service-interface (515 commits, verify unique)
Step 8: git merge worktree-smartworkz-mobile-phases          (498 commits, interceptors+app)
Step 9: git merge feature/component-libraries                 (641 commits, realtime+security)
Step 10: git merge feature/phase2-part2                       (647 commits, macOS+SignalR)
Step 11: Manual copy — GraphQL files from root Core.Web
```

After each merge: run `dotnet build` and verify 0 errors before proceeding.

---

## Root Project Deletion (After Merge Complete)

```
DELETE: C:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.External
DELETE: C:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Shared
DELETE: C:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Mobile
REVIEW: C:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web  (keep if using Blazor)
```

---

## Worktree Cleanup (After Merge Complete)

**Already merged — delete immediately:**
```
git worktree remove .worktrees/phase1-mobile-location
git worktree remove .worktrees/phase2-mobile-camera
git worktree remove .claude/worktrees/phase3-mobile-biometric
git worktree remove .claude/worktrees/phase3-mobile-biometric-android
git worktree remove .claude/worktrees/phase3-mobile-biometric-windows
git worktree remove .claude/worktrees/phase3-mobile-filepicker
```

**Delete after merge in Steps 1–10:**
```
git worktree remove .claude/worktrees/phase4-redis-cqrs-observability
git worktree remove .worktrees/feat-p1-critical-gaps
git worktree remove .claude/worktrees/core-documentation
git worktree remove .claude/worktrees/phase4-biometric-platform-tests
git worktree remove .claude/worktrees/phase4-biometric-tests
git worktree remove .claude/worktrees/phase4-bluetooth-refinements
git worktree remove .claude/worktrees/phase4-bluetooth-service-interface
git worktree remove .claude/worktrees/smartworkz-mobile-phases
git worktree remove .worktrees/component-libraries
git worktree remove .worktrees/phase2-part2
```

---

## After All Merges: Final Structure

```
SmartWorkz.StarterKitMVC/
├── .github/workflows/ci.yml     ← from phase4-redis
├── .github/workflows/cd.yml     ← from phase4-redis
├── Dockerfile.public             ← from phase4-redis
├── docker-compose.yml            ← from phase4-redis
├── MERGE_PLAN.md                 ← this file
├── README.md                     ← keep
└── src/
    ├── SmartWorkz.Core/
    ├── SmartWorkz.Core.External/
    ├── SmartWorkz.Core.MAUI/     ← all mobile + realtime + interceptors
    ├── SmartWorkz.Core.Shared/   ← + CircuitBreaker, Security, CQRS
    ├── SmartWorkz.Core.Web/      ← + Auth TagHelpers, GraphQL complete
    ├── SmartWorkz.Sample.ECommerce/
    └── SmartWorkz.Sample.ECommerce.Mobile/  ← full MAUI demo
```
