#!/bin/bash
# Script: build/scripts/run-aot-tests.sh
# Purpose: Execute AotTestApp with NativeAOT compilation and validate compatibility
# Description: Comprehensive validation of AppCore's NativeAOT compatibility through
#              automated compilation, execution, and analysis of the AotTestApp sample.

set -e

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
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

print_section() {
    echo -e "${CYAN}===================================================================${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}===================================================================${NC}"
}

# Configuration
SCRIPT_DIR=$(dirname "$(readlink -f "$0")")
SOLUTION_DIR=$(cd "$SCRIPT_DIR/../.." && pwd)
AOT_PROJECT_DIR="$SOLUTION_DIR/samples/AotTestApp"
PUBLISH_DIR="$AOT_PROJECT_DIR/bin/Release/net10.0/linux-x64/publish"
AOT_RESULTS_DIR="$SOLUTION_DIR/TestResults/AOT"
WEB_PORT=5280

print_section "AppCore NativeAOT Compatibility Validation"
print_info "Solution: $SOLUTION_DIR"
print_info "Project:  $AOT_PROJECT_DIR"
echo ""

# Check if project exists
if [ ! -d "$AOT_PROJECT_DIR" ]; then
    print_error "AotTestApp project not found at: $AOT_PROJECT_DIR"
    exit 1
fi

if [ ! -f "$AOT_PROJECT_DIR/AotTestApp.csproj" ]; then
    print_error "AotTestApp.csproj not found"
    exit 1
fi

# Clean previous results
if [ -d "$AOT_RESULTS_DIR" ]; then
    print_info "Cleaning previous AOT test results..."
    rm -rf "$AOT_RESULTS_DIR"
fi
mkdir -p "$AOT_RESULTS_DIR"

# Track timing
START_TIME=$(date +%s)

# Step 1: Build AppCore library
print_section "Step 1/6: Building AppCore Library"
dotnet build "$SOLUTION_DIR/src/AppCore/AppCore.csproj" \
    --configuration Release \
    --verbosity quiet

if [ $? -eq 0 ]; then
    print_success "AppCore library built successfully"
else
    print_error "AppCore library build failed"
    exit 1
fi
echo ""

# Step 2: Publish AotTestApp with NativeAOT
print_section "Step 2/6: Publishing with NativeAOT"
print_info "This may take 2-5 minutes depending on your system..."
print_info "Output: $PUBLISH_DIR"

PUBLISH_START=$(date +%s)

dotnet publish "$AOT_PROJECT_DIR/AotTestApp.csproj" \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained \
    --output "$PUBLISH_DIR" \
    --verbosity normal 2>&1 | tee "$AOT_RESULTS_DIR/publish.log"

PUBLISH_EXIT_CODE=${PIPESTATUS[0]}
PUBLISH_END=$(date +%s)
PUBLISH_DURATION=$((PUBLISH_END - PUBLISH_START))

if [ $PUBLISH_EXIT_CODE -eq 0 ]; then
    print_success "NativeAOT compilation completed in ${PUBLISH_DURATION}s"
else
    print_error "NativeAOT compilation failed (Exit code: $PUBLISH_EXIT_CODE)"
    print_error "Check detailed logs at: $AOT_RESULTS_DIR/publish.log"
    exit 1
fi
echo ""

# Step 3: Analyze warnings
print_section "Step 3/6: Analyzing AOT Warnings"

IL2026_COUNT=$(grep -c "warning IL2026" "$AOT_RESULTS_DIR/publish.log" || true)
IL3050_COUNT=$(grep -c "warning IL3050" "$AOT_RESULTS_DIR/publish.log" || true)
IL2091_COUNT=$(grep -c "warning IL2091" "$AOT_RESULTS_DIR/publish.log" || true)
TOTAL_WARNINGS=$(grep -c "warning IL" "$AOT_RESULTS_DIR/publish.log" || true)

echo "  IL2026 (RequiresUnreferencedCode):      $IL2026_COUNT"
echo "  IL3050 (RequiresDynamicCode):           $IL3050_COUNT"
echo "  IL2091 (DynamicallyAccessedMembers):    $IL2091_COUNT"
echo "  ─────────────────────────────────────────────────"
echo "  Total AOT Warnings:                     $TOTAL_WARNINGS"

