#!/bin/bash
set -e

echo "=========================================="
echo "SmartWorkz EF Core Migrations Deployment"
echo "=========================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Change to src directory
cd "$(dirname "$0")/../src/SmartWorkz.StarterKitMVC.Infrastructure"

# Check that the project is built
if [ ! -f "bin/Release/net9.0/SmartWorkz.StarterKitMVC.Infrastructure.dll" ]; then
    echo -e "${RED}Error: Project not built. Run 'dotnet build -c Release' first.${NC}"
    exit 1
fi

echo -e "${YELLOW}Running EF Core migrations for all DbContexts...${NC}"
echo ""

# Function to run migration for a context
run_migration() {
    local context=$1
    echo -e "${YELLOW}► Running migration for $context...${NC}"

    if dotnet ef database update --context "$context" --no-build; then
        echo -e "${GREEN}✓ $context migrated successfully${NC}"
    else
        echo -e "${RED}✗ $context migration failed${NC}"
        exit 1
    fi
    echo ""
}

# Run migrations for all contexts
run_migration "AuthDbContext"
run_migration "MasterDbContext"
run_migration "SharedDbContext"
run_migration "TransactionDbContext"
run_migration "ReportDbContext"

echo -e "${GREEN}=========================================="
echo "✓ All migrations completed successfully"
echo "==========================================${NC}"
exit 0
