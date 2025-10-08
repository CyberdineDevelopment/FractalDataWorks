# Local NuGet Package Cache

This directory contains locally built FractalDataWorks packages for template development and testing.

## Purpose

Templates use **NuGet package references** instead of project references. This folder serves as a local package source during development before packages are published to Azure Artifacts or NuGet.org.

## Setup

### 1. Pack Required FractalDataWorks Projects

```powershell
# Navigate to Developer-Kit root
cd C:\development\github\Developer-Kit

# Pack core projects
dotnet pack src/FractalDataWorks.Abstractions -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.Services.Abstractions -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.Services -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.Collections -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.EnhancedEnums -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.Results -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.Results.Abstractions -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.Configuration -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.Configuration.Abstractions -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.Messages -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.ServiceTypes -c Release -o templates/local-packages

# Pack source generators
dotnet pack src/FractalDataWorks.Collections.SourceGenerators -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.ServiceTypes.SourceGenerators -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.Messages.SourceGenerators -c Release -o templates/local-packages

# For connections
dotnet pack src/FractalDataWorks.Services.Connections.Abstractions -c Release -o templates/local-packages
dotnet pack src/FractalDataWorks.Services.Connections -c Release -o templates/local-packages
```

### 2. Verify Packages

```powershell
Get-ChildItem templates/local-packages/*.nupkg | Select-Object Name, Length, LastWriteTime
```

### 3. Use Templates with Local Packages

```powershell
# Create new domain with local packages
cd some-new-project
dotnet new fractaldataworks-domain -n MyDomain

# Restore uses local packages via NuGet.Config
dotnet restore
dotnet build
```

## NuGet.Config Integration

The `NuGet.Config` in the templates directory configures:
1. **FractalDataWorksLocal** - This local-packages folder (highest priority)
2. **FractalDataWorksAzureArtifacts** - Azure DevOps feed (commented by default)
3. **nuget.org** - Public packages (fallback)

## For Azure Artifacts

### Setup Azure Artifacts Feed

1. Create feed in Azure DevOps:
   ```
   Organization: {your-org}
   Feed Name: FractalDataWorks
   Visibility: Private
   ```

2. Get feed URL:
   ```
   https://pkgs.dev.azure.com/{org}/_packaging/FractalDataWorks/nuget/v3/index.json
   ```

3. Authenticate:
   ```powershell
   dotnet nuget add source "https://pkgs.dev.azure.com/{org}/_packaging/FractalDataWorks/nuget/v3/index.json" `
     --name FractalDataWorksAzureArtifacts `
     --username az `
     --password {PAT} `
     --store-password-in-clear-text
   ```

4. Push packages to Azure Artifacts:
   ```powershell
   dotnet nuget push templates/local-packages/*.nupkg `
     --source FractalDataWorksAzureArtifacts `
     --api-key az
   ```

### Enable Azure Artifacts in NuGet.Config

Uncomment these lines in `templates/NuGet.Config`:
```xml
<add key="FractalDataWorksAzureArtifacts" value="https://pkgs.dev.azure.com/{organization}/_packaging/{feed}/nuget/v3/index.json" />
```

And in packageSourceMapping:
```xml
<packageSource key="FractalDataWorksAzureArtifacts">
  <package pattern="FractalDataWorks.*" />
</packageSource>
```

## Workflow

### Local Development
1. Make changes to FractalDataWorks framework
2. Pack changed projects: `dotnet pack src/Project -o templates/local-packages`
3. Test templates use updated packages
4. Templates find packages in local-packages first

### Team Development (Azure Artifacts)
1. CI/CD packs and pushes to Azure Artifacts on commit
2. Developers authenticate to Azure Artifacts once
3. Templates restore from Azure Artifacts
4. Everyone uses same package versions

### External Distribution
1. Publish stable releases to NuGet.org
2. Templates reference public packages
3. External users don't need Azure Artifacts access

## Package Versioning

Use `version.json` (Nerdbank.GitVersioning) for consistent versioning:
- Local: `1.0.0-preview.{commits}`
- Azure Artifacts: `1.0.0-alpha.{build}`
- NuGet.org: `1.0.0` (stable releases)

## Cleaning

Remove old packages:
```powershell
Remove-Item templates/local-packages/*.nupkg
```

## .gitignore

Add to `.gitignore`:
```
templates/local-packages/*.nupkg
templates/local-packages/*.snupkg
```

Keep the README.md tracked so developers know how to set up.
