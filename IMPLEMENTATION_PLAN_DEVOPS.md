# DevOps & CI/CD Implementation Plan

**Goal:** Establish complete CI/CD pipeline with GitHub Actions, Docker containerization, automated testing, code quality checks, security scanning, and infrastructure monitoring.

**Architecture:** 
- GitHub Actions workflows for build, test, quality, security
- Docker multi-stage builds for Web, Mobile, Shared
- Health checks and readiness probes
- Prometheus metrics collection
- ELK stack for log aggregation (optional advanced)

**Tech Stack:** GitHub Actions, Docker, .NET 9, xUnit, SonarQube, OWASP Dependency Check, Prometheus

**Timeline:** 2-3 weeks, 1-2 DevOps engineers  
**Effort:** ~40 developer days  
**Priority:** CRITICAL - Blocks production deployment

---

## Current State vs Target

### Current
- No CI/CD pipeline
- No automated testing
- No code quality checks
- No security scanning
- No containerization
- No deployment automation

### Target
- Full CI/CD pipeline (build, test, quality, security, deploy)
- Automated unit, integration, E2E tests
- Code coverage reporting (SonarQube)
- SAST security scanning (Snyk)
- Dependency vulnerability scanning
- Docker images for all projects
- Automated staging/production deployment
- Health checks and monitoring

---

## Phase 1: GitHub Actions Setup (3-5 days)

### Task 1: Basic Build & Test Pipeline

**Files to create:**
- `.github/workflows/build.yml`
- `.github/workflows/test.yml`
- `.github/workflows/quality.yml`

```yaml
# .github/workflows/build.yml
name: Build

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
      with:
        fetch-depth: 0  # For SonarQube analysis
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore SmartWorkz.Core.sln
    
    - name: Build
      run: dotnet build SmartWorkz.Core.sln --no-restore --configuration Release
    
    - name: Build success notification
      if: success()
      run: echo "✓ Build completed successfully"
    
    - name: Build failure notification
      if: failure()
      run: |
        echo "✗ Build failed"
        exit 1
```

```yaml
# .github/workflows/test.yml
name: Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Install dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Run Unit Tests
      run: |
        dotnet test \
          SmartWorkz.Core.Web.Tests/SmartWorkz.Core.Web.Tests.csproj \
          SmartWorkz.Core.Mobile.Tests/SmartWorkz.Core.Mobile.Tests.csproj \
          SmartWorkz.Core.Shared.Tests/SmartWorkz.Core.Shared.Tests.csproj \
          --no-build --configuration Release \
          --logger "trx;LogFileName=test-results.trx" \
          --collect:"XPlat Code Coverage" \
          --settings CodeCoverage.runsettings
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage/coverage.xml
        fail_ci_if_error: true
    
    - name: Publish test results
      if: always()
      uses: EnricoMi/publish-unit-test-result-action@v2
      with:
        files: '**/test-results.trx'
    
    - name: Report test failures
      if: failure()
      run: |
        echo "::error::Tests failed"
        exit 1
```

```yaml
# .github/workflows/quality.yml
name: Code Quality

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  quality:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
      with:
        fetch-depth: 0  # Required for SonarQube
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Install SonarQube Scanner
      run: dotnet tool install --global dotnet-sonarscanner
    
    - name: Start SonarQube analysis
      run: |
        dotnet sonarscanner begin \
          /k:"smartworkz-core" \
          /d:sonar.login=${{ secrets.SONAR_TOKEN }} \
          /d:sonar.host.url=https://sonarqube.example.com
    
    - name: Restore dependencies
      run: dotnet restore SmartWorkz.Core.sln
    
    - name: Build
      run: dotnet build SmartWorkz.Core.sln --no-restore
    
    - name: End SonarQube analysis
      run: |
        dotnet sonarscanner end \
          /d:sonar.login=${{ secrets.SONAR_TOKEN }}
    
    - name: Run StyleCop analysis
      run: |
        dotnet build SmartWorkz.Core.sln \
          /p:EnforceCodeStyleInBuild=true \
          /p:EnableNETAnalyzers=true
    
    - name: Report quality issues
      if: failure()
      run: |
        echo "::error::Code quality checks failed"
        exit 1
```

- [ ] Create `.github/workflows/` directory structure
- [ ] Implement build.yml workflow
- [ ] Implement test.yml with code coverage
- [ ] Implement quality.yml with SonarQube
- [ ] Test workflows in repository
- [ ] Commit workflow files

