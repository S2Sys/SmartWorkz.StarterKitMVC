# Observability & Dashboards Systems

**Purpose**: Provide real-time dashboards, monitoring views, and system metrics for observability and executive reporting.

## 📂 Files in This Group

| # | File | Purpose | Key Tables | Key Procedures |
|---|------|---------|-----------|-----------------|
| 1 | 01_DASHBOARD_METRICS.sql | Real-time dashboard views and metrics | 5 materialized views (vw_DashboardUserMetrics, vw_DashboardAuthMetrics, vw_DashboardContentMetrics, vw_DashboardSystemMetrics, vw_DashboardTenantMetrics) | spGetDashboardSummary, spGetDashboardTile |

## 🎯 When to Deploy

**Phase**: After Phase 1 foundation, before UI development  
**Timing**: Early in deployment to design UI around available metrics  
**Effort**: 30 minutes to deploy + 2 hours to build UI

## 🔧 Quick Setup

### 1. Deploy
```sql
USE Boilerplate;
:r observability/01_DASHBOARD_METRICS.sql
```

### 2. View Available Dashboard Data
```sql
-- Get dashboard summary (all metrics)
EXEC spGetDashboardSummary;

-- Get specific dashboard tile
EXEC spGetDashboardTile @TileName='UserMetrics';
EXEC spGetDashboardTile @TileName='AuthMetrics';
EXEC spGetDashboardTile @TileName='ContentMetrics';
```

### 3. Query Materialized Views Directly
```sql
-- User metrics
SELECT * FROM dbo.vw_DashboardUserMetrics;

-- Authentication metrics
SELECT * FROM dbo.vw_DashboardAuthMetrics;

-- Content metrics
SELECT * FROM dbo.vw_DashboardContentMetrics;

-- System metrics
SELECT * FROM dbo.vw_DashboardSystemMetrics;

-- Tenant metrics
SELECT * FROM dbo.vw_DashboardTenantMetrics;
```

## 📊 Available Dashboard Views

### 1. vw_DashboardUserMetrics
**Purpose**: User activity and status overview

**Columns**:
- TotalUsers: Total user count
- ActiveUsers: Active in last 30 days
- LockedUsers: Currently locked accounts
- DeletedUsers: Soft-deleted users
- AverageActivityWindow: Days since last login

**Use Cases**:
- User engagement monitoring
- Account health status
- Growth tracking

### 2. vw_DashboardAuthMetrics
**Purpose**: Authentication system health

**Columns**:
- LoginAttempts24h: Logins in last 24 hours
- SuccessfulLogins: Successful login count
- FailedLogins: Failed login attempts
- SuccessRate: Login success percentage
- LockedAccountCount: Accounts locked due to failed attempts

**Use Cases**:
- Security monitoring
- Authentication quality
- Anomaly detection (sudden failures)

### 3. vw_DashboardContentMetrics
**Purpose**: Content management overview

**Columns**:
- TotalBlogPosts: Total posts
- PublishedPosts: Published posts
- DraftPosts: In-draft posts
- TotalCustomPages: Custom pages
- PublishedPages: Published pages
- TotalCategories: Category count
- AverageBlogPostViewCount: Average views per post

**Use Cases**:
- Content pipeline tracking
- Engagement metrics
- Publication status

### 4. vw_DashboardSystemMetrics
**Purpose**: System-wide operational metrics

**Columns**:
- TotalAuditLogs: Audit entries
- UnreadNotifications: User notifications
- EnabledFeatureFlags: Active feature flags
- ConfigurationEntries: Config count
- AvgAuditLogsPerDay: Daily audit volume

**Use Cases**:
- System load understanding
- Feature flag tracking
- Configuration management

### 5. vw_DashboardTenantMetrics
**Purpose**: Multi-tenant system overview

**Columns**:
- TotalTenants: Tenant count
- ActiveTenants: Tenants with recent activity
- TrialTenants: Subscription status
- PaidTenants: Paying customers
- AverageTenantsPerUser: Tenant distribution

**Use Cases**:
- SaaS business metrics
- Tenant health
- Subscription tracking

## 💡 Common Tasks

### Task 1: Build User Dashboard
```sql
EXEC spGetDashboardTile @TileName='UserMetrics';

-- Result:
-- - Total Users: 150
-- - Active Users: 120
-- - Locked Users: 5
-- - Average Activity Window: 8 days
```

### Task 2: Monitor Authentication Health
```sql
EXEC spGetDashboardTile @TileName='AuthMetrics';

-- Result:
-- - Login Attempts (24h): 450
-- - Success Rate: 98.5%
-- - Failed Logins: 7
-- - Locked Accounts: 2
```

