# Package References Issue & Solution

## Problem Identified

The current template generates projects with **local project references** that won't work outside the FractalDataWorks solution:

```xml
<ProjectReference Include="..\FractalDataWorks.Services.Abstractions\FractalDataWorks.Services.Abstractions.csproj"/>
```

This causes:
1. ❌ Build failures when used outside the FractalDataWorks solution
2. ❌ Cannot be used by external developers
3. ❌ Defeats the purpose of distributable templates

## Solution: Dual Reference System

### Approach: Conditional References Based on Parameter

Add `UsePackageReferences` parameter (default: true) to switch between:

**Package References (External Use):**
```xml
<PackageReference Include="FractalDataWorks.Services.Abstractions" Version="*" />
<PackageReference Include="FractalDataWorks.Abstractions" Version="*" />
<PackageReference Include="FractalDataWorks.Collections" Version="*" />
<!-- etc -->
```

**Project References (Internal Development):**
```xml
<ProjectReference Include="..\FractalDataWorks.Services.Abstractions\..." />
<ProjectReference Include="..\FractalDataWorks.Abstractions\..." />
<!-- etc -->
```

### Implementation Steps

1. **Add template parameter:**
```json
"UsePackageReferences": {
  "type": "parameter",
  "datatype": "bool",
  "description": "Use NuGet packages instead of project references",
  "defaultValue": "true"
},
"FractalDataWorksPackageVersion": {
  "type": "parameter",
  "datatype": "string",
  "description": "FractalDataWorks package version (* for latest)",
  "defaultValue": "*",
  "replaces": "FRACTALDATAWORKS_VERSION"
}
```

2. **Create two versions of .csproj:**
   - `FractalDataWorks.Services.DomainName.Abstractions.csproj` (package refs)
   - `FractalDataWorks.Services.DomainName.Abstractions.local.csproj` (project refs)

3. **Use conditional source modifiers:**
```json
"sources": [
  {
    "modifiers": [
      {
        "condition": "(UsePackageReferences)",
        "exclude": ["**/*.local.csproj"],
        "rename": {
          "FractalDataWorks.Services.DomainName.Abstractions.csproj": "FractalDataWorks.Services.DomainName.Abstractions.csproj"
        }
      },
      {
        "condition": "(!UsePackageReferences)",
        "exclude": ["**/FractalDataWorks.Services.DomainName.Abstractions.csproj"],
        "rename": {
          "FractalDataWorks.Services.DomainName.Abstractions.local.csproj": "FractalDataWorks.Services.DomainName.Abstractions.csproj"
        }
      }
    ]
  }
]
```

## Recommended Project Files

### For Package References (Default)

**FractalDataWorks.Services.DomainName.Abstractions.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <PackageReference Include="FractalDataWorks.Services.Abstractions" Version="FRACTALDATAWORKS_VERSION" />
    <PackageReference Include="FractalDataWorks.Abstractions" Version="FRACTALDATAWORKS_VERSION" />
    <PackageReference Include="FractalDataWorks.EnhancedEnums" Version="FRACTALDATAWORKS_VERSION" />
    <PackageReference Include="FractalDataWorks.Collections" Version="FRACTALDATAWORKS_VERSION" />

    <!-- Source generators via packages -->
    <PackageReference Include="FractalDataWorks.ServiceTypes.SourceGenerators" Version="FRACTALDATAWORKS_VERSION">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="FractalDataWorks.Collections.SourceGenerators" Version="FRACTALDATAWORKS_VERSION">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="FractalDataWorks.Messages.SourceGenerators" Version="FRACTALDATAWORKS_VERSION">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>preview</LangVersion>
    <Configurations>Debug;Release;Experimental;Alpha;Beta;Preview;Refactor</Configurations>
  </PropertyGroup>

</Project>
```

**FractalDataWorks.Services.DomainName.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>IMPLEMENTATION_FRAMEWORKS</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>preview</LangVersion>
    <Configurations>Debug;Release;Experimental;Alpha;Beta;Preview;Refactor</Configurations>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\FractalDataWorks.Services.DomainName.Abstractions\FractalDataWorks.Services.DomainName.Abstractions.csproj" />

    <PackageReference Include="FractalDataWorks.Services" Version="FRACTALDATAWORKS_VERSION" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>

</Project>
```

## Usage Examples

### External Developer (Default)
```powershell
dotnet new fractaldataworks-domain -n Billing
# Uses packages from NuGet, builds immediately
```

### Internal FractalDataWorks Development
```powershell
dotnet new fractaldataworks-domain -n Billing --UsePackageReferences false
# Uses local project references
```

### Specific Version
```powershell
dotnet new fractaldataworks-domain -n Billing --FractalDataWorksPackageVersion "1.0.0"
# Uses specific package version
```

## Current Status

- ✅ Template creates correct file structure
- ✅ Token replacement works
- ✅ All code patterns correct
- ❌ **Uses project references (won't build externally)**
- ⏳ **Needs package reference version** (this document)

## Next Steps

1. Create alternate .csproj files with package references
2. Add conditional logic to template.json
3. Test both modes
4. Update documentation
5. Publish FractalDataWorks packages to NuGet (if not already done)

## Alternative: Simpler Approach

If you want the **simplest solution right now**:

1. Replace ALL project references with package references
2. Use `*` for version (latest)
3. Users need FractalDataWorks packages on NuGet first

**This means templates require FractalDataWorks framework packages to be published to NuGet before external use.**

## Recommendation

**For immediate use within FractalDataWorks solution:** Keep current project references
**For external distribution:** Implement package reference system above

The templates work perfectly **within the FractalDataWorks solution** for internal development. The package reference system is only needed for external developers who don't have the FractalDataWorks source code.