**Effort:** 3-5 days

---

### Task 2: Security Scanning Workflows

```yaml
# .github/workflows/security.yml
name: Security Scan

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
  schedule:
    - cron: '0 2 * * 0'  # Weekly scan

jobs:
  sast:
    runs-on: ubuntu-latest
    name: SAST Scanning
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Run Snyk SAST scan
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        args: --severity-threshold=high
    
    - name: Upload Snyk results
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: snyk.sarif

  dependencies:
    runs-on: ubuntu-latest
    name: Dependency Check
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: List project dependencies
      run: dotnet list package --vulnerable
    
    - name: Run OWASP Dependency-Check
      uses: dependency-check/Dependency-Check_Action@main
      with:
        project: 'SmartWorkz.Core'
        path: '.'
        format: 'JSON'
        args: >
          --enableExperimental
          --enableVulnerability
    
    - name: Upload dependency results
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: 'dependency-check-report.sarif'

  container-scan:
    runs-on: ubuntu-latest
    name: Container Scan
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Build Docker image
      run: docker build -t smartworkz-core:latest .
    
    - name: Scan image with Trivy
      uses: aquasecurity/trivy-action@master
      with:
        image-ref: 'smartworkz-core:latest'
        format: 'sarif'
        output: 'trivy-results.sarif'
    
    - name: Upload Trivy results
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: 'trivy-results.sarif'
```

- [ ] Create security.yml workflow
- [ ] Configure Snyk integration
- [ ] Configure Dependency-Check
- [ ] Configure container scanning
- [ ] Commit security workflow

**Effort:** 2-3 days

---

## Phase 2: Docker Containerization (3-5 days)

### Task 3: Dockerfile for Web & API

```dockerfile
# Dockerfile for SmartWorkz.Core.Web
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj", "SmartWorkz.Core.Web/"]
COPY ["SmartWorkz.Core.Shared/SmartWorkz.Core.Shared.csproj", "SmartWorkz.Core.Shared/"]

# Restore dependencies
RUN dotnet restore "SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj"

# Copy remaining source
COPY . .
WORKDIR "/src/SmartWorkz.Core.Web"

# Build application
RUN dotnet build "SmartWorkz.Core.Web.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "SmartWorkz.Core.Web.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "SmartWorkz.Core.Web.dll"]
```

```yaml
# docker-compose.yml
version: '3.8'

services:
  web:
    build:
      context: .
      dockerfile: SmartWorkz.Core.Web/Dockerfile
    ports:
      - "5000:5000"
      - "5001:5001"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=postgres;Port=5432;Database=smartworkz;User Id=postgres;Password=password;
    depends_on:
      - postgres
      - redis

  postgres:
    image: postgres:15-alpine
    environment:
      - POSTGRES_DB=smartworkz
      - POSTGRES_PASSWORD=password
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  postgres_data:
```

- [ ] Create Dockerfile for Web project
- [ ] Create docker-compose.yml for local development
- [ ] Add health check endpoint
- [ ] Create Dockerfile for Mobile (MAUI)
- [ ] Test Docker builds

**Effort:** 3-5 days

---

## Phase 3: Deployment Pipeline (2-3 days)

### Task 4: Staging & Production Deployment

```yaml
# .github/workflows/deploy.yml
name: Deploy

on:
  push:
    branches: [ main ]
    paths-ignore:
      - 'docs/**'
      - 'README.md'

jobs:
  deploy-staging:
    runs-on: ubuntu-latest
    environment: staging
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Build Docker image
      run: |
        docker build -t smartworkz-core:${{ github.sha }} .
        docker tag smartworkz-core:${{ github.sha }} smartworkz-core:latest
    
    - name: Push to registry
      run: |
        echo ${{ secrets.REGISTRY_PASSWORD }} | docker login -u ${{ secrets.REGISTRY_USERNAME }} --password-stdin ${{ secrets.REGISTRY_URL }}
        docker push smartworkz-core:${{ github.sha }}
        docker push smartworkz-core:latest
    
    - name: Deploy to staging
      run: |
        curl -X POST \
          -H "Authorization: Bearer ${{ secrets.DEPLOY_TOKEN }}" \
          -H "Content-Type: application/json" \
          -d '{"version":"${{ github.sha }}","environment":"staging"}' \
          https://deploy.example.com/api/deploy
    
    - name: Health check
      run: |
        for i in {1..10}; do
          curl -f https://staging.example.com/health && exit 0
          sleep 10
        done
        exit 1

  deploy-production:
    runs-on: ubuntu-latest
    environment: production
    needs: deploy-staging
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Deploy to production
      run: |
        curl -X POST \
          -H "Authorization: Bearer ${{ secrets.DEPLOY_TOKEN }}" \
          -H "Content-Type: application/json" \
          -d '{"version":"${{ github.sha }}","environment":"production"}' \
          https://deploy.example.com/api/deploy
    
    - name: Verify deployment
      run: |
        for i in {1..10}; do
          curl -f https://api.example.com/health && exit 0
          sleep 10
        done
        exit 1
    
    - name: Notify deployment
      if: success()
      uses: slackapi/slack-github-action@v1
      with:
        payload: |
          {
            "text": "✓ Production deployment successful",
            "blocks": [
              {
                "type": "section",
                "text": {
                  "type": "mrkdwn",
                  "text": "*SmartWorkz.Core deployed to production*\n${{ github.sha }}\nAuthor: ${{ github.actor }}"
                }
              }
            ]
          }
```

