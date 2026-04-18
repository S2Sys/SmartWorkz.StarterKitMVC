# System Stored Procedures - SmartWorkz v3

**File:** `00_System_SPs.sql`  
**Lines:** 692  
**Procedures:** 10  
**Purpose:** Database administration, maintenance, and monitoring utilities  

---

## 📋 Procedures Overview

### 1. **spTableExists** - Table Existence Check
Check if a table exists in the database.

```sql
DECLARE @Exists BIT;
EXEC spTableExists 
    @SchemaName = 'Master',
    @TableName = 'Tenants',
    @Exists = @Exists OUTPUT;

SELECT @Exists AS TableExists; -- Returns: 1 = Exists, 0 = Not Found
```

**Usage:**
- Conditional table creation
- Migration validation
- Dependency checks

---

### 2. **spViewExists** - View Existence Check
Check if a view exists in the database.

```sql
DECLARE @Exists BIT;
EXEC spViewExists 
    @SchemaName = 'dbo',
    @ViewName = 'vw_LookupHierarchy',
    @Exists = @Exists OUTPUT;
```

**Usage:**
- Verify materialized views
- Pre-deployment checks

---

### 3. **spStoredProcedureExists** - Procedure Existence Check
Check if a stored procedure exists.

```sql
DECLARE @Exists BIT;
EXEC spStoredProcedureExists
    @SchemaName = 'Master',
    @ProcedureName = 'spUpsertTenant',
    @Exists = @Exists OUTPUT;
```

**Usage:**
- Validate procedure deployment
- Conditional execution logic

---

### 4. **spFindTableDependencies** - Find All Dependencies
Comprehensive dependency analysis for a table.

```sql
EXEC spFindTableDependencies
    @SchemaName = 'Master',
    @TableName = 'Lookup';
```

**Returns:**
- Foreign key dependencies (tables referencing this table)
- Stored procedures referencing the table
- Views referencing the table
- Indexes on the table
- Constraints on the table

**Use Case:**
- Before modifying table structure
- Before deleting a table
- Impact analysis

---

### 5. **spCleanTableWithDependencies** - Safe Table Cleanup
Delete table data and optionally drop constraints/indexes with dry-run capability.

```sql
-- Dry Run: See what would happen
EXEC spCleanTableWithDependencies
    @SchemaName = 'Master',
    @TableName = 'BlogPosts',
    @DeleteData = 1,
    @DropForeignKeys = 0,
    @DropIndexes = 0,
    @DryRun = 1;  -- Set to 0 to actually execute

-- Delete everything
EXEC spCleanTableWithDependencies
    @SchemaName = 'Master',
    @TableName = 'BlogPosts',
    @DeleteData = 1,
    @DropForeignKeys = 1,
    @DropIndexes = 1,
    @DryRun = 0;
```

**Parameters:**
- `@DeleteData` - Delete all rows (BIT, 0/1)
- `@DropForeignKeys` - Drop FK constraints (BIT, 0/1)
- `@DropIndexes` - Drop non-PK indexes (BIT, 0/1)
- `@DryRun` - Preview changes without executing (BIT, 0/1, default=1)

**Use Case:**
- Safe data cleanup with dependency awareness
- Preview changes before execution
- Development environment resets

---

### 6. **spIndexStatisticsReport** - Index Health Report
Comprehensive index statistics and fragmentation analysis.

```sql
-- Report on specific schema
EXEC spIndexStatisticsReport
    @SchemaName = 'Master',
    @OrderBy = 'Fragmentation';  -- 'Fragmentation', 'Seeks', 'Scans', 'Size'

-- Report on specific table
EXEC spIndexStatisticsReport
    @SchemaName = 'Master',
    @TableName = 'Lookup',
    @OrderBy = 'Fragmentation';

-- Report all indexes
EXEC spIndexStatisticsReport;
```

**Output Columns:**
- **SchemaName, TableName, IndexName** - Index location
- **IndexType** - CLUSTERED, NONCLUSTERED, etc.
- **Pages, SizeMB** - Index size metrics
- **Seeks, Scans, Lookups, Updates** - Access patterns
- **FragmentationPercent** - Logical fragmentation
- **HealthStatus** - Good/Moderate/High Fragmentation
- **DaysSinceLastSeek** - Last access date

