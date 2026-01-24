@echo off
REM Build and analysis script for AppCore on Windows
REM This script runs all quality checks locally before pushing

echo 🚀 Starting AppCore build and analysis...

REM Check if .NET is installed
dotnet --version > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK is not installed or not in PATH
    exit /b 1
)

echo [INFO] Using .NET SDK version:
dotnet --version

REM Clean previous builds
echo [INFO] Cleaning previous builds...
dotnet clean --verbosity quiet
if exist TestResults rmdir /s /q TestResults

REM Restore dependencies
echo [INFO] Restoring dependencies...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to restore dependencies
    exit /b 1
)

REM Build the solution
echo [INFO] Building solution...
dotnet build --configuration Release --no-restore
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed
    exit /b 1
)
echo [SUCCESS] Build completed successfully

REM Run code formatting check
echo [INFO] Checking code formatting...
where dotnet-format > nul 2>&1
if %ERRORLEVEL% EQU 0 (
    dotnet format --verify-no-changes --verbosity diagnostic
    if %ERRORLEVEL% EQU 0 (
        echo [SUCCESS] Code formatting is correct
    ) else (
        echo [WARNING] Code formatting issues found. Run 'dotnet format' to fix them.
    )
) else (
    echo [WARNING] dotnet-format not installed. Install with: dotnet tool install -g dotnet-format
)

REM Create TestResults directory
if not exist TestResults mkdir TestResults

REM Run unit tests with coverage
echo [INFO] Running unit tests with code coverage...
dotnet test tests/AppCore.UnitTests ^
    --configuration Release ^
    --no-build ^
    --logger "trx;LogFileName=unit-tests.trx" ^
    --collect:"XPlat Code Coverage" ^
    --results-directory ./TestResults ^
    --verbosity minimal

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Unit tests failed
    exit /b 1
)
echo [SUCCESS] Unit tests passed

REM Run SpecFlow tests
echo [INFO] Running SpecFlow BDD tests...
dotnet test tests/AppCore.SpecFlow ^
    --configuration Release ^
    --no-build ^
    --logger "trx;LogFileName=specflow-tests.trx" ^
    --results-directory ./TestResults ^
    --verbosity minimal

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] SpecFlow tests failed
    exit /b 1
)
echo [SUCCESS] SpecFlow tests passed

REM Generate coverage report
echo [INFO] Generating code coverage report...
where reportgenerator > nul 2>&1
if %ERRORLEVEL% EQU 0 (
    reportgenerator ^
        "-reports:./TestResults/**/coverage.cobertura.xml" ^
        "-targetdir:./TestResults/Coverage" ^
        "-reporttypes:Html;Cobertura;TextSummary" ^
        -verbosity:Warning
    
    if exist "./TestResults/Coverage/Summary.txt" (
        echo [INFO] Coverage Summary:
        type "./TestResults/Coverage/Summary.txt"
    )
) else (
    echo [WARNING] ReportGenerator not installed. Install with: dotnet tool install -g dotnet-reportgenerator-globaltool
)

REM Package creation test
echo [INFO] Testing package creation...
if not exist packages mkdir packages
dotnet pack src/AppCore/AppCore.csproj ^
    --configuration Release ^
    --no-build ^
    --output ./packages ^
    --verbosity minimal

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Package creation failed
    exit /b 1
)
echo [SUCCESS] Package created successfully

REM List created packages
echo [INFO] Created packages:
dir packages\*.nupkg packages\*.snupkg

REM Final summary
echo.
echo [INFO] Build and analysis summary:
echo ✅ Build: Successful
echo ✅ Unit Tests: Passed
echo ✅ SpecFlow Tests: Passed
echo ✅ Package Creation: Successful

if exist "./TestResults/Coverage/index.html" (
    echo 📊 Code Coverage: Available in .\TestResults\Coverage\index.html
)

echo.
echo [SUCCESS] All checks completed successfully! 🎉
echo.
echo [INFO] Next steps:
echo   - Review coverage report: open .\TestResults\Coverage\index.html
echo   - Review test results: check .\TestResults\*.trx
echo   - Test package locally: dotnet add package .\packages\*.nupkg