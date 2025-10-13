# Public Mirror Publishing Guide

This guide explains how to manage the public open-source mirror of the FractalDataWorks Developer Kit.

## Overview

The project uses a **private repo + public mirror** strategy:
- **Private repository** (this repo): Contains all code, including proprietary implementations
- **Public repository**: Contains only open-source framework components marked as public
- **Automatic synchronization**: GitHub Actions automatically syncs public projects on every push to `master` or `develop`

## Architecture

### Project Marking System

Projects are marked as public by adding a property to their `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <!-- Mark as public open-source project -->
    <IsPublicProject>true</IsPublicProject>
  </PropertyGroup>
</Project>
```

The build system (via `PublicProjects.props`) tracks these projects and provides tooling for discovery.

### What Gets Published

**Public projects** (marked with `<IsPublicProject>true</IsPublicProject>`):
- Core abstractions and base types
- Source generator infrastructure (CodeBuilder)
- Collections system with source generators
- Enhanced enums system
- Messages and Results (Railway-Oriented Programming)
- Services core and abstractions (not implementations)
- ServiceTypes system
- Configuration system
- All associated tests

**Private projects** (remain in private repo only):
- Domain service implementations (Authentication, Connections, Data, etc.)
- MCP Tools
- Internal templates
- Company-specific configurations

**Always synced**:
- Root configuration files (Directory.Build.props, etc.)
- LICENSE and NOTICE files
- Documentation in `/docs`
- Public samples (SourceGenerators, EnhancedEnums)

## Workflow

### Automatic Synchronization

The sync happens automatically via GitHub Actions whenever you push to `master` or `develop`:

1. **Trigger**: Push to `master` or `develop` branch
2. **Discovery**: GitHub Actions runs `scripts/Get-PublicProjects.ps1` to find all projects marked as public
3. **Sync**: Copies public projects + configuration files to public repository
4. **Commit**: Creates commit in public repo with reference to private repo commit SHA
5. **Push**: Pushes changes to public repository

**Configuration** (GitHub Secrets required):
- `PRIVATE_REPO_TOKEN`: Personal access token for private repo (with `repo` scope)
- `PUBLIC_REPO_TOKEN`: Personal access token for public repo (with `repo` scope)
- `PUBLIC_REPO_NAME`: Full name of public repo (e.g., `YourOrg/FractalDataWorks.Framework`)

### Manual Synchronization

To manually trigger a sync:

1. Go to **Actions** tab in GitHub
2. Select **Sync Public Mirror** workflow
3. Click **Run workflow**
4. Choose branch and click **Run workflow**

### Local Discovery

To see which projects are currently marked as public:

```powershell
# List public projects
.\scripts\Get-PublicProjects.ps1

# Show directory paths (for scripting)
.\scripts\Get-PublicProjects.ps1 -ShowDirectories

# Show full paths
.\scripts\Get-PublicProjects.ps1 -ShowPaths
```

## Marking Projects as Public

### Option 1: Bulk Marking (Recommended)

Use the provided script to mark multiple projects at once:

```powershell
# Preview what would be marked (dry run)
.\scripts\Mark-PublicProjects.ps1 -WhatIf

# Mark all matching projects as public
.\scripts\Mark-PublicProjects.ps1

# Mark specific patterns
.\scripts\Mark-PublicProjects.ps1 -ProjectPatterns "*Results*","*Collections*"
```

The script uses these default patterns:
- `*Abstractions` - All abstraction projects
- `*CodeBuilder*` - Source generator infrastructure
- `*Collections*` - Collections system
- `*EnhancedEnums*` - Enhanced enums
- `*Messages*` - Messaging system
- `*Results*` - Railway-Oriented Programming
- `*Services*` - Services core (but not implementations)
- `*ServiceTypes*` - Service type system
- `*SourceGenerators*` - All source generators
- And more...

### Option 2: Manual Marking

Edit the `.csproj` file and add to the first `<PropertyGroup>`:

```xml
<!-- Mark as public open-source project -->
<IsPublicProject>true</IsPublicProject>
```

### Verification

After marking projects, verify with:

```powershell
.\scripts\Get-PublicProjects.ps1
```

Build the project to trigger the marker file creation:

```powershell
dotnet build
```

## Setting Up the Public Repository

### Initial Setup

1. **Create public repository** on GitHub:
   ```bash
   gh repo create YourOrg/FractalDataWorks.Framework --public
   ```

2. **Configure GitHub Secrets** in your private repo:
   - Go to Settings → Secrets and variables → Actions
   - Add these secrets:
     - `PUBLIC_REPO_NAME`: `YourOrg/FractalDataWorks.Framework`
     - `PUBLIC_REPO_TOKEN`: Personal access token with `repo` scope
     - `PRIVATE_REPO_TOKEN`: Personal access token with `repo` scope

3. **Run initial sync**:
   - Go to Actions → Sync Public Mirror → Run workflow
   - Or push a commit to `master` or `develop`

### Creating a Custom Public README

You can create a separate README specifically for the public repo:

1. Create `.github/PUBLIC_README.md` in the private repo
2. This file will be copied as `README.md` in the public repo
3. If `PUBLIC_README.md` doesn't exist, the main `README.md` is used

Example:

```markdown
# FractalDataWorks Framework

Open-source .NET framework for building service-oriented applications with Railway-Oriented Programming.

This is the public distribution of the FractalDataWorks Developer Kit, containing the core framework components and abstractions.

For the full developer kit including implementations, visit our private repository (access required).

## Installation

\`\`\`bash
dotnet add package FractalDataWorks.Results
dotnet add package FractalDataWorks.Collections
\`\`\`

[Rest of public-specific README...]
```

## Development Workflow

### Making Changes to Public Projects

1. **Make changes** in your private repo as normal
2. **Test locally**:
   ```powershell
   dotnet build
   dotnet test
   ```
3. **Commit and push** to `master` or `develop`:
   ```bash
   git add .
   git commit -m "Update Results pattern implementation"
   git push origin develop
   ```
4. **GitHub Actions automatically syncs** to public repo
5. **Verify sync** succeeded:
   - Check Actions tab in GitHub
   - Or visit public repo and verify changes appear

### Adding a New Public Project

1. **Create the project** in your private repo
2. **Mark it as public** in the `.csproj`:
   ```xml
   <IsPublicProject>true</IsPublicProject>
   ```
3. **Add to solution**:
   ```bash
   dotnet sln add src/FractalDataWorks.NewProject/FractalDataWorks.NewProject.csproj
   ```
4. **Build and verify**:
   ```powershell
   dotnet build
   .\scripts\Get-PublicProjects.ps1  # Verify it appears
   ```
5. **Commit and push** - sync happens automatically

### Making a Project Private

To remove a project from public distribution:

1. **Remove or change** `<IsPublicProject>` in `.csproj`:
   ```xml
   <!-- Remove this line, or set to false -->
   <IsPublicProject>false</IsPublicProject>
   ```
2. **Commit and push** - project will no longer sync
3. **Manually remove from public repo** if needed:
   ```bash
   # In public repo
   git rm -r src/FractalDataWorks.ProjectName
   git commit -m "Remove ProjectName from public distribution"
   git push
   ```

## Troubleshooting

### Sync Workflow Fails

**Check GitHub Secrets**:
- Verify `PUBLIC_REPO_TOKEN` has `repo` scope
- Verify `PUBLIC_REPO_NAME` is correct (e.g., `YourOrg/RepoName`)
- Tokens must not be expired

**Check repository permissions**:
- Bot account must have push access to public repo
- Private repo token must have read access

### Project Not Syncing

**Verify project is marked public**:
```powershell
.\scripts\Get-PublicProjects.ps1
```

**Check for build errors**:
```powershell
dotnet build
```

**Verify paths in workflow**:
- Check `.github/workflows/sync-public-mirror.yml`
- Ensure paths match your project structure

### Public Repo Out of Sync

**Force a full sync**:
1. Go to Actions → Sync Public Mirror
2. Click **Run workflow**
3. Check **force_sync** checkbox
4. Run workflow

**Or manually sync locally** (advanced):
```powershell
# Discover public projects
$projects = .\scripts\Get-PublicProjects.ps1 -ShowDirectories

# Clone public repo
git clone https://github.com/YourOrg/FractalDataWorks.Framework.git ../public-repo

# Copy projects manually
foreach ($project in $projects) {
    Copy-Item -Path $project -Destination "../public-repo/$project" -Recurse -Force
}

# Commit and push
cd ../public-repo
git add .
git commit -m "Manual sync from private repo"
git push
```

## Best Practices

### Commit Messages

Use clear commit messages that make sense in both private and public context:

✅ **Good**:
- "Add support for async configuration loading"
- "Fix null reference in ServiceTypeCollection"
- "Update documentation for Railway-Oriented Programming"

❌ **Avoid**:
- "Update internal API endpoints" (mentions private details)
- "Fix bug reported by [internal team member]" (internal reference)

### Documentation

- Public documentation goes in `/docs` (synced to public repo)
- Private documentation goes in `/internal-docs` (not synced)
- API documentation should work for public consumers

### Testing

- Public projects should have comprehensive tests
- Tests are synced to public repo (transparency)
- Avoid tests that reference private implementations

### Versioning

- Use Nerdbank.GitVersioning (configured in `version.json`)
- Same version numbers in both private and public repos
- Public repo automatically inherits version from sync

## Security Considerations

### Secrets and Credentials

**Never commit**:
- API keys, passwords, or tokens
- Connection strings to internal systems
- Company-specific configurations

**The sync process does NOT filter secrets** - you must ensure public projects don't contain sensitive data before marking them public.

### Code Review

Before marking a project as public:

1. **Review for sensitive information**:
   - Connection strings
   - API endpoints
   - Company-specific business logic
   - Customer data or references

2. **Verify licensing**:
   - Ensure code is properly licensed (Apache 2.0)
   - No GPL or other incompatible licenses in dependencies
   - Third-party code properly attributed

3. **Check documentation**:
   - No internal-only documentation
   - Examples don't reference internal systems

## License

All public projects are licensed under **Apache License 2.0** with additional rights granted to Rayburn Electric Cooperative.

See `LICENSE` file for details.

## Support

For issues related to:
- **Public mirror sync**: Open issue in private repo (DevOps team)
- **Public framework**: Open issue in public repo (community)
- **Private implementations**: Internal issue tracker