**Health Thresholds:**
- `< 10%` - Good (no action needed)
- `10-30%` - Moderate (consider reorganize)
- `> 30%` - High (rebuild recommended)

---

### 7. **spIndexRecommendationReport** - Optimization Recommendations
AI-driven recommendations for index maintenance and creation.

```sql
-- Recommendations for fragmentation > 10% and 100+ seeks
EXEC spIndexRecommendationReport
    @MinFragmentation = 10,
    @MinSeeks = 100;

-- Strict recommendations
EXEC spIndexRecommendationReport
    @MinFragmentation = 20,
    @MinSeeks = 500;
```

**Sections:**
1. **HIGH FRAGMENTATION** - Indexes needing rebuild/reorganize
   - Shows REBUILD command for >30% fragmentation
   - Shows REORGANIZE command for 10-30% fragmentation

2. **UNUSED INDEXES** - Candidates for removal
   - Indexes with no seeks/scans but receiving updates
   - Drop commands provided

3. **MISSING INDEXES** - High-impact opportunities
   - Top 20 missing indexes ranked by improvement potential
   - CREATE INDEX commands ready to use

**Output Example:**
```
Improvement    | Equality Columns | Included Columns | Seeks
===============================================================
45678.23       | [TenantId]      | [IsActive]       | 1250
32145.67       | [Email]         | [IsDeleted]      | 890
```

---

### 8. **spDatabaseObjectReport** - Object Inventory
Complete database object count and statistics.

```sql
EXEC spDatabaseObjectReport;
```

**Sections:**
1. **SCHEMAS** - Count of objects per schema
2. **OBJECT TYPES** - Tables, Views, Procedures, Triggers, etc.
3. **TABLE STATISTICS**
   - Column count
   - Index count
   - Foreign key count
   - Row count
4. **STORED PROCEDURE COUNT** - Per schema
5. **VIEW COUNT** - Per schema
6. **SUMMARY** - Total counts by type

**Use Case:**
- Database documentation
- Pre-migration inventory
- Capacity planning

---

### 9. **spProcedureExecutionStats** - Procedure Performance
Execution statistics for stored procedures.

```sql
-- All procedures (default: ordered by execution count)
EXEC spProcedureExecutionStats;

-- Specific schema
EXEC spProcedureExecutionStats
    @SchemaName = 'Master',
    @OrderBy = 'TotalTime';  -- 'ExecutionCount', 'TotalTime', 'AvgTime'

-- Find slow procedures
EXEC spProcedureExecutionStats
    @OrderBy = 'AvgTime';
```

**Output Columns:**
- **ExecutionCount** - Number of times executed
- **TotalTimeMs** - Total execution time (milliseconds)
- **AvgTimeMs** - Average execution time per run
- **MaxTimeMs** - Worst-case execution time
- **PhysicalReads** - Disk read operations
- **LogicalReads** - Cache/buffer read operations
- **LogicalWrites** - Write operations

**Use Case:**
- Identify slow-running procedures
- Find most-called procedures
- Performance bottleneck analysis

---

### 10. **spSystemHealthCheck** - Overall Database Health
Comprehensive health check with actionable recommendations.

```sql
EXEC spSystemHealthCheck;
```

**Checks Performed:**

1. **DISK SPACE USAGE**
   - File size and free space
   - Usage percentage
   - File path locations

2. **INTEGRITY CHECK STATUS**
   - Last DBCC CHECKDB run
   - Days since last check

3. **HIGH FRAGMENTATION INDEXES** (>30%)
   - Requires immediate rebuild

4. **MISSING INDEXES** (Top 5)
   - Highest-impact missing indexes

5. **BLOCKED PROCESSES**
   - Active blocking chains
   - Login, hostname, program

**Output Example:**
```
Disk Space Usage: 45.25% used
Integrity Check: Last run 3 days ago
High Fragmentation Indexes: 2 found
Missing Indexes: 3 opportunities
Blocked Processes: None
```

---

## 📊 Usage Scenarios