### Task 3: Track Content Pipeline
```sql
EXEC spGetDashboardTile @TileName='ContentMetrics';

-- Result:
-- - Blog Posts: 45 (32 published, 13 drafts)
-- - Custom Pages: 8 (6 published)
-- - Categories: 12
-- - Avg Post Views: 156
```

### Task 4: Monitor System Operations
```sql
EXEC spGetDashboardTile @TileName='SystemMetrics';

-- Result:
-- - Audit Logs: 5,234
-- - Unread Notifications: 42
-- - Feature Flags: 18 (12 enabled)
-- - Configuration Entries: 156
-- - Daily Audit Volume: 185 events/day
```

### Task 5: SaaS Business Dashboard
```sql
EXEC spGetDashboardTile @TileName='TenantMetrics';

-- Result:
-- - Total Tenants: 25
-- - Active Tenants: 22
-- - Paid Tenants: 20
-- - Trial Tenants: 5
-- - Avg Tenants/User: 1.8
```

### Task 6: Executive Summary
```sql
EXEC spGetDashboardSummary;

-- Returns all dashboard tiles in one query
-- Perfect for executive dashboards
```

## 🎯 Dashboard Integration Patterns

### Pattern 1: Real-time Web Dashboard
```javascript
// Fetch all dashboard data
fetch('/api/dashboard/summary')
  .then(response => response.json())
  .then(data => {
    // Render user metrics
    renderUserMetrics(data.userMetrics);
    
    // Render auth metrics
    renderAuthMetrics(data.authMetrics);
    
    // Render content metrics
    renderContentMetrics(data.contentMetrics);
    
    // Render system metrics
    renderSystemMetrics(data.systemMetrics);
    
    // Render tenant metrics
    renderTenantMetrics(data.tenantMetrics);
  });
```

### Pattern 2: Tile-based Dashboard
```javascript
// Fetch specific tile
async function getTile(tileName) {
  const response = await fetch(`/api/dashboard/tile?name=${tileName}`);
  return response.json();
}

// Load on demand
getTile('UserMetrics').then(data => renderUserCard(data));
getTile('AuthMetrics').then(data => renderAuthCard(data));
```

### Pattern 3: Automated Refresh
```javascript
// Refresh every 5 minutes
setInterval(() => {
  fetch('/api/dashboard/summary')
    .then(response => response.json())
    .then(data => updateAllTiles(data));
}, 5 * 60 * 1000);
```

## 📈 Performance Characteristics

| View | Typical Query Time | Refresh Frequency | Data Freshness |
|------|-------------------|-------------------|-----------------|
| vw_DashboardUserMetrics | <100ms | 5 min | Real-time |
| vw_DashboardAuthMetrics | <100ms | 5 min | Real-time |
| vw_DashboardContentMetrics | <50ms | 5 min | Real-time |
| vw_DashboardSystemMetrics | <100ms | 5 min | Real-time |
| vw_DashboardTenantMetrics | <100ms | 5 min | Real-time |

## 🔄 Materialized View Maintenance

### Refresh Views Daily
```sql
-- Schedule in Nightly Maintenance job
ALTER MATERIALIZATION VIEW dbo.vw_DashboardUserMetrics REBUILD;
ALTER MATERIALIZATION VIEW dbo.vw_DashboardAuthMetrics REBUILD;
ALTER MATERIALIZATION VIEW dbo.vw_DashboardContentMetrics REBUILD;
ALTER MATERIALIZATION VIEW dbo.vw_DashboardSystemMetrics REBUILD;
ALTER MATERIALIZATION VIEW dbo.vw_DashboardTenantMetrics REBUILD;
```

### Monitor View Health
```sql
-- Check for outdated data
SELECT 
    name,
    DATEDIFF(MINUTE, last_update, GETUTCDATE()) AS MinutesSinceUpdate,
    row_count
FROM sys.materialized_view_statistics
ORDER BY MinutesSinceUpdate DESC;
```

## 🎯 Metrics Definitions

### User Health Score
```
Formula: (ActiveUsers / TotalUsers) * 100
Target: >80%
Alert: <70% (potential user churn)
```

### Authentication Health Score
```
Formula: (SuccessfulLogins / LoginAttempts24h) * 100
Target: >99%
Alert: <95% (authentication issues)
```

### Content Engagement Score
```
Formula: (PublishedPosts / TotalPosts) * 100
Target: >70%
Alert: <50% (low publication rate)
```

