# Phase 3.0.1: Scale Foundation (Months 1-2)

**Objective**: Build the infrastructure foundation to handle 10M+ concurrent users  
**Duration**: 8 weeks  
**Team**: 2-3 backend engineers, 2 DevOps engineers, 1 database engineer  
**Success Metric**: Support 10x current load with <100ms P95 latency

---

## SPRINT BREAKDOWN (8 Weeks)

### **SPRINT 1-2 (Weeks 1-2): Redis Cluster & Advanced Caching**

**Objective**: Implement L2 distributed cache for multi-instance scaling

**Tasks**:

#### Task 1.1: Redis Cluster Setup (3 days)
```bash
# Deploy Redis Cluster (3 master nodes + 3 replicas)
docker-compose up -d redis-cluster

# Or via cloud provider:
# AWS ElastiCache: Create Redis cluster (6 nodes, cluster mode enabled)
# Azure Redis: Create Enterprise tier (6 nodes)
# GCP: Create Memorystore with High Availability

# Cluster nodes: 3 shards x 2 replicas = 6 nodes
# Memory per node: 32GB
# Total cluster memory: 192GB (256GB reserved)
```

**Checklist**:
- [ ] Deploy Redis Cluster (3 shards, 6 total nodes)
- [ ] Configure cluster nodes (16GB heap per node)
- [ ] Set up master-replica replication
- [ ] Enable cluster authentication (strong passwords)
- [ ] Test failover (kill a master, verify auto-failover)
- [ ] Configure cluster persistence (RDB snapshots every 5 min)
- [ ] Backup strategy (daily snapshots to S3/Blob)
- [ ] Monitoring (Redis exporter → Prometheus)

**Deliverable**: Production Redis Cluster running, monitored, backed up

---

#### Task 1.2: RedisClusterCache Implementation (4 days)
```csharp
// File: SmartWorkz.Core.Shared/Caching/RedisClusterCache.cs

using StackExchange.Redis;
using System.Text.Json;

public class RedisClusterCache : IDistributedCache
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisClusterCache> _logger;
    private readonly IMetricsCollector _metrics;
    
    public RedisClusterCache(
        IConnectionMultiplexer redis,
        ILogger<RedisClusterCache> logger,
        IMetricsCollector metrics)
    {
        _redis = redis;
        _logger = logger;
        _metrics = metrics;
    }
    
    public async Task<T> GetAsync<T>(string key)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var value = await _redis.GetDatabase().StringGetAsync(key);
            stopwatch.Stop();
            _metrics.RecordCacheLatency("redis.get", stopwatch.ElapsedMilliseconds);
            
            if (!value.HasValue)
            {
                _metrics.IncrementCacheMetric("redis.miss");
                return default;
            }
            
            _metrics.IncrementCacheMetric("redis.hit");
            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection failed for key {Key}, falling back to memory", key);
            _metrics.IncrementCacheMetric("redis.fallback");
            // Fallback to L1 memory cache
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis get failed for key {Key}", key);
            _metrics.IncrementCacheMetric("redis.error");
            throw;
        }
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _redis.GetDatabase().StringSetAsync(key, json, expiry);
            stopwatch.Stop();
            _metrics.RecordCacheLatency("redis.set", stopwatch.ElapsedMilliseconds);
            _metrics.IncrementCacheMetric("redis.set_count");
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection failed for key {Key}", key);
            _metrics.IncrementCacheMetric("redis.set_fallback");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis set failed for key {Key}", key);
            _metrics.IncrementCacheMetric("redis.error");
            throw;
        }
    }
    
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _redis.GetDatabase().KeyDeleteAsync(key);
            _metrics.IncrementCacheMetric("redis.delete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis remove failed for key {Key}", key);
        }
    }
    
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);
            
            var db = _redis.GetDatabase();
            foreach (var key in keys)
            {
                await db.KeyDeleteAsync(key);
            }
            
            _logger.LogInformation("Removed {KeyCount} keys matching pattern {Pattern}", keys.Count(), pattern);
            _metrics.IncrementCacheMetric("redis.pattern_delete", keys.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis pattern remove failed for pattern {Pattern}", pattern);
        }
    }
    
    // Health check
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var pong = await _redis.GetDatabase().ExecuteAsync("PING");
            return pong.IsNull == false;
        }
        catch
        {
            return false;
        }
    }
}

// File: SmartWorkz.Core.Shared/Caching/RedisCacheStartupExtensions.cs
public static class RedisCacheStartupExtensions
{
    public static IServiceCollection AddRedisClusterCache(this IServiceCollection services, IConfiguration config)
    {
        var redisOptions = ConfigurationOptions.Parse(config.GetConnectionString("Redis"));
        
        // Cluster configuration
        redisOptions.EndPoints.Clear();
        var clusterNodes = config.GetSection("Redis:ClusterNodes").Get<string[]>();
        foreach (var node in clusterNodes)
        {
            redisOptions.EndPoints.Add(node);
        }
        
        // Connection settings
        redisOptions.ConnectTimeout = 5000;
        redisOptions.SyncTimeout = 5000;
        redisOptions.ConnectRetry = 3;
        redisOptions.DefaultVersion = new Version(7, 0);
        redisOptions.TieBreaker = "";
        
        var multiplexer = ConnectionMultiplexer.Connect(redisOptions);
        
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        services.AddSingleton<IDistributedCache, RedisClusterCache>();
        
        // Health check
        services.AddHealthChecks()
            .AddCheck("redis", async () =>
            {
                try
                {
                    var db = multiplexer.GetDatabase();
                    await db.ExecuteAsync("PING");
                    return HealthCheckResult.Healthy();
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy($"Redis unhealthy: {ex.Message}");
                }
            });
        
        return services;
    }
}
```

