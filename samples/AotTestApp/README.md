# AotTestApp - NativeAOT Validation Suite

## Overview

`AotTestApp` is a comprehensive test application designed to validate the **NativeAOT compatibility** of the `AppCore` library. It ensures that all public APIs, patterns, and features work correctly when compiled with ahead-of-time (AOT) compilation.

## Purpose

- **Validate AOT Compatibility**: Test that all `AppCore` features compile and run correctly with `PublishAot=true`
- **Performance Benchmarking**: Measure performance characteristics of AOT-compiled code
- **Regression Prevention**: Detect AOT-breaking changes early in the development cycle
- **Warning Monitoring**: Track and validate expected IL trimming warnings (IL2026, IL3050, IL2091)

## Test Coverage

### 1. Exception Tests (`Tests/ExceptionTests.cs`)
- ✅ Exception hierarchy creation and inheritance
- ✅ Pattern matching with switch expressions (no reflection)
- ✅ Exception serialization with `ValidationException.Errors`
- ✅ `[CallerMemberName]` attribute functionality

### 2. JSON Serialization Tests (`Tests/JsonSerializationTests.cs`)
- ✅ `AppCoreJsonContext` source generator validation
- ✅ `Response<T>` serialization for multiple types
- ✅ `PaginationResponse<T>` serialization
- ✅ Nested generic types (`Response<PaginationResponse<T>>`)
- ✅ Round-trip serialization/deserialization

### 3. Collection Expression Tests (`Tests/CollectionTests.cs`)
- ✅ C# 14 collection expressions `[]`
- ✅ Spread operator `[..collection]`
- ✅ Mixed spreads and elements `[x, ..arr, y]`
- ✅ Array/List conversions
- ✅ Collection expressions in DTOs

### 4. Integration Tests (in `Program.cs`)
- ✅ Response wrapper creation
- ✅ PaginationResponse with collection expressions
- ✅ JSON serialization with `AppCoreJsonContext`

## Running Tests

### Local Execution

#### Run all tests (default):
```bash
cd samples/AotTestApp
dotnet run
```

#### Run tests only (no benchmarks):
```bash
dotnet run -- --tests
# or
dotnet run -- -t
```

#### Run benchmarks only:
```bash
dotnet run -- --benchmark
# or
dotnet run -- -b
```

#### Run everything:
```bash
dotnet run -- --all
# or
dotnet run -- -a
```

### Automated AOT Validation Script

Use the comprehensive validation script that builds, publishes with AOT, analyzes warnings, and runs tests:

```bash
# From repository root
./build/scripts/run-aot-tests.sh
```

This script performs:
1. ✅ **Build AppCore** library
2. ✅ **Publish with AOT** (`PublishAot=true`)
3. ✅ **Analyze IL warnings** against expected thresholds
4. ✅ **Analyze binary** (ELF format, size, metadata)
5. ✅ **Execute tests** with result capture
6. ✅ **Generate summary report**

Outputs are saved in `TestResults/aot/`:
- `publish.log` - AOT compilation output with warnings
- `standard-tests.log` - Test execution results
- `benchmarks.log` - Performance benchmark data
- `summary.txt` - Complete validation summary

## Expected AOT Warnings

The following IL trimming warnings are **expected and acceptable**:

| Warning | Count | Reason |
|---------|-------|--------|
| IL2026  | ≤ 23  | RequiresUnreferencedCode (EntityFramework, third-party libs) |
| IL3050  | ≤ 21  | RequiresDynamicCode (EF expression compilation) |
| IL2091  | ≤ 1   | Generic parameter type requirements |
| **Total** | **≤ 45** | **Validated as safe** |

⚠️ **Warning Threshold Violations**: If warnings exceed these thresholds, the validation script will **fail** to prevent regressions.

## CI/CD Integration

### GitHub Actions

The AOT validation is integrated into `.github/workflows/ci-cd.yml`:

```yaml
- name: Run AOT Validation Tests
  run: ./build/scripts/run-aot-tests.sh
```

### Build Scripts

Add to `build/scripts/build-and-analyze.sh` (optional):

```bash
# Optional: Run AOT validation tests
if [ "${RUN_AOT_TESTS}" = "true" ]; then
    echo "Running AOT validation tests..."
    ./build/scripts/run-aot-tests.sh
fi
```

Enable with: `RUN_AOT_TESTS=true ./build/scripts/build-and-analyze.sh`

## Performance Benchmarks

When run with `--benchmark` or `benchmark` argument, the application executes performance tests:

- **Response Wrapper Creation**: 100,000 iterations
- **PaginationResponse Creation**: 10,000 iterations (with collection expressions)
- **Memory Usage**: GC statistics (Gen0/Gen1/Gen2 collections)

Example output:
```
  Response Wrapper: 100,000 iterations
    Total Time: 45.23 ms
    Per Operation: 0.000452 ms
    Operations/sec: 2,210,884

  Memory Used: 2.45 MB
  GC Gen0 Collections: 3
  GC Gen1 Collections: 1
  GC Gen2 Collections: 0
```

## Project Structure

```
samples/AotTestApp/
├── Program.cs                          # Main orchestration and CLI
├── Tests/
│   ├── ExceptionTests.cs               # Exception hierarchy validation
│   ├── JsonSerializationTests.cs       # JSON + source generator tests
│   └── CollectionTests.cs              # C# 14 collection expressions
├── AotTestApp.csproj                   # Project with PublishAot=true
└── README.md                           # This file
```

## Key Configuration

### AotTestApp.csproj

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net10.0</TargetFramework>
  <LangVersion>14</LangVersion>
  <Nullable>enable</Nullable>
  <PublishAot>true</PublishAot>                    <!-- Enable AOT -->
  <InvariantGlobalization>true</InvariantGlobalization>
  <StripSymbols>false</StripSymbols>               <!-- Keep symbols for debugging -->
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="../../src/AppCore/AppCore.csproj" />
</ItemGroup>
```

## Troubleshooting

### Test Failures

If tests fail:
1. Check `TestResults/aot/standard-tests.log` for error details
2. Verify `AppCore` library builds successfully (`dotnet build`)
3. Ensure .NET 10 SDK is installed (`dotnet --version`)
4. Check for new IL warnings in `TestResults/aot/publish.log`

### Warning Threshold Exceeded

If `run-aot-tests.sh` reports warning count exceeded:
1. Review new warnings in `publish.log`
2. Determine if warnings are acceptable (from dependencies)
3. Update thresholds in `run-aot-tests.sh` if justified
4. Document new warnings in this README

### AOT Compilation Failure

If `dotnet publish` fails:
1. Check for reflection usage in new code
2. Ensure JSON types are registered in `AppCoreJsonContext`
3. Validate no dynamic code generation (LINQ expressions, etc.)
4. Use `[DynamicallyAccessedMembers]` or `[RequiresUnreferencedCode]` as needed

## Adding New Tests

To add coverage for new `AppCore` features:

1. **Create Test File**: Add to `Tests/` directory (e.g., `Tests/NewFeatureTests.cs`)
2. **Implement `RunAll()` Method**: Follow existing patterns
3. **Update Program.cs**: Add `NewFeatureTests.RunAll()` call in `Main()`
4. **Run Validation**: Execute `./build/scripts/run-aot-tests.sh`
5. **Document**: Update this README with new coverage

## References

- [NativeAOT Deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [System.Text.Json Source Generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)
- [IL Trim Warnings](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-warnings)
- [Collection Expressions (C# 14)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions)

---

**Last Updated**: January 2026  
**Maintainer**: OrionSoft Development Team  
**Status**: ✅ All tests passing | 45 expected warnings
