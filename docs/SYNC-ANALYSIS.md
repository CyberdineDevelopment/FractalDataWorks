# Sync Workflow Analysis

This document provides a comprehensive analysis of the sync workflow (`.github/workflows/sync-public-mirror.yml`) and identifies which packages will and will not be synced to the public repository.

## Sync Workflow Overview

The sync workflow copies projects from this private repository to the public mirror repository. It uses a **blacklist approach**: all projects are synced unless they match an exclusion pattern.

**Sync Strategy**: BLACKLIST - All projects sync by default unless excluded by pattern

## Exclusion Patterns (Blacklist)

The workflow defines the following exclusion patterns. Projects matching these patterns will NOT be synced:

| Pattern | Description | Reason |
|---------|-------------|--------|
| `*McpTools*` | MCP tools | Private only |
| `*\.Mcp\.*` | MCP projects | Private only |
| `*\.MCP\.*` | MCP projects (uppercase) | Private only |
| `*SqlServer*` | SQL Server implementations | Private only |
| `*\.Rest\.*` | REST implementations | May have proprietary integrations |

## Projects That WILL Be Synced (57)

All projects will be synced EXCEPT those matching exclusion patterns:

1. ✓ FractalDataWorks.Abstractions
2. ✓ FractalDataWorks.CodeBuilder.Abstractions
3. ✓ FractalDataWorks.CodeBuilder.Analysis
4. ✓ FractalDataWorks.CodeBuilder.Analysis.CSharp
5. ✓ FractalDataWorks.CodeBuilder.CSharp
6. ✓ FractalDataWorks.Collections
7. ✓ FractalDataWorks.Collections.Analyzers
8. ✓ FractalDataWorks.Collections.CodeFixes
9. ✓ FractalDataWorks.Collections.SourceGenerators
10. ✓ FractalDataWorks.Commands.Abstractions
11. ✓ FractalDataWorks.Configuration
12. ✓ FractalDataWorks.Configuration.Abstractions
13. ✓ FractalDataWorks.Data.Abstractions
14. ✓ FractalDataWorks.Data.DataContainers.Abstractions
15. ✓ FractalDataWorks.Data.DataSets.Abstractions
16. ✓ FractalDataWorks.Data.DataStores
17. ✓ FractalDataWorks.Data.DataStores.Abstractions
18. ✓ FractalDataWorks.Data.DataStores.FileSystem
19. ✓ FractalDataWorks.Data.DataStores.Rest
20. ✓ FractalDataWorks.DependencyInjection
21. ✓ FractalDataWorks.EnhancedEnums
22. ✓ FractalDataWorks.EnhancedEnums.Analyzers
23. ✓ FractalDataWorks.EnhancedEnums.CodeFixes
24. ✓ FractalDataWorks.EnhancedEnums.SourceGenerators
25. ✓ FractalDataWorks.MCP.Abstractions
26. ✓ FractalDataWorks.Messages
27. ✓ FractalDataWorks.Messages.SourceGenerators
28. ✓ FractalDataWorks.Results
29. ✓ FractalDataWorks.Results.Abstractions
30. ✓ FractalDataWorks.Services
31. ✓ FractalDataWorks.Services.Abstractions
32. ✓ FractalDataWorks.Services.Authentication
33. ✓ FractalDataWorks.Services.Authentication.Abstractions
34. ✓ FractalDataWorks.Services.Authentication.Entra
35. ✓ FractalDataWorks.Services.Connections
36. ✓ FractalDataWorks.Services.Connections.Abstractions
37. ✓ FractalDataWorks.Services.Connections.Http.Abstractions
38. ✓ FractalDataWorks.Services.Connections.MsSql
39. ✓ FractalDataWorks.Services.Connections.Rest
40. ✓ FractalDataWorks.Services.Data
41. ✓ FractalDataWorks.Services.Data.Abstractions
42. ✓ FractalDataWorks.Services.Execution
43. ✓ FractalDataWorks.Services.Execution.Abstractions
44. ✓ FractalDataWorks.Services.Scheduling
45. ✓ FractalDataWorks.Services.Scheduling.Abstractions
46. ✓ FractalDataWorks.Services.SecretManagers
47. ✓ FractalDataWorks.Services.SecretManagers.Abstractions
48. ✓ FractalDataWorks.Services.SecretManagers.AzureKeyVault
49. ✓ FractalDataWorks.Services.Transformations
50. ✓ FractalDataWorks.Services.Transformations.Abstractions
51. ✓ FractalDataWorks.ServiceTypes
52. ✓ FractalDataWorks.ServiceTypes.Analyzers
53. ✓ FractalDataWorks.ServiceTypes.CodeFixes
54. ✓ FractalDataWorks.ServiceTypes.SourceGenerators
55. ✓ FractalDataWorks.SourceGenerators
56. ✓ FractalDataWorks.Web.Http.Abstractions
57. ✓ FractalDataWorks.Web.RestEndpoints