**Configuration**:
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "Redis": "redis.smartworkz.com:6379,redis2.smartworkz.com:6379,redis3.smartworkz.com:6379"
  },
  "Redis": {
    "ClusterNodes": [
      "redis.smartworkz.com:6379",
      "redis2.smartworkz.com:6379",
      "redis3.smartworkz.com:6379"
    ],
    "Ssl": true,
    "Password": "$(REDIS_PASSWORD)"
  }
}
```

**Checklist**:
- [ ] Implement RedisClusterCache class
- [ ] Add connection pooling (30-50 connections)
- [ ] Implement fallback to L1 memory if Redis unavailable
- [ ] Add metrics (hit/miss/latency)
- [ ] Add health checks
- [ ] Test failover scenarios
- [ ] Load test (1000 ops/sec sustained)
- [ ] Monitor memory usage and eviction

**Deliverable**: RedisClusterCache integrated, tested, metrics in place

---

#### Task 1.3: Cache Warming & Refresh (3 days)
```csharp
// File: SmartWorkz.Core.Shared/Caching/CacheWarmingService.cs

public class CacheWarmingService : IHostedService
{
    private readonly IDistributedCache _cache;
    private readonly IRepository _repo;
    private readonly ILogger<CacheWarmingService> _logger;
    private Timer _refreshTimer;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache warming service starting");
        
        // Warm critical data on startup
        await WarmCriticalDataAsync();
        
