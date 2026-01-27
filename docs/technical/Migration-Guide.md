# AppCore Transfer & Migration Guide

This guide details how to migrate existing applications to use the new AppCore restructuring and GitHub Packages feed.

## 1. Connecting to GitHub Packages

AppCore is now hosted exclusively on GitHub Packages. You must configure your environment to access it.

### Step 1: Generate a PAT (Personal Access Token)
1. Go to GitHub Settings > Developer settings > Personal access tokens > Tokens (classic).
2. Generate a new token with `read:packages` scope.
3. Authorize the token for the `OrionSoft` organization (if applicable) or your user account.

### Step 2: Configure `nuget.config`
Add a `nuget.config` file to your solution root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/Harol-Reina/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_PAT" />
    </github>
  </packageSourceCredentials>
</configuration>
```

> [!WARNING]
> Do not commit your PAT to source control. Use environment variables or global `nuget.config` (~/.nuget/NuGet/NuGet.Config) for local development, and Secrets for CI/CD.

## 2. Updating Project References

Remove direct project references and replace them with the NuGet package.

### Old (.csproj)
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\AppCore\AppCore.csproj" />
</ItemGroup>
```

### New (.csproj)
```xml
<ItemGroup>
  <PackageReference Include="OrionSoft.AppCore" Version="1.0.0-preview.1" />
</ItemGroup>
```

## 3. Adapting to NativeAOT Changes

AppCore is now fully compatible with NativeAOT. If you are migrating a standard application, these changes are optional but recommended. If you are building an AOT application, they are **mandatory**.

### JSON Serialization
**Deprecated (Reflection):**
```csharp
JsonSerializer.Serialize(myObject);
```

**New (Source Generation):**
Use `AppCoreJsonContext` for serialization:
```csharp
var options = new JsonSerializerOptions { 
    TypeInfoResolver = AppCoreJsonContext.Default 
};
JsonSerializer.Serialize(myObject, options);
```

### AutoMapper Replacement
AutoMapper has been removed due to AOT incompatibility.
Use `IMappingService<TSource, TDestination>`:

```csharp
// Inject
private readonly IMappingService<User, UserDto> _mapper;

// Use
var dto = _mapper.Map(user);
```

### Exception Handling
`ValidationBehaviour` now uses Source Generated validators. Ensure your validators are registered in the DI container.

## 4. Modern C# 14 Features

The library now exploits C# 14 features.

- Use **Collection Expressions** (`[]`) instead of `new List<T>()`.
- Use **Primary Constructors** for DTOs.
- Use **Pattern Matching** for cleaner logic.

## 5. Troubleshooting

**Error: 401 Unauthorized from GitHub Packages**
- Verify your PAT has `read:packages`.
- Ensure you are using the correct username in `nuget.config`.

**Error: IL2026/IL3050 Warnings**
- These are AOT analysis warnings. If you are not building for AOT, you can suppress them or ignore them.
- If building for AOT, ensure you are not using reflection-based APIs.