EXPECTED_IL2026=23
EXPECTED_IL3050=21
EXPECTED_IL2091=1
EXPECTED_TOTAL=45

if [ "$IL2026_COUNT" -gt "$EXPECTED_IL2026" ]; then
    print_warning "IL2026 warnings increased! Expected: ≤$EXPECTED_IL2026, Got: $IL2026_COUNT"
fi

if [ "$IL3050_COUNT" -gt "$EXPECTED_IL3050" ]; then
    print_warning "IL3050 warnings increased! Expected: ≤$EXPECTED_IL3050, Got: $IL3050_COUNT"
fi

if [ "$TOTAL_WARNINGS" -gt "$EXPECTED_TOTAL" ]; then
    print_warning "Total warnings increased! Expected: ≤$EXPECTED_TOTAL, Got: $TOTAL_WARNINGS"
else
    print_success "AOT warning count within expected range"
fi
echo ""

# Step 4: Analyze binary
print_section "Step 4/6: Analyzing NativeAOT Binary"

if [ ! -f "$PUBLISH_DIR/AotTestApp" ]; then
    print_error "AotTestApp executable not found at: $PUBLISH_DIR/AotTestApp"
    exit 1
fi

chmod +x "$PUBLISH_DIR/AotTestApp"

BINARY_SIZE=$(du -h "$PUBLISH_DIR/AotTestApp" | cut -f1)
BINARY_SIZE_BYTES=$(stat -f%z "$PUBLISH_DIR/AotTestApp" 2>/dev/null || stat -c%s "$PUBLISH_DIR/AotTestApp" 2>/dev/null)
BINARY_SIZE_MB=$(echo "scale=2; $BINARY_SIZE_BYTES / 1024 / 1024" | bc)

echo "  Binary Size:         $BINARY_SIZE (${BINARY_SIZE_MB} MB)"
echo "  Binary Path:         $PUBLISH_DIR/AotTestApp"
echo "  Stripped Symbols:    $(grep -q 'StripSymbols>false' "$AOT_PROJECT_DIR/AotTestApp.csproj" && echo "No" || echo "Yes")"

# Check if binary is actually native
file "$PUBLISH_DIR/AotTestApp" > "$AOT_RESULTS_DIR/binary-info.txt"
IS_ELF=$(grep -c "ELF" "$AOT_RESULTS_DIR/binary-info.txt" || echo "0")

if [ "$IS_ELF" -gt 0 ]; then
    print_success "Binary is native ELF executable (not managed assembly)"
else
    print_warning "Binary may not be fully native AOT compiled"
fi
echo ""

# Step 5: Execute Console Tests & Benchmarks
print_section "Step 5/6: Executing Console Tests & Benchmarks"

# Run standard tests
print_info "Running standard compatibility tests..."
"$PUBLISH_DIR/AotTestApp" --tests 2>&1 | tee "$AOT_RESULTS_DIR/standard-tests.log"
TESTS_EXIT_CODE=${PIPESTATUS[0]}

if [ $TESTS_EXIT_CODE -eq 0 ]; then
    print_success "Standard AOT compatibility tests PASSED"
else
    print_error "Standard AOT compatibility tests FAILED (Exit code: $TESTS_EXIT_CODE)"
    echo ""
    print_error "Test output:"
    cat "$AOT_RESULTS_DIR/standard-tests.log"
    exit 1
fi
echo ""

# Run benchmarks
print_info "Running performance benchmarks..."
"$PUBLISH_DIR/AotTestApp" --benchmark 2>&1 | tee "$AOT_RESULTS_DIR/benchmarks.log"
BENCHMARK_EXIT_CODE=${PIPESTATUS[0]}

