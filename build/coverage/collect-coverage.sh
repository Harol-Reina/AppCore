#!/bin/bash

# Coverage collection and reporting script for AppCore
# This script collects coverage from all test projects and generates unified reports
#
# Usage:
#   bash build/coverage/collect-coverage.sh            # build + test + coverage
#   bash build/coverage/collect-coverage.sh --no-build  # test + coverage (skip build)
#   bash build/coverage/collect-coverage.sh --open       # open HTML report after

set -e

# Load shared functions and configuration
SCRIPT_DIR=$(dirname "$(readlink -f "$0")")
source "$SCRIPT_DIR/../scripts/common.sh"

# Parse arguments
NO_BUILD=false
OPEN_REPORT=false
for arg in "$@"; do
    case "$arg" in
        --no-build) NO_BUILD=true ;;
        --open)     OPEN_REPORT=true ;;
    esac
done

# Configuration
SOLUTION_DIR=$(cd "$SCRIPT_DIR/../.." && pwd)
TEST_RESULTS_DIR="$SOLUTION_DIR/TestResults"
COVERAGE_DIR="$TEST_RESULTS_DIR/Coverage"
COVERAGE_REPORTS="$TEST_RESULTS_DIR/**/coverage.cobertura.xml"

DOTNET_NO_BUILD_FLAG=""
if [ "$NO_BUILD" = true ]; then
    DOTNET_NO_BUILD_FLAG="--no-build"
fi

print_info "Starting coverage collection for AppCore..."
print_info "Solution directory: $SOLUTION_DIR"
if [ "$NO_BUILD" = true ]; then
    print_info "Skipping build (--no-build)"
fi

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
    $DOTNET_NO_BUILD_FLAG \
    --logger "trx;LogFileName=unit-tests.trx" \
    --collect:"XPlat Code Coverage" \
    --results-directory "$TEST_RESULTS_DIR" \
    --settings "$SOLUTION_DIR/$COVERAGE_RUNSETTINGS" || {
    print_error "Unit tests failed"
    exit 1
}

# Run SpecFlow tests with coverage
print_info "Running SpecFlow tests with coverage collection..."
dotnet test tests/AppCore.SpecFlow \
    --configuration Release \
    $DOTNET_NO_BUILD_FLAG \
    --logger "trx;LogFileName=specflow-tests.trx" \
    --collect:"XPlat Code Coverage" \
    --results-directory "$TEST_RESULTS_DIR" \
    --settings "$SOLUTION_DIR/$COVERAGE_RUNSETTINGS" || {
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
    "-reporttypes:$COVERAGE_REPORT_TYPES" \
    "-verbosity:Error" \
    "-title:AppCore Coverage Report" \
    "-filefilters:$COVERAGE_FILE_FILTERS" \
    "-classfilters:$COVERAGE_CLASS_FILTERS" \
    "-tag:$(git rev-parse --short HEAD 2>/dev/null || echo 'local')" || {
    print_error "Failed to generate coverage report"
    exit 1
}

# Parse coverage results
if [ -f "$COVERAGE_DIR/Summary.json" ]; then
    # Extract coverage percentage from JSON summary
    LINE_COVERAGE=$(jq -r '.summary.linecoverage // 0' "$COVERAGE_DIR/Summary.json" 2>/dev/null || echo "0")
    BRANCH_COVERAGE=$(jq -r '.summary.branchcoverage // 0' "$COVERAGE_DIR/Summary.json" 2>/dev/null || echo "0")

    print_success "Coverage Analysis Complete!"
    echo "📊 Line Coverage: ${LINE_COVERAGE}%"
    echo "📊 Branch Coverage: ${BRANCH_COVERAGE}%"

    # Check coverage thresholds
    LINE_COVERAGE_NUM=$(echo "$LINE_COVERAGE" | cut -d'.' -f1)

    if [ "$LINE_COVERAGE_NUM" -ge "$COVERAGE_THRESHOLD" ]; then
        print_success "✅ Coverage meets threshold (${LINE_COVERAGE}% >= ${COVERAGE_THRESHOLD}%)"
    else
        print_warning "⚠️  Coverage below threshold (${LINE_COVERAGE}% < ${COVERAGE_THRESHOLD}%)"
    fi

elif [ -f "$COVERAGE_DIR/Summary.txt" ]; then
    # Fallback to text summary
    print_info "Coverage Summary:"
    cat "$COVERAGE_DIR/Summary.txt"

    # Extract line coverage from text
    LINE_COVERAGE=$(grep -oE 'Line coverage: [0-9.]+%' "$COVERAGE_DIR/Summary.txt" | grep -oE '[0-9.]+' || echo "0")
    LINE_COVERAGE_NUM=$(echo "$LINE_COVERAGE" | cut -d'.' -f1)

    if [ "$LINE_COVERAGE_NUM" -ge "$COVERAGE_THRESHOLD" ] 2>/dev/null; then
        print_success "✅ Coverage meets threshold (${LINE_COVERAGE}% >= ${COVERAGE_THRESHOLD}%)"
    else
        print_warning "⚠️  Coverage below threshold (${LINE_COVERAGE}% < ${COVERAGE_THRESHOLD}%)"
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

if [ "$OPEN_REPORT" = true ]; then
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
