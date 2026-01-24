#!/bin/bash

# Coverage collection and reporting script for AppCore
# This script collects coverage from all test projects and generates unified reports

set -e

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

print_info() {
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

# Configuration
# Find the solution directory (where AppCore.sln is located)
SCRIPT_DIR=$(dirname "$(readlink -f "$0")")
SOLUTION_DIR=$(cd "$SCRIPT_DIR/../.." && pwd)
TEST_RESULTS_DIR="$SOLUTION_DIR/TestResults"
COVERAGE_DIR="$TEST_RESULTS_DIR/Coverage"
COVERAGE_REPORTS="$TEST_RESULTS_DIR/**/coverage.cobertura.xml"

print_info "Starting coverage collection for AppCore..."
print_info "Solution directory: $SOLUTION_DIR"

# Clean previous results
if [ -d "$TEST_RESULTS_DIR" ]; then
    print_info "Cleaning previous test results..."
    rm -rf "$TEST_RESULTS_DIR"
fi

mkdir -p "$TEST_RESULTS_DIR"
mkdir -p "$COVERAGE_DIR"

# Change to solution directory for dotnet commands
cd "$SOLUTION_DIR"

# Run unit tests with coverage
print_info "Running unit tests with coverage collection..."
dotnet test tests/AppCore.UnitTests \
    --configuration Release \
    --logger "trx;LogFileName=unit-tests.trx" \
    --collect:"XPlat Code Coverage" \
    --results-directory "$TEST_RESULTS_DIR" \
    --settings "$SOLUTION_DIR/build/coverage/coverage.runsettings" || {
    print_error "Unit tests failed"
    exit 1
}

# Run SpecFlow tests with coverage  
print_info "Running SpecFlow tests with coverage collection..."
dotnet test tests/AppCore.SpecFlow \
    --configuration Release \
    --logger "trx;LogFileName=specflow-tests.trx" \
    --collect:"XPlat Code Coverage" \
    --results-directory "$TEST_RESULTS_DIR" \
    --settings "$SOLUTION_DIR/build/coverage/coverage.runsettings" || {
    print_warning "SpecFlow tests failed (expected - step definitions not yet implemented)"
    print_info "Continuing with coverage report generation from unit tests..."
}

# Check if reportgenerator is available
if ! command -v reportgenerator &> /dev/null; then
    print_warning "ReportGenerator not found. Installing..."
    dotnet tool install --global dotnet-reportgenerator-globaltool || {
        print_error "Failed to install ReportGenerator"
        exit 1
    }
fi

# Generate unified coverage report
print_info "Generating unified coverage report..."
reportgenerator \
    "-reports:$COVERAGE_REPORTS" \
    "-targetdir:$COVERAGE_DIR" \
    "-reporttypes:Html;Cobertura;JsonSummary;Badges;TextSummary" \
    "-verbosity:Warning" \
    "-title:AppCore Coverage Report" \
    "-tag:$(git rev-parse --short HEAD 2>/dev/null || echo 'local')" || {
    print_error "Failed to generate coverage report"
    exit 1
}

# Parse coverage results
if [ -f "$COVERAGE_DIR/Summary.json" ]; then
    # Extract coverage percentage from JSON summary
    LINE_COVERAGE=$(jq -r '.summary.linecoverage' "$COVERAGE_DIR/Summary.json" 2>/dev/null || echo "0")
    BRANCH_COVERAGE=$(jq -r '.summary.branchcoverage' "$COVERAGE_DIR/Summary.json" 2>/dev/null || echo "0")
    
    print_success "Coverage Analysis Complete!"
    echo "📊 Line Coverage: ${LINE_COVERAGE}%"
    echo "📊 Branch Coverage: ${BRANCH_COVERAGE}%"
    
    # Check coverage thresholds
    THRESHOLD=80
    LINE_COVERAGE_NUM=$(echo "$LINE_COVERAGE" | cut -d'.' -f1)
    
    if [ "$LINE_COVERAGE_NUM" -ge "$THRESHOLD" ]; then
        print_success "✅ Coverage meets threshold (${LINE_COVERAGE}% >= ${THRESHOLD}%)"
    else
        print_warning "⚠️  Coverage below threshold (${LINE_COVERAGE}% < ${THRESHOLD}%)"
    fi
    
elif [ -f "$COVERAGE_DIR/Summary.txt" ]; then
    # Fallback to text summary
    print_info "Coverage Summary:"
    cat "$COVERAGE_DIR/Summary.txt"
    
    # Extract line coverage from text
    LINE_COVERAGE=$(grep -oE 'Line coverage: [0-9.]+%' "$COVERAGE_DIR/Summary.txt" | grep -oE '[0-9.]+' || echo "0")
    LINE_COVERAGE_NUM=$(echo "$LINE_COVERAGE" | cut -d'.' -f1)
    
    THRESHOLD=80
    if [ "$LINE_COVERAGE_NUM" -ge "$THRESHOLD" ] 2>/dev/null; then
        print_success "✅ Coverage meets threshold (${LINE_COVERAGE}% >= ${THRESHOLD}%)"
    else
        print_warning "⚠️  Coverage below threshold (${LINE_COVERAGE}% < ${THRESHOLD}%)"
    fi
else
    print_warning "Could not parse coverage results"
fi

# Generate badge
if [ -f "$COVERAGE_DIR/badge_linecoverage.svg" ]; then
    cp "$COVERAGE_DIR/badge_linecoverage.svg" "$SOLUTION_DIR/coverage-badge.svg"
    print_info "Coverage badge updated: coverage-badge.svg"
fi

# Output file locations
print_info "Coverage artifacts generated:"
echo "📄 HTML Report: file://$COVERAGE_DIR/index.html"
echo "📄 Cobertura XML: $COVERAGE_DIR/Cobertura.xml"
echo "📄 JSON Summary: $COVERAGE_DIR/Summary.json"

if [ "$1" = "--open" ]; then
    print_info "Opening coverage report..."
    if command -v xdg-open &> /dev/null; then
        xdg-open "$COVERAGE_DIR/index.html"
    elif command -v open &> /dev/null; then
        open "$COVERAGE_DIR/index.html"
    elif command -v start &> /dev/null; then
        start "$COVERAGE_DIR/index.html"
    else
        print_info "Please open: $COVERAGE_DIR/index.html"
    fi
fi

print_success "Coverage collection completed! 🎉"