if [ $BENCHMARK_EXIT_CODE -eq 0 ]; then
    print_success "AOT performance benchmarks completed"

    # Extract benchmark metrics
    RESPONSE_OPS=$(grep "Operations/sec" "$AOT_RESULTS_DIR/benchmarks.log" | head -1 | awk '{print $2}')
    PAGINATION_OPS=$(grep "Operations/sec" "$AOT_RESULTS_DIR/benchmarks.log" | tail -1 | awk '{print $2}')
    MEMORY_USED=$(grep "Memory Used" "$AOT_RESULTS_DIR/benchmarks.log" | awk '{print $3, $4}')

    echo ""
    echo "  Performance Metrics:"
    echo "  ├─ Response Wrapper:  $RESPONSE_OPS ops/sec"
    echo "  ├─ Pagination:        $PAGINATION_OPS ops/sec"
    echo "  └─ Memory Used:       $MEMORY_USED"
else
    print_warning "AOT benchmarks failed (Exit code: $BENCHMARK_EXIT_CODE)"
fi
echo ""

# Step 6: Web Endpoint Validation (ExceptionHandlingMiddleware)
print_section "Step 6/6: Validating Web Endpoints (Middleware)"
print_info "Starting AOT binary in web mode on port $WEB_PORT..."

"$PUBLISH_DIR/AotTestApp" --urls "http://localhost:$WEB_PORT" > "$AOT_RESULTS_DIR/web-server.log" 2>&1 &
WEB_PID=$!

# Wait for server to be ready (up to 10 seconds)
WEB_READY=false
for i in $(seq 1 20); do
    if curl -s -o /dev/null -w "" "http://localhost:$WEB_PORT/api/v1/exceptions/success" 2>/dev/null; then
        WEB_READY=true
        break
    fi
    sleep 0.5
done

WEB_EXIT_CODE=0
WEB_PASSED=0
WEB_FAILED=0

if [ "$WEB_READY" = true ]; then
    print_success "AOT web server started (PID: $WEB_PID)"
    echo ""

    # Define expected endpoints and status codes
    declare -A ENDPOINTS=(
        ["success"]=200
        ["not-found"]=404
        ["bad-request"]=400
        ["validation"]=400
        ["validation-multiple"]=400
        ["unauthorized"]=401
        ["forbidden"]=403
        ["custom-error"]=400
        ["unhandled"]=500
    )

    # Ordered list for consistent output
    ENDPOINT_ORDER="success not-found bad-request validation validation-multiple unauthorized forbidden custom-error unhandled"

    echo "  Endpoint                          Expected  Actual  Status"
    echo "  ─────────────────────────────────────────────────────────────"

    for endpoint in $ENDPOINT_ORDER; do
        expected=${ENDPOINTS[$endpoint]}
        actual=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:$WEB_PORT/api/v1/exceptions/$endpoint")

        if [ "$actual" = "$expected" ]; then
            status="✓"
            WEB_PASSED=$((WEB_PASSED + 1))
        else
            status="✗"
            WEB_FAILED=$((WEB_FAILED + 1))
            WEB_EXIT_CODE=1
        fi

        printf "  %-35s %3s       %3s     %s\n" "/api/v1/exceptions/$endpoint" "$expected" "$actual" "$status"
    done

    echo ""
    echo "  Results: $WEB_PASSED passed, $WEB_FAILED failed (${#ENDPOINTS[@]} total)"

    # Kill the web server
    kill $WEB_PID 2>/dev/null
    wait $WEB_PID 2>/dev/null

    if [ $WEB_EXIT_CODE -eq 0 ]; then
        print_success "All web endpoint tests PASSED"
    else
        print_error "Some web endpoint tests FAILED"
    fi
else
    print_error "AOT web server failed to start within 10 seconds"
    kill $WEB_PID 2>/dev/null
    wait $WEB_PID 2>/dev/null
    print_error "Server log:"
    cat "$AOT_RESULTS_DIR/web-server.log"
    WEB_EXIT_CODE=1
fi
echo ""

# Calculate total time
END_TIME=$(date +%s)
TOTAL_DURATION=$((END_TIME - START_TIME))
MINUTES=$((TOTAL_DURATION / 60))
SECONDS=$((TOTAL_DURATION % 60))

# Generate comprehensive summary report
print_section "Generating Summary Report"

cat > "$AOT_RESULTS_DIR/summary.txt" <<EOF
AppCore NativeAOT Validation Summary
====================================
Date:                   $(date)
Build Configuration:    Release
Target Runtime:         linux-x64 (NativeAOT)
Total Execution Time:   ${MINUTES}m ${SECONDS}s