        // Schedule periodic refresh (every 1 hour)
        _refreshTimer = new Timer(
            async _ => await RefreshHotDataAsync(),
            null,
            TimeSpan.FromHours(1),
            TimeSpan.FromHours(1)
        );
    }
    
    private async Task WarmCriticalDataAsync()
    {
        try
        {
            // Load hot data that doesn't change frequently
            var products = await _repo.GetTopProductsAsync(1000); // Top 1000 products
            foreach (var product in products)
            {
                await _cache.SetAsync($"product:{product.Id}", product, TimeSpan.FromHours(24));
            }
            _logger.LogInformation("Warmed {Count} products", products.Count);
            
            var categories = await _repo.GetCategoriesAsync();
            foreach (var category in categories)
            {
                await _cache.SetAsync($"category:{category.Id}", category, TimeSpan.FromDays(7));
            }
            _logger.LogInformation("Warmed {Count} categories", categories.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache warming failed");
        }
    }
    
    private async Task RefreshHotDataAsync()
    {
        try
        {
            // Proactively refresh data about to expire
            var refreshThreshold = TimeSpan.FromHours(1); // Refresh if expires in < 1 hour
            
            var hottestProducts = await _repo.GetMostViewedProductsAsync(100);
            foreach (var product in hottestProducts)
            {
                await _cache.SetAsync($"product:{product.Id}", product, TimeSpan.FromHours(24));
            }
            
            _logger.LogInformation("Refreshed {Count} hot products", hottestProducts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache refresh failed");
        }
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _refreshTimer?.Dispose();
        _logger.LogInformation("Cache warming service stopped");
    }
}
```

**Checklist**:
- [ ] Implement cache warming service
- [ ] Warm critical data on startup
- [ ] Schedule periodic refresh (1-hour interval)
- [ ] Monitor memory usage
- [ ] Add telemetry for warm rate
- [ ] Test on production-like data volume

**Deliverable**: Cache warming service operational, metrics tracked

---

### **SPRINT 3-4 (Weeks 3-4): Database Scaling**

**Objective**: Implement sharding and read replicas to handle 100K queries/sec

**Tasks**:

#### Task 2.1: Database Architecture (4 days)
```csharp
// File: SmartWorkz.Core.Data/Sharding/ShardingStrategy.cs

public interface IShardingStrategy
{
    int GetShardId<T>(T entity) where T : class;
    string GetConnectionString(int shardId);
}

// User-based sharding (most users' data in one shard)
public class UserBasedShardingStrategy : IShardingStrategy
{
    private readonly IConfiguration _config;
    private readonly int _shardCount;
    
    public UserBasedShardingStrategy(IConfiguration config)
    {
        _config = config;
        _shardCount = int.Parse(config["Database:ShardCount"] ?? "4");
    }
    
    public int GetShardId<T>(T entity) where T : class
    {
        // Extract UserId from entity
        if (entity is IUserScoped userScoped)
        {
            return Math.Abs(userScoped.UserId.GetHashCode()) % _shardCount;
        }
        
        throw new InvalidOperationException($"Entity {typeof(T).Name} is not user-scoped");
    }
    
    public string GetConnectionString(int shardId)
    {
        return _config.GetConnectionString($"Database:Shard:{shardId}");
    }
}

// File: SmartWorkz.Core.Data/Sharding/ShardedDbContext.cs

public class ShardedDbContext
{
    private readonly Dictionary<int, DbContext> _shards;
    private readonly IShardingStrategy _shardingStrategy;
    
    public ShardedDbContext(IShardingStrategy shardingStrategy)
    {
        _shardingStrategy = shardingStrategy;
        _shards = new Dictionary<int, DbContext>();
    }
    
    private DbContext GetShardContext<T>(T entity) where T : class
    {
        var shardId = _shardingStrategy.GetShardId(entity);
        
        if (!_shards.ContainsKey(shardId))
        {
            var connectionString = _shardingStrategy.GetConnectionString(shardId);
            var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>()
                .UseSqlServer(connectionString);
            
            _shards[shardId] = new MasterDbContext(optionsBuilder.Options);
        }
        
        return _shards[shardId];
    }
    
    public async Task<T> GetByUserIdAsync<T>(int userId, int id) where T : class, IUserScoped
    {
        var shardId = Math.Abs(userId.GetHashCode()) % _shards.Count;
        var context = _shards[shardId];
        return await context.Set<T>().FindAsync(id);
    }
    
    public async Task AddAsync<T>(T entity) where T : class, IUserScoped
    {
        var context = GetShardContext(entity);
        await context.AddAsync(entity);
        await context.SaveChangesAsync();
    }
}

// File: SmartWorkz.Core.Data/Sharding/IUserScoped.cs
public interface IUserScoped
{
    int UserId { get; }
}
```

**Checklist**:
- [ ] Design sharding strategy (user-based)
- [ ] Create shard metadata store
- [ ] Implement sharded DbContext
- [ ] Create 4 database instances (1 primary + 1 replica each)
- [ ] Test shard routing
- [ ] Implement cross-shard queries (if needed)

**Deliverable**: Database sharding architecture implemented

---

#### Task 2.2: Read Replicas (3 days)
```csharp
// File: SmartWorkz.Core.Data/Replication/ReadReplicaDbContext.cs

public class ReadReplicaDbContext : DbContext
{
    // Read-only context pointing to replica
    // Used for reporting, analytics, non-critical reads
    
    public ReadReplicaDbContext(DbContextOptions<ReadReplicaDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=read-replica.smartworkz.com;Database=Master;",
                options => options.EnableRetryOnFailure()
            );
        }
    }
    
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Use same model as primary, but read-only
        base.OnModelCreating(modelBuilder);
        
        // All entities are read-only
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            entityType.SetIsReadOnly(true);
        }
    }
}

