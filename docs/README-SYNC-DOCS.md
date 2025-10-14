# Sync Workflow Documentation

This directory contains analysis and documentation for the repository's sync workflow.

## Quick Answer

**Q: Which packages will not be synced to the public repository?**

**A: Only 9 out of 66 packages (13.6%) will NOT be synced.**

The sync uses a **blacklist approach** - all packages sync by default unless they match an exclusion pattern.

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

### Will Be Synced (57) ✓

**All projects sync by default** except those matching exclusion patterns. This includes:
- All core abstractions and implementations
- All analyzers, source generators, and code fixes
- All services layer projects
- All data layer projects (except SQL Server)
- All configuration and DI projects
- All web abstractions
- MCP.Abstractions (not McpTools)

### Will NOT Be Synced (9) ✗

**Only projects matching blacklist patterns:**
- **SQL Server (1)**: FractalDataWorks.Data.DataStores.SqlServer
- **MCP Tools (8)**: All FractalDataWorks.McpTools.* projects

**Why?**
- Match exclusion patterns defined in the workflow (blacklist)

## How the Sync Works

The sync workflow (`.github/workflows/sync-public-mirror.yml`) uses a **BLACKLIST approach**:

**All projects are synced by default** unless they match an exclusion pattern:
- `*McpTools*` - MCP tools (private only) - **8 matches**
- `*\.Mcp\.*` - MCP projects (with dots) - **0 matches**
- `*\.MCP\.*` - MCP projects uppercase (with dots) - **0 matches**
- `*SqlServer*` - SQL Server implementations (private only) - **1 match**
- `*\.Rest\.*` - REST implementations (with dots) - **0 matches**

## Making Changes

### To Sync Currently Excluded Packages:
1. Remove or modify the matching exclusion pattern in the workflow
2. Ensure the package doesn't contain proprietary code
3. Verify dependencies are appropriate for public distribution

### To Prevent Additional Packages from Syncing:
1. Add a new pattern to the `$excludePatterns` array in the workflow
2. Ensure the pattern accurately matches only the projects you want to exclude
3. Test the pattern carefully (patterns with dots like `*\.word\.*` require dots around the word)

## Related Files

- `.github/workflows/sync-public-mirror.yml` - The sync workflow (defines exclusion patterns)
- `PublicProjects.props` - MSBuild properties (no longer used for sync control)

---

**Generated:** 2025-10-14  
**Analysis Based On:** Commit be182ff and later  
**Repository:** CyberdineDevelopment/Developer-Kit-Private
