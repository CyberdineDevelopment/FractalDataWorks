# Complete NuGet Package Management Solution for Templates

## ✅ What's Implemented

### 1. **Local NuGet Packages** (`templates/local-packages/`)
- Local package cache for development
- Pack script: `Pack-FractalDataWorksPackages.ps1`
- Used for testing templates before publishing

### 2. **NuGet.Config** (`templates/NuGet.Config`)
- Configures multiple package sources
- Package source mapping for FractalDataWorks.* packages
- Supports local, Azure Artifacts, and NuGet.org

### 3. **Updated Project Files**
- ✅ Use `PackageReference` instead of `ProjectReference`
- ✅ Source generators as analyzer packages
- ✅ Version="*" for latest packages

## 🎯 Template Parameters Needed

Add these to `template.json`:

```json
{
  "PackageSource": {
    "type": "parameter",
    "datatype": "choice",
    "choices": [
      {
        "choice": "Local",
        "description": "Use local packages folder (development)"
      },
      {
        "choice": "AzureArtifacts",
        "description": "Use Azure DevOps Artifacts feed"
      },
      {
        "choice": "NuGetOrg",
        "description": "Use public NuGet.org packages"
      }
    ],
    "defaultValue": "Local",
    "description": "Package source for FractalDataWorks dependencies"
  },
  "AzureArtifactsUrl": {
    "type": "parameter",
    "datatype": "string",
    "description": "Azure Artifacts feed URL (if using AzureArtifacts)",
    "defaultValue": "",
    "replaces": "AZURE_ARTIFACTS_URL"
  },
  "UseCentralPackageManagement": {
    "type": "parameter",
    "datatype": "bool",
    "description": "Use centrally managed package versions (Directory.Packages.props)",
    "defaultValue": "false"
  },
  "FractalDataWorksPackageVersion": {
    "type": "parameter",
    "datatype": "string",
    "description": "FractalDataWorks package version (* for latest, or specific like 1.0.0)",
    "defaultValue": "*",
    "replaces": "FRACTALDATAWORKS_VERSION"
  }
}
```

## 📁 Generated NuGet.Config (Conditional)

### Local Development
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="FractalDataWorksLocal" value="../../templates/local-packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="FractalDataWorksLocal">
      <package pattern="FractalDataWorks.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

### Azure Artifacts
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="FractalDataWorksArtifacts" value="AZURE_ARTIFACTS_URL" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="FractalDataWorksArtifacts">
      <package pattern="FractalDataWorks.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

### NuGet.org Only
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

## 📦 Central Package Management Support

### When `UseCentralPackageManagement = true`

**Generate:** `Directory.Packages.props`
```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- FractalDataWorks Framework -->
    <PackageVersion Include="FractalDataWorks.Abstractions" Version="FRACTALDATAWORKS_VERSION" />
    <PackageVersion Include="FractalDataWorks.Services.Abstractions" Version="FRACTALDATAWORKS_VERSION" />
    <PackageVersion Include="FractalDataWorks.Services" Version="FRACTALDATAWORKS_VERSION" />
    <PackageVersion Include="FractalDataWorks.Collections" Version="FRACTALDATAWORKS_VERSION" />
    <PackageVersion Include="FractalDataWorks.EnhancedEnums" Version="FRACTALDATAWORKS_VERSION" />
    <PackageVersion Include="FractalDataWorks.Results" Version="FRACTALDATAWORKS_VERSION" />
    <PackageVersion Include="FractalDataWorks.Configuration" Version="FRACTALDATAWORKS_VERSION" />

    <!-- Source Generators -->
    <PackageVersion Include="FractalDataWorks.Collections.SourceGenerators" Version="FRACTALDATAWORKS_VERSION" />
    <PackageVersion Include="FractalDataWorks.ServiceTypes.SourceGenerators" Version="FRACTALDATAWORKS_VERSION" />
    <PackageVersion Include="FractalDataWorks.Messages.SourceGenerators" Version="FRACTALDATAWORKS_VERSION" />

    <!-- Microsoft Packages -->
    <PackageVersion Include="Microsoft.Extensions.Configuration.Abstractions" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
  </ItemGroup>
</Project>
```

**Project files WITHOUT versions:**
```xml
<ItemGroup>
  <PackageReference Include="FractalDataWorks.Services.Abstractions" />
  <PackageReference Include="FractalDataWorks.Abstractions" />
  <!-- No Version attribute when using central management -->
</ItemGroup>
```

### When `UseCentralPackageManagement = false`

**Project files WITH versions:**
```xml
<ItemGroup>
  <PackageReference Include="FractalDataWorks.Services.Abstractions" Version="*" />
  <PackageReference Include="FractalDataWorks.Abstractions" Version="*" />
</ItemGroup>
```

## 🔄 Template File Structure