// File: SmartWorkz.Core.Data/Replication/ReadReplicaRepository.cs
public class ReadReplicaRepository<T> : IReadRepository<T> where T : class
{
    private readonly ReadReplicaDbContext _context;
    
    public ReadReplicaRepository(ReadReplicaDbContext context)
    {
        _context = context;
    }
    
    public async Task<T> GetByIdAsync(int id)
    {
        // Read from replica (eventually consistent)
        return await _context.Set<T>().FindAsync(id);
    }
    
    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }
    
    // Reports and analytics use read replicas
    public async Task<ReportData> GetSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<Order>()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new ReportData
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.Total),
                OrderCount = g.Count(),
                AverageOrderValue = g.Average(o => o.Total)
            })
            .ToListAsync();
    }
}
```

**Configuration**:
```json
{
  "ConnectionStrings": {
    "MasterDatabase": "Server=master.smartworkz.com;Database=Master;User=admin;Password=...;",
    "ReadReplica1": "Server=read-replica-1.smartworkz.com;Database=Master;User=readonly;Password=...;",
    "ReadReplica2": "Server=read-replica-2.smartworkz.com;Database=Master;User=readonly;Password=...;",
    "ReadReplica3": "Server=read-replica-3.smartworkz.com;Database=Master;User=readonly;Password=...;"
  }
}
```

**Checklist**:
- [ ] Create read replicas (3-5 replicas)
- [ ] Configure replication lag monitoring
- [ ] Route reads to replicas (writes to primary)
- [ ] Implement replica load balancing
- [ ] Test replica failover
- [ ] Monitor replication lag (<1 second)

**Deliverable**: Read replicas operational, reads distributed

---

#### Task 2.3: Connection Pooling Optimization (2 days)
```csharp
// File: SmartWorkz.Core.Data/Database/ConnectionPoolConfiguration.cs

public static class ConnectionPoolConfiguration
{
    public static DbContextOptionsBuilder ConfigureOptimalPooling(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString)
    {
        var csvBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            // Pooling configuration
            Pooling = true,
            MaxPoolSize = 128,      // Increased from default 100
            MinPoolSize = 5,
            
            // Connection timeouts
            ConnectTimeout = 30,
            ConnectRetryCount = 3,
            ConnectRetryInterval = 10,
            
            // Connection validation
            ValidateConnection = true,
            
            // Application intent (for read replicas)
            ApplicationIntent = ApplicationIntent.ReadWrite,
            
            // MultiSubnetFailover (for multi-region)
            MultiSubnetFailover = true,
            
            // Other optimizations
            Encrypt = true,
            TrustServerCertificate = false,
            
            // Connection idle timeout
            IdleTimeout = 300  // 5 minutes
        };
        
        return optionsBuilder.UseSqlServer(
            csvBuilder.ConnectionString,
            options => options
                .EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelaySeconds: 30,
                    errorNumbersToAdd: null
                )
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
        );
    }
}

