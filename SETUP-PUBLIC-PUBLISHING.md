# Setup Guide: Public Mirror + NuGet Publishing

This guide walks you through setting up **both** public source code mirroring and NuGet package publishing.

## Overview

You'll have:
- **Private repo** (this one): Full source code with all implementations
- **Public repo**: Mirror of open-source framework components (automatic sync)
- **NuGet.org**: Public packages for easy installation

## Prerequisites

- GitHub account with repo creation permissions
- NuGet.org account for package publishing
- Git and PowerShell installed

## Step 1: Mark Public Projects

Mark which projects should be public by adding `<IsPublicProject>true</IsPublicProject>` to their `.csproj` files.

### Option A: Bulk Marking (Recommended)

```powershell
# Preview what will be marked
.\scripts\Mark-PublicProjects.ps1 -WhatIf

# Mark all public projects (abstractions, core framework, etc.)
.\scripts\Mark-PublicProjects.ps1
```

This marks projects matching these patterns:
- All `*Abstractions` projects
- `*CodeBuilder*` - Source generator infrastructure
- `*Collections*` - Collections system
- `*EnhancedEnums*` - Enhanced enum system
- `*Messages*` - Messaging system
- `*Results*` - Railway-Oriented Programming
- `*Services*` - Service core and abstractions
- `*ServiceTypes*` - Service type system
- `*SourceGenerators*` - All source generators
- All corresponding test projects

### Option B: Manual Marking

Edit each `.csproj` file and add:

```xml
<PropertyGroup>
  <!-- Mark as public open-source project -->
  <IsPublicProject>true</IsPublicProject>
</PropertyGroup>
```

### Verify

```powershell
# List all public projects
.\scripts\Get-PublicProjects.ps1
```

You should see all the core framework projects listed.

## Step 2: Create Public Repository

### Using GitHub CLI

```bash
# Replace with your organization/username
gh repo create YourOrg/FractalDataWorks.Framework --public --description "Open-source .NET framework for service-oriented applications with Railway-Oriented Programming"
```

### Using GitHub Web UI

1. Go to https://github.com/new
2. Repository name: `FractalDataWorks.Framework` (or your preferred name)
3. Description: "Open-source .NET framework for service-oriented applications"
4. Visibility: **Public**
5. **Don't initialize with README** (we'll sync from private repo)
6. Click **Create repository**

## Step 3: Generate Personal Access Tokens

You need tokens for the sync workflow to access both repos.

### Create Tokens

1. Go to https://github.com/settings/tokens/new
2. Create **two** tokens:

