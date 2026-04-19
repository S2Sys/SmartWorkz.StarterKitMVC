# Background Jobs

Two orthogonal patterns:

1. **`IHostedService`** — startup + shutdown hooks, plus timer-driven workers. The framework ships one: `TranslationCacheWarmupService`.
2. **`IBackgroundJobScheduler`** — fire-and-forget enqueue of one-off work from request handlers. Default implementation (`InMemoryBackgroundJobScheduler`) is **deliberately minimal** — plug in Hangfire / Quartz for durable queues.

## Purpose

- **Warm caches / state** before the first request lands.
- **Run recurring tasks** without blocking HTTP threads (email dispatch, cache refresh, cleanup).
- **Defer slow work** from request handlers (send welcome email, emit event) so the user isn't kept waiting.
- **Keep the contract stable** — `IBackgroundJobScheduler` is the abstraction; swap the implementation when you outgrow in-memory.

## Architecture

| Component | Role | File |
|-----------|------|------|
| `IBackgroundJobScheduler` | Contract for fire-and-forget enqueue | [`Application/Abstractions/IBackgroundJobScheduler.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Abstractions/IBackgroundJobScheduler.cs) |
| `InMemoryBackgroundJobScheduler` | Trivial `Task.Run` implementation | [`Infrastructure/BackgroundJobs/InMemoryBackgroundJobScheduler.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/BackgroundJobs/InMemoryBackgroundJobScheduler.cs) |
| `TranslationCacheWarmupService` | `IHostedService` that warms the translation cache | [`Infrastructure/BackgroundJobs/TranslationCacheWarmupService.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/BackgroundJobs/TranslationCacheWarmupService.cs) |

## DI Registration

Wired by `AddApplicationStack`:

```csharp
// Warm-up — runs on host start
services.AddHostedService<TranslationCacheWarmupService>();
```

`IBackgroundJobScheduler` is **not** registered automatically — opt in where you need it:

```csharp
services.AddSingleton<IBackgroundJobScheduler, InMemoryBackgroundJobScheduler>();
```

Use `AddSingleton` — the scheduler holds no request state and must outlive any scoped service that uses it.

## `IHostedService` Pattern

### Built-in: `TranslationCacheWarmupService`

Runs once at host start. Reads supported locales from `Features:Localization:SupportedCultures`, iterates over every known tenant (`DEFAULT`, `DEMO`), and calls `ITranslationService.WarmCacheAsync(tenant, locale)`. Failures are logged at **Warning** and skipped — missing translations should never block the app from starting.

```csharp
await translationService.WarmCacheAsync("DEFAULT", "en-US");
```

Configuration source:

```json
"Features": {
  "Localization": {
    "SupportedCultures": ["en-US", "es-ES", "fr-FR", "de-DE", "hi-IN"]
  }
}
```

### Writing your own `IHostedService`

```csharp
public sealed class EmailQueueWorker(
    IEmailQueueRepository queue,
    IEmailDispatcher dispatcher,
    ILogger<EmailQueueWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var batch = await queue.DequeueBatchAsync(size: 25, stoppingToken);
                foreach (var email in batch)
                    await dispatcher.SendAsync(email, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Email worker iteration failed");
            }
        }
    }
}
```

Register:

```csharp
services.AddHostedService<EmailQueueWorker>();
```

Use `BackgroundService` (abstract base) when you want a long-running loop; implement `IHostedService` directly when you only need `StartAsync` / `StopAsync` hooks (like warm-up).

### Rules for hosted services

- **Don't inject scoped services directly** — `IHostedService` is Singleton. Resolve scoped services via `IServiceScopeFactory` inside `ExecuteAsync`.
- **Respect the `CancellationToken`** — exit cleanly when the host shuts down, otherwise the host will kill you after the shutdown timeout (default 30s).
- **Never throw unhandled from `StartAsync`** — an exception aborts the host. Log and continue.

## `IBackgroundJobScheduler` Pattern

### Built-in: `InMemoryBackgroundJobScheduler`

```csharp
public string Enqueue(Func<CancellationToken, Task> job, string? description = null)
{
    _ = Task.Run(() => job(CancellationToken.None));
    return Guid.NewGuid().ToString();
}
```

- No persistence — a crash loses pending jobs.
- No retry — if the delegate throws, the exception is unhandled by `Task.Run` and effectively swallowed.
- No ordering, no priorities, no concurrency limits.

Use it for trivial deferred work **in dev** or for work that's genuinely fine to lose (log flush, metric emit). Everything else needs a durable queue.

### Using it

```csharp
public class AccountService
{
    private readonly IBackgroundJobScheduler _jobs;
    private readonly ITemplatedEmailSender _mail;

    public AccountService(IBackgroundJobScheduler jobs, ITemplatedEmailSender mail)
    {
        _jobs = jobs;
        _mail = mail;
    }