// File: SmartWorkz.Core.Data/Database/ConnectionPoolMonitor.cs
public class ConnectionPoolMonitor : IHostedService
{
    private readonly DbContext _context;
    private readonly ILogger<ConnectionPoolMonitor> _logger;
    private Timer _monitoringTimer;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _monitoringTimer = new Timer(
            async _ => await MonitorPoolAsync(),
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1)
        );
    }
    
    private async Task MonitorPoolAsync()
    {
        try
        {
            // Get pool statistics
            using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            {
                // Query sys.dm_exec_sessions to understand connection usage
                var sql = @"
                    SELECT COUNT(*) as ActiveConnections
                    FROM sys.dm_exec_sessions
                    WHERE database_id = DB_ID()
                ";
                
                using (var cmd = new SqlCommand(sql, connection))
                {
                    await connection.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    _logger.LogInformation("Active database connections: {Count}", result);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection pool monitoring failed");
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _monitoringTimer?.Dispose();
        return Task.CompletedTask;
    }
}
```

**Checklist**:
- [ ] Configure max pool size (128)
- [ ] Set connection validation
- [ ] Enable retry logic
- [ ] Monitor active connections
- [ ] Test under load (1000 concurrent requests)
- [ ] Tune pool size based on testing

**Deliverable**: Connection pooling optimized, monitored

---

### **SPRINT 5-6 (Weeks 5-6): Kubernetes Deployment**

**Objective**: Containerize and deploy to Kubernetes with auto-scaling

**Tasks**:

#### Task 3.1: Containerization (3 days)
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution
COPY ["SmartWorkz.StarterKitMVC.sln", "."]
COPY ["src/", "src/"]

# Build
RUN dotnet restore
RUN dotnet build -c Release

# Test
RUN dotnet test -c Release --no-build

# Publish
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "SmartWorkz.StarterKitMVC.Public.dll"]
```

**Build & Push**:
```bash
# Build image
docker build -t smartworkz-api:1.0.0 .

# Tag for registry
docker tag smartworkz-api:1.0.0 smartworkz.azurecr.io/api:1.0.0

# Push to Azure Container Registry
docker push smartworkz.azurecr.io/api:1.0.0

# Verify
docker inspect smartworkz.azurecr.io/api:1.0.0
```

**Checklist**:
- [ ] Create Dockerfile (multi-stage build)
- [ ] Minimize image size (<500MB)
- [ ] Add health checks
- [ ] Use non-root user
- [ ] Build and test locally
- [ ] Push to container registry
- [ ] Scan for vulnerabilities

**Deliverable**: Container images built, tested, pushed to registry

---

#### Task 3.2: Kubernetes Cluster Setup (3 days)
```bash
# Create AKS cluster (Azure)
az aks create \
  --resource-group smartworkz \
  --name smartworkz-k8s \
  --node-count 3 \
  --vm-set-type VirtualMachineScaleSets \
  --load-balancer-sku standard \
  --enable-managed-identity \
  --network-plugin azure \
  --service-cidr 10.0.0.0/16 \
  --dns-service-ip 10.0.0.10 \
  --docker-bridge-address 172.17.0.1/16 \
  --zones 1 2 3

# Get credentials
az aks get-credentials \
  --resource-group smartworkz \
  --name smartworkz-k8s

# Verify cluster
kubectl get nodes
kubectl cluster-info
```

**Checklist**:
- [ ] Create Kubernetes cluster (3+ nodes)
- [ ] Configure networking (CNI)
- [ ] Set up storage (Azure Disks, Azure Files)
- [ ] Deploy monitoring (Prometheus, Grafana)
- [ ] Configure RBAC
- [ ] Set up ingress controller (NGINX)
- [ ] Configure TLS termination

**Deliverable**: Kubernetes cluster operational

---

#### Task 3.3: Deployment Manifests (3 days)
```yaml
# kubernetes/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: smartworkz-api
  namespace: production
  labels:
    app: smartworkz-api
    version: v1
spec:
  replicas: 3  # Start with 3, HPA will scale
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1          # Add 1 pod during update
      maxUnavailable: 0    # Zero downtime
  selector:
    matchLabels:
      app: smartworkz-api
  template:
    metadata:
      labels:
        app: smartworkz-api
        version: v1
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/port: "5000"
        prometheus.io/path: "/metrics"
    spec:
      # Pod disruption budget (for safe rollouts)
      terminationGracePeriodSeconds: 30
      
      # Security context
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
      
      containers:
      - name: api
        image: smartworkz.azurecr.io/api:1.0.0
        imagePullPolicy: Always
        
        # Port
        ports:
        - containerPort: 5000
          name: http
          protocol: TCP
        
        # Environment
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: ASPNETCORE_URLS
          value: "http://+:5000"
        - name: ASPNETCORE_FORWARDEDHEADERS_ENABLED
          value: "true"
        
        # Secrets from KeyVault
        envFrom:
        - secretRef:
            name: smartworkz-api-secrets
        
        # Database connection
        - name: ConnectionStrings__MasterDatabase
          valueFrom:
            secretKeyRef:
              name: db-connection
              key: master
        
        # Redis connection
        - name: Redis__ClusterNodes
          valueFrom:
            configMapKeyRef:
              name: redis-config
              key: cluster-nodes
        
        # Resources
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"
        
        # Liveness probe (is container alive?)
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
            scheme: HTTP
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 3
          failureThreshold: 3
        
        # Readiness probe (is container ready for traffic?)
        readinessProbe:
          httpGet:
            path: /ready
            port: 5000
            scheme: HTTP
          initialDelaySeconds: 5
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        
        # Security context
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: false
          capabilities:
            drop:
            - ALL
        
        # Logging
        volumeMounts:
        - name: logs
          mountPath: /app/logs
      
      # Volumes
      volumes:
      - name: logs
        emptyDir: {}
      
      # Node affinity (schedule on specific nodes)
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
          - weight: 100
            podAffinityTerm:
              labelSelector:
                matchExpressions:
                - key: app
                  operator: In
                  values:
                  - smartworkz-api
              topologyKey: kubernetes.io/hostname

---
# kubernetes/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: smartworkz-api-service
  namespace: production
spec:
  type: ClusterIP
  selector:
    app: smartworkz-api
  ports:
  - port: 80
    targetPort: 5000
    protocol: TCP
    name: http

---
# kubernetes/hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: smartworkz-api-hpa
  namespace: production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: smartworkz-api
  
  # Scaling limits
  minReplicas: 3
  maxReplicas: 100
  
  # CPU-based scaling
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  
  # Memory-based scaling
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  
  # Custom metrics (if using Prometheus)
  - type: Pods
    pods:
      metric:
        name: http_requests_per_second
      target:
        type: AverageValue
        averageValue: "1000"
  
  # Scaling behavior
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 50
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 0
      policies:
      - type: Percent
        value: 100
        periodSeconds: 30
      - type: Pods
        value: 5
        periodSeconds: 30
      selectPolicy: Max

---
# kubernetes/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: smartworkz-ingress
  namespace: production
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/rate-limit: "100"
    nginx.ingress.kubernetes.io/proxy-body-size: "10m"
spec:
  tls:
  - hosts:
    - api.smartworkz.com
    secretName: smartworkz-tls
  
  rules:
  - host: api.smartworkz.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: smartworkz-api-service
            port:
              number: 80
```

**Deploy**:
```bash
# Create namespace
kubectl create namespace production

# Create secrets
kubectl create secret generic smartworkz-api-secrets \
  --from-literal=ConnectionString__MasterDatabase="..." \
  --from-literal=Redis__Password="..." \
  -n production

# Deploy
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/hpa.yaml
kubectl apply -f kubernetes/ingress.yaml

# Check deployment
kubectl get pods -n production
kubectl describe pod <pod-name> -n production
kubectl logs <pod-name> -n production

# Watch rollout
kubectl rollout status deployment/smartworkz-api -n production
```

**Checklist**:
- [ ] Create deployment manifest
- [ ] Configure resource limits
- [ ] Set up health checks (liveness/readiness)
- [ ] Configure HPA
- [ ] Create service
- [ ] Configure ingress with TLS
- [ ] Deploy to Kubernetes
- [ ] Verify pods running
- [ ] Test rolling updates

**Deliverable**: Application deployed to Kubernetes with auto-scaling

---

### **SPRINT 7-8 (Weeks 7-8): Load Testing & Optimization**

**Objective**: Validate infrastructure can handle 10x load

**Tasks**:

#### Task 4.1: Load Testing Framework (2 days)
```bash
# Install k6 (load testing tool)
brew install k6  # macOS
# or
apt-get install k6  # Linux
# or download from https://k6.io/download/
```

```javascript
// load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
let errorRate = new Rate('errors');
let duration = new Trend('request_duration');

export const options = {
  stages: [
    { duration: '2m', target: 100 },   // Ramp-up to 100 users
    { duration: '5m', target: 500 },   // Ramp-up to 500 users
    { duration: '10m', target: 1000 }, // Ramp-up to 1000 users
    { duration: '5m', target: 1000 },  // Stay at 1000 users
    { duration: '5m', target: 500 },   // Ramp-down to 500 users
    { duration: '2m', target: 0 },     // Ramp-down to 0 users
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500', 'p(99)<1000'],  // 95th percentile < 500ms
    'http_req_duration{staticAsset:yes}': ['p(99)<1000'], // Static assets < 1s
    'errors': ['rate<0.1'],  // Error rate < 0.1%
  },
};

export default function () {
  // Get products
  let response = http.get('https://api.smartworkz.com/api/products?skip=0&take=10');
  
  let success = check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });
  
  errorRate.add(!success);
  duration.add(response.timings.duration);
  
  sleep(1); // Think time between requests
  
  // Create order (some users)
  if (__VU % 10 === 0) {  // 10% of users create orders
    let orderResponse = http.post(
      'https://api.smartworkz.com/api/orders',
      JSON.stringify({
        items: [
          { productId: 1, quantity: 2 }
        ]
      }),
      {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${__ENV.API_TOKEN}`
        }
      }
    );
    
    check(orderResponse, {
      'order created': (r) => r.status === 201,
    });
  }
  
  sleep(2);
}
```

**Run load test**:
```bash
# Run with 1000 max concurrent users
k6 run --vus 1000 --duration 30m load-test.js