```
FractalDataWorks.Service.Domain/
├── .template.config/
│   └── template.json (with parameters)
├── FractalDataWorks.Services.DomainName.Abstractions/
│   ├── FractalDataWorks.Services.DomainName.Abstractions.csproj (no versions)
│   └── ... source files
├── FractalDataWorks.Services.DomainName/
│   ├── FractalDataWorks.Services.DomainName.csproj (no versions)
│   └── ... source files
├── NuGet.Local.Config (for Local)
├── NuGet.Azure.Config (for AzureArtifacts)
├── NuGet.Public.Config (for NuGetOrg)
└── Directory.Packages.props (optional)
```

## 🎮 Usage Examples

### Local Development (Default)
```powershell
dotnet new fractaldataworks-domain -n Billing
# Uses local packages from templates/local-packages
```

### Azure Artifacts
```powershell
dotnet new fractaldataworks-domain -n Billing `
  --PackageSource AzureArtifacts `
  --AzureArtifactsUrl "https://pkgs.dev.azure.com/fractaldataworks/_packaging/FractalDataWorks/nuget/v3/index.json"
# Generates NuGet.Config pointing to Azure Artifacts
```

### NuGet.org with Central Management
```powershell
dotnet new fractaldataworks-domain -n Billing `
  --PackageSource NuGetOrg `
  --UseCentralPackageManagement true `
  --FractalDataWorksPackageVersion "1.0.0"
# Generates Directory.Packages.props with version 1.0.0
```

### Specific Version
```powershell
dotnet new fractaldataworks-domain -n Billing `
  --FractalDataWorksPackageVersion "1.0.0-preview.1"
# Uses specific preview version
```

## 📝 Template.json Modifications Needed

```json
"sources": [
  {
    "modifiers": [
      // Conditional NuGet.Config inclusion
      {
        "condition": "(PackageSource == 'Local')",
        "rename": {
          "NuGet.Local.Config": "NuGet.Config"
        },
        "exclude": ["NuGet.Azure.Config", "NuGet.Public.Config"]
      },
      {
        "condition": "(PackageSource == 'AzureArtifacts')",
        "rename": {
          "NuGet.Azure.Config": "NuGet.Config"
        },
        "exclude": ["NuGet.Local.Config", "NuGet.Public.Config"]
      },
      {
        "condition": "(PackageSource == 'NuGetOrg')",
        "rename": {
          "NuGet.Public.Config": "NuGet.Config"
        },
        "exclude": ["NuGet.Local.Config", "NuGet.Azure.Config"]
      },
      // Conditional Directory.Packages.props
      {
        "condition": "(!UseCentralPackageManagement)",
        "exclude": ["Directory.Packages.props"]
      }
    ]
  }
]
```

## 🚀 Workflow

### Step 1: Pack FractalDataWorks Framework
```powershell
cd C:\development\github\Developer-Kit
.\templates\Pack-FractalDataWorksPackages.ps1
```

### Step 2: Test Template Locally
```powershell
# Reinstall template
dotnet new uninstall FractalDataWorks.Service.Domain.Template
dotnet new install templates/FractalDataWorks.Service.Domain

# Create test project
cd temp-test
dotnet new fractaldataworks-domain -n TestDomain --PackageSource Local

# Build (should succeed with local packages)
cd TestDomain
dotnet restore
dotnet build
```

### Step 3: Push to Azure Artifacts
```powershell
dotnet nuget push templates/local-packages/*.nupkg `
  --source https://pkgs.dev.azure.com/fractaldataworks/_packaging/FractalDataWorks/nuget/v3/index.json `
  --api-key az
```

### Step 4: Test with Azure Artifacts
```powershell
dotnet new fractaldataworks-domain -n ProductionDomain `
  --PackageSource AzureArtifacts `
  --AzureArtifactsUrl "https://pkgs.dev.azure.com/fractaldataworks/_packaging/FractalDataWorks/nuget/v3/index.json"
```

## ✅ Benefits

1. **Local Development**: Fast iteration without publishing
2. **Team Collaboration**: Azure Artifacts for consistent versions
3. **External Distribution**: NuGet.org for public consumption
4. **Central Management**: Optional centralized versioning
5. **Flexibility**: Switch between sources via parameters
6. **No Manual Edits**: All configuration generated by template

## 📋 Checklist

- [x] Pack script created
- [x] NuGet.Config template created
- [x] Project files updated to use PackageReference
- [ ] Add parameters to template.json
- [ ] Create conditional NuGet.Config files
- [ ] Create Directory.Packages.props template
- [ ] Update project files to support central management
- [ ] Test all three package source scenarios
- [ ] Document in TEMPLATES_USAGE.md
- [ ] Set up Azure Artifacts feed
- [ ] Configure CI/CD to push packages

## 🎯 Next Implementation

The critical files to create:
1. `NuGet.Local.Config`
2. `NuGet.Azure.Config`
3. `NuGet.Public.Config`
4. `Directory.Packages.props`
5. Update `template.json` with all parameters and conditions

This provides a complete enterprise-grade package management solution!