**Token 1: Private Repo Access**
- Note: `Private Repo Sync Token`
- Expiration: 1 year (or custom)
- Scopes: `repo` (full control)
- Click **Generate token**
- **Copy token immediately** (you won't see it again)

**Token 2: Public Repo Access**
- Note: `Public Repo Sync Token`
- Expiration: 1 year
- Scopes: `repo` (full control)
- Click **Generate token**
- **Copy token immediately**

**Best practice**: Use a bot account or service account for these tokens.

## Step 4: Configure GitHub Secrets (Private Repo)

Add secrets to your **private repository**:

1. Go to your private repo: `https://github.com/YourOrg/Developer-Kit`
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**

Add these three secrets:

| Secret Name | Value | Example |
|-------------|-------|---------|
| `PUBLIC_REPO_NAME` | Full repo name | `YourOrg/FractalDataWorks.Framework` |
| `PUBLIC_REPO_TOKEN` | Token from Step 3 | `ghp_xxxxxxxxxxxxx` |
| `PRIVATE_REPO_TOKEN` | Token from Step 3 | `ghp_yyyyyyyyyyy` |

## Step 5: Initial Sync to Public Repo

Now commit and push your changes to trigger the first sync:

```bash
# Stage all changes (marked projects, workflows, etc.)
git add .

# Commit
git commit -m "Configure public project publishing"

# Push to develop branch (or master)
git push origin develop
```

### Monitor the Sync

1. Go to **Actions** tab in your private repo
2. Find the **Sync Public Mirror** workflow
3. Click on the running workflow to watch progress
4. It should:
   - Discover public projects
   - Clone public repo
   - Copy public projects
   - Commit and push to public repo

### Verify Sync Success

Check your public repo: `https://github.com/YourOrg/FractalDataWorks.Framework`

You should see:
- All public projects in `src/`
- Tests in `tests/`
- Configuration files
- Documentation
- LICENSE and NOTICE files
- README.md

## Step 6: Create NuGet.org API Key

To publish packages to NuGet.org:

1. Go to https://www.nuget.org
2. Sign in (or create account)
3. Click your username → **API Keys**
4. Click **Create**

Configure the key:
- **Key Name**: `FractalDataWorks CI/CD`
- **Package Owner**: Your account/organization
- **Glob Pattern**: `FractalDataWorks.*` (allows all packages starting with FractalDataWorks)
- **Expiration**: 365 days (or custom)
- Click **Create**
- **Copy the API key immediately** (shown only once)

## Step 7: Configure NuGet Secret (Private Repo)

Add the NuGet API key to your **private repository** secrets:

1. Go to private repo Settings → Secrets and variables → Actions
2. Click **New repository secret**
3. Name: `NUGET_API_KEY`
4. Value: Paste the API key from Step 6
5. Click **Add secret**

## Step 8: Publish First Release

You can publish packages by creating a version tag:

```bash
# Make sure you're on master branch
git checkout master
git pull

# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0
```

### Monitor NuGet Publishing

1. Go to **Actions** tab in private repo
2. Find **Publish to NuGet** workflow
3. Watch it build and publish packages

### Verify on NuGet.org

After 5-10 minutes, search for your packages on https://www.nuget.org:
- `FractalDataWorks.Results`
- `FractalDataWorks.Collections`
- `FractalDataWorks.Services`
- etc.

## Ongoing Workflow

### Normal Development

Just work normally in your private repo:

```bash
# Make changes
git add .
git commit -m "Add new feature"
git push origin develop
```

**Automatic sync** happens on every push to `master` or `develop`.

### Publishing New Version

When ready to publish:

```bash
# Update version.json if needed (Nerdbank.GitVersioning)
git checkout master
git merge develop

# Tag the version
git tag v1.1.0
git push origin v1.1.0
```

**Automatic publish** happens when you push the tag.

## Testing the Setup

### Test Sync Manually

Trigger a manual sync:
1. Go to Actions → **Sync Public Mirror**
2. Click **Run workflow**
3. Select branch and run

### Test Package Build Locally

```powershell
# Build all projects
dotnet build -c Release

# Pack public projects
dotnet pack -c Release -o ./artifacts

# Check packages
dir artifacts
```

### Test Local Publishing (Dry Run)

```powershell
# This validates packages without publishing
dotnet nuget push artifacts/*.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_KEY --skip-duplicate --dry-run
```

## Troubleshooting

### Sync Workflow Fails

**Check secrets are configured**:
```powershell
# In private repo settings, verify:
# - PUBLIC_REPO_NAME
# - PUBLIC_REPO_TOKEN
# - PRIVATE_REPO_TOKEN
```

**Check token permissions**:
- Tokens must have `repo` scope
- Tokens must not be expired
- Bot account must have write access to public repo

### NuGet Publish Fails

**Check API key**:
- Verify `NUGET_API_KEY` secret is set
- Check key hasn't expired on NuGet.org
- Verify glob pattern matches your package names

**Check package metadata**:
- Ensure `Directory.Build.props` has correct package properties
- Verify `LICENSE` file exists
- Check `version.json` for GitVersioning

### Public Repo Empty

**Force full sync**:
```powershell
# In private repo
git add .
git commit -m "Force sync" --allow-empty
git push origin develop
```

Or manually trigger via Actions → Sync Public Mirror → Run workflow

### Packages Not Found on NuGet

Wait 5-10 minutes after publish (indexing delay).

Check package status:
1. Go to https://www.nuget.org/account/Packages
2. Find your packages
3. Check status (Published, Validating, etc.)

## Best Practices

### Commit Messages

Write clear messages that work in both contexts:
- ✅ "Add async configuration loading support"
- ✅ "Fix null reference in ServiceTypeCollection"
- ❌ "Fix internal API bug" (too vague for public)

### Versioning

Use semantic versioning:
- `v1.0.0` - Major release
- `v1.1.0` - Minor features
- `v1.0.1` - Patches
- `v1.0.0-beta.1` - Pre-releases

### Documentation

- Keep public docs in `/docs` (synced to public repo)
- Put internal docs in `/internal-docs` (not synced)
- Update README.md for major features

## Security Checklist

Before marking a project as public:

- [ ] No API keys, passwords, or secrets in code
- [ ] No connection strings to internal systems
- [ ] No customer data or references
- [ ] No internal-only documentation
- [ ] Dependencies are compatible with Apache 2.0
- [ ] Code reviewed for sensitive information

## Success Criteria

After setup, you should have:

- ✅ Private repo with all source code
- ✅ Public repo with framework components (auto-synced)
- ✅ Packages published on NuGet.org
- ✅ Automatic sync on every push
- ✅ Automatic publish on version tags
- ✅ Community can see source code
- ✅ Community can install packages easily

## Next Steps

1. **Update README.md** in public repo with installation instructions
2. **Add badges** to README (NuGet version, build status, etc.)
3. **Create CONTRIBUTING.md** for community contributions
4. **Set up GitHub Pages** for documentation (optional)
5. **Announce** on relevant channels

## Support

- **Sync issues**: Check Actions logs in private repo
- **NuGet issues**: Check https://www.nuget.org/account/Packages
- **General questions**: See `docs/Public-Mirror-Guide.md`

## Quick Reference

```powershell
# List public projects
.\scripts\Get-PublicProjects.ps1

# Mark projects as public
.\scripts\Mark-PublicProjects.ps1

# Test build
dotnet build -c Release

# Test pack
dotnet pack -c Release -o ./artifacts

# Create release
git tag v1.0.0
git push origin v1.0.0
```

**GitHub Secrets Required:**
- `PUBLIC_REPO_NAME` - Full name of public repo
- `PUBLIC_REPO_TOKEN` - PAT with repo scope
- `PRIVATE_REPO_TOKEN` - PAT with repo scope
- `NUGET_API_KEY` - NuGet.org API key

**Workflows:**
- `.github/workflows/sync-public-mirror.yml` - Syncs to public repo
- `.github/workflows/publish-nuget.yml` - Publishes to NuGet.org