# Run with defined stages (gradual ramp-up)
k6 run load-test.js

# Run with results output to file
k6 run --out json=results.json load-test.js

# Stream results to cloud
k6 run --cloud load-test.js
```

**Checklist**:
- [ ] Install k6
- [ ] Create load test scenarios
- [ ] Test with 100 users (baseline)
- [ ] Test with 1000 users
- [ ] Test with 10,000 users
- [ ] Monitor metrics (latency, errors, throughput)
- [ ] Identify bottlenecks

**Deliverable**: Load testing framework in place, baseline established

---

#### Task 4.2: Performance Analysis & Optimization (3 days)
```csharp
// Profile hot paths using dotTrace
// 1. Run load test while profiling
// 2. Identify top methods by CPU time
// 3. Optimize

// Example optimization: Enable EF Core query caching
public class ProductRepository
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;
    
    public async Task<List<Product>> GetTopProductsAsync(int count)
    {
        // Check cache first
        if (_cache.TryGetValue("top_products", out List<Product> cached))
        {
            return cached;
        }
        
        // Query with compiled query (faster than interpreted)
        var topProducts = await GetTopProductsCompiledAsync(count);
        
        // Cache for 1 hour
        _cache.Set("top_products", topProducts, TimeSpan.FromHours(1));
        
        return topProducts;
    }
    
    // Compiled query (pre-compiled by EF Core)
    private static readonly Func<DbContext, int, Task<List<Product>>> GetTopProductsCompiledAsync =
        EF.CompileAsyncQuery((DbContext ctx, int count) =>
            ctx.Set<Product>()
                .OrderByDescending(p => p.Views)
                .Take(count)
                .AsNoTracking()
                .ToList()
        );
}