    public async Task<Result> RegisterAsync(RegisterRequest r)
    {
        // … persist user …

        // Don't make the HTTP response wait on SMTP
        _jobs.Enqueue(async ct =>
        {
            await _mail.SendTemplatedEmailAsync("welcome-email", r.Email,
                new Dictionary<string, object> { ["UserName"] = r.DisplayName }, ct);
        }, description: $"welcome-email:{r.Email}");

        return Result.Ok();
    }
}
```

## Provider Swap — durable queues

`InMemoryBackgroundJobScheduler` is fine for dev and trivial cases. For anything real, swap in Hangfire, Quartz, Azure Queue Storage, RabbitMQ, etc.

### Hangfire (recommended default for .NET)

```csharp
services.AddHangfire(c => c.UseSqlServerStorage(cs));
services.AddHangfireServer();

services.AddSingleton<IBackgroundJobScheduler, HangfireJobScheduler>();

public sealed class HangfireJobScheduler : IBackgroundJobScheduler
{
    public string Enqueue(Func<CancellationToken, Task> job, string? description = null)
        => Hangfire.BackgroundJob.Enqueue(() => InvokeAsync(job));

    public static Task InvokeAsync(Func<CancellationToken, Task> job)
        => job(CancellationToken.None);
}
```

Pipeline exposure (optional Hangfire dashboard behind an auth gate):

```csharp
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AdminOnlyAuthorizationFilter() }
});
```

Feature flag:

```json
"Features": {
  "BackgroundJobs": {
    "Provider": "Hangfire",
    "Hangfire": { "Enabled": true, "DashboardPath": "/hangfire", "DashboardAuthorization": true }
  }
}
```

### Quartz.NET (when you need cron-style schedules)

Good fit for complex schedules (`0 0 * * MON-FRI`) and stateful jobs. Config lives under `Features:BackgroundJobs:Quartz` — currently placeholder; wire in Quartz when you need it.

### Azure Queue Storage / Service Bus / RabbitMQ

For cross-service / cross-machine jobs. Wrap the enqueue as `IBackgroundJobScheduler` and implement a separate consumer host. Messages should carry a tenant id and correlation id so the worker can replay under the same identity and trace.

## Tenant Awareness

The built-in schedulers have **no tenant scoping**. If your job needs the current tenant, capture it at enqueue time and rehydrate inside the delegate:

```csharp
var tenantId = HttpContext.Items["TenantId"] as string ?? "DEFAULT";
var userId = User.GetUserId();

_jobs.Enqueue(async ct =>
{
    using var scope = _scopes.CreateScope();
    var svc = scope.ServiceProvider.GetRequiredService<IReportService>();
    await svc.RunForAsync(tenantId, userId, ct);
});
```

For durable queues, persist `tenantId` in the job payload and rebuild the `ClaimsPrincipal` at the start of the worker — never rely on `HttpContext` from a background job.

## Cross-Client Notes

Background jobs rarely surface to clients directly, but two scenarios bleed through:

| Client | Impact |
|--------|--------|
| **Angular / React** | UIs that poll for "ready" state need a job-status endpoint; decide up-front whether jobs are fire-and-forget or tracked |
| **.NET MAUI** | Offline sync jobs — consider a lightweight in-app queue, push server job results on reconnect |

Adding a durable provider usually means adding a **public job-status REST endpoint** (`GET /api/jobs/{id}`). That's a new public surface — add a page to the wiki for it.

## Common Mistakes

- **Injecting scoped services into an `IHostedService`** — they outlive the scope and hold stale references. Use `IServiceScopeFactory.CreateScope()` inside `ExecuteAsync`.
- **Throwing from `StartAsync`** — aborts host startup.
- **Blocking inside `ExecuteAsync`** — don't `Thread.Sleep`; use `PeriodicTimer` or `Task.Delay(ct)`.
- **Relying on `InMemoryBackgroundJobScheduler` for critical work** — it loses jobs on crash and has no retries. Promote to Hangfire / durable queue before production.
- **Skipping cancellation** — a worker that ignores `stoppingToken` will be killed by the host's shutdown timeout; you lose the chance to finalize work.
- **Enqueuing closures over `HttpContext`** — the context is disposed by the time the background task runs. Copy primitive values out.
- **Forgetting to scope `DbContext`** — EF Core contexts are scoped. Create a scope per job run; don't share across iterations.

## See Also

- [00 — Getting Started](./00-getting-started.md) — `AddHostedService<TranslationCacheWarmupService>()` is wired inside `AddApplicationStack`
- [01 — Translation System](./01-translation-system.md) — what the warm-up service feeds
- [12 — Hybrid Cache](./12-hybrid-cache.md) — for caches that need warming
- [13 — Email Templates](./13-email-templates.md) — `TemplatedEmailSender` pushes to a queue that a hosted worker drains
- [20 — Middleware Stack](./20-middleware-stack.md) — request-time middleware, complementary to these out-of-band flows
