# Sync Workflow Documentation

This directory contains analysis and documentation for the repository's sync workflow.

## Quick Answer

**Q: Which packages will not be synced to the public repository?**

**A: 55 out of 66 packages (83.3%) will NOT be synced.**

Only 11 packages are marked as public and will be synced to the public mirror repository.

## Documentation Files

### 📊 [SYNC-ANALYSIS.md](./SYNC-ANALYSIS.md)
**Comprehensive Analysis Report**

Contains detailed information about:
- How the sync workflow operates
- Exclusion patterns and their purpose
- Complete list of all 66 projects categorized by sync status
- Why each project is or isn't synced
- Statistics and recommendations

**Best for:** Understanding the full picture and making decisions about sync configuration.

### 📋 [SYNC-STATUS-TABLE.md](./SYNC-STATUS-TABLE.md)
**Quick Reference Tables**

Contains:
- Simple tables listing all projects by sync status
- Projects organized by functional category
- Summary statistics by category
- Exclusion pattern reference

**Best for:** Quick lookups to check if a specific package will be synced.

## Quick Summary

### Will Be Synced (11) ✓
- FractalDataWorks.Abstractions
- FractalDataWorks.Collections (+ Analyzers)
- FractalDataWorks.EnhancedEnums (+ Analyzers)
- FractalDataWorks.Messages
- FractalDataWorks.Results
- FractalDataWorks.Services (+ Execution)
- FractalDataWorks.ServiceTypes (+ Analyzers)

### Will NOT Be Synced (55) ✗

**By Category:**
- Services Layer: 21 projects
- MCP/Tools: 10 projects
- Source Generators: 8 projects
- Data Layer: 7 projects
- Other: 9 projects

**Why?**
- All are missing `<IsPublicProject>true</IsPublicProject>` in their `.csproj` files

## How the Sync Works

The sync workflow (`.github/workflows/sync-public-mirror.yml`) uses two criteria:

1. **IsPublicProject Flag**: Projects must have `<IsPublicProject>true</IsPublicProject>` in their `.csproj` file
2. **Exclusion Patterns**: Even if marked public, projects matching these patterns are excluded:
   - `*McpTools*` - MCP tools (private only)
   - `*\.Mcp\.*` - MCP projects
   - `*\.MCP\.*` - MCP projects (uppercase)
   - `*SqlServer*` - SQL Server implementations (private only)
   - `*\.Rest\.*` - REST implementations (proprietary integrations)

## Making Changes

### To Sync Additional Packages:
1. Add `<IsPublicProject>true</IsPublicProject>` to the project's `.csproj` file
2. Ensure it doesn't contain proprietary code
3. Verify it doesn't match any exclusion pattern
4. Check that all its dependencies are also public

### To Prevent Syncing:
- Remove `<IsPublicProject>true</IsPublicProject>` from the `.csproj` file
- Or add a matching exclusion pattern to the workflow

## Related Files

- `.github/workflows/sync-public-mirror.yml` - The sync workflow
- `PublicProjects.props` - MSBuild properties for public project marking
- Individual `.csproj` files - Contain `<IsPublicProject>` settings

---

**Generated:** 2025-10-14  
**Analysis Based On:** Commit be182ff and later  
**Repository:** CyberdineDevelopment/Developer-Kit-Private