// Optimize database queries
public class OrderRepository
{
    public async Task<Order> GetOrderWithDetailsAsync(int orderId)
    {
        // BAD: N+1 query problem
        // var order = await _context.Orders.FindAsync(orderId);
        // var items = await _context.OrderItems.Where(x => x.OrderId == orderId).ToListAsync();
        
        // GOOD: Single query with includes
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .AsNoTracking()  // Read-only query
            .FirstOrDefaultAsync(o => o.Id == orderId);
        
        return order;
    }
}

// Add caching to expensive operations
public class AnalyticsService
{
    [Cache(Duration = 3600)]  // Cache for 1 hour
    public async Task<RevenueMetrics> GetRevenueMetricsAsync(DateTime from, DateTime to)
    {
        // Expensive calculation
        var orders = await _orderRepo.GetOrdersAsync(from, to);
        
        return new RevenueMetrics
        {
            Total = orders.Sum(o => o.Total),
            Count = orders.Count,
            Average = orders.Average(o => o.Total)
        };
    }
}
```

**Optimization Checklist**:
- [ ] Profile with dotTrace or similar
- [ ] Enable EF Core query caching
- [ ] Add strategic caching (Cache attribute)
- [ ] Optimize slow queries
- [ ] Add database indexes
- [ ] Enable async/await throughout
- [ ] Use AsNoTracking() for read-only queries
- [ ] Implement query batching (via Dapper)

**Deliverable**: Performance optimized, 10x load target achieved

---

#### Task 4.3: Monitoring & Alerting Setup (2 days)
```yaml
# prometheus/prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'smartworkz-api'
    static_configs:
      - targets: ['localhost:5000']
    metrics_path: '/metrics'

  - job_name: 'kubernetes-pods'
    kubernetes_sd_configs:
      - role: pod
        namespaces:
          names:
            - production

