# Publishing Quick Reference

## Overview

**Publishing happens automatically** when you push to `master` or `develop` branches:

| Branch | Version Example | NuGet Tag | Use Case |
|--------|----------------|-----------|----------|
| `master` | `0.1.0` | Stable | Production releases |
| `develop` | `0.1.0-alpha.45` | Pre-release | Testing, CI/CD |

Version is **automatically generated** by Nerdbank.GitVersioning from `version.json`.

## What Happens Automatically

### On Push to `master` or `develop`

**Two workflows run in parallel:**

1. **Sync Public Mirror** (`.github/workflows/sync-public-mirror.yml`)
   - Discovers projects marked with `<IsPublicProject>true</IsPublicProject>`
   - Copies them to public GitHub repo
   - Pushes to `https://github.com/YourOrg/FractalDataWorks.Framework`

2. **Publish to NuGet** (`.github/workflows/publish-nuget.yml`)
   - Discovers public projects
   - Builds with Nerdbank.GitVersioning (auto version)
   - Packs NuGet packages
   - Publishes to NuGet.org
   - Creates GitHub release

## Versioning

Controlled by `version.json`:

```json
{
  "version": "0.1-alpha",
  "branches": {
    "master": { "tag": "" },           // 0.1.0
    "develop": { "tag": "alpha" }      // 0.1.0-alpha.45
  }
}
```

**To bump version:**
1. Edit `version.json` and change `"version": "0.2-alpha"`
2. Commit and push
3. New version automatically used

## Workflows

### Normal Development (Develop Branch)

```bash
# Make changes
git checkout develop
git add .
git commit -m "Add new feature"
git push origin develop
```

**Result:**
- Public repo updated with latest code
- NuGet packages published: `0.1.0-alpha.{height}`
- Available on NuGet.org as pre-release

### Production Release (Master Branch)

```bash
# Merge develop to master
git checkout master
git merge develop
git push origin master
```

**Result:**
- Public repo updated
- NuGet packages published: `0.1.0` (stable)
- Available on NuGet.org as stable release
- GitHub release created

### Manual Trigger

If you need to republish without code changes:

1. Go to **Actions** tab in private repo
2. Select workflow:
   - **Sync Public Mirror** - to sync code only
   - **Publish to NuGet** - to publish packages only
3. Click **Run workflow**
4. Select branch and run

## Project Visibility

Mark projects as public:

```xml
<!-- In .csproj file -->
<PropertyGroup>
  <IsPublicProject>true</IsPublicProject>
</PropertyGroup>
```

**Check what's public:**
```powershell
.\scripts\Get-PublicProjects.ps1
```

**Bulk mark projects:**
```powershell
.\scripts\Mark-PublicProjects.ps1
```

## Required Secrets

Configure in private repo Settings → Secrets → Actions:

| Secret | Description | Example |
|--------|-------------|---------|
| `PUBLIC_REPO_NAME` | Full name of public repo | `YourOrg/FractalDataWorks.Framework` |
| `PUBLIC_REPO_TOKEN` | GitHub PAT with repo scope | `ghp_xxxxx...` |
| `PRIVATE_REPO_TOKEN` | GitHub PAT with repo scope | `ghp_yyyyy...` |
| `NUGET_API_KEY` | NuGet.org API key | `oy2a...` |

## Monitoring

### Check Workflow Status

1. Go to **Actions** tab in private repo
2. See recent workflow runs
3. Click on a run to see details

### Verify Published Packages

**NuGet.org:**
- Go to https://www.nuget.org/profiles/YourProfile
- Check package list
- Verify version numbers

**Public Repo:**
- Visit `https://github.com/YourOrg/FractalDataWorks.Framework`
- Check commit history
- Verify synced files

## Troubleshooting

### Workflow Fails

**Check logs:**
1. Actions tab → Failed workflow
2. Click on failed step
3. Read error message

**Common issues:**
- Missing GitHub secrets
- Expired tokens
- NuGet API key invalid

### Version Wrong

**Check version locally:**
```bash
dotnet tool install -g nbgv
nbgv get-version
```

**Expected output:**
```
Version:                      0.1.45-alpha
AssemblyVersion:              0.1.0.0
AssemblyInformationalVersion: 0.1.45-alpha+abc1234
NuGetPackageVersion:          0.1.45-alpha
```

### Packages Not Appearing

**Wait 5-10 minutes** - NuGet.org has indexing delay.

**Check package status:**
- https://www.nuget.org/account/Packages
- Look for validation/indexing status

## Quick Commands

```powershell
# List public projects
.\scripts\Get-PublicProjects.ps1

# Mark projects as public
.\scripts\Mark-PublicProjects.ps1

# Check version
nbgv get-version

# Build locally
dotnet build -c Release

# Pack locally (test)
dotnet pack -c Release -o ./artifacts

# Push to develop (triggers publish)
git push origin develop

# Push to master (triggers stable release)
git push origin master
```

## Full Documentation

- **Setup**: See `SETUP-PUBLIC-PUBLISHING.md`
- **Detailed Guide**: See `docs/Public-Mirror-Guide.md`
- **Publishing Overview**: See `PUBLISHING.md`

## Support

- **Sync issues**: Check `.github/workflows/sync-public-mirror.yml` logs
- **NuGet issues**: Check `.github/workflows/publish-nuget.yml` logs
- **Version issues**: Check `version.json` and run `nbgv get-version`
