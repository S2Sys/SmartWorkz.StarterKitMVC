#!/bin/bash

# SmartWorkz SDK Build Script
# This script builds the Node.js/TypeScript SDK for npm distribution

set -e

echo "SmartWorkz SDK Build Process"
echo "============================"
echo ""

# Check if Node.js and npm are installed
if ! command -v node &> /dev/null; then
    echo "ERROR: Node.js is not installed"
    echo "Please install Node.js 14+ from https://nodejs.org/"
    exit 1
fi

if ! command -v npm &> /dev/null; then
    echo "ERROR: npm is not installed"
    echo "Please install npm or Node.js from https://nodejs.org/"
    exit 1
fi

echo "Node.js version: $(node --version)"
echo "npm version: $(npm --version)"
echo ""

# Install dependencies
echo "1. Installing dependencies..."
npm install
echo "   ✓ Dependencies installed"
echo ""

# Run TypeScript compiler
echo "2. Compiling TypeScript..."
npm run build
echo "   ✓ TypeScript compiled successfully"
echo ""

# Verify dist directory
if [ -d "dist" ]; then
    echo "3. Verifying build output..."
    if [ -f "dist/index.js" ] && [ -f "dist/index.d.ts" ]; then
        echo "   ✓ Build output verified"
        echo ""
        echo "Build completed successfully!"
        echo "Output: ./dist/"
        echo ""
        echo "Next steps:"
        echo "  - Run tests: npm test"
        echo "  - Create package: npm pack"
        echo "  - Publish: npm publish (requires npm account)"
    else
        echo "ERROR: Build output is incomplete"
        echo "Missing: dist/index.js or dist/index.d.ts"
        exit 1
    fi
else
    echo "ERROR: dist directory was not created"
    exit 1
fi