alerting:
  alertmanagers:
    - static_configs:
        - targets: ['localhost:9093']

rule_files:
  - 'alerts.yml'
```

```yaml
# prometheus/alerts.yml
groups:
  - name: smartworkz_alerts
    interval: 30s
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.05
        for: 5m
        annotations:
          summary: "High error rate detected ({{ $value | humanizePercentage }})"
      
      - alert: HighLatency
        expr: histogram_quantile(0.95, http_request_duration_seconds) > 0.5
        for: 5m
        annotations:
          summary: "P95 latency > 500ms: {{ $value }}"
      
      - alert: PodCrashLooping
        expr: rate(kube_pod_container_status_restarts_total[15m]) > 0.1
        annotations:
          summary: "Pod {{ $labels.pod }} is crash looping"
      
      - alert: HighMemoryUsage
        expr: container_memory_usage_bytes / 1e9 > 0.9
        annotations:
          summary: "Container using {{ $value }}GB memory"
```

**Deploy monitoring**:
```bash
# Deploy Prometheus
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm install prometheus prometheus-community/kube-prometheus-stack -n monitoring

# Deploy Grafana
helm repo add grafana https://grafana.github.io/helm-charts
helm install grafana grafana/grafana -n monitoring

# Access Grafana
kubectl port-forward -n monitoring svc/grafana 3000:80
# Open http://localhost:3000 (admin/admin)

# Import dashboards
# - Kubernetes cluster
# - Application metrics
# - Database performance
```

**Checklist**:
- [ ] Deploy Prometheus
- [ ] Deploy Grafana
- [ ] Create dashboards (API, Database, Kubernetes)
- [ ] Set up alerts (error rate, latency, memory)
- [ ] Connect to Slack/PagerDuty
- [ ] Test alerts (generate failures)

**Deliverable**: Full monitoring and alerting operational

---

## ✅ SPRINT COMPLETION CHECKLIST

### By End of Phase 3.0.1:

**Infrastructure**:
- [ ] Redis Cluster deployed (6 nodes, 192GB)
- [ ] Database sharded (4 shards x 2 replicas)
- [ ] Kubernetes cluster operational (3+ nodes)
- [ ] Connection pooling tuned (128 connections)

**Code**:
- [ ] RedisClusterCache implemented and tested
- [ ] ShardedDbContext working
- [ ] ReadReplicaRepository for reporting
- [ ] CacheWarmingService operational
- [ ] Kubernetes manifests created

**Testing**:
- [ ] Load tested to 10,000 concurrent users
- [ ] P95 latency < 100ms under load
- [ ] Database handles 100K queries/sec
- [ ] Zero errors under sustained load

**Monitoring**:
- [ ] Prometheus scraping metrics
- [ ] Grafana dashboards created
- [ ] Alerts configured
- [ ] Health checks operational

**Deployment**:
- [ ] Rolling updates work (zero downtime)
- [ ] Auto-scaling tested
- [ ] Failover scenarios tested
- [ ] Disaster recovery plan documented

---

## 📊 SUCCESS METRICS

```
PERFORMANCE:
  Target P95 latency:     < 100ms ✓
  Target throughput:      > 100K req/sec ✓
  Target error rate:      < 0.1% ✓

SCALE:
  Concurrent users:       10,000+ ✓
  Database queries/sec:   100K+ ✓
  Cache hit ratio:        > 95% ✓

RELIABILITY:
  Uptime:                 99.9% ✓
  MTTR (failover):        < 30 sec ✓
  Auto-scaling latency:   < 2 min ✓
```

---

## TEAM ASSIGNMENTS

```
Backend Engineers (2-3):
  • Task 1.x: Redis implementation
  • Task 2.x: Database scaling
  • Task 4.x: Performance optimization

DevOps Engineers (2):
  • Task 3.x: Kubernetes deployment
  • Monitoring & infrastructure

Database Engineer (1):
  • Task 2.x: Sharding strategy
  • Replication setup
  • Performance tuning
```

---

**Phase 3.0.1 Complete: Foundation built for 10M+ users** 🚀

Next: Phase 3.0.2 (Data & Analytics) begins after 2-week integration period

