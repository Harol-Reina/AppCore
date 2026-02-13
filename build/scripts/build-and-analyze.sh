#!/bin/bash

# Build and analysis script for AppCore
# This script runs all quality checks locally before pushing

set -e

# Load shared functions and configuration
SCRIPT_DIR=$(dirname "$(readlink -f "$0")")
source "$SCRIPT_DIR/common.sh"
SOLUTION_DIR=$(cd "$SCRIPT_DIR/../.." && pwd)

echo "🚀 Starting AppCore build and analysis..."

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK is not installed or not in PATH"
    exit 1
fi

print_info "Using .NET SDK version: $(dotnet --version)"

# Clean previous builds and artifacts
print_info "Cleaning previous builds..."
dotnet clean --verbosity quiet
rm -rf TestResults/ || true
rm -rf packages/ || true

# Restore dependencies
print_info "Restoring dependencies..."
dotnet restore

# Build the solution
print_info "Building solution..."
if dotnet build --configuration Release --no-restore; then
    print_success "Build completed successfully"
else
    print_error "Build failed"
    exit 1
fi

# Run code formatting check
print_info "Checking code formatting..."
if command -v dotnet-format &> /dev/null; then
    if dotnet format --verify-no-changes --verbosity diagnostic; then
        print_success "Code formatting is correct"
    else
        print_warning "Code formatting issues found. Run 'dotnet format' to fix them."
    fi
else
    print_warning "dotnet-format not installed. Install with: dotnet tool install -g dotnet-format"
fi

# Run tests with coverage (delegated to collect-coverage.sh)
print_info "Running tests and collecting coverage..."
bash "$SOLUTION_DIR/build/coverage/collect-coverage.sh" --no-build

# Run security analysis
print_info "Running security analysis..."
print_info "Checking for vulnerable packages..."
SECURITY_OUTPUT=$(dotnet list AppCore.sln package --vulnerable --include-transitive 2>&1) || true
echo "$SECURITY_OUTPUT"

if echo "$SECURITY_OUTPUT" | grep -q "has the following vulnerable packages"; then
    print_warning "Vulnerable packages detected. Please review the output above and update affected dependencies."
    HAS_VULNERABILITIES=true
else
    print_success "No vulnerable packages found"
    HAS_VULNERABILITIES=false
fi

# Note: For static code analysis, Roslyn analyzers are already running as part of the build process.


# Package creation test
print_info "Testing package creation..."
if dotnet pack src/AppCore/AppCore.csproj \
    --configuration Release \
    --no-build \
    --output ./packages \
    --verbosity minimal; then
    print_success "Package created successfully"

    # List created packages
    print_info "Created packages:"
    ls -la packages/*.nupkg packages/*.snupkg 2>/dev/null || true
else
    print_error "Package creation failed"
    exit 1
fi

# Final summary
print_info "Build and analysis summary:"
echo "✅ Build: Successful"
echo "✅ Tests & Coverage: Passed"
echo "✅ Package Creation: Successful"

if [ -f "./TestResults/Coverage/Summary.txt" ]; then
    echo "📊 Code Coverage: Available in ./TestResults/Coverage/index.html"
fi

if [ "$HAS_VULNERABILITIES" = true ]; then
    echo "⚠️  Security: Vulnerable packages detected — review output above"
else
    echo "🔒 Security: No vulnerable packages found"
fi

print_success "All checks completed successfully! 🎉"

echo ""
print_info "Next steps:"
echo "  - Review coverage report: open ./TestResults/Coverage/index.html"
echo "  - Review test results: check ./TestResults/*.trx"
echo "  - Test package locally:"
echo "      dotnet nuget add source \$(pwd)/packages --name local-appcore"
echo "      dotnet add package OrionSoft.AppCore --source local-appcore"
