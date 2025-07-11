# New Repository Setup Checklist

## Pre-Setup
- [ ] Repository created in Azure DevOps
- [ ] Local clone ready
- [ ] Azure DevOps permissions confirmed

## File Setup
- [ ] Copy all template files to repo root
- [ ] Create `/src`, `/tests`, `/docs` directories
- [ ] Add `.gitkeep` files to empty directories

## Configuration Updates
- [ ] Update `Directory.Build.props`:
  - [ ] Authors
  - [ ] Company
  - [ ] Product
  - [ ] PackageProjectUrl
  - [ ] RepositoryUrl
- [ ] Update `nuget.config` if using different feed
- [ ] Set initial version in `version.json`
- [ ] Update `LICENSE` file

## Azure DevOps Configuration
- [ ] Create pipeline from `azure-pipelines.yml`
- [ ] Grant build service **Reader** access to feed
- [ ] Grant build service **Contributor** access to feed (if publishing)
- [ ] Add any required pipeline variables

## First Build
- [ ] Commit and push all files
- [ ] Create `develop` branch from `master`
- [ ] Push to `develop` to trigger first build
- [ ] Verify pipeline runs successfully

## Post-Setup
- [ ] Document any customizations
- [ ] Set up branch policies if needed
- [ ] Configure PR build validation