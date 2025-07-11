# Repository Setup Instructions

This template provides a standardized structure for .NET projects with Azure DevOps CI/CD pipeline integration.

## Initial Setup for New Repository

### 1. Copy Essential Files

Copy these files to your new repository root:
- `azure-pipelines.yml` - CI/CD pipeline configuration
- `Directory.Build.props` - Centralized build configuration
- `Directory.Packages.props` - Centralized package versions
- `.editorconfig` - Code style configuration
- `.gitignore` - Git ignore patterns
- `coverlet.runsettings` - Code coverage settings
- `nuget.config` - Package feed configuration
- `version.json` - Version configuration for Nerdbank.GitVersioning
- `LICENSE` - License file (update as needed)

### 2. Create Directory Structure

Create these directories:
```
/src       - Source code projects
/tests     - Test projects  
/docs      - Documentation
/samples   - Sample code (optional)
```

### 3. Modify Configuration Files

#### Directory.Build.props
Update these sections for your project:
```xml
<!-- Update package metadata -->
<Authors>Your Team Name</Authors>
<Company>Your Company</Company>
<Product>Your Product Name</Product>
<PackageProjectUrl>https://github.com/YourOrg/YourRepo</PackageProjectUrl>
<RepositoryUrl>https://github.com/YourOrg/YourRepo.git</RepositoryUrl>

<!-- Update strong naming key if applicable -->
<AssemblyOriginatorKeyFile>YourKey.snk</AssemblyOriginatorKeyFile>
```

#### nuget.config
Update if using different feeds or organization:
```xml
<!-- Update feed URL if different -->
<add key="dotnet-packages" value="https://pkgs.dev.azure.com/YourOrg/_packaging/your-feed/nuget/v3/index.json" />

<!-- Update package source mapping for your packages -->
<packageSource key="dotnet-packages">
  <package pattern="YourCompany.*" />
</packageSource>
```

#### version.json
Set your initial version:
```json
{
  "$schema": "https://raw.githubusercontent.com/dotnet/Nerdbank.GitVersioning/master/src/NerdBank.GitVersioning/version.schema.json",
  "version": "0.1-alpha",  // Change to your desired starting version
  "publicReleaseRefSpec": [
    "^refs/heads/master$",
    "^refs/heads/v\\d+(?:\\.\\d+)?$"
  ]
}
```

### 4. Azure DevOps Setup

#### Create Pipeline
1. Go to Pipelines → New Pipeline
2. Select your repository
3. Choose "Existing Azure Pipelines YAML file"
4. Select `/azure-pipelines.yml`
5. Save (don't run yet)

#### Configure Feed Access
1. Go to Artifacts → Select your feed → Feed Settings → Permissions
2. Add "[Your Project] Build Service" with **Reader** role (for restore)
3. Add "[Your Project] Build Service" with **Contributor** role (for publish)

#### Pipeline Variables (Optional)
If your pipeline needs secrets or configuration:
1. Edit pipeline → Variables
2. Add any required variables
3. Mark sensitive values as secret

### 5. Branch Strategy

The pipeline is configured for this branch strategy:
- `master` - Production releases (Release configuration)
- `develop` - Alpha builds (tests can fail)
- `experimental/*` - Experimental features (minimal quality gates)
- `beta/*` - Beta releases (70% coverage required)
- `release/*` - Release candidates (85% coverage required)
- `feature/*` - Feature development (Debug configuration)

### 6. Quality Gates by Branch

| Branch | Build Config | Tests Must Pass | Coverage Required | Security Scans |
|--------|-------------|-----------------|-------------------|----------------|
| master | Release | Yes | 90% | Yes |
| release/* | Preview | Yes | 85% | Yes |
| beta/* | Beta | Yes | 70% | Yes |
| develop | Alpha | No | 0% | Yes |
| experimental/* | Experimental | No | 0% | No |
| feature/* | Debug | No | 0% | No |

## Adding to Existing Repository

If adding to an existing repo, be careful not to overwrite:

1. **Merge** `.gitignore` entries instead of replacing
2. **Merge** `.editorconfig` rules with existing ones
3. **Review** `Directory.Build.props` for conflicts with existing project settings
4. **Check** package versions in `Directory.Packages.props` against your current versions
5. **Update** `azure-pipelines.yml` if you have custom build steps

## Customization

### Adding Build Steps
Add custom steps in the Build stage after the restore step:
```yaml
- task: YourCustomTask@1
  displayName: 'Your custom step'
  inputs:
    ...
```

### Modifying Package Metadata
All package metadata is centralized in `Directory.Build.props`. Project-specific overrides can be added in individual `.csproj` files.

### Changing Versioning Strategy
The template uses Nerdbank.GitVersioning. To use a different strategy:
1. Remove the `version.json` file
2. Remove `Nerdbank.GitVersioning` from `Directory.Packages.props`
3. Update pipeline to set version differently

## Troubleshooting

### Feed Access Errors (401/403)
- Ensure build service has Reader access to the feed
- Run `NuGetAuthenticate@1` task before any restore operations
- Check feed URL in `nuget.config` is correct

### Version Conflicts
- Check `Directory.Packages.props` for version mismatches
- Ensure all projects use `<PackageReference Include="..." />` without Version attribute

### Pipeline Failures
- Check branch-specific quality gates in pipeline variables
- Verify all required files are present (especially `coverlet.runsettings`)
- Ensure .NET SDK version specified exists (currently .NET 10 preview)