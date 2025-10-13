# Quick Start: Public Mirror Publishing

This repo uses an **attribute-based system** to designate which projects are part of the public open-source distribution.

## TL;DR

1. **Mark a project as public**: Add `<IsPublicProject>true</IsPublicProject>` to its `.csproj`
2. **Push to master or develop**: GitHub Actions automatically syncs to public repo
3. **Done!**

## Marking Projects as Public

### Option 1: Bulk (Fastest)

```powershell
# Preview what would be marked
.\scripts\Mark-PublicProjects.ps1 -WhatIf

# Mark all default public projects (abstractions, core framework, etc.)
.\scripts\Mark-PublicProjects.ps1
```

### Option 2: Individual Project

Edit the `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <!-- Mark as public open-source project -->
    <IsPublicProject>true</IsPublicProject>
  </PropertyGroup>
</Project>
```

## Checking What's Public

```powershell
# List all public projects
.\scripts\Get-PublicProjects.ps1
```

## How It Works

1. **You mark projects** with `<IsPublicProject>true</IsPublicProject>`
2. **GitHub Actions discovers** these projects automatically on push
3. **Sync happens** to configured public repository
4. **Public repo updated** with latest code from public projects

## Initial Setup (One Time)

### 1. Create Public Repository

```bash
gh repo create YourOrg/FractalDataWorks.Framework --public
```

### 2. Configure GitHub Secrets

In your private repo, add these secrets (Settings → Secrets → Actions):

| Secret Name | Value | Purpose |
|-------------|-------|---------|
| `PUBLIC_REPO_NAME` | `YourOrg/FractalDataWorks.Framework` | Target public repo |
| `PUBLIC_REPO_TOKEN` | `ghp_...` | Personal access token (repo scope) |
| `PRIVATE_REPO_TOKEN` | `ghp_...` | Personal access token (repo scope) |

### 3. Run Initial Sync

```powershell
# Mark your public projects
.\scripts\Mark-PublicProjects.ps1

# Commit and push
git add .
git commit -m "Configure public project publishing"
git push origin develop
```

GitHub Actions will automatically sync to the public repo.

## What Gets Published

**✓ Public** (when marked):
- Core abstractions
- Source generators and analyzers
- Collections, EnhancedEnums, Messages, Results
- Services core and abstractions
- Configuration system
- All tests for public projects

**✗ Private** (always):
- Service implementations (Authentication, Connections, Data, etc.)
- MCP Tools
- Internal templates
- Company-specific code

**Always synced**:
- LICENSE, NOTICE, README.md
- Build configuration files
- Documentation in `/docs`
- Selected samples

## Workflow

### Normal Development

1. Make changes in private repo
2. Commit and push to `master` or `develop`
3. **Automatic sync** to public repo happens
4. Public repo stays up to date

### Adding New Public Project

1. Create project
2. Add `<IsPublicProject>true</IsPublicProject>` to `.csproj`
3. Commit and push
4. **Automatic sync** includes new project

### Making Project Private

1. Remove `<IsPublicProject>true</IsPublicProject>` from `.csproj`
2. Commit and push
3. **Automatic sync** stops including project
4. Manually clean from public repo if needed

## Manual Sync

Trigger manually via GitHub Actions:
1. Go to **Actions** tab
2. Select **Sync Public Mirror**
3. Click **Run workflow**

## Troubleshooting

### Projects not syncing?
```powershell
# Verify project is marked public
.\scripts\Get-PublicProjects.ps1

# Check for it in the list
```

### Workflow failing?
- Check GitHub Secrets are set correctly
- Verify tokens have `repo` scope
- Ensure public repo exists

## Full Documentation

See [docs/Public-Mirror-Guide.md](docs/Public-Mirror-Guide.md) for complete documentation including:
- Detailed workflow explanation
- Security considerations
- Advanced troubleshooting
- Best practices

## License

Public projects are licensed under **Apache License 2.0** with additional rights for Rayburn Electric Cooperative.

See [LICENSE](LICENSE) for details.