### Scenario 1: Pre-Deployment Validation
```sql
-- Verify all tables and views exist
DECLARE @Result BIT;

EXEC spTableExists @SchemaName='Master', @TableName='Tenants', @Exists=@Result OUTPUT;
IF @Result = 0 PRINT 'ERROR: Master.Tenants table not found!';

EXEC spViewExists @SchemaName='dbo', @ViewName='vw_LookupHierarchy', @Exists=@Result OUTPUT;
IF @Result = 0 PRINT 'ERROR: vw_LookupHierarchy view not found!';

-- Check for blocking procedures
EXEC spProcedureExecutionStats;
```

### Scenario 2: Index Maintenance
```sql
-- Get health report
EXEC spIndexStatisticsReport @OrderBy='Fragmentation';

-- Get recommendations
EXEC spIndexRecommendationReport @MinFragmentation=10;

-- Execute recommendations (copy-paste from output)
ALTER INDEX idx_Master_Lookup_TenantId ON Master.Lookup REBUILD;
ALTER INDEX idx_Master_BlogPosts_CategoryId ON Master.BlogPosts REORGANIZE;
```

### Scenario 3: Table Migration
```sql
-- Before modifying table structure
EXEC spFindTableDependencies @SchemaName='Master', @TableName='Lookup';

-- Identify impact
-- Modify table...

-- Clean up test data safely
EXEC spCleanTableWithDependencies 
    @SchemaName='Master',
    @TableName='BlogPosts',
    @DeleteData=1,
    @DryRun=1;  -- Preview first
```

### Scenario 4: Nightly Health Check
```sql
-- Run as SQL Agent job
EXEC spSystemHealthCheck;

-- If issues found:
IF (SELECT COUNT(*) FROM sys.indexes i 
    INNER JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') d
    ON i.object_id = d.object_id
    WHERE d.avg_fragmentation_in_percent > 30) > 0
BEGIN
    EXEC spIndexRecommendationReport @MinFragmentation=30;
    -- Email or log results
END
```

---

## 🔧 Maintenance Schedule

**Daily:**
- Run `spSystemHealthCheck` during off-peak hours
- Monitor for blocked processes

**Weekly:**
- Run `spIndexStatisticsReport`
- Review `spIndexRecommendationReport` for optimization opportunities

**Monthly:**
- Full `spDatabaseObjectReport` for documentation
- Review `spProcedureExecutionStats` for performance tuning

**Before Major Changes:**
- Run `spFindTableDependencies` for affected tables
- Review in dry-run mode with `spCleanTableWithDependencies`

---

## 📌 Best Practices

1. **Always use DryRun first** before destructive operations
2. **Schedule health checks** during low-traffic windows
3. **Archive reports** for trending analysis
4. **Act on high fragmentation** (>30%) within 2 weeks
5. **Monitor procedure stats** for performance regressions
6. **Document dependencies** before schema changes

---

## ⚠️ Safety Notes

- **Destructive operations** require explicit parameter flags
- **DryRun mode** (default=1) prevents accidental execution
- **Soft deletes only** - uses IsDeleted flag, no hard deletion
- **No cascading deletes** - dependencies must be handled explicitly
- **Readonly queries** - Statistics procedures don't modify data

---

## 📋 Full Procedure List

| # | Procedure | Purpose | Risk |
|---|-----------|---------|------|
| 1 | spTableExists | Check table existence | None |
| 2 | spViewExists | Check view existence | None |
| 3 | spStoredProcedureExists | Check procedure existence | None |
| 4 | spFindTableDependencies | Dependency analysis | None |
| 5 | spCleanTableWithDependencies | Safe cleanup | High (needs DryRun=1) |
| 6 | spIndexStatisticsReport | Index health | None |
| 7 | spIndexRecommendationReport | Optimization advice | None |
| 8 | spDatabaseObjectReport | Inventory | None |
| 9 | spProcedureExecutionStats | Performance metrics | None |
| 10 | spSystemHealthCheck | Overall health | None |

---

**Created:** 2026-04-18  
**Compatible with:** SQL Server 2016+  
**Database:** Boilerplate v3  
