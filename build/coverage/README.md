# Code Coverage Configuration for AppCore

## Tools Configuration

This directory contains configuration files for various code coverage tools used in the AppCore project.

### Coverage Tools Used

1. **Coverlet** - .NET Core code coverage library
2. **ReportGenerator** - Generates human-readable coverage reports  
3. **CodeCov** - Cloud-based coverage reporting (optional)
4. **SonarQube** - Code quality and coverage analysis

### Local Development

To generate coverage reports locally:

```bash
# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Generate HTML report
reportgenerator \
  "-reports:./TestResults/**/coverage.cobertura.xml" \
  "-targetdir:./TestResults/Coverage" \
  "-reporttypes:Html;Cobertura"

# Open report
open ./TestResults/Coverage/index.html  # macOS
start ./TestResults/Coverage/index.html # Windows
xdg-open ./TestResults/Coverage/index.html # Linux
```

### CI/CD Integration

Coverage collection is automatically configured in:
- GitHub Actions workflow (`.github/workflows/ci-cd.yml`)
- Build scripts (`build/scripts/`)

### Coverage Thresholds

- **Minimum Coverage**: 80%
- **Target Coverage**: 90%
- **Critical Path Coverage**: 95%

### Exclusions

The following are excluded from coverage analysis:
- Generated files (`*.generated.cs`)
- Test files (`**/*Tests.cs`, `**/*Test.cs`)
- Infrastructure boilerplate
- Third-party integrations