## Projects That WILL NOT Be Synced (9)

### Excluded by Blacklist Patterns (9 projects)

Only 9 projects match exclusion patterns:

#### SQL Server Implementations (1)
- ✗ **FractalDataWorks.Data.DataStores.SqlServer** - Matches pattern `*SqlServer*`

#### MCP Tools (8)
- ✗ **FractalDataWorks.McpTools.Abstractions** - Matches pattern `*McpTools*`
- ✗ **FractalDataWorks.McpTools.CodeAnalysis** - Matches pattern `*McpTools*`
- ✗ **FractalDataWorks.McpTools.ProjectDependencies** - Matches pattern `*McpTools*`
- ✗ **FractalDataWorks.McpTools.Refactoring** - Matches pattern `*McpTools*`
- ✗ **FractalDataWorks.McpTools.ServerManagement** - Matches pattern `*McpTools*`
- ✗ **FractalDataWorks.McpTools.SessionManagement** - Matches pattern `*McpTools*`
- ✗ **FractalDataWorks.McpTools.TypeAnalysis** - Matches pattern `*McpTools*`
- ✗ **FractalDataWorks.McpTools.VirtualEditing** - Matches pattern `*McpTools*`

**Note:** The following patterns are defined but have no current matches:
- `*\.Mcp\.*` - No projects match this pattern
- `*\.MCP\.*` - No projects match this pattern (FractalDataWorks.MCP.Abstractions does NOT match because it's `.MCP.` not `.Mcp.`)
- `*\.Rest\.*` - No projects match this exact pattern (FractalDataWorks.Data.DataStores.Rest and FractalDataWorks.Services.Connections.Rest do not have dots around "Rest")

## Summary Statistics

| Category | Count |
|----------|-------|
| **Total Projects** | **66** |
| **Will be synced** | **57** (86.4%) |
| **Will NOT be synced** | **9** (13.6%) |
| - Excluded by pattern | 9 |

## Key Findings

1. **57 out of 66 projects (86.4%) will be synced** to the public repository
2. **Only 9 projects (13.6%) are excluded** by blacklist patterns
3. **Sync strategy changed from whitelist to blacklist**: All projects now sync by default unless explicitly excluded
4. **Excluded projects**:
   - 8 MCP/McpTools projects (internal tooling)
   - 1 SQL Server implementation (private only)

## Pattern Match Details

### Active Patterns (have matches)

**SQL Server Pattern (`*SqlServer*`)** - 1 match:
- FractalDataWorks.Data.DataStores.SqlServer ✗

**MCP Tools Pattern (`*McpTools*`)** - 8 matches:
- FractalDataWorks.McpTools.Abstractions ✗
- FractalDataWorks.McpTools.CodeAnalysis ✗
- FractalDataWorks.McpTools.ProjectDependencies ✗
- FractalDataWorks.McpTools.Refactoring ✗
- FractalDataWorks.McpTools.ServerManagement ✗
- FractalDataWorks.McpTools.SessionManagement ✗
- FractalDataWorks.McpTools.TypeAnalysis ✗
- FractalDataWorks.McpTools.VirtualEditing ✗

### Inactive Patterns (no current matches)

**MCP Pattern (`*\.Mcp\.*`)** - 0 matches:
- FractalDataWorks.MCP.Abstractions does NOT match (pattern requires dots around "Mcp")

**MCP Uppercase Pattern (`*\.MCP\.*`)** - 0 matches:
- No projects match this pattern

**REST Pattern (`*\.Rest\.*`)** - 0 matches:
- FractalDataWorks.Data.DataStores.Rest does NOT match (no dots around "Rest")
- FractalDataWorks.Services.Connections.Rest does NOT match (no dots around "Rest")

**Note:** Projects like FractalDataWorks.Services.Connections.MsSql do NOT match `*SqlServer*` pattern.

## Recommendations

**To prevent additional packages from syncing:**
1. Add a new pattern to the `$excludePatterns` array in the workflow
2. Ensure the pattern accurately matches only the projects you want to exclude
3. Test the pattern with wildcard matching to avoid unintended exclusions

**To sync currently excluded packages:**
1. Remove or modify the matching exclusion pattern in the workflow
2. Ensure the package doesn't contain proprietary code
3. Verify dependencies are appropriate for public distribution

**Pattern Tips:**
- Use `*word*` to match anywhere in the name
- Use `*\.word\.*` to match when word is between dots (e.g., `Company.word.Feature`)
- Test patterns carefully - some may not match as expected (see inactive patterns above)
