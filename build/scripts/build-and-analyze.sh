#!/bin/bash

# Build and analysis script for AppCore
# This script runs all quality checks locally before pushing

set -e

echo "🚀 Starting AppCore build and analysis..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK is not installed or not in PATH"
    exit 1
fi

print_status "Using .NET SDK version: $(dotnet --version)"

# Clean previous builds
print_status "Cleaning previous builds..."
dotnet clean --verbosity quiet
rm -rf TestResults/ || true

# Restore dependencies
print_status "Restoring dependencies..."
dotnet restore

# Build the solution
print_status "Building solution..."
if dotnet build --configuration Release --no-restore; then
    print_success "Build completed successfully"
else
    print_error "Build failed"
    exit 1
fi

# Run code formatting check
print_status "Checking code formatting..."
if command -v dotnet-format &> /dev/null; then
    if dotnet format --verify-no-changes --verbosity diagnostic; then
        print_success "Code formatting is correct"
    else
        print_warning "Code formatting issues found. Run 'dotnet format' to fix them."
    fi
else
    print_warning "dotnet-format not installed. Install with: dotnet tool install -g dotnet-format"
fi

# Run unit tests with coverage
print_status "Running unit tests with code coverage..."
mkdir -p TestResults

if dotnet test tests/AppCore.UnitTests \
    --configuration Release \
    --no-build \
    --logger "trx;LogFileName=unit-tests.trx" \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults \
    --verbosity minimal; then
    print_success "Unit tests passed"
else
    print_error "Unit tests failed"
    exit 1
fi

# Run SpecFlow tests
print_status "Running SpecFlow BDD tests..."
if dotnet test tests/AppCore.SpecFlow \
    --configuration Release \
    --no-build \
    --logger "trx;LogFileName=specflow-tests.trx" \
    --results-directory ./TestResults \
    --verbosity minimal; then
    print_success "SpecFlow tests passed"
else
    print_error "SpecFlow tests failed"
    exit 1
fi

# Generate coverage report
print_status "Generating code coverage report..."
if command -v reportgenerator &> /dev/null; then
    reportgenerator \
        "-reports:./TestResults/**/coverage.cobertura.xml" \
        "-targetdir:./TestResults/Coverage" \
        "-reporttypes:Html;Cobertura;TextSummary" \
        -verbosity:Warning
    
    if [ -f "./TestResults/Coverage/Summary.txt" ]; then
        print_status "Coverage Summary:"
        cat "./TestResults/Coverage/Summary.txt"
        
        # Extract coverage percentage
        COVERAGE_LINE=$(grep "Line coverage" "./TestResults/Coverage/Summary.txt" || true)
        if [ ! -z "$COVERAGE_LINE" ]; then
            COVERAGE_PERCENT=$(echo "$COVERAGE_LINE" | grep -oE '[0-9]+\.[0-9]+%')
            COVERAGE_NUM=$(echo "$COVERAGE_PERCENT" | grep -oE '[0-9]+\.[0-9]+')
            
            if (( $(echo "$COVERAGE_NUM >= 80" | bc -l 2>/dev/null) )); then
                print_success "Code coverage ($COVERAGE_PERCENT) meets the 80% threshold"
            else
                print_warning "Code coverage ($COVERAGE_PERCENT) is below the 80% threshold"
            fi
        fi
    fi
else
    print_warning "ReportGenerator not installed. Install with: dotnet tool install -g dotnet-reportgenerator-globaltool"
fi

# Run security analysis
print_status "Running security analysis..."
if command -v security-scan &> /dev/null; then
    if security-scan --project AppCore.sln --output security-report.json; then
        print_success "Security analysis completed"
    else
        print_warning "Security analysis found potential issues. Check security-report.json"
    fi
else
    print_warning "Security scanner not installed. Install with: dotnet tool install -g security-scan"
fi

# Package creation test
print_status "Testing package creation..."
if dotnet pack src/AppCore/AppCore.csproj \
    --configuration Release \
    --no-build \
    --output ./packages \
    --verbosity minimal; then
    print_success "Package created successfully"
    
    # List created packages
    print_status "Created packages:"
    ls -la packages/*.nupkg packages/*.snupkg 2>/dev/null || true
else
    print_error "Package creation failed"
    exit 1
fi

# Final summary
print_status "Build and analysis summary:"
echo "✅ Build: Successful"
echo "✅ Unit Tests: Passed"
echo "✅ SpecFlow Tests: Passed"
echo "✅ Package Creation: Successful"

if [ -f "./TestResults/Coverage/Summary.txt" ]; then
    echo "📊 Code Coverage: Available in ./TestResults/Coverage/index.html"
fi

if [ -f "security-report.json" ]; then
    echo "🔒 Security Analysis: Available in security-report.json"
fi

print_success "All checks completed successfully! 🎉"

echo ""
print_status "Next steps:"
echo "  - Review coverage report: open ./TestResults/Coverage/index.html"
echo "  - Review test results: check ./TestResults/*.trx"
echo "  - Test package locally: dotnet add package ./packages/*.nupkg"