- [ ] Create deploy.yml workflow
- [ ] Configure staging environment
- [ ] Configure production environment
- [ ] Add approval gates for production
- [ ] Add deployment notifications
- [ ] Test deployment pipeline

**Effort:** 2-3 days

---

## Phase 4: Monitoring & Observability (2-3 days)

### Task 5: Health Checks & Monitoring

```csharp
// Health Check Endpoint
public static void AddHealthChecks(this IServiceCollection services)
{
    services.AddHealthChecks()
        .AddCheck("Self", () => HealthCheckResult.Healthy(), tags: new[] { "ready", "live" })
        .AddDbContextCheck<ApplicationDbContext>(tags: new[] { "ready" })
        .AddRedis(Configuration["Redis:ConnectionString"], tags: new[] { "ready" })
        .AddUrlGroup(new Uri("https://api.example.com"), "External API", tags: new[] { "ready" });
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

```yaml
# Prometheus configuration
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'smartworkz'
    static_configs:
      - targets: ['localhost:9090']
```

- [ ] Add health check endpoints
- [ ] Configure Prometheus metrics
- [ ] Setup log aggregation
- [ ] Configure alerting rules
- [ ] Create monitoring dashboard

**Effort:** 2-3 days

---

## Summary

### Deliverables
- ✅ Complete GitHub Actions CI/CD pipeline
- ✅ Build automation
- ✅ Automated testing (unit, integration, E2E)
- ✅ Code quality scanning (SonarQube)
- ✅ Security scanning (SAST, DAST, dependency check)
- ✅ Docker containerization
- ✅ Docker Compose for local development
- ✅ Automated deployment (staging + production)
- ✅ Health checks and monitoring
- ✅ Deployment notifications

### Workflows Created
1. ✅ build.yml - Build verification
2. ✅ test.yml - Automated testing + coverage
3. ✅ quality.yml - Code quality checks
4. ✅ security.yml - Security scanning
5. ✅ deploy.yml - Staging + production deployment

### Success Metrics
- [ ] All workflows passing
- [ ] 80%+ code coverage
- [ ] Zero critical security findings
- [ ] Automated deployment working
- [ ] Health checks passing
- [ ] Monitoring dashboard active

### Timeline
- **Phase 1** (3-5 days): GitHub Actions setup
- **Phase 2** (2-3 days): Security scanning
- **Phase 3** (3-5 days): Docker containerization
- **Phase 4** (2-3 days): Deployment pipeline
- **Phase 5** (2-3 days): Monitoring & observability

**Total: 2-3 weeks, 1-2 DevOps engineers**

---

## Post-Implementation

### Continuous Improvement
- Monitor pipeline performance
- Optimize Docker build times
- Refine security scanning rules
- Add additional integration tests
- Implement performance benchmarks
- Setup compliance scanning (if needed)

### Operations Guide
- **Deploying**: Push to main branch, GitHub Actions handles build/test/deploy
- **Monitoring**: Check health endpoint at `/health` and `/ready`
- **Logging**: Access logs via Prometheus/ELK stack
- **Scaling**: Configure Kubernetes deployment manifests based on Docker images
- **Rollback**: GitHub Actions maintains deployment history for quick rollback

---

**Implementation Complete:** All 5 projects + DevOps have detailed plans ready for execution.