═══════════════════════════════════════════════════════════════════
BUILD RESULTS
═══════════════════════════════════════════════════════════════════
AppCore Library:        SUCCESS
NativeAOT Publish:      SUCCESS
Publish Duration:       ${PUBLISH_DURATION}s
Binary Size:            ${BINARY_SIZE_MB} MB
Binary Type:            $([ "$IS_ELF" -gt 0 ] && echo "Native ELF" || echo "Unknown")

═══════════════════════════════════════════════════════════════════
WARNING ANALYSIS
═══════════════════════════════════════════════════════════════════
IL2026 (RequiresUnreferencedCode):     $IL2026_COUNT  (Expected: ≤$EXPECTED_IL2026)
IL3050 (RequiresDynamicCode):          $IL3050_COUNT  (Expected: ≤$EXPECTED_IL3050)
IL2091 (DynamicallyAccessedMembers):   $IL2091_COUNT  (Expected: ≤$EXPECTED_IL2091)
─────────────────────────────────────────────────────────────────
Total Warnings:                        $TOTAL_WARNINGS  (Expected: ≤$EXPECTED_TOTAL)

Status: $([ "$TOTAL_WARNINGS" -le "$EXPECTED_TOTAL" ] && echo "✅ WITHIN EXPECTED RANGE" || echo "⚠️ WARNINGS INCREASED")

═══════════════════════════════════════════════════════════════════
TEST RESULTS
═══════════════════════════════════════════════════════════════════
Console Compatibility Tests:   $([ $TESTS_EXIT_CODE -eq 0 ] && echo "✅ PASSED" || echo "❌ FAILED")
Performance Benchmarks:        $([ $BENCHMARK_EXIT_CODE -eq 0 ] && echo "✅ COMPLETED" || echo "⚠️ FAILED")
Web Endpoint Validation:       $([ $WEB_EXIT_CODE -eq 0 ] && echo "✅ PASSED ($WEB_PASSED/$((WEB_PASSED + WEB_FAILED)) endpoints)" || echo "❌ FAILED ($WEB_FAILED failures)")

$([ $BENCHMARK_EXIT_CODE -eq 0 ] && cat <<PERF
Performance Metrics:
├─ Response Wrapper:            $RESPONSE_OPS ops/sec
├─ Pagination:                  $PAGINATION_OPS ops/sec
└─ Memory Footprint:            $MEMORY_USED
PERF
)

═══════════════════════════════════════════════════════════════════
ARTIFACTS
═══════════════════════════════════════════════════════════════════
Native Binary:          $PUBLISH_DIR/AotTestApp
Logs Directory:         $AOT_RESULTS_DIR/
Publish Log:            $AOT_RESULTS_DIR/publish.log
Test Output:            $AOT_RESULTS_DIR/standard-tests.log
Benchmark Results:      $AOT_RESULTS_DIR/benchmarks.log
Web Server Log:         $AOT_RESULTS_DIR/web-server.log
Binary Info:            $AOT_RESULTS_DIR/binary-info.txt

═══════════════════════════════════════════════════════════════════
CONCLUSION
═══════════════════════════════════════════════════════════════════
$([ $TESTS_EXIT_CODE -eq 0 ] && [ $WEB_EXIT_CODE -eq 0 ] && [ "$TOTAL_WARNINGS" -le "$EXPECTED_TOTAL" ] && echo "✅ AppCore is FULLY COMPATIBLE with NativeAOT" || echo "⚠️ Issues detected - review logs above")
EOF

# Display summary
cat "$AOT_RESULTS_DIR/summary.txt"

print_section "AOT Validation Completed"
print_info "Detailed summary saved to: $AOT_RESULTS_DIR/summary.txt"
echo ""

if [ $TESTS_EXIT_CODE -eq 0 ] && [ $WEB_EXIT_CODE -eq 0 ] && [ "$TOTAL_WARNINGS" -le "$EXPECTED_TOTAL" ]; then
    print_success "✅ All AOT compatibility checks PASSED"
    exit 0
else
    print_warning "⚠️ Some checks failed or warnings increased"
    exit 1
fi
