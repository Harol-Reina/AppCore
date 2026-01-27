# AppCore Release Process

This document outlines the automated release process for AppCore v2.0+ managed via GitHub Actions.

## Overview
The release pipeline is triggered by Git tags. It handles:
1. Versioning (MinVer)
2. Build & Test (including AOT validation)
3. Packing (NuGet & Snupkg)
4. Publishing to GitHub Packages

## Versioning Strategy
We use [MinVer](https://github.com/adamralph/minver) for semantic versioning.
- **Release:** Tag with `v1.0.0`
- **Preview:** Tag with `v1.0.0-preview.1`
- **CI/Dev:** Commits without tags generate generic preview versions (e.g., `0.0.0-alpha.0.5`).

## Triggering a Release

### 1. Pre-Release Validation
Ensure the local build passes and verify AOT compatibility:
```bash
./scripts/build-and-analyze.sh
dotnet run --project samples/AotTestApp/AotTestApp.csproj -- --all
```

### 2. Create Tag
```bash
git tag v1.0.0
git push origin v1.0.0
```

### 3. Monitor Pipeline
Go to [GitHub Actions](https://github.com/Harol-Reina/AppCore/actions) and monitor the "CI/CD Pipeline".
The pipeline has the following stages:
- **Build**: Compiles code and generates artifacts.
- **Test**: Runs Unit Checks and AOT analysis.
- **Pack**: Creates `.nupkg` and `.snupkg`.
- **Publish**: Pushes to GitHub Packages (requires successful tests).

## Verification
After success:
1. Check the [packages page](https://github.com/orgs/OrionSoft/packages) for the new version.
2. Verify symbols are available.
3. Check release notes in CHANGELOG.md (manual update required before tagging).

## Quality Gates
The pipeline enforces:
- **Test Coverage:** Must be ≥ 80%.
- **Analysis:** No critical security issues.
- **AOT Compatibility:** Clean AOT analysis required.

## Rollback
If a release is broken:
- You cannot delete a version from GitHub Packages easily (immutable).
- **Fix:** Fix the issue, commit, and tag a new patch version (e.g., `v1.0.1`).
- **Yank:** (Optional) Unlist the bad package version from the UI.
