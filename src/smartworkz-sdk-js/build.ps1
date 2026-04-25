# SmartWorkz SDK Build Script (PowerShell)
# This script builds the Node.js/TypeScript SDK for npm distribution

param(
    [switch]$NoTest = $false,
    [switch]$Pack = $false
)

Write-Host "SmartWorkz SDK Build Process" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green
Write-Host ""

# Check if Node.js and npm are installed
$nodeVersion = node --version 2>$null
$npmVersion = npm --version 2>$null

if (-not $nodeVersion) {
    Write-Host "ERROR: Node.js is not installed" -ForegroundColor Red
    Write-Host "Please install Node.js 14+ from https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

if (-not $npmVersion) {
    Write-Host "ERROR: npm is not installed" -ForegroundColor Red
    Write-Host "Please install npm or Node.js from https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

Write-Host "Node.js version: $nodeVersion" -ForegroundColor Cyan
Write-Host "npm version: $npmVersion" -ForegroundColor Cyan
Write-Host ""

# Install dependencies
Write-Host "1. Installing dependencies..." -ForegroundColor Yellow
npm install
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to install dependencies" -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Dependencies installed" -ForegroundColor Green
Write-Host ""

# Run TypeScript compiler
Write-Host "2. Compiling TypeScript..." -ForegroundColor Yellow
npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: TypeScript compilation failed" -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ TypeScript compiled successfully" -ForegroundColor Green
Write-Host ""

# Verify dist directory
if (Test-Path "dist") {
    Write-Host "3. Verifying build output..." -ForegroundColor Yellow
    if ((Test-Path "dist/index.js") -and (Test-Path "dist/index.d.ts")) {
        Write-Host "   ✓ Build output verified" -ForegroundColor Green
        Write-Host ""

        $distSize = (Get-Item "dist" -Recurse | Measure-Object -Property Length -Sum).Sum / 1KB
        Write-Host "Build Summary:" -ForegroundColor Cyan
        Write-Host "  Output directory: ./dist/" -ForegroundColor Gray
        Write-Host "  Size: $('{0:N0}' -f $distSize) KB" -ForegroundColor Gray
        Write-Host ""

        # Run tests if not skipped
        if (-not $NoTest) {
            Write-Host "4. Running tests..." -ForegroundColor Yellow
            npm test 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ✓ Tests passed" -ForegroundColor Green
            } else {
                Write-Host "   ⚠ Tests failed or not configured" -ForegroundColor Yellow
            }
            Write-Host ""
        }

        # Create package if requested
        if ($Pack) {
            Write-Host "5. Creating npm package..." -ForegroundColor Yellow
            npm pack
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ✓ Package created" -ForegroundColor Green
            } else {
                Write-Host "   ✗ Failed to create package" -ForegroundColor Red
                exit 1
            }
            Write-Host ""
        }

        Write-Host "Build completed successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Cyan
        Write-Host "  - Run tests: npm test" -ForegroundColor Gray
        Write-Host "  - Create package: npm pack" -ForegroundColor Gray
        Write-Host "  - Publish: npm publish (requires npm account)" -ForegroundColor Gray
    } else {
        Write-Host "ERROR: Build output is incomplete" -ForegroundColor Red
        Write-Host "Missing: dist/index.js or dist/index.d.ts" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "ERROR: dist directory was not created" -ForegroundColor Red
    exit 1
}