### System Load Score
```
Formula: 100 - (AuditLogsPerDay / ExpectedMax) * 100
Target: <70%
Alert: >80% (high system activity)
```

## 📊 Dashboard Tile Specifications

Each tile includes:
- **Metric Name**: Display label
- **Value**: Current metric value
- **Previous**: Previous period value (for trending)
- **Trend**: Up/Down/Stable indicator
- **ChangePercent**: % change from previous period
- **Status**: Green/Yellow/Red health status

## 🔗 Integration Points

- **Performance System**: Operational metrics (CPU, memory, query performance)
- **Operations/Scheduling**: Dashboard refresh timing
- **API/Integration**: External service health status
- **Data Quality**: Quality score integration
- **Monitoring/Backup**: Backup status display

## 📋 Procedures

### spGetDashboardSummary
Retrieves all dashboard metrics in one call

**Parameters**: None

**Output**: Result sets for each dashboard tile:
1. UserMetrics
2. AuthMetrics
3. ContentMetrics
4. SystemMetrics
5. TenantMetrics

**Usage**:
```sql
EXEC spGetDashboardSummary;
```

### spGetDashboardTile
Retrieves a specific dashboard tile

**Parameters**:
- @TileName: Tile name (UserMetrics, AuthMetrics, ContentMetrics, SystemMetrics, TenantMetrics)

**Output**: Single result set with tile metrics

**Usage**:
```sql
EXEC spGetDashboardTile @TileName='UserMetrics';
```

## 🚨 Troubleshooting

### "Dashboard showing stale data"
```sql
-- Check view last update
SELECT name, last_update FROM sys.materialized_view_statistics;

-- Rebuild views
ALTER MATERIALIZED VIEW dbo.vw_DashboardUserMetrics REBUILD;
ALTER MATERIALIZED VIEW dbo.vw_DashboardAuthMetrics REBUILD;
ALTER MATERIALIZED VIEW dbo.vw_DashboardContentMetrics REBUILD;
ALTER MATERIALIZED VIEW dbo.vw_DashboardSystemMetrics REBUILD;
ALTER MATERIALIZED VIEW dbo.vw_DashboardTenantMetrics REBUILD;
```

### "Dashboard queries taking too long"
```sql
-- Check view statistics
SELECT 
    OBJECT_NAME(v.object_id) AS ViewName,
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups
FROM sys.materialized_view_indexes v
JOIN sys.indexes i ON v.object_id = i.object_id
JOIN sys.dm_db_index_usage_stats s ON i.object_id = s.object_id
WHERE database_id = DB_ID();

-- Add missing indexes if needed
CREATE INDEX IX_[ViewName]_[Column] ON [ViewName]([Column]);
```

### "Metrics not updating"
```sql
-- Verify data is being inserted
SELECT COUNT(*) FROM Master.Users WHERE IsDeleted=0;
SELECT COUNT(*) FROM Auth.LoginAttempts WHERE CreatedAt >= DATEADD(DAY, -1, GETUTCDATE());
SELECT COUNT(*) FROM Transaction.BlogPosts WHERE IsDeleted=0;

-- Refresh views manually
ALTER MATERIALIZED VIEW dbo.vw_DashboardUserMetrics REBUILD;
```

## 📚 Example Dashboard Layouts

### Executive Dashboard
```
┌─────────────────────────────────────┐
│ USER METRICS          AUTH METRICS   │
│ Total: 150            Success: 98.5% │
│ Active: 120           Locked: 2      │
└─────────────────────────────────────┘
│ CONTENT METRICS       TENANT METRICS │
│ Posts: 45             Active: 22     │
│ Published: 32         Paid: 20       │
└─────────────────────────────────────┘
│ SYSTEM METRICS                       │
│ Audit Logs: 5,234    Notifications: 42│
│ Feature Flags: 18 (12 enabled)       │
└─────────────────────────────────────┘
```

### Operations Dashboard
```
┌──────────────────────┬──────────────────────┐
│ USER ENGAGEMENT      │ SYSTEM HEALTH        │
│ 24h Active: 120      │ Errors: 0            │
│ Growth: +5%          │ Performance: Good    │
└──────────────────────┴──────────────────────┘
│ CONTENT PERFORMANCE                         │
│ Avg Views: 156       New Posts: 3 (today)   │
│ Engagement Rate: 78% Published Rate: 89%    │
└─────────────────────────────────────────────┘
```

---

**Group**: Observability & Dashboards  
**Files**: 1 SQL script  
**Views**: 5 materialized views  
**Procedures**: 2 dashboard procedures  
**Status**: Production-ready
